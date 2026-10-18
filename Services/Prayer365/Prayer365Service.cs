using System.Data;
using BulletinBoard.Helpers;
using BulletinBoard.Models.Prayer365;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BulletinBoard.Services.Prayer365
{
    public interface IPrayer365Service
    {
        Task EnsureTableAsync();
        Task<Prayer365Item?> GetAsync(string md);
        /// <summary>제목만 (홈 배너용 — 가벼운 조회).  없으면 null</summary>
        Task<string?> GetTitleAsync(string md);
        /// <summary>조회 수 +1 하고 새 값을 돌려준다 (기도문이 없으면 0)</summary>
        Task<int> AddViewAsync(string md, DateTime viewDate);
        /// <summary>오늘(KST) 조회 수 · 누적 조회 수(모든 기도문 합계) (2026-10-19)</summary>
        Task<(int today, int total)> GetViewStatsAsync(DateTime viewDate);
        /// <summary>
        /// "오늘 기도 했어요" — 로그인한 사람은 개인별 기록(Prayer365PrayLog)에 하루 한 줄 넣고 합계 +1.
        /// 이미 기록이 있으면 added=false.  count = 새 합계
        /// </summary>
        Task<(bool added, int count)> AddPrayAsync(string md, string? userName, string? userId, DateTime prayDate);
        /// <summary>이 사람이 기도한 날들 (하루에 한 번만 — 기도문이 달라도 같은 날이면 하루)</summary>
        Task<List<DateTime>> GetPrayDatesAsync(string userName);
        /// <summary>모든 참여자의 (아이디, 기도한 날, 그날 마지막 누른 시각) — 관리자 집계용</summary>
        Task<List<(string user, DateTime day, DateTime at)>> GetAllPrayDaysAsync();
        /// <summary>참여자 아이디 → 이름(성+이름, AspNetUsers) — 참여자 화면용 (2026-10-19)</summary>
        Task<Dictionary<string, string>> GetParticipantNamesAsync();
        /// <summary>이 사람이 그날(KST) 이 기도문으로 기도했는지</summary>
        Task<bool> HasPrayedAsync(string md, string userName, DateTime prayDate);
        Task<int> GetPrayCountAsync(string md);
        Task<Dictionary<string, Prayer365Status>> GetStatusMapAsync();
        Task<List<Prayer365Status>> SearchAsync(string q);
        /// <summary>저장 (있으면 고치고 없으면 넣는다)</summary>
        Task SaveAsync(Prayer365Item item, string? by);
        Task<bool> DeleteAsync(string md);
    }

    /// <summary>
    ///  365일 기도문 표 (dbo.Prayer365).  표는 처음 쓸 때 만든다 (Sql/Prayer365.sql 과 같은 내용).
    /// </summary>
    public class Prayer365Service : IPrayer365Service
    {
        private readonly string _connStr;
        private static bool _tableChecked;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public Prayer365Service(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing.");
        }

        private SqlConnection Conn() => new(_connStr);

        public const string Ddl = @"
IF OBJECT_ID(N'dbo.Prayer365', N'U') IS NULL
CREATE TABLE dbo.Prayer365 (
    MonthDay    CHAR(4)        NOT NULL CONSTRAINT PK_Prayer365 PRIMARY KEY,   -- '0101' ~ '1231', '0229' 포함
    Title       NVARCHAR(200)  NOT NULL CONSTRAINT DF_Prayer365_Title DEFAULT(N''),
    VerseRef    NVARCHAR(100)  NULL,
    VerseText   NVARCHAR(1000) NULL,
    Body        NVARCHAR(MAX)  NOT NULL CONSTRAINT DF_Prayer365_Body DEFAULT(N''),
    Fixes       NVARCHAR(MAX)  NULL,        -- Claude 교정 기록 JSON [{From,To}]
    IsReviewed  BIT            NOT NULL CONSTRAINT DF_Prayer365_Rev DEFAULT(0),
    UpdatedAt   DATETIME2      NOT NULL,
    UpdatedBy   NVARCHAR(100)  NULL
);
-- 2026-10-03 : 조회 수 (관리자 보기는 세지 않는다)
IF COL_LENGTH(N'dbo.Prayer365', N'ViewCount') IS NULL
    ALTER TABLE dbo.Prayer365 ADD ViewCount INT NOT NULL CONSTRAINT DF_Prayer365_View DEFAULT(0);
-- 2026-10-17 : ""오늘 기도 했어요"" 누른 수
IF COL_LENGTH(N'dbo.Prayer365', N'PrayCount') IS NULL
    ALTER TABLE dbo.Prayer365 ADD PrayCount INT NOT NULL CONSTRAINT DF_Prayer365_Pray DEFAULT(0);
-- 2026-10-17 : ""오늘 기도 했어요"" 개인별 기록 — 한 사람 · 한 날(KST) · 한 기도문에 한 줄
IF OBJECT_ID(N'dbo.Prayer365PrayLog', N'U') IS NULL
BEGIN
CREATE TABLE dbo.Prayer365PrayLog (
    LogId     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Prayer365PrayLog PRIMARY KEY,
    MonthDay  CHAR(4)        NOT NULL,
    PrayDate  DATE           NOT NULL,
    UserName  NVARCHAR(256)  NOT NULL,
    UserId    NVARCHAR(450)  NULL,
    PrayedAt  DATETIME2      NOT NULL,
    CONSTRAINT UQ_Prayer365PrayLog UNIQUE (UserName, PrayDate, MonthDay)
);
CREATE INDEX IX_Prayer365PrayLog_Day ON dbo.Prayer365PrayLog (PrayDate, MonthDay);
END
-- 2026-10-19 : 날짜(KST)별 조회 수 — ""오늘의 조회수"" (누적은 Prayer365.ViewCount 합계)
IF OBJECT_ID(N'dbo.Prayer365ViewDay', N'U') IS NULL
CREATE TABLE dbo.Prayer365ViewDay (
    ViewDate  DATE NOT NULL CONSTRAINT PK_Prayer365ViewDay PRIMARY KEY,
    ViewCount INT  NOT NULL CONSTRAINT DF_Prayer365ViewDay_Cnt DEFAULT(0)
);";

        public async Task EnsureTableAsync()
        {
            if (_tableChecked) return;
            await _lock.WaitAsync();
            try
            {
                if (_tableChecked) return;
                await using var con = Conn();
                await con.OpenAsync();
                await using var cmd = new SqlCommand(Ddl, con);
                await cmd.ExecuteNonQueryAsync();
                _tableChecked = true;
            }
            finally { _lock.Release(); }
        }

        public async Task<Prayer365Item?> GetAsync(string md)
        {
            await EnsureTableAsync();
            await using var con = Conn();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(@"
SELECT MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy, ViewCount,
       PrayCount + (SELECT COUNT(*) FROM dbo.Prayer365PrayLog L WITH (NOLOCK) WHERE L.MonthDay = dbo.Prayer365.MonthDay) AS PrayTotal
FROM dbo.Prayer365 WITH (NOLOCK) WHERE MonthDay=@Md;", con);
            cmd.Parameters.Add("@Md", SqlDbType.Char, 4).Value = md;
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;
            List<Prayer365Fix> fixes = new();
            if (!r.IsDBNull(5))
            {
                try { fixes = JsonConvert.DeserializeObject<List<Prayer365Fix>>(r.GetString(5)) ?? new(); } catch { }
            }
            return new Prayer365Item
            {
                MonthDay   = r.GetString(0),
                Title      = r.GetString(1),
                VerseRef   = r.IsDBNull(2) ? null : r.GetString(2),
                VerseText  = r.IsDBNull(3) ? null : r.GetString(3),
                Body       = r.GetString(4),
                Fixes      = fixes,
                IsReviewed = r.GetBoolean(6),
                UpdatedAt  = r.GetDateTime(7),
                UpdatedBy  = r.IsDBNull(8) ? null : r.GetString(8),
                ViewCount  = r.GetInt32(9),
                PrayCount  = r.GetInt32(10),   // 기록 표 줄 수 + 기록 없는 클릭 (PrayTotal)
            };
        }

        public async Task<(bool added, int count)> AddPrayAsync(string md, string? userName, string? userId, DateTime prayDate)
        {
            await EnsureTableAsync();
            await using var con = Conn();
            await con.OpenAsync();
            // 로그인한 사람: 오늘 이 기도문 기록이 없을 때만 한 줄 넣고 합계 +1 (한 번에)
            // 로그인 안 한 사람: 기록 없이 합계만 +1 (하루 한 번은 세션에서 막는다)
            await using var cmd = new SqlCommand(@"
SET XACT_ABORT ON;
DECLARE @added BIT = 0;
BEGIN TRAN;
IF EXISTS (SELECT 1 FROM dbo.Prayer365 WITH (UPDLOCK) WHERE MonthDay=@Md)
   AND (@User IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.Prayer365PrayLog WITH (UPDLOCK, HOLDLOCK)
                                      WHERE UserName=@User AND PrayDate=@Day AND MonthDay=@Md))
BEGIN
    IF @User IS NOT NULL
        INSERT INTO dbo.Prayer365PrayLog (MonthDay, PrayDate, UserName, UserId, PrayedAt)
        VALUES (@Md, @Day, @User, @UserId, @At);
    -- 로그인한 사람은 기록 표 줄 수가 곧 숫자 — 합계 칸은 기록 없는 클릭(로그인 안 한 사람)만 올린다
    IF @User IS NULL
        UPDATE dbo.Prayer365 SET PrayCount = PrayCount + 1 WHERE MonthDay=@Md;
    SET @added = 1;
END
COMMIT;
SELECT @added, ISNULL((SELECT PrayCount FROM dbo.Prayer365 WHERE MonthDay=@Md), 0)
             + (SELECT COUNT(*) FROM dbo.Prayer365PrayLog WHERE MonthDay=@Md);", con);
            cmd.Parameters.Add("@Md", SqlDbType.Char, 4).Value = md;
            cmd.Parameters.Add("@Day", SqlDbType.Date).Value = prayDate.Date;
            cmd.Parameters.Add("@User", SqlDbType.NVarChar, 256).Value = string.IsNullOrWhiteSpace(userName) ? DBNull.Value : userName;
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 450).Value = (object?)userId ?? DBNull.Value;
            cmd.Parameters.Add("@At", SqlDbType.DateTime2).Value = DateHelper.GetKoreaNow();
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return (false, 0);
            return (r.GetBoolean(0), r.GetInt32(1));
        }

        public async Task<List<DateTime>> GetPrayDatesAsync(string userName)
        {
            await EnsureTableAsync();
            var list = new List<DateTime>();
            await using var con = Conn();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(
                "SELECT DISTINCT PrayDate FROM dbo.Prayer365PrayLog WITH (NOLOCK) WHERE UserName=@User;", con);
            cmd.Parameters.Add("@User", SqlDbType.NVarChar, 256).Value = userName;
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(r.GetDateTime(0));
            return list;
        }

        public async Task<List<(string user, DateTime day, DateTime at)>> GetAllPrayDaysAsync()
        {
            await EnsureTableAsync();
            var list = new List<(string, DateTime, DateTime)>();
            await using var con = Conn();
            await con.OpenAsync();
            // 사람 · 날짜마다 한 줄 (같은 날 여러 기도문을 눌러도 하루) — 그날 가장 늦게 누른 시각
            await using var cmd = new SqlCommand(
                "SELECT UserName, PrayDate, MAX(PrayedAt) FROM dbo.Prayer365PrayLog WITH (NOLOCK) GROUP BY UserName, PrayDate;", con);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add((r.GetString(0), r.GetDateTime(1), r.GetDateTime(2)));
            return list;
        }

        public async Task<Dictionary<string, string>> GetParticipantNamesAsync()
        {
            await EnsureTableAsync();
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            await using var con = Conn();
            await con.OpenAsync();
            // 기도 기록이 있는 아이디만 — 이름 = 성 + 이름 (AspNetUsers)
            await using var cmd = new SqlCommand(@"
SELECT u.UserName, LTRIM(RTRIM(ISNULL(u.LastName,'') + ISNULL(u.FirstName,'')))
FROM dbo.AspNetUsers u WITH (NOLOCK)
WHERE u.UserName IN (SELECT DISTINCT UserName FROM dbo.Prayer365PrayLog WITH (NOLOCK));", con);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                if (!r.IsDBNull(0) && !r.IsDBNull(1)) map[r.GetString(0)] = r.GetString(1);
            return map;
        }

        public async Task<bool> HasPrayedAsync(string md, string userName, DateTime prayDate)
        {
            await EnsureTableAsync();
            await using var con = Conn();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM dbo.Prayer365PrayLog WITH (NOLOCK) WHERE UserName=@User AND PrayDate=@Day AND MonthDay=@Md;", con);
            cmd.Parameters.Add("@Md", SqlDbType.Char, 4).Value = md;
            cmd.Parameters.Add("@Day", SqlDbType.Date).Value = prayDate.Date;
            cmd.Parameters.Add("@User", SqlDbType.NVarChar, 256).Value = userName;
            return (int)(await cmd.ExecuteScalarAsync() ?? 0) > 0;
        }

        public async Task<int> GetPrayCountAsync(string md)
        {
            await EnsureTableAsync();
            await using var con = Conn();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(
                "SELECT ISNULL((SELECT PrayCount FROM dbo.Prayer365 WITH (NOLOCK) WHERE MonthDay=@Md), 0) + (SELECT COUNT(*) FROM dbo.Prayer365PrayLog WITH (NOLOCK) WHERE MonthDay=@Md);", con);
            cmd.Parameters.Add("@Md", SqlDbType.Char, 4).Value = md;
            var v = await cmd.ExecuteScalarAsync();
            return v == null || v == DBNull.Value ? 0 : (int)v;
        }

        public async Task<int> AddViewAsync(string md, DateTime viewDate)
        {
            await EnsureTableAsync();
            await using var con = Conn();
            await con.OpenAsync();
            // 기도문 조회 수 +1, 기도문이 있으면 그날(KST) 조회 수도 +1
            await using var cmd = new SqlCommand(@"
SET XACT_ABORT ON;
DECLARE @v TABLE (Cnt INT);
BEGIN TRAN;
UPDATE dbo.Prayer365 SET ViewCount = ViewCount + 1 OUTPUT inserted.ViewCount INTO @v WHERE MonthDay=@Md;
IF EXISTS (SELECT 1 FROM @v)
BEGIN
    UPDATE dbo.Prayer365ViewDay WITH (UPDLOCK, HOLDLOCK) SET ViewCount = ViewCount + 1 WHERE ViewDate=@Day;
    IF @@ROWCOUNT = 0 INSERT INTO dbo.Prayer365ViewDay (ViewDate, ViewCount) VALUES (@Day, 1);
END
COMMIT;
SELECT ISNULL((SELECT TOP 1 Cnt FROM @v), 0);", con);
            cmd.Parameters.Add("@Md", SqlDbType.Char, 4).Value = md;
            cmd.Parameters.Add("@Day", SqlDbType.Date).Value = viewDate.Date;
            var v = await cmd.ExecuteScalarAsync();
            return v == null || v == DBNull.Value ? 0 : (int)v;
        }

        public async Task<(int today, int total)> GetViewStatsAsync(DateTime viewDate)
        {
            await EnsureTableAsync();
            await using var con = Conn();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(@"
SELECT ISNULL((SELECT ViewCount FROM dbo.Prayer365ViewDay WITH (NOLOCK) WHERE ViewDate=@Day), 0),
       ISNULL((SELECT SUM(CAST(ViewCount AS BIGINT)) FROM dbo.Prayer365 WITH (NOLOCK)), 0);", con);
            cmd.Parameters.Add("@Day", SqlDbType.Date).Value = viewDate.Date;
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return (0, 0);
            return (r.GetInt32(0), (int)Math.Min(int.MaxValue, r.GetInt64(1)));
        }

        public async Task<string?> GetTitleAsync(string md)
        {
            await EnsureTableAsync();
            await using var con = Conn();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("SELECT Title FROM dbo.Prayer365 WITH (NOLOCK) WHERE MonthDay=@Md;", con);
            cmd.Parameters.Add("@Md", SqlDbType.Char, 4).Value = md;
            var v = await cmd.ExecuteScalarAsync();
            return v == null || v == DBNull.Value ? null : (string)v;
        }

        public async Task<Dictionary<string, Prayer365Status>> GetStatusMapAsync()
        {
            await EnsureTableAsync();
            var map = new Dictionary<string, Prayer365Status>();
            await using var con = Conn();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("SELECT MonthDay, Title, IsReviewed FROM dbo.Prayer365 WITH (NOLOCK);", con);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                map[r.GetString(0)] = new Prayer365Status { MonthDay = r.GetString(0), Title = r.GetString(1), IsReviewed = r.GetBoolean(2) };
            return map;
        }

        public async Task<List<Prayer365Status>> SearchAsync(string q)
        {
            await EnsureTableAsync();
            var list = new List<Prayer365Status>();
            q = (q ?? "").Trim();
            if (q.Length == 0) return list;
            await using var con = Conn();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(@"
SELECT TOP 60 MonthDay, Title, IsReviewed FROM dbo.Prayer365 WITH (NOLOCK)
WHERE Title LIKE @Q OR Body LIKE @Q OR VerseRef LIKE @Q OR VerseText LIKE @Q
ORDER BY MonthDay;", con);
            // LIKE 특수문자는 글자 그대로 찾게 막는다
            var esc = q.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
            cmd.Parameters.Add("@Q", SqlDbType.NVarChar, 210).Value = "%" + esc + "%";
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(new Prayer365Status { MonthDay = r.GetString(0), Title = r.GetString(1), IsReviewed = r.GetBoolean(2) });
            return list;
        }

        public async Task SaveAsync(Prayer365Item item, string? by)
        {
            await EnsureTableAsync();
            await using var con = Conn();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(@"
MERGE dbo.Prayer365 AS t
USING (SELECT @Md AS MonthDay) AS s ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title=@Title, VerseRef=@VerseRef, VerseText=@VerseText, Body=@Body,
                             Fixes=@Fixes, IsReviewed=@Rev, UpdatedAt=@At, UpdatedBy=@By
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
                      VALUES (@Md, @Title, @VerseRef, @VerseText, @Body, @Fixes, @Rev, @At, @By);", con);
            cmd.Parameters.Add("@Md", SqlDbType.Char, 4).Value = item.MonthDay;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = item.Title ?? "";
            cmd.Parameters.Add("@VerseRef", SqlDbType.NVarChar, 100).Value = (object?)NullIfEmpty(item.VerseRef) ?? DBNull.Value;
            cmd.Parameters.Add("@VerseText", SqlDbType.NVarChar, 1000).Value = (object?)NullIfEmpty(item.VerseText) ?? DBNull.Value;
            cmd.Parameters.Add("@Body", SqlDbType.NVarChar, -1).Value = item.Body ?? "";
            cmd.Parameters.Add("@Fixes", SqlDbType.NVarChar, -1).Value = item.Fixes is { Count: > 0 } ? JsonConvert.SerializeObject(item.Fixes) : DBNull.Value;
            cmd.Parameters.Add("@Rev", SqlDbType.Bit).Value = item.IsReviewed;
            cmd.Parameters.Add("@At", SqlDbType.DateTime2).Value = DateHelper.GetKoreaNow();
            cmd.Parameters.Add("@By", SqlDbType.NVarChar, 100).Value = (object?)by ?? DBNull.Value;
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> DeleteAsync(string md)
        {
            await EnsureTableAsync();
            await using var con = Conn();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("DELETE FROM dbo.Prayer365 WHERE MonthDay=@Md;", con);
            cmd.Parameters.Add("@Md", SqlDbType.Char, 4).Value = md;
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}

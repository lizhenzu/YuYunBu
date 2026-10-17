using BulletinBoard.DataAccessLayer.Birthday.Models;
using BulletinBoard.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.DataAccessLayer.Birthday
{
    public class BirthdayRepository : IBirthdayRepository
    {
        private readonly string _connStr;

        public BirthdayRepository(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ViewPersonMaster2026Row>> GetNoBirthdayListAsync()
        {
            var sql = @"
SELECT
    v.*,
    ciPick.ClassYear,
    ciPick.ClassBan,

    -- ✅ 최신 선물 선택 1건(활성 우선)
    pPick.PickId,
    pPick.ProductId          AS PickProductId,
    pPick.ProductName        AS PickProductName,
    pPick.ProductDescription AS PickProductDescription,
    pPick.PickedAt           AS PickedAt,
    pPick.IsActive           AS PickIsActive,
    pPick.IsSent             AS PickIsSent,
    pPick.SentAt             AS PickSentAt,
    pPick.sourceType         AS PickSourceType,

    -- ✅ 선물 선택 요청 문자(SMS) 발송 여부
    smsPick.SmsSent          AS PickSmsSent,
    smsPick.SmsSentAt        AS PickSmsSentAt,

    -- ✅ 메모: BirthdayGiftPickMeMo (PersonID 키)
    memoRow.Memo             AS PickMemo

FROM dbo.view_PersonMaster_2026 v

OUTER APPLY (
    SELECT TOP 1 ci.ClassYear, ci.ClassBan
    FROM dbo.ClassInfo_2026 ci
    WHERE ci.PersonID = v.PersonId
      AND ci.PersonID IS NOT NULL
    ORDER BY ci.updateDT DESC, ci.Id DESC
) ciPick

OUTER APPLY (
    SELECT TOP 1
        pp.PickId, pp.ProductId, pp.PickedAt, pp.IsActive, pp.IsSent, pp.SentAt, pp.sourceType,
        ISNULL(a.ProductName,        pp.ProductName)        AS ProductName,
        ISNULL(a.ProductDescription, pp.ProductDescription) AS ProductDescription
    FROM dbo.PresentItemPick pp
    LEFT JOIN dbo.AwardItem a ON a.ProductID = pp.ProductId
    WHERE pp.UserId = v.UserId
      AND pp.UserId IS NOT NULL
    ORDER BY
        pp.IsActive DESC,
        pp.PickedAt DESC,
        pp.PickId DESC
) pPick

OUTER APPLY (
    SELECT TOP 1 s.*
    FROM dbo.BirthdayGiftPickSms s
    WHERE s.UserId = v.UserId
      AND s.UserId IS NOT NULL
    ORDER BY s.UpdatedAt DESC
) smsPick

OUTER APPLY (
    SELECT TOP 1 m.Memo
    FROM dbo.BirthdayGiftPickMeMo m
    WHERE m.PersonID = v.PersonId
) memoRow

WHERE v.PersonId IN (
    SELECT DISTINCT PersonID
    FROM dbo.ClassInfo_2026
    WHERE PersonID IS NOT NULL
)
AND (
    ISNULL(v.bMonth, 0) = 0 OR ISNULL(v.bDay, 0) = 0
)
ORDER BY v.fullName;";

            var list = new List<ViewPersonMaster2026Row>();

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                list.Add(MapViewRow(reader));

            return list;
        }

        public async Task<List<ViewPersonMaster2026Row>> GetListAsync(int? month)
        {
            var sql = @"
SELECT
    v.*,
    ciPick.ClassYear,
    ciPick.ClassBan,

    -- ✅ 최신 선물 선택 1건(활성 우선)
    pPick.PickId,
    pPick.ProductId          AS PickProductId,
    pPick.ProductName        AS PickProductName,
    pPick.ProductDescription AS PickProductDescription,
    pPick.PickedAt           AS PickedAt,
    pPick.IsActive           AS PickIsActive,
    pPick.IsSent             AS PickIsSent,
    pPick.SentAt             AS PickSentAt,
    pPick.sourceType         AS PickSourceType,

    -- ✅ 선물 선택 요청 문자(SMS) 발송 여부
    smsPick.SmsSent          AS PickSmsSent,
    smsPick.SmsSentAt        AS PickSmsSentAt,

    -- ✅ 메모: BirthdayGiftPickMeMo (PersonID 키)
    memoRow.Memo             AS PickMemo

FROM dbo.view_PersonMaster_2026 v

OUTER APPLY (
    SELECT TOP 1 ci.ClassYear, ci.ClassBan
    FROM dbo.ClassInfo_2026 ci
    WHERE ci.PersonID = v.PersonId
      AND ci.PersonID IS NOT NULL
    ORDER BY ci.updateDT DESC, ci.Id DESC
) ciPick

OUTER APPLY (
    SELECT TOP 1
        pp.PickId, pp.ProductId, pp.PickedAt, pp.IsActive, pp.IsSent, pp.SentAt, pp.sourceType,
        ISNULL(a.ProductName,        pp.ProductName)        AS ProductName,
        ISNULL(a.ProductDescription, pp.ProductDescription) AS ProductDescription
    FROM dbo.PresentItemPick pp
    LEFT JOIN dbo.AwardItem a ON a.ProductID = pp.ProductId
    WHERE pp.UserId = v.UserId
      AND pp.UserId IS NOT NULL
    ORDER BY
        pp.IsActive DESC,
        pp.PickedAt DESC,
        pp.PickId DESC
) pPick

OUTER APPLY (
    SELECT TOP 1 s.*
    FROM dbo.BirthdayGiftPickSms s
    WHERE s.UserId = v.UserId
      AND s.UserId IS NOT NULL
    ORDER BY s.UpdatedAt DESC
) smsPick

OUTER APPLY (
    SELECT TOP 1 m.Memo
    FROM dbo.BirthdayGiftPickMeMo m
    WHERE m.PersonID = v.PersonId
) memoRow

WHERE v.PersonId IN (
    SELECT DISTINCT PersonID
    FROM dbo.ClassInfo_2026
    WHERE PersonID IS NOT NULL
)
" + (month.HasValue ? " AND v.bMonth = @Month " : "") + @"
ORDER BY v.bMonth, v.bDay, v.fullName;";

            var list = new List<ViewPersonMaster2026Row>();

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            if (month.HasValue)
                cmd.Parameters.AddWithValue("@Month", month.Value);
 
            DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");


            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapViewRow(reader));
            }

            return list;
        }

        public async Task<ViewPersonMaster2026Row?> GetOneAsync(int personId)
        {
            var sql = @"
SELECT TOP 1
    v.*,
    ciPick.ClassYear,
    ciPick.ClassBan,

    -- ✅ 최신 선물 선택 1건(활성 우선)
    pPick.PickId,
    pPick.ProductId          AS PickProductId,
    pPick.ProductName        AS PickProductName,
    pPick.ProductDescription AS PickProductDescription,
    pPick.PickedAt           AS PickedAt,
    pPick.IsActive           AS PickIsActive,
    pPick.IsSent             AS PickIsSent,
    pPick.SentAt             AS PickSentAt,
    pPick.sourceType         AS PickSourceType,

    -- ✅ 선물 선택 요청 문자(SMS) 발송 여부
    smsPick.SmsSent          AS PickSmsSent,
    smsPick.SmsSentAt        AS PickSmsSentAt,

    -- ✅ 메모: BirthdayGiftPickMeMo (PersonID 키)
    memoRow.Memo             AS PickMemo

FROM dbo.view_PersonMaster_2026 v

OUTER APPLY (
    SELECT TOP 1 ci.ClassYear, ci.ClassBan
    FROM dbo.ClassInfo_2026 ci
    WHERE ci.PersonID = v.PersonId
      AND ci.PersonID IS NOT NULL
    ORDER BY ci.updateDT DESC, ci.Id DESC
) ciPick

OUTER APPLY (
    SELECT TOP 1
        pp.PickId, pp.ProductId, pp.PickedAt, pp.IsActive, pp.IsSent, pp.SentAt, pp.sourceType,
        ISNULL(a.ProductName,        pp.ProductName)        AS ProductName,
        ISNULL(a.ProductDescription, pp.ProductDescription) AS ProductDescription
    FROM dbo.PresentItemPick pp
    LEFT JOIN dbo.AwardItem a ON a.ProductID = pp.ProductId
    WHERE pp.UserId = v.UserId
      AND pp.UserId IS NOT NULL
    ORDER BY
        pp.IsActive DESC,
        pp.PickedAt DESC,
        pp.PickId DESC
) pPick

OUTER APPLY (
    SELECT TOP 1 s.*
    FROM dbo.BirthdayGiftPickSms s
    WHERE s.UserId = v.UserId
      AND s.UserId IS NOT NULL
    ORDER BY s.UpdatedAt DESC
) smsPick

OUTER APPLY (
    SELECT TOP 1 m.Memo
    FROM dbo.BirthdayGiftPickMeMo m
    WHERE m.PersonID = v.PersonId
) memoRow

WHERE v.PersonId = @PersonId
  AND v.PersonId IN (
      SELECT DISTINCT PersonID
      FROM dbo.ClassInfo_2026
      WHERE PersonID IS NOT NULL
  );";

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PersonId", personId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapViewRow(reader);

            return null;
        }

        // -----------------------
        // Data Reader Mapping
        // -----------------------
        private static ViewPersonMaster2026Row MapViewRow(SqlDataReader r)
        {
            return new ViewPersonMaster2026Row
            {
                PersonId = GetInt(r, "PersonId") ?? 0,
                UserId = GetString(r, "UserId"),
                PhoneNumber = GetString(r, "PhoneNumber"),
                bYear = GetInt(r, "bYear"),
                bMonth = GetInt(r, "bMonth"),
                bDay = GetInt(r, "bDay"),
                fullName = GetString(r, "fullName"),
                sex = GetString(r, "sex"),
                PhoneNumber2 = GetString(r, "PhoneNumber2"),
                HomeAaddress = GetString(r, "HomeAaddress"),
                school = GetString(r, "school"),
                readBibleAllCount = GetInt(r, "readBibleAllCount"),
                plusMins = GetString(r, "plusMins"),
                birthDay_PublishYN = GetString(r, "birthDay_PublishYN"),
                UserName = GetString(r, "UserName"),
                LastName = GetString(r, "LastName"),
                FirstName = GetString(r, "FirstName"),
                email = GetString(r, "email"),
                insertDT = GetDateTime(r, "insertDT"),
                updateDT = GetDateTime(r, "updateDT"),

                // 조인 컬럼(반)
                ClassYear = GetInt(r, "ClassYear"),
                ClassBan = GetInt(r, "ClassBan"),

                // ✅ 선물 선택 조인 컬럼
                PickId = GetInt(r, "PickId"),
                PickProductId = GetInt(r, "PickProductId"),
                PickProductName = GetString(r, "PickProductName"),
                PickProductDescription = GetString(r, "PickProductDescription"),
                PickedAt = GetDateTime(r, "PickedAt"),
                PickIsActive = GetBool(r, "PickIsActive"),
                PickIsSent = GetBool(r, "PickIsSent"),
                PickSentAt = GetDateTime(r, "PickSentAt"),
                PickSourceType = GetString(r, "PickSourceType"),

                // ✅ 선물 선택 요청 문자(SMS)
                PickSmsSent = GetBool(r, "PickSmsSent"),
                PickSmsSentAt = GetDateTime(r, "PickSmsSentAt"),
                PickMemo = GetString(r, "PickMemo"),
            };
        }

        private static int? GetInt(SqlDataReader r, string col)
        {
            var i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? (int?)null : r.GetInt32(i);
        }

        private static bool? GetBool(SqlDataReader r, string col)
        {
            int i;
            try { i = r.GetOrdinal(col); }
            catch (IndexOutOfRangeException) { return null; }

            return r.IsDBNull(i) ? (bool?)null : r.GetBoolean(i);
        }

        private static string? GetString(SqlDataReader r, string col)
        {
            int i;
            try { i = r.GetOrdinal(col); }
            catch (IndexOutOfRangeException) { return null; }

            return r.IsDBNull(i) ? null : r.GetString(i);
        }

        private static DateTime? GetDateTime(SqlDataReader r, string col)
        {
            int i;
            try { i = r.GetOrdinal(col); }
            catch (IndexOutOfRangeException) { return null; }

            return r.IsDBNull(i) ? (DateTime?)null : r.GetDateTime(i);
        }

        // -----------------------------------------
        // ✅ 기존: 선물 상태 업데이트(PresentItemPick)
        // -----------------------------------------
        public async Task<int> UpdateGiftStateAsync(string userId, bool isActive, bool isSent)
        {
            var sql = @"
;WITH target AS (
    SELECT TOP 1 PickId
    FROM PresentItemPick WITH (READPAST)
    WHERE UserId = @UserId AND IsActive = 1
    ORDER BY PickedAt DESC, PickId DESC
),
fallback AS (
    SELECT TOP 1 PickId
    FROM PresentItemPick WITH (READPAST)
    WHERE UserId = @UserId
    ORDER BY PickedAt DESC, PickId DESC
),
pick AS (
    SELECT PickId FROM target
    UNION ALL
    SELECT PickId FROM fallback
)
UPDATE p
SET
    p.IsActive = @IsActive,
    p.IsSent = @IsSent,
    p.SentAt = CASE 
                    WHEN @IsSent = 1 THEN ISNULL(p.SentAt, @SentAt) 
                    ELSE NULL 
               END
FROM PresentItemPick p
WHERE p.PickId = (SELECT TOP 1 PickId FROM pick);
";
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@IsActive", isActive);
            cmd.Parameters.AddWithValue("@IsSent", isSent);
            cmd.Parameters.AddWithValue("@SentAt", DateHelper.GetKoreaNow());
            var affected = await cmd.ExecuteNonQueryAsync();
            return affected;
        }

        // -----------------------------------------
        // ✅ 선물 선택 요청 문자(SMS) 발송 여부 업서트
        // -----------------------------------------
        public async Task<int> UpdatePickSmsStateAsync(string userId, bool smsSent)
        {
            var sql = @"
MERGE dbo.BirthdayGiftPickSms AS tgt
USING (SELECT @UserId AS UserId) AS src
ON (tgt.UserId = src.UserId)
WHEN MATCHED THEN
    UPDATE SET
        SmsSent   = @SmsSent,
        SmsSentAt = CASE WHEN @SmsSent = 1 THEN ISNULL(tgt.SmsSentAt, @SmsSentAt) ELSE NULL END,
        UpdatedAt = @SmsSentAt
WHEN NOT MATCHED THEN
    INSERT (UserId, SmsSent, SmsSentAt, UpdatedAt)
    VALUES (
        @UserId,
        @SmsSent,
        CASE WHEN @SmsSent = 1 THEN @SmsSentAt ELSE NULL END,
        @SmsSentAt
    );";

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@SmsSent", smsSent);
            cmd.Parameters.AddWithValue("@SmsSentAt", DateHelper.GetKoreaNow());
            return await cmd.ExecuteNonQueryAsync();
        }

        // -----------------------------------------
        // ✅ 메모 업서트: BirthdayGiftPickMeMo (PersonID 키)
        // -----------------------------------------
        public async Task<int> UpsertMemoAsync(int personId, string? memo)
        {
            var sql = @"
IF EXISTS (SELECT 1 FROM dbo.BirthdayGiftPickMeMo WHERE PersonID = @PersonId)
    UPDATE dbo.BirthdayGiftPickMeMo SET Memo = @Memo WHERE PersonID = @PersonId;
ELSE
    INSERT INTO dbo.BirthdayGiftPickMeMo (PersonID, Memo) VALUES (@PersonId, @Memo);";

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PersonId", personId);
            cmd.Parameters.AddWithValue("@Memo", (object?)memo ?? DBNull.Value);
            return await cmd.ExecuteNonQueryAsync();
        }
    }
}

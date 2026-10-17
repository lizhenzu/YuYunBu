using BulletinBoard.BusinessLayer;
using BulletinBoard.DataAccessLayer;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Models.DailyMission2;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Services.DailyMission2
{
    public class DailyMission2Service : IDailyMission2Service
    {
        private readonly string _connectionString;

        public DailyMission2Service(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private static DateTime AsMidnight(DateTime d) => d.Date; // 00:00

        // ✅ SaveResult에 IsRead/IsBookmarked 같은 속성이 없어도 컴파일되게(컨트롤러는 reflection으로 읽음)
        private class SaveResultEx : SaveResult
        {
            public bool IsRead { get; set; }
            public bool IsBookmarked { get; set; }
        }
        public async Task<int> GetTargetPointFromAwardBaseAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT ISNULL(MAX(AwardLevel), 0) 
                FROM dbo.AwardBase WITH (NOLOCK);";

            using var cmd = new SqlCommand(sql, conn);
            var obj = await cmd.ExecuteScalarAsync();

            return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
        }


        /// <summary>
        /// 시상 기준 점수(AwardBase.AwardLevel)를 작은 것부터 돌려 준다.
        /// "다음 선물까지 몇 점 남았는지" 를 계산하는 데 쓴다.
        /// </summary>
        public async Task<List<int>> GetAwardLevelsAsync()
        {
            var list = new List<int>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT AwardLevel
                        FROM dbo.AwardBase WITH (NOLOCK)
                        WHERE AwardLevel > 0
                        ORDER BY AwardLevel;";

            using var cmd = new SqlCommand(sql, conn);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(Convert.ToInt32(r["AwardLevel"]));
            }
            return list;
        }


        private async Task<List<FamilyMapItem>> LoadFamilyMapAsync()
        {
            var list = new List<FamilyMapItem>();

            const string sql = @"
SELECT
    m.FamilyTeamId,
    t.TeamName,
    m.UserName,
    m.RoleCode,
    m.SortNo,
    m.RelationCode
FROM dbo.DM2_FamilyMember m WITH (NOLOCK)
JOIN dbo.DM2_FamilyTeam t WITH (NOLOCK) ON t.FamilyTeamId = m.FamilyTeamId
WHERE m.IsActive = 1 AND t.IsActive = 1;
";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();                // ✅ 이 한 줄이 핵심
            using var cmd = new SqlCommand(sql, conn);

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new FamilyMapItem
                {
                    FamilyTeamId = rd.GetInt32(0),
                    TeamName = rd.GetString(1),
                    UserName = (rd.GetString(2) ?? "").Trim(),
                    RoleCode = Convert.ToInt32(rd.GetByte(3)),
                    SortNo = rd.GetInt32(4),
                    RelationCode = rd.IsDBNull(5) ? 9 : Convert.ToInt32(rd.GetByte(5))
                });
            }

            return list;
        }


        public async Task<List<ParticipantCardRow>> GetParticipantCardsAsync(int year)
        {
            // ✅ (구) 혼합 페이지용: 가족팀 구성원은 개인 목록에서 제외하고, 가족팀 카드 + 개인 카드로 섞어 보여줌
            var basePeople = await LoadParticipantBaseAsync(year, includeFamilyTeamInfoForPerson: false);

            var familyMap = await LoadFamilyMapAsync();
            var mapByUser = familyMap
                .Where(x => !string.IsNullOrWhiteSpace(x.UserName))
                .GroupBy(x => x.UserName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var familyPeople = new List<(ParticipantCardRow row, FamilyMapItem map)>();
            var individuals = new List<ParticipantCardRow>();

            foreach (var p in basePeople)
            {
                var key = (p.UserName ?? "").Trim();
                if (key.Length > 0 && mapByUser.TryGetValue(key, out var m))
                    familyPeople.Add((p, m));
                else
                    individuals.Add(p);
            }

            var familyCards = BuildFamilyCards(familyPeople);

            var merged = new List<ParticipantCardRow>(familyCards.Count + individuals.Count);
            merged.AddRange(familyCards);
            merged.AddRange(individuals);

            return merged
                .OrderByDescending(x => x.YearPoint)
                .ThenBy(x => x.IsFamily ? 0 : 1)
                .ThenBy(x => x.FullName ?? "")
                .ToList();
        }

        public async Task<List<ParticipantCardRow>> GetParticipantPersonCardsAsync(int year)
        {
            // ✅ 개인 페이지용: 개인 카드만(= 사람 목록) 반환
            // ✅ 중요: 가족팀 소속이어도 개인 목록에서 제외하지 않는다.
            var people = await LoadParticipantBaseAsync(year, includeFamilyTeamInfoForPerson: true);

            return people
                .OrderByDescending(x => x.YearPoint)
                .ThenBy(x => x.FullName ?? "")
                .ToList();
        }

        public async Task<List<ParticipantCardRow>> GetParticipantTeamCardsAsync(int year)
        {
            // ── 최적화: 가족 구성원만 단일 쿼리로 조회 (streak CROSS APPLY 제거, 전체 유저 스캔 제거)
            var from = new DateTime(year, 1, 1);
            var to   = new DateTime(year, 12, 31);

            const string sql = @"
SELECT
    m.FamilyTeamId,
    t.TeamName,
    m.RoleCode,
    m.RelationCode,
    m.SortNo,
    u.Id        AS UserId,
    u.UserName,
    ISNULL(pm.FullName, u.UserName)           AS FullName,
    ISNULL(ui.ProfilePicture, '')             AS ProfilePicture,
    ci.ClassYear,
    ci.ClassBan,
    ISNULL(dm.YearPoint, 0)                   AS YearPoint,
    dm.LastActionDate
FROM dbo.DM2_FamilyMember m WITH (NOLOCK)
JOIN dbo.DM2_FamilyTeam   t WITH (NOLOCK) ON t.FamilyTeamId = m.FamilyTeamId AND t.IsActive = 1
JOIN [Identity].[User]    u WITH (NOLOCK) ON u.UserName = m.UserName
LEFT JOIN dbo.view_PersonMaster_2026     pm WITH (NOLOCK) ON pm.UserId = u.Id
LEFT JOIN dbo.view_UserCompositeInfo_2026 ui WITH (NOLOCK) ON ui.UserId = u.Id
LEFT JOIN dbo.ClassInfo_2026              ci WITH (NOLOCK) ON ci.PersonId = pm.PersonId
CROSS APPLY (
    SELECT
        SUM(ISNULL(x.TotalPoint, 0))                                        AS YearPoint,
        MAX(CASE WHEN ISNULL(x.TotalPoint,0) > 0 THEN x.DateBy END)        AS LastActionDate
    FROM dbo.ufn_DM2_GetUserDailyPoints(u.UserName, @F, @T) x
) dm
WHERE m.IsActive = 1
ORDER BY m.FamilyTeamId, m.RoleCode, m.RelationCode;";

            var rows = new List<(ParticipantCardRow row, FamilyMapItem map)>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(sql, conn) { CommandTimeout = 120 };
            cmd.Parameters.AddWithValue("@F", from);
            cmd.Parameters.AddWithValue("@T", to);
            await conn.OpenAsync();

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var map = new FamilyMapItem
                {
                    FamilyTeamId = rd.GetInt32(0),
                    TeamName     = rd.GetString(1),
                    RoleCode     = Convert.ToInt32(rd.GetByte(2)),
                    RelationCode = rd.IsDBNull(3) ? 9 : Convert.ToInt32(rd.GetByte(3)),
                    SortNo       = rd.GetInt32(4),
                    UserName     = rd.IsDBNull(6) ? "" : rd.GetString(6).Trim(),
                };
                var prow = new ParticipantCardRow
                {
                    UserId         = rd.IsDBNull(5)  ? "" : rd.GetString(5),
                    UserName       = map.UserName,
                    FullName       = rd.IsDBNull(7)  ? map.UserName : rd.GetString(7),
                    ProfilePicture = rd.IsDBNull(8)  ? "" : rd.GetString(8),
                    ClassYear      = rd.IsDBNull(9)  ? null : (int?)rd.GetInt32(9),
                    ClassBan       = rd.IsDBNull(10) ? null : (int?)rd.GetInt32(10),
                    YearPoint      = rd.GetInt32(11),
                    LastActionDate = rd.IsDBNull(12) ? null : (DateTime?)rd.GetDateTime(12),
                };
                rows.Add((prow, map));
            }

            if (rows.Count == 0) return new();

            var familyCards = BuildFamilyCards(rows);
            return familyCards
                .OrderByDescending(x => x.YearPoint)
                .ThenBy(x => x.FamilyTeamName ?? x.FullName ?? "")
                .ToList();
        }

        private async Task<List<ParticipantCardRow>> LoadParticipantBaseAsync(int year, bool includeFamilyTeamInfoForPerson)
        {
            var list = new List<ParticipantCardRow>();

            var from = new DateTime(year, 1, 1);
            var to = new DateTime(year, 12, 31);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
;WITH U AS (
    SELECT u.Id AS UserId, u.UserName
    FROM [Identity].[User] u WITH (NOLOCK)
    WHERE ISNULL(u.UserName,'') <> ''
)
SELECT
    u.UserId,
    u.UserName,

    pm.FullName,
    pm.PersonId,

    ci.ClassYear,
    ci.ClassBan,

    ap.DeptCode,
    ap.RoleCode,
    cd.MinorNM AS RoleName,

    ui.ProfilePicture,

    ISNULL(dm.YearPoint, 0) AS YearPoint,
    dm.LastActionDate,
    ISNULL(streak.ConsecutiveDays, 0) AS ConsecutiveDays

FROM U u
LEFT JOIN dbo.view_PersonMaster_2026 pm WITH (NOLOCK)
    ON pm.UserId = u.UserId
LEFT JOIN dbo.ClassInfo_2026 ci WITH (NOLOCK)
    ON ci.PersonId = pm.PersonId
LEFT JOIN dbo.view_All_Person_2026 ap WITH (NOLOCK)
    ON ap.UserId = u.UserId
LEFT JOIN dbo.CodeData cd WITH (NOLOCK)
    ON cd.MajorCD = 'T001'
   AND cd.MinorCD = ap.RoleCode
LEFT JOIN dbo.view_UserCompositeInfo_2026 ui WITH (NOLOCK)
    ON ui.UserId = u.UserId

CROSS APPLY (
    SELECT
        SUM(ISNULL(x.TotalPoint,0)) AS YearPoint,
        MAX(CASE WHEN ISNULL(x.TotalPoint,0) > 0 THEN x.DateBy END) AS LastActionDate
    FROM dbo.ufn_DM2_GetUserDailyPoints(u.UserName, @F, @T) x
) dm
CROSS APPLY (
    SELECT ISNULL(COUNT(*), 0) AS ConsecutiveDays
    FROM (
        SELECT
            DATEDIFF(day, a.DateBy, dm.LastActionDate)
            - (ROW_NUMBER() OVER (ORDER BY a.DateBy DESC) - 1) AS grp
        FROM (
            SELECT DateBy
            FROM dbo.ufn_DM2_GetUserDailyPoints(u.UserName, @F, @T)
            WHERE ISNULL(TotalPoint, 0) > 0
        ) a
    ) t
    WHERE t.grp = 0
) streak
WHERE ISNULL(dm.YearPoint,0) > 0
ORDER BY ISNULL(dm.YearPoint,0) DESC, ISNULL(pm.FullName,'') ASC;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@F", from);
            cmd.Parameters.AddWithValue("@T", to);
     
            DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var row = new ParticipantCardRow
                {
                    UserId = r["UserId"]?.ToString() ?? "",
                    UserName = r["UserName"]?.ToString() ?? "",
                    FullName = r["FullName"]?.ToString() ?? "",
                    ProfilePicture = r["ProfilePicture"]?.ToString() ?? "",
                    YearPoint = Convert.ToInt32(r["YearPoint"]),
                    LastActionDate = (r["LastActionDate"] == DBNull.Value) ? null : (DateTime?)Convert.ToDateTime(r["LastActionDate"]),
                    ConsecutiveDays = (r["ConsecutiveDays"] == DBNull.Value) ? 0 : Convert.ToInt32(r["ConsecutiveDays"]),
                    ClassYear = (r["ClassYear"] == DBNull.Value) ? null : (int?)Convert.ToInt32(r["ClassYear"]),
                    ClassBan = (r["ClassBan"] == DBNull.Value) ? null : (int?)Convert.ToInt32(r["ClassBan"]),
                    DeptCode = r["DeptCode"]?.ToString() ?? "",
                    RoleCode = r["RoleCode"]?.ToString() ?? "",
                    RoleName = r["RoleName"]?.ToString() ?? "",
                };

                // ✅ 역할 판별 (학생/교사/학부모(확인필요))
                row.IsStudent = row.ClassYear.HasValue && row.ClassBan.HasValue;
                row.IsParent = (!row.IsStudent && string.IsNullOrWhiteSpace(row.RoleName));
                row.IsTeacher = (!row.IsStudent && !row.IsParent);

                // ✅ 화면용 텍스트
                if (row.IsStudent)
                    row.DisplayRoleText = $"학생 ({row.ClassYear}학년 {row.ClassBan}반)";
                else
                    row.DisplayRoleText = string.IsNullOrWhiteSpace(row.RoleName) ? "확인필요" : $"{row.RoleName}";

                list.Add(row);
            }

            if (includeFamilyTeamInfoForPerson)
            {
                var familyMap = await LoadFamilyMapAsync();
                var teamByUser = familyMap
                    .Where(x => !string.IsNullOrWhiteSpace(x.UserName))
                    .GroupBy(x => x.UserName.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                foreach (var p in list)
                {
                    var key = (p.UserName ?? "").Trim();
                    if (key.Length > 0 && teamByUser.TryGetValue(key, out var m))
                    {
                        p.FamilyTeamId = m.FamilyTeamId;
                        p.FamilyTeamName = m.TeamName ?? "";
                    }
                }
            }

            return list;
        }

        private static List<ParticipantCardRow> BuildFamilyCards(List<(ParticipantCardRow row, FamilyMapItem map)> familyPeople)
        {
            var familyCards = new List<ParticipantCardRow>();

            foreach (var g in familyPeople.GroupBy(x => x.map.FamilyTeamId))
            {
                var members = g
                    .Select(x =>
                    {
                        var roleLabel = x.map.RoleCode switch
                        {
                            0 => "학생",
                            1 => "엄마",
                            2 => "아빠",
                            3 => "할머니",
                            4 => "할아버지",
                            _ => "구성원"
                        };

                        return new FamilyMemberRow
                        {
                            UserId = x.row.UserId,
                            UserName = x.row.UserName,
                            FullName = x.row.FullName,
                            ProfilePicture = x.row.ProfilePicture,
                            RoleCode = x.map.RoleCode,
                            RoleLabel = roleLabel,
                            RelationCode = x.map.RelationCode,
                            ClassYear = x.row.ClassYear,   // ✅ 학생 학년
                            ClassBan  = x.row.ClassBan,    // ✅ 학생 반
                            YearPoint = x.row.YearPoint,
                            LastActionDate = x.row.LastActionDate
                        };
                    })
                    .OrderBy(m => m.RoleCode)
                    .ThenBy(m => m.FullName)
                    .ToList();

                var student = members.FirstOrDefault(m => m.RoleCode == 0) ?? members.First();

                decimal weighted = 0m;
                foreach (var m in members)
                {
                    var w = (m.RoleCode == 0) ? 1.0m : 1.0m;
                    weighted += (m.YearPoint * w);
                }
                int totalPoint = (int)Math.Round(weighted, 0, MidpointRounding.AwayFromZero);

                var teamName = g.First().map.TeamName;
                if (string.IsNullOrWhiteSpace(teamName))
                    teamName = $"{student.FullName} 가족";

                DateTime? last = members
                    .Where(m => m.LastActionDate.HasValue)
                    .Select(m => m.LastActionDate!.Value)
                    .DefaultIfEmpty()
                    .Max();

                familyCards.Add(new ParticipantCardRow
                {
                    IsFamily = true,
                    FamilyTeamId = g.Key,
                    FamilyTeamName = teamName,
                    FamilyMembers = members,

                    UserId = student.UserId,
                    UserName = student.UserName,
                    FullName = teamName,
                    ProfilePicture = student.ProfilePicture,

                    YearPoint = totalPoint,
                    LastActionDate = last,

                    // ✅ 대표 학생의 학년/반 설정 (카드 헤더 배지 표시용)
                    ClassYear = g.Where(x => x.map.RoleCode == 0).Select(x => x.row).FirstOrDefault()?.ClassYear,
                    ClassBan  = g.Where(x => x.map.RoleCode == 0).Select(x => x.row).FirstOrDefault()?.ClassBan,

                    IsStudent = true,
                    IsTeacher = false,
                    IsParent = false,
                    DisplayRoleText = "가족"
                });
            }

            return familyCards;
        }

        public async Task<int> GetUserTotalPointsAsync(string userName, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrWhiteSpace(userName)) return 0;

            // 날짜 뒤집힘 방어
            if (toDate < fromDate)
            {
                var tmp = fromDate;
                fromDate = toDate;
                toDate = tmp;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // ✅ 핵심: ufn_DM2_GetUserDailyPoints 결과에서 TotalPoint SUM
            const string sql = @"
SELECT ISNULL(SUM(TotalPoint), 0)
FROM dbo.ufn_DM2_GetUserDailyPoints(@U, @F, @T);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@U", SqlDbType.NVarChar, 100) { Value = userName });
            cmd.Parameters.Add(new SqlParameter("@F", SqlDbType.Date) { Value = fromDate.Date });
            cmd.Parameters.Add(new SqlParameter("@T", SqlDbType.Date) { Value = toDate.Date });

            // (선택) 디버그 로그
            try
            { 
                await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            }
            catch { /* ignore */ }

            var obj = await cmd.ExecuteScalarAsync();
            var totalValue = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            return totalValue;

        }

        public async Task<int> GetUserTotalPointsThisYearAsync(string userName, int year)
        {
            if (string.IsNullOrWhiteSpace(userName)) return 0;
            if (year <= 0) year = DateHelper.GetKoreaNow().Year;

            // 연간 점수 계산은 GetYearPointAsync 하나로 통일한다 (홈 · 미션홈 · Branch 모두 같은 값)
            var yp = await GetYearPointAsync(userName, year);
            return yp.YearTotalPoint;
        }

        // ------------------------------------------------------------
        // ✅ TodayWordToRead 목록(기존 BLL 재사용)
        // ------------------------------------------------------------
        public async Task<List<TodayWordToReads>> GetTodayWordAsync(int Id, bool activeData, DateTime workDT)
        {
            ManageTodayWordToRead manageTodayWordToRead = new ManageTodayWordToRead();
            List<TodayWordToReads> TodayWordToReads = manageTodayWordToRead.getTodayWordToRead(Id, activeData, workDT);
            return TodayWordToReads;
        }

        public async Task<List<UserDailyPointRow>> GetUserDailyPointsAsync(string userName, DateTime fromDate, DateTime toDate)
        {
            var list = new List<UserDailyPointRow>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
SELECT 
    DateBy,
    WordPoint, QuizPoint, PrayPoint, RecitePoint,
    TotalPoint
FROM dbo.ufn_DM2_GetUserDailyPoints(@U, @F, @T)
ORDER BY DateBy;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@U", SqlDbType.NVarChar, 100) { Value = userName });
            cmd.Parameters.Add(new SqlParameter("@F", SqlDbType.Date) { Value = fromDate.Date });
            cmd.Parameters.Add(new SqlParameter("@T", SqlDbType.Date) { Value = toDate.Date });
 
            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new UserDailyPointRow
                {
                    DateBy = r.GetDateTime(r.GetOrdinal("DateBy")),
                    WordPoint = Convert.ToInt32(r["WordPoint"]),
                    QuizPoint = Convert.ToInt32(r["QuizPoint"]),
                    PrayPoint = Convert.ToInt32(r["PrayPoint"]),
                    RecitePoint = Convert.ToInt32(r["RecitePoint"]),
                    TotalPoint = Convert.ToInt32(r["TotalPoint"]),
                });
            }

            return list;
        }
     
    // ------------------------------------------------------------
    // ✅ TodayWordToRead 상세 + 본문(eBibles) 가져오기
    // ------------------------------------------------------------
    public async Task<TodayWordDetailResult?> GetTodayWordDetailAsync(DateTime selectedDate, string langCD, string userId)
        {
            var day = AsMidnight(selectedDate);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
SELECT TOP 1
    Id, ExposeDate, BibleSubject, BibleText, FromJang, FromZul, ToJang, ToZul,
    BibleSummary, synopsis
FROM dbo.TodayWordToRead
WHERE CONVERT(date, ExposeDate) = @D
ORDER BY Id;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = day });

            // 🔍 디버그 SQL 출력 (있으면)
            try
            { 
                DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            }
            catch { /* ignore */ }

            using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync())
                return null;

            var detail = new TodayWordDetailResult
            {
                Id = Convert.ToInt32(r["Id"]),
                ExposeDate = Convert.ToDateTime(r["ExposeDate"]),
                BibleSubject = r["BibleSubject"] as string,
                BibleText = (r["BibleText"] as string) ?? "",
                FromJang = Convert.ToInt32(r["FromJang"]),
                FromZul = Convert.ToInt32(r["FromZul"]),
                ToJang = Convert.ToInt32(r["ToJang"]),
                ToZul = Convert.ToInt32(r["ToZul"]),
                BibleSummary = r["BibleSummary"] as string,
                synopsis = r["synopsis"] as string
            };

            // ✅ ManageeBible로 본문 가져오기 (실패해도 상세는 표시되도록)
            try
            {
                ManageeBible manageeBible = new ManageeBible();
                detail.TodayeBiblesData = manageeBible.geteBible2(
                    detail.BibleText,
                    detail.FromJang, detail.FromZul,
                    detail.ToJang, detail.ToZul,
                    langCD, userId
                );

                DbLogger.LogDebugAsync($"성경책 정보 : {detail.TodayeBiblesData?.eBibleDatas?.Count ?? 0}");
            }
            catch
            {
                detail.TodayeBiblesData = null;
            }

            return detail;
        }

        // ------------------------------------------------------------
        // ✅ 구절 범위로 본문 가져오기 (날짜가 아니라 넘겨받은 범위 기준)
        //    예) 주일공과 읽을말씀 → 로마서 8:18~22
        //    화면은 BibleRead 와 같은 모양(TodayWordDetailResult)으로 쓴다.
        //    잘못된 범위(성경 이름 없음, 장 0 이하, 끝이 시작보다 앞)면 null.
        // ------------------------------------------------------------
        public Task<TodayWordDetailResult?> GetWordDetailByRangeAsync(
            string bibleText, int fromJang, int fromZul, int toJang, int toZul, string langCD, string userId)
        {
            bibleText = (bibleText ?? "").Trim();
            if (string.IsNullOrEmpty(bibleText) || fromJang <= 0) return Task.FromResult<TodayWordDetailResult?>(null);

            // 끝 장이 없으면 시작 장 한 장만
            if (toJang <= 0) toJang = fromJang;
            if (fromZul < 0) fromZul = 0;
            if (toZul   < 0) toZul   = 0;
            if (toJang < fromJang || (toJang == fromJang && toZul > 0 && fromZul > 0 && toZul < fromZul))
                return Task.FromResult<TodayWordDetailResult?>(null);

            var detail = new TodayWordDetailResult
            {
                Id           = 0,                           // TodayWordToRead 행이 아님
                ExposeDate   = DateHelper.GetKoreaNow().Date,
                BibleSubject = null,
                BibleText    = bibleText,
                FromJang     = fromJang,
                FromZul      = fromZul,
                ToJang       = toJang,
                ToZul        = toZul,
                BibleSummary = null,
                synopsis     = null
            };

            // 본문 — GetTodayWordDetailAsync 와 같은 방식 (실패해도 상세는 돌려준다)
            try
            {
                var manageeBible = new ManageeBible();
                detail.TodayeBiblesData = manageeBible.geteBible2(
                    detail.BibleText,
                    detail.FromJang, detail.FromZul,
                    detail.ToJang, detail.ToZul,
                    string.IsNullOrWhiteSpace(langCD) ? "al" : langCD, userId
                );
            }
            catch
            {
                detail.TodayeBiblesData = null;
            }

            return Task.FromResult<TodayWordDetailResult?>(detail);
        }

        // ------------------------------------------------------------
        // 1) 콘텐츠 ID 선택
        // ------------------------------------------------------------
        public async Task<DailyMission2ContentIds> GetContentIdsAsync(DateTime selectedDate)
        {
            var day = AsMidnight(selectedDate);
            var result = new DailyMission2ContentIds();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // TodayWordToRead
            {
                string sql = @"
SELECT TOP 1 Id
FROM dbo.TodayWordToRead
WHERE CONVERT(date, ExposeDate) = @D
ORDER BY Id;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = day });
                var obj = await cmd.ExecuteScalarAsync();
                result.TodayWordToReadId = (obj == null || obj == DBNull.Value) ? (int?)null : Convert.ToInt32(obj);
            }

            // PrayPhrase
            {
                string sql = @"
SELECT TOP 1 Id
FROM dbo.PrayPhrase
WHERE CONVERT(date, ExposeDate) = @D
ORDER BY Id;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = day });
                var obj = await cmd.ExecuteScalarAsync();
                result.PrayPhraseId = (obj == null || obj == DBNull.Value) ? (int?)null : Convert.ToInt32(obj);
            }

            // WeeklyWordToRead (기간 매칭)
            {
                string sql = @"
SELECT TOP 1 Id
FROM dbo.WeeklyWordToRead
WHERE CONVERT(date, DateFrom) <= @D
  AND CONVERT(date, DateTo) >= @D
ORDER BY DateFrom DESC, Id DESC;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = day });
                var obj = await cmd.ExecuteScalarAsync();
                result.WeeklyWordToReadId = (obj == null || obj == DBNull.Value) ? (int?)null : Convert.ToInt32(obj);
            }

            // Quiz content
            {
                string sql = @"
SELECT TOP 1
    Id,
    QuestionType,
    QuestionQ,
    Question1, Question2, Question3, Question4,
    CorrectAnswer,
    AnswerGuide,
    QuestionBackground
FROM dbo.QuestionData
WHERE CONVERT(date, ExposeDate) = @D
ORDER BY Id;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = day });

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    result.Quiz.QuestionId = r["Id"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["Id"]);
                    result.Quiz.QuestionType = r["QuestionType"] as string;
                    result.Quiz.QuestionQ = r["QuestionQ"] as string;
                    result.Quiz.Question1 = r["Question1"] as string;
                    result.Quiz.Question2 = r["Question2"] as string;
                    result.Quiz.Question3 = r["Question3"] as string;
                    result.Quiz.Question4 = r["Question4"] as string;
                    result.Quiz.CorrectAnswer = r["CorrectAnswer"] as string;
                    result.Quiz.AnswerGuide = r["AnswerGuide"] as string;
                    result.Quiz.QuestionBackground = r["QuestionBackground"] as string;
                }
            }

            return result;
        }

        // ------------------------------------------------------------
        // ✅ BibleText 요약(기존 BLL 재사용)
        // ------------------------------------------------------------
        public async Task<eBibles> GetTodayBibleSummaryAsync(string BibleText)
        {
            eBibles eBibles = new eBibles();
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();

            List<BibleBasicInfos2> data = manageeBulkInputBible.getBibleBasicListByBibleText2(0, BibleText);
            if (data.Count > 0) eBibles._BibleBasicInfos2 = data[0];

            List<BibleBookOverviewDetail> overview = manageeBulkInputBible.getBibleBasicListByBibleText3(0, BibleText);
            if (overview.Count > 0) eBibles._BibleBookOverviewDetail = overview[0];

            return eBibles;
        }

        // ------------------------------------------------------------
        // 2) 선택일 기준 완료 판정
        // ------------------------------------------------------------
        public async Task<DailyMission2DayStatus> GetDayStatusAsync(string userName, DateTime selectedDate)
        {
            var day = AsMidnight(selectedDate);
            var status = new DailyMission2DayStatus { Date = day };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Word
            {
                string sql = @"
SELECT TOP 1 ToMe, ToParent
FROM dbo.ClickHistoryForTodayWordToRead
WHERE UserName = @UserName
  AND ClickDate = @ClickDate
ORDER BY Id DESC;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = day });

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    var toMe = (r["ToMe"]?.ToString() ?? "n");
                    var toParent = (r["ToParent"]?.ToString() ?? "n");

                    status.Word_ToMe = toMe.Equals("y", StringComparison.OrdinalIgnoreCase);
                    status.Word_ToParent = toParent.Equals("y", StringComparison.OrdinalIgnoreCase);
                    status.WordDone = status.Word_ToMe || status.Word_ToParent;
                }
            }

            // Pray
            {
                string sql = @"
SELECT TOP 1 ToMe, ToParent, ToFriend
FROM dbo.ClickHistoryForPrayPhrase
WHERE UserName = @UserName
  AND ClickDate = @ClickDate
ORDER BY Id DESC;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = day });

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    var toMe = (r["ToMe"]?.ToString() ?? "n");
                    var toParent = (r["ToParent"]?.ToString() ?? "n");
                    var toFriend = (r["ToFriend"]?.ToString() ?? "n");

                    status.Pray_ToMe = toMe.Equals("y", StringComparison.OrdinalIgnoreCase);
                    status.Pray_ToParent = toParent.Equals("y", StringComparison.OrdinalIgnoreCase);
                    status.Pray_ToFriend = toFriend.Equals("y", StringComparison.OrdinalIgnoreCase);
                    status.PrayDone = status.Pray_ToMe || status.Pray_ToParent || status.Pray_ToFriend;
                }
            }

            // Quiz
            {
                string sql = @"
SELECT
    ISNULL(MAX(TryCnt), 0) AS TryCntMax,
    MAX(CASE WHEN AnswerQ = CorrectAnswer THEN 1 ELSE 0 END) AS CorrectYn
FROM dbo.ClickHistoryForQuestion
WHERE UserName = @UserName
  AND ClickDate = @ClickDate;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = day });

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    status.QuizTryCntMax = r["TryCntMax"] == DBNull.Value ? 0 : Convert.ToInt32(r["TryCntMax"]);
                    status.QuizCorrect = r["CorrectYn"] != DBNull.Value && Convert.ToInt32(r["CorrectYn"]) == 1;
                    status.QuizDone = status.QuizCorrect;
                }
            }

            // Recite (이번 주 weeklyId 찾아서 완료 여부)
            {
                int? weeklyId = null;

                string sqlWeek = @"
SELECT TOP 1 Id
FROM dbo.WeeklyWordToRead
WHERE CONVERT(date, DateFrom) <= @D
  AND CONVERT(date, DateTo) >= @D
ORDER BY DateFrom DESC, Id DESC;";
                using (var cmd = new SqlCommand(sqlWeek, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = day });
                    var obj = await cmd.ExecuteScalarAsync();
                    if (obj != null && obj != DBNull.Value)
                        weeklyId = Convert.ToInt32(obj);
                }

                if (weeklyId.HasValue)
                {
                    string sql = @"
SELECT TOP 1 1
FROM dbo.ClickHistoryForWeeklyWordToRead
WHERE UserName = @UserName
  AND WeeklyWordToReadId = @WeeklyId;";
                    using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                    cmd.Parameters.Add(new SqlParameter("@WeeklyId", SqlDbType.Int) { Value = weeklyId.Value });

                    var obj = await cmd.ExecuteScalarAsync();
                    status.ReciteDone = (obj != null);
                }
            }

            return status;
        }

        // ------------------------------------------------------------
        // 3) 연간 누적 점수
        // ------------------------------------------------------------
        public async Task<DailyMission2YearPoint> GetYearPointAsync(string userName, int cYear)
        {
            var yp = new DailyMission2YearPoint { UserName = userName, cYear = cYear };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // ✅ 하루 단위로 묶어서 계산한다 (홈 화면과 같은 dbo.ufn_DM2_GetUserDailyPoints 사용)
            //    - 말씀/기도는 하루 최대 2점, 퀴즈는 정답이면 1점, 암송은 기록이 있으면 1점
            //    - 같은 날 같은 기록이 중복 저장돼 있어도 두 번 세지 않는다
            {
                const string sql = @"
SELECT
    ISNULL(SUM(WordPoint),   0),
    ISNULL(SUM(PrayPoint),   0),
    ISNULL(SUM(QuizPoint),   0),
    ISNULL(SUM(RecitePoint), 0)
FROM dbo.ufn_DM2_GetUserDailyPoints(@UserName, @From, @To);";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 100) { Value = userName ?? "" });
                cmd.Parameters.Add(new SqlParameter("@From", SqlDbType.Date) { Value = new DateTime(cYear, 1, 1) });
                // 올해면 오늘까지, 지난 해면 12/31까지 — GetUserTotalPointsThisYearAsync(홈)와 같은 범위
                var kToday = DateHelper.GetKoreaNow().Date;
                cmd.Parameters.Add(new SqlParameter("@To",   SqlDbType.Date) { Value = kToday.Year == cYear ? kToday : new DateTime(cYear, 12, 31) });
                using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    yp.WordPoint   = rd.IsDBNull(0) ? 0 : Convert.ToInt32(rd.GetValue(0));
                    yp.PrayPoint   = rd.IsDBNull(1) ? 0 : Convert.ToInt32(rd.GetValue(1));
                    yp.QuizPoint   = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd.GetValue(2));
                    yp.RecitePoint = rd.IsDBNull(3) ? 0 : Convert.ToInt32(rd.GetValue(3));
                }
            }

            yp.YearTotalPoint = yp.WordPoint + yp.PrayPoint + yp.QuizPoint + yp.RecitePoint;
            return yp;
        }

        // ------------------------------------------------------------
        // 4) 저장(Word/Pray/Quiz/Recite)
        // ------------------------------------------------------------
        public async Task<SaveResult> SaveWordAsync(string userName, DateTime selectedDate, int todayWordToReadId, bool toMe, bool toParent)
        {
            var clickDate = AsMidnight(selectedDate);
            var cYear = clickDate.Year;

            if (!toMe && !toParent)
                return new SaveResult { Ok = false, Message = "체크를 선택해 주세요. (내가 읽기/부모님께 읽어드리기)" };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // ✅ 1) 기존 값 조회
            string getSql = @"
SELECT TOP 1 ToMe, ToParent
FROM dbo.ClickHistoryForTodayWordToRead
WHERE UserName = @UserName AND ClickDate = @ClickDate;";

            bool oldToMe = false;
            bool oldToParent = false;
            bool exists = false;

            using (var getCmd = new SqlCommand(getSql, conn))
            {
                getCmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                getCmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = clickDate });

                using var r = await getCmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    exists = true;
                    oldToMe = (r["ToMe"]?.ToString() ?? "n").Equals("y", StringComparison.OrdinalIgnoreCase);
                    oldToParent = (r["ToParent"]?.ToString() ?? "n").Equals("y", StringComparison.OrdinalIgnoreCase);
                }
            }

            // ✅ 2) 새 요청값을 기존값에 "합쳐서(OR)" 저장
            bool mergedToMe = oldToMe || toMe;
            bool mergedToParent = oldToParent || toParent;

            if (exists)
            {
                string upSql = @"
UPDATE dbo.ClickHistoryForTodayWordToRead
SET TodayWordToReadId = @TodayWordToReadId,
    ToMe = @ToMe,
    ToParent = @ToParent,
    updateDT = GETDATE(),
    cYear = @cYear
WHERE UserName = @UserName AND ClickDate = @ClickDate;";

                using var cmd = new SqlCommand(upSql, conn);
                cmd.Parameters.Add(new SqlParameter("@TodayWordToReadId", SqlDbType.Int) { Value = todayWordToReadId });
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@ToMe", SqlDbType.NVarChar, 1) { Value = mergedToMe ? "y" : "n" });
                cmd.Parameters.Add(new SqlParameter("@ToParent", SqlDbType.NVarChar, 1) { Value = mergedToParent ? "y" : "n" });
                cmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = clickDate });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });

                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                string insSql = @"
INSERT INTO dbo.ClickHistoryForTodayWordToRead
(TodayWordToReadId, UserName, ToMe, ToParent, ClickDate, InsertDT, updateDT, cYear)
VALUES
(@TodayWordToReadId, @UserName, @ToMe, @ToParent, @ClickDate, GETDATE(), GETDATE(), @cYear);";

                using var cmd = new SqlCommand(insSql, conn);
                cmd.Parameters.Add(new SqlParameter("@TodayWordToReadId", SqlDbType.Int) { Value = todayWordToReadId });
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@ToMe", SqlDbType.NVarChar, 1) { Value = mergedToMe ? "y" : "n" });
                cmd.Parameters.Add(new SqlParameter("@ToParent", SqlDbType.NVarChar, 1) { Value = mergedToParent ? "y" : "n" });
                cmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = clickDate });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });

                await cmd.ExecuteNonQueryAsync();
            }

            return new SaveResult { Ok = true, Message = "말씀 완료! ✅" };
        }

        public async Task<SaveResult> SavePrayAsync(string userName, DateTime selectedDate, int prayPhraseId, bool toMe, bool toParent, bool toFriend)
        {
            var clickDate = AsMidnight(selectedDate);
            var cYear = clickDate.Year;

            if (!toMe && !toParent && !toFriend)
                return new SaveResult { Ok = false, Message = "체크를 선택해 주세요. (나/부모/친구)" };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
IF EXISTS (
    SELECT 1 FROM dbo.ClickHistoryForPrayPhrase
    WHERE UserName = @UserName AND ClickDate = @ClickDate
)
BEGIN
    UPDATE dbo.ClickHistoryForPrayPhrase
    SET PrayPhraseId = @PrayPhraseId,
        ToMe = @ToMe,
        ToParent = @ToParent,
        ToFriend = @ToFriend,
        cYear = @cYear
    WHERE UserName = @UserName AND ClickDate = @ClickDate;
END
ELSE
BEGIN
    INSERT INTO dbo.ClickHistoryForPrayPhrase
    (PrayPhraseId, ToMe, ToParent, ToFriend, UserName, ClickDate, InsertDT, cYear)
    VALUES
    (@PrayPhraseId, @ToMe, @ToParent, @ToFriend, @UserName, @ClickDate, GETDATE(), @cYear);
END";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@PrayPhraseId", SqlDbType.Int) { Value = prayPhraseId });
            cmd.Parameters.Add(new SqlParameter("@ToMe", SqlDbType.NVarChar, 1) { Value = toMe ? "y" : "n" });
            cmd.Parameters.Add(new SqlParameter("@ToParent", SqlDbType.NVarChar, 1) { Value = toParent ? "y" : "n" });
            cmd.Parameters.Add(new SqlParameter("@ToFriend", SqlDbType.NVarChar, 1) { Value = toFriend ? "y" : "n" });
            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
            cmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = clickDate });
            cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });

            await cmd.ExecuteNonQueryAsync();
            return new SaveResult { Ok = true, Message = "기도 완료! ✅" };
        }

        public async Task<SaveResult> SaveQuizAsync(string userName, DateTime selectedDate, int questionId, string answerQ, string? correctAnswer = null)
        {
            var clickDate = AsMidnight(selectedDate);
            var cYear = clickDate.Year;

            answerQ = (answerQ ?? "").Trim();
            if (string.IsNullOrWhiteSpace(answerQ))
                return new SaveResult { Ok = false, Message = "답을 선택해 주세요." };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string dbCorrect = "";
            {
                string sql = @"SELECT TOP 1 CorrectAnswer FROM dbo.QuestionData WHERE Id=@Id;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = questionId });
                var obj = await cmd.ExecuteScalarAsync();
                dbCorrect = (obj == null || obj == DBNull.Value) ? "" : obj.ToString()!.Trim();
            }

            if (string.IsNullOrWhiteSpace(dbCorrect))
                dbCorrect = (correctAnswer ?? "").Trim();

            if (string.IsNullOrWhiteSpace(dbCorrect))
                return new SaveResult { Ok = false, Message = "정답 데이터가 없습니다." };

            int tryMax = 0;
            bool alreadyCorrect = false;

            string stateSql = @"
SELECT
    ISNULL(MAX(TryCnt), 0) AS TryCntMax,
    MAX(CASE WHEN AnswerQ = CorrectAnswer THEN 1 ELSE 0 END) AS CorrectYn
FROM dbo.ClickHistoryForQuestion
WHERE UserName=@UserName AND QuestionId=@QuestionId AND ClickDate=@ClickDate;";

            using (var cmd = new SqlCommand(stateSql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@QuestionId", SqlDbType.Int) { Value = questionId });
                cmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = clickDate });

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    tryMax = r["TryCntMax"] == DBNull.Value ? 0 : Convert.ToInt32(r["TryCntMax"]);
                    alreadyCorrect = r["CorrectYn"] != DBNull.Value && Convert.ToInt32(r["CorrectYn"]) == 1;
                }
            }

            if (alreadyCorrect)
                return new SaveResult { Ok = false, Message = "이미 정답을 맞췄어요! 🎉" };

            if (tryMax >= 2)
                return new SaveResult { Ok = false, Message = "이미 2번 시도했어요! 🙂" };

            int nextTry = tryMax + 1;

            string insSql = @"
INSERT INTO dbo.ClickHistoryForQuestion
(QuestionId, UserName, AnswerQ, CorrectAnswer, TryCnt, ClickDate, InsertDT, cYear)
VALUES
(@QuestionId, @UserName, @AnswerQ, @CorrectAnswer, @TryCnt, @ClickDate, GETDATE(), @cYear);";

            using (var cmd = new SqlCommand(insSql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@QuestionId", SqlDbType.Int) { Value = questionId });
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@AnswerQ", SqlDbType.NVarChar, 200) { Value = answerQ });
                cmd.Parameters.Add(new SqlParameter("@CorrectAnswer", SqlDbType.NVarChar, 200) { Value = dbCorrect });
                cmd.Parameters.Add(new SqlParameter("@TryCnt", SqlDbType.Int) { Value = nextTry });
                cmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = clickDate });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });

                await cmd.ExecuteNonQueryAsync();
            }

            bool isCorrect = string.Equals(answerQ, dbCorrect, StringComparison.Ordinal);
            return new SaveResult
            {
                Ok = true,
                Message = isCorrect ? "정답! 🎉 +1점" : $"오답이에요. ({nextTry}/2)"
            };
        }

        /// <summary>
        /// 이 사람이 그 날 그 퀴즈에 고른 답을 시도 순서(TryCnt)대로 돌려 준다.
        /// 퀴즈를 끝낸 뒤 "내가 고른 자리"를 화면에 표시하는 데 쓴다.
        /// </summary>
        public async Task<List<string>> GetQuizMyAnswersAsync(string userName, DateTime selectedDate, int questionId)
        {
            var list = new List<string>();
            if (questionId <= 0 || string.IsNullOrWhiteSpace(userName)) return list;

            var clickDate = AsMidnight(selectedDate);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
SELECT AnswerQ
FROM dbo.ClickHistoryForQuestion
WHERE UserName=@UserName AND QuestionId=@QuestionId AND ClickDate=@ClickDate
ORDER BY TryCnt;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
            cmd.Parameters.Add(new SqlParameter("@QuestionId", SqlDbType.Int) { Value = questionId });
            cmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = clickDate });

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var a = r["AnswerQ"] as string;
                if (!string.IsNullOrWhiteSpace(a)) list.Add(a.Trim());
            }
            return list;
        }

        public async Task<SaveResult> SaveReciteAsync(string userName, DateTime selectedDate, int weeklyWordToReadId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            DateTime? dateFrom = null;
            {
                string sql = @"SELECT TOP 1 CONVERT(date, DateFrom) FROM dbo.WeeklyWordToRead WHERE Id=@Id;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = weeklyWordToReadId });
                var obj = await cmd.ExecuteScalarAsync();
                if (obj != null && obj != DBNull.Value)
                    dateFrom = Convert.ToDateTime(obj).Date;
            }

            if (!dateFrom.HasValue)
                return new SaveResult { Ok = false, Message = "주간 암송 데이터(DateFrom)를 찾을 수 없어요." };

            var clickDate = dateFrom.Value.Date;
            var cYear = clickDate.Year;

            {
                string sql = @"
SELECT TOP 1 1
FROM dbo.ClickHistoryForWeeklyWordToRead
WHERE UserName=@UserName AND WeeklyWordToReadId=@WeeklyId;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@WeeklyId", SqlDbType.Int) { Value = weeklyWordToReadId });

                var exists = await cmd.ExecuteScalarAsync();
                if (exists != null)
                    return new SaveResult { Ok = false, Message = "이번 주 암송은 이미 완료했어요! 🎉" };
            }

            string ins = @"
INSERT INTO dbo.ClickHistoryForWeeklyWordToRead
(WeeklyWordToReadId, UserName, ClickDate, InsertDT, cYear)
VALUES
(@WeeklyId, @UserName, @ClickDate, GETDATE(), @cYear);";

            using (var cmd = new SqlCommand(ins, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@WeeklyId", SqlDbType.Int) { Value = weeklyWordToReadId });
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@ClickDate", SqlDbType.DateTime) { Value = clickDate });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });

                await cmd.ExecuteNonQueryAsync();
            }

            return new SaveResult { Ok = true, Message = "이번 주 암송 완료! 🎉 +1점" };
        }

        // ------------------------------------------------------------
        // 5) 달력 42칸 AllDone 맵
        // ------------------------------------------------------------
        public async Task<Dictionary<string, bool>> GetCalendarAllDoneMapAsync(string userName, DateTime selectedDate)
        {
            var monthFirst = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            int startOffset = (int)monthFirst.DayOfWeek; // Sunday=0
            var gridStart = monthFirst.AddDays(-startOffset).Date;
            var gridEndExclusive = gridStart.AddDays(42).Date;

            var days = Enumerable.Range(0, 42).Select(i => gridStart.AddDays(i).Date).ToList();

            var wordDoneDates = await GetWordDoneDatesAsync(userName, gridStart, gridEndExclusive);
            var prayDoneDates = await GetPrayDoneDatesAsync(userName, gridStart, gridEndExclusive);
            var quizDoneDates = await GetQuizDoneDatesAsync(userName, gridStart, gridEndExclusive);
            var reciteDoneMap = await GetReciteDoneMapByDayAsync(userName, gridStart, gridEndExclusive);

            var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            foreach (var day in days)
            {
                bool word = wordDoneDates.Contains(day);
                bool pray = prayDoneDates.Contains(day);
                bool quiz = quizDoneDates.Contains(day);
                bool recite = reciteDoneMap.TryGetValue(day, out var r) && r;

                map[day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)] = (word && pray && quiz && recite);
            }

            return map;
        }

        // 그날 "오늘의 말씀"을 수행한 사람 수 (같은 사람이 여러 번 눌러도 1명)
        public async Task<int> GetTodayWordParticipantCountAsync(DateTime day)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
SELECT COUNT(DISTINCT UserName)
FROM dbo.ClickHistoryForTodayWordToRead
WHERE ClickDate >= @Start AND ClickDate < @End
  AND (ToMe IN ('y','Y') OR ToParent IN ('y','Y'));";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Start", SqlDbType.DateTime) { Value = day.Date });
            cmd.Parameters.Add(new SqlParameter("@End", SqlDbType.DateTime) { Value = day.Date.AddDays(1) });

            var v = await cmd.ExecuteScalarAsync();
            return (v == null || v == DBNull.Value) ? 0 : Convert.ToInt32(v);
        }

        private async Task<HashSet<DateTime>> GetWordDoneDatesAsync(string userName, DateTime start, DateTime endExclusive)
        {
            var set = new HashSet<DateTime>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
SELECT CONVERT(date, ClickDate) AS D
FROM dbo.ClickHistoryForTodayWordToRead
WHERE UserName = @UserName
  AND ClickDate >= @Start AND ClickDate < @End
  AND (ToMe IN ('y','Y') OR ToParent IN ('y','Y'))
GROUP BY CONVERT(date, ClickDate);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
            cmd.Parameters.Add(new SqlParameter("@Start", SqlDbType.DateTime) { Value = start });
            cmd.Parameters.Add(new SqlParameter("@End", SqlDbType.DateTime) { Value = endExclusive });

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                set.Add(Convert.ToDateTime(r["D"]).Date);

            return set;
        }

        private async Task<HashSet<DateTime>> GetPrayDoneDatesAsync(string userName, DateTime start, DateTime endExclusive)
        {
            var set = new HashSet<DateTime>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
SELECT CONVERT(date, ClickDate) AS D
FROM dbo.ClickHistoryForPrayPhrase
WHERE UserName = @UserName
  AND ClickDate >= @Start AND ClickDate < @End
  AND (ToMe IN ('y','Y') OR ToParent IN ('y','Y') OR ToFriend IN ('y','Y'))
GROUP BY CONVERT(date, ClickDate);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
            cmd.Parameters.Add(new SqlParameter("@Start", SqlDbType.DateTime) { Value = start });
            cmd.Parameters.Add(new SqlParameter("@End", SqlDbType.DateTime) { Value = endExclusive });
 await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                set.Add(Convert.ToDateTime(r["D"]).Date);

            return set;
        }

        private async Task<HashSet<DateTime>> GetQuizDoneDatesAsync(string userName, DateTime start, DateTime endExclusive)
        {
            var set = new HashSet<DateTime>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
SELECT CONVERT(date, ClickDate) AS D
FROM dbo.ClickHistoryForQuestion
WHERE UserName = @UserName
  AND ClickDate >= @Start AND ClickDate < @End
  AND AnswerQ = CorrectAnswer
GROUP BY CONVERT(date, ClickDate);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
            cmd.Parameters.Add(new SqlParameter("@Start", SqlDbType.DateTime) { Value = start });
            cmd.Parameters.Add(new SqlParameter("@End", SqlDbType.DateTime) { Value = endExclusive });
 await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                set.Add(Convert.ToDateTime(r["D"]).Date);

            return set;
        }

        private async Task<Dictionary<DateTime, bool>> GetReciteDoneMapByDayAsync(string userName, DateTime start, DateTime endExclusive)
        {
            var result = new Dictionary<DateTime, bool>();

            var weeks = new List<(int Id, DateTime From, DateTime To)>();
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sqlWeeks = @"
SELECT Id, DateFrom, DateTo
FROM dbo.WeeklyWordToRead
WHERE DateFrom <= @EndMinus1
  AND DateTo >= @Start;";

                using var cmd = new SqlCommand(sqlWeeks, conn);
                cmd.Parameters.Add(new SqlParameter("@Start", SqlDbType.DateTime) { Value = start });
                cmd.Parameters.Add(new SqlParameter("@EndMinus1", SqlDbType.DateTime) { Value = endExclusive.AddDays(-1) });
 await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    int id = Convert.ToInt32(r["Id"]);
                    var from = Convert.ToDateTime(r["DateFrom"]).Date;
                    var to = Convert.ToDateTime(r["DateTo"]).Date;
                    weeks.Add((id, from, to));
                }
            }

            var doneWeekIds = new HashSet<int>();
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sqlDone = @"
SELECT DISTINCT WeeklyWordToReadId
FROM dbo.ClickHistoryForWeeklyWordToRead
WHERE UserName = @UserName;";

                using var cmd = new SqlCommand(sqlDone, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    doneWeekIds.Add(Convert.ToInt32(r["WeeklyWordToReadId"]));
            }

            for (int i = 0; i < 42; i++)
            {
                var day = start.AddDays(i).Date;
                var w = weeks.FirstOrDefault(x => x.From <= day && day <= x.To);
                result[day] = (w.Id != 0) && doneWeekIds.Contains(w.Id);
            }

            return result;
        }

        // ------------------------------------------------------------
        // ✅ Bible Read/Bookmark 관련 Wrapper
        // ------------------------------------------------------------

        public Task<SaveResult> MarkVerseReadAsync(string UserId, DateTime selectedDate, string langCD, string bibleText, int jang, int zul, int VerseId)
        {
            ManageeBible manageeBible = new ManageeBible();

            // ⚠️ 현재 InsertOrDeleteUserBibleProgress가 "토글"이면:
            // nowInserted == true  => 읽음(INSERT)
            // nowInserted == false => 미읽음(DELETE)
            bool nowInserted = manageeBible.InsertOrDeleteUserBibleProgress(UserId, VerseId);

            return Task.FromResult<SaveResult>(new SaveResultEx
            {
                Ok = true,
                IsRead = nowInserted,
                Message = nowInserted ? "📖 읽었음 처리했어요!" : "📗 아직 읽지 않음으로 바꿨어요!"
            });
        }

        public Task<SaveResult> MarkVerseUnreadAsync(string UserId, DateTime selectedDate, string langCD, string bibleText, int jang, int zul, int VerseId)
        {
            ManageeBible manageeBible = new ManageeBible();

            // ⚠️ 토글 기반이면 '미읽음 요청'도 토글이라 정확히 강제하기 어려움
            bool nowInserted = manageeBible.InsertOrDeleteUserBibleProgress(UserId, VerseId);

            return Task.FromResult<SaveResult>(new SaveResultEx
            {
                Ok = true,
                IsRead = nowInserted,
                Message = nowInserted ? "📖 읽었음 처리되었어요(토글)!" : "📗 아직 읽지 않음 처리되었어요(토글)!"
            });
        }

        public Task<SaveResult> ToggleBookmarkAsync(string UserId, DateTime selectedDate, string langCD, string bibleText, int jang, int zul, int VerseId)
        {
            try
            {
                var manageeBible = new ManageeBible();

                // ✅ 실제 토글 함수 호출 (프로젝트 함수명과 동일해야 함)
                bool isBookmarkedNow = manageeBible.InsertOrDeleteUserBookMark(UserId, VerseId);

                return Task.FromResult<SaveResult>(new SaveResultEx
                {
                    Ok = true,
                    IsBookmarked = isBookmarkedNow,
                    Message = isBookmarkedNow ? "🔖 책갈피에 저장했어요!" : "🔖 책갈피를 해제했어요!"
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult<SaveResult>(new SaveResult
                {
                    Ok = false,
                    Message = $"책갈피 처리 중 오류: {ex.Message}"
                });
            }
        }

        public Task<SaveResult> MarkPageReadAsync(string UserId, DateTime selectedDate, string langCD, string bibleText, int fromJang, int fromZul, int toJang, int toZul)
        {
            try
            {
                ControleBible controleBible = new ControleBible();
                controleBible.InsertUserBibleReadRange(bibleText, fromJang, fromZul, toJang, toZul, UserId);

                return Task.FromResult<SaveResult>(new SaveResult { Ok = true, Message = "✅ 이 페이지를 다 읽었어요!" });
            }
            catch (Exception ex)
            {
                return Task.FromResult<SaveResult>(new SaveResult { Ok = false, Message = $"페이지 읽음 저장 중 오류: {ex.Message}" });
            }
        }

        public Task<SaveResult> MarkAllReadAsync(string UserId, DateTime selectedDate, string langCD, string bibleText, int fromJang, int fromZul, int toJang, int toZul)
        {
            // ✅ 필요 시: 전체 범위 완료 저장 (별도 테이블/로직)
            return Task.FromResult<SaveResult>(new SaveResult { Ok = true, Message = "🎉 성경 본문을 다 읽었어요!" });
        }

        // ── 읽기 범위 전체 토글 (읽음 ↔ 미읽음) ──────────────────────────────
        public async Task<(bool isRead, int readVerses, int totalVerses)> ToggleBibleRangeAsync(
            string userId, string bibleText, int fromJang, int fromZul, int toJang, int toZul)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // 범위 내 전체 절수
            const string countSql = @"
                SELECT COUNT(*) FROM eBible
                WHERE BibleText = @bt
                  AND (Jang > @fj OR (Jang = @fj AND Zul >= @fz))
                  AND (Jang < @tj OR (Jang = @tj AND Zul <= @tz))";
            using var cntCmd = new SqlCommand(countSql, conn);
            cntCmd.Parameters.AddWithValue("@bt", bibleText);
            cntCmd.Parameters.AddWithValue("@fj", fromJang);
            cntCmd.Parameters.AddWithValue("@fz", fromZul);
            cntCmd.Parameters.AddWithValue("@tj", toJang);
            cntCmd.Parameters.AddWithValue("@tz", toZul);
            int total = Convert.ToInt32(await cntCmd.ExecuteScalarAsync() ?? 0);
            if (total == 0) return (false, 0, 0);

            // 현재 읽은 절수
            const string readCntSql = @"
                SELECT COUNT(*) FROM UserBibleProgress ubp
                INNER JOIN eBible eb ON ubp.VerseId = eb.Id
                WHERE ubp.UserId = @uid
                  AND eb.BibleText = @bt
                  AND (eb.Jang > @fj OR (eb.Jang = @fj AND eb.Zul >= @fz))
                  AND (eb.Jang < @tj OR (eb.Jang = @tj AND eb.Zul <= @tz))";
            using var rcCmd = new SqlCommand(readCntSql, conn);
            rcCmd.Parameters.AddWithValue("@uid", userId);
            rcCmd.Parameters.AddWithValue("@bt", bibleText);
            rcCmd.Parameters.AddWithValue("@fj", fromJang);
            rcCmd.Parameters.AddWithValue("@fz", fromZul);
            rcCmd.Parameters.AddWithValue("@tj", toJang);
            rcCmd.Parameters.AddWithValue("@tz", toZul);
            int alreadyRead = Convert.ToInt32(await rcCmd.ExecuteScalarAsync() ?? 0);

            bool makeRead = alreadyRead < total;
            var now = DateHelper.GetKoreaNow();

            if (makeRead)
            {
                const string insertSql = @"
                    INSERT INTO UserBibleProgress (UserId, VerseId, ReadDate)
                    SELECT @uid, eb.Id, @now
                    FROM eBible eb
                    WHERE eb.BibleText = @bt
                      AND (eb.Jang > @fj OR (eb.Jang = @fj AND eb.Zul >= @fz))
                      AND (eb.Jang < @tj OR (eb.Jang = @tj AND eb.Zul <= @tz))
                      AND NOT EXISTS (
                          SELECT 1 FROM UserBibleProgress ubp2
                          WHERE ubp2.UserId = @uid AND ubp2.VerseId = eb.Id
                      )";
                using var insCmd = new SqlCommand(insertSql, conn);
                insCmd.Parameters.AddWithValue("@uid", userId);
                insCmd.Parameters.AddWithValue("@now", now);
                insCmd.Parameters.AddWithValue("@bt", bibleText);
                insCmd.Parameters.AddWithValue("@fj", fromJang);
                insCmd.Parameters.AddWithValue("@fz", fromZul);
                insCmd.Parameters.AddWithValue("@tj", toJang);
                insCmd.Parameters.AddWithValue("@tz", toZul);
                await insCmd.ExecuteNonQueryAsync();
                return (true, total, total);
            }
            else
            {
                const string deleteSql = @"
                    DELETE FROM UserBibleProgress
                    WHERE UserId = @uid
                      AND VerseId IN (
                          SELECT Id FROM eBible
                          WHERE BibleText = @bt
                            AND (Jang > @fj OR (Jang = @fj AND Zul >= @fz))
                            AND (Jang < @tj OR (Jang = @tj AND Zul <= @tz))
                      )";
                using var delCmd = new SqlCommand(deleteSql, conn);
                delCmd.Parameters.AddWithValue("@uid", userId);
                delCmd.Parameters.AddWithValue("@bt", bibleText);
                delCmd.Parameters.AddWithValue("@fj", fromJang);
                delCmd.Parameters.AddWithValue("@fz", fromZul);
                delCmd.Parameters.AddWithValue("@tj", toJang);
                delCmd.Parameters.AddWithValue("@tz", toZul);
                await delCmd.ExecuteNonQueryAsync();
                return (false, 0, total);
            }
        }

        // ------------------------------------------------------------
        // ✅ 주간 암송(WeeklyWordToRead) 상세 + 암송 본문(eBibles)
        // ------------------------------------------------------------
        public async Task<WeeklyWordDetailResult?> GetWeeklyWordDetailAsync(DateTime selectedDate, string langCD, string userId, int cYear)
        {
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();
            ManageeBible manageeBible = new ManageeBible();

            int weeklyId = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string sql = @"
SELECT TOP 1 Id
FROM dbo.WeeklyWordToRead
WHERE @D >= CONVERT(date, DateFrom)
  AND @D <= CONVERT(date, DateTo)
  AND cYear = @cYear
ORDER BY DateFrom DESC, Id DESC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@D", selectedDate.Date);
                cmd.Parameters.AddWithValue("@cYear", cYear);

                var obj = await cmd.ExecuteScalarAsync();
                if (obj == null || obj == DBNull.Value)
                    return null;

                weeklyId = Convert.ToInt32(obj);
            }

            var list = manageWeeklyWordToRead.getWeeklyWordToRead(weeklyId, true, selectedDate, cYear);
            if (list == null || list.Count == 0)
                return null;

            var ww = list[0];

            var result = new WeeklyWordDetailResult
            {
                WeeklyWordToReadData = ww,
                WeekeBiblesDataToRecite = manageeBible.geteBible2(
                    ww.BibleTextToRecite,
                    ww.BibleTextToReciteFromJang,
                    ww.BibleTextToReciteFromZul,
                    ww.BibleTextToReciteToJang,
                    ww.BibleTextToReciteToZul,
                    langCD,
                    userId
                ),
                WeekeBiblesDataToRead = manageeBible.geteBible2(
                    ww.BibleTextToRead,
                    ww.BibleTextToReadFromJang,
                    ww.BibleTextToReadFromZul,
                    ww.BibleTextToReadToJang,
                    ww.BibleTextToReadToZul,
                    langCD,
                    userId
                ),
                WeekeBiblesDataToRead_1 = manageeBible.geteBible2(
                    ww.BibleTextToRead_1,
                    ww.BibleTextToReadFromJang_1,
                    ww.BibleTextToReadFromZul_1,
                    ww.BibleTextToReadToJang_1,
                    ww.BibleTextToReadToZul_1,
                    langCD,
                    userId
                )
            };

            return result;
        }

        // ------------------------------------------------------------
        // ✅ 오늘 퀴즈(상세)
        // ------------------------------------------------------------
        public async Task<DailyMission2QuizContent?> GetTodayQuestionAsync(DateTime selectedDate)
        {
            var day = AsMidnight(selectedDate);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
SELECT TOP 1
    Id,
    ExposeDate,
    QuestionType,
    QuestionQ,
    Question1, Question2, Question3, Question4,
    CorrectAnswer,
    AnswerGuide,
    QuestionBackground
FROM dbo.QuestionData
WHERE CONVERT(date, ExposeDate) = @D
ORDER BY Id;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = day });
 await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync())
                return null;

            return new DailyMission2QuizContent
            {
                QuestionId = Convert.ToInt32(r["Id"]),
                ExposeDate = (DateTime)r["ExposeDate"],
                QuestionType = r["QuestionType"] as string,
                QuestionQ = r["QuestionQ"] as string,
                Question1 = r["Question1"] as string,
                Question2 = r["Question2"] as string,
                Question3 = r["Question3"] as string,
                Question4 = r["Question4"] as string,
                CorrectAnswer = r["CorrectAnswer"] as string,
                AnswerGuide = r["AnswerGuide"] as string,
                QuestionBackground = r["QuestionBackground"] as string
            };
        }
    }
}

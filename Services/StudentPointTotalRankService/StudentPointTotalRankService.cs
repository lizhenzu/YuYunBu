using BulletinBoard.Common;
using BulletinBoard.DataAccessLayer.Worldview; // WorldviewLessonRepository 네임스페이스에 맞게
using BulletinBoard.Helpers;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services.DailyMission2;
using BulletinBoard.ViewModels.Dashboard;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BulletinBoard.Services
{
    public class StudentPointTotalRankService : IStudentPointTotalRankService
    {
        private readonly IDailyMission2Service _dailyMission2Service;
        private readonly WorldviewLessonRepository _worldviewRepo;
        private readonly IConfiguration _config;
        private readonly IAttendanceService _attendanceService;

        public StudentPointTotalRankService(
            IDailyMission2Service dailyMission2Service,
            WorldviewLessonRepository worldviewRepo,
            IConfiguration config,
            IAttendanceService attendanceService)
        {
            _dailyMission2Service = dailyMission2Service;
            _worldviewRepo = worldviewRepo;
            _config = config;
            _attendanceService = attendanceService;
        }

        public async Task<StudentPointTotalRankVM> GetTotalRankAsync(int year)
        {
            // 1) 도전성경(해당연도)
            var bibleRows = await _dailyMission2Service.GetParticipantPersonCardsAsync(year);

            // 2) 미션북(SubmittedAt 기준 해당연도)
            var missionRows = _worldviewRepo.GetSubmitRankByUserByYear(year, null);

            // 3) 말씀&일기(Ym 기준 해당연도)
            var devotionMap = await LoadDevotionTotalPointsByUserIdAsync(year);

            // 4) 출석카운트 * 2 (해당연도 누적)
            var attendancePointMap = await LoadAttendancePointMapByUserIdAsync(year);

            // 5) 전도 횟수 (userId별)
            var evangelismCountMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var map = new Dictionary<string, StudentPointTotalRankRowVM>(StringComparer.OrdinalIgnoreCase);

            void Ensure(string userId, string userName, string fullName)
            {
                if (string.IsNullOrWhiteSpace(userId)) return;

                if (!map.TryGetValue(userId, out var row))
                {
                    map[userId] = new StudentPointTotalRankRowVM
                    {
                        UserId = userId,
                        UserName = userName ?? "",
                        FullName = fullName ?? "",
                        RoleLabel = "기타"
                    };
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(row.FullName) && !string.IsNullOrWhiteSpace(fullName))
                        row.FullName = fullName;

                    if (string.IsNullOrWhiteSpace(row.UserName) && !string.IsNullOrWhiteSpace(userName))
                        row.UserName = userName;
                }
            }

            // 도전성경
            foreach (var r in bibleRows)
            {
                Ensure(r.UserId, r.UserName, r.FullName);
                if (map.TryGetValue(r.UserId, out var row))
                {
                    row.BiblePoint = r.YearPoint;
                    // 도전성경 최근 활동일
                    if (r.LastActionDate.HasValue &&
                        (!row.LastActivityDate.HasValue || r.LastActionDate.Value > row.LastActivityDate.Value))
                        row.LastActivityDate = r.LastActionDate.Value;
                }
            }

            // 미션북
            foreach (var r in missionRows)
            {
                var uid = r.UserId ?? "";
                Ensure(uid, r.UserName ?? "", "");
                if (map.TryGetValue(uid, out var row))
                    row.MissionBookPoint = r.TotalSum;
            }

            // 말씀&일기
            foreach (var kv in devotionMap)
            {
                Ensure(kv.Key, "", kv.Value.FullName);
                if (map.TryGetValue(kv.Key, out var row))
                    row.DevotionPoint = kv.Value.Points;
            }

            // 출석
            foreach (var kv in attendancePointMap)
            {
                Ensure(kv.Key, "", "");
                if (map.TryGetValue(kv.Key, out var row))
                    row.AttendancePoint = kv.Value;
            }

            // 전도 횟수 로드
            foreach (var userId in map.Keys.ToList())
            {
                try
                {
                    var cnt = await _attendanceService.GetEvangelismCountByUserIdAsync(userId);
                    if (cnt > 0) evangelismCountMap[userId] = cnt;
                }
                catch { }
            }

            foreach (var kv in evangelismCountMap)
            {
                if (map.TryGetValue(kv.Key, out var row))
                    row.EvangelismPoint = kv.Value * 50;
            }

            // 6) 이름 정보 일괄 보정
            var nameMap = await LoadUserNameMapByUserIdAsync(map.Keys);

            foreach (var row in map.Values)
            {
                if (!string.IsNullOrWhiteSpace(row.UserId) &&
                    nameMap.TryGetValue(row.UserId, out var nameInfo))
                {
                    if (string.IsNullOrWhiteSpace(row.UserName) && !string.IsNullOrWhiteSpace(nameInfo.UserName))
                        row.UserName = nameInfo.UserName;

                    if (string.IsNullOrWhiteSpace(row.FullName) && !string.IsNullOrWhiteSpace(nameInfo.FullName))
                        row.FullName = nameInfo.FullName;

                    if (nameInfo.ClassYear.HasValue) row.ClassYear = nameInfo.ClassYear;
                    if (nameInfo.ClassBan.HasValue)  row.ClassBan  = nameInfo.ClassBan;
                }
            }

            // 7) 최근 활동일 (출석 + 말씀일기 기준) — bible은 이미 위에서 세팅
            var activityDateMap = await LoadLastActivityDateMapAsync(year, map.Keys);
            foreach (var kv in activityDateMap)
            {
                if (map.TryGetValue(kv.Key, out var row) &&
                    (!row.LastActivityDate.HasValue || kv.Value > row.LastActivityDate.Value))
                    row.LastActivityDate = kv.Value;
            }

            // 6) 역할 정보 일괄 로드
            var roleMap = await LoadRoleLabelMapByUserIdAsync(map.Keys);

            foreach (var row in map.Values)
            {
                if (!string.IsNullOrWhiteSpace(row.UserId) &&
                    roleMap.TryGetValue(row.UserId, out var roleLabel) &&
                    !string.IsNullOrWhiteSpace(roleLabel))
                {
                    row.RoleLabel = roleLabel;
                }
                else
                {
                    row.RoleLabel = "기타";
                }
            }

            // 참여자만 + 총점 순
            var rows = map.Values
                .Select(x =>
                {
                    x.TotalPoint = x.BiblePoint + x.MissionBookPoint + x.DevotionPoint + x.AttendancePoint + x.EvangelismPoint;
                    return x;
                })
                .Where(x => x.TotalPoint > 0)
                .OrderByDescending(x => x.TotalPoint)
                .ThenBy(x =>
                    !string.IsNullOrWhiteSpace(x.FullName) ? x.FullName :
                    !string.IsNullOrWhiteSpace(x.UserName) ? x.UserName :
                    x.UserId)
                .ToList();

            for (int i = 0; i < rows.Count; i++)
                rows[i].Rank = i + 1;

            return new StudentPointTotalRankVM
            {
                Year = year,
                Rows = rows
            };
        }

        private async Task<Dictionary<string, int>> LoadAttendancePointMapByUserIdAsync(int year)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // 현재 AttendanceService의 연도 누적 로직은 운영 연도 기준으로 동작하므로
            // 요청 year가 현재 운영 연도와 다른 경우는 0점 처리합니다.
            if (year != DateHelper.GetKoreaNow().Year)
                return dict;

            var connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand(@"
SELECT DISTINCT
    pm.UserId
FROM dbo.AttendanceCheck_2026 ac WITH (NOLOCK)
JOIN dbo.view_PersonMaster_2026 pm WITH (NOLOCK) ON pm.PersonId = ac.PersonId
WHERE ac.IsPresent = 1
  AND ac.AttendDate >= @YearStart
  AND ac.AttendDate <= @YearEnd
  AND ISNULL(pm.UserId, '') <> '';", conn);

            cmd.Parameters.AddWithValue("@YearStart", new DateTime(year, 1, 1));
            cmd.Parameters.AddWithValue("@YearEnd", new DateTime(year, 12, 31));

            await conn.OpenAsync();

            var userIds = new List<string>();
            using (var rd = await cmd.ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                {
                    var userId = rd["UserId"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(userId))
                        userIds.Add(userId);
                }
            }

            foreach (var userId in userIds.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var attendanceCount = await _attendanceService.GetMyAttendanceCountYearToDateAsync(userId);
                var attendancePoint = Math.Max(0, attendanceCount) * ConstatsCommon.attendancePoint;
                if (attendancePoint > 0)
                    dict[userId] = attendancePoint;
            }

            return dict;
        }

        // ✅ 말씀&일기: year 기준 누적
        private async Task<Dictionary<string, (int Points, string FullName)>> LoadDevotionTotalPointsByUserIdAsync(int year)
        {
            var dict = new Dictionary<string, (int Points, string FullName)>(StringComparer.OrdinalIgnoreCase);
            var connStr = _config.GetConnectionString("DefaultConnection");

            const string sql = @"
SELECT
    u.UserId,
    u.FullName,
    ISNULL(SUM(CASE WHEN dm.Ym LIKE @YmPrefix THEN dm.Points ELSE 0 END), 0) AS TotalPoints
FROM dbo.view_UserCompositeInfo_2026 u WITH (NOLOCK)
LEFT JOIN dbo.Point_DevotionMonthly dm WITH (NOLOCK)
    ON dm.PersonId = u.PersonId
GROUP BY u.UserId, u.FullName;
";

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@YmPrefix", $"{year}-%");

            await conn.OpenAsync();

            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var userId = rd["UserId"]?.ToString() ?? "";
                var fullName = rd["FullName"]?.ToString() ?? "";
                var points = Convert.ToInt32(rd["TotalPoints"]);

                if (!string.IsNullOrWhiteSpace(userId) && points > 0)
                    dict[userId] = (points, fullName);
            }

            return dict;
        }

        // ✅ UserId 기준 이름 + 학년/반 일괄 로드
        private async Task<Dictionary<string, (string UserName, string FullName, int? ClassYear, int? ClassBan)>> LoadUserNameMapByUserIdAsync(IEnumerable<string> userIds)
        {
            var result = new Dictionary<string, (string UserName, string FullName, int? ClassYear, int? ClassBan)>(StringComparer.OrdinalIgnoreCase);
            var connStr = _config.GetConnectionString("DefaultConnection");

            var ids = userIds?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (ids.Count == 0)
                return result;

            using var conn = new SqlConnection(connStr);
            using var cmd = conn.CreateCommand();

            var paramNames = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                var paramName = $"@p{i}";
                cmd.Parameters.AddWithValue(paramName, ids[i]);
                paramNames.Add(paramName);
            }

            cmd.CommandText = $@"
SELECT
    u.UserId,
    ISNULL(u.UserName, '') AS UserName,
    ISNULL(u.FullName, '') AS FullName,
    ci.ClassYear,
    ci.ClassBan
FROM dbo.view_UserCompositeInfo_2026 u WITH (NOLOCK)
LEFT JOIN dbo.view_PersonMaster_2026 pm WITH (NOLOCK) ON pm.UserId = u.UserId
LEFT JOIN dbo.ClassInfo_2026 ci         WITH (NOLOCK) ON ci.PersonId = pm.PersonId
WHERE u.UserId IN ({string.Join(", ", paramNames)});
";

            await conn.OpenAsync();

            if (cmd is SqlCommand sqlCmd)
                await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(sqlCmd)}");

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var userId   = rd["UserId"]?.ToString() ?? "";
                var userName = rd["UserName"]?.ToString() ?? "";
                var fullName = rd["FullName"]?.ToString() ?? "";
                var classYear = rd.IsDBNull(rd.GetOrdinal("ClassYear")) ? (int?)null : Convert.ToInt32(rd["ClassYear"]);
                var classBan  = rd.IsDBNull(rd.GetOrdinal("ClassBan"))  ? (int?)null : Convert.ToInt32(rd["ClassBan"]);

                if (!string.IsNullOrWhiteSpace(userId))
                    result[userId] = (userName, fullName, classYear, classBan);
            }

            return result;
        }

        // ✅ UserId 기준 최근 활동일 (출석 + 말씀일기 기준 최댓값)
        private async Task<Dictionary<string, DateTime>> LoadLastActivityDateMapAsync(int year, IEnumerable<string> userIds)
        {
            var result = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            var connStr = _config.GetConnectionString("DefaultConnection");

            var ids = userIds?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (ids.Count == 0) return result;

            using var conn = new SqlConnection(connStr);
            using var cmd  = conn.CreateCommand();

            var paramNames = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@uid{i}", ids[i]);
                paramNames.Add($"@uid{i}");
            }
            cmd.Parameters.AddWithValue("@YearStart", new DateTime(year, 1, 1));
            cmd.Parameters.AddWithValue("@YearEnd",   new DateTime(year, 12, 31));
            cmd.Parameters.AddWithValue("@YmPrefix",  $"{year}-%");

            var inList = string.Join(", ", paramNames);
            cmd.CommandText = $@"
SELECT UserId, MAX(LastDate) AS LastDate
FROM (
    -- 출석
    SELECT pm.UserId, MAX(ac.AttendDate) AS LastDate
    FROM dbo.AttendanceCheck_2026 ac WITH (NOLOCK)
    JOIN dbo.view_PersonMaster_2026 pm WITH (NOLOCK) ON pm.PersonId = ac.PersonId
    WHERE ac.IsPresent = 1
      AND ac.AttendDate BETWEEN @YearStart AND @YearEnd
      AND pm.UserId IN ({inList})
    GROUP BY pm.UserId

    UNION ALL

    -- 말씀&일기 (Ym: '2026-01' 형식 → 해당 월 1일로 변환)
    SELECT pm2.UserId,
           MAX(DATEFROMPARTS(CAST(LEFT(dm.Ym,4) AS INT), CAST(RIGHT(dm.Ym,2) AS INT), 1)) AS LastDate
    FROM dbo.Point_DevotionMonthly dm WITH (NOLOCK)
    JOIN dbo.view_PersonMaster_2026 pm2 WITH (NOLOCK) ON pm2.PersonId = dm.PersonId
    WHERE dm.Ym LIKE @YmPrefix
      AND pm2.UserId IN ({inList})
    GROUP BY pm2.UserId
) t
WHERE UserId IS NOT NULL AND UserId <> ''
GROUP BY UserId;
";

            await conn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var userId = rd["UserId"]?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(userId) && !rd.IsDBNull(rd.GetOrdinal("LastDate")))
                    result[userId] = Convert.ToDateTime(rd["LastDate"]);
            }

            return result;
        }

        // ✅ UserId 기준 역할 라벨 일괄 로드
        private async Task<Dictionary<string, string>> LoadRoleLabelMapByUserIdAsync(IEnumerable<string> userIds)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var connStr = _config.GetConnectionString("DefaultConnection");

            var ids = userIds?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (ids.Count == 0)
                return result;

            using var conn = new SqlConnection(connStr);
            using var cmd = conn.CreateCommand();

            var paramNames = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                var paramName = $"@p{i}";
                cmd.Parameters.AddWithValue(paramName, ids[i]);
                paramNames.Add(paramName);
            }

            cmd.CommandText = $@"
SELECT
    u.UserId,
    u.RoleCode
FROM dbo.view_UserCompositeInfo_2026 u WITH (NOLOCK)
WHERE u.UserId IN ({string.Join(", ", paramNames)});
";

            await conn.OpenAsync();

            if (cmd is SqlCommand sqlCmd)
                await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(sqlCmd)}");

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var userId = rd["UserId"]?.ToString() ?? "";
                var roleCode = rd["RoleCode"]?.ToString() ?? "";

                if (!string.IsNullOrWhiteSpace(userId))
                    result[userId] = ToRoleLabel(roleCode);
            }

            return result;
        }

        // ✅ 화면 표시용 라벨 변환
        private string ToRoleLabel(string roleCode)
        {
            if (string.IsNullOrWhiteSpace(roleCode))
                return "기타";

            switch (roleCode.Trim().ToUpperInvariant())
            {
                case "STU":
                case "STUDENT":
                case "T1001":
                case "S":
                    return "학생";

                case "TCH":
                case "TEACHER":
                case "T1002":
                case "T1003":
                case "T1004":
                case "T1005":
                case "T1006":
                case "T1008":
                case "T1009":
                case "T1010":
                case "T1013":
                case "T1015":
                case "T1016":
                case "T1017":
                case "T1018":
                case "T1019":
                case "T1020":
                case "T1099":
                case "T":
                    return "교사";

                default:
                    return "기타";
            }
        }
    }
}
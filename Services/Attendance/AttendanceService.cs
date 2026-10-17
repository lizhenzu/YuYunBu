using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Attendance;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace BulletinBoard.Services
{
    public partial class AttendanceService : IAttendanceService
    {
        private readonly string _connectionString;
  
        public AttendanceService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection not found.");
        }

        private SqlConnection OpenConn()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
        public async Task<AttendanceAdminStudentMatrixVM> GetAdminStudentMatrixAsync(DateTime latestSunday, int weeks)
        {
            if (weeks <= 0) weeks = 2;
            if (weeks > 53) weeks = 53; // 1년치(최대 53주일)까지

            var vm = new AttendanceAdminStudentMatrixVM
            {
                Weeks = weeks,
                BaseDate = latestSunday.Date
            };

            // ✅ 미래 주일 방지
            var today = DateHelper.GetKoreaToday().Date;
            var safeLastSunday = today.AddDays(-(int)today.DayOfWeek).Date;
            if (latestSunday.Date > safeLastSunday) latestSunday = safeLastSunday;

            vm.EndSunday = latestSunday.Date;
            vm.StartSunday = latestSunday.AddDays(-7 * (weeks - 1)).Date;

            for (var d = vm.StartSunday; d <= vm.EndSunday; d = d.AddDays(7))
                vm.Sundays.Add(d.Date);

            using var conn = OpenConn();

            const string sql = @"
DECLARE @StartSunday date = @pStart;
DECLARE @EndSunday   date = @pEnd;

;WITH Sundays AS (
    SELECT @StartSunday AS SundayDate
    UNION ALL
    SELECT DATEADD(day, 7, SundayDate)
    FROM Sundays
    WHERE DATEADD(day, 7, SundayDate) <= @EndSunday
),
Students AS (
    SELECT DISTINCT
        ci.PersonId,
        ci.ClassYear,
        ci.ClassBan,
        StudentName = COALESCE(NULLIF(LTRIM(RTRIM(pm.fullName)), N''), N'친구')
    FROM dbo.[ClassInfo_2026] ci
    INNER JOIN dbo.[PersonMaster_2026] pm
        ON pm.PersonId = ci.PersonId
    WHERE ci.PersonId IS NOT NULL
      AND (ci.ts IS NULL OR ci.ts <> 'd')
)
SELECT
    st.PersonId,
    st.StudentName,
    st.ClassYear,
    st.ClassBan,
    su.SundayDate,
    IsPresent = CASE WHEN ac.PersonId IS NULL THEN 0 ELSE 1 END
FROM Students st
CROSS JOIN Sundays su
LEFT JOIN dbo.[AttendanceCheck_2026] ac
    ON ac.PersonId = st.PersonId
   AND ac.AttendDate = su.SundayDate
   AND CAST(ISNULL(ac.IsPresent, 0) AS bit) = 1
ORDER BY st.ClassYear, st.ClassBan, st.StudentName, su.SundayDate
OPTION (MAXRECURSION 400);";

            var map = new Dictionary<int, AttendanceAdminStudentMatrixVM.Row>();

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@pStart", SqlDbType.Date) { Value = vm.StartSunday });
                cmd.Parameters.Add(new SqlParameter("@pEnd", SqlDbType.Date) { Value = vm.EndSunday });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var personId = r.IsDBNull(0) ? 0 : Convert.ToInt32(r.GetValue(0));
                    if (personId <= 0) continue;

                    if (!map.TryGetValue(personId, out var row))
                    {
                        row = new AttendanceAdminStudentMatrixVM.Row
                        {
                            PersonId = personId,
                            StudentName = r.IsDBNull(1) ? "친구" : (Convert.ToString(r.GetValue(1)) ?? "친구"),
                            ClassYear = r.IsDBNull(2) ? 0 : Convert.ToInt32(r.GetValue(2)),
                            ClassBan = r.IsDBNull(3) ? 0 : Convert.ToInt32(r.GetValue(3))
                        };
                        map[personId] = row;
                    }

                    var sunday = r.GetDateTime(4).Date;
                    var key = sunday.ToString("yyyy-MM-dd");
                    var isPresent = !r.IsDBNull(5) && Convert.ToInt32(r.GetValue(5)) == 1;
                    row.PresentMap[key] = isPresent;
                }
            }

            vm.Rows = new List<AttendanceAdminStudentMatrixVM.Row>(map.Values);

            // 누락 키 채우기
            foreach (var row in vm.Rows)
                foreach (var su in vm.Sundays)
                {
                    var k = su.ToString("yyyy-MM-dd");
                    if (!row.PresentMap.ContainsKey(k)) row.PresentMap[k] = false;
                }

            return vm;
        }
        private async Task<(int PersonId, int PresentCount, int WeeksCount)> GetMyAttendanceYearToDateCoreAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return (0, 0, 1);

            var today = DateHelper.GetKoreaToday().Date;
            var latestSunday = today.AddDays(-(int)today.DayOfWeek).Date; // 미래 주일 방지
            var yearStart = new DateTime(latestSunday.Year, 1, 1);

            int CalcWeeksCount(DateTime start, DateTime endInclusiveSunday)
            {
                var firstSunday = start.Date.AddDays(((int)DayOfWeek.Sunday - (int)start.DayOfWeek + 7) % 7);
                if (firstSunday > endInclusiveSunday.Date) return 0;
                var days = (endInclusiveSunday.Date - firstSunday.Date).Days;
                return (days / 7) + 1;
            }

            var weeksCount = CalcWeeksCount(yearStart, latestSunday);
            if (weeksCount <= 0) weeksCount = 1;

            using var conn = OpenConn();

            int personId = 0;
            using (var cmd = new SqlCommand(@"
SELECT TOP(1) PersonId
FROM dbo.view_PersonMaster_2026
WHERE UserId = @UserId;", conn))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 450).Value = userId.Trim();
                var obj = await cmd.ExecuteScalarAsync();
                if (obj == null || obj == DBNull.Value) return (0, 0, weeksCount);
                personId = Convert.ToInt32(obj);
            }
            if (personId <= 0) return (0, 0, weeksCount);

            int presentCount = 0;
            using (var cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM dbo.AttendanceCheck_2026
WHERE PersonId = @PersonId
  AND IsPresent = 1
  AND AttendDate >= @YearStart
  AND AttendDate <= @LatestSunday;", conn))
            {
                cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
                cmd.Parameters.Add("@YearStart", SqlDbType.Date).Value = yearStart.Date;
                cmd.Parameters.Add("@LatestSunday", SqlDbType.Date).Value = latestSunday.Date;

                var obj = await cmd.ExecuteScalarAsync();
                presentCount = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            }

            if (presentCount < 0) presentCount = 0;
            if (presentCount > weeksCount) presentCount = weeksCount;

            return (personId, presentCount, weeksCount);
        }

        public async Task<int> GetMyAttendanceCountYearToDateAsync(string userId)
        {
            var info = await GetMyAttendanceYearToDateCoreAsync(userId);
            return info.PresentCount < 0 ? 0 : info.PresentCount;
        }


        public async Task<int> GetMyAttendanceWeeksYearToDateAsync(string userId)
        {
            var info = await GetMyAttendanceYearToDateCoreAsync(userId);
            return info.WeeksCount <= 0 ? 1 : info.WeeksCount;
        }

        public async Task<int> GetMyAttendanceRateYearToDatePercentAsync(string userId)
        {
            var info = await GetMyAttendanceYearToDateCoreAsync(userId);
            var weeksCount = info.WeeksCount <= 0 ? 1 : info.WeeksCount;
            var presentCount = info.PresentCount;

            var rate = (int)Math.Round((presentCount * 100.0) / weeksCount, 0);
            if (rate < 0) rate = 0;
            if (rate > 100) rate = 100;
            return rate;
        }
        public async Task<AttendanceCheckVM> GetCheckPageAsync(DateTime date, int classYear, int classBan)
        {
            var vm = new AttendanceCheckVM
            {
                Date = date.Date,
                ClassYear = classYear,
                ClassBan = classBan
            };

            using var conn = OpenConn();

            // 1) 반 목록
            const string sqlClasses = @"
                                        SELECT
                                            ClassYear,
                                            ClassBan,
                                            ISNULL(ClassName, CONCAT(ClassYear, N'학년 ', ClassBan, N'반')) AS ClassName
                                        FROM dbo.ClassMaster
                                        WHERE IsActive = 1
                                        ORDER BY SortOrder, ClassYear, ClassBan;";

            using (var cmd = new SqlCommand(sqlClasses, conn))
            using (var r = await cmd.ExecuteReaderAsync())
            {
                while (await r.ReadAsync())
                {
                    vm.Classes.Add(new AttendanceCheckVM.ClassItem
                    {
                        ClassYear = r.GetInt32(0),
                        ClassBan = r.GetInt32(1),
                        ClassName = r.IsDBNull(2) ? "" : r.GetString(2)
                    });
                }
            }

            // ✅ Q40-2(2차): 오늘(기준 주일) 아직 미체크인 반(출석 기록 0건) 목록
            // 정책: "한 반에 최소 한 명은 무조건 온다"가 보장되므로,
            //       해당 주일에 AttendanceCheck 레코드가 0건인 반 = 아직 체크하지 않은 반.
            const string sqlNotChecked = @"
SELECT
    cm.ClassYear,
    cm.ClassBan,
    ISNULL(cm.ClassName, CONCAT(cm.ClassYear, N'학년 ', cm.ClassBan, N'반')) AS ClassName
FROM dbo.ClassMaster cm
WHERE cm.IsActive = 1
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.ClassInfo_2026 ci
        INNER JOIN dbo.AttendanceCheck_2026 ac
            ON ac.PersonId = ci.PersonId
           AND ac.AttendDate = @D
        WHERE ci.ClassYear = cm.ClassYear
          AND ci.ClassBan  = cm.ClassBan
  )
  AND (
        @Y <= 0 OR @B <= 0
        OR cm.ClassYear <> @Y
        OR cm.ClassBan  <> @B
  )
ORDER BY cm.SortOrder, cm.ClassYear, cm.ClassBan;";

            using (var cmd = new SqlCommand(sqlNotChecked, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = date.Date });
                cmd.Parameters.Add(new SqlParameter("@Y", SqlDbType.Int) { Value = classYear });
                cmd.Parameters.Add(new SqlParameter("@B", SqlDbType.Int) { Value = classBan });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.NotCheckedClasses.Add(new AttendanceCheckVM.ClassItem
                    {
                        ClassYear = r.GetInt32(0),
                        ClassBan = r.GetInt32(1),
                        ClassName = r.IsDBNull(2) ? "" : r.GetString(2)
                    });
                }
            }
            vm.NotCheckedCount = vm.NotCheckedClasses.Count;

            if (classYear <= 0 || classBan <= 0)
                return vm;

            // 2) 선택 반 이름
            const string sqlClassName = @"
SELECT TOP 1
    ISNULL(ClassName, CONCAT(ClassYear, N'학년 ', ClassBan, N'반'))
FROM dbo.ClassMaster
WHERE ClassYear=@Y AND ClassBan=@B;";

            using (var cmd = new SqlCommand(sqlClassName, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@Y", SqlDbType.Int) { Value = classYear });
                cmd.Parameters.Add(new SqlParameter("@B", SqlDbType.Int) { Value = classBan });

                vm.ClassName = (await cmd.ExecuteScalarAsync())?.ToString();
            }

            // 3) 학생 목록 + 출석(BOOL) 조인 (레코드 없으면 false)
            const string sqlStudents = @"
SELECT
    ci.PersonId AS PersonId,
    COALESCE(NULLIF(LTRIM(RTRIM(pm.fullName)), N''), N'친구') AS StudentName,
    CAST(ISNULL(ac.IsPresent, 0) AS bit) AS IsPresent
FROM dbo.[ClassInfo_2026] AS ci
INNER JOIN dbo.[PersonMaster_2026] AS pm
    ON pm.PersonId = ci.PersonId
LEFT JOIN dbo.[AttendanceCheck_2026] AS ac
    ON ac.PersonId = ci.PersonId
   AND ac.AttendDate = @D
WHERE ci.ClassYear = @Y
  AND ci.ClassBan  = @B
  AND ci.PersonId IS NOT NULL
ORDER BY StudentName;";

            using (var cmd = new SqlCommand(sqlStudents, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = date.Date });
                cmd.Parameters.Add(new SqlParameter("@Y", SqlDbType.Int) { Value = classYear });
                cmd.Parameters.Add(new SqlParameter("@B", SqlDbType.Int) { Value = classBan });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.Students.Add(new AttendanceCheckVM.StudentRow
                    {
                        PersonId = r.IsDBNull(0) ? 0 : r.GetInt32(0),
                        StudentName = r.IsDBNull(1) ? "친구" : r.GetString(1),
                        IsPresent = !r.IsDBNull(2) && r.GetBoolean(2)
                    });
                }
            }

            return vm;
        }
        public async Task<AttendanceAdminStudentRankVM> GetAdminStudentRank3WeeksAsync(DateTime baseDate)
        {
            var vm = new AttendanceAdminStudentRankVM
            {
                BaseDate = baseDate.Date
            };

            // ✅ 기준 주일 계산 (AdminTrend3Weeks와 동일 규칙)
            var d = baseDate.Date;
            int backToSunday = (int)d.DayOfWeek;
            var thisSunday = d.AddDays(-backToSunday);

            vm.ThisSunday = thisSunday.Date;
            vm.PrevSunday1 = thisSunday.AddDays(-7).Date;
            vm.PrevSunday2 = thisSunday.AddDays(-14).Date;

            using var conn = OpenConn();

            // 1) 현재 총원
            const string sqlTotalNow = @"
SELECT COUNT(DISTINCT ci.PersonId)
FROM dbo.[ClassInfo_2026] ci
WHERE ci.PersonId IS NOT NULL
  AND (ci.ts IS NULL OR ci.ts <> 'd');";
            using (var cmd = new SqlCommand(sqlTotalNow, conn))
            {
                var obj = await cmd.ExecuteScalarAsync();
                vm.TotalStudentsNow = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            }

            // 2) 최근 3주(주일 3번) 출석 횟수로 학생 랭킹
            const string sql = @"
DECLARE @S0 date = @ThisSunday;
DECLARE @S1 date = @PrevSunday1;
DECLARE @S2 date = @PrevSunday2;

;WITH Sundays AS (
    SELECT @S0 AS SundayDate
    UNION ALL SELECT @S1
    UNION ALL SELECT @S2
),
Students AS (
    SELECT DISTINCT
        ci.PersonId,
        ci.ClassYear,
        ci.ClassBan,
        StudentName = COALESCE(NULLIF(LTRIM(RTRIM(pm.fullName)), N''), N'친구')
    FROM dbo.[ClassInfo_2026] ci
    INNER JOIN dbo.[PersonMaster_2026] pm
        ON pm.PersonId = ci.PersonId
    WHERE ci.PersonId IS NOT NULL
      AND (ci.ts IS NULL OR ci.ts <> 'd')
),
Agg AS (
    SELECT
        st.PersonId,
        PresentCount = SUM(CASE WHEN ac.PersonId IS NOT NULL THEN 1 ELSE 0 END)
    FROM Students st
    CROSS JOIN Sundays su
    LEFT JOIN dbo.[AttendanceCheck_2026] ac
        ON ac.PersonId = st.PersonId
       AND ac.AttendDate = su.SundayDate
       AND CAST(ISNULL(ac.IsPresent, 0) AS bit) = 1
    GROUP BY st.PersonId
)
SELECT
    RankNo = DENSE_RANK() OVER (ORDER BY a.PresentCount DESC),
    st.PersonId,
    st.StudentName,
    st.ClassYear,
    st.ClassBan,
    a.PresentCount
FROM Students st
INNER JOIN Agg a ON a.PersonId = st.PersonId
ORDER BY a.PresentCount DESC, st.StudentName ASC;";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@ThisSunday", SqlDbType.Date) { Value = vm.ThisSunday });
                cmd.Parameters.Add(new SqlParameter("@PrevSunday1", SqlDbType.Date) { Value = vm.PrevSunday1 });
                cmd.Parameters.Add(new SqlParameter("@PrevSunday2", SqlDbType.Date) { Value = vm.PrevSunday2 });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.Rows.Add(new AttendanceAdminStudentRankVM.Row
                    {
                        RankNo = r.IsDBNull(0) ? 0 : Convert.ToInt32(r.GetValue(0)),      // ✅ 핵심
                        PersonId = r.IsDBNull(1) ? 0 : Convert.ToInt32(r.GetValue(1)),    // ✅ 핵심
                        StudentName = r.IsDBNull(2) ? "친구" : Convert.ToString(r.GetValue(2)) ?? "친구",
                        ClassYear = r.IsDBNull(3) ? 0 : Convert.ToInt32(r.GetValue(3)),   // ✅ 핵심
                        ClassBan = r.IsDBNull(4) ? 0 : Convert.ToInt32(r.GetValue(4)),    // ✅ 핵심
                        PresentCount = r.IsDBNull(5) ? 0 : Convert.ToInt32(r.GetValue(5)) // ✅ 핵심 (SUM이 bigint로 나오는 경우)
                    });
                }
            }


            return vm;
        }

        public async Task<AttendanceAdminStudentRankVM> GetAdminStudentRankYearToDateAsync(DateTime latestSunday)
        {
            var vm = new AttendanceAdminStudentRankVM();

            // ✅ 오늘(한국) 기준 최근 주일(미래 방지)
            var today =  DateHelper.GetKoreaToday().Date;
            var safeLastSunday = today.AddDays(-(int)today.DayOfWeek).Date;

            // latestSunday가 미래면 무조건 안전한 최근 주일로 교정
            if (latestSunday.Date > safeLastSunday)
                latestSunday = safeLastSunday;

            vm.ThisSunday = latestSunday.Date;

            // ✅ 올해 시작일
            var yearStart = new DateTime(vm.ThisSunday.Year, 1, 1);

            // ✅ 올해 첫 주일: 1/1 이후 첫 "일요일"
            // (DayOfWeek: Sunday=0)
            var firstSunday = yearStart.AddDays((7 - (int)yearStart.DayOfWeek) % 7).Date;

            // date picker 등에 사용할 기준일(원하면 yearStart/firstSunday로 둬도 됨)
            vm.BaseDate = vm.ThisSunday;

            using var conn = OpenConn();

            // 1) 현재 총원
            const string sqlTotalNow = @"
SELECT COUNT(DISTINCT ci.PersonId)
FROM dbo.[ClassInfo_2026] ci
WHERE ci.PersonId IS NOT NULL
  AND (ci.ts IS NULL OR ci.ts <> 'd');";

            using (var cmd = new SqlCommand(sqlTotalNow, conn))
            {
                var obj = await cmd.ExecuteScalarAsync();
                vm.TotalStudentsNow = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            }

            // 2) WeeksCount(올해 누적 주일 수) 먼저 구해두기
            //    - 아래 메인 쿼리에서 row가 0개면 WeeksCount 세팅이 안될 수 있으니 안전장치
            const string sqlWeeksOnly = @"
DECLARE @FirstSunday date = @pFirstSunday;
DECLARE @LastSunday  date = @pLastSunday;

;WITH Sundays AS (
    SELECT @FirstSunday AS SundayDate
    UNION ALL
    SELECT DATEADD(day, 7, SundayDate)
    FROM Sundays
    WHERE DATEADD(day, 7, SundayDate) <= @LastSunday
)
SELECT COUNT(1) AS WeeksCount
FROM Sundays
OPTION (MAXRECURSION 400);";

            using (var cmd = new SqlCommand(sqlWeeksOnly, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@pFirstSunday", SqlDbType.Date) { Value = firstSunday });
                cmd.Parameters.Add(new SqlParameter("@pLastSunday", SqlDbType.Date) { Value = vm.ThisSunday });

                var obj = await cmd.ExecuteScalarAsync();
                vm.WeeksCount = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            }

            // 3) 올해 주일 전체 생성 + 학생별 출석 누적 집계 + 등수
            const string sql = @"
DECLARE @FirstSunday date = @pFirstSunday;
DECLARE @LastSunday  date = @pLastSunday;

;WITH Sundays AS (
    SELECT @FirstSunday AS SundayDate
    UNION ALL
    SELECT DATEADD(day, 7, SundayDate)
    FROM Sundays
    WHERE DATEADD(day, 7, SundayDate) <= @LastSunday
),
Students AS (
    SELECT DISTINCT
        ci.PersonId,
        ci.ClassYear,
        ci.ClassBan,
        StudentName = COALESCE(NULLIF(LTRIM(RTRIM(pm.fullName)), N''), N'친구')
    FROM dbo.[ClassInfo_2026] ci
    INNER JOIN dbo.[PersonMaster_2026] pm
        ON pm.PersonId = ci.PersonId
    WHERE ci.PersonId IS NOT NULL
      AND (ci.ts IS NULL OR ci.ts <> 'd')
),
Agg AS (
    SELECT
        st.PersonId,
        PresentCount = SUM(CASE WHEN ac.PersonId IS NOT NULL THEN 1 ELSE 0 END)
    FROM Students st
    CROSS JOIN Sundays su
    LEFT JOIN dbo.[AttendanceCheck_2026] ac
        ON ac.PersonId = st.PersonId
       AND ac.AttendDate = su.SundayDate
       AND CAST(ISNULL(ac.IsPresent, 0) AS bit) = 1
    GROUP BY st.PersonId
)
SELECT
    RankNo = DENSE_RANK() OVER (ORDER BY a.PresentCount DESC),
    st.PersonId,
    st.StudentName,
    st.ClassYear,
    st.ClassBan,
    a.PresentCount
FROM Students st
INNER JOIN Agg a ON a.PersonId = st.PersonId
ORDER BY a.PresentCount DESC, st.StudentName ASC
OPTION (MAXRECURSION 400);";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@pFirstSunday", SqlDbType.Date) { Value = firstSunday });
                cmd.Parameters.Add(new SqlParameter("@pLastSunday", SqlDbType.Date) { Value = vm.ThisSunday });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.Rows.Add(new AttendanceAdminStudentRankVM.Row
                    {
                        RankNo = r.IsDBNull(0) ? 0 : Convert.ToInt32(r.GetValue(0)),
                        PersonId = r.IsDBNull(1) ? 0 : Convert.ToInt32(r.GetValue(1)),
                        StudentName = r.IsDBNull(2) ? "친구" : (Convert.ToString(r.GetValue(2)) ?? "친구"),
                        ClassYear = r.IsDBNull(3) ? 0 : Convert.ToInt32(r.GetValue(3)),
                        ClassBan = r.IsDBNull(4) ? 0 : Convert.ToInt32(r.GetValue(4)),
                        PresentCount = r.IsDBNull(5) ? 0 : Convert.ToInt32(r.GetValue(5)),
                    });
                }
            }

            return vm;
        }
        public async Task<SavePresentResult> SavePresentAsync(DateTime attendSunday, int personId, bool isPresent, string? updatedBy)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                await con.OpenAsync();

                if (isPresent)
                {
                    // ✅ ON: Insert(출석=1만 저장). 이미 있으면 무시.
                    var sql = @"
                                IF NOT EXISTS (
                                    SELECT 1
                                    FROM dbo.[AttendanceCheck_2026]
                                    WHERE AttendDate = @AttendDate AND PersonId = @PersonId
                                )
                                BEGIN
                                    INSERT INTO dbo.[AttendanceCheck_2026] (AttendDate, PersonId, IsPresent, UpdatedAt, UpdatedBy)
                                    VALUES (@AttendDate, @PersonId, 1, @UpdatedAt, @UpdatedBy);
                                END
                                ELSE
                                BEGIN
                                    -- 이미 있으면 UpdatedAt/By만 갱신(선택)
                                    UPDATE dbo.[AttendanceCheck_2026]
                                    SET IsPresent = 1,
                                        UpdatedAt = @UpdatedAt,
                                        UpdatedBy = @UpdatedBy
                                    WHERE AttendDate = @AttendDate AND PersonId = @PersonId;
                                END
                                ";
                    using var cmd = new SqlCommand(sql, con);
                    cmd.Parameters.Add("@AttendDate", SqlDbType.Date).Value = attendSunday.Date;
                    cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
                    cmd.Parameters.AddWithValue("@UpdatedAt", SharedComm.GetKoreaNow());
                    cmd.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar, 100).Value = (object?)updatedBy ?? DBNull.Value;
                    await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
                    await cmd.ExecuteNonQueryAsync();
                    return new SavePresentResult { Ok = true, Message = "저장됐어요" };
                }
                else
                {
                    // ✅ OFF: Delete(행 삭제). 없으면 그냥 성공.
                    var sql = @"
                                DELETE FROM dbo.[AttendanceCheck_2026]
                                WHERE AttendDate = @AttendDate AND PersonId = @PersonId;
                                ";
                    using var cmd = new SqlCommand(sql, con);
                    cmd.Parameters.Add("@AttendDate", SqlDbType.Date).Value = attendSunday.Date;
                    cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
                    await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
                    await cmd.ExecuteNonQueryAsync();
                    return new SavePresentResult { Ok = true, Message = "해제됐어요" };
                }
            }
            catch (Exception ex)
            {
                // 로그는 프로젝트 기존 로깅 사용
                return new SavePresentResult { Ok = false, Message = "저장 중 오류가 발생했습니다." };
            }
        }

        public async Task<AttendanceDashboardVM> GetDashboardAsync(DateTime date, int classYear, int classBan)
        {
            var vm = new AttendanceDashboardVM
            {
                Date = date.Date,
                ClassYear = classYear,
                ClassBan = classBan
            };

            using var conn = OpenConn();

            // 1) 반 목록
            const string sqlClasses = @"
                                        SELECT ClassYear, ClassBan,
                                               ISNULL(ClassName, CONCAT(ClassYear, N'학년 ', ClassBan, N'반')) AS ClassName
                                        FROM dbo.ClassMaster
                                        WHERE IsActive = 1
                                        ORDER BY SortOrder, ClassYear, ClassBan;";

            using (var cmd = new SqlCommand(sqlClasses, conn))
            using (var r = await cmd.ExecuteReaderAsync())
            {
                while (await r.ReadAsync())
                {
                    vm.Classes.Add(new AttendanceDashboardVM.ClassItem
                    {
                        ClassYear = r.GetInt32(0),
                        ClassBan = r.GetInt32(1),
                        ClassName = r.IsDBNull(2) ? "" : r.GetString(2)
                    });
                }
            }

            if (classYear <= 0 || classBan <= 0)
                return vm;

            // 2) 반 이름
            const string sqlClassName = @"
                                            SELECT TOP 1 ISNULL(ClassName, CONCAT(ClassYear, N'학년 ', ClassBan, N'반'))
                                            FROM dbo.ClassMaster
                                            WHERE ClassYear=@Y AND ClassBan=@B;";

            using (var cmd = new SqlCommand(sqlClassName, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@Y", SqlDbType.Int) { Value = classYear });
                cmd.Parameters.Add(new SqlParameter("@B", SqlDbType.Int) { Value = classBan });
                vm.ClassName = (await cmd.ExecuteScalarAsync())?.ToString();
            }

            // 주간 범위 (월~일)
            // date가 어떤 요일이든, 해당 주의 월요일을 시작으로 잡음
            var d = date.Date;
            int diff = ((int)d.DayOfWeek + 6) % 7; // Monday=0 ... Sunday=6
            var weekStart = d.AddDays(-diff);
            var weekEnd = weekStart.AddDays(6);
            vm.WeekStart = weekStart;
            vm.WeekEnd = weekEnd;

            // 3) 오늘 요약(총원/출석수)
            // 레코드 없으면 0(false)로 간주 => 결석 처리
            const string sqlTodaySummary = @"
                                        ;WITH classStudents AS (
                                            SELECT ci.PersonId
                                            FROM dbo.[ClassInfo_2026] ci
                                            WHERE ci.ClassYear=@Y AND ci.ClassBan=@B
                                              AND ci.PersonId IS NOT NULL
                                              AND (ci.ts IS NULL OR ci.ts <> 'd')
                                        ),
                                        todayPresence AS (
                                            SELECT s.PersonId, CAST(ISNULL(ac.IsPresent, 0) AS BIT) AS IsPresent
                                            FROM classStudents s
                                            LEFT JOIN dbo.[AttendanceCheck_2026] ac
                                              ON ac.PersonId = s.PersonId
                                             AND ac.AttendDate = @D
                                        )
                                        SELECT
                                            (SELECT COUNT(*) FROM classStudents) AS TotalStudents,
                                            (SELECT COUNT(*) FROM todayPresence WHERE IsPresent=1) AS PresentCount;";

            using (var cmd = new SqlCommand(sqlTodaySummary, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = d });
                cmd.Parameters.Add(new SqlParameter("@Y", SqlDbType.Int) { Value = classYear });
                cmd.Parameters.Add(new SqlParameter("@B", SqlDbType.Int) { Value = classBan });

                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    vm.TotalStudents = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                    vm.PresentCount = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                }
            }

            // 4) 오늘 결석자 목록 (IsPresent=0 또는 레코드 없음)
            const string sqlAbsentList = @"
                                            SELECT
                                                ci.PersonID AS PersonId,
                                                COALESCE(NULLIF(LTRIM(RTRIM(pm.fullName)), N''), N'친구') AS StudentName
                                            FROM dbo.[ClassInfo_2026] AS ci
                                            INNER JOIN dbo.PersonMaster_2026 AS pm
                                                ON pm.PersonId = ci.PersonID
                                            LEFT JOIN dbo.[AttendanceCheck_2026] AS ac
                                                ON ac.PersonId = ci.PersonID
                                               AND ac.AttendDate = @D
                                            WHERE ci.ClassYear = @Y
                                              AND ci.ClassBan  = @B
                                              AND ci.PersonID IS NOT NULL
                                              AND CAST(ISNULL(ac.IsPresent, 0) AS bit) = 0
                                            ORDER BY StudentName;";

            using (var cmd = new SqlCommand(sqlAbsentList, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = d });
                cmd.Parameters.Add(new SqlParameter("@Y", SqlDbType.Int) { Value = classYear });
                cmd.Parameters.Add(new SqlParameter("@B", SqlDbType.Int) { Value = classBan });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.AbsentStudents.Add(new AttendanceDashboardVM.StudentRow
                    {
                        PersonId = r.IsDBNull(0) ? 0 : r.GetInt32(0),
                        StudentName = r.IsDBNull(1) ? "친구" : r.GetString(1)
                    });
                }
            }

            // 5) 주간 통계(월~일) : 날짜별 출석수/출석률
            // 핵심: "해당 반 학생들" 기준으로 날짜별 출석수를 세고, 총원은 고정(그 주 기준 반편성)
            // (주의) 반편성이 주간 중간에 변할 수 있다면, 총원은 날짜별로 계산하는 방식으로 바꿔야 함.
            const string sqlWeekly = @"
;WITH classStudents AS (
    SELECT DISTINCT ci.PersonId
    FROM dbo.[ClassInfo_2026] ci
    WHERE ci.ClassYear=@Y AND ci.ClassBan=@B
      AND ci.PersonId IS NOT NULL
      AND (ci.ts IS NULL OR ci.ts <> 'd')
),
dates AS (
    SELECT @S AS D
    UNION ALL SELECT DATEADD(DAY, 1, D) FROM dates WHERE D < @E
),
daily AS (
    SELECT
        d.D AS AttendDate,
        (SELECT COUNT(*) FROM classStudents) AS TotalStudents,
        (
            SELECT COUNT(*)
            FROM classStudents s
            LEFT JOIN dbo.[AttendanceCheck_2026] ac
              ON ac.PersonId = s.PersonId AND ac.AttendDate = d.D
            WHERE CAST(ISNULL(ac.IsPresent, 0) AS BIT) = 1
        ) AS PresentCount
    FROM dates d
)
SELECT AttendDate, TotalStudents, PresentCount
FROM daily
ORDER BY AttendDate
OPTION (MAXRECURSION 1000);";

            using (var cmd = new SqlCommand(sqlWeekly, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@Y", SqlDbType.Int) { Value = classYear });
                cmd.Parameters.Add(new SqlParameter("@B", SqlDbType.Int) { Value = classBan });
                cmd.Parameters.Add(new SqlParameter("@S", SqlDbType.Date) { Value = weekStart });
                cmd.Parameters.Add(new SqlParameter("@E", SqlDbType.Date) { Value = weekEnd });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var day = r.GetDateTime(0).Date;
                    var total = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                    var present = r.IsDBNull(2) ? 0 : r.GetInt32(2);
                    var rate = total <= 0 ? 0m : Math.Round((decimal)present * 100m / (decimal)total, 1);

                    vm.Weekly.Add(new AttendanceDashboardVM.DailyStat
                    {
                        Date = day,
                        Total = total,
                        Present = present,
                        Rate = rate
                    });
                }
            }

            return vm;
        }




        public async Task<AttendanceAdminTrendVM> GetAdminTrend3WeeksAsync(DateTime baseDate)
        {
            var vm = new AttendanceAdminTrendVM
            {
                BaseDate = baseDate.Date
            };

            // ✅ 기준 주일(일요일) 계산: baseDate 기준 "가장 최근의 일요일"
            // DayOfWeek: Sunday=0 ... Saturday=6
            var d = baseDate.Date;
            int backToSunday = (int)d.DayOfWeek;   // 일요일이면 0, 월요일이면 1...
            var thisSunday = d.AddDays(-backToSunday);
            vm.ThisSunday = thisSunday;

            using var conn = OpenConn();

            // 1) 현재 총원(참고용)
            const string sqlTotalNow = @"
SELECT COUNT(DISTINCT ci.PersonId)
FROM dbo.[ClassInfo_2026] ci
WHERE ci.PersonId IS NOT NULL
  AND (ci.ts IS NULL OR ci.ts <> 'd');";

            using (var cmd = new SqlCommand(sqlTotalNow, conn))
            {
                var obj = await cmd.ExecuteScalarAsync();
                vm.TotalStudentsNow = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            }

            // 2) 3주 집계 (한 번에)
            // - @S0 = 이번주 주일, @S1=2주전(1주전), @S2=3주전(2주전)
            var s0 = thisSunday;
            var s1 = thisSunday.AddDays(-7);
            var s2 = thisSunday.AddDays(-14);

            const string sql = @"
;WITH Students AS (
    SELECT DISTINCT ci.PersonId
    FROM dbo.[ClassInfo_2026] ci
    WHERE ci.PersonId IS NOT NULL
      AND (ci.ts IS NULL OR ci.ts <> 'd')
),
Base AS (
    SELECT CAST(@S0 AS date) AS SundayDate, N'이번주' AS Label, 0 AS SortNo
    UNION ALL SELECT CAST(@S1 AS date), N'지난주', 1
    UNION ALL SELECT CAST(@S2 AS date), N'지지난주', 2
),
Agg AS (
    SELECT
        b.Label,
        b.SortNo,
        b.SundayDate,
        (SELECT COUNT(*) FROM Students) AS TotalStudents,
        (
            SELECT COUNT(*)
            FROM Students s
            LEFT JOIN dbo.[AttendanceCheck_2026] ac
              ON ac.PersonId = s.PersonId
             AND ac.AttendDate = b.SundayDate
            WHERE CAST(ISNULL(ac.IsPresent, 0) AS bit) = 1
        ) AS PresentCount
    FROM Base b
)
SELECT Label, SundayDate, TotalStudents, PresentCount
FROM Agg
ORDER BY SortNo DESC;  -- 3주전 -> 2주전 -> 이번주";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@S0", SqlDbType.Date) { Value = s0 });
                cmd.Parameters.Add(new SqlParameter("@S1", SqlDbType.Date) { Value = s1 });
                cmd.Parameters.Add(new SqlParameter("@S2", SqlDbType.Date) { Value = s2 });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.Weeks.Add(new AttendanceAdminTrendVM.WeekRow
                    {
                        Label = r.IsDBNull(0) ? "" : r.GetString(0),
                        SundayDate = r.GetDateTime(1).Date,
                        Total = r.IsDBNull(2) ? 0 : r.GetInt32(2),
                        Present = r.IsDBNull(3) ? 0 : r.GetInt32(3)
                    });
                }
            }

            return vm;
        }

        public async Task<AttendanceAllClassesDashboardVM> GetAllClassesDashboardAsync(DateTime attendSunday, string? userId)
        {
            var vm = new AttendanceAllClassesDashboardVM
            {
                Date = attendSunday.Date
            };

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            // 0) 내 반 찾기(교사일 때만)
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var my = await GetMyClassByUserIdAsync(userId);
                if (my != null)
                {
                    vm.MyClassYear = my.ClassYear;
                    vm.MyClassBan = my.ClassBan;
                }
            }

            // 1) 전체 반 카드 데이터
            var sql = @"
DECLARE @AttendDate date = @D;

;WITH C AS (
    SELECT
        ci.ClassYear,
        ci.ClassBan,
        TotalStudents = COUNT(DISTINCT ci.PersonID)
    FROM dbo.ClassInfo_2026 ci
    WHERE ci.ClassYear IS NOT NULL AND ci.ClassBan IS NOT NULL AND ci.PersonID IS NOT NULL
    GROUP BY ci.ClassYear, ci.ClassBan
),
P AS (
    SELECT
        ci.ClassYear,
        ci.ClassBan,
        PresentCount = COUNT(DISTINCT ac.PersonId)
    FROM dbo.ClassInfo_2026 ci
    INNER JOIN dbo.AttendanceCheck_2026 ac
        ON ac.PersonId = ci.PersonID
       AND ac.AttendDate = @AttendDate
    GROUP BY ci.ClassYear, ci.ClassBan
),
T AS (
    SELECT
        m.ClassYear,
        m.ClassBan,
        m.TeacherPersonId,

        -- ✅ 담임 + 보조 표시
        TeacherName =
            CASE
                WHEN ISNULL(m.SubTeacherPersonId, 0) > 0
                     AND NULLIF(LTRIM(RTRIM(ISNULL(vs.fullName, N''))), N'') IS NOT NULL
                THEN CONCAT(
                        ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(vt.fullName, N''))), N''), N'미지정'),
                        N' / ',
                        NULLIF(LTRIM(RTRIM(ISNULL(vs.fullName, N''))), N'')
                     )
                ELSE ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(vt.fullName, N''))), N''), N'미지정')
            END

    FROM dbo.ClassTeacherMap_2026 m
    LEFT JOIN dbo.view_PersonMaster_2026 vt ON vt.PersonId = m.TeacherPersonId
    LEFT JOIN dbo.view_PersonMaster_2026 vs ON vs.PersonId = NULLIF(m.SubTeacherPersonId, 0)
    WHERE m.IsActive = 1
)
SELECT
    c.ClassYear,
    c.ClassBan,
    c.TotalStudents,
    PresentCount = ISNULL(p.PresentCount, 0),
    TeacherPersonId = t.TeacherPersonId,
    TeacherName = ISNULL(t.TeacherName, N'미지정'),
    IsChecked = CASE WHEN ISNULL(p.PresentCount,0) > 0 THEN 1 ELSE 0 END
FROM C c
LEFT JOIN P p ON p.ClassYear = c.ClassYear AND p.ClassBan = c.ClassBan
LEFT JOIN T t ON t.ClassYear = c.ClassYear AND t.ClassBan = c.ClassBan
ORDER BY
    CASE WHEN ISNULL(p.PresentCount,0) = 0 THEN 0 ELSE 1 END ASC,
    c.ClassYear ASC,
    c.ClassBan ASC;
";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@D", SqlDbType.Date).Value = attendSunday.Date;

                using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    int y = rd.GetInt32(rd.GetOrdinal("ClassYear"));
                    int b = rd.GetInt32(rd.GetOrdinal("ClassBan"));
                    int total = rd.GetInt32(rd.GetOrdinal("TotalStudents"));
                    int present = rd.GetInt32(rd.GetOrdinal("PresentCount"));
                    bool isChecked = rd.GetInt32(rd.GetOrdinal("IsChecked")) == 1;

                    int ordTeacherId = rd.GetOrdinal("TeacherPersonId");
                    int? teacherId = rd.IsDBNull(ordTeacherId) ? null : rd.GetInt32(ordTeacherId);

                    var teacherName = rd.GetString(rd.GetOrdinal("TeacherName"));

                    vm.Classes.Add(new AttendanceAllClassesDashboardVM.ClassCard
                    {
                        ClassYear = y,
                        ClassBan = b,
                        ClassName = $"{y}학년 {b}반",
                        TotalStudents = total,
                        PresentCount = present,
                        IsChecked = isChecked,
                        TeacherPersonId = teacherId,
                        TeacherName = string.IsNullOrWhiteSpace(teacherName) ? "미지정" : teacherName.Trim()
                    });

                    vm.TotalStudents += total;
                    vm.TotalPresent += present;
                }
            }

            // ✅ Q40-2(1차): 미체크 반 요약(출석 기록 0건)
            // 정책: "한 반에 최소 한 명은 무조건 온다"가 보장되므로,
            //       출석 기록이 0건인 반은 "아직 체크하지 않음"으로 간주.
            vm.NotCheckedClasses.Clear();
            foreach (var c in vm.Classes)
            {
                if (!c.IsChecked)
                {
                    vm.NotCheckedClasses.Add(new AttendanceAllClassesDashboardVM.NotCheckedClass
                    {
                        ClassYear = c.ClassYear,
                        ClassBan = c.ClassBan,
                        ClassName = c.ClassName,
                        TeacherName = c.TeacherName
                    });
                }
            }
            vm.NotCheckedCount = vm.NotCheckedClasses.Count;

            vm.TotalClasses = vm.Classes.Count;
            return vm;
        }

    
        public async Task<AttendanceMonthlyStatsVM> GetMonthlyStatsAsync(int year, int month)
        {
            // 1) 월 범위 계산 (해당 월 1일 ~ 말일)
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // 2) 해당 월의 모든 일요일(주일) 목록
            var sundays = GetSundaysOfMonth(year, month); // DateTime(date) 리스트

            var vm = new AttendanceMonthlyStatsVM
            {
                Year = year,
                Month = month,
                MonthCards = BuildMonthCards(year, month),
            };

            // 3) DB 집계
            var weeklyMap = new Dictionary<DateTime, int>(); // AttendDate -> TotalCount
            var classRows = new List<AttendanceMonthlyStatsVM.ClassMonthlyTotalRow>();

            await using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // (A) 주별(전체) 출석 인원: AttendDate별 COUNT
                // - 달 범위로 뽑고, 아래에서 일요일만 매핑/보정(누락은 0)
                const string sqlWeekly = @"
SELECT
    ac.AttendDate,
    COUNT(1) AS TotalCount
FROM dbo.AttendanceCheck_2026 ac
WHERE ac.AttendDate >= @StartDate
  AND ac.AttendDate <= @EndDate
  AND ac.IsPresent = 1
GROUP BY ac.AttendDate
ORDER BY ac.AttendDate;";

                await using (var cmd = new SqlCommand(sqlWeekly, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date) { Value = startDate.Date });
                    cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date) { Value = endDate.Date });

                    await using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        var d = rd.GetDateTime(0).Date;
                        var c = rd.GetInt32(1);
                        weeklyMap[d] = c;
                    }
                }

                // (B) 월별 반별 총 출석(누적) - Active 반만
                // ClassMaster(IsActive=1) 기준으로 반 목록 고정
                const string sqlClassMonthly = @"
;WITH ActiveClasses AS (
    SELECT ClassYear, ClassBan, ClassName
    FROM dbo.ClassMaster
    WHERE IsActive = 1
)
SELECT
    a.ClassYear,
    a.ClassBan,
    a.ClassName,
    COUNT(ac.AttendanceCheckId) AS TotalCount
FROM ActiveClasses a
LEFT JOIN dbo.ClassInfo_2026 ci
    ON ci.ClassYear = a.ClassYear
   AND ci.ClassBan  = a.ClassBan
LEFT JOIN dbo.AttendanceCheck_2026 ac
    ON ac.PersonId   = ci.PersonId
   AND ac.AttendDate >= @StartDate
   AND ac.AttendDate <= @EndDate
   AND ac.IsPresent  = 1
GROUP BY a.ClassYear, a.ClassBan, a.ClassName
ORDER BY TotalCount DESC, a.ClassYear ASC, a.ClassBan ASC;";

                await using (var cmd = new SqlCommand(sqlClassMonthly, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date) { Value = startDate.Date });
                    cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date) { Value = endDate.Date });

                    await using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        classRows.Add(new AttendanceMonthlyStatsVM.ClassMonthlyTotalRow
                        {
                            ClassYear = rd.GetInt32(0),
                            ClassBan = rd.GetInt32(1),
                            ClassName = rd.IsDBNull(2) ? $"{rd.GetInt32(0)}학년 {rd.GetInt32(1)}반" : rd.GetString(2),
                            TotalCount = rd.GetInt32(3),
                            Ratio = 0d, // 아래에서 계산
                        });
                    }
                }
            }

            // 4) 주별 리스트 만들기 (해당 월 일요일 전부 채우기: 누락은 0)
            foreach (var s in sundays)
            {
                vm.WeeklyTotals.Add(new AttendanceMonthlyStatsVM.WeeklyTotalRow
                {
                    AttendDate = s,
                    TotalCount = weeklyMap.TryGetValue(s, out var c) ? c : 0
                });
            }

            // 5) 반별 Ratio 계산 (전체 대비 %)
            var allTotal = classRows.Sum(x => x.TotalCount);
            foreach (var row in classRows)
            {
                row.Ratio = (allTotal <= 0)
                    ? 0d
                    : Math.Round((row.TotalCount * 100.0) / allTotal, 1);
            }
            vm.ClassMonthlyTotals = classRows;

            // 6) Top 3 계산 (TotalCount DESC, Ratio DESC)
            var top = classRows
                .OrderByDescending(x => x.TotalCount)
                .ThenByDescending(x => x.Ratio)
                .Take(3)
                .ToList();

            for (int i = 0; i < top.Count; i++)
            {
                vm.TopRanks.Add(new AttendanceMonthlyStatsVM.TopRankRow
                {
                    Rank = i + 1,
                    ClassYear = top[i].ClassYear,
                    ClassBan = top[i].ClassBan,
                    ClassName = top[i].ClassName,
                    TotalCount = top[i].TotalCount,
                    Ratio = top[i].Ratio
                });
            }

            return vm;
        }
        private static List<DateTime> GetSundaysInMonth(int year, int month)
        {
            var list = new List<DateTime>();
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            // 첫 일요일 찾기
            var d = start;
            while (d < end && d.DayOfWeek != DayOfWeek.Sunday)
                d = d.AddDays(1);

            while (d < end)
            {
                list.Add(d.Date);
                d = d.AddDays(7);
            }
            return list;
        }

          

        public async Task<MyClassInfo?> GetMyClassByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            // 1) UserId -> PersonId
            int? personId = null;
            using (var cmd = new SqlCommand(@"
SELECT TOP(1) PersonId
FROM dbo.view_PersonMaster_2026
WHERE UserId = @UserId;", con))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 450).Value = userId.Trim();
                var obj = await cmd.ExecuteScalarAsync();
                if (obj != null && obj != DBNull.Value) personId = Convert.ToInt32(obj);
            }

            if (personId == null || personId.Value <= 0) return null;

            // 2) PersonId -> 담당 반
            using (var cmd = new SqlCommand(@"
SELECT TOP(1) ClassYear, ClassBan
FROM dbo.ClassTeacherMap_2026
WHERE TeacherPersonId = @PersonId
  AND IsActive = 1
ORDER BY UpdateDT DESC, Id DESC;", con))
            {
                cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId.Value;

                using var rd = await cmd.ExecuteReaderAsync();
                if (!await rd.ReadAsync()) return null;

                return new MyClassInfo
                {
                    ClassYear = rd.GetInt32(rd.GetOrdinal("ClassYear")),
                    ClassBan = rd.GetInt32(rd.GetOrdinal("ClassBan"))
                };
            }
        }
         


        public async Task<TeacherAssignVM> GetTeacherAssignPageAsync()
        {
            var vm = new TeacherAssignVM();

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            // 1) TeacherCandidate 목록 (활성만)
            {
                var sql = @"
SELECT tc.PersonId, ISNULL(v.fullName,'(이름없음)') AS FullName
FROM dbo.TeacherCandidate_2026 tc
LEFT JOIN dbo.view_PersonMaster_2026 v ON v.PersonId = tc.PersonId
WHERE tc.IsActive = 1
ORDER BY ISNULL(v.fullName,'') ASC;";
                using var cmd = new SqlCommand(sql, con);
                using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    vm.Teachers.Add(new TeacherAssignVM.TeacherOption
                    {
                        PersonId = rd.GetInt32(rd.GetOrdinal("PersonId")),
                        FullName = rd.GetString(rd.GetOrdinal("FullName"))
                    });
                }
                rd.Close();
            }

            // 2) 반 목록(학생 배정 기반) + 현재 배정 교사
            {
                var sql = @"
;WITH C AS (
    SELECT
        ci.ClassYear,
        ci.ClassBan,
        TotalStudents = COUNT(DISTINCT ci.PersonID)
    FROM dbo.ClassInfo_2026 ci
    WHERE ci.ClassYear IS NOT NULL AND ci.ClassBan IS NOT NULL AND ci.PersonID IS NOT NULL
    GROUP BY ci.ClassYear, ci.ClassBan
),
M AS (
    SELECT
        m.ClassYear,
        m.ClassBan,
        m.TeacherPersonId
    FROM dbo.ClassTeacherMap_2026 m
    WHERE m.IsActive = 1
)
SELECT
    c.ClassYear,
    c.ClassBan,
    c.TotalStudents,
    m.TeacherPersonId,
    TeacherName = ISNULL(v.fullName, N'미지정')
FROM C c
LEFT JOIN M m ON m.ClassYear = c.ClassYear AND m.ClassBan = c.ClassBan
LEFT JOIN dbo.view_PersonMaster_2026 v ON v.PersonId = m.TeacherPersonId
ORDER BY c.ClassYear ASC, c.ClassBan ASC;";
                using var cmd = new SqlCommand(sql, con);
                using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    int y = rd.GetInt32(rd.GetOrdinal("ClassYear"));
                    int b = rd.GetInt32(rd.GetOrdinal("ClassBan"));
                    int total = rd.GetInt32(rd.GetOrdinal("TotalStudents"));

                    int ordTeacherId = rd.GetOrdinal("TeacherPersonId");
                    int? teacherId = rd.IsDBNull(ordTeacherId) ? null : rd.GetInt32(ordTeacherId);

                    vm.Classes.Add(new TeacherAssignVM.ClassRow
                    {
                        ClassYear = y,
                        ClassBan = b,
                        ClassName = $"{y}학년 {b}반",
                        TotalStudents = total,
                        AssignedTeacherPersonId = teacherId,
                        AssignedTeacherName = rd.GetString(rd.GetOrdinal("TeacherName"))
                    });
                }
            }

            return vm;
        }

        private TeacherInfo? GetTeacherInfoByUserId(string? loginUserId)
        {
            if (string.IsNullOrWhiteSpace(loginUserId))
                return null;

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
SELECT TOP 1 PersonId, fullName, UserId
FROM dbo.view_PersonMaster_2026
WHERE UserId = @UserId
", con);

            cmd.Parameters.AddWithValue("@UserId", loginUserId);

            con.Open();
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            return new TeacherInfo
            {
                PersonId = Convert.ToInt32(rd["PersonId"]),
                FullName = (rd["fullName"] as string) ?? "",
                UserId = (rd["UserId"] as string) ?? loginUserId
            };
        }
   
        public async Task<bool> SaveTeacherAssignAsync(int classYear, int classBan, int teacherPersonId, string? updatedBy = null)
        {
            if (classYear <= 0 || classBan <= 0) return false;

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            // teacherPersonId == 0 => 미지정(배정 해제): 해당 반 맵핑 비활성화
            if (teacherPersonId <= 0)
            {
                var sql = @"
UPDATE dbo.ClassTeacherMap_2026
SET IsActive = 0,
    UpdateDT = GETDATE()
WHERE ClassYear = @Y AND ClassBan = @B AND IsActive = 1;";
                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@Y", SqlDbType.Int).Value = classYear;
                cmd.Parameters.Add("@B", SqlDbType.Int).Value = classBan;
                await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
                await cmd.ExecuteNonQueryAsync();
                return true;
            }

            // ✅ 후보 교사인지 검증(TeacherCandidate_2026, IsActive=1)
            {
                var sqlCheck = @"
SELECT COUNT(1)
FROM dbo.TeacherCandidate_2026
WHERE PersonId = @T AND IsActive = 1;";
                using var cmdCheck = new SqlCommand(sqlCheck, con);
                cmdCheck.Parameters.Add("@T", SqlDbType.Int).Value = teacherPersonId;
                var cnt = Convert.ToInt32(await cmdCheck.ExecuteScalarAsync());
                if (cnt <= 0) return false;
            }

            // ✅ UPSERT (반은 UNIQUE(ClassYear, ClassBan))
            var sqlUpsert = @"
IF EXISTS (SELECT 1 FROM dbo.ClassTeacherMap_2026 WHERE ClassYear=@Y AND ClassBan=@B)
BEGIN
    UPDATE dbo.ClassTeacherMap_2026
    SET TeacherPersonId = @T,
        IsActive = 1,
        UpdateDT = GETDATE()
    WHERE ClassYear=@Y AND ClassBan=@B;
END
ELSE
BEGIN
    INSERT INTO dbo.ClassTeacherMap_2026(ClassYear, ClassBan, TeacherPersonId, IsActive, InsertDT, UpdateDT)
    VALUES(@Y, @B, @T, 1, GETDATE(), GETDATE());
END";
            using (var cmd = new SqlCommand(sqlUpsert, con))
            {
                cmd.Parameters.Add("@Y", SqlDbType.Int).Value = classYear;
                cmd.Parameters.Add("@B", SqlDbType.Int).Value = classBan;
                cmd.Parameters.Add("@T", SqlDbType.Int).Value = teacherPersonId;
                await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }

        public AttendanceCheckVM BuildCheckVm(string? dateStr, int? classYear, int? classBan, string? loginUserId)
        {
            var attendDate = ParseDateOrToday(dateStr);

            var teacher = GetTeacherInfoByUserId(loginUserId);
            var myClass = teacher == null ? null : GetMyClassByTeacherPersonId(teacher.PersonId);

            // ✅ 쿼리스트링에 반이 없으면 내 반으로 기본 선택
            int finalYear = (classYear.HasValue && classYear.Value > 0) ? classYear.Value : (myClass?.classYear ?? 0);
            int finalBan = (classBan.HasValue && classBan.Value > 0) ? classBan.Value : (myClass?.classBan ?? 0);

            var vm = new AttendanceCheckVM
            {
                Date = attendDate,
                ClassYear = finalYear,
                ClassBan = finalBan,
                IsMyClass = (myClass != null && finalYear == myClass.Value.classYear && finalBan == myClass.Value.classBan)
            };

            // ✅ 반 목록 로드
            vm.Classes = LoadAllClasses(attendDate); // 프로젝트 기존 로직 사용 가능
            vm.ClassName = vm.Classes.Find(x => x.ClassYear == finalYear && x.ClassBan == finalBan)?.ClassName;

            // ✅ 학생 + 출석여부 로드 (PersonId 기준)
            if (finalYear > 0 && finalBan > 0)
            {
                vm.Students = LoadStudentsWithPresent(attendDate, finalYear, finalBan);
            }

            return vm;
        }

        private DateTime ParseDateOrToday(string? dateStr)
        {
            if (!string.IsNullOrWhiteSpace(dateStr) && DateTime.TryParse(dateStr, out var dt))
                return dt.Date;
            return DateHelper.GetKoreaNow();
        }



        public SavePresentResult SavePresentByPersonId(string attendDateStr, int personId, bool isPresent, string? loginUserId)
        {
            if (!DateTime.TryParse(attendDateStr, out var dt))
                return new SavePresentResult { Ok = false, Message = "날짜 형식이 올바르지 않습니다." };

            var attendDate = dt.Date;

            var teacher = GetTeacherInfoByUserId(loginUserId);
            var teacherName = teacher?.FullName?.Trim();
            if (string.IsNullOrWhiteSpace(teacherName)) teacherName = "Unknown";

            var updatedBy = $"{teacherName}({loginUserId ?? "Anonymous"})";

            using var con = new SqlConnection(_connectionString);
            con.Open();

            if (isPresent)
            {
                // ✅ Insert (중복이면 무시 또는 업데이트)
                using var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.AttendanceCheck_2026 WHERE AttendDate=@AttendDate AND PersonId=@PersonId)
BEGIN
    INSERT INTO dbo.AttendanceCheck_2026(AttendDate, PersonId, IsPresent, UpdatedAt, UpdatedBy)
    VALUES(@AttendDate, @PersonId, 1, @UpdatedAt, @UpdatedBy)
END
ELSE
BEGIN
    UPDATE dbo.AttendanceCheck_2026
    SET IsPresent=1, UpdatedAt=@UpdatedAt, UpdatedBy=@UpdatedBy
    WHERE AttendDate=@AttendDate AND PersonId=@PersonId
END
", con);

                cmd.Parameters.AddWithValue("@AttendDate", attendDate);
                cmd.Parameters.AddWithValue("@PersonId", personId);
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                cmd.Parameters.AddWithValue("@AttendDate", DateHelper.GetKoreaNow());

                cmd.ExecuteNonQuery();
                return new SavePresentResult { Ok = true, Message = "저장됐어요" };
            }
            else
            {
                // ✅ Delete (출석 해제)
                using var cmd = new SqlCommand(@"
DELETE FROM dbo.AttendanceCheck_2026
WHERE AttendDate=@AttendDate AND PersonId=@PersonId
", con);

                cmd.Parameters.AddWithValue("@AttendDate", attendDate);
                cmd.Parameters.AddWithValue("@PersonId", personId);

                cmd.ExecuteNonQuery();
                return new SavePresentResult { Ok = true, Message = "삭제됐어요" };
            }
        }
    

        private List<AttendanceCheckVM.StudentRow> LoadStudentsWithPresent(DateTime attendDate, int classYear, int classBan)
        {
            var list = new List<AttendanceCheckVM.StudentRow>();

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
SELECT ci.PersonID AS PersonId,
       pm.fullName AS StudentName,
       CASE WHEN ac.AttendanceCheckId IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS IsPresent
FROM dbo.ClassInfo_2026 ci
JOIN dbo.PersonMaster_2026 pm ON pm.PersonId = ci.PersonID
LEFT JOIN dbo.AttendanceCheck_2026 ac
    ON ac.AttendDate = @AttendDate
   AND ac.PersonId = ci.PersonID
WHERE ci.ClassYear = @ClassYear
  AND ci.ClassBan  = @ClassBan
ORDER BY pm.fullName
", con);

            cmd.Parameters.AddWithValue("@AttendDate", attendDate.Date);
            cmd.Parameters.AddWithValue("@ClassYear", classYear);
            cmd.Parameters.AddWithValue("@ClassBan", classBan);

            con.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new AttendanceCheckVM.StudentRow
                {
                    PersonId = Convert.ToInt32(rd["PersonId"]),
                    StudentName = (rd["StudentName"] as string) ?? "친구",
                    IsPresent = Convert.ToBoolean(rd["IsPresent"])
                });
            }

            return list;
        }

        private List<AttendanceCheckVM.ClassItem> LoadAllClasses(DateTime attendDate)
        {
            var list = new List<AttendanceCheckVM.ClassItem>();

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
SELECT DISTINCT ClassYear, ClassBan
FROM dbo.ClassInfo_2026
WHERE ClassYear IS NOT NULL AND ClassBan IS NOT NULL
ORDER BY ClassYear, ClassBan
", con);

            con.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                var y = Convert.ToInt32(rd["ClassYear"]);
                var b = Convert.ToInt32(rd["ClassBan"]);
                list.Add(new AttendanceCheckVM.ClassItem
                {
                    ClassYear = y,
                    ClassBan = b,
                    ClassName = $"{y}학년 {b}반"
                });
            }

            return list;
        }


        private (int classYear, int classBan)? GetMyClassByTeacherPersonId(int teacherPersonId)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
SELECT TOP 1 ClassYear, ClassBan
FROM dbo.ClassTeacherMap_2026
WHERE TeacherPersonId = @TeacherPersonId
", con);

            cmd.Parameters.AddWithValue("@TeacherPersonId", teacherPersonId);

            con.Open();
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            return (Convert.ToInt32(rd["ClassYear"]), Convert.ToInt32(rd["ClassBan"]));
        }



        private static List<AttendanceMonthlyStatsVM.MonthCard> BuildMonthCards(int year, int selectedMonth)
        {
            var cards = new List<AttendanceMonthlyStatsVM.MonthCard>(12);
            for (int m = 1; m <= 12; m++)
            {
                cards.Add(new AttendanceMonthlyStatsVM.MonthCard
                {
                    Year = year,
                    Month = m,
                    Label = $"{m}월",
                    IsSelected = (m == selectedMonth),
                    Url = $"/Attendance/statistics?year={year}&month={m}"
                });
            }
            return cards;
        }


        private static List<DateTime> GetSundaysOfMonth(int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            // 첫 일요일 찾기
            var d = start;
            while (d.DayOfWeek != DayOfWeek.Sunday)
                d = d.AddDays(1);

            var list = new List<DateTime>();
            while (d <= end)
            {
                list.Add(d.Date);
                d = d.AddDays(7);
            }
            return list;
        }


        public async Task<AttendanceSundayClassDetailVM> GetSundayClassDetailAsync(DateTime sunday)
        {
            sunday = sunday.Date;

            var sql = @"
SELECT
    ci.ClassYear,
    ci.ClassBan,

    -- 반 이름
    MAX(ISNULL(cm.ClassName, CONCAT(ci.ClassYear, N'학년 ', ci.ClassBan, N'반'))) AS ClassName,

    -- ✅ 담임 + 보조교사 표시
    MAX(
        CASE
            WHEN ISNULL(ctm.SubTeacherPersonId, 0) > 0
                 AND NULLIF(LTRIM(RTRIM(ISNULL(stpm.FullName, N''))), N'') IS NOT NULL
            THEN CONCAT(
                    ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(tpm.FullName, N''))), N''), N''),
                    N' / ',
                    NULLIF(LTRIM(RTRIM(ISNULL(stpm.FullName, N''))), N'')
                 )
            ELSE ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(tpm.FullName, N''))), N''), N'')
        END
    ) AS TeacherName,

    COUNT(DISTINCT ci.PersonId) AS TotalStudents,
    ISNULL(SUM(CASE WHEN ac.IsPresent = 1 THEN 1 ELSE 0 END), 0) AS PresentCount
FROM ClassInfo_2026 ci

LEFT JOIN ClassMaster cm
    ON cm.ClassYear = ci.ClassYear
   AND cm.ClassBan  = ci.ClassBan
   AND cm.IsActive  = 1

LEFT JOIN ClassTeacherMap_2026 ctm
    ON ctm.ClassYear = ci.ClassYear
   AND ctm.ClassBan  = ci.ClassBan
   AND ctm.IsActive  = 1

-- 담임
LEFT JOIN PersonMaster_2026 tpm
    ON tpm.PersonId = ctm.TeacherPersonId

-- 보조 (SubTeacherPersonId > 0)
LEFT JOIN PersonMaster_2026 stpm
    ON stpm.PersonId = NULLIF(ctm.SubTeacherPersonId, 0)

LEFT JOIN AttendanceCheck_2026 ac
    ON ac.PersonId   = ci.PersonId
   AND ac.AttendDate = @SundayDate

GROUP BY ci.ClassYear, ci.ClassBan
ORDER BY ci.ClassYear, ci.ClassBan;
";

            var vm = new AttendanceSundayClassDetailVM
            {
                SundayDate = sunday
            };

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add(new SqlParameter("@SundayDate", SqlDbType.Date) { Value = sunday });

            await conn.OpenAsync();

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var classYear = rd.GetInt32(rd.GetOrdinal("ClassYear"));
                var classBan = rd.GetInt32(rd.GetOrdinal("ClassBan"));

                var className = rd["ClassName"]?.ToString() ?? $"{classYear}학년 {classBan}반";
                var teacherName = rd["TeacherName"]?.ToString() ?? "";

                var total = Convert.ToInt32(rd["TotalStudents"]);
                var present = Convert.ToInt32(rd["PresentCount"]);
                var absent = Math.Max(0, total - present);
                var rate = total <= 0 ? 0 : Math.Round(present * 100.0 / total, 1);

                vm.Classes.Add(new AttendanceSundayClassRowVM
                {
                    ClassYear = classYear,
                    ClassBan = classBan,
                    ClassName = className,
                    TeacherName = teacherName, // ✅ 여기로 "담임 / 보조"가 들어옴
                    TotalStudents = total,
                    PresentCount = present,
                    AbsentCount = absent,
                    Rate = rate
                });
            }

            return vm;
        }

        public async Task<AttendanceSundayStudentsVM> GetSundayStudentsDetailAsync(DateTime sunday)
        {
            sunday = sunday.Date;

            // 올해 시작일
            var yearStart = new DateTime(sunday.Year, 1, 1);

            // 기준일이 주일이 아니면 해당 주의 최근 주일로 보정
            var diff = (int)sunday.DayOfWeek; // Sun=0
            var thisSunday = sunday.AddDays(-diff).Date;
            sunday = thisSunday;

            // 올해 누적 주일 수
            int CalcWeeksCount(DateTime start, DateTime endInclusiveSunday)
            {
                var firstSunday = start.Date.AddDays(((int)DayOfWeek.Sunday - (int)start.DayOfWeek + 7) % 7);
                if (firstSunday > endInclusiveSunday.Date) return 0;

                var days = (endInclusiveSunday.Date - firstSunday.Date).Days;
                return (days / 7) + 1;
            }

            var weeksCount = CalcWeeksCount(yearStart, sunday);
            if (weeksCount <= 0) weeksCount = 1;

            var sql = @"
SELECT
    ci.ClassYear,
    ci.ClassBan,
    ci.visibleOrder,
    ISNULL(cm.ClassName, CONCAT(ci.ClassYear, N'학년 ', ci.ClassBan, N'반')) AS ClassName,

    TeacherName =
        CASE
            WHEN ISNULL(ctm.SubTeacherPersonId, 0) > 0
                 AND NULLIF(LTRIM(RTRIM(ISNULL(stpm.FullName, N''))), N'') IS NOT NULL
            THEN
                CONCAT(
                    NULLIF(LTRIM(RTRIM(ISNULL(tpm.FullName, N''))), N''),
                    N' / ',
                    NULLIF(LTRIM(RTRIM(ISNULL(stpm.FullName, N''))), N'')
                )
            ELSE
                ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(tpm.FullName, N''))), N''), N'')
        END,

    ci.PersonId AS StudentPersonId,
    ISNULL(spm.FullName, N'') AS StudentName,

    CASE WHEN ac.PersonId IS NULL THEN 0 ELSE 1 END AS IsPresent,

    ISNULL(yc.YearPresentCount, 0) AS YearPresentCount,

    CASE WHEN ncInfo.NewComerId IS NULL THEN 0 ELSE 1 END AS IsNewComer,
    ISNULL(ncInfo.NewComerVisitCount, 0) AS NewComerVisitCount

FROM ClassInfo_2026 ci
LEFT JOIN ClassMaster cm
    ON cm.ClassYear = ci.ClassYear
   AND cm.ClassBan  = ci.ClassBan
   AND cm.IsActive  = 1

LEFT JOIN (
    SELECT ClassYear, ClassBan, TeacherPersonId, SubTeacherPersonId
    FROM ClassTeacherMap_2026
    WHERE IsActive = 1
      AND Id IN (
          SELECT MAX(Id)
          FROM ClassTeacherMap_2026
          WHERE IsActive = 1
          GROUP BY ClassYear, ClassBan
      )
) ctm
    ON ctm.ClassYear = ci.ClassYear
   AND ctm.ClassBan  = ci.ClassBan

LEFT JOIN PersonMaster_2026 tpm
    ON tpm.PersonId = ctm.TeacherPersonId

LEFT JOIN PersonMaster_2026 stpm
    ON stpm.PersonId = NULLIF(ctm.SubTeacherPersonId, 0)

LEFT JOIN PersonMaster_2026 spm
    ON spm.PersonId = ci.PersonId

LEFT JOIN AttendanceCheck_2026 ac
    ON ac.PersonId   = ci.PersonId
   AND ac.AttendDate = @SundayDate
   AND ac.IsPresent  = 1

OUTER APPLY
(
    SELECT COUNT(1) AS YearPresentCount
    FROM AttendanceCheck_2026 a
    WHERE a.PersonId = ci.PersonId
      AND a.IsPresent = 1
      AND a.AttendDate >= @YearStart
      AND a.AttendDate <= @SundayDate
) yc

OUTER APPLY
(
    SELECT TOP 1
        nc.Id AS NewComerId,
        (
            SELECT COUNT(1)
            FROM AttendanceNewComerVisit v
            WHERE v.NewComerId = nc.Id
        ) AS NewComerVisitCount
    FROM AttendanceNewComer nc
    WHERE nc.PersonId = ci.PersonId
      AND ISNULL(nc.IsDeleted, 0) = 0
    ORDER BY nc.Id DESC
) ncInfo

ORDER BY ci.ClassYear, ci.ClassBan, ci.visibleOrder;
";

            var vm = new AttendanceSundayStudentsVM
            {
                SundayDate = sunday,
                WeeksCount = weeksCount
            };

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add(new SqlParameter("@SundayDate", SqlDbType.Date) { Value = sunday });
            cmd.Parameters.Add(new SqlParameter("@YearStart", SqlDbType.Date) { Value = yearStart });

            await conn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();

            var map = new Dictionary<string, AttendanceSundayClassCardVM>();

            while (await rd.ReadAsync())
            {
                var classYear = Convert.ToInt32(rd["ClassYear"]);
                var classBan = Convert.ToInt32(rd["ClassBan"]);
                var visibleOrder = Convert.ToInt32(rd["visibleOrder"]);

                var className = rd["ClassName"]?.ToString() ?? $"{classYear}학년 {classBan}반";
                var teacherName = rd["TeacherName"]?.ToString() ?? "";

                var studentId = Convert.ToInt32(rd["StudentPersonId"]);
                var studentName = rd["StudentName"]?.ToString() ?? "";
                var isPresent = Convert.ToInt32(rd["IsPresent"]) == 1;

                var yearPresentCount = 0;
                try { yearPresentCount = Convert.ToInt32(rd["YearPresentCount"]); } catch { yearPresentCount = 0; }

                if (yearPresentCount < 0) yearPresentCount = 0;
                if (yearPresentCount > weeksCount) yearPresentCount = weeksCount;

                var isNewComer = false;
                try { isNewComer = Convert.ToInt32(rd["IsNewComer"]) == 1; } catch { isNewComer = false; }

                var newComerVisitCount = 0;
                try { newComerVisitCount = Convert.ToInt32(rd["NewComerVisitCount"]); } catch { newComerVisitCount = 0; }

                if (newComerVisitCount < 0) newComerVisitCount = 0;

                var key = $"{classYear}_{classBan}";
                if (!map.TryGetValue(key, out var card))
                {
                    card = new AttendanceSundayClassCardVM
                    {
                        ClassYear = classYear,
                        ClassBan = classBan,
                        ClassName = className,
                        TeacherName = teacherName
                    };
                    map[key] = card;
                    vm.Classes.Add(card);
                }

                card.Students.Add(new AttendanceSundayStudentRowVM
                {
                    PersonId = studentId,
                    VisibleOrder = visibleOrder,
                    StudentName = string.IsNullOrWhiteSpace(studentName) ? $"#{studentId}" : studentName,
                    IsPresent = isPresent,
                    YearPresentCount = yearPresentCount,
                    IsNewComer = isNewComer,
                    NewComerVisitCount = newComerVisitCount
                });
            }

            return vm;
        }


        // ── 장기 결석자 목록 ─────────────────────────────────────────────────
        public async Task<LongAbsentListVM> GetLongAbsentStudentsAsync(IWebHostEnvironment env, int absentDays = 90)
        {
            if (absentDays < 1) absentDays = 90;

            var today      = DateHelper.GetKoreaToday().Date;
            var lastSunday = today.AddDays(-(int)today.DayOfWeek).Date;
            var yearStart  = new DateTime(lastSunday.Year, 1, 1).Date;
            var cutoffDate = today.AddDays(-absentDays).Date;    // 이 날짜 이전이면 장기 결석

            // 총 주일 수 계산
            var firstSunday = yearStart.AddDays(((int)DayOfWeek.Sunday - (int)yearStart.DayOfWeek + 7) % 7);
            int weeksCount  = firstSunday > lastSunday ? 0
                              : ((lastSunday - firstSunday).Days / 7) + 1;
            if (weeksCount < 1) weeksCount = 1;

            // 1) 전체 학생 목록
            var manage = new ManageHumanResource();
            var users  = manage.getUserInfos(env,
                string.Empty, string.Empty, 0, 0, 0,
                string.Empty, string.Empty,
                RankQueryOption.StudentOnly);

            if (users == null || users.Count == 0)
                return new LongAbsentListVM { AbsentDays = absentDays, WeeksCount = weeksCount };

            var personIds = users
                .Where(u => u.PersonId > 0)
                .Select(u => u.PersonId)
                .Distinct()
                .ToList();

            // 2) 올해 출석 횟수 + 마지막 출석일 (한 번에 조회)
            var attendMap   = new Dictionary<int, int>();          // PersonId → 출석 횟수
            var lastDateMap = new Dictionary<int, DateTime>();     // PersonId → 마지막 출석일
            var memoMap     = new Dictionary<int, string>();       // PersonId → 메모

            if (personIds.Count > 0)
            {
                var paramNames = personIds.Select((_, i) => "@P" + i).ToArray();

                var sqlAttend = $@"
SELECT PersonId,
       COUNT(*)        AS AttendCount,
       MAX(AttendDate) AS LastDate
FROM dbo.AttendanceCheck_2026 WITH (NOLOCK)
WHERE IsPresent = 1
  AND PersonId IN ({string.Join(",", paramNames)})
GROUP BY PersonId;";

                using var conn1 = new SqlConnection(_connectionString);
                await conn1.OpenAsync();
                using var cmd1 = new SqlCommand(sqlAttend, conn1);
                for (int i = 0; i < personIds.Count; i++)
                    cmd1.Parameters.Add(new SqlParameter(paramNames[i], SqlDbType.Int) { Value = personIds[i] });

                using var r1 = await cmd1.ExecuteReaderAsync();
                while (await r1.ReadAsync())
                {
                    if (r1["PersonId"] == DBNull.Value) continue;
                    int pid = Convert.ToInt32(r1["PersonId"]);
                    attendMap[pid]  = r1["AttendCount"] == DBNull.Value ? 0 : Convert.ToInt32(r1["AttendCount"]);
                    lastDateMap[pid] = r1["LastDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r1["LastDate"]);
                }

                // 3) 메모 조회 (BirthdayGiftPickMeMo)
                var sqlMemo = $@"
SELECT PersonID, Memo
FROM dbo.BirthdayGiftPickMeMo WITH (NOLOCK)
WHERE PersonID IN ({string.Join(",", paramNames)})
  AND Memo IS NOT NULL AND Memo <> '';";

                using var conn2 = new SqlConnection(_connectionString);
                await conn2.OpenAsync();
                using var cmd2 = new SqlCommand(sqlMemo, conn2);
                for (int i = 0; i < personIds.Count; i++)
                    cmd2.Parameters.Add(new SqlParameter(paramNames[i], SqlDbType.Int) { Value = personIds[i] });

                using var r2 = await cmd2.ExecuteReaderAsync();
                while (await r2.ReadAsync())
                {
                    if (r2["PersonID"] == DBNull.Value) continue;
                    int pid  = Convert.ToInt32(r2["PersonID"]);
                    var memo = r2["Memo"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(memo)) memoMap[pid] = memo;
                }
            }

            // 4) 필터 & 매핑 (90일 이상 미출석)
            var result = new List<LongAbsentStudentVM>();
            foreach (var u in users)
            {
                if (u.PersonId <= 0) continue;

                attendMap.TryGetValue(u.PersonId, out int cnt);
                lastDateMap.TryGetValue(u.PersonId, out DateTime lastDate);
                memoMap.TryGetValue(u.PersonId, out string? memo);

                bool hasAttend   = lastDate != DateTime.MinValue;
                var  lastDateVal = hasAttend ? (DateTime?)lastDate : null;
                int  daysAbsent  = hasAttend ? (today - lastDate.Date).Days : int.MaxValue;

                // 장기 결석 기준: 마지막 출석일이 cutoff보다 오래됐거나, 아예 출석 기록 없음
                if (hasAttend && lastDate.Date > cutoffDate) continue;

                int rate = weeksCount > 0 ? (int)Math.Round(cnt * 100.0 / weeksCount) : 0;
                if (rate > 100) rate = 100;

                result.Add(new LongAbsentStudentVM
                {
                    User          = u,
                    AttendCount   = cnt,
                    WeeksCount    = weeksCount,
                    AttendRate    = rate,
                    LastAttendDate = lastDateVal,
                    DaysAbsent    = daysAbsent == int.MaxValue ? -1 : daysAbsent,
                    Memo          = memo
                });
            }

            // 5) 정렬: 출석률 낮은 순 → 결석 일수 긴 순
            result = result
                .OrderBy(s => s.AttendRate)
                .ThenByDescending(s => s.DaysAbsent)
                .ThenBy(s => s.User.ClassYear)
                .ThenBy(s => s.User.ClassBan)
                .ThenBy(s => s.User.FullName)
                .ToList();

            return new LongAbsentListVM
            {
                AbsentDays = absentDays,
                WeeksCount = weeksCount,
                Students   = result
            };
        }

        // ── PersonId별 마지막 출석일 ──────────────────────────────────────────
        public async Task<Dictionary<int, DateTime>> GetLastAttendDateMapAsync(IEnumerable<int> personIds)
        {
            var map = new Dictionary<int, DateTime>();

            var ids = personIds?.Distinct().ToList();
            if (ids == null || ids.Count == 0) return map;

            var paramNames = ids.Select((_, i) => "@P" + i).ToArray();

            var sql = $@"
SELECT PersonId, MAX(AttendDate) AS LastDate
FROM dbo.AttendanceCheck_2026 WITH (NOLOCK)
WHERE IsPresent = 1
  AND PersonId IN ({string.Join(",", paramNames)})
GROUP BY PersonId;";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            for (int i = 0; i < ids.Count; i++)
                cmd.Parameters.Add(new SqlParameter(paramNames[i], SqlDbType.Int) { Value = ids[i] });

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                if (r["PersonId"] == DBNull.Value || r["LastDate"] == DBNull.Value) continue;
                map[Convert.ToInt32(r["PersonId"])] = Convert.ToDateTime(r["LastDate"]);
            }

            return map;
        }
    }
}

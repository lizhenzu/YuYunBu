using BulletinBoard.Helpers;
using BulletinBoard.Services.Abstract;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace BulletinBoard.Services
{
    public class DevotionPointsService : IDevotionPointsService
    {
        private readonly string _connStr;

        public DevotionPointsService(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<DevotionTopVM>> GetParticipantRankAllAsync(string ym, int classYear, int classBan)
        {
            ym = (ym ?? "").Trim();
            if (string.IsNullOrWhiteSpace(ym))
                return new List<DevotionTopVM>();

            // ✅ ym 형식 안전장치 (YYYY-MM)
            if (ym.Length != 7 || ym[4] != '-')
                throw new ArgumentException("Invalid ym format (YYYY-MM).", nameof(ym));

            const string sql = @"
DECLARE @Ym CHAR(7) = @YmParam;
DECLARE @ClassYear INT = @ClassYearParam;
DECLARE @ClassBan  INT = @ClassBanParam;

SELECT
    s.ClassYear,
    s.ClassBan,
    s.FullName_A,
    ISNULL(p.Points, 0) AS Points
FROM dbo.ClassInfo_2026 s WITH (NOLOCK)
LEFT JOIN dbo.Point_DevotionMonthly p WITH (NOLOCK)
  ON p.PersonId = s.PersonID
 AND p.Ym = @Ym
WHERE (@ClassYear = 0 OR s.ClassYear = @ClassYear)
  AND (@ClassBan  = 0 OR s.ClassBan  = @ClassBan)
ORDER BY
    ISNULL(p.Points, 0) DESC,
    s.FullName_A ASC;";

            var list = new List<DevotionTopVM>();

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@YmParam", ym);
            cmd.Parameters.AddWithValue("@ClassYearParam", classYear);
            cmd.Parameters.AddWithValue("@ClassBanParam", classBan);

            await conn.OpenAsync();
             await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                list.Add(new DevotionTopVM
                {
                    ClassYear = Convert.ToInt32(rd["ClassYear"]),
                    ClassBan = Convert.ToInt32(rd["ClassBan"]),
                    FullName = Convert.ToString(rd["FullName_A"]) ?? "",
                    Points = Convert.ToInt32(rd["Points"])
                });
            }

            return list;
        }

        public async Task<List<ClassOptionVM>> LoadAllClassesAsync()
        {
            const string sql = @"
SELECT ClassYear, ClassBan,
       ISNULL(ClassName, CONCAT(ClassYear, N'학년 ', ClassBan, N'반')) AS ClassName
FROM dbo.ClassMaster WITH (NOLOCK)
WHERE IsActive = 1
ORDER BY ClassYear, ClassBan;";

            var list = new List<ClassOptionVM>();
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
 await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new ClassOptionVM
                {
                    ClassYear = rd.GetInt32(0),
                    ClassBan = rd.GetInt32(1),
                    ClassName = rd.GetString(2)
                });
            }
            return list;
        }

        public async Task<MyClassVM?> TryGetMyClassAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            const string sql = @"
;WITH Me AS (
    SELECT TOP 1 PersonId
    FROM dbo.view_UserCompositeInfo_2026 WITH (NOLOCK)
    WHERE UserId = @UserId
)
SELECT TOP 1 ClassYear, ClassBan
FROM dbo.ClassTeacherMap_2026 WITH (NOLOCK)
WHERE IsActive = 1
  AND (
      TeacherPersonId = (SELECT PersonId FROM Me)
      OR SubTeacherPersonId = (SELECT PersonId FROM Me)
  )
ORDER BY ClassYear, ClassBan;";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
             await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return null;

            return new MyClassVM
            {
                ClassYear = rd.GetInt32(0),
                ClassBan = rd.GetInt32(1)
            };
        }
        public async Task<int> GetMyTotalPointsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return 0;

            const string sql = @"
;WITH Me AS (
    SELECT TOP 1 PersonId
    FROM dbo.view_UserCompositeInfo_2026 WITH (NOLOCK)
    WHERE UserId = @UserId
)
SELECT ISNULL(SUM(dm.Points), 0) AS TotalPoints
FROM dbo.Point_DevotionMonthly dm WITH (NOLOCK)
WHERE dm.PersonId = (SELECT PersonId FROM Me);";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            var obj = await cmd.ExecuteScalarAsync();
            return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
        }

        public async Task<List<DevotionMyMonthlyPointVM>> GetMyMonthlyPointsAsync(string userId, int months)
        {
            if (string.IsNullOrWhiteSpace(userId)) return new List<DevotionMyMonthlyPointVM>();
            if (months <= 0) months = 12;
            if (months > 60) months = 60; // ✅ 안전 제한

            const string sql = @"
;WITH Me AS (
    SELECT TOP 1 PersonId
    FROM dbo.view_UserCompositeInfo_2026 WITH (NOLOCK)
    WHERE UserId = @UserId
)
SELECT TOP (@TopN)
    dm.Ym,
    dm.Points
FROM dbo.Point_DevotionMonthly dm WITH (NOLOCK)
WHERE dm.PersonId = (SELECT PersonId FROM Me)
ORDER BY dm.Ym DESC;";

            var list = new List<DevotionMyMonthlyPointVM>();

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@TopN", months);

            await conn.OpenAsync();
             await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                list.Add(new DevotionMyMonthlyPointVM
                {
                    Ym = Convert.ToString(rd["Ym"]) ?? "",
                    Points = Convert.ToInt32(rd["Points"])
                });
            }

            return list;
        }

        public async Task<List<DevotionRowVM>> LoadGridRowsAsync(int classYear, int classBan, string ym)
        {
            const string sql = @"
DECLARE @ClassYear INT = @ClassYearParam, @ClassBan INT = @ClassBanParam;
DECLARE @Ym CHAR(7) = @YmParam;

SELECT
  ci.PersonID,
  ci.FullName_A,
  ISNULL(dm.Points, 0) AS Points
FROM dbo.ClassInfo_2026 ci WITH (NOLOCK)
LEFT JOIN dbo.Point_DevotionMonthly dm WITH (NOLOCK)
  ON dm.PersonId = ci.PersonID AND dm.Ym = @Ym
WHERE ci.ClassYear = @ClassYear
  AND ci.ClassBan  = @ClassBan
ORDER BY ISNULL(ci.visibleOrder,100), ci.FullName_A;";

            var list = new List<DevotionRowVM>();
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ClassYearParam", classYear);
            cmd.Parameters.AddWithValue("@ClassBanParam", classBan);
            cmd.Parameters.AddWithValue("@YmParam", ym);

            await conn.OpenAsync();
             await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new DevotionRowVM
                {
                    PersonId = Convert.ToInt32(rd["PersonID"]),
                    FullName = Convert.ToString(rd["FullName_A"]) ?? "",
                    Points = Convert.ToInt32(rd["Points"])
                });
            }
            return list;
        }

        public async Task<int> AdjustMonthlyTotalAsync(string ym, int personId, int delta)
        {
            const string sql = @"
DECLARE @Ym CHAR(7) = @YmParam;
DECLARE @PersonId INT = @PersonIdParam;
DECLARE @Delta INT = @DeltaParam;

MERGE dbo.Point_DevotionMonthly AS T
USING (SELECT @Ym AS Ym, @PersonId AS PersonId) AS S
ON (T.Ym = S.Ym AND T.PersonId = S.PersonId)
WHEN MATCHED THEN
  UPDATE SET T.Points = CASE WHEN T.Points + @Delta < 0 THEN 0 ELSE T.Points + @Delta END,
             T.UpdateDT = GETDATE()
WHEN NOT MATCHED THEN
  INSERT (Ym, PersonId, Points, InsertDT, UpdateDT)
  VALUES (@Ym, @PersonId, CASE WHEN @Delta < 0 THEN 0 ELSE @Delta END, GETDATE(), GETDATE())
OUTPUT inserted.Points;";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@YmParam", ym);
            cmd.Parameters.AddWithValue("@PersonIdParam", personId);
            cmd.Parameters.AddWithValue("@DeltaParam", delta);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<DevotionDashboardSummaryVM> GetDashboardSummaryAsync(string ym, int classYear, int classBan)
        {
            var vm = new DevotionDashboardSummaryVM();

            // ✅ KPI + Top10을 위해 Scores를 임시테이블로 생성 (#Scores)
            const string sql = @"
DECLARE @Ym CHAR(7) = @YmParam;
DECLARE @ClassYear INT = @ClassYearParam;
DECLARE @ClassBan  INT = @ClassBanParam;

IF OBJECT_ID('tempdb..#Scores') IS NOT NULL DROP TABLE #Scores;

SELECT
    s.PersonID,
    s.FullName_A,
    s.ClassYear,
    s.ClassBan,
    ISNULL(p.Points, 0) AS Points
INTO #Scores
FROM (
    SELECT PersonID, FullName_A, ClassYear, ClassBan
    FROM dbo.ClassInfo_2026 WITH (NOLOCK)
    WHERE (@ClassYear = 0 OR ClassYear = @ClassYear)
      AND (@ClassBan  = 0 OR ClassBan  = @ClassBan)
) s
LEFT JOIN dbo.Point_DevotionMonthly p WITH (NOLOCK)
  ON p.PersonId = s.PersonID AND p.Ym = @Ym;

-- Resultset #1: KPI
SELECT
    COUNT(*) AS StudentCount,
    ISNULL(SUM(Points),0) AS TotalPoints,
    ISNULL(AVG(CAST(Points AS FLOAT)),0) AS AvgPoints,
    ISNULL(MAX(Points),0) AS MaxPoints
FROM #Scores;

-- Resultset #2: Top 10
SELECT TOP 10
    ClassYear, ClassBan, FullName_A, Points
FROM #Scores
ORDER BY Points DESC, FullName_A ASC;";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@YmParam", ym);
                cmd.Parameters.AddWithValue("@ClassYearParam", classYear);
                cmd.Parameters.AddWithValue("@ClassBanParam", classBan);

                await conn.OpenAsync();
                 await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
                using var rd = await cmd.ExecuteReaderAsync();

                // Resultset #1: KPI
                if (await rd.ReadAsync())
                {
                    vm.StudentCount = Convert.ToInt32(rd["StudentCount"]);
                    vm.TotalPoints = Convert.ToInt32(rd["TotalPoints"]);
                    vm.AvgPoints = Convert.ToDouble(rd["AvgPoints"]);
                    vm.MaxPoints = Convert.ToInt32(rd["MaxPoints"]);
                }

                // Resultset #2: Top10
                if (await rd.NextResultAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        vm.Top.Add(new DevotionTopVM
                        {
                            ClassYear = Convert.ToInt32(rd["ClassYear"]),
                            ClassBan = Convert.ToInt32(rd["ClassBan"]),
                            FullName = Convert.ToString(rd["FullName_A"]) ?? "",
                            Points = Convert.ToInt32(rd["Points"])
                        });
                    }
                }
            }

            // 반별 집계는 "전체 모드(classYear=0,classBan=0)"일 때만
            if (classYear == 0 && classBan == 0)
            {
                const string sqlClassAgg = @"
DECLARE @Ym CHAR(7) = @YmParam;

;WITH Students AS (
    SELECT PersonID, ClassYear, ClassBan
    FROM dbo.ClassInfo_2026 WITH (NOLOCK)
),
Scores AS (
    SELECT s.ClassYear, s.ClassBan, ISNULL(p.Points,0) AS Points
    FROM Students s
    LEFT JOIN dbo.Point_DevotionMonthly p WITH (NOLOCK)
      ON p.PersonId = s.PersonID AND p.Ym = @Ym
)
SELECT
    ClassYear, ClassBan,
    COUNT(*) AS StudentCount,
    ISNULL(SUM(Points),0) AS TotalPoints,
    ISNULL(AVG(CAST(Points AS FLOAT)),0) AS AvgPoints
FROM Scores
GROUP BY ClassYear, ClassBan
ORDER BY TotalPoints DESC, AvgPoints DESC;";

                using var conn2 = new SqlConnection(_connStr);
                using var cmd2 = new SqlCommand(sqlClassAgg, conn2);
                cmd2.Parameters.AddWithValue("@YmParam", ym);

                await conn2.OpenAsync();
                 await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd2)}");
                using var rd2 = await cmd2.ExecuteReaderAsync();
                while (await rd2.ReadAsync())
                {
                    vm.ClassAgg.Add(new DevotionClassAggVM
                    {
                        ClassYear = Convert.ToInt32(rd2["ClassYear"]),
                        ClassBan = Convert.ToInt32(rd2["ClassBan"]),
                        StudentCount = Convert.ToInt32(rd2["StudentCount"]),
                        TotalPoints = Convert.ToInt32(rd2["TotalPoints"]),
                        AvgPoints = Convert.ToDouble(rd2["AvgPoints"])
                    });
                }
            }

            return vm;
        }

        public async Task<List<DevotionTrendPointVM>> GetDashboardTrendAsync(int months, int classYear, int classBan)
        {
            if (months < 3) months = 3;
            if (months > 24) months = 24;

            const string sql = @"
DECLARE @Months INT = @MonthsParam;
DECLARE @ClassYear INT = @ClassYearParam;
DECLARE @ClassBan  INT = @ClassBanParam;

;WITH Students AS (
    SELECT PersonID
    FROM dbo.ClassInfo_2026 WITH (NOLOCK)
    WHERE (@ClassYear = 0 OR ClassYear = @ClassYear)
      AND (@ClassBan  = 0 OR ClassBan  = @ClassBan)
),
MonthList AS (
    SELECT TOP (@Months)
        CONVERT(CHAR(7),
                DATEADD(MONTH, -(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1), GETDATE()),
                120) AS Ym
    FROM sys.objects
)
SELECT
    ml.Ym,
    ISNULL(SUM(ISNULL(p.Points, 0)), 0) AS TotalPoints
FROM MonthList ml
CROSS JOIN Students s
LEFT JOIN dbo.Point_DevotionMonthly p WITH (NOLOCK)
  ON p.Ym = ml.Ym AND p.PersonId = s.PersonID
GROUP BY ml.Ym
ORDER BY ml.Ym ASC;";

            var list = new List<DevotionTrendPointVM>();
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MonthsParam", months);
            cmd.Parameters.AddWithValue("@ClassYearParam", classYear);
            cmd.Parameters.AddWithValue("@ClassBanParam", classBan);

            await conn.OpenAsync();
             await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new DevotionTrendPointVM
                {
                    Ym = Convert.ToString(rd["Ym"]) ?? "",
                    TotalPoints = Convert.ToInt32(rd["TotalPoints"])
                });
            }
            return list;
        }









        public async Task<List<DevotionStudentListItemVM>> LoadMyClassStudentsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<DevotionStudentListItemVM>();

            const string sql = @"
;WITH Me AS (
    SELECT TOP 1 PersonId
    FROM dbo.view_UserCompositeInfo_2026 WITH (NOLOCK)
    WHERE UserId = @UserId
),
MyClass AS (
    SELECT TOP 1 ClassYear, ClassBan
    FROM dbo.ClassTeacherMap_2026 WITH (NOLOCK)
    WHERE IsActive = 1
      AND (
            TeacherPersonId = (SELECT PersonId FROM Me)
         OR SubTeacherPersonId = (SELECT PersonId FROM Me)
      )
    ORDER BY ClassYear, ClassBan
)
SELECT
    ci.PersonID,
    ci.FullName_A,
    ci.ClassYear,
    ci.ClassBan,
    ISNULL(SUM(dm.Points), 0) AS TotalPoints
FROM dbo.ClassInfo_2026 ci WITH (NOLOCK)
INNER JOIN MyClass mc
    ON mc.ClassYear = ci.ClassYear
   AND mc.ClassBan  = ci.ClassBan
LEFT JOIN dbo.Point_DevotionMonthly dm WITH (NOLOCK)
    ON dm.PersonId = ci.PersonID
GROUP BY
    ci.PersonID,
    ci.FullName_A,
    ci.ClassYear,
    ci.ClassBan,
    ci.visibleOrder
ORDER BY ISNULL(ci.visibleOrder, 100), ci.FullName_A;";

            var list = new List<DevotionStudentListItemVM>();

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                list.Add(new DevotionStudentListItemVM
                {
                    PersonId = Convert.ToInt32(rd["PersonID"]),
                    FullName = Convert.ToString(rd["FullName_A"]) ?? "",
                    ClassYear = Convert.ToInt32(rd["ClassYear"]),
                    ClassBan = Convert.ToInt32(rd["ClassBan"]),
                    TotalPoints = Convert.ToInt32(rd["TotalPoints"])
                });
            }

            return list;
        }

        public async Task<List<DevotionYearMonthPointVM>> GetStudentYearPointsAsync(int personId, int year)
        {
            const string sql = @"
DECLARE @PersonId INT = @PersonIdParam;
DECLARE @Year INT = @YearParam;

;WITH M AS (
    SELECT 1 AS MM UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
    UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8
    UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12
)
SELECT
    M.MM AS [Month],
    CONCAT(@Year, '-', RIGHT('0' + CAST(M.MM AS VARCHAR(2)), 2)) AS Ym,
    ISNULL(P.Points, 0) AS Points
FROM M
LEFT JOIN dbo.Point_DevotionMonthly P WITH (NOLOCK)
    ON P.PersonId = @PersonId
   AND P.Ym = CONCAT(@Year, '-', RIGHT('0' + CAST(M.MM AS VARCHAR(2)), 2))
ORDER BY M.MM;";

            var list = new List<DevotionYearMonthPointVM>();

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PersonIdParam", personId);
            cmd.Parameters.AddWithValue("@YearParam", year);

            await conn.OpenAsync();
            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                list.Add(new DevotionYearMonthPointVM
                {
                    Month = Convert.ToInt32(rd["Month"]),
                    Ym = Convert.ToString(rd["Ym"]) ?? "",
                    Points = Convert.ToInt32(rd["Points"])
                });
            }

            return list;
        }

        public async Task<int> SetMonthlyTotalAsync(string ym, int personId, int points)
        {
            if (points < 0) points = 0;

            const string sql = @"
DECLARE @Ym CHAR(7) = @YmParam;
DECLARE @PersonId INT = @PersonIdParam;
DECLARE @Points INT = @PointsParam;

MERGE dbo.Point_DevotionMonthly AS T
USING (SELECT @Ym AS Ym, @PersonId AS PersonId) AS S
ON (T.Ym = S.Ym AND T.PersonId = S.PersonId)
WHEN MATCHED THEN
    UPDATE SET T.Points = @Points,
               T.UpdateDT = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (Ym, PersonId, Points, InsertDT, UpdateDT)
    VALUES (@Ym, @PersonId, @Points, GETDATE(), GETDATE())
OUTPUT inserted.Points;";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@YmParam", ym);
            cmd.Parameters.AddWithValue("@PersonIdParam", personId);
            cmd.Parameters.AddWithValue("@PointsParam", points);

            await conn.OpenAsync();
            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }
}

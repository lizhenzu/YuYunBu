using BulletinBoard.ViewModels.Worldview;
using BulletinBoard.ViewModels.Worldview.Admin;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BulletinBoard.DataAccessLayer.Worldview
{
    public class WorldviewLessonRepository
    {
        private readonly string _connStr;

        public WorldviewLessonRepository(IConfiguration cfg)
        {
            _connStr = cfg.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' not found.");
        }



        public List<WorldviewSubmitRankUserRowVM> GetSubmitRankByUserByYear(int year, int? lessonNo = null)
        {
            // ✅ Draft 제외(BasePoint > 0) + ✅ SubmittedAt 기준 year만 집계
            const string sql = @"
SELECT
    UserId,
    [dbo].[GetFullName](MAX(UserName)) AS UserName,
    COUNT(1) AS SubmitCount,
    SUM(BasePoint) AS BaseSum,
    SUM(BonusPoint) AS BonusSum,
    SUM(TotalPoint) AS TotalSum,
    MAX(SubmittedAt) AS LastSubmittedAt
FROM dbo.WorldviewLessonSubmitHistory WITH (NOLOCK)
WHERE BasePoint > 0
  AND SubmittedAt >= @StartDt
  AND SubmittedAt <  @EndDt
  AND (@LessonNo IS NULL OR LessonNo = @LessonNo)
GROUP BY UserId
ORDER BY
    SUM(TotalPoint) DESC,
    MAX(SubmittedAt) ASC;
";

            var list = new List<WorldviewSubmitRankUserRowVM>();

            var start = new DateTime(year, 1, 1);
            var end = start.AddYears(1);

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@StartDt", SqlDbType.DateTime).Value = start;
            cmd.Parameters.Add("@EndDt", SqlDbType.DateTime).Value = end;
            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = (object?)lessonNo ?? DBNull.Value;

            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new WorldviewSubmitRankUserRowVM
                {
                    UserId = r["UserId"] == DBNull.Value ? null : r["UserId"].ToString(),
                    UserName = r["UserName"] == DBNull.Value ? null : r["UserName"].ToString(),
                    SubmitCount = Convert.ToInt32(r["SubmitCount"]),
                    BaseSum = Convert.ToInt32(r["BaseSum"]),
                    BonusSum = Convert.ToInt32(r["BonusSum"]),
                    TotalSum = Convert.ToInt32(r["TotalSum"]),
                    LastSubmittedAt = Convert.ToDateTime(r["LastSubmittedAt"])
                });
            }
            return list;
        }





        public List<WorldviewSubmitRankRowVM> GetSubmitRankRows(int? lessonNo = null)
    {
        const string sql = @"
SELECT
    LessonNo,
    [dbo].[GetFullName](UserName) UserName,
    IsAllCorrect,
    BasePoint,
    BonusPoint,
    TotalPoint,
    SubmittedAt,
    CheckedAt
FROM dbo.WorldviewLessonSubmitHistory WITH (NOLOCK)
WHERE (@LessonNo IS NULL OR LessonNo = @LessonNo)

ORDER BY
    SubmittedAt  asc,         -- ✅ 점수 높은 사람이 위
    TotalPoint ASC;         -- ✅ 동점이면 제출시간 빠른 사람이 위
";

        var list = new List<WorldviewSubmitRankRowVM>();

        using var conn = new SqlConnection(_connStr);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = (object?)lessonNo ?? DBNull.Value;

        conn.Open();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new WorldviewSubmitRankRowVM
            {
                LessonNo = Convert.ToInt32(r["LessonNo"]),
                UserName = r["UserName"] == DBNull.Value ? null : r["UserName"].ToString(),
                IsAllCorrect = Convert.ToBoolean(r["IsAllCorrect"]),
                BasePoint = Convert.ToInt32(r["BasePoint"]),
                BonusPoint = Convert.ToInt32(r["BonusPoint"]),
                TotalPoint = Convert.ToInt32(r["TotalPoint"]),
                SubmittedAt = Convert.ToDateTime(r["SubmittedAt"]),
                CheckedAt = r["CheckedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["CheckedAt"])
            });
        }

        return list;
    }


    // ✅ KST 현재시각(시/분/초 포함)
    private static DateTime GetKoreaNow()
        {
            var utcNow = DateTime.UtcNow;
            var kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, kst);
        }

        // 정답으로 완료한 경우만 true (틀린 제출은 재도전 허용)
        public bool ExistsSubmit(int lessonNo, string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return true;

            const string sql = @"
SELECT 1
FROM dbo.WorldviewLessonSubmitHistory
WHERE LessonNo=@LessonNo AND UserId=@UserId
  AND BasePoint > 0
  AND IsAllCorrect = 1;";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = lessonNo;
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = userId;

            conn.Open();
            return cmd.ExecuteScalar() != null;
        }

        // 이전에 오답으로 제출한 레코드가 있는지 확인
        public bool ExistsWrongSubmit(int lessonNo, string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return false;

            const string sql = @"
SELECT 1
FROM dbo.WorldviewLessonSubmitHistory
WHERE LessonNo=@LessonNo AND UserId=@UserId
  AND BasePoint > 0
  AND IsAllCorrect = 0;";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = lessonNo;
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = userId;

            conn.Open();
            return cmd.ExecuteScalar() != null;
        }

        // 오답 제출 → 정답으로 업그레이드 (보너스 포인트 추가)
        public bool UpgradeToCorrect(int lessonNo, string userId, string? userName, int bonusPoint)
        {
            var now = GetKoreaNow();

            const string sql = @"
UPDATE TOP(1) dbo.WorldviewLessonSubmitHistory
SET IsAllCorrect   = 1,
    BonusPoint     = @BonusPoint,
    TotalPoint     = BasePoint + @BonusPoint,
    UserName       = COALESCE(@UserName, UserName),
    CheckedAt      = @Now,
    FinalSubmittedAt = @Now,
    SubmittedAt    = @Now
WHERE LessonNo = @LessonNo
  AND UserId   = @UserId
  AND BasePoint > 0
  AND IsAllCorrect = 0;
SELECT @@ROWCOUNT;";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@LessonNo",   SqlDbType.Int).Value = lessonNo;
            cmd.Parameters.Add("@UserId",     SqlDbType.NVarChar, 100).Value = userId;
            cmd.Parameters.Add("@UserName",   SqlDbType.NVarChar, 100).Value = (object?)userName ?? DBNull.Value;
            cmd.Parameters.Add("@BonusPoint", SqlDbType.Int).Value = bonusPoint;
            cmd.Parameters.Add("@Now",        SqlDbType.DateTime2).Value = now;

            conn.Open();
            var result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }

        public int UpsertDraftSubmitBonus(int lessonNo, string userId, string? userName, bool isAllCorrect, int bonusPoint)
        {
            var now = GetKoreaNow();

            const string sql = @"
DECLARE @Id int;

;WITH cte AS (
    SELECT TOP(1) SubmitId
    FROM dbo.WorldviewLessonSubmitHistory WITH (UPDLOCK, ROWLOCK)
    WHERE LessonNo=@LessonNo AND UserId=@UserId
      AND BasePoint=0 AND TotalPoint=0
    ORDER BY SubmitId DESC
)
UPDATE t
SET UserName=@UserName,
    IsAllCorrect=@IsAllCorrect,
    BonusPoint=@BonusPoint,
    CheckedAt=@Now,
    SubmittedAt=@Now
FROM dbo.WorldviewLessonSubmitHistory t
JOIN cte ON t.SubmitId = cte.SubmitId;

IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO dbo.WorldviewLessonSubmitHistory
    (LessonNo, UserId, UserName, IsAllCorrect, BasePoint, BonusPoint, TotalPoint, CheckedAt, SubmittedAt)
    VALUES
    (@LessonNo, @UserId, @UserName, @IsAllCorrect, 0, @BonusPoint, 0, @Now, @Now);

    SET @Id = CAST(SCOPE_IDENTITY() AS int);
END
ELSE
BEGIN
    SELECT TOP(1) @Id = SubmitId
    FROM dbo.WorldviewLessonSubmitHistory WITH (NOLOCK)
    WHERE LessonNo=@LessonNo AND UserId=@UserId
      AND BasePoint=0 AND TotalPoint=0
    ORDER BY SubmitId DESC;
END

SELECT @Id;
";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = lessonNo;
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = userId;
            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = (object?)userName ?? DBNull.Value;
            cmd.Parameters.Add("@IsAllCorrect", SqlDbType.Bit).Value = isAllCorrect;
            cmd.Parameters.Add("@BonusPoint", SqlDbType.Int).Value = bonusPoint;
            cmd.Parameters.Add("@Now", SqlDbType.DateTime2).Value = now;

            conn.Open();
            var obj = cmd.ExecuteScalar();
            if (obj == null || obj == DBNull.Value)
                throw new InvalidOperationException("UpsertDraftSubmitBonus failed to return SubmitId.");
            return Convert.ToInt32(obj);
        }

        public (bool updated, bool isAllCorrect, int basePoint, int bonusPoint, int totalPoint) FinalizeSubmitFromDraft(
            int lessonNo, string userId, string? userName, int basePoint)
        {
            var now = GetKoreaNow();

            const string sql = @"
;WITH cte AS (
    SELECT TOP(1) SubmitId
    FROM dbo.WorldviewLessonSubmitHistory WITH (UPDLOCK, ROWLOCK)
    WHERE LessonNo=@LessonNo AND UserId=@UserId
      AND BasePoint=0 AND TotalPoint=0
    ORDER BY SubmitId DESC
)
UPDATE t
SET UserName=@UserName,
    BasePoint=@BasePoint,
    TotalPoint=@BasePoint + t.BonusPoint,
    FinalSubmittedAt=@Now,
    SubmittedAt=@Now
OUTPUT inserted.IsAllCorrect, inserted.BasePoint, inserted.BonusPoint, inserted.TotalPoint
FROM dbo.WorldviewLessonSubmitHistory t
JOIN cte ON t.SubmitId = cte.SubmitId;
";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = lessonNo;
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = userId;
            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = (object?)userName ?? DBNull.Value;
            cmd.Parameters.Add("@BasePoint", SqlDbType.Int).Value = basePoint;
            cmd.Parameters.Add("@Now", SqlDbType.DateTime2).Value = now;

            conn.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read())
                return (false, false, 0, 0, 0);

            bool isAllCorrect = r.GetBoolean(0);
            int bp = r.GetInt32(1);
            int bonus = r.GetInt32(2);
            int total = r.GetInt32(3);

            return (true, isAllCorrect, bp, bonus, total);
        }

        public void InsertSubmitHistory(int lessonNo, string? userId, string? userName, bool isAllCorrect, int basePoint, int bonusPoint, int totalPoint)
        {
            var now = GetKoreaNow();

            const string sql = @"
INSERT INTO dbo.WorldviewLessonSubmitHistory
(LessonNo, UserId, UserName, IsAllCorrect, BasePoint, BonusPoint, TotalPoint, CheckedAt, FinalSubmittedAt, SubmittedAt)
VALUES
(@LessonNo, @UserId, @UserName, @IsAllCorrect, @BasePoint, @BonusPoint, @TotalPoint, @Now, @Now, @Now);
";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = lessonNo;
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = (object?)userId ?? DBNull.Value;
            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = (object?)userName ?? DBNull.Value;
            cmd.Parameters.Add("@IsAllCorrect", SqlDbType.Bit).Value = isAllCorrect;
            cmd.Parameters.Add("@BasePoint", SqlDbType.Int).Value = basePoint;
            cmd.Parameters.Add("@BonusPoint", SqlDbType.Int).Value = bonusPoint;
            cmd.Parameters.Add("@TotalPoint", SqlDbType.Int).Value = totalPoint;
            cmd.Parameters.Add("@Now", SqlDbType.DateTime2).Value = now;

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public Dictionary<int, LessonDashStatusVM> GetDashboardStatusMap(string userId)
        {
            var map = new Dictionary<int, LessonDashStatusVM>();

            const string sql = @"
SELECT LessonNo, MAX(TotalPoint) AS Score
FROM dbo.WorldviewLessonSubmitHistory WITH (NOLOCK)
WHERE UserId = @UserId
  AND BasePoint > 0
GROUP BY LessonNo;
";

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = userId;

            conn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                int lessonNo = Convert.ToInt32(rd["LessonNo"]);
                int score = Convert.ToInt32(rd["Score"]);

                map[lessonNo] = new LessonDashStatusVM
                {
                    LessonNo = lessonNo,
                    Score = score,
                    SubmittedAt = null
                };
            }

            return map;
        }

        public Dictionary<string, string> GetContentMap(int lessonNo)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var conn = new SqlConnection(_connStr);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT SectionKey, ContentText
FROM dbo.WorldviewLessonContent WITH (NOLOCK)
WHERE LessonNo = @LessonNo AND IsActive = 1
ORDER BY SortNo ASC;";
            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = lessonNo;

            conn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                var key = rd["SectionKey"]?.ToString() ?? "";
                var val = rd["ContentText"]?.ToString() ?? "";

                if (!string.IsNullOrWhiteSpace(key))
                    map[key.Trim()] = val;
            }

            return map;
        }

        public string GetDefTextTemplate(int lessonNo)
        {
            using var conn = new SqlConnection(_connStr);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT TOP(1) ContentText
FROM dbo.WorldviewLessonContent WITH (NOLOCK)
WHERE LessonNo = @LessonNo
  AND IsActive = 1
  AND SectionKey = N'DefTextTemplate'
ORDER BY SortNo ASC;";
            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = lessonNo;

            conn.Open();
            var obj = cmd.ExecuteScalar();
            return obj?.ToString() ?? "";
        }

        public Dictionary<string, List<string>> GetBlankCharAnswerMap(
            int lessonNo,
            out Dictionary<string, Dictionary<int, string>> initialMap)
        {
            const string sql = @"
SELECT GroupKey, CharIndex, AnswerChar, InitialChar
FROM dbo.WorldviewLessonBlankChar WITH (NOLOCK)
WHERE LessonNo = @LessonNo AND IsActive = 1
ORDER BY GroupKey ASC, CharIndex ASC;";

            var answerMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            initialMap = new Dictionary<string, Dictionary<int, string>>(StringComparer.OrdinalIgnoreCase);

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = lessonNo;

            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var groupKey = r["GroupKey"]?.ToString() ?? "";
                var idx = Convert.ToInt32(r["CharIndex"]);

                var answer = (r["AnswerChar"]?.ToString() ?? "").Trim();
                var initial = r["InitialChar"] == DBNull.Value
                                ? ""
                                : (r["InitialChar"]?.ToString() ?? "").Trim();

                if (!answerMap.TryGetValue(groupKey, out var list))
                {
                    list = new List<string>();
                    answerMap[groupKey] = list;
                }
                list.Add(answer);

                if (!initialMap.TryGetValue(groupKey, out var idxMap))
                {
                    idxMap = new Dictionary<int, string>();
                    initialMap[groupKey] = idxMap;
                }
                idxMap[idx] = initial;
            }

            return answerMap;
        }

        public void InsertBlankSubmitChar(int lessonNo, string? userId, string submitJson, bool isCorrect)
        {
            var now = GetKoreaNow();

            using var conn = new SqlConnection(_connStr);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO dbo.WorldviewLessonBlankSubmitChar(LessonNo, UserId, SubmitJson, IsCorrect, InsertDT)
VALUES(@LessonNo, @UserId, @SubmitJson, @IsCorrect, @Now);";
            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = lessonNo;
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = (object?)userId ?? DBNull.Value;
            cmd.Parameters.Add("@SubmitJson", SqlDbType.NVarChar).Value = submitJson;
            cmd.Parameters.Add("@IsCorrect", SqlDbType.Bit).Value = isCorrect;
            cmd.Parameters.Add("@Now", SqlDbType.DateTime2).Value = now;

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public List<WorldviewSubmitRankUserRowVM> GetSubmitRankByUser(int? lessonNo = null)
        {
            // ✅ Draft(임시 저장) 제외:
            //   UpsertDraftSubmitBonus()에서 BasePoint=0, TotalPoint=0 인 행이 생길 수 있으니
            //   BasePoint > 0 인 "최종 제출"만 집계하도록 필터합니다.
            const string sql = @"
SELECT
    UserId,
    [dbo].[GetFullName](MAX(UserName)) AS UserName,
    COUNT(1) AS SubmitCount,
    SUM(BasePoint) AS BaseSum,
    SUM(BonusPoint) AS BonusSum,
    SUM(TotalPoint) AS TotalSum,
    MAX(SubmittedAt) AS LastSubmittedAt
FROM dbo.WorldviewLessonSubmitHistory WITH (NOLOCK)
WHERE BasePoint > 0
  AND (@LessonNo IS NULL OR LessonNo = @LessonNo)
GROUP BY UserId
ORDER BY
    SUM(TotalPoint) DESC,
    MAX(SubmittedAt) ASC;
";

            var list = new List<WorldviewSubmitRankUserRowVM>();

            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@LessonNo", SqlDbType.Int).Value = (object?)lessonNo ?? DBNull.Value;

            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new WorldviewSubmitRankUserRowVM
                {
                    UserId = r["UserId"] == DBNull.Value ? null : r["UserId"].ToString(),
                    UserName = r["UserName"] == DBNull.Value ? null : r["UserName"].ToString(),
                    SubmitCount = Convert.ToInt32(r["SubmitCount"]),
                    BaseSum = Convert.ToInt32(r["BaseSum"]),
                    BonusSum = Convert.ToInt32(r["BonusSum"]),
                    TotalSum = Convert.ToInt32(r["TotalSum"]),
                    LastSubmittedAt = r["LastSubmittedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["LastSubmittedAt"])
                });
            }

            return list;
        }


    }
}

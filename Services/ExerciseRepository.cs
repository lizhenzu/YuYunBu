using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient; // NuGet: Microsoft.Data.SqlClient
using Microsoft.Extensions.Configuration;
using BulletinBoard.Models;

namespace BulletinBoard.Services
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly string _conn;
        public ExerciseRepository(IConfiguration configuration)
        {
            _conn = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        /// <summary>
        /// 목록 조회(필터/페이징). 첫 결과셋에 TotalCount 포함 (CTE 범위 문제 방지)
        /// </summary>
        public async Task<(IEnumerable<ExerciseSummary> Items, int Total)> GetExercisesAsync(ExerciseFilter filter)
        {
            var items = new List<ExerciseSummary>();
            int total = 0;

            filter ??= new ExerciseFilter();
            int page = filter.Page <= 0 ? 1 : filter.Page;
            int take = filter.PageSize <= 0 ? 20 : filter.PageSize;
            int skip = (page - 1) * take;

            var where = new StringBuilder("WHERE 1=1 ");
            var prms = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                where.Append(" AND E.Name LIKE @Name ");
                prms.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = $"%{filter.Name}%" });
            }
            if (!string.IsNullOrWhiteSpace(filter.Difficulty))
            {
                where.Append(" AND E.Difficulty = @Difficulty ");
                prms.Add(new SqlParameter("@Difficulty", SqlDbType.NVarChar, 20) { Value = filter.Difficulty! });
            }
            if (filter.IsAnaerobic.HasValue)
            {
                where.Append(" AND E.IsAnaerobic = @IsAnaerobic ");
                prms.Add(new SqlParameter("@IsAnaerobic", SqlDbType.Bit) { Value = filter.IsAnaerobic.Value });
            }
            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                where.Append(@"
 AND EXISTS (
    SELECT 1
    FROM dbo.exercise_Category_Map CM
    JOIN dbo.exercise_Category C ON C.CategoryId = CM.CategoryId
    WHERE CM.ExerciseId = E.ExerciseId AND C.CategoryName = @Category
 )");
                prms.Add(new SqlParameter("@Category", SqlDbType.NVarChar, 50) { Value = filter.Category! });
            }

            var sql = $@"
;WITH base AS (
    SELECT E.ExerciseId, E.Name, E.NameEn, E.IsAnaerobic, E.Difficulty, E.Description
    FROM dbo.exercise_Exercise E
    {where}
),
paged AS (
    SELECT
        b.*,
        ROW_NUMBER() OVER (ORDER BY b.Name) AS RN,
        COUNT(*) OVER() AS TotalCount
    FROM base b
)
SELECT
    p.ExerciseId, p.Name, p.NameEn, p.IsAnaerobic, p.Difficulty, p.Description,
    P2.RepsMin, P2.RepsMax, P2.SetsMin, P2.SetsMax, P2.RestSecMin, P2.RestSecMax, P2.Tempo, P2.Notes AS PrescriptionNotes,
    p.TotalCount
FROM paged p
LEFT JOIN dbo.exercise_Prescription P2 ON P2.ExerciseId = p.ExerciseId
WHERE p.RN BETWEEN @Skip + 1 AND @Skip + @Take
ORDER BY p.RN;";

            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand(sql, con);
            foreach (var p in prms) cmd.Parameters.Add(p);
            cmd.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });

            await using var rdr = await cmd.ExecuteReaderAsync();

            bool first = true;
            while (await rdr.ReadAsync())
            {
                if (first)
                {
                    // SELECT 목록의 마지막 컬럼: TotalCount (0-based 14)
                    total = rdr.IsDBNull(14) ? 0 : rdr.GetInt32(14);
                    first = false;
                }

                items.Add(new ExerciseSummary
                {
                    ExerciseId = rdr.GetInt32(0),
                    Name = rdr.GetString(1),
                    NameEn = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                    IsAnaerobic = rdr.GetBoolean(3),
                    Difficulty = rdr.GetString(4),
                    Description = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                    RepsMin = rdr.IsDBNull(6) ? (int?)null : rdr.GetInt32(6),
                    RepsMax = rdr.IsDBNull(7) ? (int?)null : rdr.GetInt32(7),
                    SetsMin = rdr.IsDBNull(8) ? (int?)null : rdr.GetInt32(8),
                    SetsMax = rdr.IsDBNull(9) ? (int?)null : rdr.GetInt32(9),
                    RestSecMin = rdr.IsDBNull(10) ? (int?)null : rdr.GetInt32(10),
                    RestSecMax = rdr.IsDBNull(11) ? (int?)null : rdr.GetInt32(11),
                    Tempo = rdr.IsDBNull(12) ? null : rdr.GetString(12),
                    PrescriptionNotes = rdr.IsDBNull(13) ? null : rdr.GetString(13)
                });
            }

            return (items, total);
        }

        /// <summary>
        /// 상세: Basic / Steps / Muscles / Benefits / KidsInfo(어린이)
        /// </summary>
        public async Task<BulletinBoard.Models.ExerciseDetail?> GetExerciseDetailAsync(int id)
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            var sql = @"
SET NOCOUNT ON;

-- 1) Basic (exercise_Exercise + exercise_Prescription)
SELECT 
    E.ExerciseId,
    E.Name,
    E.NameEn,
    E.IsAnaerobic,
    E.Difficulty,
    E.Description,
    P.RepsMin,
    P.RepsMax,
    P.SetsMin,
    P.SetsMax,
    P.RestSecMin,
    P.RestSecMax,
    P.Tempo,
    P.Notes AS PrescriptionNotes
FROM dbo.exercise_Exercise E
LEFT JOIN dbo.exercise_Prescription P ON P.ExerciseId = E.ExerciseId
WHERE E.ExerciseId = @ExerciseId;

-- 2) Steps
SELECT 
    S.StepNo,
    S.Phase,
    S.Title,
    S.Instruction
FROM dbo.exercise_Step S
WHERE S.ExerciseId = @ExerciseId
ORDER BY S.StepNo;

-- 3) Muscles
SELECT
    M.MuscleName,
    M.Region,
    MM.Role,
    MM.SortOrder
FROM dbo.exercise_Muscle_Map MM
JOIN dbo.exercise_Muscle M ON M.MuscleId = MM.MuscleId
WHERE MM.ExerciseId = @ExerciseId
ORDER BY CASE WHEN MM.Role = N'주요' THEN 0 ELSE 1 END, ISNULL(MM.SortOrder, 999), M.MuscleName;

-- 4) Benefits
SELECT EF.EffectText
FROM dbo.exercise_Effect_Map EM
JOIN dbo.exercise_Effect EF ON EF.EffectId = EM.EffectId
WHERE EM.ExerciseId = @ExerciseId
ORDER BY EF.EffectText;

-- 5) KidsInfo (어린이만)
SELECT A.Audience, A.Explanation
FROM dbo.exercise_Audience A
WHERE A.ExerciseId = @ExerciseId AND A.Audience = N'어린이';
";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@ExerciseId", SqlDbType.Int) { Value = id });

            await using var rdr = await cmd.ExecuteReaderAsync();

            // ===== 1) Basic =====
            // 첫 ResultSet에서 한 행도 없으면 상세 없음
            if (!await rdr.ReadAsync())
                return null;

            var dto = new BulletinBoard.Models.ExerciseDetail
            {
                Basic = new List<BulletinBoard.Models.Exercise>(),
                Steps = new List<BulletinBoard.Models.ExerciseStep>(),
                Muscles = new List<BulletinBoard.Models.ExerciseMuscle>(),
                Benefits = new List<BulletinBoard.Models.ExerciseBenefit>(),
                KidsInfo = new List<BulletinBoard.Models.ExerciseKids>()
            };

            dto.Basic.Add(new BulletinBoard.Models.Exercise
            {
                ExerciseId = rdr.GetInt32(0),
                Name = rdr.GetString(1),
                NameEn = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                IsAnaerobic = rdr.GetBoolean(3),
                Difficulty = rdr.GetString(4),
                Description = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                RepsMin = rdr.IsDBNull(6) ? (int?)null : rdr.GetInt32(6),
                RepsMax = rdr.IsDBNull(7) ? (int?)null : rdr.GetInt32(7),
                SetsMin = rdr.IsDBNull(8) ? (int?)null : rdr.GetInt32(8),
                SetsMax = rdr.IsDBNull(9) ? (int?)null : rdr.GetInt32(9),
                RestSecMin = rdr.IsDBNull(10) ? (int?)null : rdr.GetInt32(10),
                RestSecMax = rdr.IsDBNull(11) ? (int?)null : rdr.GetInt32(11),
                Tempo = rdr.IsDBNull(12) ? null : rdr.GetString(12),
                PrescriptionNotes = rdr.IsDBNull(13) ? null : rdr.GetString(13)
            });

            // ===== 2) Steps =====
            if (await rdr.NextResultAsync())
            {
                while (await rdr.ReadAsync())
                {
                    dto.Steps.Add(new BulletinBoard.Models.ExerciseStep
                    {
                        StepNo = rdr.GetInt32(0),
                        Phase = rdr.GetString(1),
                        Title = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2),
                        Instruction = rdr.GetString(3)
                    });
                }
            }

            // ===== 3) Muscles =====
            if (await rdr.NextResultAsync())
            {
                while (await rdr.ReadAsync())
                {
                    dto.Muscles.Add(new BulletinBoard.Models.ExerciseMuscle
                    {
                        MuscleName = rdr.GetString(0),
                        Region = rdr.IsDBNull(1) ? null : rdr.GetString(1),
                        Role = rdr.GetString(2),
                        SortOrder = rdr.IsDBNull(3) ? (int?)null : rdr.GetInt32(3)
                    });
                }
            }

            // ===== 4) Benefits =====
            if (await rdr.NextResultAsync())
            {
                while (await rdr.ReadAsync())
                {
                    dto.Benefits.Add(new BulletinBoard.Models.ExerciseBenefit
                    {
                        EffectText = rdr.GetString(0)
                    });
                }
            }

            // ===== 5) KidsInfo =====
            if (await rdr.NextResultAsync())
            {
                while (await rdr.ReadAsync())
                {
                    dto.KidsInfo.Add(new BulletinBoard.Models.ExerciseKids
                    {
                        Audience = rdr.GetString(0),
                        Explanation = rdr.GetString(1)
                    });
                }
            }

            return dto;
        }

    }
}

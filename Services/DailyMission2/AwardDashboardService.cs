using BulletinBoard.Models;
using BulletinBoard.Models.DailyMission2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BulletinBoard.Services.DailyMission2
{
    
    public class AwardDashboardService : IAwardDashboardService
    {

        private readonly string _connectionString;


        public AwardDashboardService(IConfiguration configuration)
        { 
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<AwardPopupResult> HandleAwardIfReachedAsync(
      string userName,
      int cYear,
      int yearTotalPoint,
      int addedPoint
  )
        {
            var result = new AwardPopupResult { HasNewAwards = false };

            if (addedPoint <= 0) return result;
            if (string.IsNullOrWhiteSpace(_connectionString)) return result;

            // 기존 시스템 URL 유지
            string awardPickUrl = $"/DailyMission2Award/PickGifts?year={cYear}";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql = @"
;WITH Achieved AS (
    SELECT ab.Id AS AwardBaseId, ab.AwardLevel
    FROM dbo.AwardBase ab
    WHERE ab.AwardLevel <= @YearTotalPoint
),
Missing AS (
    SELECT a.AwardBaseId, a.AwardLevel
    FROM Achieved a
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.AwardUserClaim uc
        WHERE uc.UserName = @UserName
          AND uc.cYear = @cYear
          AND uc.AwardLevel = a.AwardLevel
    )
)
INSERT INTO dbo.AwardUserClaim
(
    UserName, cYear, AwardLevel, AwardBaseId,
    Status, SelectedProductID, SelectedDT, InsertDT, UpdateDT
)
OUTPUT inserted.AwardLevel
SELECT
    @UserName, @cYear, m.AwardLevel, m.AwardBaseId,
    'Eligible', NULL, NULL, GETDATE(), GETDATE()
FROM Missing m;
";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });
                cmd.Parameters.Add(new SqlParameter("@YearTotalPoint", SqlDbType.Int) { Value = yearTotalPoint });

                var newlyCreated = new List<int>();

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    newlyCreated.Add(Convert.ToInt32(r[0]));
                }

                if (newlyCreated.Count > 0)
                {
                    string levels = string.Join(", ", newlyCreated);

                    result.HasNewAwards = true;
                    result.LevelsText = levels;
                    result.AwardUrl = awardPickUrl;

                    result.Message =
                        $"🎉 축하해요!\n" +
                        $"시상 포인트({levels}점)에 도달했어요!\n" +
                        $"⭐ 올해 총 {yearTotalPoint}점!\n" +
                        $"🎁 지금 선물 선택 화면으로 갈까요?";
                }
            }
            catch
            {
                // 보조 기능이므로 실패해도 메인 저장이 깨지면 안 됨
            }

            return result;
        }
        public async Task<(bool hasPending, int awardLevel, int amountOfMoney)> GetNextPendingAwardAsync(string userName, int cYear, int? startLevel = null)
        {
            if (string.IsNullOrWhiteSpace(userName)) return (false, 0, 0);

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // startLevel이 있으면: 그 레벨이 미선택이면 우선 보여주고,
                // 아니면 기존 규칙대로 "가장 낮은 미선택 레벨"
                string sql = @"
;WITH Pending AS (
    SELECT uc.AwardLevel, ab.AmountOfMoney
    FROM dbo.AwardUserClaim uc
    LEFT JOIN dbo.AwardBase ab ON ab.AwardLevel = uc.AwardLevel
    WHERE uc.UserName = @UserName
      AND uc.cYear = @cYear
      AND (uc.Status = 'Eligible' OR uc.Status = 'Selected')
      AND (uc.SelectedProductID IS NULL OR uc.SelectedProductID = 0)
)
SELECT TOP 1 AwardLevel, ISNULL(AmountOfMoney, 0) AS AmountOfMoney
FROM Pending
ORDER BY
    CASE WHEN @StartLevel IS NOT NULL AND AwardLevel = @StartLevel THEN 0 ELSE 1 END,
    AwardLevel ASC;
";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });
                cmd.Parameters.Add(new SqlParameter("@StartLevel", SqlDbType.Int) { Value = (object?)startLevel ?? DBNull.Value });

                using var r = await cmd.ExecuteReaderAsync();
                if (!await r.ReadAsync()) return (false, 0, 0);

                int lvl = Convert.ToInt32(r["AwardLevel"]);
                int amt = Convert.ToInt32(r["AmountOfMoney"]);
                return (true, lvl, amt);
            }
            catch
            {
                return (false, 0, 0);
            }
        }

        public async Task<List<AwardItemDTO>> GetItemsForAwardLevelAsync(int awardLevel)
        {
            var list = new List<AwardItemDTO>();
            if (awardLevel <= 0) return list;

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // AwardBase.AmountOfMoney 조회
                int amount = 0;
                {
                    string sqlAmt = @"SELECT TOP 1 AmountOfMoney FROM dbo.AwardBase WHERE AwardLevel = @AwardLevel;";
                    using var cmdAmt = new SqlCommand(sqlAmt, conn);
                    cmdAmt.Parameters.Add(new SqlParameter("@AwardLevel", SqlDbType.Int) { Value = awardLevel });
                    var obj = await cmdAmt.ExecuteScalarAsync();
                    amount = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
                }

                // amount가 0이면(등록 안 됨) 상품 노출 불가
                if (amount <= 0) return list;

                // UnitPrice < AmountOfMoney (미만) + 이름순 정렬
                string sql = @"
SELECT ProductID, ProductName, ProductDescription, UnitPrice
FROM dbo.AwardItem
WHERE UnitPrice < @Amount  and [IsHidden]  = 0 
ORDER BY ProductName ASC, ProductID ASC;
";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Int) { Value = amount });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    list.Add(new AwardItemDTO
                    {
                        ProductID = Convert.ToInt32(r["ProductID"]),
                        ProductName = (r["ProductName"]?.ToString() ?? ""),
                        ProductDescription = (r["ProductDescription"] == DBNull.Value) ? null : r["ProductDescription"]?.ToString(),
                        UnitPrice = Convert.ToInt32(r["UnitPrice"])
                    });
                }

                return list;
            }
            catch
            {
                return list;
            }
        }

        public async Task<bool> SaveGiftSelectionAsync(string userName, int cYear, int awardLevel, int productId)
        {
            if (string.IsNullOrWhiteSpace(userName)) return false;
            if (cYear <= 0 || awardLevel <= 0 || productId <= 0) return false;

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // Delivered면 막고, Eligible/Selected + 미선택일 때만 업데이트
                // 또한 가격 필터(UnitPrice < AmountOfMoney) 조건도 DB에서 한번 더 검증(안전)
                string sql = @"
DECLARE @Amount INT = (SELECT TOP 1 AmountOfMoney FROM dbo.AwardBase WHERE AwardLevel = @AwardLevel);

IF (@Amount IS NULL OR @Amount <= 0)
BEGIN
    SELECT 0;
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM dbo.AwardItem WHERE ProductID = @ProductID AND UnitPrice < @Amount)
BEGIN
    SELECT 0;
    RETURN;
END

UPDATE uc
SET
    uc.SelectedProductID = @ProductID,
    uc.SelectedDT = GETDATE(),
    uc.Status = 'Selected',
    uc.UpdateDT = GETDATE()
FROM dbo.AwardUserClaim uc
WHERE uc.UserName = @UserName
  AND uc.cYear = @cYear
  AND uc.AwardLevel = @AwardLevel
  AND uc.Status <> 'Delivered'
  AND (uc.SelectedProductID IS NULL OR uc.SelectedProductID = 0);

SELECT CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });
                cmd.Parameters.Add(new SqlParameter("@AwardLevel", SqlDbType.Int) { Value = awardLevel });
                cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = productId });

                var obj = await cmd.ExecuteScalarAsync();
                return (obj != null && obj != DBNull.Value && Convert.ToInt32(obj) == 1);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetGiftSelectionAsync(string userName, int cYear, int awardLevel)
        {
            if (string.IsNullOrWhiteSpace(userName)) return false;
            if (cYear <= 0 || awardLevel <= 0) return false;

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // Delivered는 잠금(재선택 불가)
                string sql = @"
UPDATE dbo.AwardUserClaim
SET
    SelectedProductID = NULL,
    SelectedDT = NULL,
    Status = 'Eligible',
    UpdateDT = GETDATE()
WHERE UserName = @UserName
  AND cYear = @cYear
  AND AwardLevel = @AwardLevel
  AND Status <> 'Delivered';

SELECT CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });
                cmd.Parameters.Add(new SqlParameter("@AwardLevel", SqlDbType.Int) { Value = awardLevel });

                var obj = await cmd.ExecuteScalarAsync();
                return (obj != null && obj != DBNull.Value && Convert.ToInt32(obj) == 1);
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<AwardClaimWithItemDTO>> GetMyAwardClaimsWithItemsAsync(string userName, int cYear)
        {
            var list = new List<AwardClaimWithItemDTO>();
            if (string.IsNullOrWhiteSpace(userName)) return list;

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql = @"
SELECT
    uc.AwardLevel,
    uc.Status,
    uc.SelectedProductID,
    uc.SelectedDT,
    ai.ProductName,
    ai.ProductDescription,
    ai.UnitPrice
FROM dbo.AwardUserClaim uc
LEFT JOIN dbo.AwardItem ai
    ON ai.ProductID = uc.SelectedProductID
WHERE uc.UserName = @UserName
  AND uc.cYear = @cYear
ORDER BY uc.AwardLevel ASC;
";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    list.Add(new AwardClaimWithItemDTO
                    {
                        AwardLevel = Convert.ToInt32(r["AwardLevel"]),
                        Status = (r["Status"]?.ToString() ?? ""),
                        SelectedProductID = (r["SelectedProductID"] == DBNull.Value) ? null : (int?)Convert.ToInt32(r["SelectedProductID"]),
                        SelectedDT = (r["SelectedDT"] == DBNull.Value) ? null : (DateTime?)Convert.ToDateTime(r["SelectedDT"]),
                        ProductName = (r["ProductName"] == DBNull.Value) ? null : r["ProductName"]?.ToString(),
                        ProductDescription = (r["ProductDescription"] == DBNull.Value) ? null : r["ProductDescription"]?.ToString(),
                        UnitPrice = (r["UnitPrice"] == DBNull.Value) ? null : (int?)Convert.ToInt32(r["UnitPrice"])
                    });
                }

                return list;
            }
            catch
            {
                return list;
            }
        }


        public async Task EnsureEligibleAwardsByTotalAsync(string userName, int cYear, int yearTotalPoint)
        {
            if (string.IsNullOrWhiteSpace(userName)) return;
            if (string.IsNullOrWhiteSpace(_connectionString)) return;
            if (yearTotalPoint <= 0) return;

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql = @"
;WITH Achieved AS (
    SELECT ab.Id AS AwardBaseId, ab.AwardLevel
    FROM dbo.AwardBase ab
    WHERE ab.AwardLevel <= @YearTotalPoint
),
Missing AS (
    SELECT a.AwardBaseId, a.AwardLevel
    FROM Achieved a
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.AwardUserClaim uc
        WHERE uc.UserName = @UserName
          AND uc.cYear = @cYear
          AND uc.AwardLevel = a.AwardLevel
    )
)
INSERT INTO dbo.AwardUserClaim
(
    UserName, cYear, AwardLevel, AwardBaseId,
    Status, SelectedProductID, SelectedDT, InsertDT, UpdateDT
)
SELECT
    @UserName, @cYear, m.AwardLevel, m.AwardBaseId,
    'Eligible', NULL, NULL, GETDATE(), GETDATE()
FROM Missing m;
";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });
                cmd.Parameters.Add(new SqlParameter("@YearTotalPoint", SqlDbType.Int) { Value = yearTotalPoint });

                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // award 보조 기능이므로 실패해도 상위 흐름 깨지지 않게
            }
        }

        public async Task<(bool hasPending, int pendingLevel)> HasPendingAwardSelectionAsync(string userName, int cYear, int yearTotalPoint)
        {
            if (string.IsNullOrWhiteSpace(userName)) return (false, 0);
            if (string.IsNullOrWhiteSpace(_connectionString)) return (false, 0);

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // ✅ AwardLevel <= @YearTotalPoint 조건 추가
                //    → 실제 누적 포인트를 넘는 레벨은 절대 팝업 대상이 되지 않음
                string sql = @"
SELECT TOP 1 AwardLevel
FROM dbo.AwardUserClaim
WHERE UserName = @UserName
  AND cYear = @cYear
  AND AwardLevel <= @YearTotalPoint
  AND (Status = 'Eligible' OR Status = 'Selected')
  AND (SelectedProductID IS NULL OR SelectedProductID = 0)
ORDER BY AwardLevel DESC;
";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
                cmd.Parameters.Add(new SqlParameter("@cYear", SqlDbType.Int) { Value = cYear });
                cmd.Parameters.Add(new SqlParameter("@YearTotalPoint", SqlDbType.Int) { Value = yearTotalPoint });

                var obj = await cmd.ExecuteScalarAsync();
                if (obj == null || obj == DBNull.Value) return (false, 0);

                return (true, Convert.ToInt32(obj));
            }
            catch
            {
                return (false, 0);
            }
        }


        public async Task<List<int>> EnsureEligibleAwardsAsync(string userName, int cYear, int yearTotalPoint)
        {
            var createdLevels = new List<int>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // ✅ AwardLevel(포인트 기준) <= yearTotalPoint 인데
            // ✅ AwardUserClaim에 아직 없으면 생성(Eligible)
            string sql = @"
;WITH Achieved AS (
    SELECT ab.Id AS AwardBaseId, ab.AwardLevel
    FROM dbo.AwardBase ab
    WHERE ab.AwardLevel <= @YearTotalPoint
),
Missing AS (
    SELECT a.AwardBaseId, a.AwardLevel
    FROM Achieved a
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.AwardUserClaim uc
        WHERE uc.UserName = @UserName
          AND uc.cYear    = @cYear
          AND uc.AwardLevel = a.AwardLevel
    )
)
INSERT INTO dbo.AwardUserClaim
(
    UserName, cYear, AwardLevel, AwardBaseId,
    Status, InsertDT, UpdateDT
)
OUTPUT inserted.AwardLevel
SELECT
    @UserName, @cYear, m.AwardLevel, m.AwardBaseId,
    'Eligible', GETDATE(), GETDATE()
FROM Missing m;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserName", userName);
            cmd.Parameters.AddWithValue("@cYear", cYear);
            cmd.Parameters.AddWithValue("@YearTotalPoint", yearTotalPoint);

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                createdLevels.Add(Convert.ToInt32(r[0]));
            }

            return createdLevels;
        }


        public async Task<List<AwardDashboardRow>> GetDashboardAsync(int cYear)
        {
            var list = new List<AwardDashboardRow>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // =====================================================
            // 1. 기본 Dashboard 데이터
            // =====================================================
            using (var cmd = new SqlCommand("dbo.usp_DM2_GetAwardDashboard", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@cYear", cYear);

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    list.Add(new AwardDashboardRow
                    {
                        UserName = r["UserName"].ToString()!,
                        cYear = cYear,
                        AwardLevel = Convert.ToInt32(r["AwardLevel"]),
                        Status = r["Status"].ToString()!,
                        SelectedProductID = r["SelectedProductID"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["SelectedProductID"]),
                        YearTotalPoint = Convert.ToInt32(r["YearTotalPoint"]),

                        // ✅ 서버에서 채우는 값(아래 단계에서 매핑)
                        UserId = null
                    });
                }
            }

            if (list.Count == 0)
                return list;

            // =====================================================
            // 2. UserName → UserId (Identity.User)
            // =====================================================
            var userNames = list.Select(x => x.UserName)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList();

            var userIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (userNames.Count > 0)
            {
                var p = new List<string>();
                using var cmd = new SqlCommand();
                cmd.Connection = conn;

                for (int i = 0; i < userNames.Count; i++)
                {
                    string pn = "@u" + i;
                    p.Add(pn);
                    cmd.Parameters.AddWithValue(pn, userNames[i]);
                }

                cmd.CommandText = $@"
SELECT UserName, Id
FROM [Identity].[User]
WHERE UserName IN ({string.Join(",", p)});
";

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    string userName = r["UserName"].ToString()!;
                    string userId = r["Id"].ToString()!;
                    if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(userId))
                        userIdMap[userName] = userId;
                }
            }

            // =====================================================
            // 3. UserId → Person (view_PersonMaster_2026)
            // =====================================================
            var userIds = userIdMap.Values
                                   .Where(x => !string.IsNullOrWhiteSpace(x))
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .ToList();

            var personMap = new Dictionary<string, (int personId, string fullName)>(StringComparer.OrdinalIgnoreCase);

            if (userIds.Count > 0)
            {
                var p = new List<string>();
                using var cmd = new SqlCommand();
                cmd.Connection = conn;

                for (int i = 0; i < userIds.Count; i++)
                {
                    string pn = "@pid" + i;
                    p.Add(pn);
                    cmd.Parameters.AddWithValue(pn, userIds[i]);
                }

                cmd.CommandText = $@"
SELECT UserId, PersonId, FullName
FROM dbo.view_PersonMaster_2026
WHERE UserId IN ({string.Join(",", p)});
";

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    string userId = r["UserId"].ToString()!;
                    int personId = Convert.ToInt32(r["PersonId"]);
                    string fullName = r["FullName"].ToString()!;

                    if (!string.IsNullOrWhiteSpace(userId))
                        personMap[userId] = (personId, fullName);
                }
            }

            // =====================================================
            // 4. PersonId → ClassInfo (학생 반정보)
            // =====================================================
            var personIds = personMap.Values.Select(x => x.personId).Distinct().ToList();
            var classMap = new Dictionary<int, (int? classYear, int? classBan)>();

            if (personIds.Count > 0)
            {
                var p = new List<string>();
                using var cmd = new SqlCommand();
                cmd.Connection = conn;

                for (int i = 0; i < personIds.Count; i++)
                {
                    string pn = "@cid" + i;
                    p.Add(pn);
                    cmd.Parameters.AddWithValue(pn, personIds[i]);
                }

                cmd.CommandText = $@"
SELECT Id, ClassYear, ClassBan
FROM dbo.ClassInfo_2026
WHERE Id IN ({string.Join(",", p)});
";

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    int id = Convert.ToInt32(r["Id"]);
                    int? year = r["ClassYear"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["ClassYear"]);
                    int? ban = r["ClassBan"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["ClassBan"]);

                    classMap[id] = (year, ban);
                }
            }

            // =====================================================
            // 4.5 교사 역할/부서 (학생 반정보가 없으면 교사로 간주)
            //   - view_All_Person_2026(UserId, DeptCode, RoleCode, PersonId, FullName)
            //   - CodeData(MajorCD='T001') 와 RoleCode=MinorCD 조인해서 MinorNM(RoleName)
            // =====================================================
            var teacherCandidateUserIds = new List<string>();
            foreach (var kv in personMap) // kv.Key = userId, kv.Value = (personId, fullName)
            {
                var userId = kv.Key;
                var personId = kv.Value.personId;

                if (!classMap.ContainsKey(personId))
                    teacherCandidateUserIds.Add(userId);
            }

            teacherCandidateUserIds = teacherCandidateUserIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var teacherMapByUserId = new Dictionary<string, (string deptCode, string roleCode, string roleName)>(StringComparer.OrdinalIgnoreCase);

            if (teacherCandidateUserIds.Count > 0)
            {
                var p = new List<string>();
                using var cmd = new SqlCommand();
                cmd.Connection = conn;

                for (int i = 0; i < teacherCandidateUserIds.Count; i++)
                {
                    string pn = "@tuid" + i;
                    p.Add(pn);
                    cmd.Parameters.AddWithValue(pn, teacherCandidateUserIds[i]);
                }

                cmd.CommandText = $@"
SELECT
    t.UserId,
    t.DeptCode,
    t.RoleCode,
    cd.MinorNM AS RoleName
FROM dbo.view_All_Person_2026 t
LEFT JOIN dbo.CodeData cd
    ON cd.MajorCD = 'T001'
   AND cd.MinorCD = t.RoleCode
WHERE t.UserId IN ({string.Join(",", p)});
";

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var userId = r["UserId"].ToString()!;
                    var deptCode = r["DeptCode"]?.ToString() ?? "";
                    var roleCode = r["RoleCode"]?.ToString() ?? "";
                    var roleName = (r["RoleName"] == DBNull.Value) ? "" : r["RoleName"].ToString()!;

                    if (!string.IsNullOrWhiteSpace(userId))
                        teacherMapByUserId[userId] = (deptCode, roleCode, roleName);
                }
            }

            // =====================================================
            // 5. 상품 정보 (AwardItem)
            // =====================================================
            var productIds = list.Where(x => x.SelectedProductID.HasValue)
                                 .Select(x => x.SelectedProductID!.Value)
                                 .Distinct()
                                 .ToList();

            var productMap = new Dictionary<int, (string name, string? desc)>();

            if (productIds.Count > 0)
            {
                var p = new List<string>();
                using var cmd = new SqlCommand();
                cmd.Connection = conn;

                for (int i = 0; i < productIds.Count; i++)
                {
                    string pn = "@p" + i;
                    p.Add(pn);
                    cmd.Parameters.AddWithValue(pn, productIds[i]);
                }

                cmd.CommandText = $@"
SELECT ProductID, ProductName, ProductDescription
FROM dbo.AwardItem
WHERE ProductID IN ({string.Join(",", p)});
";

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    int id = Convert.ToInt32(r["ProductID"]);
                    string name = r["ProductName"].ToString()!;
                    string? desc = r["ProductDescription"] == DBNull.Value ? null : r["ProductDescription"].ToString();

                    productMap[id] = (name, desc);
                }
            }

            // =====================================================
            // 6. 최종 매핑 (학생/교사 판별 + 뷰용 표시 문자열까지 서버에서 완성)
            //    ✅ 여기서 row.UserId 를 반드시 채워줌
            // =====================================================
            foreach (var row in list)
            {
                // 기본 fallback
                row.DisplayGroupText = "학년/반 없음";
                row.IsStudent = false;
                row.IsTeacher = false;

                // ✅ UserId 먼저 채워두기 (Person 매핑 성공 여부와 무관하게)
                if (userIdMap.TryGetValue(row.UserName, out var userId))
                {
                    row.UserId = userId;
                }
                else
                {
                    row.UserId = null;
                }

                // User → Person
                if (!string.IsNullOrWhiteSpace(row.UserId) &&
                    personMap.TryGetValue(row.UserId!, out var person))
                {
                    row.FullName = person.fullName;

                    // 학생(반정보 존재)
                    if (classMap.TryGetValue(person.personId, out var cls) &&
                        cls.classYear.HasValue && cls.classBan.HasValue &&
                        cls.classYear.Value > 0)
                    {
                        row.ClassYear = cls.classYear;
                        row.ClassBan = cls.classBan;

                        row.IsStudent = true;
                        row.IsTeacher = false;
                        row.DisplayGroupText = $"{row.ClassYear}학년 {row.ClassBan}반";
                    }
                    else
                    {
                        // 교사(반정보 없음)
                        row.IsStudent = false;
                        row.IsTeacher = true;

                        if (teacherMapByUserId.TryGetValue(row.UserId!, out var t))
                        {
                            row.DeptCode = t.deptCode;
                            row.RoleCode = t.roleCode;
                            row.RoleName = string.IsNullOrWhiteSpace(t.roleName) ? "교사" : t.roleName;

                            // ✅ 뷰에서 그대로 출력할 값 (기본은 역할명)
                            row.DisplayGroupText = row.RoleName!;
                        }
                        else
                        {
                            row.RoleName = "교사";
                            row.DisplayGroupText = "교사";
                        }
                    }
                }

                // Product
                if (row.SelectedProductID.HasValue &&
                    productMap.TryGetValue(row.SelectedProductID.Value, out var prod))
                {
                    row.SelectedProductName = prod.name;
                    row.SelectedProductDescription = prod.desc;
                }

                // Image rule + fallback (상품)
                row.ProductImageUrl = row.SelectedProductID.HasValue
                    ? $"/images/AwardItem/{row.SelectedProductID.Value}.jpg"
                    : "/images/common/no-image.jpg";

                // ✅ 사람사진은 "뷰 규칙"으로 처리 (서버에서 file exists 체크는 안 함)
                //    - 뷰에서: /images/profile/{UserId}.png?t=... 로 넣고,
                //      onerror 로 /images/icon/people/people_01.webp 로 교체
            }

            return list;
        }

        public async Task<List<AwardUserClaimRow>> GetDashboard2Async(int cYear)
        {
            var list = new List<AwardUserClaimRow>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("dbo.usp_DM2_GetAwardDashboard2", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@cYear", cYear);

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(Map(r));
            }

            return list;
        }

        public async Task<List<AwardUserClaimRow>> GetMyAwardsAsync(string userName)
        {
            var list = new List<AwardUserClaimRow>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("dbo.usp_DM2_GetMyAwards", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userName", userName);

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(Map(r));
            }

            return list;
        }

        public async Task MarkDeliveredAsync(string userName, int cYear, int awardLevel)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("dbo.usp_DM2_MarkAwardDelivered", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userName", userName);
            cmd.Parameters.AddWithValue("@cYear", cYear);
            cmd.Parameters.AddWithValue("@awardLevel", awardLevel);

            await cmd.ExecuteNonQueryAsync();
        }

        private static AwardUserClaimRow Map(SqlDataReader r)
        {
            return new AwardUserClaimRow
            {
                Id = Convert.ToInt32(r["Id"]),
                UserName = r["UserName"].ToString()!,
                cYear = Convert.ToInt32(r["cYear"]),
                AwardLevel = Convert.ToInt32(r["AwardLevel"]),
                AwardBaseId = r["AwardBaseId"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["AwardBaseId"]),
                Status = r["Status"].ToString()!,
                SelectedProductID = r["SelectedProductID"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["SelectedProductID"]),
                SelectedDT = r["SelectedDT"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(r["SelectedDT"]),
                InsertDT = Convert.ToDateTime(r["InsertDT"]),
                UpdateDT = Convert.ToDateTime(r["UpdateDT"])
            };
        }
    }
}

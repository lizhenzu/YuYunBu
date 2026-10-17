using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BulletinBoard.Services
{
    public class PresentPickService : IPresentPickService
    {
        private readonly string _connStr;

        public PresentPickService(IConfiguration config)
        {
            // appsettings.json: ConnectionStrings:DefaultConnection
            _connStr = config.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("DefaultConnection is missing.");
        }

        private SqlConnection CreateConn() => new SqlConnection(_connStr);

        /// <summary>
        /// 서버/DB 시간 대신 "한국 표준시(KST)" 기준으로 저장하기 위한 현재 시각.
        /// (클라이언트 기준과 가장 현실적으로 일치, 조작 위험 없음)
        /// </summary>
        private static DateTime GetKoreaNow()
        {
            // Windows 서버: "Korea Standard Time"
            // Linux 서버면 "Asia/Seoul"을 써야 하는데, SmarterASP는 보통 Windows라 이걸 우선 사용
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        /// <summary>
        /// ✅ (관리화면 UI용) 누군가 선택한 적(현재 테이블에 존재) 있는 상품ID 목록
        /// - PresentItemPick에 ProductId가 존재하면 삭제 불가(UI disable)
        /// - 향후 선택 테이블이 늘어나면 여기에서 UNION으로 확장 가능
        /// </summary>
        public async Task<HashSet<int>> GetPickedProductIdsAsync()
        {
            const string sql = @"
SELECT DISTINCT ProductId FROM [dbo].[PresentItemPick] WITH (NOLOCK)
union 
select DISTINCT ProductId FROM [dbo].[AwardItemPick] WITH (NOLOCK)
union 
select DISTINCT SelectedProductID FROM dbo.AwardUserClaim uc JOIN dbo.AwardItem ai  ON ai.ProductID = uc.SelectedProductID

";

            var set = new HashSet<int>();

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                if (r["ProductId"] != DBNull.Value)
                    set.Add(Convert.ToInt32(r["ProductId"]));
            }

            return set;
        }

        // ✅ 내가 현재 선택한 선물(활성 1개)
        public async Task<PresentItemPick?> GetMyBirthPickAsync(string userId)
        {
            const string sql = @"
                                SELECT TOP 1
                                    PickId, UserId, ProductId, ProductName, ProductDescription,SourceType, PickedAt, IsActive, IsPurchased, PurchasedAt
                                FROM dbo.PresentItemPick WITH (NOLOCK)
                                WHERE UserId = @UserId
                                  AND IsActive = 1
                                ORDER BY PickedAt DESC;";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 50) { Value = userId });

            await conn.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await r.ReadAsync()) return null;

            return new PresentItemPick
            {
                PickId = Convert.ToInt32(r["PickId"]),
                UserId = r["UserId"]?.ToString() ?? "",
                ProductId = Convert.ToInt32(r["ProductId"]),
                ProductName = r["ProductName"]?.ToString() ?? "",
                ProductDescription = r["ProductDescription"] == DBNull.Value ? null : r["ProductDescription"]?.ToString(),
                SourceType = r["SourceType"] == DBNull.Value ? null : r["SourceType"]?.ToString(),
                PickedAt = Convert.ToDateTime(r["PickedAt"]),
                IsActive = Convert.ToBoolean(r["IsActive"]),
                IsPurchased = r["IsPurchased"] != DBNull.Value && Convert.ToBoolean(r["IsPurchased"]),
                PurchasedAt = r["PurchasedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["PurchasedAt"]),
            };
        }

        // ✅ overwrite 저장: (히스토리 없이 1개만 유지) -> 기존 전부 DELETE 후 INSERT
        public async Task SaveBirthPickAsync(string userId, AwardItem item)
        {
            if (item?.ProductID == null || item.ProductID <= 0)
                throw new ArgumentException("Invalid ProductID.");

            // ✅ 저장 시각을 한국시간으로 고정
            var pickedAtKst = GetKoreaNow();

            const string sqlDelete = @"
DELETE FROM dbo.PresentItemPick
WHERE UserId = @UserId;";

            // ✅ GETDATE() 제거 -> @PickedAt 파라미터로 저장
            const string sqlInsert = @"
INSERT INTO dbo.PresentItemPick
(
    UserId,
    ProductId,
    ProductName,
    ProductDescription,
    SourceType,
    PickedAt,
    IsActive
)
VALUES
(
    @UserId,
    @ProductId,
    @ProductName,
    @ProductDescription,
    @SourceType,
    @PickedAt,
    1
);";

            using var conn = CreateConn();
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                // 1) 기존 선택 전부 삭제
                using (var cmd = new SqlCommand(sqlDelete, conn, tx))
                {
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 50) { Value = userId });
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2) 새 선택 INSERT
                using (var cmd = new SqlCommand(sqlInsert, conn, tx))
                {
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 50) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = item.ProductID });

                    cmd.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.NVarChar, 200)
                    {
                        Value = (object?)item.ProductName ?? ""
                    });

                    cmd.Parameters.Add(new SqlParameter("@ProductDescription", SqlDbType.NVarChar, 500)
                    {
                        Value = (object?)item.ProductDescription ?? DBNull.Value
                    });

                    cmd.Parameters.Add(new SqlParameter("@SourceType", SqlDbType.NVarChar, 100)
                    {
                        Value = (object?)item.SourceType ?? DBNull.Value
                    });

                    // ✅ 한국시간으로 저장
                    cmd.Parameters.Add(new SqlParameter("@PickedAt", SqlDbType.DateTime2)
                    {
                        Value = pickedAtKst
                    });

                    await cmd.ExecuteNonQueryAsync();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ✅ 선택 취소: (히스토리 없이 유지 구조) -> DELETE
        public async Task CancelBirthPickAsync(string userId)
        {
            const string sql = @"DELETE FROM dbo.PresentItemPick WHERE UserId = @UserId;";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 50) { Value = userId });

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ✅ 관리자: 누가 무엇을 선택했는지 (사용자정보는 getUserInfos로 조인)
        public async Task<BirthPickAdminListVM> GetAdminBirthPicksAsync(
            string? q,
            bool includeInactive,
            IWebHostEnvironment hostingEnvironment)
        {
            q = (q ?? "").Trim();

            // 1) 사용자 목록(사진 포함)
            ManageHumanResource manageHumanResourceA = new ManageHumanResource();
            List<UserInfo> users = manageHumanResourceA.getUserInfos(
                hostingEnvironment,
                string.Empty,
                string.Empty,
                0, 0, 0,
                string.Empty,
                "D1001",
                RankQueryOption.StudentOnly);

            // UserId 없는 항목은 제외(안전)
            users = users
                .Where(u => !string.IsNullOrWhiteSpace(u.UserId))
                .ToList();

            // PresentItemPick.UserId 와 UserInfos.UserId 를 매칭한다고 가정
            var userMap = users
                .GroupBy(u => u.UserId!)
                .ToDictionary(g => g.Key, g => g.First());

            // 2) PresentItemPick 조회 (선택한 사람들만 rows로 내려오는 구조)
            var rows = await LoadBirthPickRowsAsync(q, includeInactive, userMap);

            // 3) rows -> UserId 기준 매핑 (선택 기록 있는 사람만)
            var rowMap = rows
                .Where(r => r.User != null && !string.IsNullOrWhiteSpace(r.User.UserId))
                .GroupBy(r => r.User!.UserId!)
                .ToDictionary(g => g.Key, g => g.First()); // 같은 유저가 여러 건이면 첫 건만

            // 4) ✅ users 기준으로 rows를 "항상 전체 인원"으로 채우기
            //    q 검색도 users 기준으로 적용
            bool hasQuery = !string.IsNullOrWhiteSpace(q);
            var filledRows = new List<BirthPickAdminRowVM>(users.Count);

            foreach (var user in users)
            {
                // (A) users 기준 검색 필터
                if (hasQuery)
                {
                    bool match =
                        (user.FullName ?? "").Contains(q)
                        || (user.UserName ?? "").Contains(q)
                        || (user.LastName ?? "").Contains(q)
                        || (user.FirstName ?? "").Contains(q)
                        || user.ClassYear.ToString().Contains(q)
                        || user.ClassBan.ToString().Contains(q);

                    if (!match)
                        continue;
                }

                // (B) 선택 기록 있으면 기존 row 사용, 없으면 빈 row 생성
                if (rowMap.TryGetValue(user.UserId!, out var existingRow))
                {
                    // 혹시 User가 null로 들어왔거나 매칭이 이상하면 user로 강제 보정
                    existingRow.User ??= user;
                    filledRows.Add(existingRow);
                }
                else
                {
                    // ✅ 빈 row 채움 (선물 선택 정보는 비워두기)
                    filledRows.Add(new BirthPickAdminRowVM
                    {
                        User = user
                    });
                }
            }

            // ✅ 생일(다가오는 생일) 기준 정렬
            // - bMonth/bDay 없으면 맨 아래
            // - 이미 지난 생일이면 내년 기준으로 D-Day 계산
            int GetBirthdayOrder(UserInfo? u)
            {
                if (u == null) return int.MaxValue;

                int month = 0, day = 0;

                // bMonth/bDay가 nullable일 수도 있으니 안전 처리
                try
                {
                    month = Convert.ToInt32(u.bMonth);
                    day = Convert.ToInt32(u.bDay);
                }
                catch
                {
                    return int.MaxValue;
                }

                if (month <= 0 || day <= 0) return int.MaxValue;

                var today = GetKoreaNow().Date;

                DateTime target;
                try
                {
                    target = new DateTime(today.Year, month, day);
                }
                catch
                {
                    return int.MaxValue; // 2/30 같은 잘못된 날짜 방어
                }

                if (target < today)
                    target = target.AddYears(1);

                return (target - today).Days;
            }

            rows = filledRows
                .OrderBy(r => GetBirthdayOrder(r.User))
                .ThenBy(r => r.User?.ClassYear)
                .ThenBy(r => r.User?.ClassBan)
                .ThenBy(r => r.User?.FullName)
                .ToList();

            return new BirthPickAdminListVM
            {
                Q = q,
                IncludeInactive = includeInactive,
                TotalCount = rows.Count,
                Rows = rows
            };
        }

        public async Task SetBirthPickSentAsync(long pickId, bool isSent)
        {
            const string sql = @"
UPDATE dbo.PresentItemPick
SET
    IsSent = @IsSent,
    SentAt = @SentAt
WHERE PickId = @PickId;
";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add(new SqlParameter("@PickId", SqlDbType.BigInt) { Value = pickId });
            cmd.Parameters.Add(new SqlParameter("@IsSent", SqlDbType.Bit) { Value = isSent });

            // ✅ 한국시간 저장
            cmd.Parameters.Add(new SqlParameter("@SentAt", SqlDbType.DateTime2)
            {
                Value = (object?)GetKoreaNow() ?? DBNull.Value
            });

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SetBirthPickPurchasedAsync(long pickId, bool isPurchased)
        {
            const string sql = @"
UPDATE dbo.PresentItemPick
SET
    IsPurchased = @IsPurchased,
    PurchasedAt = @PurchasedAt
WHERE PickId = @PickId;
";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add(new SqlParameter("@PickId", SqlDbType.BigInt) { Value = pickId });
            cmd.Parameters.Add(new SqlParameter("@IsPurchased", SqlDbType.Bit) { Value = isPurchased });

            // ✅ 한국시간 저장
            cmd.Parameters.Add(new SqlParameter("@PurchasedAt", SqlDbType.DateTime2)
            {
                Value = (object?)GetKoreaNow() ?? DBNull.Value
            });

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<List<BirthPickAdminRowVM>> LoadBirthPickRowsAsync(string? q, bool includeInactive, Dictionary<string, UserInfo> userMap)
        {
            var rows = new List<BirthPickAdminRowVM>();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT");
            sb.AppendLine("  PickId, UserId, ProductId, ProductName, ProductDescription, SourceType, PickedAt, IsActive, IsSent, SentAt, IsPurchased, PurchasedAt");
            sb.AppendLine("FROM dbo.PresentItemPick WITH (NOLOCK)");
            sb.AppendLine("WHERE 1=1");

            // includeInactive=false면 활성만
            if (!includeInactive)
            {
                sb.AppendLine("AND IsActive = 1");
            }

            // 1차 검색: UserId / ProductName
            if (!string.IsNullOrEmpty(q))
            {
                sb.AppendLine("AND (UserId LIKE @Q OR ProductName LIKE @Q)");
            }

            sb.AppendLine("ORDER BY PickedAt DESC, PickId DESC;");

            using (var conn = CreateConn())
            using (var cmd = new SqlCommand(sb.ToString(), conn))
            {
                cmd.CommandType = CommandType.Text;

                if (!string.IsNullOrEmpty(q))
                {
                    cmd.Parameters.Add(new SqlParameter("@Q", SqlDbType.NVarChar, 200) { Value = "%" + q + "%" });
                }

                await conn.OpenAsync();

                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        var pick = new BirthPickAdminRowVM
                        {
                            PickId = Convert.ToInt64(r["PickId"]),
                            UserId = r["UserId"]?.ToString() ?? "",

                            ProductId = Convert.ToInt32(r["ProductId"]),
                            ProductName = r["ProductName"]?.ToString() ?? "",
                            ProductDescription = r["ProductDescription"] == DBNull.Value ? "" : (r["ProductDescription"]?.ToString() ?? ""),
                            SourceType = r["SourceType"] == DBNull.Value ? "" : (r["SourceType"]?.ToString() ?? ""),
                            IsSent = r["IsSent"] != DBNull.Value && Convert.ToBoolean(r["IsSent"]),
                            SentAt = r["SentAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["SentAt"]),
                            IsPurchased = r["IsPurchased"] != DBNull.Value && Convert.ToBoolean(r["IsPurchased"]),
                            PurchasedAt = r["PurchasedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["PurchasedAt"]),
                            IsActive = Convert.ToBoolean(r["IsActive"]),
                            PickedAt = Convert.ToDateTime(r["PickedAt"]),
                            UpdatedAt = null // PresentItemPick에 UpdatedAt 컬럼이 없다고 가정
                        };

                        // 메모리 조인(UserInfos)
                        if (!string.IsNullOrWhiteSpace(pick.UserId) && userMap.TryGetValue(pick.UserId, out var u))
                            pick.User = u;

                        rows.Add(pick);
                    }
                }
            }

            return rows;
        }

        public async Task<UpcomingBirthdayListVM> GetUpcomingBirthdaysAsync(int withinDays, string filter, IWebHostEnvironment hostingEnvironment)
        {
            var today = GetKoreaNow().Date;
            int year  = today.Year;

            // 1) 학생 전체 로딩
            var manage = new ManageHumanResource();
            var users = manage.getUserInfos(
                hostingEnvironment,
                string.Empty, string.Empty,
                0, 0, 0,
                string.Empty, string.Empty,
                RankQueryOption.StudentOnly);

            // 2) 올해(1/1~12/31) 생일 후보 전체
            var candidates = new List<(UserInfo User, DateTime BirthDate, int DDay)>();
            foreach (var u in users)
            {
                if (string.Equals(u.birthDay_PublishYN, "h", StringComparison.OrdinalIgnoreCase))
                    continue;

                int m, d;
                try { m = Convert.ToInt32(u.bMonth); d = Convert.ToInt32(u.bDay); }
                catch { continue; }

                if (m <= 0 || d <= 0) continue;

                DateTime birthDate;
                try { birthDate = new DateTime(year, m, d).Date; }
                catch { continue; } // 윤년 2/29 등 방어

                int dday = (birthDate - today).Days;
                candidates.Add((u, birthDate, dday));
            }

            if (candidates.Count == 0)
            {
                return new UpcomingBirthdayListVM
                {
                    WithinDays = withinDays,
                    TotalCount = 0,
                    Rows = new List<UpcomingBirthdayRowVM>()
                };
            }

            // 3) 활성 선택(pick) 맵 — UserId 기반
            var userIds = candidates
                .Select(c => c.User.UserId ?? "")
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();
            var pickMap = await LoadActivePickMapAsync(userIds);

            // 4) 메모 맵 — PersonId 기반
            var personIds = candidates.Select(c => c.User.PersonId).Distinct().ToList();
            var memoMap   = await LoadMemoMapAsync(personIds);

            // 5) 미처리 보류(skip) 셋 — PersonId 기반
            var skipSet = await LoadSkipSetAsync(personIds);

            // 6) 합치기 + 정렬: 생일 날짜 순 (1월 1일 → 12월 31일)
            var rows = candidates
                .OrderBy(x => x.User.bMonth)
                .ThenBy(x => x.User.bDay)
                .ThenBy(x => x.User.ClassYear)
                .ThenBy(x => x.User.ClassBan)
                .ThenBy(x => x.User.FullName)
                .Select(x =>
                {
                    pickMap.TryGetValue(x.User.UserId ?? "", out var p);
                    memoMap.TryGetValue(x.User.PersonId, out var memo);
                    bool isSkipped = skipSet.Contains(x.User.PersonId);

                    return new UpcomingBirthdayRowVM
                    {
                        User              = x.User,
                        NextBirthdayDate  = x.BirthDate,
                        DDay              = x.DDay,
                        HasPickedGift     = p != null,
                        PickId            = p?.PickId,
                        ProductId         = p?.ProductId,
                        ProductName       = p?.ProductName,
                        ProductDescription= p?.ProductDescription,
                        SourceType        = p?.SourceType,
                        IsSent            = p?.IsSent ?? false,
                        SentAt            = p?.SentAt,
                        IsPurchased       = p?.IsPurchased ?? false,
                        PurchasedAt       = p?.PurchasedAt,
                        Memo              = memo,
                        IsSkipped         = isSkipped
                    };
                })
                .ToList();

            return new UpcomingBirthdayListVM
            {
                WithinDays = withinDays,
                TotalCount = rows.Count,
                Rows       = rows
            };
        }

        /// <summary>미처리 보류 상태 upsert</summary>
        public async Task SetBirthdaySkipAsync(int personId, bool isSkipped, string? updatedBy)
        {
            if (personId <= 0) return;

            const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.BirthdayGiftSkip WHERE PersonID = @PersonId)
    UPDATE dbo.BirthdayGiftSkip
    SET    IsSkipped = @IsSkipped, SkippedAt = @Now, UpdatedBy = @UpdatedBy
    WHERE  PersonID  = @PersonId;
ELSE
    INSERT INTO dbo.BirthdayGiftSkip (PersonID, IsSkipped, SkippedAt, UpdatedBy)
    VALUES (@PersonId, @IsSkipped, @Now, @UpdatedBy);";

            using var conn = CreateConn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@PersonId",  SqlDbType.Int)          { Value = personId });
            cmd.Parameters.Add(new SqlParameter("@IsSkipped", SqlDbType.Bit)          { Value = isSkipped });
            cmd.Parameters.Add(new SqlParameter("@Now",       SqlDbType.DateTime2)    { Value = GetKoreaNow() });
            cmd.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.NVarChar, 100){ Value = (object?)updatedBy ?? DBNull.Value });

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>BirthdayGiftSkip 에서 미처리 PersonId 셋 로드</summary>
        private async Task<HashSet<int>> LoadSkipSetAsync(List<int> personIds)
        {
            var set = new HashSet<int>();
            if (personIds == null || personIds.Count == 0) return set;

            var paramNames = personIds.Select((_, i) => "@P" + i).ToArray();
            var sql = $@"
SELECT PersonID
FROM   dbo.BirthdayGiftSkip WITH (NOLOCK)
WHERE  IsSkipped = 1
  AND  PersonID IN ({string.Join(",", paramNames)});";

            using var conn = CreateConn();
            using var cmd  = new SqlCommand(sql, conn);
            for (int i = 0; i < personIds.Count; i++)
                cmd.Parameters.Add(new SqlParameter(paramNames[i], SqlDbType.Int) { Value = personIds[i] });

            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                if (r[0] != DBNull.Value) set.Add(Convert.ToInt32(r[0]));

            return set;
        }

        private async Task<Dictionary<int, string>> LoadMemoMapAsync(List<int> personIds)
        {
            var map = new Dictionary<int, string>();

            if (personIds == null || personIds.Count == 0)
                return map;

            var paramNames = personIds.Select((_, i) => "@P" + i).ToArray();

            var sql = $@"
SELECT PersonID, Memo
FROM dbo.BirthdayGiftPickMeMo WITH (NOLOCK)
WHERE PersonID IN ({string.Join(",", paramNames)})
  AND Memo IS NOT NULL
  AND Memo <> '';";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);

            for (int i = 0; i < personIds.Count; i++)
                cmd.Parameters.Add(new SqlParameter(paramNames[i], SqlDbType.Int) { Value = personIds[i] });

            await conn.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                if (r["PersonID"] == DBNull.Value) continue;
                var personId = Convert.ToInt32(r["PersonID"]);
                var memo     = r["Memo"]?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(memo))
                    map[personId] = memo;
            }

            return map;
        }

        private async Task<Dictionary<string, ActivePickInfo>> LoadActivePickMapAsync(List<string> userIds)
        {
            var map = new Dictionary<string, ActivePickInfo>(StringComparer.OrdinalIgnoreCase);

            if (userIds == null || userIds.Count == 0)
                return map;

            var paramNames = userIds.Select((_, i) => "@U" + i).ToArray();

            var sql = $@"
SELECT
    pp.PickId, pp.UserId, pp.ProductId,
    ISNULL(a.ProductName,        pp.ProductName)        AS ProductName,
    ISNULL(a.ProductDescription, pp.ProductDescription) AS ProductDescription,
    pp.SourceType, pp.IsSent, pp.SentAt, pp.IsPurchased, pp.PurchasedAt, pp.PickedAt
FROM dbo.PresentItemPick pp WITH (NOLOCK)
LEFT JOIN dbo.AwardItem a ON a.ProductID = pp.ProductId
WHERE pp.IsActive = 1
  AND pp.UserId IN ({string.Join(",", paramNames)})
ORDER BY pp.PickedAt DESC, pp.PickId DESC;";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);

            for (int i = 0; i < userIds.Count; i++)
                cmd.Parameters.Add(new SqlParameter(paramNames[i], SqlDbType.NVarChar, 50) { Value = userIds[i] });

            await conn.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var userId = r["UserId"]?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(userId)) continue;

                // 최신순으로 읽으므로, 유저당 첫 건만 채택
                if (map.ContainsKey(userId)) continue;

                var info = new ActivePickInfo
                {
                    PickId = Convert.ToInt64(r["PickId"]),
                    ProductId = Convert.ToInt32(r["ProductId"]),
                    ProductName = r["ProductName"]?.ToString() ?? "",
                    ProductDescription = r["ProductDescription"]?.ToString() ?? "",
                    SourceType = r["SourceType"]?.ToString() ?? "",
                    IsSent = r["IsSent"] != DBNull.Value && Convert.ToBoolean(r["IsSent"]),
                    SentAt = r["SentAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["SentAt"]),
                    IsPurchased = r["IsPurchased"] != DBNull.Value && Convert.ToBoolean(r["IsPurchased"]),
                    PurchasedAt = r["PurchasedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["PurchasedAt"])
                };

                map[userId] = info;
            }

            return map;
        }
    }
}

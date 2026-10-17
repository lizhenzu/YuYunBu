using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Services.Abstract;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Services.StudentPointGift
{
    public sealed class StudentPointGiftService : IStudentPointGiftService
    {
        private readonly string _connStr;
        private readonly IAttendanceService _attendanceService;

        private readonly BulletinBoard.Services.GiftCatalog.IGiftCatalogService _giftCatalog;

        public StudentPointGiftService(
            IConfiguration config,
            IAttendanceService attendanceService,
            BulletinBoard.Services.GiftCatalog.IGiftCatalogService giftCatalog)
        {
            _connStr = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing.");

            _attendanceService = attendanceService
                ?? throw new ArgumentNullException(nameof(attendanceService));

            _giftCatalog = giftCatalog
                ?? throw new ArgumentNullException(nameof(giftCatalog));
        }

        private SqlConnection Conn() => new SqlConnection(_connStr);

        private static string BuildProfileUrl(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return "/images/icon-user.png";
            // ProfileUpdateSqlService와 동일 규칙: /images/profile/{userId}.png
            return $"/images/profile/{userId}.png";
        }

        private async Task<int> GetBiblePointsThisYearAsync(string userName, int year)
        {
            if (string.IsNullOrWhiteSpace(userName)) return 0;
            if (year <= 0) year = DateHelper.GetKoreaNow().Year;

            var from = new DateTime(year, 1, 1);
            var to = DateHelper.GetKoreaNow().Date;
            if (to.Year != year) to = new DateTime(year, 12, 31);

            using var conn = Conn();
            await conn.OpenAsync();

            const string sql = @"
SELECT ISNULL(SUM(TotalPoint), 0)
FROM dbo.ufn_DM2_GetUserDailyPoints(@U, @F, @T);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@U", SqlDbType.NVarChar, 100) { Value = userName });
            cmd.Parameters.Add(new SqlParameter("@F", SqlDbType.Date) { Value = from.Date });
            cmd.Parameters.Add(new SqlParameter("@T", SqlDbType.Date) { Value = to.Date });

            var obj = await cmd.ExecuteScalarAsync();
            return obj == null ? 0 : Convert.ToInt32(obj);
        }

        private async Task<int> GetMissionBookPointAsync(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return 0;

            using var conn = Conn();
            await conn.OpenAsync();

            const string sql = @"
;WITH X AS (
    SELECT LessonNo, MAX(TotalPoint) AS Score
    FROM dbo.WorldviewLessonSubmitHistory WITH (NOLOCK)
    WHERE UserId = @UserId
      AND BasePoint > 0
    GROUP BY LessonNo
)
SELECT ISNULL(SUM(Score), 0) FROM X;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = userId;

            var obj = await cmd.ExecuteScalarAsync();
            return obj == null ? 0 : Convert.ToInt32(obj);
        }

        private async Task<int> GetThanksPointTotalAsync(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return 0;

            using var conn = Conn();
            await conn.OpenAsync();

            const string sql = @"
;WITH Me AS (
    SELECT TOP 1 PersonId
    FROM dbo.view_UserCompositeInfo_2026 WITH (NOLOCK)
    WHERE UserId = @UserId
)
SELECT ISNULL(SUM(dm.Points), 0) AS TotalPoints
FROM dbo.Point_DevotionMonthly dm WITH (NOLOCK)
WHERE dm.PersonId = (SELECT PersonId FROM Me);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = userId;

            var obj = await cmd.ExecuteScalarAsync();
            return obj == null ? 0 : Convert.ToInt32(obj);
        }

        private async Task<int> GetAttendancePointAsync(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return 0;

            try
            {
                var attendanceCount = await _attendanceService.GetMyAttendanceCountYearToDateAsync(userId);
                if (attendanceCount < 0) attendanceCount = 0;

                return attendanceCount * ConstatsCommon.attendancePoint;
            }
            catch
            {
                return 0;
            }
        }

        private static int ToMilestone100(int integrated)
        {
            if (integrated < 100) return 0;
            var m = (integrated / 100) * 100;
            if (m > 2000) m = 2000;
            return m;
        }

        //        public async Task HideOrderAsync(int orderId, string adminId)
        //        {
        //            using var conn = Conn();
        //            await conn.OpenAsync();
        //
        //            using var cmd = new SqlCommand(@"
        //UPDATE dbo.StudentPointGiftOrder
        //SET Status='HIDDEN', AdminId=@A
        //WHERE OrderId=@Id;", conn);
        //
        //            cmd.Parameters.AddWithValue("@Id", orderId);
        //            cmd.Parameters.AddWithValue("@A", adminId);
        //
        //            await cmd.ExecuteNonQueryAsync();
        //        }

        public async Task HidePurchaseAsync(int orderId, string adminId)
        {
            using var conn = Conn();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
UPDATE dbo.StudentPointGiftOrder
SET Status='HIDDEN', AdminId=@A, DeliveredAt=@D
WHERE OrderId=@Id;", conn);

            cmd.Parameters.AddWithValue("@Id", orderId);
            cmd.Parameters.AddWithValue("@A", adminId);
            // 전달일 (2026-09-26 칸 추가) — 홈 "축하해 주세요~~" 선물 칸에 보여 준다
            cmd.Parameters.Add("@D", SqlDbType.DateTime).Value = DateHelper.GetKoreaNow();

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>전달 완료 되돌리기 — HIDDEN(전달완료)을 다시 구매완료로</summary>
        public async Task UndoHideAsync(int orderId, string adminId)
        {
            using var conn = Conn();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
UPDATE dbo.StudentPointGiftOrder
SET Status='PURCHASED', AdminId=@A, DeliveredAt=NULL
WHERE OrderId=@Id AND Status='HIDDEN';", conn);

            cmd.Parameters.AddWithValue("@Id", orderId);
            cmd.Parameters.AddWithValue("@A", adminId ?? "");

            var n = await cmd.ExecuteNonQueryAsync();
            if (n == 0) throw new InvalidOperationException("전달 완료 상태가 아니거나 대상이 없습니다.");
        }

        // ✅ integratedTotal과 무관한 DB 조회를 미리 실행 (쿼리 1~9와 병렬화용)
        public async Task<GiftEvalPreload> PreloadEvalStateAsync(string userName)
        {
            using var conn = Conn();
            await conn.OpenAsync();

            // 1) popup state
            int? lastSeen = null;
            DateTime? blockUntilUtc = null;
            using (var cmd = new SqlCommand(@"
                SELECT LastSeenMilestone, BlockUntilUtc
                FROM dbo.GiftPopupState WITH (NOLOCK)
                WHERE UserName=@UserName;", conn))
            {
                cmd.Parameters.AddWithValue("@UserName", userName);
                using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                if (await r.ReadAsync())
                {
                    lastSeen = r.IsDBNull(0) ? null : r.GetInt32(0);
                    blockUntilUtc = r.IsDBNull(1) ? null : r.GetDateTime(1);
                }
            }

            // 3) spent credit (integratedTotal 불필요)
            int spentCreditCost = 0;
            int spentMilestone = 0;
            using (var cmd = new SqlCommand(@"
                SELECT
                    ISNULL(SUM(o.CreditCostSnapshot), 0) AS SpentCredit,
                    ISNULL(MAX(o.Milestone), 0)          AS SpentMilestone
                FROM dbo.StudentPointGiftOrder o WITH (NOLOCK)
                WHERE o.UserName = @UserName
                  AND o.Status IN ('PURCHASED', 'HIDDEN')
                  AND ISNULL(o.Memo,'') NOT LIKE N'가족팀:%'
                  AND ISNULL(o.Memo,'') NOT LIKE N'가족합산:%';", conn))
            {
                cmd.Parameters.AddWithValue("@UserName", userName);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    spentCreditCost = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                    spentMilestone  = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
                }
            }

            bool isStudent = await IsStudentAsync(userName);

            return new GiftEvalPreload
            {
                LastSeen       = lastSeen,
                BlockUntilUtc  = blockUntilUtc,
                SpentCreditCost = spentCreditCost,
                SpentMilestone  = spentMilestone,
                IsStudent       = isStudent,
            };
        }

        public async Task<GiftPopupEval> EvaluateAsync(string userName, int integratedTotal, GiftEvalPreload? preload = null)
        {
            var milestone = ToMilestone100(integratedTotal);
            if (milestone < 100)
                return new GiftPopupEval { Show = false, Milestone = milestone };

            // preload가 없으면 직접 조회 (하위 호환)
            if (preload == null)
                preload = await PreloadEvalStateAsync(userName);

            int? lastSeen      = preload.LastSeen;
            DateTime? blockUntilUtc = preload.BlockUntilUtc;
            int spentCreditCost = preload.SpentCreditCost;
            int spentMilestone  = preload.SpentMilestone;

            using var conn = Conn();
            await conn.OpenAsync();

            // 2) earned credit (milestone 필요 → 여기서 실행)
            int earned = 0;
            using (var cmd = new SqlCommand(@"
                SELECT ISNULL(SUM(Credit),0)
                FROM dbo.StudentPointMilestoneCredit WITH (NOLOCK)
                WHERE IsActive=1 AND MilestonePoint<=@M;", conn))
            {
                cmd.Parameters.AddWithValue("@M", milestone);
                DbLogger.LogDebugAsync($" 2) earned credit : {SqlDebugHelper.ToDebugSql(cmd)}");
                earned = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            var available = Math.Max(0, earned - spentCreditCost);

            // 4) affordable gift exists?
            bool hasGift = false;
            if (available > 0)
            {
                using var cmd = new SqlCommand(@"
                    SELECT TOP 1 1
                    FROM dbo.AwardItem WITH (NOLOCK)
                    WHERE ISNULL(IsHidden,0)=0 AND UnitPrice<=@C;", conn);
                cmd.Parameters.AddWithValue("@C", available);
                DbLogger.LogDebugAsync($" 4) affordable gift exists? : {SqlDebugHelper.ToDebugSql(cmd)}");
                var o = await cmd.ExecuteScalarAsync();
                hasGift = (o != null && o != DBNull.Value);
            }

            // 5) show condition
            bool blockedByDefer = blockUntilUtc.HasValue && blockUntilUtc.Value > DateHelper.GetKoreaNow();
            bool alreadySeenThisMilestone = lastSeen.HasValue && lastSeen.Value >= milestone;

            bool show =
                preload.IsStudent &&
                !blockedByDefer &&
                !alreadySeenThisMilestone &&
                available > 0 &&
                hasGift;

            return new GiftPopupEval
            {
                Show             = show,
                Milestone        = milestone,
                EarnedCredit     = earned,
                lastSeen         = lastSeen ?? 0,
                SpentCredit      = spentCreditCost,
                spentMilestone   = spentMilestone,
                AvailableCredit  = available,
                HasAffordableGift = preload.IsStudent && hasGift
            };
        }

        public async Task<List<int>> GetGiftMilestonePointsAsync()
        {
            var list = new List<int>();
            using var conn = Conn();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT DISTINCT MilestonePoint
                FROM dbo.StudentPointMilestoneCredit WITH (NOLOCK)
                WHERE IsActive=1 AND MilestonePoint > 0
                ORDER BY MilestonePoint;", conn);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) list.Add(Convert.ToInt32(r.GetValue(0)));
            return list;
        }

        public async Task<bool> IsStudentAsync(string userName)
        {
            if (string.Equals(userName, "test@gmail.com", StringComparison.OrdinalIgnoreCase))
                return true;

            using var conn = Conn();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(@"
                                                SELECT CASE 
                                                         WHEN RoleCode = @StudentCode THEN 1 
                                                         ELSE 0 
                                                       END
                                                FROM [dbo].[view_UserCompositeInfo_2026]
                                                WHERE UserName = @UserName;", conn);

            cmd.Parameters.AddWithValue("@UserName", userName);
            cmd.Parameters.AddWithValue("@StudentCode", RoleCodeType.T1001.ToString());
            DbLogger.LogDebugAsync($" 학생여부 : {SqlDebugHelper.ToDebugSql(cmd)}");
            var result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result) == 1;
        }

        public async Task SavePopupActionAsync(string userName, int milestone, string action)
        {
            // ✅ 팝업 버튼 액션을 GiftPopupState에 반영(통합 로직)
            // - later / accumulate / go_pick : 해당 milestone을 "봤다"로 처리하여 재팝업 방지
            // - defer3 / defer7             : BlockUntilUtc 설정(전체 팝업 차단)
            DateTime? blockUntilUtc = null;
            var nowUtc = DateHelper.GetKoreaNow();

            if (action == "later")
            {
                return;
            }

            if (string.Equals(action, "defer3", StringComparison.OrdinalIgnoreCase))
                blockUntilUtc = nowUtc.AddDays(3);
            else if (string.Equals(action, "defer7", StringComparison.OrdinalIgnoreCase))
                blockUntilUtc = nowUtc.AddDays(7);
            // 그 외(later/accumulate/go_pick)는 blockUntilUtc = null 유지

            using var conn = Conn();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
MERGE dbo.GiftPopupState AS t
USING (SELECT @UserName AS UserName) AS s
ON t.UserName = s.UserName
WHEN MATCHED THEN
  UPDATE SET
    LastSeenMilestone = CASE WHEN @Milestone > t.LastSeenMilestone THEN @Milestone ELSE t.LastSeenMilestone END,
    BlockUntilUtc = CASE 
                      WHEN @BlockUntilUtc IS NULL THEN t.BlockUntilUtc
                      WHEN t.BlockUntilUtc IS NULL THEN @BlockUntilUtc
                      WHEN @BlockUntilUtc > t.BlockUntilUtc THEN @BlockUntilUtc
                      ELSE t.BlockUntilUtc
                    END,
    UpdatedAtUtc = @UpdatedAtUtc
WHEN NOT MATCHED THEN
  INSERT (UserName, LastSeenMilestone, BlockUntilUtc, UpdatedAtUtc)
  VALUES (@UserName, @Milestone, @BlockUntilUtc, @UpdatedAtUtc);", conn);

            cmd.Parameters.AddWithValue("@UserName", userName);
            cmd.Parameters.AddWithValue("@Milestone", milestone);
            cmd.Parameters.Add("@BlockUntilUtc", SqlDbType.DateTime2).Value = (object?)blockUntilUtc ?? DBNull.Value;
            cmd.Parameters.Add("@UpdatedAtUtc", SqlDbType.DateTime2).Value = (object?)DateHelper.GetKoreaNow() ?? DBNull.Value;

            await DbLogger.LogDebugAsync($" Birth AwardItem : {SqlDebugHelper.ToDebugSql(cmd)}");

            await cmd.ExecuteNonQueryAsync();
        }

        // ✅ Controller → Service 단일 진입점(권장)
        public async Task HandlePopupActionAsync(string userName, int milestone, PopupActionType actionType)
        {
            if (string.IsNullOrWhiteSpace(userName) || milestone <= 0) return;

            var action = actionType switch
            {
                PopupActionType.ACCUMULATE => "accumulate",
                PopupActionType.LATER => "later",
                PopupActionType.DEFER3 => "defer3",
                PopupActionType.DEFER7 => "defer7",
                PopupActionType.GO_PICK => "go_pick",
                _ => "later"
            };

            // 기존 저장 로직 재사용
            await SavePopupActionAsync(userName, milestone, action);

            // (선택) 액션 로그
            try
            {
                using var conn = Conn();
                await conn.OpenAsync();
                using var cmd = new SqlCommand(@"
INSERT INTO dbo.GiftPopupActionLog(UserName, Milestone, ActionType, CreatedAtUtc)
VALUES (@UserName, @Milestone, @ActionType, @CreatedAtUtc);", conn);
                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@Milestone", milestone);
                cmd.Parameters.AddWithValue("@ActionType", actionType.ToString());
                cmd.Parameters.AddWithValue("@CreatedAtUtc", DateHelper.GetKoreaNow());

                DbLogger.LogDebugAsync($" Birth AwardItem : {SqlDebugHelper.ToDebugSql(cmd)}");

                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // 로그 실패는 기능에 영향 없도록 무시
            }
        }

        public async Task<List<GiftItemVM>> GetGiftListAsync(int availableCredit)
        {
            // 🔁 품목 조회는 GiftCatalog 한 곳에서 (숨김 제외 · 금액 이하 · 금액 오름차순)
            var items = await _giftCatalog.GetAsync(
                BulletinBoard.Services.GiftCatalog.GiftCatalogQuery.ByLimit(availableCredit));

            return items.Select(a => new GiftItemVM
            {
                ProductID          = a.ProductID,
                ProductName        = a.ProductName,
                ProductDescription = a.ProductDescription,
                UnitPrice          = a.UnitPrice
            }).ToList();
        }

        public async Task<GiftBoxVM> GetMyGiftBoxAsync(string userName, int integratedTotal)
        {
            var eval = await EvaluateAsync(userName, integratedTotal);

            var vm = new GiftBoxVM
            {
                IntegratedTotal = integratedTotal,
                Milestone = eval.Milestone,
                SpentMilestone = eval.spentMilestone,
                EarnedCredit = eval.EarnedCredit,
                SpentCredit = eval.SpentCredit,
                AvailableCredit = eval.AvailableCredit
            };

            using var conn = Conn();
            await conn.OpenAsync();

            // current draft
            using (var cmd = new SqlCommand(@"
                                                SELECT TOP 1 o.OrderId, o.ProductID, a.ProductName, a.ProductDescription, o.CreditCostSnapshot, o.Status, o.CreatedAt, o.PurchasedAt, isnull(c.milestone,0) milestone
                                                FROM dbo.StudentPointGiftOrder o WITH (NOLOCK)
                                                JOIN dbo.AwardItem a WITH (NOLOCK) ON a.ProductID=o.ProductID
                                                LEFT OUTER JOIN [dbo].[GiftPopupActionLog] c WITH (NOLOCK) ON c.Id =o.OrderId
                                                WHERE o.UserName=@U AND o.Status='DRAFT' AND o.IsCurrentDraft=1
                                                  AND ISNULL(o.Memo,'') NOT LIKE N'가족합산:%'
                                                ORDER BY o.OrderId DESC;", conn))
            {
                cmd.Parameters.AddWithValue("@U", userName);

                DbLogger.LogDebugAsync($" current draft gift {SqlDebugHelper.ToDebugSql(cmd)}");

                using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                if (await r.ReadAsync())
                {
                    vm.CurrentDraft = new GiftOrderVM
                    {
                        OrderId = r.GetInt32(0),
                        ProductID = r.GetInt32(1),
                        ProductName = r.GetString(2),
                        ProductDescription = r.GetString(3),
                        CreditCostSnapshot = r.GetInt32(4),
                        Status = r.GetString(5),
                        CreatedAt = r.GetDateTime(6),
                        PurchasedAt = r.IsDBNull(7) ? null : r.GetDateTime(7),
                        Milestone = r.GetInt32(8)
                    };
                }
            }

            // history
            using (var cmd = new SqlCommand(@"
                                            SELECT TOP 50 o.OrderId, o.ProductID, a.ProductName, o.CreditCostSnapshot, o.Status, o.CreatedAt, o.PurchasedAt, o.milestone
                                            FROM dbo.StudentPointGiftOrder o WITH (NOLOCK)
                                            JOIN dbo.AwardItem a WITH (NOLOCK) ON a.ProductID=o.ProductID
                                            WHERE o.UserName=@U
                                              AND NOT (o.Status='DRAFT' AND o.IsCurrentDraft=1)
                                              AND ISNULL(o.Memo,'') NOT LIKE N'가족팀:%'
                                              AND ISNULL(o.Memo,'') NOT LIKE N'가족합산:%'
                                            ORDER BY o.OrderId DESC;", conn))
            {
                cmd.Parameters.AddWithValue("@U", userName);

                DbLogger.LogDebugAsync($" 재선택가능선물 : {SqlDebugHelper.ToDebugSql(cmd)}");

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.History.Add(new GiftOrderVM
                    {
                        OrderId = r.GetInt32(0),
                        ProductID = r.GetInt32(1),
                        ProductName = r.GetString(2),
                        CreditCostSnapshot = r.GetInt32(3),
                        Status = r.GetString(4),
                        CreatedAt = r.GetDateTime(5),
                        PurchasedAt = r.IsDBNull(6) ? null : r.GetDateTime(6),
                        Milestone = r.GetInt32(7)
                    });
                }
            }

            // affordable gifts
            if (vm.AvailableCredit > 0)
                vm.AffordableGifts = await GetGiftListAsync(vm.AvailableCredit);

            return vm;
        }

        public async Task SelectGiftAsync(string userName, int productId, int integratedTotal)
        {
            // 가격 snapshot (UnitPrice)
            int price;
            using (var conn = Conn())
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 UnitPrice
FROM dbo.AwardItem WITH (NOLOCK)
WHERE ProductID=@P AND ISNULL(IsHidden,0)=0;", conn))
            {
                cmd.Parameters.AddWithValue("@P", productId);
                await conn.OpenAsync();

                await DbLogger.LogDebugAsync($" Birth AwardItem : {SqlDebugHelper.ToDebugSql(cmd)}");

                var o = await cmd.ExecuteScalarAsync();
                if (o == null || o == DBNull.Value) throw new InvalidOperationException("상품이 없거나 숨김 처리되었습니다.");
                price = Convert.ToInt32(o);
            }

            // availableCredit 체크
            var eval = await EvaluateAsync(userName, integratedTotal);
            if (eval.AvailableCredit < price)
                throw new InvalidOperationException("잔여 credit이 부족합니다.");

            using var conn2 = Conn();
            await conn2.OpenAsync();
            using var tx = conn2.BeginTransaction();

            // ✅ 재선택 시: 기존 선택 이력(현재 draft 및 취소류)을 DB에서 삭제
            //    - 요구사항: "재선택으로 취소"는 남기지 않고, 재선택하면 기존 선택 이력 모두 삭제
            using (var cmd = new SqlCommand(@"
DELETE FROM dbo.StudentPointGiftOrder
WHERE UserName=@U
  AND ISNULL(Status,'') IN ('DRAFT','CANCELED_BY_RESELECT','CANCELED_BY_UPGRADE')
  AND ISNULL(Memo,'') NOT LIKE N'가족합산:%';", conn2, tx))
            {
                cmd.Parameters.AddWithValue("@U", userName);

                DbLogger.LogDebugAsync($" Birth AwardItem : {SqlDebugHelper.ToDebugSql(cmd)}");

                await cmd.ExecuteNonQueryAsync();
            }

            // 새 draft 생성 (유효 1개)
            using (var cmd = new SqlCommand(@"
INSERT dbo.StudentPointGiftOrder(UserName, ProductID, CreditCostSnapshot, MileStone,  Status, IsCurrentDraft,[CreatedAt])
VALUES(@U,@P,@C, @MileStone, 'DRAFT',1,@CreatedAt);", conn2, tx))
            {
                cmd.Parameters.AddWithValue("@U", userName);
                cmd.Parameters.AddWithValue("@P", productId);
                cmd.Parameters.AddWithValue("@C", eval.AvailableCredit); // ✅ 구매 시 잔액이 남지 않도록, 현재 사용가능한 credit 전체를 소멸 처리
                cmd.Parameters.AddWithValue("@MileStone", eval.Milestone);
                cmd.Parameters.AddWithValue("@CreatedAt", DateHelper.GetKoreaNow());

                DbLogger.LogDebugAsync($" 새 draft 생성 (유효 1개) : {SqlDebugHelper.ToDebugSql(cmd)}");

                await cmd.ExecuteNonQueryAsync();
            }

            tx.Commit();
        }

        /// <summary>
        /// 내 선물함에서 현재 선택한 선물(DRAFT)을 취소합니다.
        /// 취소 = 완전 삭제(이력 남기지 않음).
        /// </summary>
        public async Task CancelSelectedGiftAsync(string userName, int orderId)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("userName is required.", nameof(userName));
            if (orderId <= 0) throw new ArgumentOutOfRangeException(nameof(orderId));

            using var conn = Conn();
            await conn.OpenAsync();

            // ✅ 본인 주문 + DRAFT(구매 전)만 삭제 허용
            using var cmd = new SqlCommand(@"
DELETE FROM dbo.StudentPointGiftOrder
WHERE OrderId=@Id
  AND UserName=@U
  AND Status='DRAFT'
  AND ISNULL(IsCurrentDraft,0)=1;", conn);

            cmd.Parameters.AddWithValue("@Id", orderId);
            cmd.Parameters.AddWithValue("@U", userName);

            DbLogger.LogDebugAsync($" CancelSelectedGift : {SqlDebugHelper.ToDebugSql(cmd)}");

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<AdminPurchaseRowVM>> GetAdminPurchaseListAsync(bool includeHidden = false)
        {
            var list = new List<AdminPurchaseRowVM>();
            using var conn = Conn();

            // ✅ includeHidden=true(Admin/SuperAdmin)이면 HIDDEN도 포함, 아니면 제외
            var whereClause = includeHidden
                ? "WHERE ISNULL(o.Status,'') NOT IN ('CANCELED_BY_RESELECT')"
                : "WHERE ISNULL(o.Status,'') NOT IN ('HIDDEN','CANCELED_BY_RESELECT')";

            using var cmd = new SqlCommand($@"
SELECT
    o.OrderId,
    o.UserName,
    LTRIM(RTRIM(ISNULL(u.LastName,'') + ' ' + ISNULL(u.FirstName,''))) AS UserFullName,
    u.Id AS UserId,
    o.ProductID,
    a.ProductName,
    a.[UnitPrice],
    a.ProductDescription,
    o.CreditCostSnapshot,
    o.CreatedAt,
    o.Status,
    uc.RoleCode,
    uc.ClassYear,
    uc.ClassBan,
    o.PurchasedAt,
    o.Memo,
    CASE WHEN o.MileStone=0 THEN 1000 ELSE o.MileStone END AS MileStoneVal,
    o.DeliveredAt
FROM dbo.StudentPointGiftOrder o WITH (NOLOCK)
JOIN dbo.AwardItem a WITH (NOLOCK) ON a.ProductID=o.ProductID
LEFT JOIN view_UserCompositeInfo_2026 uc ON uc.UserName = o.UserName
LEFT JOIN [Identity].[User] u WITH (NOLOCK) ON u.UserName = o.UserName
{whereClause}
ORDER BY
    CASE
        WHEN o.Status='DRAFT' THEN 0
        WHEN o.Status='PURCHASED' THEN 1
        WHEN o.Status='HIDDEN' THEN 3
        ELSE 2
    END,
    o.CreatedAt ASC;", conn);

            await conn.OpenAsync();
            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new AdminPurchaseRowVM
                {
                    OrderId = r.GetInt32(0),
                    UserName = r.GetString(1),
                    UserFullName = r.IsDBNull(2) ? null : r.GetString(2),
                    UserId = r.IsDBNull(3) ? null : r.GetString(3),

                    ProductID = r.GetInt32(4),
                    ProductName = r.GetString(5),
                    UnitPrice = r.GetInt32(6),
                    ProductDescription = r.GetString(7),
                    CreditCostSnapshot = r.GetInt32(8),
                    CreatedAt = r.GetDateTime(9),

                    // ✅ 추가
                    Status = r.IsDBNull(10) ? "" : r.GetString(10),
                    RoleCode = r.IsDBNull(11) ? "" : r.GetString(11),
                    ClassYear = r.IsDBNull(12) ? (int?)null : r.GetInt32(12),
                    ClassBan = r.IsDBNull(13) ? (int?)null : r.GetInt32(13),
                    PurchasedAt = r.IsDBNull(14) ? (DateTime?)null : r.GetDateTime(14),
                    Memo          = r.IsDBNull(15) ? null : r.GetString(15),
                    MileStoneVal  = r.IsDBNull(16) ? 0 : r.GetInt32(16),
                    DeliveredAt   = r.IsDBNull(17) ? (DateTime?)null : r.GetDateTime(17)
                });
            }

            // ✅ 사용자 프로필 사진 + 최종 도토리(통합) 포인트 계산
            try
            {
                var year = DateHelper.GetKoreaNow().Year;

                var userKeys = list
                    .Where(x => x != null && !string.IsNullOrWhiteSpace(x.UserName))
                    .Select(x => new { x.UserName, x.UserId })
                    .Distinct()
                    .ToList();

                var cache = new Dictionary<string, (int bible, int mission, int thanks, int attendance, int evangelism, int total, string photo)>(StringComparer.OrdinalIgnoreCase);

                foreach (var k in userKeys)
                {
                    var key = (k.UserName ?? "").ToLowerInvariant();
                    if (cache.ContainsKey(key)) continue;

                    var photo = BuildProfileUrl(k.UserId);

                    // 각 점수는 개별 테이블/함수에서 계산 (대시보드와 동일 규칙)
                    int bible = 0, mission = 0, thanks = 0, attendance = 0, evangelism = 0;

                    try { bible = await GetBiblePointsThisYearAsync(k.UserName, year); } catch { bible = 0; }
                    try { mission = await GetMissionBookPointAsync(k.UserId); } catch { mission = 0; }
                    try { thanks = await GetThanksPointTotalAsync(k.UserId); } catch { thanks = 0; }
                    try { attendance = await GetAttendancePointAsync(k.UserId); } catch { attendance = 0; }
                    try { evangelism = await _attendanceService.GetEvangelismCountByUserIdAsync(k.UserId ?? ""); } catch { evangelism = 0; }

                    var total = bible + mission + thanks + attendance + (evangelism * 50);
                    cache[key] = (bible, mission, thanks, attendance, evangelism, total, photo);
                }

                foreach (var row in list)
                {
                    if (row == null) continue;

                    var key = (row.UserName ?? "").ToLowerInvariant();
                    if (!cache.TryGetValue(key, out var v)) continue;

                    row.UserPhotoUrl = v.photo;
                    row.BiblePoint = v.bible;
                    row.MissionBookPoint = v.mission;
                    row.ThanksPointTotal = v.thanks;
                    row.AttendancePoint = v.attendance;
                    row.IntegratedTotal = v.total;
                }
            }
            catch
            {
                // 점수/프로필은 부가 정보이므로 실패해도 목록은 표시
            }

            // 가족합산 선물 → 가족 명단 조회
            try
            {
                var familyItems = list.Where(x => x.IsFamilyCombinedGift).ToList();
                if (familyItems.Any())
                {
                    using var conn2 = Conn();
                    await conn2.OpenAsync();
                    foreach (var item in familyItems)
                    {
                        var teamName = item.Memo!.Substring("가족합산:".Length).Trim();
                        using var cmd2 = new SqlCommand(@"
SELECT LTRIM(RTRIM(ISNULL(u.LastName,'') + ' ' + ISNULL(u.FirstName,'')))
FROM dbo.DM2_FamilyTeam ft WITH (NOLOCK)
JOIN dbo.DM2_FamilyMember fm WITH (NOLOCK) ON fm.FamilyTeamId = ft.Id
LEFT JOIN [Identity].[User] u WITH (NOLOCK) ON u.UserName = fm.UserName
WHERE ft.TeamName = @TeamName
ORDER BY u.LastName, u.FirstName;", conn2);
                        cmd2.Parameters.AddWithValue("@TeamName", teamName);
                        using var r2 = await cmd2.ExecuteReaderAsync();
                        while (await r2.ReadAsync())
                        {
                            if (!r2.IsDBNull(0))
                                item.FamilyMemberNames.Add(r2.GetString(0).Trim());
                        }
                    }
                }
            }
            catch { /* 가족 명단은 부가 정보 */ }

            return list;
        }

        public async Task CompletePurchaseAsync(int orderId, string adminId)
        {
            using var conn = Conn();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
UPDATE dbo.StudentPointGiftOrder
SET Status='PURCHASED', PurchasedAt = @PurchasedAt, AdminId=@A, IsCurrentDraft=0
WHERE OrderId=@Id AND Status='DRAFT';", conn);
            cmd.Parameters.AddWithValue("@Id", orderId);
            cmd.Parameters.AddWithValue("@A", adminId);
            cmd.Parameters.AddWithValue("@PurchasedAt", DateHelper.GetKoreaNow());

            DbLogger.LogDebugAsync($" Birth AwardItem : {SqlDebugHelper.ToDebugSql(cmd)}");

            var n = await cmd.ExecuteNonQueryAsync();
            if (n == 0) throw new InvalidOperationException("이미 처리되었거나 대상이 아닙니다.");
        }

        /// <summary>구매 완료 되돌리기 — 다시 '고른 상태(DRAFT)'로 (잘못 눌렀을 때)</summary>
        public async Task UndoPurchaseAsync(int orderId, string adminId)
        {
            using var conn = Conn();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
UPDATE dbo.StudentPointGiftOrder
SET Status='DRAFT', PurchasedAt = NULL, AdminId = @A, IsCurrentDraft = 1
WHERE OrderId = @Id AND Status = 'PURCHASED';", conn);
            cmd.Parameters.AddWithValue("@Id", orderId);
            cmd.Parameters.AddWithValue("@A", adminId ?? "");

            DbLogger.LogDebugAsync($" 구매 완료 취소 : {SqlDebugHelper.ToDebugSql(cmd)}");

            var n = await cmd.ExecuteNonQueryAsync();
            if (n == 0) throw new InvalidOperationException("구매 완료 상태가 아니거나 대상이 없습니다.");
        }

        // ─────────────────────────────────────────────────────────────
        // ✅ 팀 선물 받은 학생 UserName 목록
        // ─────────────────────────────────────────────────────────────
        public async Task CancelTeamGiftAsync(string studentUserName, int milestone)
        {
            if (string.IsNullOrWhiteSpace(studentUserName)) throw new ArgumentException("studentUserName required");

            const string sql = @"
DELETE FROM dbo.StudentPointGiftOrder
WHERE UserName  = @U
  AND CASE WHEN MileStone=0 THEN 1000 ELSE MileStone END = @MS
  AND Status    = 'HIDDEN'
  AND Memo LIKE N'가족팀:%';";

            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U",  studentUserName);
            cmd.Parameters.AddWithValue("@MS", milestone);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<HashSet<string>> GetMyFamilyStudentNamesAsync(string userName)
        {
            // 내가 속한 가족팀의 학생(RoleCode=0) UserName 목록
            const string sql = @"
SELECT DISTINCT m2.UserName
FROM dbo.DM2_FamilyMember m1 WITH (NOLOCK)
JOIN dbo.DM2_FamilyMember m2 WITH (NOLOCK)
    ON  m2.FamilyTeamId = m1.FamilyTeamId
   AND  m2.IsActive = 1
   AND  m2.RoleCode = 0   -- 학생만
WHERE m1.UserName = @U AND m1.IsActive = 1;";

            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U", userName);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var u = r.IsDBNull(0) ? null : r.GetString(0);
                if (!string.IsNullOrWhiteSpace(u)) set.Add(u);
            }
            // 본인도 추가 (본인이 학생인 경우)
            set.Add(userName);
            return set;
        }

        public async Task<FamilyTeamGiftVM?> GetFamilyTeamGiftStatusAsync(string myUserName, int year, int targetPoint)
        {
            // Step 1: 내 가족팀 구성원 목록 + 팀 이름 조회
            const string sqlMembers = @"
SELECT DISTINCT
    m2.UserName,
    ISNULL(pm.FullName, m2.UserName) AS FullName,
    ui.ProfilePicture,
    t.TeamName,
    t.FamilyTeamId,
    ISNULL(m2.RelationCode, 9) AS RelationCode,
    ISNULL(m2.RelationLabel, '') AS RelationLabel
FROM dbo.DM2_FamilyMember m1 WITH (NOLOCK)
JOIN dbo.DM2_FamilyTeam t WITH (NOLOCK) ON t.FamilyTeamId = m1.FamilyTeamId AND t.IsActive = 1
JOIN dbo.DM2_FamilyMember m2 WITH (NOLOCK) ON m2.FamilyTeamId = m1.FamilyTeamId AND m2.IsActive = 1
JOIN [Identity].[User] u WITH (NOLOCK) ON u.UserName = m2.UserName
LEFT JOIN dbo.view_PersonMaster_2026 pm WITH (NOLOCK) ON pm.UserId = u.Id
LEFT JOIN dbo.view_UserCompositeInfo_2026 ui WITH (NOLOCK) ON ui.UserId = u.Id
WHERE m1.UserName = @MyUser AND m1.IsActive = 1;
";
            var memberRows = new List<(string UserName, string FullName, string? Photo, string TeamName, int FamilyTeamId, int RelationCode, string RelationLabel)>();
            using (var conn = Conn())
            using (var cmd  = new SqlCommand(sqlMembers, conn))
            {
                cmd.Parameters.AddWithValue("@MyUser", myUserName);
                await conn.OpenAsync();
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var photo = r["ProfilePicture"] == DBNull.Value ? null : r["ProfilePicture"]?.ToString();
                    memberRows.Add((
                        UserName:      r["UserName"]?.ToString() ?? "",
                        FullName:      r["FullName"]?.ToString() ?? "",
                        Photo:         string.IsNullOrWhiteSpace(photo) ? "/images/icon/people/people_01.webp" : photo,
                        TeamName:      r["TeamName"]?.ToString() ?? "",
                        FamilyTeamId:  Convert.ToInt32(r["FamilyTeamId"]),
                        RelationCode:  Convert.ToInt32(r["RelationCode"]),
                        RelationLabel: r["RelationLabel"]?.ToString() ?? ""
                    ));
                }
            }

            if (memberRows.Count == 0) return null;

            var teamName     = memberRows[0].TeamName;
            var familyTeamId = memberRows[0].FamilyTeamId;

            // Step 2: 각 구성원의 DM2 포인트만 계산 (합산 기준)
            var members = new List<FamilyMemberGiftRow>();
            foreach (var m in memberRows)
            {
                int dm2 = 0;
                try { dm2 = await GetBiblePointsThisYearAsync(m.UserName, year); } catch { }
                members.Add(new FamilyMemberGiftRow
                {
                    UserName      = m.UserName,
                    FullName      = m.FullName,
                    Dm2Point      = dm2,
                    Photo         = m.Photo ?? "/images/icon/people/people_01.webp",
                    RelationCode  = m.RelationCode,
                    RelationLabel = m.RelationLabel,
                });
            }

            int teamTotal = members.Sum(x => x.Dm2Point);
            if (teamTotal < targetPoint) return null;  // 1000점 미만이면 null

            // 선물 수령자: RelationCode=0(본인) 우선, 없으면 첫 번째 멤버
            var topScorer = members.FirstOrDefault(x => x.RelationCode == 0)
                         ?? members.First();

            // Step 3: 도달한 '선물' 단계(1000 · 3000 …)를 낮은 것부터 보며,
            //         아직 전달(HIDDEN)되지 않은 첫 단계를 고른다.  전부 전달됐으면 마지막 단계(HIDDEN)를 돌려 화면이 감추게 한다.
            //         (예전엔 1000점 열쇠만 봐서, 1000점 선물을 전달한 뒤 3000점에 닿아도 배너가 다시 뜨지 않았다)
            var thresholds = new List<int>();
            const string sqlCfg = @"
SELECT PointThreshold
FROM dbo.DM2_FamilyMilestoneConfig WITH (NOLOCK)
WHERE Year = @Year AND IsActive = 1 AND MilestoneType = 'AWARD'
ORDER BY PointThreshold;";
            using (var conn = Conn())
            using (var cmd  = new SqlCommand(sqlCfg, conn))
            {
                cmd.Parameters.AddWithValue("@Year", year);
                await conn.OpenAsync();
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync()) thresholds.Add(r.GetInt32(0));
            }
            if (thresholds.Count == 0) thresholds.Add(targetPoint);
            var reached = thresholds.Where(t => t <= teamTotal).OrderBy(t => t).ToList();
            if (reached.Count == 0) return null;

            const string sqlGift = @"
SELECT TOP 1 o.OrderId, o.Status, o.ProductId, ai.ProductName, ai.ProductDescription
FROM dbo.StudentPointGiftOrder o WITH (NOLOCK)
LEFT JOIN dbo.AwardItem ai WITH (NOLOCK) ON ai.ProductID = o.ProductId
WHERE o.Memo = @MemoKey
  AND o.Status IN ('DRAFT','PURCHASED','HIDDEN')
ORDER BY CASE o.Status WHEN 'DRAFT' THEN 1 WHEN 'PURCHASED' THEN 2 ELSE 3 END;
";
            int threshold = reached[^1];
            string? giftStatus = null;
            int orderId = 0;
            int? productId = null;
            string? productName = null;
            string? productDesc = null;

            foreach (var t in reached)
            {
                string? st = null; int oid = 0; int? pid = null; string? pn = null, pd = null;
                using (var conn = Conn())
                using (var cmd  = new SqlCommand(sqlGift, conn))
                {
                    cmd.Parameters.AddWithValue("@MemoKey", MilestoneGiftMemoKey(teamName, t));
                    await conn.OpenAsync();
                    using var r = await cmd.ExecuteReaderAsync();
                    if (await r.ReadAsync())
                    {
                        oid = r.GetInt32(0);
                        st  = r.IsDBNull(1) ? null : r.GetString(1);
                        pid = r.IsDBNull(2) ? (int?)null : r.GetInt32(2);
                        pn  = r.IsDBNull(3) ? null : r.GetString(3);
                        pd  = r.IsDBNull(4) ? null : r.GetString(4);
                    }
                }
                threshold = t; giftStatus = st; orderId = oid; productId = pid; productName = pn; productDesc = pd;
                if (!string.Equals(st, "HIDDEN", StringComparison.OrdinalIgnoreCase)) break;   // 아직 안 끝난 단계 → 이걸 보여 준다
            }

            return new FamilyTeamGiftVM
            {
                FamilyTeamId       = familyTeamId,
                TeamName           = teamName,
                TeamTotalDm2       = teamTotal,
                TopScorerUserName  = topScorer.UserName,
                TopScorerFullName  = topScorer.FullName,
                TopScorerPhoto     = topScorer.Photo,
                Members            = members.OrderBy(x => x.RelationCode).ThenByDescending(x => x.Dm2Point).ToList(),
                Threshold          = threshold,
                GiftStatus         = giftStatus,
                OrderId            = orderId,
                ProductId          = productId,
                ProductName        = productName,
                ProductDescription = productDesc,
            };
        }

        public async Task SelectFamilyGiftAsync(string topScorerUserName, int productId, int teamTotalDm2, string teamName)
        {
            if (string.IsNullOrWhiteSpace(topScorerUserName)) throw new ArgumentException("topScorerUserName required");
            string memoKey = $"가족합산:{teamName}";

            // 기존 DRAFT가 있으면 삭제 후 새로 삽입 (변경 허용)
            const string sql = @"
DELETE FROM dbo.StudentPointGiftOrder
WHERE Memo = @Memo AND Status = 'DRAFT';

INSERT INTO dbo.StudentPointGiftOrder
    (UserName, ProductId, CreditCostSnapshot, Status, CreatedAt, IsCurrentDraft, Memo, MileStone)
VALUES
    (@UserName, @ProductId, @TeamTotal, 'DRAFT', GETDATE(), 0, @Memo, 1000);
";
            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserName",  topScorerUserName);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@TeamTotal", teamTotalDm2);
            cmd.Parameters.AddWithValue("@Memo",      memoKey);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Dictionary<string, (int ProductId, string ProductName, string Status)>> GetFamilyCombinedGiftStatusAllAsync()
        {
            var result = new Dictionary<string, (int, string, string)>(StringComparer.OrdinalIgnoreCase);
            const string sql = @"
SELECT REPLACE(o.Memo, N'가족합산:', '') AS TeamName,
       o.ProductID,
       a.ProductName,
       o.Status
FROM dbo.StudentPointGiftOrder o WITH (NOLOCK)
JOIN dbo.AwardItem a WITH (NOLOCK) ON a.ProductID = o.ProductID
WHERE o.Memo LIKE N'가족합산:%'
  AND ISNULL(o.Status,'') IN ('DRAFT','PURCHASED','HIDDEN')
ORDER BY o.CreatedAt DESC;";
            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var teamName2  = r.IsDBNull(0) ? "" : r.GetString(0).Trim();
                var pid        = r.GetInt32(1);
                var pname      = r.IsDBNull(2) ? "" : r.GetString(2);
                var status     = r.IsDBNull(3) ? "" : r.GetString(3);
                if (!string.IsNullOrWhiteSpace(teamName2) && !result.ContainsKey(teamName2))
                    result[teamName2] = (pid, pname, status);
            }
            return result;
        }

        public async Task<List<EligibleFamilyRowVM>> GetAllEligibleFamilyTeamsAsync(int year, int targetPoint)
        {
            // Step 1: 모든 활성 가족팀 구성원 조회
            const string sqlMembers = @"
SELECT t.FamilyTeamId, t.TeamName,
       m.UserName, ISNULL(m.RelationCode,9) AS RelationCode,
       ISNULL(pm.FullName, m.UserName) AS FullName,
       u.Id AS UserId,
       ISNULL(m.RelationLabel, '') AS RelationLabel
FROM dbo.DM2_FamilyTeam t WITH (NOLOCK)
JOIN dbo.DM2_FamilyMember m WITH (NOLOCK) ON m.FamilyTeamId = t.FamilyTeamId AND m.IsActive = 1
JOIN [Identity].[User] u WITH (NOLOCK) ON u.UserName = m.UserName
LEFT JOIN dbo.view_PersonMaster_2026 pm WITH (NOLOCK) ON pm.UserId = u.Id
WHERE t.IsActive = 1
ORDER BY t.FamilyTeamId, m.RelationCode;";

            var teamMemberMap = new Dictionary<int, (string TeamName, List<(string UserName, string FullName, string Photo, int RelationCode, string RelationLabel)> Members)>();

            using (var conn = Conn())
            using (var cmd = new SqlCommand(sqlMembers, conn))
            {
                await conn.OpenAsync();
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    int tid      = r.GetInt32(0);
                    var tname    = r.IsDBNull(1) ? "" : r.GetString(1);
                    var uname    = r.IsDBNull(2) ? "" : r.GetString(2);
                    int rel      = r.IsDBNull(3) ? 9 : Convert.ToInt32(r[3]);
                    var full     = r.IsDBNull(4) ? uname : r.GetString(4);
                    var userId   = r.IsDBNull(5) ? null : r.GetString(5);
                    var relLabel = r.IsDBNull(6) ? "" : r.GetString(6);
                    var photo    = string.IsNullOrWhiteSpace(userId)
                        ? "/images/icon/people/people_01.webp"
                        : $"/HumanResource/GetProfilePicture?userId={Uri.EscapeDataString(userId)}";

                    if (!teamMemberMap.ContainsKey(tid))
                        teamMemberMap[tid] = (tname, new List<(string, string, string, int, string)>());
                    teamMemberMap[tid].Members.Add((uname, full, photo, rel, relLabel));
                }
            }

            if (teamMemberMap.Count == 0) return new List<EligibleFamilyRowVM>();

            // Step 2: 기존 가족합산 선물 현황 일괄 조회
            var giftMap = await GetFamilyCombinedGiftStatusAllAsync();

            // Step 3: 각 팀 DM2 합산 계산 후 eligible 팀 구성
            var from = new DateTime(year, 1, 1);
            var to   = DateHelper.GetKoreaNow().Date;
            if (to.Year != year) to = new DateTime(year, 12, 31);

            var result = new List<EligibleFamilyRowVM>();

            foreach (var (tid, (teamName, members)) in teamMemberMap)
            {
                int teamTotal = 0;
                foreach (var m in members)
                {
                    if (!string.IsNullOrWhiteSpace(m.UserName))
                    {
                        try { teamTotal += await GetBiblePointsThisYearAsync(m.UserName, year); } catch { }
                    }
                }
                if (teamTotal < targetPoint) continue;

                var recipient = members.FirstOrDefault(x => x.RelationCode == 0);
                if (recipient == default) recipient = members[0];

                string? giftStatus  = null;
                int?    productId   = null;
                string? productName = null;
                if (giftMap.TryGetValue(teamName, out var g))
                {
                    giftStatus  = g.Status;
                    productId   = g.ProductId;
                    productName = g.ProductName;
                }

                result.Add(new EligibleFamilyRowVM
                {
                    FamilyTeamId       = tid,
                    TeamName           = teamName,
                    TeamTotalDm2       = teamTotal,
                    RecipientFullName  = recipient.FullName,
                    RecipientPhoto     = recipient.Photo,
                    RecipientUserName  = recipient.UserName,
                    GiftStatus         = giftStatus,
                    ProductId          = productId,
                    ProductName        = productName,
                    Members            = members.Select(x => (x.FullName, x.RelationLabel)).ToList(),
                });
            }

            return result.OrderByDescending(x => x.TeamTotalDm2).ToList();
        }

        public async Task CancelFamilyGiftAsync(string topScorerUserName, string teamName)
        {
            string memoKey = $"가족합산:{teamName}";
            const string sql = @"
DELETE FROM dbo.StudentPointGiftOrder
WHERE Memo = @Memo AND Status = 'DRAFT';
";
            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Memo", memoKey);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Dictionary<string, Dictionary<int,(int ProductId, string ProductName)>>> GetTeamGiftedInfoAsync()
        {
            // UserName → { Milestone → (ProductId, ProductName) }
            // MileStone=0인 구버전 레코드는 1000점으로 처리
            const string sql = @"
WITH ranked AS (
    SELECT o.UserName,
           CASE WHEN o.MileStone = 0 THEN 1000 ELSE o.MileStone END AS MileStone,
           o.ProductID,
           ISNULL(a.ProductName, '') AS ProductName,
           ROW_NUMBER() OVER (
               PARTITION BY o.UserName,
                            CASE WHEN o.MileStone = 0 THEN 1000 ELSE o.MileStone END
               ORDER BY o.OrderId DESC
           ) AS rn
    FROM dbo.StudentPointGiftOrder o WITH (NOLOCK)
    LEFT JOIN dbo.AwardItem a WITH (NOLOCK) ON a.ProductID = o.ProductID
    WHERE o.Status = 'HIDDEN' AND o.Memo LIKE N'가족팀:%'
)
SELECT UserName, MileStone, ProductID, ProductName
FROM ranked WHERE rn = 1;";

            var result = new Dictionary<string, Dictionary<int,(int,string)>>(StringComparer.OrdinalIgnoreCase);
            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var u  = r.IsDBNull(0) ? null : r.GetString(0);
                if (string.IsNullOrWhiteSpace(u)) continue;
                var ms = r.GetInt32(1);
                var pid = r.GetInt32(2);
                var pname = r.IsDBNull(3) ? "" : r.GetString(3);
                if (!result.ContainsKey(u))
                    result[u] = new Dictionary<int,(int,string)>();
                result[u][ms] = (pid, pname);
            }
            return result;
        }

        // ─────────────────────────────────────────────────────────────
        // ✅ 가족팀 1000점 달성 선물 즉시 지급 (Status = HIDDEN)
        // ─────────────────────────────────────────────────────────────
        public async Task InsertTeamGiftAsync(string studentUserName, int productId, int unitPrice, string teamName, string adminId, int milestone)
        {
            if (string.IsNullOrWhiteSpace(studentUserName)) throw new ArgumentException("studentUserName required");

            // ✅ UPSERT: (UserName, MileStone) 조합이 이미 있으면 UPDATE, 없으면 INSERT
            //    → 재지급해도 중복 레코드가 생기지 않음
            // MileStone=0은 구버전 레코드 → 1000으로 정규화하여 매칭
            const string sql = @"
MERGE dbo.StudentPointGiftOrder AS tgt
USING (SELECT @U AS UserName, @MS AS MileStone) AS src
    ON tgt.UserName = src.UserName
   AND CASE WHEN tgt.MileStone=0 THEN 1000 ELSE tgt.MileStone END = src.MileStone
   AND tgt.Status   = 'HIDDEN'
   AND tgt.Memo LIKE N'가족팀:%'
WHEN MATCHED THEN
    UPDATE SET
        ProductID          = @P,
        CreditCostSnapshot = @C,
        MileStone          = @MS,   -- 구버전 0 → 정규화된 값으로 갱신
        AdminId            = @A,
        Memo               = @M,
        PurchasedAt        = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (UserName, ProductID, CreditCostSnapshot, Status, IsCurrentDraft, MileStone, AdminId, Memo, CreatedAt, PurchasedAt)
    VALUES (@U, @P, @C, 'HIDDEN', 0, @MS, @A, @M, GETDATE(), GETDATE());";

            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U",  studentUserName);
            cmd.Parameters.AddWithValue("@P",  productId);
            cmd.Parameters.AddWithValue("@C",  0);            // ✅ 가족팀 선물은 개인 도토리 소비 없음 — 항상 0
            cmd.Parameters.AddWithValue("@MS", milestone);
            cmd.Parameters.AddWithValue("@A",  adminId ?? "");
            cmd.Parameters.AddWithValue("@M",  $"가족팀: {teamName}");

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // 마일스톤 시스템 (DB 기반 동적 설정)
        // ═══════════════════════════════════════════════════════════

        public async Task<List<FamilyMilestoneConfigVM>> GetMilestoneConfigsAsync(int year)
        {
            const string sql = @"
SELECT PointThreshold, MilestoneType, ISNULL(DisplayEmoji,'') DisplayEmoji
FROM dbo.DM2_FamilyMilestoneConfig WITH (NOLOCK)
WHERE Year = @Year AND IsActive = 1
ORDER BY SortOrder, PointThreshold;";
            var list = new List<FamilyMilestoneConfigVM>();
            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Year", year);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(new FamilyMilestoneConfigVM
                {
                    PointThreshold = r.GetInt32(0),
                    MilestoneType  = r.GetString(1),
                    DisplayEmoji   = r.GetString(2),
                });
            return list;
        }

        // AWARD 마일스톤 memo 키: threshold=1000이면 기존 포맷 유지
        private static string MilestoneGiftMemoKey(string teamName, int threshold)
            => threshold == 1000
               ? $"가족합산:{teamName}"
               : $"가족합산:{teamName}:{threshold}";

        public async Task<List<FamilyMilestoneDisplayVM>> GetHomepageFamilyMilestonesAsync(int year)
        {
            // 1. 활성 마일스톤 설정 읽기
            const string sqlCfg = @"
SELECT Id, PointThreshold, MilestoneType, DisplayTitle, ISNULL(DisplayEmoji,'') DisplayEmoji
FROM dbo.DM2_FamilyMilestoneConfig WITH (NOLOCK)
WHERE Year = @Year AND IsActive = 1
ORDER BY SortOrder, PointThreshold;";

            var configs = new List<(int Id, int Threshold, string Type, string Title, string Emoji)>();
            using (var conn = Conn())
            using (var cmd  = new SqlCommand(sqlCfg, conn))
            {
                cmd.Parameters.AddWithValue("@Year", year);
                await conn.OpenAsync();
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    configs.Add((r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetString(3), r.GetString(4)));
            }
            if (configs.Count == 0) return new();

            // 2. 모든 가족팀 구성원 조회 (기존 로직과 동일)
            const string sqlMembers = @"
SELECT t.FamilyTeamId, t.TeamName,
       m.UserName, ISNULL(m.RelationCode,9) AS RelationCode,
       ISNULL(pm.FullName, m.UserName) AS FullName,
       u.Id AS UserId,
       ISNULL(m.RoleCode, 9) AS RoleCode,
       ISNULL(pm.Sex, '') AS Sex
FROM dbo.DM2_FamilyTeam t WITH (NOLOCK)
JOIN dbo.DM2_FamilyMember m WITH (NOLOCK) ON m.FamilyTeamId = t.FamilyTeamId AND m.IsActive = 1
JOIN [Identity].[User] u WITH (NOLOCK) ON u.UserName = m.UserName
LEFT JOIN dbo.view_PersonMaster_2026 pm WITH (NOLOCK) ON pm.UserId = u.Id
WHERE t.IsActive = 1
ORDER BY t.FamilyTeamId, m.RoleCode, m.RelationCode;";

            // RoleCode: 0=학생, 1=엄마, 2=아빠, 3=할머니, 4=할아버지, 기타=구성원
            static string MemberRoleLabel(int roleCode, int relCode) => roleCode switch
            {
                1 => "엄마",
                2 => "아빠",
                3 => "할머니",
                4 => "할아버지",
                0 => relCode switch
                {
                    0 => "본인", 1 => "동생", 2 => "동생", 3 => "언니", 4 => "누나",
                    5 => "형", 6 => "오빠", _ => ""
                },
                _ => "구성원"
            };

            var teamMap = new Dictionary<int, (string Name, List<(string UserName, string FullName, string Photo, int RelCode, string RelLabel, string Sex)> Members)>();
            using (var conn = Conn())
            using (var cmd  = new SqlCommand(sqlMembers, conn))
            {
                await conn.OpenAsync();
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    int tid      = r.GetInt32(0);
                    var tname    = r.IsDBNull(1) ? "" : r.GetString(1);
                    var uname    = r.IsDBNull(2) ? "" : r.GetString(2);
                    int relCode  = r.IsDBNull(3) ? 9 : Convert.ToInt32(r[3]);
                    var full     = r.IsDBNull(4) ? uname : r.GetString(4);
                    var uid      = r.IsDBNull(5) ? null : r.GetString(5);
                    int roleCode = r.IsDBNull(6) ? 9 : Convert.ToInt32(r[6]);
                    var sex      = r.IsDBNull(7) ? "" : r.GetString(7);
                    var photo = string.IsNullOrWhiteSpace(uid)
                        ? "/images/icon/people/people_01.webp"
                        : $"/HumanResource/GetProfilePicture?userId={Uri.EscapeDataString(uid)}";
                    var displayLabel = MemberRoleLabel(roleCode, relCode);

                    if (!teamMap.ContainsKey(tid))
                        teamMap[tid] = (tname, new());
                    teamMap[tid].Members.Add((uname, full, photo, relCode, displayLabel, sex));
                }
            }
            if (teamMap.Count == 0) return new();

            // 3. 팀별 DM2 합산 + 멤버별 점수 — 단일 배치 CROSS APPLY
            var teamTotals   = new Dictionary<int, int>();
            var memberPoints = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            {
                var from = new DateTime(year, 1, 1);
                var to   = DateHelper.GetKoreaNow().Date;
                if (to.Year != year) to = new DateTime(year, 12, 31);

                var allUsers = teamMap.Values
                    .SelectMany(t => t.Members.Select(m => m.UserName))
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (allUsers.Count > 0)
                {
                    // VALUES (@u0),(@u1),... 동적 생성
                    var valParts = allUsers.Select((_, i) => $"(@u{i})");
                    var sqlBatch = $@"
SELECT n.UserName, ISNULL(SUM(ISNULL(x.TotalPoint,0)),0) AS YearPoint
FROM (VALUES {string.Join(",", valParts)}) AS n(UserName)
CROSS APPLY dbo.ufn_DM2_GetUserDailyPoints(n.UserName, @F, @T) x
GROUP BY n.UserName;";

                    using var conn = Conn();
                    using var cmd  = new SqlCommand(sqlBatch, conn);
                    cmd.Parameters.Add(new SqlParameter("@F", SqlDbType.Date) { Value = from.Date });
                    cmd.Parameters.Add(new SqlParameter("@T", SqlDbType.Date) { Value = to.Date });
                    for (int i = 0; i < allUsers.Count; i++)
                        cmd.Parameters.Add(new SqlParameter($"@u{i}", SqlDbType.NVarChar, 100) { Value = allUsers[i] });

                    await conn.OpenAsync();
                    using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                        memberPoints[r.GetString(0)] = r.IsDBNull(1) ? 0 : Convert.ToInt32(r[1]);
                }

                foreach (var (tid, (_, members)) in teamMap)
                {
                    int total = members.Sum(m => memberPoints.TryGetValue(m.UserName, out int p) ? p : 0);
                    teamTotals[tid] = total;
                }
            }

            // 4. AWARD 마일스톤: 선물 주문 현황 일괄 조회 (memo 키 기반)
            // memo 포맷: '가족합산:{teamName}' (threshold=1000) 또는 '가족합산:{teamName}:{threshold}'
            var giftOrders = new Dictionary<string, (string Status, int? ProductId, string? ProductName)>(StringComparer.OrdinalIgnoreCase);
            const string sqlGifts = @"
SELECT o.Memo, o.Status, o.ProductID, a.ProductName
FROM dbo.StudentPointGiftOrder o WITH (NOLOCK)
LEFT JOIN dbo.AwardItem a WITH (NOLOCK) ON a.ProductID = o.ProductID
WHERE o.Memo LIKE N'가족합산:%'
  AND ISNULL(o.Status,'') IN ('DRAFT','PURCHASED','HIDDEN')
ORDER BY o.CreatedAt DESC;";
            using (var conn = Conn())
            using (var cmd  = new SqlCommand(sqlGifts, conn))
            {
                await conn.OpenAsync();
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var memo    = r.IsDBNull(0) ? "" : r.GetString(0);
                    var status  = r.IsDBNull(1) ? "" : r.GetString(1);
                    int? prodId = r.IsDBNull(2) ? null : r.GetInt32(2);
                    var  pname  = r.IsDBNull(3) ? null : r.GetString(3);
                    if (!string.IsNullOrWhiteSpace(memo) && !giftOrders.ContainsKey(memo))
                        giftOrders[memo] = (status, prodId, pname);
                }
            }

            // 5. APPLAUSE 마일스톤: 숨김 현황 조회
            var hiddenSet = new HashSet<(int ConfigId, int TeamId)>();
            const string sqlHidden = @"
SELECT MilestoneConfigId, FamilyTeamId
FROM dbo.DM2_FamilyMilestoneHidden WITH (NOLOCK);";
            using (var conn = Conn())
            using (var cmd  = new SqlCommand(sqlHidden, conn))
            {
                await conn.OpenAsync();
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    hiddenSet.Add((r.GetInt32(0), r.GetInt32(1)));
            }

            // 6. 조합: 팀 × 마일스톤 → 표시 목록
            var result = new List<FamilyMilestoneDisplayVM>();
            foreach (var (tid, (teamName, members)) in teamMap)
            {
                if (!teamTotals.TryGetValue(tid, out int teamTotal)) continue;
                var recipient = members.FirstOrDefault(x => x.RelCode == 0);
                if (recipient == default) recipient = members[0];

                foreach (var (cfgId, threshold, type, title, emoji) in configs)
                {
                    if (teamTotal < threshold) continue;

                    var vm = new FamilyMilestoneDisplayVM
                    {
                        MilestoneConfigId = cfgId,
                        PointThreshold    = threshold,
                        MilestoneType     = type,
                        DisplayTitle      = title,
                        DisplayEmoji      = emoji,
                        FamilyTeamId      = tid,
                        TeamName          = teamName,
                        TeamTotalDm2      = teamTotal,
                        RecipientUserName = recipient.UserName,
                        RecipientFullName = recipient.FullName,
                        RecipientPhoto    = recipient.Photo,
                        RecipientSex      = recipient.Sex,
                        Members           = members.Select(m => (m.FullName, m.RelLabel, memberPoints.TryGetValue(m.UserName, out int mp) ? mp : 0, m.Photo, m.Sex)).ToList(),
                        MemberUserNames   = members.Select(m => m.UserName).Where(u => !string.IsNullOrWhiteSpace(u)).ToHashSet(StringComparer.OrdinalIgnoreCase),
                    };

                    if (type == "AWARD")
                    {
                        var memoKey = MilestoneGiftMemoKey(teamName, threshold);
                        if (giftOrders.TryGetValue(memoKey, out var g))
                        {
                            vm.GiftStatus  = g.Status;
                            vm.ProductId   = g.ProductId;
                            vm.ProductName = g.ProductName;
                        }
                    }
                    else // APPLAUSE
                    {
                        vm.IsHidden = hiddenSet.Contains((cfgId, tid));
                    }

                    // 전달 완료(HIDDEN) · 박수 숨김도 같이 돌려준다 — 홈 타임라인이 지난 단계를 상품과 함께 보여 주고,
                    // 선물함 '지난 선물' 에도 나온다.  감출지는 화면이 정한다 (vm.ShouldShow).
                    result.Add(vm);
                }
            }

            return result.OrderBy(x => x.PointThreshold).ThenByDescending(x => x.TeamTotalDm2).ToList();
        }

        /// <summary>
        ///  내가 속한 가족팀이 달성한 '선물' 단계 전부 (점수 낮은 단계부터).
        ///  박수(APPLAUSE) 단계는 선물이 없어 뺀다. 선물 선택 상태도 함께 담겨 온다.
        /// </summary>
        public async Task<List<FamilyMilestoneDisplayVM>> GetMyFamilyMilestonesAsync(string userName, int year)
        {
            if (string.IsNullOrWhiteSpace(userName)) return new();

            var all = await GetHomepageFamilyMilestonesAsync(year);
            return all.Where(m => string.Equals(m.MilestoneType, "AWARD", StringComparison.OrdinalIgnoreCase)
                               && m.MemberUserNames.Contains(userName))
                      .OrderBy(m => m.PointThreshold)
                      .ToList();
        }

        public async Task HideApplauseMilestoneAsync(int milestoneConfigId, int familyTeamId)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.DM2_FamilyMilestoneHidden WHERE MilestoneConfigId=@C AND FamilyTeamId=@T)
    INSERT INTO dbo.DM2_FamilyMilestoneHidden (MilestoneConfigId, FamilyTeamId) VALUES (@C, @T);";
            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@C", milestoneConfigId);
            cmd.Parameters.AddWithValue("@T", familyTeamId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> IsUserInFamilyTeamAsync(string userName, int familyTeamId)
        {
            const string sql = @"
SELECT COUNT(1) FROM dbo.DM2_FamilyMember WITH (NOLOCK)
WHERE UserName = @U AND FamilyTeamId = @T AND IsActive = 1;";
            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@U", SqlDbType.NVarChar, 100) { Value = userName });
            cmd.Parameters.AddWithValue("@T", familyTeamId);
            await conn.OpenAsync();
            var cnt = await cmd.ExecuteScalarAsync();
            return cnt != null && Convert.ToInt32(cnt) > 0;
        }

        /// <summary>
        ///  팀 이름으로 그 가족팀 구성원인지 확인한다.
        ///  가족 합산 선물 화면은 팀 ID 없이 팀 이름만 들고 다녀서 이름으로 확인할 길이 필요하다.
        /// </summary>
        public async Task<bool> IsUserInFamilyTeamByNameAsync(string userName, string teamName)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(teamName)) return false;

            const string sql = @"
SELECT COUNT(1)
FROM dbo.DM2_FamilyMember m WITH (NOLOCK)
JOIN dbo.DM2_FamilyTeam   t WITH (NOLOCK) ON t.FamilyTeamId = m.FamilyTeamId AND t.IsActive = 1
WHERE m.UserName = @U AND m.IsActive = 1 AND t.TeamName = @T;";

            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@U", SqlDbType.NVarChar, 100) { Value = userName });
            cmd.Parameters.Add(new SqlParameter("@T", SqlDbType.NVarChar, 200) { Value = teamName });
            await conn.OpenAsync();
            var cnt = await cmd.ExecuteScalarAsync();
            return cnt != null && Convert.ToInt32(cnt) > 0;
        }

        public async Task SelectFamilyGiftForMilestoneAsync(string topScorerUserName, int productId, int teamTotalDm2, string teamName, int threshold)
        {
            var memoKey = MilestoneGiftMemoKey(teamName, threshold);
            const string sql = @"
DELETE FROM dbo.StudentPointGiftOrder WHERE Memo = @Memo AND Status = 'DRAFT';
INSERT INTO dbo.StudentPointGiftOrder
    (UserName, ProductId, CreditCostSnapshot, Status, CreatedAt, IsCurrentDraft, Memo, MileStone)
VALUES
    (@UserName, @ProductId, @TeamTotal, 'DRAFT', GETDATE(), 0, @Memo, @Threshold);";
            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserName",  topScorerUserName);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@TeamTotal", teamTotalDm2);
            cmd.Parameters.AddWithValue("@Memo",      memoKey);
            cmd.Parameters.AddWithValue("@Threshold", threshold);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>가족 합산 선물 — 이 팀·이 단계에서 지금 고른 선물 (아직 안 골랐으면 null)</summary>
        public async Task<GiftOrderVM?> GetFamilyGiftPickAsync(string teamName, int threshold)
        {
            if (string.IsNullOrWhiteSpace(teamName)) return null;

            var memoKey = MilestoneGiftMemoKey(teamName, threshold);
            const string sql = @"
SELECT TOP 1 o.OrderId, o.ProductID, a.ProductName, ISNULL(a.ProductDescription, '') AS ProductDescription,
       o.CreditCostSnapshot, o.Status, o.CreatedAt, o.PurchasedAt, ISNULL(o.MileStone, 0) AS MileStone
FROM dbo.StudentPointGiftOrder o WITH (NOLOCK)
JOIN dbo.AwardItem a WITH (NOLOCK) ON a.ProductID = o.ProductID
WHERE o.Memo = @Memo AND o.Status = 'DRAFT'
ORDER BY o.OrderId DESC;";

            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Memo", memoKey);
            await conn.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await r.ReadAsync()) return null;

            return new GiftOrderVM
            {
                OrderId            = r.GetInt32(0),
                ProductID          = r.GetInt32(1),
                ProductName        = r.GetString(2),
                ProductDescription = r.GetString(3),
                CreditCostSnapshot = r.GetInt32(4),
                Status             = r.GetString(5),
                CreatedAt          = r.GetDateTime(6),
                PurchasedAt        = r.IsDBNull(7) ? null : r.GetDateTime(7),
                Milestone          = r.GetInt32(8)
            };
        }

        public async Task CancelFamilyGiftForMilestoneAsync(string teamName, int threshold)
        {
            var memoKey = MilestoneGiftMemoKey(teamName, threshold);
            const string sql = @"DELETE FROM dbo.StudentPointGiftOrder WHERE Memo = @Memo AND Status = 'DRAFT';";
            using var conn = Conn();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Memo", memoKey);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<FamilyTreeVM?> GetMyFamilyTreeAsync(string userName, int year)
        {
            var from = new DateTime(year, 1, 1);
            var to   = new DateTime(year, 12, 31);

            // Step 1: 멤버 기본 정보 (점수 제외 — CROSS APPLY 없이 안전하게)
            const string sqlMembers = @"
SELECT
    m2.UserName,
    ISNULL(pm.FullName, m2.UserName)  AS FullName,
    ISNULL(ui.ProfilePicture, '')     AS Photo,
    ISNULL(m2.RoleCode, 0)            AS RoleCode,
    ISNULL(m2.RelationCode, 9)        AS RelationCode,
    t.FamilyTeamId,
    t.TeamName
FROM dbo.DM2_FamilyMember m1 WITH (NOLOCK)
JOIN dbo.DM2_FamilyTeam   t  WITH (NOLOCK) ON t.FamilyTeamId = m1.FamilyTeamId AND t.IsActive = 1
JOIN dbo.DM2_FamilyMember m2 WITH (NOLOCK) ON m2.FamilyTeamId = m1.FamilyTeamId AND m2.IsActive = 1
JOIN [Identity].[User]    u  WITH (NOLOCK) ON u.UserName = m2.UserName
LEFT JOIN dbo.view_PersonMaster_2026      pm WITH (NOLOCK) ON pm.UserId = u.Id
LEFT JOIN dbo.view_UserCompositeInfo_2026 ui WITH (NOLOCK) ON ui.UserId = u.Id
WHERE m1.UserName = @MyUser AND m1.IsActive = 1
ORDER BY m2.RoleCode, m2.RelationCode;";

            var members = new List<FamilyTreeMemberVM>();
            int familyTeamId = 0;
            string teamName = "";

            using (var conn = Conn())
            using (var cmd  = new SqlCommand(sqlMembers, conn))
            {
                cmd.Parameters.AddWithValue("@MyUser", userName);
                cmd.CommandTimeout = 30;
                await conn.OpenAsync();
                using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    if (familyTeamId == 0)
                    {
                        familyTeamId = Convert.ToInt32(rd[5]);
                        teamName     = rd.IsDBNull(6) ? "" : rd.GetString(6);
                    }
                    var photo = rd.GetString(2);
                    members.Add(new FamilyTreeMemberVM
                    {
                        UserName     = rd.GetString(0),
                        FullName     = rd.GetString(1),
                        Photo        = string.IsNullOrWhiteSpace(photo) ? "/images/icon/people/people_01.webp" : photo,
                        RoleCode     = rd.IsDBNull(3) ? 0 : Convert.ToInt32(rd[3]),
                        RelationCode = rd.IsDBNull(4) ? 9 : Convert.ToInt32(rd[4]),
                        IsMe         = string.Equals(rd.GetString(0), userName, StringComparison.OrdinalIgnoreCase),
                    });
                }
            }

            if (members.Count == 0) return null;

            // Step 2: 멤버별 DM2 연간 점수 (학생 UserName만 함수 호출 — 실패해도 0으로 처리)
            try
            {
                var allUserNames = members.Select(m => m.UserName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var valParts = allUserNames.Select((_, i) => $"(@u{i})");
                var sqlPoints = $@"
SELECT n.UserName,
       ISNULL(SUM(ISNULL(x.TotalPoint,0)),0) AS YearPoint,
       MAX(CASE WHEN ISNULL(x.TotalPoint,0)>0 THEN x.DateBy END)  AS LastActionDate
FROM (VALUES {string.Join(",", valParts)}) AS n(UserName)
CROSS APPLY dbo.ufn_DM2_GetUserDailyPoints(n.UserName, @F, @T) x
GROUP BY n.UserName;";

                using var conn2 = Conn();
                using var cmd2  = new SqlCommand(sqlPoints, conn2);
                cmd2.Parameters.Add(new SqlParameter("@F", System.Data.SqlDbType.Date) { Value = from.Date });
                cmd2.Parameters.Add(new SqlParameter("@T", System.Data.SqlDbType.Date) { Value = to.Date });
                for (int i = 0; i < allUserNames.Count; i++)
                    cmd2.Parameters.Add(new SqlParameter($"@u{i}", System.Data.SqlDbType.NVarChar, 100) { Value = allUserNames[i] });
                cmd2.CommandTimeout = 30;
                await conn2.OpenAsync();
                using var rd2 = await cmd2.ExecuteReaderAsync();
                var pointMap = new Dictionary<string, (int Pt, DateTime? Last)>(StringComparer.OrdinalIgnoreCase);
                while (await rd2.ReadAsync())
                {
                    var uname = rd2.GetString(0);
                    int pt    = rd2.IsDBNull(1) ? 0 : rd2.GetInt32(1);
                    DateTime? last = rd2.IsDBNull(2) ? null : (DateTime?)rd2.GetDateTime(2);
                    pointMap[uname] = (pt, last);
                }
                foreach (var m in members)
                {
                    if (pointMap.TryGetValue(m.UserName, out var p))
                    {
                        m.YearPoint      = p.Pt;
                        m.LastActionDate = p.Last;
                    }
                }
            }
            catch
            {
                // 점수 조회 실패 시 0으로 유지 (가계도 자체는 표시)
            }

            // 로그인 사용자의 RoleCode 파악
            var me = members.FirstOrDefault(m => m.IsMe);
            int myRoleCode = me?.RoleCode ?? 9;

            // 관계 레이블 계산 (로그인 사용자 관점)
            foreach (var m in members)
            {
                if (m.IsMe) { m.RelLabel = "본인"; continue; }

                if (myRoleCode == 0) // 로그인: 학생
                {
                    m.RelLabel = m.RoleCode switch
                    {
                        0 => m.RelationCode switch { 1=>"형", 2=>"동생", 3=>"언니", 4=>"누나", _=>"형제" },
                        1 => "엄마", 2 => "아빠", 3 => "할머니", 4 => "할아버지", _ => "가족"
                    };
                }
                else if (myRoleCode == 1) // 로그인: 엄마
                {
                    m.RelLabel = m.RoleCode switch
                    {
                        0 => "자녀", 2 => "배우자", 3 => "어머니", 4 => "아버지", _ => "가족"
                    };
                }
                else if (myRoleCode == 2) // 로그인: 아빠
                {
                    m.RelLabel = m.RoleCode switch
                    {
                        0 => "자녀", 1 => "배우자", 3 => "어머니", 4 => "아버지", _ => "가족"
                    };
                }
                else if (myRoleCode == 3) // 로그인: 할머니
                {
                    m.RelLabel = m.RoleCode switch { 0=>"손자/녀", 1=>"며느리/딸", 2=>"아들/사위", _=>"가족" };
                }
                else if (myRoleCode == 4) // 로그인: 할아버지
                {
                    m.RelLabel = m.RoleCode switch { 0=>"손자/녀", 1=>"며느리/딸", 2=>"아들/사위", _=>"가족" };
                }
                else
                {
                    m.RelLabel = "가족";
                }
            }

            return new FamilyTreeVM
            {
                FamilyTeamId = familyTeamId,
                TeamName     = teamName,
                Members      = members,
            };
        }
    }
}
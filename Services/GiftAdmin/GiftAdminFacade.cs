using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulletinBoard.Services.Abstract;
using BibleGiftStatus = BibleReadingThrough.Models.BibleGift.BibleGiftStatus;

namespace BulletinBoard.Services.GiftAdmin
{
    /// <summary>
    ///  선물 관리 한 창구 구현.
    ///  · 읽기 : 기존 두 조회(도토리·가족 / 성경통독)를 그대로 불러 공통 모양으로 옮긴다.
    ///  · 쓰기 : 종류를 보고 원래 서비스의 메서드로 넘긴다.
    ///  테이블·기존 로직은 건드리지 않는다.
    /// </summary>
    public sealed class GiftAdminFacade : IGiftAdminFacade
    {
        private readonly IStudentPointGiftService _point;
        private readonly BibleReadingThrough.Services.BibleGift.IBibleGiftService _bible;
        private readonly IAwardItemService _awardItem;

        public GiftAdminFacade(
            IStudentPointGiftService point,
            BibleReadingThrough.Services.BibleGift.IBibleGiftService bible,
            IAwardItemService awardItem)
        {
            _point     = point;
            _bible     = bible;
            _awardItem = awardItem;
        }

        // ══════════════════════════════════════════════════════
        //  읽기
        // ══════════════════════════════════════════════════════
        public async Task<UnifiedGiftAdminPageVM> GetPageAsync(GiftKind? deliveredKind, bool isSuperAdmin)
        {
            var all = new List<UnifiedGiftAdminRowVM>();
            var failed = new List<string>();

            // 하나가 실패해도 나머지는 보이게 한다. 다만 조용히 숨기지는 않는다 —
            // 목록에서 선물이 사라진 것을 관리자가 눈치채지 못하면 안 된다.
            try { all.AddRange(await LoadPointAndFamilyAsync()); } catch (Exception ex) { failed.Add($"도토리·가족 선물을 불러오지 못했어요. ({ex.Message})"); }
            try { all.AddRange(await LoadBibleAsync());          } catch (Exception ex) { failed.Add($"성경통독 선물을 불러오지 못했어요. ({ex.Message})"); }

            await FillExternalUrlAsync(all);

            var pending   = all.Where(r => r.State != GiftState.Delivered).ToList();
            var delivered = all.Where(r => r.State == GiftState.Delivered).ToList();

            var vm = new UnifiedGiftAdminPageVM
            {
                KindFilter   = deliveredKind,
                IsSuperAdmin = isSuperAdmin,
                LoadErrors   = failed,

                WaitingCount   = pending.Count(r => r.State == GiftState.Waiting),
                PickedCount    = pending.Count(r => r.State == GiftState.Picked),
                PurchasedCount = pending.Count(r => r.State == GiftState.Purchased),

                DeliveredTotalCount          = delivered.Count,
                DeliveredPointCount          = delivered.Count(r => r.Kind == GiftKind.Point),
                DeliveredFamilyCombinedCount = delivered.Count(r => r.Kind == GiftKind.FamilyCombined),
                DeliveredBibleCount          = delivered.Count(r => r.Kind == GiftKind.Bible),

                PurchasedSum = all.Where(r => r.State == GiftState.Purchased || r.State == GiftState.Delivered)
                                  .Sum(r => r.UnitPrice)
            };

            // 미처리 — 거르지 않는다. 처리할 것을 위로, 같은 상태끼리는 오래된 것부터.
            vm.PendingRows = pending
                .OrderBy(r => (int)r.State)
                .ThenBy(r => r.SelectedAt ?? DateTime.MaxValue)
                .ThenBy(r => r.FullName)
                .ToList();

            // 전달 완료 — 종류로 거르고, 최근에 끝난 것부터 (지난 내역이라 최신이 위)
            IEnumerable<UnifiedGiftAdminRowVM> done = delivered;
            if (deliveredKind.HasValue) done = done.Where(r => r.Kind == deliveredKind.Value);

            vm.DeliveredRows = done
                .OrderByDescending(r => r.DeliveredAt ?? r.PurchasedAt ?? r.SelectedAt ?? DateTime.MinValue)
                .ThenBy(r => r.FullName)
                .ToList();

            return vm;
        }

        /// <summary>도토리 · 가족팀 · 가족 합산 — StudentPointGiftOrder 한 테이블에서 온다</summary>
        private async Task<List<UnifiedGiftAdminRowVM>> LoadPointAndFamilyAsync()
        {
            // HIDDEN(=전달완료)까지 봐야 하므로 항상 포함해 읽는다
            var src = await _point.GetAdminPurchaseListAsync(includeHidden: true);
            var list = new List<UnifiedGiftAdminRowVM>();

            foreach (var r in src)
            {
                var kind = r.IsFamilyCombinedGift ? GiftKind.FamilyCombined
                         : r.IsTeamGift           ? GiftKind.FamilyTeam
                                                  : GiftKind.Point;

                list.Add(new UnifiedGiftAdminRowVM
                {
                    Kind      = kind,
                    OrderId   = r.OrderId,
                    UserId    = r.UserId,
                    UserName  = r.UserName ?? "",
                    FullName  = string.IsNullOrWhiteSpace(r.UserFullName) ? (r.UserName ?? "") : r.UserFullName!,
                    PhotoUrl  = string.IsNullOrWhiteSpace(r.UserPhotoUrl) ? "/images/icon-user.png" : r.UserPhotoUrl!,

                    ProductId          = r.ProductID,
                    ProductName        = r.ProductName,
                    ProductDescription = r.ProductDescription,
                    ImageUrl           = $"/images/AwardItem/{r.ProductID}.jpg",
                    UnitPrice          = r.UnitPrice,

                    State      = MapPointState(r.Status),
                    SelectedAt = r.CreatedAt,
                    PurchasedAt = r.PurchasedAt,
                    // HIDDEN 에는 전달 시각 칸이 없다 (스키마를 바꾸지 않기로 했다)
                    DeliveredAt = null,

                    ContextLabel = BuildPointContext(kind, r)
                });
            }

            return list;
        }

        /// <summary>성경통독 완독 선물 — BibleGiftOrders</summary>
        private async Task<List<UnifiedGiftAdminRowVM>> LoadBibleAsync()
        {
            var page = await _bible.GetAdminAsync("all");
            var list = new List<UnifiedGiftAdminRowVM>();

            foreach (var r in page.Rows)
            {
                list.Add(new UnifiedGiftAdminRowVM
                {
                    Kind      = GiftKind.Bible,
                    OrderId   = r.OrderId,
                    UserId    = r.UserId,
                    UserName  = r.UserName ?? "",
                    FullName  = string.IsNullOrWhiteSpace(r.FullName) ? (r.UserName ?? "") : r.FullName,
                    PhotoUrl  = BuildProfileUrl(r.UserId),

                    ProductName = r.ProductName,
                    ImageUrl    = r.ImageUrl,
                    ExternalUrl = r.ExternalUrl,
                    UnitPrice   = r.UnitPrice ?? 0,

                    State       = MapBibleState(r.Status),
                    SelectedAt  = r.SelectedAt,
                    PurchasedAt = r.PurchasedAt,
                    DeliveredAt = r.DeliveredAt,

                    RoundNo      = r.RoundNo,
                    ContextLabel = $"{r.RoundNo}독"
                });
            }

            return list;
        }

        /// <summary>쿠팡 등 바로가기 주소 채우기 (품목 목록 한 번만 읽는다)</summary>
        private async Task FillExternalUrlAsync(List<UnifiedGiftAdminRowVM> rows)
        {
            try
            {
                var need = rows.Any(r => r.ProductId.HasValue && string.IsNullOrWhiteSpace(r.ExternalUrl));
                if (!need) return;

                var items = await _awardItem.GetAllAsync(BulletinBoard.Common.ProductDisplayOpton.none, includeHidden: true);
                var map = items.Where(x => !string.IsNullOrWhiteSpace(x.ExternalUrl))
                               .GroupBy(x => x.ProductID)
                               .ToDictionary(g => g.Key, g => g.First().ExternalUrl!);

                foreach (var r in rows)
                {
                    if (r.ProductId.HasValue
                        && string.IsNullOrWhiteSpace(r.ExternalUrl)
                        && map.TryGetValue(r.ProductId.Value, out var url))
                    {
                        r.ExternalUrl = url;
                    }
                }
            }
            catch { /* 바로가기는 부가 정보 */ }
        }

        // ══════════════════════════════════════════════════════
        //  쓰기 — 종류대로 원래 서비스에 넘긴다
        // ══════════════════════════════════════════════════════
        public Task<(bool ok, string msg)> PurchaseAsync(GiftKind kind, int orderId, string adminId)
            => kind == GiftKind.Bible
               ? _bible.PurchaseAsync(orderId, adminId)
               : Guard(() => _point.CompletePurchaseAsync(orderId, adminId), "구매 완료로 바꿨어요.");

        public Task<(bool ok, string msg)> UndoPurchaseAsync(GiftKind kind, int orderId, string adminId)
            => kind == GiftKind.Bible
               ? _bible.UndoPurchaseAsync(orderId, adminId)
               : Guard(() => _point.UndoPurchaseAsync(orderId, adminId), "구매를 되돌렸어요.");

        public Task<(bool ok, string msg)> DeliverAsync(GiftKind kind, int orderId, string adminId)
            => kind == GiftKind.Bible
               ? _bible.DeliverAsync(orderId, adminId)
               // 도토리·가족은 HIDDEN 이 전달완료다
               : Guard(() => _point.HidePurchaseAsync(orderId, adminId), "전달 완료로 바꿨어요.");

        public Task<(bool ok, string msg)> UndoDeliverAsync(GiftKind kind, int orderId, string adminId)
            => kind == GiftKind.Bible
               ? _bible.UndoDeliverAsync(orderId, adminId)
               : Guard(() => _point.UndoHideAsync(orderId, adminId), "전달을 되돌렸어요.");

        // ══════════════════════════════════════════════════════
        //  도우미
        // ══════════════════════════════════════════════════════

        /// <summary>도토리 쪽 서비스는 실패를 예외로 알린다 — 성경통독과 같은 모양으로 맞춘다.</summary>
        private static async Task<(bool ok, string msg)> Guard(Func<Task> action, string okMsg)
        {
            try
            {
                await action();
                return (true, okMsg);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static GiftState MapPointState(string? status) => (status ?? "").ToUpperInvariant() switch
        {
            "PURCHASED" => GiftState.Purchased,
            "HIDDEN"    => GiftState.Delivered,   // 결정: HIDDEN = 전달완료
            _           => GiftState.Picked       // DRAFT
        };

        private static GiftState MapBibleState(string? status) => (status ?? "") switch
        {
            BibleGiftStatus.Purchased => GiftState.Purchased,
            BibleGiftStatus.Delivered => GiftState.Delivered,
            BibleGiftStatus.Draft     => GiftState.Picked,
            _                         => GiftState.Waiting   // "" (안 고름) · CANCELED
        };

        private static string BuildProfileUrl(string? userId)
            => string.IsNullOrWhiteSpace(userId) ? "/images/icon-user.png" : $"/images/profile/{userId}.png";

        private static string BuildPointContext(GiftKind kind, AdminPurchaseRowVM r)
        {
            if (kind == GiftKind.FamilyCombined)
                return TeamNameFromMemo(r.Memo, "가족합산:");
            if (kind == GiftKind.FamilyTeam)
                return TeamNameFromMemo(r.Memo, "가족팀:");

            return r.MileStoneVal > 0 ? $"{r.MileStoneVal:N0}점" : "";
        }

        private static string TeamNameFromMemo(string? memo, string prefix)
        {
            if (string.IsNullOrWhiteSpace(memo)) return "";
            return memo.StartsWith(prefix, StringComparison.Ordinal)
                   ? memo.Substring(prefix.Length).Trim()
                   : memo.Trim();
        }
    }
}

using BibleReadingThrough.Data;
using BibleReadingThrough.Models.BibleGift;
using BulletinBoard.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BibleReadingThrough.Services.BibleGift;

public interface IBibleGiftService
{
    Task<BibleGiftBannerVM?> GetBannerAsync(string userId, string? fullName);
    Task<BibleGiftSelectVM?> GetSelectPageAsync(string userId, int round);
    /// <summary>선물을 받을 수 있는 회차 전부 — 통합 선물함에서 회차마다 한 칸씩 보여 준다</summary>
    Task<List<BibleGiftSelectVM>> GetMyRoundsAsync(string userId);
    /// <summary>구경 전용 — 로그인 없이(또는 대상자가 아닌 사람이) 선물 목록만 본다</summary>
    Task<BibleGiftSelectVM?> GetBrowsePageAsync(int round);
    Task<(bool ok, string msg)> SelectAsync(string userId, string? userName, int round, int productId);
    Task<(bool ok, string msg)> CancelAsync(string userId, int round);
    Task<BibleGiftAdminVM> GetAdminAsync(string tab);
    Task<(bool ok, string msg)> PurchaseAsync(int orderId, string adminId);
    Task<(bool ok, string msg)> UndoPurchaseAsync(int orderId, string adminId);
    Task<(bool ok, string msg)> DeliverAsync(int orderId, string adminId);
    Task<(bool ok, string msg)> UndoDeliverAsync(int orderId, string adminId);
    /// <summary>
    ///  회차별로 고른 선물. 열쇠는 <c>BibleGiftKeys.Of(userId, roundNo)</c> 로 만든다.
    ///  (예전에는 사람당 한 건만 돌려줘서 1독 선물이 2독 줄에 붙는 문제가 있었다)
    /// </summary>
    Task<Dictionary<string, BibleGiftPickVM>> GetCurrentPicksAsync();
    /// <summary>아이디(UserName) → 이름(FullName). 못 찾으면 null</summary>
    Task<string?> GetFullNameAsync(string? userName);
    Task<(bool ok, string msg)> ForceDeleteAsync(string userId, int round, string adminId);
}

public class BibleGiftService : IBibleGiftService
{
    private readonly AppDbContext _db;
    private readonly BulletinBoard.Services.GiftCatalog.IGiftCatalogService _catalog;

    public BibleGiftService(AppDbContext db, BulletinBoard.Services.GiftCatalog.IGiftCatalogService catalog)
    {
        _db      = db;
        _catalog = catalog;
    }

    private static int CurYear => DateHelper.GetKoreaNow().Year;

    private const string StudentRoleCode = "T1001";   // 학생

    /// <summary>UserName → (이름, 역할코드). 선물은 학생만 받는다.</summary>
    private async Task<Dictionary<string, UserCompositeInfo>> GetPersonMapAsync()
    {
        try
        {
            var rows = await _db.UserComposites.AsNoTracking().ToListAsync();
            return rows.Where(r => !string.IsNullOrWhiteSpace(r.UserName))
                       .GroupBy(r => r.UserName!, StringComparer.OrdinalIgnoreCase)
                       .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, UserCompositeInfo>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static bool IsStudent(Dictionary<string, UserCompositeInfo> map, string? userName)
        => !string.IsNullOrWhiteSpace(userName)
           && map.TryGetValue(userName!, out var p)
           && string.Equals(p.RoleCode, StudentRoleCode, StringComparison.OrdinalIgnoreCase);

    /// <summary>아이디(UserName) → 이름(FullName)</summary>
    public async Task<string?> GetFullNameAsync(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) return null;
        var map = await GetPersonMapAsync();
        return map.TryGetValue(userName!, out var p) && !string.IsNullOrWhiteSpace(p.FullName)
               ? p.FullName : null;
    }

    /// <summary>선물 대상(학생)인지 — userId 로 확인</summary>
    private async Task<bool> IsStudentByIdAsync(string userId)
    {
        var map = await GetPersonMapAsync();

        // 완독 기록(BibleCompletionLog)은 '다음 독 시작'을 눌러야 생긴다.
        // 첫 완독자는 아직 기록이 없으므로 사람 정보 뷰에서 UserId 로 먼저 찾는다.
        var byId = map.Values.FirstOrDefault(
            p => !string.IsNullOrWhiteSpace(p.UserId)
                 && string.Equals(p.UserId, userId, StringComparison.OrdinalIgnoreCase));
        if (byId != null)
            return string.Equals(byId.RoleCode, StudentRoleCode, StringComparison.OrdinalIgnoreCase);

        var name = await _db.BibleCompletionLogs.AsNoTracking()
                            .Where(l => l.UserId == userId)
                            .Select(l => l.UserName)
                            .FirstOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(name)) return false;
        return IsStudent(map, name);
    }

    /// <summary>
    ///  내 완독 회차 전부 (회차 내림차순).
    ///   · 지난 회차 : BibleCompletionLog 기록
    ///   · 이번 회차 : UserBibleProgress 가 100% 면 한 회차 더. 마친 날은 MAX(ReadDate).
    ///  '다음 독 시작'을 누르기 전에는 로그가 없어서, 이것 없이는 첫 완독자가 선물을 못 받는다.
    /// </summary>
    private async Task<List<(int RoundNo, DateTime CompletedAt)>> GetMyCompletionsAsync(string userId)
    {
        var logs = await _db.BibleCompletionLogs.AsNoTracking()
                            .Where(l => l.UserId == userId)
                            .Select(l => new { l.RoundNo, l.CompletedAt })
                            .ToListAsync();

        var list = logs.Select(l => (RoundNo: l.RoundNo, CompletedAt: l.CompletedAt)).ToList();

        int totalVerses = await _db.EBibles.CountAsync();
        if (totalVerses > 0)
        {
            int readVerses = await _db.UserBibleProgresses.AsNoTracking()
                                      .CountAsync(p => p.UserId == userId);
            if (readVerses >= totalVerses)
            {
                int maxLogged = list.Count > 0 ? list.Max(x => x.RoundNo) : 0;
                var lastRead = await _db.UserBibleProgresses.AsNoTracking()
                                        .Where(p => p.UserId == userId)
                                        .MaxAsync(p => (DateTime?)p.ReadDate);

                list.Add((maxLogged + 1, (lastRead ?? DateHelper.GetKoreaNow()).Date));
            }
        }

        return list.OrderByDescending(x => x.RoundNo).ToList();
    }

    // ── 기준 한 줄 찾기 (연도 것이 없으면 아무 연도의 같은 회차) ──
    private async Task<BibleGiftMilestoneConfig?> FindConfigAsync(int round)
    {
        return await _db.BibleGiftConfigs.AsNoTracking()
                   .Where(c => c.IsActive && c.RoundNo == round && c.CYear == CurYear)
                   .FirstOrDefaultAsync()
            ?? await _db.BibleGiftConfigs.AsNoTracking()
                   .Where(c => c.IsActive && c.RoundNo == round)
                   .OrderByDescending(c => c.CYear)
                   .FirstOrDefaultAsync();
    }

    private async Task<BibleGiftOrder?> GetCurrentAsync(string userId, int round)
        => await _db.BibleGiftOrders
                    .Where(o => o.UserId == userId && o.RoundNo == round && o.IsCurrentDraft)
                    .FirstOrDefaultAsync();

    private void AddLog(BibleGiftOrder? order, string userId, string? userName, int round,
                        string action, int? productId, int? price, string? actorId, string actorRole)
    {
        _db.BibleGiftOrderLogs.Add(new BibleGiftOrderLog
        {
            OrderId   = order?.OrderId,
            UserId    = userId,
            UserName  = userName,
            RoundNo   = round,
            Action    = action,
            ProductID = productId,
            UnitPrice = price,
            ActorId   = actorId,
            ActorRole = actorRole,
            CreatedAt = DateHelper.GetKoreaNow()
        });
    }

    // ══════════════════════════════════════════════════════
    //  홈 배너
    // ══════════════════════════════════════════════════════
    public async Task<BibleGiftBannerVM?> GetBannerAsync(string userId, string? fullName)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;
        if (!await IsStudentByIdAsync(userId)) return null;      // 선물은 학생만

        var completions = await GetMyCompletionsAsync(userId);
        if (completions.Count == 0) return null;

        var orders = await _db.BibleGiftOrders.AsNoTracking()
                              .Where(o => o.UserId == userId && o.IsCurrentDraft)
                              .ToListAsync();

        // 아직 안 고른 회차가 있으면 그 회차를 보여 준다.
        // 없으면 골랐지만 아직 전달되지 않은 회차 중 가장 최근 것을.
        // 전부 전달까지 끝났으면 배너를 내린다 — 받은 선물은 선물함의 '지난 선물'에만 남는다.
        int idx = completions.FindIndex(c => !orders.Any(o => o.RoundNo == c.RoundNo));
        if (idx < 0)
            idx = completions.FindIndex(c => orders.Any(o => o.RoundNo == c.RoundNo
                                                            && !string.Equals(o.Status, BibleGiftStatus.Delivered, StringComparison.OrdinalIgnoreCase)));
        if (idx < 0) return null;

        var target = completions[idx];
        var cfg    = await FindConfigAsync(target.RoundNo);
        if (cfg == null) return null;

        var mine = orders.FirstOrDefault(o => o.RoundNo == target.RoundNo);
        string? productName = null;
        if (mine != null)
        {
            productName = await _db.AwardItems.AsNoTracking()
                                   .Where(a => a.ProductID == mine.ProductID)
                                   .Select(a => a.ProductName)
                                   .FirstOrDefaultAsync();
        }

        return new BibleGiftBannerVM
        {
            RoundNo      = target.RoundNo,
            CompletedAt  = target.CompletedAt,
            Title        = cfg.Title,
            SubTitle     = cfg.SubTitle,
            MaxUnitPrice = cfg.MaxUnitPrice,
            UserFullName = fullName ?? "",
            Status       = mine?.Status,
            ProductName  = productName,
            TotalRounds  = completions.Count
        };
    }

    // ══════════════════════════════════════════════════════
    //  선물 고르기 화면
    // ══════════════════════════════════════════════════════
    public async Task<BibleGiftSelectVM?> GetSelectPageAsync(string userId, int round)
    {
        var completions = await GetMyCompletionsAsync(userId);
        int doneIdx = completions.FindIndex(c => c.RoundNo == round);
        if (doneIdx < 0) return null;                       // 자격 없음
        if (!await IsStudentByIdAsync(userId)) return null; // 선물은 학생만

        var cfg = await FindConfigAsync(round);
        if (cfg == null) return null;

        var mine  = await _db.BibleGiftOrders.AsNoTracking()
                             .FirstOrDefaultAsync(o => o.UserId == userId && o.RoundNo == round && o.IsCurrentDraft);

        // 품목 조회는 GiftCatalog 한 곳으로 (숨김 제외 · 금액 오름차순은 그쪽 규칙)
        var items = await _catalog.GetAsync(BulletinBoard.Services.GiftCatalog.GiftCatalogQuery.All());

        return BuildSelectVM(cfg, round, completions[doneIdx].CompletedAt, mine, items);
    }

    public async Task<List<BibleGiftSelectVM>> GetMyRoundsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return new();
        if (!await IsStudentByIdAsync(userId)) return new();

        var completions = await GetMyCompletionsAsync(userId);
        if (completions.Count == 0) return new();

        var orders = await _db.BibleGiftOrders.AsNoTracking()
                              .Where(o => o.UserId == userId && o.IsCurrentDraft)
                              .ToListAsync();

        var items = await _catalog.GetAsync(BulletinBoard.Services.GiftCatalog.GiftCatalogQuery.All());

        var list = new List<BibleGiftSelectVM>();
        foreach (var c in completions.OrderBy(x => x.RoundNo))
        {
            var cfg = await FindConfigAsync(c.RoundNo);
            if (cfg == null) continue;

            var mine = orders.FirstOrDefault(o => o.RoundNo == c.RoundNo);
            list.Add(BuildSelectVM(cfg, c.RoundNo, c.CompletedAt, mine, items));
        }
        return list;
    }

    private static BibleGiftSelectVM BuildSelectVM(
        BibleGiftMilestoneConfig cfg, int round, DateTime completedAt,
        BibleGiftOrder? mine, List<BulletinBoard.Models.AwardItem> items)
    {
        var vm = new BibleGiftSelectVM
        {
            RoundNo           = round,
            CompletedAt       = completedAt,
            MaxUnitPrice      = cfg.MaxUnitPrice,
            Title             = cfg.Title,
            Status            = mine?.Status,
            PickedProductId   = mine?.ProductID,
            SelectedAt        = mine?.SelectedAt,
            Items = items.Select(a => new BibleGiftItemVM
            {
                ProductID   = a.ProductID,
                ProductName = a.ProductName,
                Description = a.ProductDescription,
                UnitPrice   = a.UnitPrice,
                SourceType  = a.SourceType,
                ImageUrl    = $"/images/AwardItem/{a.ProductID}.jpg",
                OverLimit   = cfg.MaxUnitPrice > 0 && a.UnitPrice > cfg.MaxUnitPrice,
                IsPicked    = mine != null && mine.ProductID == a.ProductID
            }).ToList()
        };
        vm.PickedProductName = vm.Items.FirstOrDefault(i => i.IsPicked)?.ProductName;
        var pickable = vm.Items.Where(i => !i.OverLimit).ToList();
        vm.MinUnitPrice = pickable.Count > 0 ? pickable.Min(i => i.UnitPrice) : 0;
        return vm;
    }

    // ══════════════════════════════════════════════════════
    //  구경 전용 (로그인 없이 목록만)
    // ══════════════════════════════════════════════════════
    public async Task<BibleGiftSelectVM?> GetBrowsePageAsync(int round)
    {
        var cfg = await FindConfigAsync(round);
        if (cfg == null) return null;

        // 품목 조회는 GiftCatalog 한 곳으로 (숨김 제외 · 금액 오름차순은 그쪽 규칙)
        var items = await _catalog.GetAsync(BulletinBoard.Services.GiftCatalog.GiftCatalogQuery.All());

        var list = items.Select(a => new BibleGiftItemVM
        {
            ProductID   = a.ProductID,
            ProductName = a.ProductName,
            Description = a.ProductDescription,
            UnitPrice   = a.UnitPrice,
            SourceType  = a.SourceType,
            ImageUrl    = $"/images/AwardItem/{a.ProductID}.jpg",
            OverLimit   = cfg.MaxUnitPrice > 0 && a.UnitPrice > cfg.MaxUnitPrice,
            IsPicked    = false
        }).ToList();

        var pick = list.Where(i => !i.OverLimit).ToList();

        return new BibleGiftSelectVM
        {
            RoundNo      = round,
            CompletedAt  = DateHelper.GetKoreaNow().Date,
            MaxUnitPrice = cfg.MaxUnitPrice,
            MinUnitPrice = pick.Count > 0 ? pick.Min(i => i.UnitPrice) : 0,
            Title        = cfg.Title,
            IsReadOnly   = true,
            Items        = list
        };
    }

    // ══════════════════════════════════════════════════════
    //  고르기 / 바꾸기
    // ══════════════════════════════════════════════════════
    public async Task<(bool ok, string msg)> SelectAsync(string userId, string? userName, int round, int productId)
    {
        var completions = await GetMyCompletionsAsync(userId);
        if (!completions.Any(c => c.RoundNo == round))
            return (false, "아직 이 회차를 마치지 않았어요.");
        if (!await IsStudentByIdAsync(userId)) return (false, "선물은 학생만 고를 수 있어요.");

        var cfg = await FindConfigAsync(round);
        if (cfg == null) return (false, "선물 기준이 아직 준비되지 않았어요.");

        var item = await _catalog.GetByIdAsync(productId);
        if (item == null) return (false, "고를 수 없는 선물이에요.");
        if (cfg.MaxUnitPrice > 0 && item.UnitPrice > cfg.MaxUnitPrice)
            return (false, "선물 한도를 넘는 선물이에요.");

        var now  = DateHelper.GetKoreaNow();
        var mine = await GetCurrentAsync(userId, round);

        if (mine != null && (mine.Status == BibleGiftStatus.Purchased || mine.Status == BibleGiftStatus.Delivered))
            return (false, "이미 선물을 준비하고 있어서 바꿀 수 없어요.");

        if (mine == null)
        {
            mine = new BibleGiftOrder
            {
                UserId            = userId,
                UserName          = userName,
                RoundNo           = round,
                ConfigId          = cfg.Id,
                ProductID         = productId,
                UnitPriceSnapshot = item.UnitPrice,
                Status            = BibleGiftStatus.Draft,
                IsCurrentDraft    = true,
                SelectedAt        = now,
                CreatedAt         = now,
                UpdatedAt         = now
            };
            _db.BibleGiftOrders.Add(mine);
            await _db.SaveChangesAsync();
            AddLog(mine, userId, userName, round, "SELECT", productId, item.UnitPrice, userName, "USER");
        }
        else
        {
            var changed = mine.ProductID != productId;
            mine.ProductID         = productId;
            mine.UnitPriceSnapshot = item.UnitPrice;
            mine.ConfigId          = cfg.Id;
            mine.SelectedAt        = now;
            mine.UpdatedAt         = now;
            AddLog(mine, userId, userName, round, changed ? "CHANGE" : "SELECT", productId, item.UnitPrice, userName, "USER");
        }

        await _db.SaveChangesAsync();
        return (true, "선물을 골랐어요.");
    }

    // ══════════════════════════════════════════════════════
    //  선택 취소
    // ══════════════════════════════════════════════════════
    public async Task<(bool ok, string msg)> CancelAsync(string userId, int round)
    {
        var mine = await GetCurrentAsync(userId, round);
        if (mine == null) return (false, "고른 선물이 없어요.");
        if (mine.Status == BibleGiftStatus.Purchased || mine.Status == BibleGiftStatus.Delivered)
            return (false, "이미 선물을 준비하고 있어서 취소할 수 없어요.");

        var now = DateHelper.GetKoreaNow();
        mine.Status         = BibleGiftStatus.Canceled;
        mine.IsCurrentDraft = false;                 // 자리를 비워 다시 고를 수 있게 한다
        mine.UpdatedAt      = now;
        AddLog(mine, userId, mine.UserName, round, "CANCEL", mine.ProductID, mine.UnitPriceSnapshot, mine.UserName, "USER");

        await _db.SaveChangesAsync();
        return (true, "선택을 취소했어요.");
    }

    // ══════════════════════════════════════════════════════
    //  관리자 목록
    // ══════════════════════════════════════════════════════
    public async Task<BibleGiftAdminVM> GetAdminAsync(string tab)
    {
        var logs = await _db.BibleCompletionLogs.AsNoTracking()
                            .OrderByDescending(l => l.CompletedAt)
                            .ToListAsync();

        var orders = await _db.BibleGiftOrders.AsNoTracking()
                              .Where(o => o.IsCurrentDraft)
                              .ToListAsync();

        // ⚠️ DB 호환 수준이 낮아 IN(목록) 번역이 안 되므로 품목은 통째로 읽어 메모리에서 붙인다.
        var items = (await _db.AwardItems.AsNoTracking().ToListAsync())
                    .ToDictionary(a => a.ProductID);

        var persons = await GetPersonMapAsync();

        var rows = new List<BibleGiftAdminRowVM>();
        foreach (var l in logs)
        {
            if (!IsStudent(persons, l.UserName)) continue;      // 선물은 학생만
            persons.TryGetValue(l.UserName ?? "", out var p);

            var o = orders.FirstOrDefault(x => x.UserId == l.UserId && x.RoundNo == l.RoundNo);
            items.TryGetValue(o?.ProductID ?? -1, out var it);

            rows.Add(new BibleGiftAdminRowVM
            {
                OrderId     = o?.OrderId ?? 0,
                UserId      = l.UserId,
                UserName    = l.UserName ?? "",
                FullName    = string.IsNullOrWhiteSpace(p?.FullName) ? (l.UserName ?? "") : p!.FullName!,
                RoundNo     = l.RoundNo,
                CompletedAt = l.CompletedAt,
                ProductName = it?.ProductName,
                UnitPrice   = o?.UnitPriceSnapshot,
                SourceType  = it?.SourceType,
                ImageUrl    = o != null ? $"/images/AwardItem/{o.ProductID}.jpg" : null,
                ExternalUrl = it?.ExternalUrl,
                Status      = o?.Status ?? "",
                SelectedAt  = o?.SelectedAt,
                PurchasedAt = o?.PurchasedAt,
                DeliveredAt = o?.DeliveredAt,
                AdminId     = o?.AdminId
            });
        }

        // 완독 기록이 아직 없는 회차에 고른 선물도 빠짐없이 보여 준다.
        //  (완독 로그는 본인이 '숨기기'를 눌러야 남는 경우가 있어, 로그만 기준으로 하면
        //   학생이 신청한 선물이 관리 화면에서 통째로 사라진다)
        var shownOrderIds = rows.Where(r => r.OrderId > 0).Select(r => r.OrderId).ToHashSet();
        foreach (var o in orders)
        {
            if (shownOrderIds.Contains(o.OrderId)) continue;
            if (o.Status == BibleGiftStatus.Canceled) continue;
            if (!IsStudent(persons, o.UserName)) continue;

            persons.TryGetValue(o.UserName ?? "", out var p);
            items.TryGetValue(o.ProductID, out var it);

            rows.Add(new BibleGiftAdminRowVM
            {
                OrderId     = o.OrderId,
                UserId      = o.UserId,
                UserName    = o.UserName ?? "",
                FullName    = string.IsNullOrWhiteSpace(p?.FullName) ? (o.UserName ?? "") : p!.FullName!,
                RoundNo     = o.RoundNo,
                CompletedAt = o.SelectedAt ?? o.CreatedAt,   // 완독 로그가 없으니 고른 날로 대신한다
                ProductName = it?.ProductName,
                UnitPrice   = o.UnitPriceSnapshot,
                SourceType  = it?.SourceType,
                ImageUrl    = $"/images/AwardItem/{o.ProductID}.jpg",
                ExternalUrl = it?.ExternalUrl,
                Status      = o.Status,
                SelectedAt  = o.SelectedAt,
                PurchasedAt = o.PurchasedAt,
                DeliveredAt = o.DeliveredAt,
                AdminId     = o.AdminId
            });
        }

        var vm = new BibleGiftAdminVM
        {
            Tab            = string.IsNullOrWhiteSpace(tab) ? "picked" : tab,
            PickedCount    = rows.Count(r => r.Status == BibleGiftStatus.Draft),
            WaitingCount   = rows.Count(r => r.Status == ""),
            PurchasedCount = rows.Count(r => r.Status == BibleGiftStatus.Purchased),
            DeliveredCount = rows.Count(r => r.Status == BibleGiftStatus.Delivered),
            PurchasedSum   = rows.Where(r => r.Status == BibleGiftStatus.Purchased
                                          || r.Status == BibleGiftStatus.Delivered).Sum(r => r.UnitPrice ?? 0)
        };

        vm.Rows = vm.Tab switch
        {
            "waiting"   => rows.Where(r => r.Status == "").ToList(),
            "purchased" => rows.Where(r => r.Status == BibleGiftStatus.Purchased).ToList(),
            "delivered" => rows.Where(r => r.Status == BibleGiftStatus.Delivered).ToList(),
            "all"       => rows,
            _           => rows.Where(r => r.Status == BibleGiftStatus.Draft).ToList()
        };
        return vm;
    }

    // ══════════════════════════════════════════════════════
    //  구매 완료 / 되돌리기
    // ══════════════════════════════════════════════════════
    public async Task<(bool ok, string msg)> PurchaseAsync(int orderId, string adminId)
    {
        var o = await _db.BibleGiftOrders.FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (o == null) return (false, "선물 건을 찾을 수 없어요.");
        if (o.Status == BibleGiftStatus.Purchased) return (false, "이미 구매 완료예요.");

        var now = DateHelper.GetKoreaNow();
        o.Status      = BibleGiftStatus.Purchased;
        o.PurchasedAt = now;
        o.AdminId     = adminId;
        o.UpdatedAt   = now;
        AddLog(o, o.UserId, o.UserName, o.RoundNo, "PURCHASE", o.ProductID, o.UnitPriceSnapshot, adminId, "ADMIN");

        await _db.SaveChangesAsync();
        return (true, "구매 완료로 바꿨어요.");
    }

    public async Task<(bool ok, string msg)> UndoPurchaseAsync(int orderId, string adminId)
    {
        var o = await _db.BibleGiftOrders.FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (o == null) return (false, "선물 건을 찾을 수 없어요.");
        if (o.Status != BibleGiftStatus.Purchased) return (false, "구매 완료 상태가 아니에요.");

        var now = DateHelper.GetKoreaNow();
        o.Status      = BibleGiftStatus.Draft;
        o.PurchasedAt = null;
        o.AdminId     = adminId;
        o.UpdatedAt   = now;
        AddLog(o, o.UserId, o.UserName, o.RoundNo, "UNDO", o.ProductID, o.UnitPriceSnapshot, adminId, "ADMIN");

        await _db.SaveChangesAsync();
        return (true, "구매를 되돌렸어요.");
    }

    public async Task<(bool ok, string msg)> DeliverAsync(int orderId, string adminId)
    {
        var o = await _db.BibleGiftOrders.FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (o == null) return (false, "선물 건을 찾을 수 없어요.");
        if (o.Status == BibleGiftStatus.Delivered) return (false, "이미 전달 완료예요.");
        if (o.Status != BibleGiftStatus.Purchased) return (false, "먼저 구매 완료로 바꿔 주세요.");

        var now = DateHelper.GetKoreaNow();
        o.Status      = BibleGiftStatus.Delivered;
        o.DeliveredAt = now;
        o.AdminId     = adminId;
        o.UpdatedAt   = now;
        AddLog(o, o.UserId, o.UserName, o.RoundNo, "DELIVER", o.ProductID, o.UnitPriceSnapshot, adminId, "ADMIN");

        await _db.SaveChangesAsync();
        return (true, "전달 완료로 바꿨어요.");
    }

    public async Task<(bool ok, string msg)> UndoDeliverAsync(int orderId, string adminId)
    {
        var o = await _db.BibleGiftOrders.FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (o == null) return (false, "선물 건을 찾을 수 없어요.");
        if (o.Status != BibleGiftStatus.Delivered) return (false, "전달 완료 상태가 아니에요.");

        var now = DateHelper.GetKoreaNow();
        o.Status      = BibleGiftStatus.Purchased;
        o.DeliveredAt = null;
        o.AdminId     = adminId;
        o.UpdatedAt   = now;
        AddLog(o, o.UserId, o.UserName, o.RoundNo, "UNDODLV", o.ProductID, o.UnitPriceSnapshot, adminId, "ADMIN");

        await _db.SaveChangesAsync();
        return (true, "전달을 되돌렸어요.");
    }

    // ══════════════════════════════════════════════════════
    //  사람별 지금 고른 선물 (홈 카드용)
    // ══════════════════════════════════════════════════════
    public async Task<Dictionary<string, BibleGiftPickVM>> GetCurrentPicksAsync()
    {
        var result = new Dictionary<string, BibleGiftPickVM>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var orders = await _db.BibleGiftOrders.AsNoTracking()
                                  .Where(o => o.IsCurrentDraft)
                                  .ToListAsync();
            if (orders.Count == 0) return result;

            var items = (await _db.AwardItems.AsNoTracking().ToListAsync())
                        .ToDictionary(a => a.ProductID);

            // 사람+회차마다 한 건 (같은 회차에 여러 건이면 가장 최근 것)
            foreach (var g in orders.GroupBy(o => new { o.UserId, o.RoundNo }))
            {
                var o = g.OrderByDescending(x => x.OrderId).First();
                items.TryGetValue(o.ProductID, out var it);
                result[BibleGiftKeys.Of(o.UserId, o.RoundNo)] = new BibleGiftPickVM
                {
                    RoundNo     = o.RoundNo,
                    ProductID   = o.ProductID,
                    ProductName = it?.ProductName ?? "고른 선물",
                    Description = it?.ProductDescription,
                    ExternalUrl = it?.ExternalUrl,
                    ImageUrl    = $"/images/AwardItem/{o.ProductID}.jpg",
                    Status      = o.Status
                };
            }
        }
        catch { /* 표가 없으면 빈 값으로 */ }
        return result;
    }

    // ══════════════════════════════════════════════════════
    //  강제 삭제 — 그 사람의 그 회차 선물 기록·이력을 모두 지운다 (되돌릴 수 없음)
    // ══════════════════════════════════════════════════════
    public async Task<(bool ok, string msg)> ForceDeleteAsync(string userId, int round, string adminId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return (false, "대상을 찾을 수 없어요.");

        var orders = await _db.BibleGiftOrders
                              .Where(o => o.UserId == userId && o.RoundNo == round)
                              .ToListAsync();
        var logs   = await _db.BibleGiftOrderLogs
                              .Where(l => l.UserId == userId && l.RoundNo == round)
                              .ToListAsync();

        if (orders.Count == 0 && logs.Count == 0) return (false, "지울 기록이 없어요.");

        _db.BibleGiftOrders.RemoveRange(orders);
        _db.BibleGiftOrderLogs.RemoveRange(logs);
        await _db.SaveChangesAsync();

        return (true, $"{round}독 선물 기록 {orders.Count}건과 이력 {logs.Count}건을 지웠어요.");
    }

    // 이력 화면은 없앴다. 기록(BibleGiftOrderLogs)은 계속 남긴다 — 문제가 생겼을 때 추적용.
}

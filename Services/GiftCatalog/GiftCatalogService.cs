using BibleReadingThrough.Data;
using BulletinBoard.Common;
using BulletinBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Services.GiftCatalog;

/// <summary>선물 품목을 찾는 조건 — 한 곳에서만 만든다.</summary>
public sealed class GiftCatalogQuery
{
    /// <summary>목록 종류 (생일 · 새친구 · 가족팀 · 도전! 성경말씀 …)</summary>
    public ProductDisplayOpton Option { get; init; } = ProductDisplayOpton.none;

    /// <summary>도전! 성경말씀 선물처럼 점수로 상한이 정해지는 경우</summary>
    public int MyPoint { get; init; }

    /// <summary>이 값이 있으면 종류·점수와 무관하게 이 금액 이하만 (도전! 성경통독 완독 선물의 한도)</summary>
    public int? MaxUnitPrice { get; init; }

    /// <summary>한 건만 찾을 때</summary>
    public int? ProductId { get; init; }

    /// <summary>숨긴 품목까지 포함할지 (관리 화면만 true)</summary>
    public bool IncludeHidden { get; init; }

    /// <summary>가격 제한을 아예 걸지 않는다 (관리 화면 · 구경용 전체 목록)</summary>
    public bool IgnorePriceRule { get; init; }

    public static GiftCatalogQuery ById(int productId, bool includeHidden = false)
        => new() { ProductId = productId, IncludeHidden = includeHidden, IgnorePriceRule = true };

    public static GiftCatalogQuery All(bool includeHidden = false)
        => new() { IncludeHidden = includeHidden, IgnorePriceRule = true };

    public static GiftCatalogQuery ByLimit(int maxUnitPrice)
        => new() { MaxUnitPrice = maxUnitPrice, IgnorePriceRule = true };
}

public interface IGiftCatalogService
{
    /// <summary>조건에 맞는 선물 품목 (금액 오름차순)</summary>
    Task<List<AwardItem>> GetAsync(GiftCatalogQuery query);

    /// <summary>한 건</summary>
    Task<AwardItem?> GetByIdAsync(int productId, bool includeHidden = false);

    /// <summary>고를 수 있는 선물인지 확인 (서버 쪽 최종 검사)</summary>
    Task<bool> CanPickAsync(int productId, GiftCatalogQuery query);
}

/// <summary>
///  선물 품목 조회 <b>단일 창구</b>.
///  · 예전: ControlPresent(문자열 SQL) · AwardItemService · StudentPointGiftService ·
///          BibleGiftService 가 각자 AwardItem 을 조회했다.
///  · 지금: 가격 규칙은 GiftPriceRule, 조회는 이 서비스 하나로 모은다.
/// </summary>
public sealed class GiftCatalogService : IGiftCatalogService
{
    private readonly AppDbContext _db;
    public GiftCatalogService(AppDbContext db) => _db = db;

    public async Task<List<AwardItem>> GetAsync(GiftCatalogQuery query)
    {
        var q = _db.AwardItems.AsNoTracking().AsQueryable();

        if (query.ProductId is int pid)
            q = q.Where(a => a.ProductID == pid);

        if (!query.IncludeHidden)
            q = q.Where(a => !a.IsHidden);

        // 설명이 없는 품목만 보는 관리용 목록
        if (GiftPriceRule.IsDescriptionMissingOnly(query.Option))
            q = q.Where(a => a.ProductDescription == null || a.ProductDescription == "");

        else if (!query.IgnorePriceRule)
        {
            var r = GiftPriceRule.For(query.Option, query.MyPoint);
            q = q.Where(a => a.UnitPrice >= r.Min);
            if (r.Max > 0) q = q.Where(a => a.UnitPrice <= r.Max);
        }

        if (query.MaxUnitPrice is int cap && cap > 0)
            q = q.Where(a => a.UnitPrice <= cap);

        var list = await q.OrderBy(a => a.UnitPrice).ThenBy(a => a.ProductID).ToListAsync();

        // 화면에서 쓰는 "몇 점 이상" 문구를 채워 준다 (표에는 없는 계산 값)
        foreach (var it in list)
            it.PointRequirement = GiftPriceRule.PointRequirementLabel(it.UnitPrice);

        return list;
    }

    public async Task<AwardItem?> GetByIdAsync(int productId, bool includeHidden = false)
        => (await GetAsync(GiftCatalogQuery.ById(productId, includeHidden))).FirstOrDefault();

    public async Task<bool> CanPickAsync(int productId, GiftCatalogQuery query)
    {
        var item = await GetByIdAsync(productId, includeHidden: false);
        if (item == null) return false;

        if (query.MaxUnitPrice is int cap && cap > 0 && item.UnitPrice > cap) return false;

        if (!query.IgnorePriceRule)
        {
            var r = GiftPriceRule.For(query.Option, query.MyPoint);
            if (!r.Contains(item.UnitPrice)) return false;
        }
        return true;
    }
}

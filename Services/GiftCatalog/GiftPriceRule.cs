using BulletinBoard.Common;

namespace BulletinBoard.Services.GiftCatalog;

/// <summary>
///  선물 가격 규칙 — <b>여기 한 곳</b>에만 둔다.
///  · 예전에는 ControlPresent.fetchPresent / SharedComm.UnitPriceRangeSQL /
///    ProductDisplayOptonHelper 에 흩어져 있던 규칙을 모았다.
///  · SQL 문자열이 아니라 값(최소·최대 금액)으로 돌려주므로 EF·ADO 어느 쪽에서도 쓸 수 있다.
/// </summary>
public static class GiftPriceRule
{
    /// <summary>가격 범위 (원). Max = 0 이면 위쪽 제한 없음.</summary>
    public readonly record struct Range(int Min, int Max)
    {
        public bool Contains(int price) => price >= Min && (Max <= 0 || price <= Max);
        public override string ToString() => Max <= 0 ? $"{Min:N0}원 이상" : $"{Min:N0} ~ {Max:N0}원";
    }

    /// <summary>내 점수로 고를 수 있는 가격 상한 (도전! 성경말씀 선물)</summary>
    public static int MaxPriceByPoint(int myPoint) => myPoint switch
    {
        >= 2000 => 30000,
        >= 1500 => 25000,
        >= 1000 => 20000,
        >=  700 => 15000,
        >=  400 => 10000,
        >=  100 =>  5000,
        _       =>  1000      // 100점 미만은 1,000원 이하만
    };

    /// <summary>선물 목록 종류(+내 점수) → 가격 범위</summary>
    public static Range For(ProductDisplayOpton option, int myPoint = 0)
    {
        // 가족팀 합산 선물 : 2K/3K/4K/5K = 그 금액 "미만"
        if (ProductDisplayOptonHelper.FamilyTeamPriceLimit(option) is int familyLimit)
            return new Range(0, familyLimit - 1);

        return option switch
        {
            ProductDisplayOpton.challengeMissionBible => myPoint >= 100
                                                         ? new Range(1000, MaxPriceByPoint(myPoint))
                                                         : new Range(0, 1000),
            ProductDisplayOpton.birth      => new Range(1000, ConstatsCommon.birthDayPresentMax),
            ProductDisplayOpton.Special    => new Range(0, 4999),
            ProductDisplayOpton.newComer   => new Range(1000, 15000),
            ProductDisplayOpton.display    => new Range(15000, 28000),
            ProductDisplayOpton.managePrice => new Range(0, 0),        // 금액 제한 없음(설명 없는 것만 고른다)
            _                              => new Range(0, 1000)       // 모르면 가장 좁게
        };
    }

    /// <summary>설명이 비어 있는 품목만 보는 관리용 목록인가</summary>
    public static bool IsDescriptionMissingOnly(ProductDisplayOpton option)
        => option == ProductDisplayOpton.managePrice;

    /// <summary>가격 → "몇 점 이상" 안내 문구 (예전 SharedComm.PriceListCase 와 같은 기준)</summary>
    public static string? PointRequirementLabel(int unitPrice) => unitPrice switch
    {
        <=  5000 => "100점이상",
        <= 10000 => "400점이상",
        <= 15000 => "700점이상",
        <= 20000 => "1000점이상",
        <= 25000 => "1500점이상",
        <= 30000 => "2000점이상",
        _        => null
    };

    /// <summary>
    ///  "몇 점 이상" 문구를 SQL 안에서 만들 때 쓰는 CASE 조각.
    ///  (예전 SharedComm.PriceListCase — 기준은 <see cref="PointRequirementLabel"/> 과 같다)
    /// </summary>
    public static string PointRequirementCaseSql() => @"
        CASE
            WHEN UnitPrice <=  5000 THEN  '100점이상'
            WHEN UnitPrice <= 10000 THEN  '400점이상'
            WHEN UnitPrice <= 15000 THEN  '700점이상'
            WHEN UnitPrice <= 20000 THEN '1000점이상'
            WHEN UnitPrice <= 25000 THEN '1500점이상'
            WHEN UnitPrice <= 30000 THEN '2000점이상'
        END AS PointRequirement ";

    /// <summary>ADO 조회에서 쓸 WHERE 조각 (파라미터 없이 숫자만 넣으므로 주입 위험이 없다)</summary>
    public static string WhereSql(ProductDisplayOpton option, int myPoint = 0)
    {
        if (IsDescriptionMissingOnly(option))
            return " AND ISNULL([ProductDescription],'') = '' ";

        var r = For(option, myPoint);
        var sql = $" AND UnitPrice >= {r.Min} ";
        if (r.Max > 0) sql += $" AND UnitPrice <= {r.Max} ";
        return sql;
    }
}

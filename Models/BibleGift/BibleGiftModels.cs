using System;
using System.Collections.Generic;

namespace BibleReadingThrough.Models.BibleGift;

/// <summary>몇 독에 얼마짜리 선물을 주는지 정하는 기준표</summary>
public class BibleGiftMilestoneConfig
{
    public int      Id           { get; set; }
    public int      CYear        { get; set; }
    public int      RoundNo      { get; set; }
    public string   Title        { get; set; } = "";
    public string?  SubTitle     { get; set; }
    public int      MaxUnitPrice { get; set; }      // 0 = 제한 없음
    public bool     IsActive     { get; set; } = true;
    public int      SortOrder    { get; set; }
    public DateTime CreatedAt    { get; set; }
}

/// <summary>누가 몇 독으로 어떤 선물을 골랐고 구매했는지</summary>
public class BibleGiftOrder
{
    public int       OrderId           { get; set; }
    public string    UserId            { get; set; } = "";
    public string?   UserName          { get; set; }
    public int       RoundNo           { get; set; }
    public int?      ConfigId          { get; set; }
    public int       ProductID         { get; set; }
    public int       UnitPriceSnapshot { get; set; }
    public string    Status            { get; set; } = BibleGiftStatus.Draft;
    public bool      IsCurrentDraft    { get; set; } = true;
    public DateTime? SelectedAt        { get; set; }
    public DateTime? PurchasedAt       { get; set; }
    public DateTime? DeliveredAt       { get; set; }
    public string?   AdminId           { get; set; }
    public string?   Memo              { get; set; }
    public DateTime  CreatedAt         { get; set; }
    public DateTime  UpdatedAt         { get; set; }
}

/// <summary>고름/바꿈/취소/구매/되돌림 이력 (지우지 않는다)</summary>
public class BibleGiftOrderLog
{
    public long      LogId     { get; set; }
    public int?      OrderId   { get; set; }
    public string    UserId    { get; set; } = "";
    public string?   UserName  { get; set; }
    public int       RoundNo   { get; set; }
    public string    Action    { get; set; } = "";   // SELECT / CHANGE / CANCEL / PURCHASE / UNDO
    public int?      ProductID { get; set; }
    public int?      UnitPrice { get; set; }
    public string?   ActorId   { get; set; }
    public string?   ActorRole { get; set; }         // USER / ADMIN
    public DateTime  CreatedAt { get; set; }
}

public static class BibleGiftStatus
{
    public const string Draft     = "DRAFT";
    public const string Purchased = "PURCHASED";
    public const string Delivered = "DELIVERED";
    public const string Canceled  = "CANCELED";
}

/// <summary>view_UserCompositeInfo_2026 — 이름·역할만 읽는다 (T1001 = 학생)</summary>
public class UserCompositeInfo
{
    public string? UserId   { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? RoleCode { get; set; }
}

// ══════════════════════════════════════════════════════════════
//  화면용 모델
// ══════════════════════════════════════════════════════════════

/// <summary>홈 배너</summary>
public class BibleGiftBannerVM
{
    public int       RoundNo      { get; set; }
    public DateTime  CompletedAt  { get; set; }
    public string    Title        { get; set; } = "";
    public string?   SubTitle     { get; set; }
    public string    UserFullName { get; set; } = "";
    public int       MaxUnitPrice { get; set; }

    /// <summary>null = 아직 안 고름</summary>
    public string?   Status       { get; set; }
    public string?   ProductName  { get; set; }
    public int       TotalRounds  { get; set; }     // 지금까지 완독 회차 수

    public bool IsPicked    => Status == BibleGiftStatus.Draft;
    public bool IsPurchased => Status == BibleGiftStatus.Purchased || Status == BibleGiftStatus.Delivered;
}

public class BibleGiftItemVM
{
    public int     ProductID   { get; set; }
    public string  ProductName { get; set; } = "";
    public string? Description { get; set; }
    public int     UnitPrice   { get; set; }
    public string? SourceType  { get; set; }
    public string? ImageUrl    { get; set; }
    public bool    OverLimit   { get; set; }
    public bool    IsPicked    { get; set; }
}

public class BibleGiftSelectVM
{
    public int      RoundNo      { get; set; }
    public DateTime CompletedAt  { get; set; }
    public string   UserFullName { get; set; } = "";
    public int      MaxUnitPrice { get; set; }
    public string   Title        { get; set; } = "";

    public string? Status            { get; set; }
    public int?    PickedProductId   { get; set; }
    public string? PickedProductName { get; set; }
    public DateTime? SelectedAt      { get; set; }

    /// <summary>고를 수 없는 구경 전용 화면 (로그인 안 함 · 대상자 아님)</summary>
    public bool IsReadOnly { get; set; }
    public int  MinUnitPrice { get; set; }

    public bool Locked => Status == BibleGiftStatus.Purchased || Status == BibleGiftStatus.Delivered;
    public bool IsDelivered => Status == BibleGiftStatus.Delivered;
    public List<BibleGiftItemVM> Items { get; set; } = new();
}

public class BibleGiftAdminRowVM
{
    public int       OrderId     { get; set; }
    public string    UserId      { get; set; } = "";
    public string    UserName    { get; set; } = "";
    public string    FullName    { get; set; } = "";
    public int       RoundNo     { get; set; }
    public DateTime  CompletedAt { get; set; }
    public string?   ProductName { get; set; }
    public int?      UnitPrice   { get; set; }
    public string?   SourceType  { get; set; }
    public string?   ImageUrl    { get; set; }
    public string?   ExternalUrl { get; set; }
    public string    Status      { get; set; } = "";   // "" = 아직 안 고름
    public DateTime? SelectedAt  { get; set; }
    public DateTime? PurchasedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string?   AdminId     { get; set; }
}

public class BibleGiftAdminVM
{
    public string Tab { get; set; } = "picked";       // picked / waiting / purchased / all
    public int PickedCount    { get; set; }
    public int WaitingCount   { get; set; }
    public int PurchasedCount { get; set; }
    public int DeliveredCount { get; set; }
    public int PurchasedSum   { get; set; }
    public List<BibleGiftAdminRowVM> Rows { get; set; } = new();
}

/// <summary>회차별 선물을 찾을 때 쓰는 열쇠 (사람 + 회차)</summary>
public static class BibleGiftKeys
{
    public static string Of(string? userId, int roundNo) => $"{userId}|{roundNo}";
}

/// <summary>홈 카드에 보여 줄 "이 사람이 고른 선물"</summary>
public class BibleGiftPickVM
{
    public int     RoundNo     { get; set; }
    public int     ProductID   { get; set; }
    public string  ProductName { get; set; } = "";
    public string? Description { get; set; }
    public string? ExternalUrl { get; set; }
    public string  ImageUrl    { get; set; } = "";
    public string  Status      { get; set; } = "";
    // 홈 완독 카드의 선물 줄 — 홈 선물 카드 모두 같은 말 : 신청중 · 구매완료 · 전달완료 (2026-09-27 통일)
    public string  StatusLabel => Status switch
    {
        BibleGiftStatus.Delivered => "전달완료",
        BibleGiftStatus.Purchased => "구매완료",
        _                         => "신청중"
    };
}


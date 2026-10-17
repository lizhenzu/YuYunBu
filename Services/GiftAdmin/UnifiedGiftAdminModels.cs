using System;
using System.Collections.Generic;
using BulletinBoard.Common;

namespace BulletinBoard.Services.GiftAdmin
{
    /// <summary>선물 종류 — 관리 화면에서 행이 어느 시스템에서 왔는지 구분한다.</summary>
    public enum GiftKind
    {
        Point = 0,           // 나의 도토리 선물
        FamilyTeam = 1,      // 가족팀 선물 (Memo "가족팀:")
        FamilyCombined = 2,  // 가족 합산 시상 (Memo "가족합산:")
        Bible = 3            // 성경통독 완독 선물
    }

    /// <summary>
    ///  통합 상태. 시스템마다 실제 값이 달라서 여기로 모은다.
    ///   · 도토리·가족 : DRAFT → PURCHASED → HIDDEN(=전달완료)
    ///   · 성경통독    : DRAFT → PURCHASED → DELIVERED
    /// </summary>
    public enum GiftState
    {
        Waiting = 0,    // 받을 자격은 됐는데 아직 안 고름 (성경통독에만 있다)
        Picked = 1,     // 골랐고 관리자 승인 대기
        Purchased = 2,  // 구매 완료
        Delivered = 3   // 전달 완료
    }

    /// <summary>선물 종류가 달라도 관리 화면에서는 같은 모양으로 그린다.</summary>
    public sealed class UnifiedGiftAdminRowVM
    {
        public GiftKind Kind { get; set; }

        /// <summary>각 시스템의 주문 번호. 테이블이 달라 번호가 겹치므로 Kind와 짝으로만 쓴다.</summary>
        public int OrderId { get; set; }

        public string? UserId { get; set; }
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string PhotoUrl { get; set; } = "/images/icon-user.png";

        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? ImageUrl { get; set; }
        public string? ExternalUrl { get; set; }
        public int UnitPrice { get; set; }

        public GiftState State { get; set; }
        public DateTime? SelectedAt { get; set; }
        public DateTime? PurchasedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        /// <summary>왜 받는지 한 줄 — 도토리 "1,000점" · 성경 "2독" · 가족 "테스트 가족"</summary>
        public string ContextLabel { get; set; } = "";

        /// <summary>성경통독 강제 삭제에 필요 (사람 + 회차)</summary>
        public int RoundNo { get; set; }

        // 이름은 ConstatsCommon 한 곳에서 온다 — 화면에 같은 말을 또 적지 않는다
        public string KindLabel => Kind switch
        {
            GiftKind.Point          => ConstatsCommon.myDoToRi,
            GiftKind.FamilyTeam     => "가족팀",
            GiftKind.FamilyCombined => $"{ConstatsCommon.MainKeyTitle} 가족합산",
            GiftKind.Bible          => ConstatsCommon.ReadBibleAllTitleButton,
            _                       => ""
        };

        public string StateLabel => State switch
        {
            GiftState.Waiting   => "아직 안 고름",
            GiftState.Picked    => "신청중 (승인 대기)",
            GiftState.Purchased => "구매완료",
            GiftState.Delivered => "전달완료",
            _                   => ""
        };

        public bool CanPurchase      => State == GiftState.Picked;
        public bool CanUndoPurchase  => State == GiftState.Purchased;
        public bool CanDeliver       => State == GiftState.Purchased;
        public bool CanUndoDeliver   => State == GiftState.Delivered;
        /// <summary>강제 삭제는 성경통독에만 있다 (SuperAdmin 전용)</summary>
        public bool CanForceDelete   => Kind == GiftKind.Bible && State != GiftState.Waiting;
    }

    /// <summary>
    ///  통합 관리 화면 한 장.
    ///  미처리(전달 전)는 언제나 전부 보여 주고, 종류 거르기는 전달 완료 묶음에만 건다.
    /// </summary>
    public sealed class UnifiedGiftAdminPageVM
    {
        /// <summary>전달 완료 묶음에만 적용되는 종류 거르기</summary>
        public GiftKind? KindFilter { get; set; }

        /// <summary>아직 전달 전 — 거르지 않는다</summary>
        public List<UnifiedGiftAdminRowVM> PendingRows { get; set; } = new();

        /// <summary>전달 완료 — KindFilter 적용된 목록</summary>
        public List<UnifiedGiftAdminRowVM> DeliveredRows { get; set; } = new();

        // 미처리 집계
        public int WaitingCount   { get; set; }
        public int PickedCount    { get; set; }
        public int PurchasedCount { get; set; }

        // 전달 완료 집계 — 거르기 전 기준 (칩 옆 숫자)
        public int DeliveredTotalCount     { get; set; }
        public int DeliveredPointCount     { get; set; }
        public int DeliveredFamilyCombinedCount { get; set; }
        public int DeliveredBibleCount     { get; set; }

        /// <summary>구매완료 + 전달완료 금액 합계</summary>
        public int PurchasedSum { get; set; }

        public bool IsSuperAdmin { get; set; }

        /// <summary>한쪽 조회가 실패했을 때 — 조용히 빠지지 않게 화면에 알린다</summary>
        public List<string> LoadErrors { get; set; } = new();
    }
}

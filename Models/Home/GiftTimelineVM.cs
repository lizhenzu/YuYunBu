using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Home
{
    /// <summary>홈 "축하해 주세요~~" — 아이 한 명의 선물 점수 타임라인</summary>
    public class GiftTimelineKidVM
    {
        public string UserName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Photo { get; set; } = "/images/icon/people/people_01.webp";
        public string? ClassLabel { get; set; }          // 학생만 "2학년 2반"
        public string? RoleCode { get; set; }            // T1001 학생 · T1002~ 교사 (홈 축하 요약 카드 표시용, 2026-10-04)
        public int Total { get; set; }                   // 지금 도토리 점수
        public bool IsMe { get; set; }
        public List<GiftTimelineItemVM> Items { get; set; } = new();   // 점수 오름차순
        public int? PendingMilestone { get; set; }       // 닿았는데 아직 안 고른 단계 (나에게만)
        public int? NextPoint { get; set; }              // 다음 선물 점수 (없으면 최고 단계)
        public DateTime LastAt { get; set; }             // 최근 활동 (정렬용)
    }

    public class GiftTimelineItemVM
    {
        public int Milestone { get; set; }               // 이 선물을 신청한 점수
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string Status { get; set; } = "";         // DRAFT=신청중 · PURCHASED=구매완료 · HIDDEN=전달완료
        public DateTime CreatedAt { get; set; }           // 선물 고른 날
        public DateTime? PurchasedAt { get; set; }        // 선생님이 구매한 날
        public DateTime? DeliveredAt { get; set; }        // 전달한 날 (2026-09-26 부터 저장 — 그 전 선물은 null)
        public int CreditCost { get; set; }               // 쓴 credit
    }
}

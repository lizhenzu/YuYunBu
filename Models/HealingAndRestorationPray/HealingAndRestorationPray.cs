using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models.HealingAndRestorationPrays
{
    /// <summary>
    /// 치유와 회복을 위한 기도문 — [dbo].[HealingAndRestorationPray]
    /// </summary>
    public class HealingAndRestorationPray
    {
        public int PrayId { get; set; }

        [Display(Name = "순서")]
        public int SortOrder { get; set; }

        [Display(Name = "기도 제목")]
        public string Title { get; set; } = "";

        [Display(Name = "기도문")]
        public string PrayText { get; set; } = "";

        [Display(Name = "성경 구절")]
        public string BibleRef { get; set; } = "";

        [Display(Name = "성경 본문")]
        public string BibleText { get; set; } = "";

        [Display(Name = "읽은 수")]
        public int ClickCount { get; set; }

        [Display(Name = "좋아요")]
        public int LikeCount { get; set; }

        [Display(Name = "사용")]
        public string UseYN { get; set; } = "Y";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>순환 목록에서의 위치 (1부터). 목록 화면 표시용</summary>
        public int RotationNo { get; set; }

        // ── 검증 (2026-10-11, 부부 기도와 같은 구조) — 수정하면 다시 "검증 전"으로 ──
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDT { get; set; }
        public string VerifiedBy { get; set; } = "";
    }
}

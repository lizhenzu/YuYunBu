using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models.EntranceExamPrays
{
    /// <summary>
    /// 수시·수능 준비생을 위한 기도문 — [dbo].[EntranceExamPray]
    /// ※ 구조는 HealingAndRestorationPray 와 동일합니다.
    /// </summary>
    public class EntranceExamPray
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

        /// <summary>
        /// 매일 돌아가는 순환에 포함할지. 'N' 이면 순환에서 빠지고
        /// 목록에만 남아 필요한 날 직접 골라 읽습니다. (예: 수능 날 아침에)
        /// </summary>
        [Display(Name = "매일 순환")]
        public string RotateYN { get; set; } = "Y";

        /// <summary>순환에서 빠진 기도문인지 — 화면에서 쓰기 편하게</summary>
        public bool IsOccasion =>
            string.Equals(RotateYN, "N", System.StringComparison.OrdinalIgnoreCase);

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>순환 목록에서의 위치 (1부터). 목록 화면 표시용</summary>
        public int RotationNo { get; set; }
    }
}

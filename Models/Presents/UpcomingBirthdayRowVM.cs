// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class UpcomingBirthdayRowVM
    {
        public UserInfo User { get; set; } = default!;

        public DateTime NextBirthdayDate { get; set; } // 다음 생일 날짜(올해/내년)
        public int DDay { get; set; }                  // 오늘(KST) 기준 D-Day

        public bool HasPickedGift { get; set; }        // IsActive=1 선택 존재 여부
        public long? PickId { get; set; }

        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? SourceType { get; set; }

        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }

        public bool IsPurchased { get; set; }
        public DateTime? PurchasedAt { get; set; }

        // ✅ BirthdayGiftPickSms.Memo
        public string? Memo { get; set; }

        // ✅ 미처리 보류 상태 (BirthdayGiftSkip 테이블)
        public bool IsSkipped { get; set; }
    }
}

using System;

namespace BulletinBoard.ViewModels.Birthday
{
    public class BirthdayCardVM
    {
        public int PersonId { get; set; }
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string RoleLabel { get; set; } = "";
        public string ClassLabel { get; set; } = "";
        public int? BMonth { get; set; }
        public int? BDay { get; set; }
        public string PublishYN { get; set; } = "";

        public string? PhoneNumber { get; set; }
        public string? PhoneNumber2 { get; set; }

        public string ProfileImageUrl { get; set; } = "";

        // ===========================
        // ✅ 선물 선택 정보 (PresentItemPick)
        // ===========================
        public bool GiftPicked { get; set; } = false;
        public int? GiftProductId { get; set; }
        public string? GiftProductName { get; set; }
        public string? GiftProductDescription { get; set; }
        public DateTime? GiftPickedAt { get; set; }
        public bool GiftIsActive { get; set; } = false;
        public bool GiftIsSent { get; set; } = false;
        public DateTime? GiftSentAt { get; set; }
        public string? GiftSourceType { get; set; }

        // ===========================
        // ✅ 선물 선택 요청 문자(SMS) 발송 여부
        // ===========================
        public bool PickSmsSent { get; set; } = false;
        public DateTime? PickSmsSentAt { get; set; }

        // ===========================
        // ✅ 메모 / 비고
        // ===========================
        public string? Memo { get; set; }
    }
}

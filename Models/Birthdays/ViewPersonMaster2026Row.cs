using System;

namespace BulletinBoard.DataAccessLayer.Birthday.Models
{
    public class ViewPersonMaster2026Row
    {
        public int PersonId { get; set; }
        public string? UserId { get; set; }
        public string? PhoneNumber { get; set; }

        public int? bYear { get; set; }
        public int? bMonth { get; set; }
        public int? bDay { get; set; }

        public string? fullName { get; set; }
        public string? sex { get; set; }
        public string? PhoneNumber2 { get; set; }
        public string? HomeAaddress { get; set; }
        public string? school { get; set; }
        public int? readBibleAllCount { get; set; }
        public string? plusMins { get; set; }
        public string? birthDay_PublishYN { get; set; }

        public string? UserName { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? email { get; set; }

        public int? ClassYear { get; set; }
        public int? ClassBan { get; set; }

        public DateTime? insertDT { get; set; }
        public DateTime? updateDT { get; set; }

        // ===========================
        // ✅ PresentItemPick (선물 선택)
        // ===========================
        public int? PickId { get; set; }
        public int? PickProductId { get; set; }
        public string? PickProductName { get; set; }
        public string? PickProductDescription { get; set; }
        public DateTime? PickedAt { get; set; }
        public bool? PickIsActive { get; set; }
        public bool? PickIsSent { get; set; }
        public DateTime? PickSentAt { get; set; }
        public string? PickSourceType { get; set; }

        // ===========================
        // ✅ 선물 선택 요청 문자(SMS)
        // ===========================
        public bool? PickSmsSent { get; set; }
        public DateTime? PickSmsSentAt { get; set; }
        public string? PickMemo { get; set; }
    }
}

//using System;

//namespace BulletinBoard.ViewModels.Birthday
//{
//    public class BirthdayCardVM
//    {
//        public int PersonId { get; set; }
//        public string UserId { get; set; } = "";
//        public string FullName { get; set; } = "";
//        public string RoleLabel { get; set; } = "";    // 학생/교사/기타
//        public string ClassLabel { get; set; } = "";   // "3학년 2반" 등 (있으면)
//        public int? BMonth { get; set; }
//        public int? BDay { get; set; }
//        public string PublishYN { get; set; } = "";    // birthDay_PublishYN

//        public string? PhoneNumber { get; set; }
//        public string? PhoneNumber2 { get; set; }

//        public string ProfileImageUrl { get; set; } = "";

//        // ===========================
//        // ✅ 선물 선택 정보 (PresentItemPick)
//        // ===========================

//        /// <summary>
//        /// 추천 기준(A): PickId가 있고 IsActive=1 일 때만 true
//        /// </summary>
//        public bool GiftPicked { get; set; } = false;

//        public int? GiftProductId { get; set; }
//        public string? GiftProductName { get; set; }
//        public string? GiftProductDescription { get; set; }

//        public DateTime? GiftPickedAt { get; set; }

//        public bool GiftIsActive { get; set; } = false;
//        public bool GiftIsSent { get; set; } = false;
//        public DateTime? GiftSentAt { get; set; }

//        public string? GiftSourceType { get; set; }
//    }
//}

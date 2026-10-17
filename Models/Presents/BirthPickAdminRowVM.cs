// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class BirthPickAdminRowVM
    {
        // Pick(선택) 정보만 유지
        public long PickId { get; set; }
        public string UserId { get; set; } = "";

        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductDescription { get; set; } = "";
        public string SourceType { get; set; } = "";        
        public bool IsActive { get; set; }
        public DateTime PickedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsSent { get; set; }          // 발송완료 여부
        public DateTime? SentAt { get; set; }     // 발송 처리 시각 (완료/취소 누른 시각)

        public bool IsPurchased { get; set; }     // 구매완료 여부
        public DateTime? PurchasedAt { get; set; }// 구매 처리 시각 (완료/취소 누른 시각)


        // ✅ 사용자 상세는 UserInfos에서 가져온 결과를 붙인다(메모리 조인)
        public UserInfo? User { get; set; }
    }
}

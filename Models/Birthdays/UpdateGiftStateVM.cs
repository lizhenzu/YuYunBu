namespace BulletinBoard.ViewModels.Birthday
{
    public class UpdateGiftStateVM
    {
        public int PersonId { get; set; }          // 화면 편의용(필수 아님)
        public string UserId { get; set; } = "";   // PresentItemPick.UserId 와 매칭

        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? ProductDescription { get; set; }

        public bool IsActive { get; set; }
        public bool IsSent { get; set; }

        public string? SourceType { get; set; } // 필요 시 사용(없으면 null 유지)


        // ✅ 선물 선택 요청 문자(SMS) 발송 여부
        public bool PickSmsSent { get; set; }

        // ✅ 메모 / 비고
        public string? Memo { get; set; }
    }
}

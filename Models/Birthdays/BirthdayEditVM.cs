namespace BulletinBoard.ViewModels.Birthday
{
    public class BirthdayEditVM
    {
        // Key
        public int PersonId { get; set; }

        // ✅ 조회(view)에서 가져오는 표시용
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string RoleLabel { get; set; } = "";
        public string ClassLabel { get; set; } = "";

        public string? PhoneNumber { get; set; }
        public string? PhoneNumber2 { get; set; }
        public string? HomeAaddress { get; set; }
        public string? School { get; set; }
        public string? Sex { get; set; }

        public string ProfileImageUrl { get; set; } = "";

        // ✅ 저장(업데이트) 허용: 생일 관련만
        public int? BYear { get; set; }
        public int? BMonth { get; set; }
        public int? BDay { get; set; }
        public string? PlusMins { get; set; }
        public string PublishYN { get; set; } = "Y";
    }
}

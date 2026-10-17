namespace BulletinBoard.ViewModels.AddressBook
{
    public class AddressBookPersonCardVM
    {
        public int? PersonId { get; set; }
        public string? UserId { get; set; }

        public string FullName { get; set; } = "";
        public string? UserName { get; set; }
        public string? Email { get; set; }

        // ✅ 추가: 전화/생일
        public string? PhoneNumber { get; set; }
        public int? BMonth { get; set; }
        public int? BDay { get; set; }

        // 학생이면 값 존재
        public int? ClassYear { get; set; }
        public int? ClassBan { get; set; }

        public string PhotoUrl { get; set; } = "/images/icon/people/people_01.webp";
    }
}

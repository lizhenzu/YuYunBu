using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels
{
    public class PersonEditVM
    {
        [Required]
        public int PersonId { get; set; }

        public int? BYear { get; set; }
        public int? BMonth { get; set; }
        public int? BDay { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(1)]
        public string? Sex { get; set; } // "M" or "F"

        public string? PhoneNumber { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber2 { get; set; }

        [MaxLength(200)]
        public string? HomeAaddress { get; set; }  // ✅ DB 컬럼명과 동일

        [MaxLength(200)]
        public string? School { get; set; }
    }
}

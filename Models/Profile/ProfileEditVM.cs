using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels.Profile
{
    public class ProfileEditVM
    {
        public string Id { get; set; }

        [Display(Name = "성(예: '홍')")]
        [Required]
        public string LastName { get; set; }

        [Display(Name = "이름(예: '길동')")]
        [Required]
        public string FirstName { get; set; }


        public string UserName { get; set; }
        public string Email { get; set; }


        [Display(Name = "핸드폰1")]
        public string PhoneNumber { get; set; }

        [Display(Name = "핸드폰2")]
        public string PhoneNumber2 { get; set; }

        [Display(Name = "생년월일의 월(Month)")]
        [Range(0, 12)]
        public int bMonth { get; set; }  // 0이면 미입력(=NULL)

        [Display(Name = "생년월일의 일(Day)")]
        [Range(0, 31)]
        public int bDay { get; set; }    // 0이면 미입력(=NULL)

        // y=공개, h=비공개
        [Display(Name = "생일공개여부")]
        [RegularExpression("^(|y|h)$")]
        public string birthDay_PublishYN { get; set; }

        // UI
        public string ProfileImageUrl { get; set; }
        public bool PersonUpdateAllowed { get; set; }
        public string NoticeMessage { get; set; }
    }
}

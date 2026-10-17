using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "영문이름[예:PARKSAERI]")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "한글실명(예:박세리)")]
        public string UserNameKOR { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "이메일")]
        public string Email { get; set; }

        [Required] 
        [Display(Name = "교사 or 학생")]
        public string roleST { get; set; }        

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "비밀번호")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "재확인 비밀번호")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "영문이름[예:PARKSAERI]")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}

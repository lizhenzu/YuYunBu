using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulletinBoard.Models
{
    public class UserInfo : UserInfoBase
    {
        [Display(Name = "No.")] 
        public int PersonId { get; set; } 
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class EveryBibles : EveryBible
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

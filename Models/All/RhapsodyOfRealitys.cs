using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class RhapsodyOfRealitys : RhapsodyOfReality
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

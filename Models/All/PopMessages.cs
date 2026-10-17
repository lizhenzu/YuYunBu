using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PopMessages : PopMessage
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

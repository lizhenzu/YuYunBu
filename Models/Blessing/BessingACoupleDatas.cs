using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BessingACoupleDatas : BessingACoupleData
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

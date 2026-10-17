using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BessingChildDatas : BessingChildData
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

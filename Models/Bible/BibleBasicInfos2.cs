using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleBasicInfos2 : BibleBasicInfo2
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

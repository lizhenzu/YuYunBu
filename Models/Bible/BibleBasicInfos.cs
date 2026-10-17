using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleBasicInfos : BibleBasicInfo
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleQuizEnrollInfos : BibleQuizEnrollInfo
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

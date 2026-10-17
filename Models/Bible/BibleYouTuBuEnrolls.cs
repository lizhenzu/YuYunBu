using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleYouTuBuEnrolls : BibleYouTuBuEnroll
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
        public decimal ReadPercentage { get; set; }
        public int ReadCount { get; set; }
        public int TotalVerseCount { get; set; } 
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class WeeklyWordToReads : WeeklyWordToRead
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }

        public DateTime ClickDate_Min { get; set; }
        public DateTime ClickDate_Max { get; set; }
        public int CountCount { get; set; }
    }
}

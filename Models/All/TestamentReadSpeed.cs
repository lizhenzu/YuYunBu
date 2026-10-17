using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class TestamentReadSpeed
    {
        public int ReadDays { get; set; }
        public int TotalVerses { get; set; }
        public decimal AvgPerDay { get; set; }
        public int TotalBibleVerses { get; set; }
        public int RemainingVerses { get; set; }
        public decimal EstimatedDaysRemaining { get; set; }
        public DateTime ExpectedCompletionDate { get; set; }
    }
}
 
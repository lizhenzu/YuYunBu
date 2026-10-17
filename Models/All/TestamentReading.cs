using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class TestamentReading
    {
        public int BibleBookId { get; set; }
        public int BibleBookSerialId { get; set; }        
        public string BibleText { get; set; }
        public decimal ProgressRate { get; set; }
        public int LastReadJang { get; set; }
        public int LastReadZul { get; set; }
        public DateTime LastReadDate { get; set; }
    }
} 
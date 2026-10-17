using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BessingChildData
    {
        public int Id { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public string PrayTitle { get; set; }
        public string PrayText { get; set; }
        public string PrayText_Eng { get; set; }

        public string HolyWord { get; set; }
        
        public int visited { get; set; }
        public DateTime? updateDT { get; set; }
        public string FromDateWeekDayKor { get; set; }
        public string ToDateWeekDayKor { get; set; }

    }
}

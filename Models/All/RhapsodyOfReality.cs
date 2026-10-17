using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class RhapsodyOfReality
    {
        public int Id { get; set; }

        public string ExposeDate { get; set; }

        public int DayData { get; set; }
        public string WeekDayKor { get; set; }
        public string YouTuBuURL { get; set; }
        public int EndMinute { get; set; }
        public int EndSecond { get; set; }

        public int Visited { get; set; }
        [Display(Name = "설교제목")]
        public string WordTitle { get; set; }
        [Display(Name = "설교")]
        public string WordPhrase { get; set; }
        [Display(Name = "기도")]
        public string WordPray { get; set; }
        [Display(Name = "성경")]
        public string HolyWord { get; set; } 
    }
}

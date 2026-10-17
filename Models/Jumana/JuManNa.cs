using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class JuManNa : TodayWordToRead
    {

        public string WeekDayKor { get; set; }

        public string note1Keyword { get; set; }
        public string note1 { get; set; }
        [Display(Name = "오늘의만나(1)")]
        public string note1Question1 { get; set; }
        [Display(Name = "오늘의만나(2)")]
        public string note1Question2 { get; set; }
        public string note2Keyword { get; set; }
        public string note2 { get; set; }
        [Display(Name = "오늘의만나(1)")]
        public string note2Question1 { get; set; }
        [Display(Name = "오늘의만나(2)")]
        public string note2Question2 { get; set; }
        [Display(Name = "개인")]
        public string todayPrayForPerson { get; set; }
        [Display(Name = "가정")]
        public string todayPrayForHome { get; set; }
        public string todayDescribe1 { get; set; }
        public string todayDescribe1KeyWord { get; set; }
        public string todayDescribe2 { get; set; }
        public string todayDescribe2KeyWord { get; set; }
        public int visited { get; set; }



    }
}

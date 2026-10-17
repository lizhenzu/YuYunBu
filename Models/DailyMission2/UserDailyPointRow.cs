using System;

namespace BulletinBoard.Models.DailyMission2
{
    public class UserDailyPointRow
    {
        public DateTime DateBy { get; set; }

        public int WordPoint { get; set; }
        public int QuizPoint { get; set; }
        public int PrayPoint { get; set; }
        public int RecitePoint { get; set; }

        public int TotalPoint { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace BulletinBoard.Models.DailyMission2
{
    public class UserDailyPointsVM
    {
        public bool IsAdmin { get; set; }

        public string UserName { get; set; } = "";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<UserDailyPointRow> Rows { get; set; } = new();

        // 합계(기간 총합)
        public int SumWord => Rows.Sum(x => x.WordPoint);
        public int SumQuiz => Rows.Sum(x => x.QuizPoint);
        public int SumPray => Rows.Sum(x => x.PrayPoint);
        public int SumRecite => Rows.Sum(x => x.RecitePoint);
        public int SumTotal => Rows.Sum(x => x.TotalPoint);
    }
}

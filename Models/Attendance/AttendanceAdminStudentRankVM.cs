using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public class AttendanceAdminStudentRankVM
    {
        public DateTime BaseDate { get; set; }
        public DateTime ThisSunday { get; set; }
        public DateTime PrevSunday1 { get; set; }
        public DateTime PrevSunday2 { get; set; }

        public int TotalStudentsNow { get; set; }
        public int WeeksCount { get; set; } = 3;

        public List<Row> Rows { get; set; } = new();

        public sealed class Row
        {
            public int RankNo { get; set; }          // Dense Rank
            public int PersonId { get; set; }
            public string StudentName { get; set; } = "";
            public int ClassYear { get; set; }
            public int ClassBan { get; set; }
            public int PresentCount { get; set; }    // 0~3
            public int AbsentCount => Math.Max(0, 3 - PresentCount);

            public string ClassLabel
                => (ClassYear > 0 && ClassBan > 0) ? $"{ClassYear}학년 {ClassBan}반" : "-";
        }
    }
}

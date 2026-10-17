using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public sealed class AttendanceAdminStudentMatrixVM
    {
        public DateTime BaseDate { get; set; }
        public DateTime StartSunday { get; set; }
        public DateTime EndSunday { get; set; }

        public int Weeks { get; set; } = 2;

        public List<DateTime> Sundays { get; set; } = new();
        public List<Row> Rows { get; set; } = new();

        public sealed class Row
        {
            public int PersonId { get; set; }
            public string StudentName { get; set; } = "";
            public int ClassYear { get; set; }
            public int ClassBan { get; set; }

            // Key: yyyy-MM-dd, Value: true=출석
            public Dictionary<string, bool> PresentMap { get; set; } = new();

            public string ClassLabel
                => (ClassYear > 0 && ClassBan > 0) ? $"{ClassYear}학년 {ClassBan}반" : "-";
        }
    }
}

using BulletinBoard.Helpers;
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public sealed class AttendanceDashboardVM
    {
        public DateTime Date { get; set; } = DateHelper.GetKoreaNow();

        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public string? ClassName { get; set; }

        public List<ClassItem> Classes { get; set; } = new();

        // 오늘 요약
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount => Math.Max(0, TotalStudents - PresentCount);
        public decimal PresentRate => TotalStudents <= 0 ? 0 : Math.Round((decimal)PresentCount * 100m / (decimal)TotalStudents, 1);

        // 오늘 결석자 목록(= 미출석자)
        public List<StudentRow> AbsentStudents { get; set; } = new();

        // 주간 통계(기본: 월~일 7일)
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<DailyStat> Weekly { get; set; } = new();

        public sealed class ClassItem
        {
            public int ClassYear { get; set; }
            public int ClassBan { get; set; }
            public string ClassName { get; set; } = "";
        }

        public sealed class StudentRow
        {
            public int PersonId { get; set; } = 0;
            public string StudentName { get; set; } = "";
        }

        public sealed class DailyStat
        {
            public DateTime Date { get; set; }
            public int Total { get; set; }
            public int Present { get; set; }
            public decimal Rate { get; set; } // %
        }
    }
}

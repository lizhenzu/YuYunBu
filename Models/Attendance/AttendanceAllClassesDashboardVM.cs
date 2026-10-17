using BulletinBoard.Helpers;
using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public sealed class AttendanceAllClassesDashboardVM
    {
        public DateTime Date { get; set; } = DateHelper.GetKoreaNow();

        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalPresent { get; set; }
        public decimal OverallRate => TotalStudents <= 0 ? 0 : Math.Round((decimal)TotalPresent * 100m / (decimal)TotalStudents, 1);

        // ✅ 추가: 교사용 "내 반"
        public int? MyClassYear { get; set; }
        public int? MyClassBan { get; set; }

        // ✅ 추가(Q40-2): 미체크 반 요약(정책: 출석 기록 0건이면 미체크)
        public int NotCheckedCount { get; set; }
        public List<NotCheckedClass> NotCheckedClasses { get; set; } = new();

        public List<ClassCard> Classes { get; set; } = new();

        public sealed class NotCheckedClass
        {
            public int ClassYear { get; set; }
            public int ClassBan { get; set; }
            public string ClassName { get; set; } = "";
            public string TeacherName { get; set; } = "미지정";
        }

        public sealed class ClassCard
        {
            public int ClassYear { get; set; }
            public int ClassBan { get; set; }
            public string ClassName { get; set; } = "";

            public int TotalStudents { get; set; }
            public int PresentCount { get; set; }
            public int AbsentCount => Math.Max(0, TotalStudents - PresentCount);
            public decimal Rate => TotalStudents <= 0 ? 0 : Math.Round((decimal)PresentCount * 100m / (decimal)TotalStudents, 1);

            // ✅ 추가: 미체크/체크 상태(정책: 출석 기록 0건이면 미체크)
            public bool IsChecked { get; set; }

            // ✅ 추가: 담당 교사 표시
            public int? TeacherPersonId { get; set; }
            public string TeacherName { get; set; } = "미지정";
        }
    }

}

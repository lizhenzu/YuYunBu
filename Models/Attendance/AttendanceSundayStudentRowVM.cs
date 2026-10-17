using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public class AttendanceSundayStudentRowVM
    {
        public int PersonId { get; set; }
        public string StudentName { get; set; } = "";
        public bool IsPresent { get; set; }
        public int VisibleOrder { get; set; }

        // 기존 학생 출석률 계산용
        public int YearPresentCount { get; set; }

        // 새친구 여부
        public bool IsNewComer { get; set; }

        // 새친구 출석 횟수
        public int NewComerVisitCount { get; set; }
    }
}
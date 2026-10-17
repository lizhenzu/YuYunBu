using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public class AttendanceSundayClassRowVM
    {
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public string ClassName { get; set; } = "";
        public string TeacherName { get; set; } = "";

        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public double Rate { get; set; }
    }
}

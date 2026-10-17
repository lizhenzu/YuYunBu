using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public class AttendanceSundayClassCardVM
    {
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public string ClassName { get; set; } = "";
        public string TeacherName { get; set; } = "";

        public List<AttendanceSundayStudentRowVM> Students { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public class AttendanceSundayClassDetailVM
    {
        public DateTime SundayDate { get; set; }
        public List<AttendanceSundayClassRowVM> Classes { get; set; } = new();
    }
}

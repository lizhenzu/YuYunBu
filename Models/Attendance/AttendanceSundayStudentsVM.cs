using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public class AttendanceSundayStudentsVM
    {
        public DateTime SundayDate { get; set; }

        // ✅ (추가) 올해 누적 주일 수(분모) - 출석률 계산용
        public int WeeksCount { get; set; }

        public List<AttendanceSundayClassCardVM> Classes { get; set; } = new();
    }
}

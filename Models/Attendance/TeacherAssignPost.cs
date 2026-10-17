using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public   class TeacherAssignPost
    {
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public int TeacherPersonId { get; set; } // 0이면 미지정
    }
}

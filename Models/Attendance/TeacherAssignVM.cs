using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public sealed class TeacherAssignVM
    {
        public List<ClassRow> Classes { get; set; } = new();
        public List<TeacherOption> Teachers { get; set; } = new();

        public sealed class ClassRow
        {
            public int ClassYear { get; set; }
            public int ClassBan { get; set; }
            public string ClassName { get; set; } = "";

            public int TotalStudents { get; set; }

            public int? AssignedTeacherPersonId { get; set; }
            public string AssignedTeacherName { get; set; } = "미지정";
        }

        public sealed class TeacherOption
        {
            public int PersonId { get; set; }
            public string FullName { get; set; } = "";
        }
    }
}

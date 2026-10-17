// 경로: /ViewModels/Notice/NoticeEditVm.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels
{
    public class ClassCardVM
    {
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public string ClassDisplayName { get; set; } = "";
        public int StudentCount { get; set; }

        public int? TeacherPersonId { get; set; }
        public string? TeacherName { get; set; }
        public string? TeacherPhone { get; set; }
    }

}

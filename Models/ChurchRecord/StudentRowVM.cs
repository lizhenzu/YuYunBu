// 경로: /ViewModels/Notice/NoticeEditVm.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels
{
    public class StudentRowVM
    {
        public int PersonId { get; set; }
        public string? FullName { get; set; }
        public string? Sex { get; set; }
        public int? BMonth { get; set; }
        public int? BDay { get; set; }
        public string? BirthDayPublishYN { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PhoneNumber2 { get; set; }
        public string? School { get; set; }
        public string? HomeAddress { get; set; }
    }

}

using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public sealed class SavePresentRequest
    {
        public string attendDate { get; set; } = ""; // yyyy-MM-dd
        public int personId { get; set; }
        public bool isPresent { get; set; }
    }
}

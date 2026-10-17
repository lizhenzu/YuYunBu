// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class UpcomingBirthdayListVM
    {
        public int WithinDays { get; set; }
        public int TotalCount { get; set; }
        public List<UpcomingBirthdayRowVM> Rows { get; set; } = new();
    }
}

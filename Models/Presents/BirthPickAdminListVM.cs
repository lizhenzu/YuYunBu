// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class BirthPickAdminListVM
    {
        public string? Q { get; set; }
        public bool IncludeInactive { get; set; }
        public int TotalCount { get; set; }
        public List<BirthPickAdminRowVM> Rows { get; set; } = new();
    }
}

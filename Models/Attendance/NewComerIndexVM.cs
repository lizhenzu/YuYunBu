using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public class NewComerIndexVM
    {
        public string Keyword { get; set; } = string.Empty;
        public List<NewComerRowVM> ActiveList { get; set; } = new();
        public List<NewComerRowVM> CompletedList { get; set; } = new();
    }
}

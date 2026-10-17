using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public class NewComerDetailVM
    {
        public NewComerRowVM Info { get; set; } = new();
        public List<NewComerVisitRowVM> Visits { get; set; } = new();
    }
}

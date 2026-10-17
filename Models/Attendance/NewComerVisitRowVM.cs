using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public class NewComerVisitRowVM
    {
        public int Id { get; set; }
        public int NewComerId { get; set; }
        public int VisitOrder { get; set; }
        public DateTime VisitDate { get; set; }
        public DateTime InsertDT { get; set; }
    }
}

using BulletinBoard.Helpers;
using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public   class AttendanceAdminTrendVM
    {
        public DateTime BaseDate { get; set; } = DateHelper.GetKoreaNow(); // 기준 날짜(요청 date)
        public DateTime ThisSunday { get; set; }                  // 이번주(기준) 주일

        public int TotalStudentsNow { get; set; }                 // 현재 반편성 기준 총원(참고)

        public List<WeekRow> Weeks { get; set; } = new();         // 3주치(3주전/2주전/이번주)

        public sealed class WeekRow
        {
            public string Label { get; set; } = "";               // "3주전", "2주전", "이번주"
            public DateTime SundayDate { get; set; }              // 해당 주일 날짜

            public int Total { get; set; }                        // 총원(현재 ClassInfo 기준)
            public int Present { get; set; }                      // 출석
            public int Absent => Math.Max(0, Total - Present);     // 결석(미출석)
            public decimal Rate => Total <= 0 ? 0 : Math.Round((decimal)Present * 100m / (decimal)Total, 1);
        }
    }
}

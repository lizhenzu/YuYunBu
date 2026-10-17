using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    /// <summary>
    /// 출석부 "오늘" 화면 (/Attendance/Index)
    /// - 모든 반 · 모든 아이를 한 화면에, 이번 주일 출석 + 최근 4주 기록
    /// </summary>
    public sealed class AttendanceTodayVM
    {
        public DateTime Sunday { get; set; }          // 보고 있는 주일
        public DateTime ThisSunday { get; set; }      // 오늘이 속한 주일 (이후로는 못 감)
        public List<DateTime> RecentSundays { get; set; } = new(); // 지난 4주 (오래된 → 최근)

        public List<ClassGroup> Classes { get; set; } = new();

        public int? MyClassYear { get; set; }
        public int? MyClassBan { get; set; }
        public bool IsStaff { get; set; }

        public int TotalCount { get; set; }
        public int PresentCount { get; set; }
        /// <summary>최근 3주 연속 결석한 아이 수 (돌봄 탭 숫자)</summary>
        public int CareCount { get; set; }

        public sealed class ClassGroup
        {
            public int ClassYear { get; set; }
            public int ClassBan { get; set; }
            public string ClassName { get; set; } = "";
            public string TeacherName { get; set; } = "";
            public List<Kid> Kids { get; set; } = new();
        }

        public sealed class Kid
        {
            public int PersonId { get; set; }
            public string Name { get; set; } = "";
            public bool IsPresent { get; set; }
            /// <summary>지난 4주 출석 (오래된 → 최근)</summary>
            public List<bool> Recent { get; set; } = new();
            /// <summary>지난 주부터 거꾸로 센 연속 결석 주 수</summary>
            public int AbsentStreak { get; set; }
        }
    }
}

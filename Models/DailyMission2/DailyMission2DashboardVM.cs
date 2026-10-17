using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.DailyMission2
{
    public class DailyMission2DashboardVM
    {
        public string UserName { get; set; } = "";

        public DateTime Today { get; set; }
        public DateTime SelectedDate { get; set; }

        public bool IsToday { get; set; }
        public bool IsActionable { get; set; } // D, D-1, D-2
        public bool IsOtherDate { get; set; }
        public string NoticeMessage { get; set; } = "";

        public DailyMission2ContentIds Content { get; set; } = new DailyMission2ContentIds();
        public DailyMission2DayStatus DayStatus { get; set; } = new DailyMission2DayStatus();
        public DailyMission2YearPoint YearPoint { get; set; } = new DailyMission2YearPoint();

        // ✅ 오늘 화면 상단 “오늘 미션 완료 🎉”용
        public bool AllDone => DayStatus.WordDone && DayStatus.PrayDone && DayStatus.QuizDone && DayStatus.ReciteDone;

        // ✅ 달력에서 날짜별 "모든 미션 완료(✓)" 표시용 (key: "yyyy-MM-dd")
        public Dictionary<string, bool> CalendarAllDoneMap { get; set; } = new Dictionary<string, bool>();

        public Dictionary<string, int> CalendarDayPointMap { get; set; } = new Dictionary<string, int>();

    }
}

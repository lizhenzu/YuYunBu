using System;

namespace BulletinBoard.Models.DailyMission2
{
    public class TodayWordDetailResult
    {
        public int Id { get; set; }
        public DateTime ExposeDate { get; set; }

        public string? BibleSubject { get; set; }
        public string BibleText { get; set; } = "";
        public int FromJang { get; set; }
        public int FromZul { get; set; }
        public int ToJang { get; set; }
        public int ToZul { get; set; }

        public string? BibleSummary { get; set; }
        public string? synopsis { get; set; }

        // ✅ ManageeBible.geteBible2(...) 결과(본문)
        public eBibles? TodayeBiblesData { get; set; }
    }
}

using System;

namespace BulletinBoard.Models.DailyMission2
{

    public class DailyMission2ContentIds
    {
        public int? TodayWordToReadId { get; set; }
        public int? PrayPhraseId { get; set; }
        public int? WeeklyWordToReadId { get; set; }

        // ✅ 퀴즈는 콘텐츠를 실제로 보여주기 위해 별도 모델로 분리
        public DailyMission2QuizContent Quiz { get; set; } = new DailyMission2QuizContent();
    }
}

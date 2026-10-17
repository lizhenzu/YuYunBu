namespace BulletinBoard.ViewModels.Dashboard
{
    public sealed class StudentPointDashboardVM
    {
        // 1. 도전 성경 말씀 포인트 00점
        public int BiblePoint { get; set; }

        // 2. 도전 성경완독 00%
        public int BibleReadPercent { get; set; }
        // 2-1. 성경통독률 소수 1자리 (장 기준 — 홈 · 성경통독 · 오늘의 말씀과 같은 값, 2026-10-04)
        public double BibleReadRate { get; set; }

        // 3. 나의 출석카운트 00회 (올해 누적 기준)
        public int AttendanceCount { get; set; }

        // 4. 출석 포인트(출석카운트 * 2)
        public int AttendancePoint { get; set; }

        // 4-1. 올해 총 출석 대상 주 수
        public int AttendanceWeeksTotal { get; set; }

        // 5. 나의 미션북 00점
        public int MissionBookPoint { get; set; }

        // 6. 내가 전도한 아이 0명
        public int EvangelismCount { get; set; }

        // 7. 감사가 자라는 말씀과 일기 0회
        public int ThanksDiaryCount { get; set; }


        /* ✅ 추가 */
        public int ThanksPointTotal { get; set; }      // DevotionPoints 누적 총점
        public int IntegratedTotal { get; set; }       // Bible + AttendancePoint + MissionBook + Thanks

        public bool ShowGiftPopup { get; set; }        // 서버 판정(정답)
        public int PopupMilestone { get; set; }        // 예: 300 (100단위)
        public int lastSeen { get; set; }        // 예: 300 (100단위)
        public int EarnedCredit { get; set; }          // milestone까지 누적
        public int SpentCredit { get; set; }           // PURCHASED 합
        public int SpentMilestone { get; set; }           // PURCHASED 합        
        public int AvailableCredit { get; set; }       // Earned - Spent

    }
}

using System;

namespace BulletinBoard.Helpers
{
    /// <summary>
    /// "오늘의 특별 기도" — 정해진 날 하루만 홈 카드에 띄우고, 홈에 들어오면 그 기도문으로 보낸다 (2026-11-17)
    ///  · 다른 날 · 다른 기도문으로 바꾸려면 아래 두 값만 고친다
    ///  · 홈 → 기도문 이동은 같은 브라우저에서 하루 한 번만 (쿠키) — 본 뒤에는 홈을 그대로 쓸 수 있다
    /// </summary>
    public static class TodayFeaturedPray
    {
        /// <summary>적용하는 날 (한국 시간)</summary>
        public static readonly DateTime Date = new DateTime(2026, 11, 19);   // 수능 당일

        /// <summary>수험생 기도문 번호 (/EntranceExamPray/Detail?PrayId=…)</summary>
        public const int PrayId = 79;

        public const string CookieName = "featuredPraySeen";

        public static string Url => $"/EntranceExamPray/Detail?PrayId={PrayId}";

        public static bool IsActive() => DateHelper.GetKoreaToday().Date == Date.Date;

        /// <summary>수능이 끝난 뒤(다음 날부터) 계속 고정해서 보여 줄 기도문 번호 (2026-11-20) — 바꾸려면 이 값만 고친다</summary>
        public const int AfterPrayId = 26;

        /// <summary>수능이 끝났는지 (한국 시간으로 수능 다음 날부터)</summary>
        public static bool IsAfter() => DateHelper.GetKoreaToday().Date > Date.Date;

        /// <summary>지금 고정해서 보여 줄 기도문 번호 — 수능 당일 PrayId, 수능 이후 AfterPrayId, 그 전에는 0(순환)</summary>
        public static int FixedPrayId() => IsActive() ? PrayId : (IsAfter() ? AfterPrayId : 0);

        public static string FixedUrl => $"/EntranceExamPray/Detail?PrayId={FixedPrayId()}";
    }
}

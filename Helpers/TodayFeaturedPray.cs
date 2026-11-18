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
        public static readonly DateTime Date = new DateTime(2026, 11, 18);

        /// <summary>수험생 기도문 번호 (/EntranceExamPray/Detail?PrayId=…)</summary>
        public const int PrayId = 79;

        public const string CookieName = "featuredPraySeen";

        public static string Url => $"/EntranceExamPray/Detail?PrayId={PrayId}";

        public static bool IsActive() => DateHelper.GetKoreaToday().Date == Date.Date;
    }
}

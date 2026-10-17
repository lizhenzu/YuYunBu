using System;

namespace BulletinBoard.Helpers
{
    public static class DateHelper
    {



        public static string GetTodayDateDayString(int modValue = 10)
        {
            int day = GetKoreaNow().Day;

            // day를 10으로 나눈 나머지를 구합니다.
            int modResult = day % modValue;

            // modResult를 두 자리 숫자로 포맷합니다.
            string formattedModResult = modResult.ToString("D2");

            return formattedModResult;
        }


        private static TimeZoneInfo GetKoreaTimeZone()
        {
            // Windows: "Korea Standard Time"
            // Linux: "Asia/Seoul"
            try { return TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"); }
        }

        public static string GetKoreaNowString()
        {
            var kst = GetKoreaTimeZone();
            var koreaNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, kst);
            return koreaNow.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static DateTime GetKoreaNow()
        {
            var kst = GetKoreaTimeZone();
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, kst);
        }

        public static DateTime GetKoreaToday()
        {
            return GetKoreaNow().Date;
        }

        public static DateTime GetBaseSunday(DateTime input)
        {
            // ✅ date 파라미터가 들어온 경우(yyyy-MM-dd)는 보통 Kind=Unspecified
            //    -> 사용자가 선택한 "한국 날짜"로 간주하고 그대로 사용
            DateTime d;
            if (input.Kind == DateTimeKind.Unspecified)
            {
                d = input.Date;
            }
            else
            {
                // ✅ 서버 Local/UTC가 무엇이든 "한국 시간"으로 변환 후 Date 처리
                var kst = GetKoreaTimeZone();
                var kstTime = TimeZoneInfo.ConvertTime(input, kst);
                d = kstTime.Date;
            }

            // DayOfWeek: Sunday=0
            int diff = (int)d.DayOfWeek;
            return d.AddDays(-diff);
        }
    }
}

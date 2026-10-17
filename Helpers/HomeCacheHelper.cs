using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BulletinBoard.Helpers
{
    /// <summary>
    /// 홈(/) 화면 메모리 캐시 키와 비우기.
    /// HomeController 의 CK_Milestones / CK_MsConfigs 와 같은 문자열이어야 한다.
    /// 가족팀·구성원·가족 선물이 바뀌면 여기서 바로 비워, 홈에 수정 내용이 즉시 보이게 한다.
    /// </summary>
    public static class HomeCacheHelper
    {
        public const string FamilyMilestonesKey = "home:milestones:v1:";   // + 연도
        public const string FamilyMsConfigsKey  = "home:msconfigs:v1:";    // + 연도
        public const string BirthdaysKey        = "home:birthdays:v2:";    // + 날짜(yyyyMMdd)

        /// <summary>가족 마일스톤 캐시 비우기 (올해·작년·내년 모두 — 연도 경계 대비)</summary>
        public static void InvalidateFamilyMilestones(HttpContext? http)
        {
            var cache = http?.RequestServices?.GetService<IMemoryCache>();
            if (cache == null) return;

            int y = DateHelper.GetKoreaNow().Year;
            for (int year = y - 1; year <= y + 1; year++)
            {
                cache.Remove(FamilyMilestonesKey + year);
                cache.Remove(FamilyMsConfigsKey  + year);
            }
        }

        /// <summary>
        ///  생일 목록 캐시 비우기.
        ///  생일(월/일)을 고치면 홈의 '오늘의 생일자'·'다가오는 생일'이 바로 달라져야 하는데,
        ///  날짜 키 + 10분 캐시라 그냥 두면 한동안 옛 목록이 계속 보인다.
        ///  날짜 경계를 대비해 어제·오늘·내일 키를 함께 지운다.
        /// </summary>
        public static void InvalidateBirthdays(HttpContext? http)
        {
            var cache = http?.RequestServices?.GetService<IMemoryCache>();
            if (cache == null) return;

            var today = DateHelper.GetKoreaNow().Date;
            for (int off = -1; off <= 1; off++)
                cache.Remove(BirthdaysKey + today.AddDays(off).ToString("yyyyMMdd"));
        }
    }
}

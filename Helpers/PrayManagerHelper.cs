using System;
using System.Linq;
using System.Security.Claims;

namespace BulletinBoard.Helpers
{
    /// <summary>
    /// 기도 화면(365일 축복 기도 · 치유 기도 · 입시 기도 · 부부 기도)의 관리자 판정 — 2026-10-11
    ///  · 관리자 = 로그인 아이디가 "Basic" 또는 "SuperAdmin" 인 두 계정뿐 (대소문자 무시)
    ///  · 역할(Role)은 보지 않는다. Admin 계정 · Admin 역할 · 역할만 SuperAdmin 인 계정은 일반 사용자와 같다.
    ///    (Roles.Basic 은 일반 회원 기본 역할이라 역할로 판정하면 모든 회원이 관리자가 된다)
    ///  · 관리자에게만: 읽음 수 안 올림 · 기도 목록(읽음 수 포함) 보기 · 기도문 수정
    ///  · 등록 · 삭제 · 리셋은 화면마다 원래 권한을 그대로 쓴다
    /// </summary>
    public static class PrayManagerHelper
    {
        private static readonly string[] ManagerUserNames = { "Basic", "SuperAdmin" };

        /// <summary>
        /// 부부 기도 — 새 기도문 쓰기 (2026-10-15 사용자 지정)
        ///  · SuperAdmin 역할을 가진 계정  또는  로그인 아이디 "Basic"
        ///  · ※ 목록 · 수정 · 검증(IsPrayManager = 아이디 Basic · SuperAdmin)과 기준이 다르다
        /// </summary>
        public static bool CanAddCouplePray(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true) return false;
            if (user.IsInRole("SuperAdmin")) return true;
            return string.Equals(user.Identity.Name, "Basic", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 365 기도 — 참여자 요약 줄 보기 · 참여자 화면 (2026-10-18 사용자 지정)
        ///  · SuperAdmin 역할을 가진 계정  또는  로그인 아이디 "Basic"  (부부 기도 쓰기와 같은 기준)
        /// </summary>
        public static bool CanSeePrayParticipants(ClaimsPrincipal user) => CanAddCouplePray(user);

        public static bool IsPrayManager(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true) return false;
            var name = user.Identity.Name;
            return !string.IsNullOrEmpty(name)
                   && ManagerUserNames.Any(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}

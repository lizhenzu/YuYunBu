using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }

        public DateTime? InsertDT { get; set; }
        public string? PhoneNumber { get; set; }

        // ── 소속 (2026-11-08) — 역할은 Common/UserRole 기준 ──
        public BulletinBoard.Common.UserRoleKind Kind { get; set; } = BulletinBoard.Common.UserRoleKind.Parent;
        public string RoleNM { get; set; } = "";         // 임원 코드 이름 (CodeData T001)
        public int ClassYear { get; set; }               // 학생 반
        public int ClassBan { get; set; }
        public List<string> TeachClasses { get; set; } = new();   // 교사 담당 반 "1학년 2반 담임"
        public bool Pending { get; set; }                // 역할 코드 없고 가입 승인 대기 중
        public string AffLabel { get; set; } = "";       // 화면에 보이는 소속 한 줄
        public DateTime? LastAccessAt { get; set; }      // 최종 접속 (dbo.loginHistory — 사용자별 마지막 기록 1건)
    }
}

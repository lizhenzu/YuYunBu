using BibleReadingThrough.Data;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services.SiteVisit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BulletinBoard.Models;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    /// <summary>방문자 통계 — 홈으로 들어온 하루 · 누적 방문자 수 (SuperAdmin 롤 · "Basic" 계정만)</summary>
    [Authorize]
    public class SiteVisitController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _db;

        public SiteVisitController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostingEnvironment,
            AppDbContext db) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _db = db;
        }

        // 2026-10-05: SuperAdmin 롤 + "Basic" 계정만 (Admin 롤은 제외) — 정보 관리 메뉴의 "SB" 와 같은 기준
        private async Task<bool> IsStaffAsync()
        {
            if (!await _authService.IsSignedIn(User)) return false;
            var user = await _authService.GetSignedUser(User);
            if (await _authService.IsInRole(user, nameof(BulletinBoard.Enums.Roles.SuperAdmin))) return true;
            return string.Equals(User.Identity?.Name, "Basic", System.StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> Index()
        {
            if (!await IsStaffAsync()) return Forbid();
            var stats = await SiteVisitCounter.GetStatsAsync(_db);
            return View(stats);
        }
    }
}

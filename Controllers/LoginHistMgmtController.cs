using BulletinBoard.BusinessLayer;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [Authorize]   // 2026-10-04: 로그인 없이 기록을 보고 지울 수 있던 문제 차단
    [LogActionFilter]
    public class LoginHistMgmtController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
 
        public LoginHistMgmtController(IAuthService authService,
      UserManager<ApplicationUser> userManager,
      IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        // 2026-10-04: 정보 관리 메뉴와 같은 기준 — SuperAdmin · Admin 롤, 예외로 "Basic" 계정
        private async Task<bool> IsStaffAsync()
        {
            if (!await _authService.IsSignedIn(User)) return false;
            var user = await _authService.GetSignedUser(User);
            if (await _authService.IsInRole(user, nameof(BulletinBoard.Enums.Roles.SuperAdmin))) return true;
            if (await _authService.IsInRole(user, nameof(BulletinBoard.Enums.Roles.Admin))) return true;
            return string.Equals(User.Identity?.Name, "Basic", System.StringComparison.OrdinalIgnoreCase);
        }

        // 2026-10-04: 기록 삭제는 SuperAdmin 만 (보기는 위 기준 그대로)
        private async Task<bool> IsSuperAdminAsync()
        {
            if (!await _authService.IsSignedIn(User)) return false;
            var user = await _authService.GetSignedUser(User);
            return await _authService.IsInRole(user, nameof(BulletinBoard.Enums.Roles.SuperAdmin));
        }

        public async Task<IActionResult> Index()
        {
            if (!await IsStaffAsync()) return Forbid();

            ViewBag.CanDelete = await IsSuperAdminAsync();   // 화면의 "기록 삭제" 버튼은 SuperAdmin 에게만

            ManageLoginHistory manageLoginHistory = new ManageLoginHistory();

            return View(manageLoginHistory.getLoginHistory(_hostingEnvironment));
        }

        public async Task<IActionResult> DeleteLoginHistory(string UserName)
        {
            if (!await IsSuperAdminAsync()) return Forbid();

            ManageLoginHistory manageLoginHistory = new ManageLoginHistory();

            manageLoginHistory.DeleteLoginHistory(UserName);

            return RedirectToAction("Index");
        }

  

    }
}



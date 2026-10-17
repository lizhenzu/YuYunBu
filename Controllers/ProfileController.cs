using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAttendanceService _attendanceService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IProfileUpdateSqlService _profileUpdateSqlService;


        public ProfileController(IAuthService authService,
          UserManager<ApplicationUser> userManager,
         IProfileUpdateSqlService profileUpdateSqlService,
               IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _profileUpdateSqlService = profileUpdateSqlService;
            _hostingEnvironment = hostingEnvironment;
        }

         
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();

            }

            var vm = await _profileUpdateSqlService.GetEditAsync(this.UserId);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditVM vm)
        {
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            if (!ModelState.IsValid)
            {
                // 사진/안내 다시 세팅
                var fresh = await _profileUpdateSqlService.GetEditAsync(this.UserId);
                if (fresh != null)
                {
                    vm.ProfileImageUrl = fresh.ProfileImageUrl;
                    vm.PersonUpdateAllowed = fresh.PersonUpdateAllowed;
                    vm.NoticeMessage = fresh.NoticeMessage;
                }
                return View(vm);
            }

            var (ok, msg, _) = await _profileUpdateSqlService.SaveAsync(vm, this.UserId);
            TempData["Toast"] = msg;

          //  return RedirectToAction(nameof(Edit));
            return RedirectToAction("ManageUserInfo", "HumanResource", new { UserId = this.UserId });
        }
    }
}

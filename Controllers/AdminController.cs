using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace BulletinBoard.Controllers
{
    [Authorize] // ✅ 로그인만
    [LogActionFilter]
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IAttendanceService _attendanceService;

        public AdminController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET: JobCategory
        public async Task<IActionResult> Index()
        {
            // 2026-10-04 — 관리 메뉴를 "정보 관리" 한 화면으로 모았다. 옛 주소는 그 화면의 콘텐츠 묶음으로 보낸다.
            return Redirect("/HumanResource/ManageUserInfo#g4");
        }
        // GET: JobCategory
        public async Task<IActionResult> LifeGuide()
        {
            return View();
        }

        // GET: /Admin/SendBirthMessage — 부모님께 문자로 보내는 "생일 선물 선택 안내"
        //      부모님은 로그인하지 않은 상태로 여는 화면이라 이 화면만 로그인 없이 열어 둔다.
        //      예전 안내 주소(…/SendBirthMessage.html)로 보낸 문자도 그대로 열리도록 두 주소를 모두 받는다.
        [AllowAnonymous]
        [Route("Admin/SendBirthMessage")]
        [Route("Admin/SendBirthMessage.html")]
        public IActionResult SendBirthMessage()
        {
            return View();
        }

        // GET: /Admin/evangelism_reward_guide_v1  — 전도상 · 선물 선택 안내 (운영 흐름 · 선택 방법 · 부모님께 보낼 문구)
        public IActionResult evangelism_reward_guide_v1()
        {
            return View();
        }
        
        // GET: JobCategory
        public async Task<IActionResult> IndexPoint()
        {
            // 2026-10-04 — 포인트 · 시상 메뉴는 없앴다 (금주 시상자 · 랭킹 · 반별 · 시상금 시뮬레이션). 정보 관리 첫 화면으로 보낸다
            return Redirect("/HumanResource/ManageUserInfo");
        }
        [Authorize(Roles = "Admin,SuperAdmin,Moderator")]
        public async Task<IActionResult> TeacherOnly()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Moderator")]
        public IActionResult PrintManage()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Moderator")]
        public async Task<IActionResult> PrintByGrade(int grade)
        {
            // 가장 최근 주일(일요일) 계산
            var today = DateHelper.GetKoreaNow().Date;
            var dayOfWeek = (int)today.DayOfWeek; // 0=일, 1=월 … 6=토
            var lastSunday = today.AddDays(-dayOfWeek);

            var vm = await _attendanceService.GetSundayStudentsDetailAsync(lastSunday);

            // 해당 학년 반만 필터, 반 번호 오름차순, 학생 가나다순
            var gradeClasses = vm.Classes
                .Where(c => c.ClassYear == grade)
                .OrderBy(c => c.ClassBan)
                .Select(c =>
                {
                    c.Students = c.Students
                        .OrderBy(s => s.StudentName)
                        .ToList();
                    return c;
                })
                .ToList();

            ViewBag.Grade    = grade;
            ViewBag.Sunday   = lastSunday;
            ViewBag.GradeStr = $"{grade}학년";

            return View(gradeClasses);
        }

        [Authorize] // 로그인만 필요
        public IActionResult _DebugRoles()
        {
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .Distinct()
                .ToList();

            return Json(new
            {
                User = User.Identity?.Name,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Roles = roles,
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
    }
}



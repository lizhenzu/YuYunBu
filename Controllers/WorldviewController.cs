using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services.WorldView.Admin;
using BulletinBoard.ViewModels.Worldview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [Route("Worldview")]
    [Authorize]
    public class WorldviewController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IWorldviewLessonService _lessonService;
        private readonly IWorldviewAdminService _adminService;

        private const string SessionKeyUserId = "UserId";
        private const string SessionKeyUserName = "UserName"; // ✅ 사용자 표시 이름(선택)

        public WorldviewController(
            IAuthService authService,
            IWorldviewAdminService adminService,
            IWorldviewLessonService lessonService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostingEnvironment
        ) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _lessonService = lessonService;
            _adminService = adminService;
        }

        [HttpGet("Index")]
        public IActionResult Index() => View();

        [Authorize]
        [HttpGet("IndexSigned")]
        public IActionResult IndexSigned() => View();

        [HttpGet("Intro")]
        public IActionResult Intro()
        {
            ViewBag.ActiveLessonNo = _adminService.GetActiveLessonNo();
            return View();
        }

        [HttpGet("StartMission")]
        public IActionResult StartMission()
        {
            ViewBag.ActiveLessonNo = _adminService.GetActiveLessonNo();
            return View();
        }

        // ✅ GET /Worldview/DashBoard
        [HttpGet("DashBoard")]
        public async Task<IActionResult> DashBoard()
        {
            // 1) ActiveLessonNo
            var activeLessonNo = _adminService.GetActiveLessonNo();
            if (activeLessonNo <= 0) activeLessonNo = 1;
            ViewBag.ActiveLessonNo = activeLessonNo;

            // ✅ 2) LessonDashMap은 Dictionary<int, LessonDashStatusVM>으로 통일
            ViewBag.LessonDashMap = new Dictionary<int, LessonDashStatusVM>();

            // 3) 로그인한 경우에만 조회
            var isSignedIn = await _authService.IsSignedIn(User);
            if (!isSignedIn)
                return View();

            string? userId = HttpContext?.Session?.GetString(SessionKeyUserId);

            // ✅ UserName(표시 이름) 확보
            string? userName = HttpContext?.Session?.GetString(SessionKeyUserName);
            if (string.IsNullOrWhiteSpace(userName))
                userName = User?.Identity?.Name;

            // ✅ userId가 없으면 그대로 빈맵 유지
            if (string.IsNullOrWhiteSpace(userId))
                return View();

            // ✅ 서비스 반환 타입: Dictionary<int, LessonDashStatusVM>
            var map = await _lessonService.GetDashboardStatusMapAsync(userId, userName);

            // ✅ null 방어도 동일 타입
            ViewBag.LessonDashMap = map ?? new Dictionary<int, LessonDashStatusVM>();

            // SuperAdmin 여부 전달
            ViewBag.IsSuperAdmin = User.IsInRole("SuperAdmin");

            return View();
        }

        // POST /Worldview/submitlesson
        [Authorize]
        [HttpPost("submitlesson")]
        public async Task<IActionResult> SubmitLesson([FromBody] LessonSubmitRequest req)
        {
            if (req == null || req.LessonNo <= 0)
                return BadRequest(new { message = "요청이 올바르지 않아요." });

            req.Answers ??= new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            // ✅ 로그인 사용자 식별
            string? userId = HttpContext?.Session?.GetString(SessionKeyUserId);

            // ✅ UserName(표시 이름) 확보
            string? userName = HttpContext?.Session?.GetString(SessionKeyUserName);
            if (string.IsNullOrWhiteSpace(userName))
                userName = User?.Identity?.Name;

            var result = await _lessonService.SubmitLessonAsync(req.LessonNo, req.Answers, userId, userName);
            return Json(result);
        }

        [HttpGet("GoDashboardAuth")]
        public async Task<IActionResult> GoDashboardAuth()
        {
            var signed = await _authService.IsSignedIn(User);
            if (signed)
                return RedirectToAction(nameof(DashBoard));

            var returnUrl = Url.Action(nameof(DashBoard), "Worldview") ?? "/Worldview/DashBoard";
            var loginUrl = $"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";
            return Redirect(loginUrl);
        }

        // ===========================
        // 📘 Lesson
        // ===========================

        [HttpGet("Lesson/{id:int}")]
        public async Task<IActionResult> LessonByRoute([FromRoute] int id)
            => await LessonInternal(id);

        [HttpGet("Lesson")]
        public async Task<IActionResult> Lesson([FromQuery] int id = 1)
            => await LessonInternal(id);

        [HttpGet("LessonById/{id:int}")]
        public async Task<IActionResult> LessonById([FromRoute] int id)
            => await LessonInternal(id);

        private async Task<IActionResult> LessonInternal(int id)
        {
            if (id <= 0) id = 1;
            var vm = await _lessonService.GetLessonAsync(id);
            return View("LessonCard", vm);
        }

        // ===========================
        // ✏️ 빈칸 채점
        // ===========================

        [HttpPost("checkblanks")]
        public async Task<IActionResult> CheckBlanks([FromBody] BlankSubmitRequest req)
        {
            if (req == null || req.LessonNo <= 0)
                return BadRequest(new { message = "요청이 올바르지 않아요." });

            req.Answers ??= new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            // ✅ 로그인 사용자 식별
            string? userId = HttpContext?.Session?.GetString(SessionKeyUserId);

            // ✅ UserName(표시 이름) 확보 (세션 우선 → Identity fallback)
            string? userName = HttpContext?.Session?.GetString(SessionKeyUserName);
            if (string.IsNullOrWhiteSpace(userName))
                userName = User?.Identity?.Name;

            // ✅ 서비스 호출: 채점 + Draft(초안) Upsert까지 수행
            var result = await _lessonService.CheckBlanksAsync(
                req.LessonNo,
                req.Answers,
                userId,
                userName
            );

            return Json(result);
        }
    }
}

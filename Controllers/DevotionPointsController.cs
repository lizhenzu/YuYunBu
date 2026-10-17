using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    public class SetMonthlyReq
    {
        public int personId { get; set; }
        public string ym { get; set; } = "";
        public int points { get; set; }
    }

    // ==========================================================
    // 감사가 자라는 말씀과 일기노트 — 월별 포인트
    //   Home    : 입력 (전체 아이 한 화면 · 이름 누르면 입력창)
    //   Status  : 현황·순위 (월 합계 · 12개월 추이 · 반별 · 개인 순위)
    //   MyMonthly : 아이 본인 화면
    // 입력·현황은 교사(Admin/SuperAdmin/Moderator)만, 아이 화면은 로그인만
    // ==========================================================
    [Authorize]
    public class DevotionPointsController : BaseController
    {
        private const string StaffRoles = "Admin,SuperAdmin,Moderator";

        // ── 입력 화면 ──
        public class EntryPageVM
        {
            public string Ym { get; set; } = "";
            public DateTime Month { get; set; }
            public DateTime ThisMonth { get; set; }
            public List<EntryClassVM> Classes { get; set; } = new();
            public int KidCount => Classes.Sum(c => c.Kids.Count);
            public int TotalPoints => Classes.Sum(c => c.Kids.Sum(k => k.Points));
            public int GotCount => Classes.Sum(c => c.Kids.Count(k => k.Points > 0));
            public int MaxPoints => Classes.SelectMany(c => c.Kids).Select(k => k.Points).DefaultIfEmpty(0).Max();
        }
        public class EntryClassVM
        {
            public int ClassYear { get; set; }
            public int ClassBan { get; set; }
            public string ClassName { get; set; } = "";
            public List<DevotionRowVM> Kids { get; set; } = new();
        }

        // ── 현황·순위 화면 ──
        public class StatusPageVM
        {
            public string Ym { get; set; } = "";
            public DateTime Month { get; set; }
            public DateTime ThisMonth { get; set; }
            public int ClassYear { get; set; }            // 0 = 전체
            public List<int> ClassYears { get; set; } = new();
            public List<DevotionTopVM> Rows { get; set; } = new();   // 선택 학년의 모든 아이 (0점 포함)
            public List<DevotionTrendPointVM> Trend { get; set; } = new();
        }

        public class AdjustReq
        {
            public int personId { get; set; }
            public string ym { get; set; } = "";
            public int delta { get; set; } // ±1,±5,±10
        }

        public class MySummaryRes
        {
            public bool ok { get; set; } = true;
            public string message { get; set; } = "";
            public int totalPoints { get; set; }
        }

        public class MyMonthlyRes
        {
            public bool ok { get; set; } = true;
            public string message { get; set; } = "";
            public List<DevotionMyMonthlyPointVM> rows { get; set; } = new();
        }

        private readonly IAuthService _authService;
        private readonly IDevotionPointsService _DevotionPointsService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public DevotionPointsController(IAuthService authService,
            IDevotionPointsService _IDevotionPointsService,
            UserManager<ApplicationUser> userManager,
                 IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _DevotionPointsService = _IDevotionPointsService;
        }

        // "YYYY-MM" → 그 달 1일. 잘못되었거나 미래 달이면 이번 달.
        private static (DateTime month, DateTime thisMonth) ResolveMonth(string? ym)
        {
            var now = DateHelper.GetKoreaNow();
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            if (!string.IsNullOrWhiteSpace(ym)
                && DateTime.TryParseExact(ym.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var m)
                && m <= thisMonth)
                return (m, thisMonth);
            return (thisMonth, thisMonth);
        }

        private static bool IsValidYm(string ym) => ym.Length == 7 && ym[4] == '-';

        // ==========================================================
        // 입력 (첫 화면)
        // ==========================================================
        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public async Task<IActionResult> Home(string? ym)
        {
            var (month, thisMonth) = ResolveMonth(ym);
            var ymStr = month.ToString("yyyy-MM", CultureInfo.InvariantCulture);

            var vm = new EntryPageVM { Ym = ymStr, Month = month, ThisMonth = thisMonth };
            foreach (var c in await _DevotionPointsService.LoadAllClassesAsync())
            {
                vm.Classes.Add(new EntryClassVM
                {
                    ClassYear = c.ClassYear,
                    ClassBan = c.ClassBan,
                    ClassName = string.IsNullOrWhiteSpace(c.ClassName) ? $"{c.ClassYear}학년 {c.ClassBan}반" : c.ClassName,
                    Kids = await _DevotionPointsService.LoadGridRowsAsync(c.ClassYear, c.ClassBan, ymStr)
                });
            }

            return View("/Views/DevotionPoints/Home.cshtml", vm);
        }

        // (AJAX) 입력창 "1년 보기" — 아이 한 명의 1~12월
        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public async Task<IActionResult> StudentYear(int personId, int year)
        {
            if (personId <= 0 || year < 2000 || year > 2100)
                return Json(new { ok = false, message = "Invalid request" });

            var rows = await _DevotionPointsService.GetStudentYearPointsAsync(personId, year);
            return Json(new { ok = true, rows });
        }

        [HttpPost]
        [Authorize(Roles = StaffRoles)]
        public async Task<IActionResult> Adjust([FromBody] AdjustReq req)
        {
            if (req == null || req.personId <= 0 || string.IsNullOrWhiteSpace(req.ym))
                return Json(new { ok = false, message = "Invalid request" });

            var ym = req.ym.Trim();
            if (!IsValidYm(ym))
                return Json(new { ok = false, message = "Invalid ym format (YYYY-MM)" });

            // delta 제한(안전장치)
            int[] allowed = { -10, -5, -1, 1, 5, 10 };
            if (!allowed.Contains(req.delta))
                return Json(new { ok = false, message = "Invalid delta" });

            try
            {
                var newValue = await _DevotionPointsService.AdjustMonthlyTotalAsync(ym, req.personId, req.delta);
                return Json(new { ok = true, newValue });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = StaffRoles)]
        public async Task<IActionResult> SetMonthly([FromBody] SetMonthlyReq req)
        {
            if (req == null || req.personId <= 0 || string.IsNullOrWhiteSpace(req.ym))
                return Json(new { ok = false, message = "Invalid request" });

            var ym = req.ym.Trim();
            if (!IsValidYm(ym))
                return Json(new { ok = false, message = "Invalid ym format (YYYY-MM)" });

            if (req.points < 0) req.points = 0;

            try
            {
                var newValue = await _DevotionPointsService.SetMonthlyTotalAsync(ym, req.personId, req.points);
                return Json(new { ok = true, newValue });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public async Task<IActionResult> GetGrid(int classYear, int classBan, string ym)
        {
            try
            {
                var rows = await _DevotionPointsService.LoadGridRowsAsync(classYear, classBan, ym);
                return Json(new { ok = true, message = "", rows });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message, rows = new List<DevotionRowVM>() });
            }
        }

        // ==========================================================
        // 현황·순위
        // ==========================================================
        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public async Task<IActionResult> Status(string? ym, int classYear = 0)
        {
            var (month, thisMonth) = ResolveMonth(ym);
            var ymStr = month.ToString("yyyy-MM", CultureInfo.InvariantCulture);

            var all = await _DevotionPointsService.GetParticipantRankAllAsync(ymStr, 0, 0);
            var years = all.Select(x => x.ClassYear).Where(y => y > 0).Distinct().OrderBy(y => y).ToList();
            if (classYear != 0 && !years.Contains(classYear)) classYear = 0;

            var vm = new StatusPageVM
            {
                Ym = ymStr,
                Month = month,
                ThisMonth = thisMonth,
                ClassYear = classYear,
                ClassYears = years,
                Rows = classYear == 0 ? all : all.Where(x => x.ClassYear == classYear).ToList(),
                Trend = await _DevotionPointsService.GetDashboardTrendAsync(12, classYear, 0)
            };

            return View("/Views/DevotionPoints/Status.cshtml", vm);
        }

        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public async Task<IActionResult> DashboardSummary(string ym, int classYear = 0, int classBan = 0)
        {
            try
            {
                var data = await _DevotionPointsService.GetDashboardSummaryAsync(ym, classYear, classBan);
                return Json(new { ok = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public async Task<IActionResult> DashboardTrend(int months = 12, int classYear = 0, int classBan = 0)
        {
            try
            {
                var list = await _DevotionPointsService.GetDashboardTrendAsync(months, classYear, classBan);
                return Json(new { ok = true, rows = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public async Task<IActionResult> ParticipantRankAll(string ym, int classYear = 0, int classBan = 0)
        {
            ym = (ym ?? "").Trim();
            if (string.IsNullOrWhiteSpace(ym))
                return Json(new { ok = false, message = "ym is required. ex) 2026-02" });

            var rows = await _DevotionPointsService.GetParticipantRankAllAsync(ym, classYear, classBan);
            return Json(new { ok = true, data = rows });
        }

        // ==========================================================
        // 예전 주소 → 새 화면으로 (북마크·다른 페이지 링크 유지)
        // ==========================================================
        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public IActionResult ClassMonthlyTotal(int? classYear, int? classBan, string? ym)
        {
            var url = Url.Action(nameof(Home), new { ym }) ?? "/DevotionPoints/Home";
            if (classYear > 0 && classBan > 0) url += $"#g-{classYear}-{classBan}";
            return Redirect(url);
        }

        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public async Task<IActionResult> ClassMonthlyMyClass()
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            var my = await _DevotionPointsService.TryGetMyClassAsync(this.UserId);
            var url = "/DevotionPoints/Home";
            if (my != null) url += $"#g-{my.ClassYear}-{my.ClassBan}";
            return Redirect(url);
        }

        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public IActionResult ClassMonthlyMyClassEdit(int personId, string? fullName, int? year)
            => RedirectToAction(nameof(Home));

        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public IActionResult Dashboard(string? ym, int classYear = 0, int classBan = 0)
            => RedirectToAction(nameof(Status), new { ym, classYear });

        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public IActionResult Participants(string? ym, int classYear = 0, int classBan = 0)
            => RedirectToAction(nameof(Status), new { ym, classYear });

        // ==========================================================
        // 아이 본인 화면 (로그인만)
        // ==========================================================
        [HttpGet]
        public IActionResult MyMonthly()
        {
            return View("/Views/DevotionPoints/MyMonthly.cshtml");
        }

        // ✅ (AJAX) 누적 총합
        [HttpGet]
        public async Task<IActionResult> MyTotal()
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();

            var total = await _DevotionPointsService.GetMyTotalPointsAsync(this.UserId);
            return Json(new MySummaryRes { ok = true, totalPoints = total });
        }

        // ✅ (AJAX) 월별 이력 데이터
        [HttpGet]
        public async Task<IActionResult> MyMonthlyData(int months = 24)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();

            var rows = await _DevotionPointsService.GetMyMonthlyPointsAsync(this.UserId, months);
            return Json(new MyMonthlyRes { ok = true, rows = rows });
        }
    }
}

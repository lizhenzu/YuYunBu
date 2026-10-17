using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulletinBoard.Helpers;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class ApplyEventController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPresentPickService _presentPickService;
        private readonly IAttendanceService _attendanceService;
        private readonly BulletinBoard.Services.Abstract.IAwardItemService _awardItemService;
        private readonly IChurchRecordService _churchRecordService;

        public ApplyEventController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IPresentPickService presentPickService,
            IAttendanceService attendanceService,
            BulletinBoard.Services.Abstract.IAwardItemService awardItemService,
            IChurchRecordService churchRecordService,
            IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _presentPickService = presentPickService;
            _attendanceService = attendanceService;
            _awardItemService = awardItemService;
            _churchRecordService = churchRecordService;
        }
        public IActionResult Index()
        {
            ManageApplyEvent manageApplyEvent = new ManageApplyEvent();
            List<EventMater> value = manageApplyEvent.getEventInfo(0);


            if (value.Count > 0)
            {
                ViewData["EventMater"] = value[0];
            }
            else
            {
                ViewData["EventMater"] = string.Empty;
            }

            return View();
        }

        // ✅ async void → async Task : 호출하는 쪽이 끝날 때까지 기다려야 목록이 비지 않는다
        private async Task UpcomingBirthdaysAsync()
        {
            int withinDays = 31;  // ✅ D-31일까지만 표시

            // ✅ 31일 이내 다가오는 생일자 + 선물선택/발송상태
            var vm = await _presentPickService.GetUpcomingBirthdaysAsync(withinDays, "all", _hostingEnvironment);


            // ✅ birthDay_PublishYN == "h" 인 행 제거
            if (vm?.Rows != null)
            {
                vm.Rows = vm.Rows
                    .Where(r =>
                        r.User != null &&
                        !string.Equals(
                            r.User.birthDay_PublishYN,
                            "h",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    .ToList();
            }


            // ✅ ViewBag으로만 전달
            ViewBag.BirthDayList7 = vm?.Rows;         // List<UpcomingBirthdayRowVM>
            ViewBag.BirthDayWithinDays = vm?.WithinDays ?? withinDays;
            ViewBag.BirthDayTotalCount = vm?.TotalCount ?? 0;

        }

        public async Task<IActionResult> Branch()
        {
            if (!await _authService.IsSignedIn(User))
            {
                return View();
            }

            await UpcomingBirthdaysAsync();

            // ── 가장 최근 주일(일요일) 출석 데이터 조회 ──
            try
            {
                var today = DateHelper.GetKoreaNow().Date;
                int dow = (int)today.DayOfWeek; // 0=일, 1=월 ...
                var lastSunday = dow == 0 ? today : today.AddDays(-dow);

                var attendVM = await _attendanceService.GetSundayStudentsDetailAsync(lastSunday);

                // PersonId → YearPresentCount 딕셔너리
                var attMap = attendVM.Classes
                    .SelectMany(c => c.Students)
                    .GroupBy(s => s.PersonId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Max(s => s.YearPresentCount)   // 누적 출석 횟수
                    );

                ViewBag.AttendanceMap        = attMap;
                ViewBag.AttendanceWeeksCount = attendVM.WeeksCount;
                ViewBag.AttendanceSunday     = lastSunday;

                // ── 생일자별 마지막 출석일 ──
                var bdList = ViewBag.BirthDayList7 as List<BulletinBoard.Models.UpcomingBirthdayRowVM>;
                if (bdList != null && bdList.Count > 0)
                {
                    var personIds = bdList
                        .Where(r => r.User != null && r.User.PersonId > 0)
                        .Select(r => r.User.PersonId)
                        .Distinct();

                    ViewBag.LastAttendDateMap = await _attendanceService.GetLastAttendDateMapAsync(personIds);
                }
                else
                {
                    ViewBag.LastAttendDateMap = new Dictionary<int, DateTime>();
                }
            }
            catch
            {
                ViewBag.AttendanceMap        = new Dictionary<int, int>();
                ViewBag.AttendanceWeeksCount = 0;
                ViewBag.AttendanceSunday     = (DateTime?)null;
                ViewBag.LastAttendDateMap    = new Dictionary<int, DateTime>();
            }

            // ── 반별 담임선생님 (이름 · 연락처) — 생일 카드에 "담임" 줄로 보여 준다 ──
            //    키 = "학년-반".  조회에 실패해도 카드는 그대로 뜨고 담임 줄만 빠진다.
            try
            {
                var teacherMap = new Dictionary<string, (string Name, string Phone)>();
                foreach (var c in _churchRecordService.GetHome(null).Classes)
                {
                    if (string.IsNullOrWhiteSpace(c.TeacherName)) continue;
                    teacherMap[$"{c.ClassYear}-{c.ClassBan}"] = (c.TeacherName.Trim(), (c.TeacherPhone ?? "").Trim());
                }
                ViewBag.ClassTeacherMap = teacherMap;
            }
            catch
            {
                ViewBag.ClassTeacherMap = new Dictionary<string, (string Name, string Phone)>();
            }

            // 관리자 여부 + URL 있는 상품 목록
            try
            {
                var signedUser = await _authService.GetSignedUser(User);
                bool isAdmin = signedUser != null &&
                    (await _authService.IsInRole(signedUser, nameof(Roles.Admin)) ||
                     await _authService.IsInRole(signedUser, nameof(Roles.SuperAdmin)));
                ViewBag.IsAdmin = isAdmin;

                if (isAdmin)
                {
                    var allItems = await _awardItemService.GetAllAsync(BulletinBoard.Common.ProductDisplayOpton.none, true);
                    ViewBag.CoupangItems = allItems
                        .Where(x => !string.IsNullOrWhiteSpace(x.ExternalUrl))
                        .ToList();
                }
                else
                {
                    ViewBag.CoupangItems = null;
                }
            }
            catch
            {
                ViewBag.IsAdmin = false;
                ViewBag.CoupangItems = null;
            }

            await SetUserInfo();

            var manageHumanResource = new ManageHumanResource();
            var userInfos = manageHumanResource.getUserInfos(
                _hostingEnvironment,
                UserId,
                string.Empty, 0, 0, 0,
                string.Empty, string.Empty,
                RankQueryOption.None);

            var me = userInfos.FirstOrDefault();
            if (me != null)
            {
                ViewData["ProfilePicturePath"] = me.ProfilePicturePath;
                ViewData["UserId"] = me.UserId;
                ViewData["UserInfo"] = me;
            }

            return View();
        }
        public IActionResult ApplyList(int EventId)
        {
            ManageHumanResource manageHumanResource = new ManageHumanResource();

            var data = manageHumanResource.getApplyEventList(_hostingEnvironment, EventId);

            ManageApplyEvent manageApplyEvent = new ManageApplyEvent();
            List<EventMater> value = manageApplyEvent.getEventInfo(EventId);
            ViewData["EventMater"] = value[0];

            return View(data);
        }
        public IActionResult EventList()
        {
            ManageApplyEvent manageApplyEvent = new ManageApplyEvent();
            List<EventMater> value = manageApplyEvent.getEventInfo(0);

            return View(value);
        }

        [Authorize]
        public IActionResult CancelEnroll(string UserId, int EventId)
        {
            ManageApplyEvent manageApplyEvent = new ManageApplyEvent();
            manageApplyEvent.CancelEnrollEvent(UserId, EventId);

            return RedirectToAction(nameof(ApplyEventController.ApplyList), "ApplyEvent", new { EventId = EventId });
        }



        [Authorize]
        public async Task<IActionResult> EnrollEventApplyInfo(int EventId)
        {

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            ManageApplyEvent manageApplyEvent = new ManageApplyEvent();
            manageApplyEvent.EnrollEventApplyInfo(UserId, EventId);

            return View("ProcessResult");
        }
    }
}



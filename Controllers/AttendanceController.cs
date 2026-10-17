using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize]
    public class AttendanceController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IAttendanceService _attendanceService;

        private string? CurrentUserId => User?.Identity?.IsAuthenticated == true
            ? User?.FindFirst("sub")?.Value ?? User?.Identity?.Name
            : null;

        public AttendanceController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IAttendanceService svc,
            IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _attendanceService = svc;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// 출석부 "오늘" — 모든 반 · 모든 아이를 한 화면에 (순서 없이 이름을 눌러 체크)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date)
        {
            var thisSunday = DateHelper.GetBaseSunday(DateHelper.GetKoreaToday());
            var sunday = DateHelper.GetBaseSunday((date ?? thisSunday).Date);
            if (sunday > thisSunday) sunday = thisSunday; // 미래 주일 방지

            if (await _authService.IsSignedIn(User))
            {
                await SetUserInfo();
            }

            var vm = new AttendanceTodayVM
            {
                Sunday = sunday,
                ThisSunday = thisSunday,
                IsStaff = IsStaff()
            };
            for (int i = 4; i >= 1; i--) vm.RecentSundays.Add(sunday.AddDays(-7 * i));

            // 반 목록(이름 · 순서) + 담임
            var classPage = await _attendanceService.GetCheckPageAsync(sunday, 0, 0);
            var teacherPage = await _attendanceService.GetTeacherAssignPageAsync();
            foreach (var c in classPage.Classes)
            {
                var t = teacherPage.Classes.FirstOrDefault(x => x.ClassYear == c.ClassYear && x.ClassBan == c.ClassBan);
                vm.Classes.Add(new AttendanceTodayVM.ClassGroup
                {
                    ClassYear = c.ClassYear,
                    ClassBan = c.ClassBan,
                    ClassName = string.IsNullOrWhiteSpace(c.ClassName) ? $"{c.ClassYear}학년 {c.ClassBan}반" : c.ClassName,
                    TeacherName = t == null || t.AssignedTeacherName == "미지정" ? "" : t.AssignedTeacherName
                });
            }

            // 아이들 — 5주(지난 4주 + 이번 주일) 출석을 한 번에
            var matrix = await _attendanceService.GetAdminStudentMatrixAsync(sunday, 5);
            string Key(DateTime d) => d.ToString("yyyy-MM-dd");
            foreach (var row in matrix.Rows)
            {
                var g = vm.Classes.FirstOrDefault(x => x.ClassYear == row.ClassYear && x.ClassBan == row.ClassBan);
                if (g == null)
                {
                    g = new AttendanceTodayVM.ClassGroup { ClassYear = row.ClassYear, ClassBan = row.ClassBan, ClassName = $"{row.ClassYear}학년 {row.ClassBan}반" };
                    vm.Classes.Add(g);
                }
                var kid = new AttendanceTodayVM.Kid
                {
                    PersonId = row.PersonId,
                    Name = row.StudentName,
                    IsPresent = row.PresentMap.TryGetValue(Key(sunday), out var p) && p
                };
                foreach (var d in vm.RecentSundays)
                    kid.Recent.Add(row.PresentMap.TryGetValue(Key(d), out var q) && q);
                for (int i = kid.Recent.Count - 1; i >= 0 && !kid.Recent[i]; i--) kid.AbsentStreak++;
                g.Kids.Add(kid);
            }
            vm.Classes.RemoveAll(g => g.Kids.Count == 0);
            foreach (var g in vm.Classes) g.Kids.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture));

            vm.TotalCount = vm.Classes.Sum(g => g.Kids.Count);
            vm.PresentCount = vm.Classes.Sum(g => g.Kids.Count(k => k.IsPresent));
            vm.CareCount = vm.Classes.Sum(g => g.Kids.Count(k => k.AbsentStreak >= 3));

            if (!string.IsNullOrWhiteSpace(this.UserId))
            {
                var my = await _attendanceService.GetMyClassByUserIdAsync(this.UserId);
                if (my != null && my.ClassYear > 0 && my.ClassBan > 0)
                {
                    vm.MyClassYear = my.ClassYear;
                    vm.MyClassBan = my.ClassBan;
                }
            }

            return View(vm);
        }

        /// <summary>출석부 "더보기" — 가끔 쓰는 메뉴</summary>
        [HttpGet]
        public IActionResult More()
        {
            ViewBag.IsStaff = IsStaff();
            return View();
        }

        private bool IsStaff() =>
            User.IsInRole(nameof(BulletinBoard.Enums.Roles.Admin))
            || User.IsInRole(nameof(BulletinBoard.Enums.Roles.SuperAdmin))
            || User.IsInRole(nameof(BulletinBoard.Enums.Roles.Moderator));

        [HttpGet]
        [Authorize(Roles = nameof(BulletinBoard.Enums.Roles.Admin) + "," + nameof(BulletinBoard.Enums.Roles.SuperAdmin))]
        public async Task<IActionResult> LongAbsent([FromQuery] int absentDays = 90)
        {
            if (absentDays < 1) absentDays = 90;
            var vm = await _attendanceService.GetLongAbsentStudentsAsync(_hostingEnvironment, absentDays);
            return View("~/Views/Attendance/LongAbsent.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadNewComerPhoto(int newComerId, string? croppedImageData)
        {
            if (newComerId <= 0 || string.IsNullOrWhiteSpace(croppedImageData))
            {
                TempData["NewComerMsg"] = "사진 데이터를 확인해 주세요.";
                return RedirectToAction(nameof(NewComer));
            }

            try
            {
                var oldPhotoPath = await _attendanceService.GetNewComerPhotoPathAsync(newComerId);

                var commaIndex = croppedImageData.IndexOf(',');
                var base64 = commaIndex >= 0
                    ? croppedImageData.Substring(commaIndex + 1)
                    : croppedImageData;

                var bytes = Convert.FromBase64String(base64);

                if (bytes.Length <= 0)
                {
                    TempData["NewComerMsg"] = "사진 데이터가 비어 있습니다.";
                    return RedirectToAction(nameof(NewComer));
                }

                var webRoot = _hostingEnvironment.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRoot))
                {
                    TempData["NewComerMsg"] = "서버 저장 경로를 찾을 수 없습니다.";
                    return RedirectToAction(nameof(NewComer));
                }

                var folderPath = Path.Combine(webRoot, "uploads", "newcomer", newComerId.ToString());
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"profile_{DateHelper.GetKoreaNow():yyyyMMddHHmmss}_{Guid.NewGuid():N}.jpg";
                var fullPath = Path.Combine(folderPath, fileName);

                await System.IO.File.WriteAllBytesAsync(fullPath, bytes);

                var dbPath = $"/uploads/newcomer/{newComerId}/{fileName}";
                var ok = await _attendanceService.SaveNewComerPhotoPathAsync(newComerId, dbPath, User?.Identity?.Name);

                if (!ok)
                {
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    TempData["NewComerMsg"] = "사진 경로 저장에 실패했습니다.";
                    return RedirectToAction(nameof(NewComer));
                }

                if (!string.IsNullOrWhiteSpace(oldPhotoPath)
                    && oldPhotoPath.StartsWith("/uploads/newcomer/", StringComparison.OrdinalIgnoreCase))
                {
                    var oldPhysicalPath = Path.Combine(
                        webRoot,
                        oldPhotoPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(oldPhysicalPath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldPhysicalPath);
                        }
                        catch
                        {
                            // 이전 파일 삭제 실패는 치명적이지 않으므로 무시
                        }
                    }
                }

                TempData["NewComerMsg"] = "사진을 저장했습니다.";
            }
            catch
            {
                TempData["NewComerMsg"] = "사진 저장 중 오류가 발생했습니다.";
            }

            return RedirectToAction(nameof(NewComer));
        }
        [HttpGet]
        public async Task<IActionResult> AdminStudentMatrixWeeks(DateTime? date, int weeks = 2)
        {
            var input = (date ?? DateHelper.GetKoreaToday()).Date;
            var latestSunday = input.AddDays(-(int)input.DayOfWeek).Date;

            var vm = await _attendanceService.GetAdminStudentMatrixAsync(latestSunday, weeks);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> StatisticsDetail(int? year, int? month)
        {
            var now = DateHelper.GetKoreaNow();
            int y = year ?? now.Year;
            int m = month ?? now.Month;

            if (m < 1) m = 1;
            if (m > 12) m = 12;

            var vm = await _attendanceService.GetMonthlyStatsAsync(y, m);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AdminStudentRank3Weeks(DateTime? date)
        {
            var input = (date ?? DateHelper.GetKoreaToday()).Date;
            var latestSunday = input.AddDays(-(int)input.DayOfWeek).Date;

            var vm = await _attendanceService.GetAdminStudentRankYearToDateAsync(latestSunday);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Statistics(int? year, int? month)
        {
            var now = DateHelper.GetKoreaToday();
            int y = year ?? now.Year;
            int m = month ?? now.Month;

            if (m < 1) m = 1;
            if (m > 12) m = 12;

            var vm = await _attendanceService.GetMonthlyStatsAsync(y, m);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Check(DateTime? date, int classYear = 0, int classBan = 0)
        {
            var input = (date ?? DateHelper.GetKoreaToday()).Date;
            var baseSunday = DateHelper.GetBaseSunday(input);

            if (await _authService.IsSignedIn(User))
            {
                await SetUserInfo();
            }

            MyClassInfo? myClass = null;
            if (!string.IsNullOrWhiteSpace(this.UserId))
            {
                myClass = await _attendanceService.GetMyClassByUserIdAsync(this.UserId);
            }

            if ((classYear <= 0 || classBan <= 0) && myClass != null)
            {
                classYear = myClass.ClassYear;
                classBan = myClass.ClassBan;
            }

            var vm = await _attendanceService.GetCheckPageAsync(baseSunday, classYear, classBan);

            if (myClass != null)
            {
                vm.MyClassYear = myClass.ClassYear;
                vm.MyClassBan = myClass.ClassBan;
                vm.MyClassLabel = $"{myClass.ClassYear}학년 {myClass.ClassBan}반";
                vm.IsMyClass = (vm.ClassYear == myClass.ClassYear && vm.ClassBan == myClass.ClassBan);
            }
            else
            {
                vm.IsMyClass = false;
                vm.MyClassYear = 0;
                vm.MyClassBan = 0;
                vm.MyClassLabel = null;
            }

            return View(vm);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> TeacherAssign()
        {
            var vm = await _attendanceService.GetTeacherAssignPageAsync();
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeacherAssign(TeacherAssignPost req)
        {
            if (await _authService.IsSignedIn(User))
            {
                await SetUserInfo();
            }

            var ok = await _attendanceService.SaveTeacherAssignAsync(
                req.ClassYear,
                req.ClassBan,
                req.TeacherPersonId,
                this.UserId);

            TempData["TeacherAssignMsg"] = ok
                ? "저장했습니다."
                : "저장 실패(교사 후보가 아니거나 값이 올바르지 않습니다).";

            return RedirectToAction(nameof(TeacherAssign));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SavePresent([FromBody] SavePresentRequest req)
        {
            if (req == null)
            {
                return Json(new { ok = false, message = "요청이 비었습니다." });
            }

            if (req.personId <= 0)
            {
                return Json(new { ok = false, message = "personId가 올바르지 않습니다." });
            }

            if (!DateTime.TryParse(req.attendDate, out var attendDate))
            {
                return Json(new { ok = false, message = "날짜 형식이 올바르지 않습니다." });
            }

            var baseSunday = DateHelper.GetBaseSunday(attendDate.Date);

            var result = await _attendanceService.SavePresentAsync(
                baseSunday,
                req.personId,
                req.isPresent,
                User?.Identity?.Name
            );

            return Json(new { ok = result.Ok, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? date, int classYear = 0, int classBan = 0)
        {
            var input = (date ?? DateHelper.GetKoreaToday()).Date;
            var baseSunday = DateHelper.GetBaseSunday(input);

            var vm = await _attendanceService.GetDashboardAsync(baseSunday, classYear, classBan);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> DashboardAll(DateTime? date)
        {
            var input = (date ?? DateHelper.GetKoreaToday()).Date;
            var baseSunday = DateHelper.GetBaseSunday(input);

            if (await _authService.IsSignedIn(User))
            {
                await SetUserInfo();
            }

            var vm = await _attendanceService.GetAllClassesDashboardAsync(baseSunday, this.UserId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AdminTrendSundayDetail(string sunday)
        {
            if (!DateTime.TryParse(sunday, out var sundayDate))
            {
                return BadRequest("Invalid sunday");
            }

            var vm = await _attendanceService.GetSundayClassDetailAsync(sundayDate);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AdminTrend3Weeks(DateTime? date)
        {
            var input = (date ?? DateHelper.GetKoreaToday()).Date;
            var baseDate = DateHelper.GetBaseSunday(input);

            var vm = await _attendanceService.GetAdminTrend3WeeksAsync(baseDate);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> SundayStudentsPC(string sunday)
        {
            if (!DateTime.TryParse(sunday, out var d))
            {
                d = DateHelper.GetKoreaToday();
            }

            var vm = await _attendanceService.GetSundayStudentsDetailAsync(d.Date);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> SundayStudentsMobile(string sunday)
        {
            if (!DateTime.TryParse(sunday, out var d))
            {
                d = DateHelper.GetKoreaToday();
            }

            var vm = await _attendanceService.GetSundayStudentsDetailAsync(d.Date);
            return View(vm);
        }

        // =========================================================
        // 새친구 관리
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> NewComer(string? keyword = "")
        {
            var vm = await _attendanceService.GetNewComerIndexAsync(keyword);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> NewComerDetail(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var vm = await _attendanceService.GetNewComerDetailAsync(id);
            if (vm == null)
            {
                return NotFound();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNewComer(NewComerSaveModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["NewComerMsg"] = "입력값을 확인해 주세요.";
                return RedirectToAction(nameof(NewComer));
            }

            var savedId = await _attendanceService.SaveNewComerAsync(model, User?.Identity?.Name);
            TempData["NewComerMsg"] = savedId > 0 ? "저장했습니다." : "저장하지 못했습니다.";

            return RedirectToAction(nameof(NewComer));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewComerVisitToday(int newComerId)
        {
            if (newComerId <= 0)
            {
                return BadRequest();
            }

            var ok = await _attendanceService.AddNewComerVisitTodayAsync(newComerId, User?.Identity?.Name);
            TempData["NewComerMsg"] = ok
                ? "오늘 출석을 추가했습니다."
                : "이미 오늘 출석이 있거나 저장에 실패했습니다.";

            return RedirectToAction(nameof(NewComer));
        }

        /// <summary>
        /// 새친구 목록에서 "오늘 방문"을 잘못 눌렀을 때 — 오늘 방문 기록(과 그날 출석)을 지운다
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelNewComerVisitToday(int newComerId)
        {
            if (newComerId <= 0)
            {
                return BadRequest();
            }

            var today = DateHelper.GetKoreaToday();
            var detail = await _attendanceService.GetNewComerDetailAsync(newComerId);
            var visit = detail?.Visits.FirstOrDefault(v => v.VisitDate.Date == today);

            var ok = visit != null && await _attendanceService.DeleteNewComerVisitAsync(visit.Id, User?.Identity?.Name);
            TempData["NewComerMsg"] = ok
                ? $"{detail!.Info.ChildName} 오늘 방문을 취소했습니다."
                : "취소할 오늘 방문 기록이 없습니다.";

            return RedirectToAction(nameof(NewComer));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewComerVisitByDate(NewComerVisitSaveModel model)
        {
            if (!ModelState.IsValid || model.VisitDate == null)
            {
                TempData["NewComerMsg"] = "출석일을 확인해 주세요.";
                return RedirectToAction(nameof(NewComerDetail), new { id = model.NewComerId });
            }

            var ok = await _attendanceService.AddNewComerVisitByDateAsync(
                model.NewComerId,
                model.VisitDate.Value.Date,
                User?.Identity?.Name);

            TempData["NewComerMsg"] = ok
                ? "출석을 추가했습니다."
                : "같은 날짜 출석이 이미 있거나 저장에 실패했습니다.";

            return RedirectToAction(nameof(NewComerDetail), new { id = model.NewComerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNewComer(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var ok = await _attendanceService.DeleteNewComerAsync(id, User?.Identity?.Name);
            TempData["NewComerMsg"] = ok ? "삭제했습니다." : "삭제하지 못했습니다.";

            return RedirectToAction(nameof(NewComer));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNewComerInfo(NewComerInfoEditModel model)
        {
            if (model.Id <= 0)
            {
                return BadRequest();
            }

            var ok = await _attendanceService.UpdateNewComerInfoAsync(model, User?.Identity?.Name);
            TempData["NewComerMsg"] = ok ? "수정했습니다." : "수정하지 못했습니다.";

            return RedirectToAction(nameof(NewComerDetail), new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNewComerVisit(int id, int newComerId)
        {
            if (id <= 0 || newComerId <= 0)
            {
                return BadRequest();
            }

            var ok = await _attendanceService.DeleteNewComerVisitAsync(id, User?.Identity?.Name);
            TempData["NewComerMsg"] = ok ? "출석을 삭제했습니다." : "출석을 삭제하지 못했습니다.";

            return RedirectToAction(nameof(NewComerDetail), new { id = newComerId });
        }
    }
}
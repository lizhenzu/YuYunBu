using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize]
    public class VBSParticipationSurveyController : BaseController
    {
        private readonly IAuthService _auth;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly IWebHostEnvironment _env;
        private readonly IVBSParticipationSurveyService _svc;

        public VBSParticipationSurveyController(
            IAuthService auth,
            UserManager<ApplicationUser> userMgr,
            IWebHostEnvironment env,
            IVBSParticipationSurveyService svc
        ) : base(auth, userMgr, env)
        {
            _auth = auth;
            _userMgr = userMgr;
            _env = env;
            _svc = svc;
        }

        private string GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        private static bool IsTeacherRoleCode(string? roleCode)
            => roleCode == "T1002" || roleCode == "T1003" || roleCode == "T1004"
            || roleCode == "T1005" || roleCode == "T1006" || roleCode == "T1007"
            || roleCode == "T1008" || roleCode == "T1009";

        private async Task<UserInfo?> GetMyInfoAsync()
        {
            if (await _auth.IsSignedIn(User))
                await SetUserInfo();

            if (string.IsNullOrWhiteSpace(UserId)) return null;

            var m = new ManageHumanResource();
            var list = m.getUserInfos(_env, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);
            return list.FirstOrDefault();
        }

        private async Task<bool> IsTeacherOrAdminAsync(UserInfo? me)
        {
            //var user = await _auth.GetSignedUser(User);
            //var isAdmin = await _auth.IsInRole(user, nameof(Roles.Admin));
            //var isSuper = await _auth.IsInRole(user, nameof(Roles.SuperAdmin));
            //return isAdmin || isSuper || IsTeacherRoleCode(me?.RoleCode);
            return true;
        }

 
        // ===== 랜딩 페이지 =====
        public async Task<IActionResult> Index()
        {
            var me = await GetMyInfoAsync();
            if (me == null) return Redirect("/Account/Login");

            ViewBag.MeFullName  = me.FullName ?? "";
            ViewBag.IsTeacher   = IsTeacherRoleCode(me.RoleCode);
            ViewBag.MeRoleNM    = me.RoleNM ?? "";
            ViewBag.CanGoAdmin  = await IsTeacherOrAdminAsync(me);

            // 내 슬롯 요약(응답 현황 미리보기)
            var slots = await _svc.GetMySlotsAsync(me.UserName);
            bool hasAnyResponse = slots.Any(kv => kv.Value.IsAttend != null || kv.Value.StatusNote != null);
            ViewBag.HasResponse = hasAnyResponse;

            return View();
        }

        // ===== 학생 참석조사 → Survey?who=student 리다이렉트 =====
        public IActionResult StudentSurvey()
            => RedirectToAction(nameof(Survey), new { who = "student" });

        // ===== 교사/교역자 참석조사 → Survey?who=staff 리다이렉트 =====
        public IActionResult StaffSurvey()
            => RedirectToAction(nameof(Survey), new { who = "staff" });

        // ===== 통계 대시보드 (교사/관리자 전용) =====
        public async Task<IActionResult> Stats(string view = "All")
        {
            var me = await GetMyInfoAsync();
            if (me == null) return Redirect("/Account/Login");
            if (!await IsTeacherOrAdminAsync(me)) return Forbid();

            RankQueryOption opt =
                (view ?? "").ToLowerInvariant() switch
                {
                    "student" => RankQueryOption.StudentOnly,
                    "teacher" => RankQueryOption.TeacherOnly,
                    _ => RankQueryOption.All
                };

            var m = new ManageHumanResource();
            var users = m.getUserInfos(_env, string.Empty, string.Empty, 0, 0, 0, string.Empty, DeptCodeType.D1001.ToString(), opt);
            var vm = await _svc.GetAdminVMAsync(users, DeptCodeType.D1001.ToString());
            ViewBag.ViewMode = view ?? "All";
            return View(vm);
        }

        // ===== 통합 참석조사 (who=student / who=staff) =====
        public async Task<IActionResult> Survey(string who = "")
        {
            var me = await GetMyInfoAsync();
            if (me == null) return Redirect("/Account/Login");

            // who 없으면 역할 자동 감지
            if (string.IsNullOrWhiteSpace(who))
            {
                who = IsTeacherRoleCode(me.RoleCode) ? "staff" : "student";
                return RedirectToAction(nameof(Survey), new { who });
            }

            who = who.ToLowerInvariant();
            ViewBag.MeFullName = me.FullName ?? "";
            ViewBag.CanGoAdmin = await IsTeacherOrAdminAsync(me);
            ViewBag.Who        = who;

            if (who == "staff")
            {
                ViewBag.MeRoleLabel        = me.RoleNM ?? "";
                ViewBag.IsLockedForStudent = false;
            }
            else // student
            {
                var hasClass = me.ClassYear > 0 && me.ClassBan > 0;
                ViewBag.MeRoleLabel        = hasClass ? $"{me.ClassYear}학년 {me.ClassBan}반" : "학생 참여자";
                var now = DateHelper.GetKoreaNow();
                ViewBag.IsLockedForStudent = !IsTeacherRoleCode(me.RoleCode) && now >= ConstatsCommon.VBSParticipationStartDate;
            }

            var slots = await _svc.GetMySlotsAsync(me.UserName);
            return View(slots);
        }
 
        public async Task<IActionResult> Admin(string view = "All")
        {
            var me = await GetMyInfoAsync();
            if (me == null) return Redirect("/Account/Login");

            if (!await IsTeacherOrAdminAsync(me))
                return Forbid();

            RankQueryOption opt =
                (view ?? "").ToLowerInvariant() switch
                {
                    "student" => RankQueryOption.StudentOnly,
                    "teacher" => RankQueryOption.TeacherOnly,
                    _ => RankQueryOption.All
                };

            var m = new ManageHumanResource();
            var users = m.getUserInfos(_env, string.Empty, string.Empty, 0, 0, 0, string.Empty, string.Empty, opt);

            var vm = await _svc.GetAdminVMAsync(users , DeptCodeType.D1001.ToString());
            ViewBag.ViewMode = view ?? "All";
            return View(vm);
        }

        public async Task<IActionResult> ViewAll(string view = "All")
        {
            var me = await GetMyInfoAsync();
            if (me == null) return Redirect("/Account/Login");

            if (!await IsTeacherOrAdminAsync(me))
                return Forbid();

            RankQueryOption opt =
                (view ?? "").ToLowerInvariant() switch
                {
                    "student" => RankQueryOption.StudentOnly,
                    "teacher" => RankQueryOption.TeacherOnly,
                    _ => RankQueryOption.All
                };

            var m = new ManageHumanResource();
            var users = m.getUserInfos(_env, string.Empty, string.Empty, 0, 0, 0, string.Empty, DeptCodeType.D1001.ToString(), opt);

            var vm = await _svc.GetAdminVMAsync(users, DeptCodeType.D1001.ToString());
            ViewBag.ViewMode = view ?? "All";
            return View(vm);
        }


        // ===== AJAX: 관리자 통계(서버 계산) =====
        // - Admin.cshtml에서 주기적으로 호출하여 Live Stats / 날짜별 합계를 서버 기준으로 갱신합니다.
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> AdminStats(string view = "All")
        {
            var me = await GetMyInfoAsync();
            if (me == null) return Unauthorized();
            if (!await IsTeacherOrAdminAsync(me)) return Forbid();

            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            RankQueryOption opt =
                (view ?? "").ToLowerInvariant() switch
                {
                    "student" => RankQueryOption.StudentOnly,
                    "teacher" => RankQueryOption.TeacherOnly,
                    _ => RankQueryOption.All
                };

            var m = new ManageHumanResource();
            var users = m.getUserInfos(_env, string.Empty, string.Empty, 0, 0, 0, string.Empty, DeptCodeType.D1001.ToString(), opt);

            var vm = await _svc.GetAdminVMAsync(users, DeptCodeType.D1001.ToString());

            var days = vm.Days ?? new[]
            {
                ConstatsCommon.VBSParticipationStartDate,
                ConstatsCommon.VBSParticipationStartDate.AddDays(1),
                ConstatsCommon.VBSParticipationStartDate.AddDays(2)
            };

            var hidePmDate = new DateTime(2026, 3, 1).Date; // ✅ 3/1 PM 완전 제외

            bool? getSlot(VBSParticipationSurveyAdminRow r, DateTime d, string slot)
                => r.Slots != null && r.Slots.TryGetValue((d.Date, slot), out var v) ? v : (bool?)null;

            // ✅ ViewAll.cshtml 과 동일한 제외 기준
            bool isExcludedAccount(VBSParticipationSurveyAdminRow r)
            {
                var role = (r.RoleCode ?? "").Trim().ToLowerInvariant();
                var roleNm = (r.RoleNM ?? "").Trim().ToLowerInvariant();
                var name = (r.FullName ?? "").Trim();

                if (role is "guest" or "admin" or "superadmin") return true;
                if (roleNm is "guest" or "admin" or "superadmin") return true;
                if (name == "관리자" || name == "구경하기") return true;

                return false;
            }

            var rows = (vm.Rows ?? new List<VBSParticipationSurveyAdminRow>())
                .Where(r => r != null && !isExcludedAccount(r))
                .ToList();

            int totalPeople = rows.Count;

            bool isFullAttend(VBSParticipationSurveyAdminRow r)
            {
                foreach (var d in days)
                {
                    if (getSlot(r, d, "AM") != true) return false;

                    // ✅ 3/1 PM 제외, 그 외 PM은 필수
                    if (d.Date != hidePmDate)
                    {
                        if (getSlot(r, d, "PM") != true) return false;
                    }
                }
                return true;
            }

            bool isPartialAttend(VBSParticipationSurveyAdminRow r)
            {
                bool anyTrue = false;

                foreach (var d in days)
                {
                    if (getSlot(r, d, "AM") == true) anyTrue = true;

                    if (d.Date != hidePmDate && getSlot(r, d, "PM") == true)
                        anyTrue = true;
                }

                return anyTrue && !isFullAttend(r);
            }

            bool isResponded(VBSParticipationSurveyAdminRow r)
            {
                foreach (var d in days)
                {
                    if (getSlot(r, d, "AM") != null) return true;
                    if (d.Date != hidePmDate && getSlot(r, d, "PM") != null) return true; // ✅ 3/1 PM 제외
                }
                return false;
            }

            int respondedPeople = rows.Count(r => isResponded(r));
            int unrespondedPeople = totalPeople - respondedPeople;
            int fullAttendPeople = rows.Count(r => isFullAttend(r));
            int partialAttendPeople = rows.Count(r => isPartialAttend(r));

            int respondedPct = (totalPeople <= 0) ? 0 : (int)Math.Round((respondedPeople * 100.0) / totalPeople);

            // ✅ 날짜별 합계도 3/1 PM 제외 + 제외계정 반영
            var dayCounts = days
                .Select(d => new
                {
                    date = d.ToString("yyyy-MM-dd"),
                    am = rows.Count(x => getSlot(x, d, "AM") == true),
                    pm = (d.Date == hidePmDate) ? (int?)null : rows.Count(x => getSlot(x, d, "PM") == true)
                })
                .ToList();

            return Ok(new
            {
                ok = true,
                peopleTotal = totalPeople,
                respondedPeople,
                unrespondedPeople,
                fullAttendPeople,
                partialAttendPeople,
                respondedPct,
                dayCounts,
                updatedAtText = DateHelper.GetKoreaNow().ToString("yyyy-MM-dd HH:mm:ss")
            });
        }



        // ===== AJAX: 슬롯 저장(학생 화면) =====
        [HttpPost]
        public async Task<IActionResult> SaveSlot([FromBody] SaveSlotReq req)
        {
            var me = await GetMyInfoAsync();
            if (me == null) return Unauthorized();

            var now = DateHelper.GetKoreaNow();
            if (!IsTeacherRoleCode(me.RoleCode) && now >= ConstatsCommon.VBSParticipationStartDate)
                return BadRequest(new { ok = false, message = "수련회가 시작되어 수정이 종료되었습니다." });

            await _svc.UpsertSlotAsync(
                UserName: me.UserName,
                fullName: me.FullName ?? "",
                roleCode: me.RoleCode ?? "",
                date: req.Date.Date,
                slot: req.Slot,
                isAttend: req.IsAttend,
                requestIp: GetIp(),
                updatedByUserName: me.UserName
            );

            return Ok(new { ok = true });
        }

        // ===== AJAX: 전일 버튼 =====
        [HttpPost]
        public async Task<IActionResult> SetAll([FromBody] SetAllReq req)
        {
            var me = await GetMyInfoAsync();
            if (me == null) return Unauthorized();

            var now = DateHelper.GetKoreaNow();
            if (!IsTeacherRoleCode(me.RoleCode) && now >= ConstatsCommon.VBSParticipationStartDate)
                return BadRequest(new { ok = false, message = "수련회가 시작되어 수정이 종료되었습니다." });

            await _svc.SetAllAsync(me.UserName, me.FullName ?? "", me.RoleCode ?? "", req.IsAttend, GetIp());
            return Ok(new { ok = true });
        }

        // ===== AJAX: 전체 날짜 상태 메모 일괄 저장 =====
        [HttpPost]
        public async Task<IActionResult> SetAllNote([FromBody] SetAllNoteReq req)
        {
            var me = await GetMyInfoAsync();
            if (me == null) return Unauthorized();

            var now = DateHelper.GetKoreaNow();
            if (!IsTeacherRoleCode(me.RoleCode) && now >= ConstatsCommon.VBSParticipationStartDate)
                return BadRequest(new { ok = false, message = "수련회가 시작되어 수정이 종료되었습니다." });

            var allowed = new[] { "CONDITIONAL", "ABSENT_CONFIRM", "UNDECIDED" };
            if (string.IsNullOrWhiteSpace(req.StatusNote) || !Array.Exists(allowed, a => a == req.StatusNote))
                return BadRequest(new { ok = false, message = "올바르지 않은 상태 코드입니다." });

            await _svc.SetAllStatusNoteAsync(me.UserName, me.FullName ?? "", me.RoleCode ?? "", req.StatusNote, GetIp());
            return Ok(new { ok = true });
        }

        // ===== AJAX: 관리자 강제 수정 (Admin 화면) =====
        [HttpPost]
        public async Task<IActionResult> AdminSaveSlot([FromBody] AdminSaveSlotReq req)
        {
            var me = await GetMyInfoAsync();
            if (me == null) return Unauthorized();
            if (!await IsTeacherOrAdminAsync(me)) return Forbid();

            await _svc.UpsertSlotAsync(
                UserName: req.UserName,
                fullName: req.FullName ?? "",
                roleCode: req.RoleCode ?? "",
                date: req.Date.Date,
                slot: req.Slot,
                isAttend: req.IsAttend,
                requestIp: GetIp(),
                updatedByUserName: me.UserName
            );

            return Ok(new { ok = true });
        }

        // ===== 홈 팝업 유예(IP) =====
        [HttpPost]
        public async Task<IActionResult> DeferGuide([FromBody] DeferGuideReq req)
        {
            var ip = GetIp();
            var days = req.Days <= 0 ? 3 : req.Days;
            await _svc.DeferGuideAsync(ip, days);
            return Ok(new { ok = true });
        }

        public class SaveSlotReq
        {
            public DateTime Date { get; set; }
            public string Slot { get; set; } = "AM";
            public bool? IsAttend { get; set; }
        }

        public class SetAllReq
        {
            public bool IsAttend { get; set; }
        }

        public class SetAllNoteReq
        {
            public string StatusNote { get; set; } = "";
        }

        public class AdminSaveSlotReq
        {
            public string UserName { get; set; } = "";
            public string? FullName { get; set; }
            public string? RoleCode { get; set; }
            public DateTime Date { get; set; }
            public string Slot { get; set; } = "AM";
            public bool? IsAttend { get; set; }
        }

        public class DeferGuideReq
        {
            public int Days { get; set; }
        }
    }
}

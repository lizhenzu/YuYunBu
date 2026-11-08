using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulletinBoard.DataAccessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Controllers
{
    /// <summary>
    /// 회원 가입 직후 마법사 — 역할(학생 · 교사 · 학부모)을 고르고 필요한 것만 차례로 입력 → 관리자 승인 요청.
    /// 관리자 = SuperAdmin 역할 + "Basic" 계정. 승인 화면: /SignupWizard/Approvals
    /// (2026-11-08 시안 확정. 예전 "같은 이름 질문(/PersonMatch)"은 이 마법사 안으로 들어왔다)
    /// </summary>
    [Authorize]
    public class SignupWizardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ControlSignup _db = new ControlSignup();

        public SignupWizardController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        private bool IsApprover()
            => User.IsInRole(nameof(Roles.SuperAdmin))
               || string.Equals(User.Identity?.Name, "Basic", StringComparison.OrdinalIgnoreCase);

        private string SafeUrl(string? u) => !string.IsNullOrEmpty(u) && Url.IsLocalUrl(u) ? u : "/";

        private static string FullNameOf(ApplicationUser u) => ((u.LastName ?? "") + (u.FirstName ?? "")).Trim();

        private static (List<int> Years, List<int> Bans) ClassOptions()
        {
            var years = new SortedSet<int>();
            var bans = new SortedSet<int>();
            try
            {
                var ds = new ControlHumanResource().fetchClassOptions();
                foreach (System.Data.DataRow r in ds.Tables[0].Rows)
                {
                    years.Add(Convert.ToInt32(r["ClassYear"]));
                    bans.Add(Convert.ToInt32(r["ClassBan"]));
                }
            }
            catch { }
            if (years.Count == 0) for (int i = 1; i <= 6; i++) years.Add(i);
            if (bans.Count == 0) for (int i = 1; i <= 8; i++) bans.Add(i);
            return (years.ToList(), bans.ToList());
        }

        // ── 회원: 마법사 ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index(string? returnUrl)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            var latest = await _db.GetLatestOfAsync(me.Id);
            if (latest != null && latest.Status == "APPROVED")
                return LocalRedirect(SafeUrl(returnUrl));

            var fullName = FullNameOf(me);
            ViewData["FullName"] = fullName;
            ViewData["ReturnUrl"] = SafeUrl(returnUrl);
            ViewData["Latest"] = latest;          // PENDING 이면 대기 화면, REJECTED 면 사유를 보여 주고 다시 입력
            ViewData["Msg"] = TempData["SignupMsg"] as string;

            // 같은 이름 기록 — 이미 내 기록이 있으면(가입 때 자동 생성) 묻지 않는다
            var match = new ControlPersonMatch();
            var same = new List<PersonCandidate>();
            try
            {
                if (!await match.HasPersonForUserAsync(me.UserName ?? ""))
                    same = await match.FindByFullNameAsync(fullName);
            }
            catch { }
            ViewData["Same"] = same;

            var (years, bans) = ClassOptions();
            ViewData["Years"] = years;
            ViewData["Bans"] = bans;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string role, int personId, int classYear, int classBan, int bMonth, int bDay,
                                                string? sex, string? school, string? phone, string[]? kidName, int[]? kidYear, int[]? kidBan, string? returnUrl)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            IActionResult Back(string m)
            {
                TempData["SignupMsg"] = m;
                return RedirectToAction(nameof(Index), new { returnUrl });
            }

            role = (role ?? "").Trim().ToUpperInvariant();
            if (role != "S" && role != "T" && role != "P") return Back("역할을 골라 주세요.");
            if (string.IsNullOrWhiteSpace(phone)) return Back("연락처를 적어 주세요.");

            var a = new SignupApplication
            {
                UserId = me.Id,
                UserName = me.UserName ?? "",
                FullName = FullNameOf(me),
                RoleKind = role,
                Phone = phone.Trim()
            };

            if (role == "S" || role == "T")
            {
                // 같은 이름 기록을 골랐으면, 정말 같은 이름이고 아직 계정이 없는 기록인지 확인
                if (personId > 0)
                {
                    var same = await new ControlPersonMatch().FindByFullNameAsync(a.FullName);
                    var t = same.FirstOrDefault(p => p.PersonId == personId);
                    if (t == null || !string.IsNullOrEmpty(t.UserName)) return Back("고른 기록은 이미 다른 계정과 연결돼 있어요. 다시 골라 주세요.");
                    a.PersonId = personId;
                }
            }

            if (role == "S")
            {
                if (bMonth < 1 || bMonth > 12 || bDay < 1 || bDay > 31) return Back("생일을 골라 주세요.");
                if (sex != "M" && sex != "F") return Back("성별을 골라 주세요.");
                a.ClassYear = Math.Max(0, classYear);
                a.ClassBan = Math.Max(0, classBan);
                if (a.ClassYear == 0 || a.ClassBan == 0) { a.ClassYear = 0; a.ClassBan = 0; }   // 하나라도 모르면 관리자가 정한다
                a.BMonth = bMonth; a.BDay = bDay; a.Sex = sex;
                a.School = (school ?? "").Trim();
            }
            else if (role == "P")
            {
                var kids = new List<string>();
                for (int i = 0; kidName != null && i < kidName.Length; i++)
                {
                    var n = (kidName[i] ?? "").Trim();
                    if (n.Length == 0) continue;
                    int y = kidYear != null && i < kidYear.Length ? kidYear[i] : 0;
                    int b = kidBan != null && i < kidBan.Length ? kidBan[i] : 0;
                    kids.Add(n + (y > 0 && b > 0 ? $" {y}학년 {b}반" : " (반 모름)"));
                }
                if (kids.Count == 0) return Back("자녀 이름을 한 명 이상 적어 주세요.");
                a.Kids = string.Join(", ", kids);
                if (a.Kids.Length > 1000) a.Kids = a.Kids.Substring(0, 1000);
            }

            if (!await _db.CreateAsync(a)) return Back("이미 승인을 요청했어요.");

            // 관리자(SuperAdmin 역할 + Basic 계정)에게 쪽지
            try
            {
                var to = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var u in await _userManager.GetUsersInRoleAsync(nameof(Roles.SuperAdmin)))
                    if (!string.IsNullOrWhiteSpace(u.UserName)) to.Add(u.UserName);
                var basic = await _userManager.FindByNameAsync("Basic");
                if (basic?.UserName != null) to.Add(basic.UserName);

                var msg = new ControlWithdraw();
                foreach (var name in to)
                    await msg.SendMessageAsync(name, a.UserName, $"{a.FullName}({a.UserName}) 님이 {a.RoleLabel}(으)로 가입 승인을 요청했어요. 정보 관리 › 가입 승인에서 확인해 주세요.");
            }
            catch { /* 쪽지가 실패해도 신청은 저장됐다 */ }

            return RedirectToAction(nameof(Index), new { returnUrl });
        }

        // ── 관리자: 승인 목록 ────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Approvals()
        {
            if (!IsApprover()) return Forbid();
            var (years, bans) = ClassOptions();
            ViewData["Years"] = years;
            ViewData["Bans"] = bans;
            return View(await _db.GetPendingAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Decide(int id, bool approve, bool linkExisting, int classYear, int classBan, string? teacherRole, string? reason)
        {
            if (!IsApprover()) return Json(new { ok = false, message = "권한이 없어요." });
            var admin = User.Identity?.Name ?? "";
            try
            {
                var notify = new ControlWithdraw();
                if (!approve)
                {
                    if (string.IsNullOrWhiteSpace(reason)) return Json(new { ok = false, message = "반려 사유를 적어 주세요." });
                    var app = await _db.RejectAsync(id, reason.Trim(), admin);
                    if (app == null) return Json(new { ok = false, message = "이미 처리된 신청이에요." });
                    try { await notify.SendMessageAsync(app.UserName, admin, $"가입 승인 요청이 반려됐어요. 사유: {reason.Trim()} — 다시 접속하면 입력 화면이 나와요."); } catch { }
                    return Json(new { ok = true, message = "반려했어요." });
                }

                var (ok, message, a) = await _db.ApproveAsync(id, admin, linkExisting, classYear, classBan, teacherRole);
                if (ok && a != null)
                {
                    try { await notify.SendMessageAsync(a.UserName, admin, "가입이 승인됐어요. 이제 모든 기능을 쓸 수 있어요."); } catch { }
                }
                return Json(new { ok, message });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = "처리 오류: " + ex.Message });
            }
        }
    }
}

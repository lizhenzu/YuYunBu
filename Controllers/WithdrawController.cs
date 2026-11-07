using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BulletinBoard.DataAccessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Controllers
{
    /// <summary>
    /// 회원 탈퇴 요청 → SuperAdmin 쪽지 알림 → SuperAdmin 이 승인하면 삭제 (설계서: docs/탈퇴요청_설계서.md)
    /// 회원: /Withdraw/Request · SuperAdmin: /Withdraw/Requests
    /// </summary>
    [Authorize]
    public class WithdrawController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ControlWithdraw _db = new ControlWithdraw();

        public WithdrawController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        private static string FullNameOf(ApplicationUser u) => ((u.LastName ?? "") + (u.FirstName ?? "")).Trim();

        // ── 회원: 탈퇴 요청 화면 ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Request()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            if (await _userManager.IsInRoleAsync(me, nameof(Roles.SuperAdmin)))
            {
                TempData["WithdrawMsg"] = "SuperAdmin 계정은 탈퇴를 요청할 수 없어요.";
                return RedirectToAction("ManageUserInfo", "HumanResource", new { UserId = me.Id });
            }

            ViewData["FullName"] = FullNameOf(me);
            ViewData["Msg"] = TempData["WithdrawMsg"] as string;
            var pending = await _db.GetPendingOfAsync(me.Id);
            ViewData["Pending"] = pending;
            if (pending == null)
                ViewData["Groups"] = await _db.PreviewAsync(me.Id, me.UserName ?? "");
            return View();
        }

        // ── 회원: 요청 보내기 ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(string? reason, bool agree)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            if (await _userManager.IsInRoleAsync(me, nameof(Roles.SuperAdmin)))
            {
                TempData["WithdrawMsg"] = "SuperAdmin 계정은 탈퇴를 요청할 수 없어요.";
                return RedirectToAction(nameof(Request));
            }
            if (!agree)
            {
                TempData["WithdrawMsg"] = "삭제될 정보를 확인했다고 체크해 주세요.";
                return RedirectToAction(nameof(Request));
            }

            var fullName = FullNameOf(me);
            bool created = await _db.CreateRequestAsync(me.Id, me.UserName ?? "", fullName, reason);
            if (!created)
            {
                TempData["WithdrawMsg"] = "이미 탈퇴 요청을 보냈어요.";
                return RedirectToAction(nameof(Request));
            }

            // 모든 SuperAdmin 에게 쪽지
            try
            {
                var admins = await _userManager.GetUsersInRoleAsync(nameof(Roles.SuperAdmin));
                foreach (var a in admins.Where(x => !string.IsNullOrWhiteSpace(x.UserName)))
                    await _db.SendMessageAsync(a.UserName!, me.UserName ?? "", $"{fullName}({me.UserName}) 님이 탈퇴를 요청했어요. 정보 관리 › 사람 · 권한 › 탈퇴 요청 확인에서 처리해 주세요.");
            }
            catch { /* 쪽지가 실패해도 요청은 저장됐다 */ }

            TempData["WithdrawMsg"] = "탈퇴 요청을 보냈어요.";
            return RedirectToAction(nameof(Request));
        }

        // ── 회원: 요청 취소 ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int requestId)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();
            bool ok = await _db.SetStatusAsync(requestId, "CANCELED", me.UserName, onlyUserId: me.Id);
            TempData["WithdrawMsg"] = ok ? "탈퇴 요청을 취소했어요." : "이미 처리된 요청이에요.";
            return RedirectToAction(nameof(Request));
        }

        // ── SuperAdmin: 요청 목록 ────────────────────────────────────
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var list = await _db.GetPendingListAsync();
            var previews = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<WithdrawGroup>>();
            foreach (var r in list)
            {
                try { previews[r.Id] = await _db.PreviewAsync(r.UserId, r.UserName); }
                catch { previews[r.Id] = new System.Collections.Generic.List<WithdrawGroup>(); }
            }
            ViewData["Previews"] = previews;
            return View(list);
        }

        // ── SuperAdmin: 승인(삭제) · 반려 ────────────────────────────
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        [HttpPost]
        public async Task<IActionResult> Decide(int requestId, bool approve)
        {
            try
            {
                var req = await _db.GetByIdAsync(requestId);
                if (req == null || req.Status != "PENDING")
                    return Json(new { ok = false, message = "이미 처리된 요청이에요." });

                var admin = User.Identity?.Name ?? "";

                if (!approve)
                {
                    if (!await _db.SetStatusAsync(requestId, "REJECTED", admin))
                        return Json(new { ok = false, message = "이미 처리된 요청이에요." });
                    try { await _db.SendMessageAsync(req.UserName, admin, "탈퇴 요청이 반려됐어요. 계속 이용하실 수 있어요."); } catch { }
                    return Json(new { ok = true, message = "반려했어요." });
                }

                var user = await _userManager.FindByIdAsync(req.UserId);
                if (user != null)
                {
                    if (string.Equals(user.Id, _userManager.GetUserId(User), StringComparison.Ordinal))
                        return Json(new { ok = false, message = "내 계정은 삭제할 수 없어요." });
                    if (await _userManager.IsInRoleAsync(user, nameof(Roles.SuperAdmin)))
                        return Json(new { ok = false, message = "SuperAdmin 계정은 삭제할 수 없어요." });
                }

                // 데이터 먼저(한 트랜잭션) → 프로필 사진 파일 → 계정. 중간에 실패하면 요청이 남아 다시 처리할 수 있다.
                await _db.DeleteDataAsync(req.UserId, req.UserName);
                DeleteProfileFiles(req.UserId);

                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                        return Json(new { ok = false, message = "계정 삭제 오류: " + string.Join(", ", result.Errors.Select(e => e.Description)) });
                }

                await _db.SetStatusAsync(requestId, "APPROVED", admin);
                return Json(new { ok = true, message = "삭제했어요." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = "처리 오류: " + ex.Message });
            }
        }

        // wwwroot/images/profile/{userId}.{확장자}
        private void DeleteProfileFiles(string userId)
        {
            try
            {
                var dir = Path.Combine(_env.WebRootPath, "images", "profile");
                if (!Directory.Exists(dir) || string.IsNullOrWhiteSpace(userId)) return;
                foreach (var f in Directory.GetFiles(dir, userId + ".*"))
                    System.IO.File.Delete(f);
            }
            catch { /* 파일 삭제 실패는 탈퇴 자체를 막지 않는다 */ }
        }
    }
}

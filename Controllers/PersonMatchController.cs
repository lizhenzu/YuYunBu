using System;
using System.Linq;
using System.Threading.Tasks;
using BulletinBoard.DataAccessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Controllers
{
    /// <summary>
    /// 회원 가입 직후 — PersonMaster_2026 에 같은 한글 이름이 이미 있을 때 "어느 쪽이 나인지" 묻는 화면.
    /// 규칙: 같은 이름이 없으면 가입 때 자동으로 만들고(UserName 포함), 있으면 이 화면으로 온다.
    /// </summary>
    [Authorize]
    public class PersonMatchController : Controller
    {
        private readonly ControlPersonMatch _db = new ControlPersonMatch();

        [HttpGet]
        public async Task<IActionResult> Index(string? fullName, string? returnUrl)
        {
            var userName = User.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(fullName))
                return LocalRedirect(SafeUrl(returnUrl));

            // 이미 내 기록이 있거나, 연결 요청을 보내 확인 대기 중이면 다시 묻지 않는다
            if (await _db.HasPersonForUserAsync(userName) || await _db.HasPendingAsync(userName))
                return LocalRedirect(SafeUrl(returnUrl));

            var list = await _db.FindByFullNameAsync(fullName);
            if (list.Count == 0)
            {
                await _db.CreateAsync(fullName, userName);
                return LocalRedirect(SafeUrl(returnUrl));
            }

            ViewData["FullName"] = fullName;
            ViewData["ReturnUrl"] = SafeUrl(returnUrl);
            return View(list);
        }

        /// <summary>personId > 0 : 그 기록에 내 계정 연결 · personId = 0 : 동명이인, 새로 만들기</summary>
        [HttpPost]
        public async Task<IActionResult> Choose(string fullName, int personId, string? returnUrl)
        {
            var userName = User.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(fullName))
                return LocalRedirect(SafeUrl(returnUrl));

            if (await _db.HasPersonForUserAsync(userName) || await _db.HasPendingAsync(userName))
                return LocalRedirect(SafeUrl(returnUrl));

            if (personId > 0)
            {
                // 목록에 있는 같은 이름의 기록이고 아직 계정이 없을 때만 요청할 수 있다.
                // 바로 연결하지 않고 요청만 저장한다 — SuperAdmin 이 확인해야 연결된다.
                var list = await _db.FindByFullNameAsync(fullName);
                var target = list.FirstOrDefault(p => p.PersonId == personId);
                if (target == null || !string.IsNullOrEmpty(target.UserName))
                {
                    TempData["PersonMatchMsg"] = "그 기록은 이미 다른 계정과 연결돼 있어요. 다시 골라 주세요.";
                    return RedirectToAction(nameof(Index), new { fullName, returnUrl });
                }
                await _db.CreateRequestAsync(userName, fullName, personId);
            }
            else
            {
                await _db.CreateAsync(fullName, userName);
            }

            return LocalRedirect(SafeUrl(returnUrl));
        }

        // ── SuperAdmin: 연결 요청 확인 ───────────────────────────────
        [Authorize(Roles = nameof(BulletinBoard.Enums.Roles.SuperAdmin))]
        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            return View(await _db.GetPendingRequestsAsync());
        }

        [Authorize(Roles = nameof(BulletinBoard.Enums.Roles.SuperAdmin))]
        [HttpPost]
        public async Task<IActionResult> Decide(int requestId, bool approve)
        {
            try
            {
                var (ok, message) = await _db.DecideAsync(requestId, approve);
                return Json(new { ok, message });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = "처리 오류: " + ex.Message });
            }
        }

        private string SafeUrl(string? returnUrl)
            => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
    }
}

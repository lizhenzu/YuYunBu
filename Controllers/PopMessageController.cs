using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.PopMessage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [Authorize]
    public class PopMessageController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPopMessageService _svc;
        private readonly IHttpContextAccessor _http;

        private const int MAX_LEN = 500;

        private static readonly string[] BLOCK_WORDS = new[]
        {
            "욕설1", "욕설2", "비속어", "광고", "도박", "음란"
        };

        public PopMessageController(
            IPopMessageService svc,
            IHttpContextAccessor http,
            IAuthService authService,
            IWebHostEnvironment hostingEnvironment,
            UserManager<ApplicationUser> userManager
        ) : base(authService, userManager, hostingEnvironment)
        {
            _svc = svc;
            _http = http;
            _authService = authService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/PopMessage/Index.cshtml");
        }

        [HttpGet]
        public IActionResult Compose(string to, string? returnUrl = null)
        {
            to = (to ?? "").Trim();
            if (string.IsNullOrWhiteSpace(to)) return NotFound();

            if (!_svc.CanSendTo(to))
                return Content("수신자(UserName)가 없거나(UserId NULL) 메시지를 보낼 수 없습니다.");

            var vm = _svc.GetComposeVM(to, returnUrl);
            return View("~/Views/PopMessage/Compose.cshtml", vm);
        }

        // ✅ 전도사님께 기도 편지 — 수신자 "onlylove224" 하드코딩
        [HttpGet]
        public IActionResult PrayLetter(string? returnUrl = null)
        {
            const string toUserName = "onlylove224";

            if (!_svc.CanSendTo(toUserName))
                return Content("전도사님 계정 정보를 불러오지 못했습니다. 잠시 후 다시 시도해 주세요.");

            var vm = _svc.GetComposeVM(toUserName, returnUrl ?? "/Home/Index");
            return View("~/Views/PopMessage/PrayLetter.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(PopMessageComposeVM vm)
        {
            var to = (vm.ToUserName ?? "").Trim();
            var msg = NormalizeNewlines(vm.MessageText);

            var from = await GetMyUserName_ByUserIdView();
            if (string.IsNullOrWhiteSpace(from))
                return Content("로그인 사용자 UserName을 가져오지 못했습니다. (UserId→view_PersonMaster_2026 조회 실패)");

            var err = ValidateForSend(to, msg);
            if (err != null) return BadRequest(err);

            _svc.Send(to, from, msg);

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl!);

            return RedirectToAction("Compose", new { to = to, returnUrl = vm.ReturnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAjax([FromForm] string ToUserName, [FromForm] string MessageText)
        {
            var to = (ToUserName ?? "").Trim();
            var msg = NormalizeNewlines(MessageText);

            var from = await GetMyUserName_ByUserIdView();
            if (string.IsNullOrWhiteSpace(from))
                return Json(new { ok = false, message = "로그인 사용자 UserName을 가져오지 못했습니다." });

            var err = ValidateForSend(to, msg);
            if (err != null)
                return Json(new { ok = false, message = err });

            try
            {
                _svc.Send(to, from, msg);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = "전송 중 오류가 발생했습니다.", detail = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> InboxList(int page = 0, int pageSize = 20)
        {
            var me = await GetMyUserName_ByUserIdView();
            if (string.IsNullOrWhiteSpace(me))
                return Json(new { ok = false, message = "내 UserName을 가져오지 못했습니다." });

            if (page < 0) page = 0;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 50) pageSize = 50;

            var total = _svc.GetInboxTotalCount(me);
            var items = _svc.GetInbox(me, page, pageSize);

            return Json(new { ok = true, totalCount = total, items });
        }

        [HttpGet]
        public async Task<IActionResult> SentList(int page = 0, int pageSize = 20)
        {
            var me = await GetMyUserName_ByUserIdView();
            if (string.IsNullOrWhiteSpace(me))
                return Json(new { ok = false, message = "내 UserName을 가져오지 못했습니다." });

            if (page < 0) page = 0;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 50) pageSize = 50;

            var total = _svc.GetSentTotalCount(me);
            var items = _svc.GetSent(me, page, pageSize);

            return Json(new { ok = true, totalCount = total, items });
        }


        // ✅ 안읽음 카운트
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var me = await GetMyUserName_ByUserIdView();
            if (string.IsNullOrWhiteSpace(me))
                return Json(new { ok = false, count = 0 });

            var count = _svc.GetInboxUnreadCount(me);
            return Json(new { ok = true, count });
        }

        // ✅ 읽음 처리
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead([FromForm] string ids)
        {
            var me = await GetMyUserName_ByUserIdView();
            if (string.IsNullOrWhiteSpace(me))
                return Json(new { ok = false, message = "내 UserName을 가져오지 못했습니다." });

            var arr = (ids ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var n) ? n : 0)
                .Where(n => n > 0)
                .Distinct()
                .ToArray();

            var updated = _svc.MarkReadBulk(me, arr);
            return Json(new { ok = true, updated });
        }

        // ✅ 삭제
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int id)
        {
            var me = await GetMyUserName_ByUserIdView();
            if (string.IsNullOrWhiteSpace(me))
                return Json(new { ok = false, message = "내 UserName을 가져오지 못했습니다." });

            var deleted = _svc.DeleteMessage(me, id);
            return Json(new { ok = deleted > 0 });
        }

        // ✅ 사람 검색
        [HttpGet]
        public IActionResult SearchPeople(string q, int take = 20)
        {
            q = (q ?? "").Trim();
            if (q.Length < 1) return Json(new { ok = true, items = new List<object>() });

            var items = _svc.SearchPeople(q, take);
            return Json(new { ok = true, items });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> AdminDeleteSuperAdminMessages()
        {
            try
            {
                // ✅ 여기서 실제 삭제
                var deleted = await _svc.DeleteAllByFromWhomAsync("SuperAdmin");

                return Json(new
                {
                    ok = true,
                    deleted = deleted
                });
            }
            catch (Exception ex)
            {
                // ✅ 운영환경에서는 상세 메시지 숨기는 것도 고려 가능
                return Json(new
                {
                    ok = false,
                    message = ex.Message
                });
            }
        }
        // ------------------------
        // helpers
        // ------------------------

        private static string NormalizeNewlines(string? input)
        {
            var s = (input ?? "").Trim();
            s = s.Replace("\r\n", "\n").Replace("\r", "\n");
            return s;
        }

        private string? ValidateForSend(string to, string msg)
        {
            if (string.IsNullOrWhiteSpace(to)) return "ToUserName empty";
            if (string.IsNullOrWhiteSpace(msg)) return "MessageText empty";

            if (msg.Length > MAX_LEN)
                return $"메시지는 최대 {MAX_LEN}자까지 가능합니다.";

            if (!_svc.CanSendTo(to))
                return "수신자(UserName)가 없거나(UserId NULL) 메시지를 보낼 수 없습니다.";

            var lower = msg.ToLowerInvariant();
            var bad = BLOCK_WORDS.FirstOrDefault(w =>
                !string.IsNullOrWhiteSpace(w) && lower.Contains(w.ToLowerInvariant()));

            if (!string.IsNullOrWhiteSpace(bad))
                return $"금칙어가 포함되어 있어요: {bad}";

            return null;
        }

        // ✅ 확정 규칙: 로그인 UserId -> view_PersonMaster_2026 -> UserName
        public async Task<string> GetMyUserName_ByUserIdView()
        {
            // 로그인 상태면 BaseController의 SetUserInfo()가 UserName을 채우는 구조로 보임
            if (await _authService.IsSignedIn(User))
            {
                await SetUserInfo();   // ✅ result 변수 필요없음 (그리고 await 꼭!)
            }
       
            return this.UserName ?? "";
        }

        /// <summary>
        /// ✅ SuperAdmin 전용: 전체 사용자에게 관리자 메세지 전송
        /// JS에서 POST: /PopMessage/AdminBroadcast
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminBroadcast(string MessageText)
        {
            try
            {
                var user = await _authService.GetSignedUser(User);
                var isSignedIn = await _authService.IsSignedIn(User);
                var isSuperAdmin = isSignedIn && await _authService.IsInRole(user, nameof(Roles.SuperAdmin));
                if (!isSuperAdmin)
                    return Json(new { ok = false, message = "권한이 없습니다." });

                var msg = (MessageText ?? "").Trim();
                if (string.IsNullOrWhiteSpace(msg))
                    return Json(new { ok = false, message = "메세지를 입력해 주세요." });

                if (msg.Length > 500)
                    return Json(new { ok = false, message = "메세지는 최대 500자입니다." });

                // ✅ 보내는자 하드코딩
                await _svc.AdminBroadcastAllUsersAsync(msg, "SuperAdmin");

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                // 운영에서는 detail 숨기는 걸 권장하지만, 지금은 디버깅 편의로 남김
                return Json(new { ok = false, message = "전송 중 오류가 발생했습니다.", detail = ex.Message });
            }
        }
    }
}

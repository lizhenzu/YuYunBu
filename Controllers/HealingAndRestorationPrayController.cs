using BulletinBoard.BusinessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Models.HealingAndRestorationPrays;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    /// <summary>
    /// 치유와 회복을 위한 기도문
    ///  - Index / Detail : 누구나
    ///  - List / Edit · 클릭 카운트 제외 : 기도 관리자(로그인 아이디 Basic · SuperAdmin) — 2026-10-11
    ///  - Create / Delete : 원래대로 슈퍼 유저(SuperAdmin 역할 · basic 아이디)
    /// </summary>
    [LogActionFilter]
    [AllowAnonymous]
    public class HealingAndRestorationPrayController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly ManageHealingAndRestorationPray _mgr = new ManageHealingAndRestorationPray();

        public HealingAndRestorationPrayController(
            IAuthService authService,
            IWebHostEnvironment hostingEnvironment,
            UserManager<ApplicationUser> userManager)
            : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>슈퍼 유저로 인정할 로그인 아이디 (역할과 별개)</summary>
        private const string SuperUserName = "basic";

        /// <summary>
        /// 슈퍼 유저 = SuperAdmin 역할  또는  로그인 아이디가 'basic'
        ///  - Roles.Basic 은 일반 사용자 기본 역할이라 쓰지 않습니다.
        ///    (그걸로 판정하면 모든 사용자가 슈퍼 유저가 됩니다)
        /// </summary>
        private async Task<bool> IsSuperUserAsync()
        {
            if (!await _authService.IsSignedIn(User)) return false;

            var user = await _authService.GetSignedUser(User);
            if (user == null) return false;

            if (await _authService.IsInRole(user, nameof(Roles.SuperAdmin)))
                return true;

            return string.Equals(user.UserName, SuperUserName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>슈퍼 유저가 아니면 오늘의 기도문으로 돌려보냅니다. (등록 · 삭제 — 원래 권한 그대로)</summary>
        private async Task<IActionResult> GuardSuperUserAsync()
        {
            if (await IsSuperUserAsync()) return null;
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 기도 관리자 = 로그인 아이디 "Basic" · "SuperAdmin" 두 계정 (2026-10-11, PrayManagerHelper)
        ///  → 읽음 수 안 올림 · 목록 · 수정
        /// </summary>
        private bool IsPrayManager => PrayManagerHelper.IsPrayManager(User);

        private IActionResult GuardPrayManager()
            => IsPrayManager ? null : RedirectToAction(nameof(Index));

        private void SetUrlBag()
        {
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain         = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName     = urlInfo.ActionName;
            ViewBag.FullURL        = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";
        }

        // ══════════════════════════════════════════
        //  오늘의 기도문 — 누구나
        // ══════════════════════════════════════════
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            SetUrlBag();

            int total = _mgr.GetActiveCount();
            var today = _mgr.GetTodayPray();

            bool isSuper = IsPrayManager;

            // 일반 사용자가 오늘의 기도문 화면에 들어오면 조회수 +1
            // (슈퍼 유저는 집계하지 않습니다)
            if (!isSuper && today != null && today.PrayId > 0)
            {
                try { _mgr.IncreaseClick(today.PrayId); } catch { }
            }

            ViewBag.IsSuperAdmin = isSuper;
            ViewBag.TotalCount   = total;
            ViewBag.RotationNo   = _mgr.GetTodayRotationNo(total);
            ViewBag.TodayKst     = DateHelper.GetKoreaNow();
            ViewBag.VerifyReady  = isSuper && _mgr.IsVerifyReady();   // 검증 칸이 있고 관리자일 때만

            return View(today);
        }

        // ══════════════════════════════════════════
        //  좋아요 — 누구나 (화면에서 ajax 로 호출)
        //  같은 사람이 여러 번 누르는 것은 브라우저(localStorage)에서 막고,
        //  서버는 같은 세션에서 같은 기도문에 한 번만 반영한다.
        // ══════════════════════════════════════════
        [AllowAnonymous]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Like(int prayId)
        {
            if (prayId <= 0)
                return Json(new { ok = false, msg = "잘못된 요청입니다." });

            var sessionKey = $"likedPray:{prayId}";

            // 이미 누른 세션이면 수만 돌려준다
            if (HttpContext.Session.GetString(sessionKey) == "1")
            {
                int cur = 0;
                try { cur = _mgr.GetLikeCount(prayId); } catch { }
                return Json(new { ok = true, already = true, count = cur });
            }

            int count;
            try
            {
                count = _mgr.IncreaseLike(prayId);
            }
            catch
            {
                return Json(new { ok = false, msg = "잠시 후 다시 눌러 주세요." });
            }

            HttpContext.Session.SetString(sessionKey, "1");
            return Json(new { ok = true, already = false, count = count });
        }

        // ══════════════════════════════════════════
        //  기도문 상세 — 누구나 (SuperAdmin 외 클릭 집계)
        // ══════════════════════════════════════════
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int PrayId)
        {
            SetUrlBag();

            bool isSuper = IsPrayManager;

            // 기도 관리자(Basic · SuperAdmin 계정)는 카운트하지 않습니다
            if (!isSuper && PrayId > 0)
            {
                try { _mgr.IncreaseClick(PrayId); } catch { }
            }

            var item = _mgr.GetById(PrayId);
            if (item == null) return RedirectToAction(nameof(Index));

            ViewBag.IsSuperAdmin = isSuper;
            ViewBag.TodayKst     = DateHelper.GetKoreaNow();
            ViewBag.VerifyReady  = isSuper && _mgr.IsVerifyReady();   // 검증 칸이 있고 관리자일 때만

            return View(item);
        }

        // ══════════════════════════════════════════
        //  검증 완료 표시 — 기도 관리자(Basic · SuperAdmin 계정)만 (부부 기도와 같은 방식)
        //  이미 검증된 기도문은 그대로 둔다. 수정하면 다시 "검증 전"으로 돌아간다.
        // ══════════════════════════════════════════
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Verify(int PrayId, string? returnTo = null)
        {
            var deny = GuardPrayManager();
            if (deny != null) return deny;

            if (PrayId > 0)
            {
                try { _mgr.Verify(PrayId, User.Identity?.Name); } catch { }
            }

            // 오늘의 기도(Index)에서 눌렀으면 그 화면으로, 아니면 한 편 보기로 돌아간다
            if (string.Equals(returnTo, "index", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction(nameof(Index));
            return RedirectToAction(nameof(Detail), new { PrayId = PrayId });
        }

        // 검증 취소 — 잘못 눌렀거나 다시 살펴봐야 할 때 "검증 전" 으로 되돌린다 (기도 관리자만)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Unverify(int PrayId, string? returnTo = null)
        {
            var deny = GuardPrayManager();
            if (deny != null) return deny;

            if (PrayId > 0)
            {
                try { _mgr.Unverify(PrayId); } catch { }
            }

            if (string.Equals(returnTo, "index", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction(nameof(Index));
            return RedirectToAction(nameof(Detail), new { PrayId = PrayId });
        }

        // ══════════════════════════════════════════
        //  목록 / 관리 — 슈퍼 유저(SuperAdmin · basic) 만
        //  ※ [Authorize(Roles=...)] 로는 "역할 또는 특정 아이디"를 표현할 수
        //     없어서, 로그인 확인은 속성으로 하고 권한 판정은 안에서 합니다.
        // ══════════════════════════════════════════
        [Authorize]
        public async Task<IActionResult> List()
        {
            var deny = GuardPrayManager();
            if (deny != null) return deny;

            SetUrlBag();
            var list = _mgr.GetList(includeUnused: true);
            ViewBag.TotalCount = _mgr.GetActiveCount();
            ViewBag.RotationNo = _mgr.GetTodayRotationNo(ViewBag.TotalCount);

            // ⚠️ 목록을 수정일 순으로 늘어놓으므로, 오늘의 기도문을 "몇 번째 줄인가"로
            //    찾으면 안 됩니다. 오늘 노출되는 기도문의 PrayId 를 직접 구해서 넘깁니다.
            ViewBag.TodayPrayId = _mgr.GetTodayPray()?.PrayId ?? 0;
            ViewBag.VerifyReady = _mgr.IsVerifyReady();   // 이 화면은 관리자 전용 — 칸만 확인

            return View(list);
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var deny = await GuardSuperUserAsync();
            if (deny != null) return deny;

            SetUrlBag();
            var list = _mgr.GetList(includeUnused: true);
            int nextOrder = 1;
            foreach (var x in list) if (x.SortOrder >= nextOrder) nextOrder = x.SortOrder + 1;

            return View("Edit", new HealingAndRestorationPray { SortOrder = nextOrder, UseYN = "Y" });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int PrayId)
        {
            var deny = GuardPrayManager();
            if (deny != null) return deny;

            SetUrlBag();
            var item = _mgr.GetById(PrayId);
            if (item == null) return RedirectToAction(nameof(List));
            ViewBag.VerifyReady = _mgr.IsVerifyReady();   // 검증된 기도문이면 "저장하면 검증이 풀려요" 안내
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Save(HealingAndRestorationPray model)
        {
            // 수정 = 기도 관리자 / 새 등록 = 원래 권한(슈퍼 유저) 그대로
            var deny = model != null && model.PrayId > 0 ? GuardPrayManager() : await GuardSuperUserAsync();
            if (deny != null) return deny;

            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.PrayText))
            {
                TempData["Error"] = "기도 제목과 기도문은 반드시 입력해 주세요.";
                SetUrlBag();
                return View("Edit", model);
            }

            bool isEdit = model.PrayId > 0;

            if (isEdit) _mgr.Update(model);
            else        _mgr.Insert(model);

            TempData["Msg"] = isEdit ? "기도문을 수정했습니다." : "기도문을 등록했습니다.";

            // 수정은 고친 기도문을 바로 확인할 수 있게 본문 보기로 보냅니다.
            // ※ 신규 등록은 새 PrayId 를 알 수 없어(Insert 가 돌려주지 않습니다) 목록으로 갑니다.
            if (isEdit) return RedirectToAction(nameof(Detail), new { PrayId = model.PrayId });

            return RedirectToAction(nameof(List));
        }

        // ══════════════════════════════════════════
        //  삭제 — 슈퍼 유저(SuperAdmin · basic) 만
        // ══════════════════════════════════════════
        [Authorize]
        public async Task<IActionResult> Delete(int PrayId)
        {
            var deny = await GuardSuperUserAsync();
            if (deny != null) return deny;

            SetUrlBag();
            var item = _mgr.GetById(PrayId);
            if (item == null) return RedirectToAction(nameof(List));
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int PrayId)
        {
            var deny = await GuardSuperUserAsync();
            if (deny != null) return deny;

            _mgr.Delete(PrayId);
            TempData["Msg"] = "기도문을 삭제했습니다.";
            return RedirectToAction(nameof(List));
        }
    }
}

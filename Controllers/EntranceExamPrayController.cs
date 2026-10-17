using BulletinBoard.BusinessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Models.EntranceExamPrays;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    /// <summary>
    /// 수시·수능 준비생을 위한 기도문
    ///  - Index(오늘의 기도) / All(전체 목록) / Detail : 누구나
    ///  - List 관리 표시 / Edit / Verify · 클릭 카운트 제외 : 기도 관리자(로그인 아이디 Basic · SuperAdmin) — 2026-10-11
    ///  - Create / Delete : 원래대로 슈퍼 유저(SuperAdmin 역할 · basic 아이디)
    /// ※ HealingAndRestorationPrayController 와 같은 구조입니다.
    /// </summary>
    [LogActionFilter]
    [AllowAnonymous]
    public class EntranceExamPrayController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly ManageEntranceExamPray _mgr = new ManageEntranceExamPray();

        public EntranceExamPrayController(
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

        /// <summary>슈퍼 유저가 아니면 기도 목록으로 돌려보냅니다. (등록 · 삭제 — 원래 권한 그대로)</summary>
        private async Task<IActionResult> GuardSuperUserAsync()
        {
            if (await IsSuperUserAsync()) return null;
            return RedirectToAction(nameof(List));
        }

        /// <summary>
        /// 기도 관리자 = 로그인 아이디 "Basic" · "SuperAdmin" 두 계정 (2026-10-11, PrayManagerHelper)
        ///  → 읽음 수 안 올림 · 목록(숨긴 것 · 운영 수치) · 수정 · 검증 표시
        /// </summary>
        private bool IsPrayManager => PrayManagerHelper.IsPrayManager(User);

        private IActionResult GuardPrayManager()
            => IsPrayManager ? null : RedirectToAction(nameof(List));

        private void SetUrlBag()
        {
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain         = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName     = urlInfo.ActionName;
            ViewBag.FullURL        = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";
        }

        // ══════════════════════════════════════════
        //  오늘의 기도문 — 누구나 (매일 라운드로빈)
        // ══════════════════════════════════════════
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            SetUrlBag();

            // 오늘의 기도문은 순환에 들어가는 것만 대상으로 합니다.
            int total = _mgr.GetRotationCount();
            var today = _mgr.GetTodayPray();

            bool isSuper = IsPrayManager;   // 기도 관리자(Basic · SuperAdmin 계정)

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

            // ⚠️ 치유와 회복 기도문과 세션 키가 겹치지 않도록 접두어를 다르게 둡니다.
            // 하루에 한 번 — 날짜(KST)를 키에 넣어 다음 날에는 다시 누를 수 있다
            var sessionKey = $"likedExamPray:{prayId}:{DateHelper.GetKoreaNow():yyyyMMdd}";

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
        //  기도문 상세 — 누구나 (슈퍼 유저 외 클릭 집계)
        // ══════════════════════════════════════════
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int PrayId)
        {
            SetUrlBag();

            bool isSuper = IsPrayManager;   // 기도 관리자(Basic · SuperAdmin 계정)

            if (!isSuper && PrayId > 0)
            {
                try { _mgr.IncreaseClick(PrayId); } catch { }
            }

            var item = _mgr.GetById(PrayId);
            if (item == null) return RedirectToAction(nameof(List));

            // 오늘의 기도문 화면과 같은 구성으로 보여 주기 위해 순번 정보도 넘깁니다.
            // 순환에서 빠진 기도문(RotateYN='N')은 GetRotationNo 가 0 을 돌려주므로
            // 화면에서 순번 대신 "특정일 기도" 표시가 나갑니다.
            ViewBag.IsSuperAdmin = isSuper;
            ViewBag.TotalCount   = _mgr.GetRotationCount();
            ViewBag.RotationNo   = _mgr.GetRotationNo(PrayId);
            ViewBag.IsTodayPray  = _mgr.GetTodayPray()?.PrayId == PrayId;
            ViewBag.TodayKst     = DateHelper.GetKoreaNow();

            return View(item);
        }

        // ══════════════════════════════════════════
        //  검증 완료 — 슈퍼 유저(SuperAdmin · basic) 만
        //
        //  내용은 그대로 두고 수정 시각만 지금으로 올립니다. 그러면 이 기도문이
        //  "최근에 본 것"이 되어 목록 맨 앞으로 가고, 이어서 가장 오래
        //  손대지 않은 기도문으로 넘어갑니다. 40편을 빠짐없이 훑기 위한 흐름입니다.
        // ══════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Verify(int PrayId)
        {
            var deny = GuardPrayManager();
            if (deny != null) return deny;

            if (PrayId > 0)
            {
                try { _mgr.TouchUpdatedAt(PrayId); }
                catch
                {
                    TempData["Error"] = "검증 표시를 저장하지 못했습니다. 잠시 후 다시 눌러 주세요.";
                    return RedirectToAction(nameof(Detail), new { PrayId });
                }
            }

            // ⚠️ 방금 표시한 것이 가장 최근이 되었으므로, 이 조회에서 자연히 빠집니다.
            int next = _mgr.GetOldestUpdatedPrayId();

            if (next <= 0 || next == PrayId)
            {
                // 기도문이 한 편뿐이거나 조회에 실패한 경우
                TempData["Msg"] = "검증 완료로 표시했습니다.";
                return RedirectToAction(nameof(Detail), new { PrayId });
            }

            var item = _mgr.GetById(next);
            TempData["Msg"] = item != null
                ? $"검증 완료로 표시했습니다. 가장 오래된 기도문 “{item.Title}”으로 넘어갑니다."
                : "검증 완료로 표시했습니다.";

            return RedirectToAction(nameof(Detail), new { PrayId = next });
        }

        // ══════════════════════════════════════════
        //  기도 목록 — 누구나 (기도홈 카드가 가리키는 화면)
        //
        //  읽는 화면과 관리 화면을 하나로 합쳤습니다.
        //   · 일반 사용자 : 사용중인 기도문만, 읽기 링크만
        //   · 슈퍼 유저   : 숨긴 것까지 전부, 수정·삭제 버튼과 운영 수치까지
        //  권한 판정은 한 곳(isSuper)에서 하고 화면이 그 값으로 갈라집니다.
        // ══════════════════════════════════════════
        [AllowAnonymous]
        public async Task<IActionResult> List()
        {
            SetUrlBag();

            bool isSuper = IsPrayManager;   // 기도 관리자(Basic · SuperAdmin 계정)

            // 숨긴 기도문(UseYN='N')은 관리자만 봅니다.
            var list = isSuper ? _mgr.GetList(includeUnused: true) : _mgr.GetPublicList();

            // ⚠️ 두 숫자가 다릅니다.
            //    TotalCount    — 목록에 보이는 전체 (그날에 읽는 기도문 포함)
            //    RotationCount — 매일 돌아가는 것만 (RotateYN='Y')
            int rotation = _mgr.GetRotationCount();

            ViewBag.IsSuperAdmin  = isSuper;
            ViewBag.TotalCount    = _mgr.GetActiveCount();
            ViewBag.RotationCount = rotation;
            ViewBag.RotationNo    = _mgr.GetTodayRotationNo(rotation);
            ViewBag.TodayPrayId   = _mgr.GetTodayPray()?.PrayId ?? 0;
            ViewBag.TodayKst      = DateHelper.GetKoreaNow();

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

            return View("Edit", new EntranceExamPray { SortOrder = nextOrder, UseYN = "Y" });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int PrayId)
        {
            var deny = GuardPrayManager();
            if (deny != null) return deny;

            SetUrlBag();
            var item = _mgr.GetById(PrayId);
            if (item == null) return RedirectToAction(nameof(List));
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Save(EntranceExamPray model)
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
            // ※ 신규 등록은 새 PrayId 를 알 수 없어 목록으로 갑니다.
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

using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models.Prayer365;
using BulletinBoard.Services.Prayer365;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Cfg = BulletinBoard.Models.Prayer365.Prayer365Config;

namespace BulletinBoard.Controllers;

/// <summary>
///  자녀를 위한 365일 축복 기도문 (2026-09-27 확정 시안).
///  · /Prayer365           — 오늘(KST) 날짜의 기도문, 누구나.  ?md=0303 으로 다른 날
///  · /Prayer365/Admin     — 365일 달력판 (관리자)
///  · /Prayer365/Edit      — 확인 · 오타 수정 · 검수 완료 (관리자)
///  관리자 = 로그인 아이디 "Basic" · "SuperAdmin" 두 계정 (2026-10-11 — 역할은 보지 않음, Admin 은 일반 사용자).
///  입력은 B 방식 — 대화창에 올린 텍스트를 Claude 가 교정해 표에 넣고, 여기서는 확인 · 고치기만.
/// </summary>
public class Prayer365Controller : Controller
{
    private readonly IPrayer365Service _svc;
    public Prayer365Controller(IPrayer365Service svc) => _svc = svc;

    // 2026-10-11: 기도 관리자 = 로그인 아이디 "Basic" · "SuperAdmin" 두 계정 (역할은 보지 않는다 — PrayManagerHelper)
    private bool IsStaff => PrayManagerHelper.IsPrayManager(User);

    // ── 오늘의 기도 ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(string? md, string? view)
    {
        var today = DateHelper.GetKoreaToday();
        string todayMd = Cfg.Md(today);
        if (!Cfg.TryParse(md, out _, out _)) md = todayMd;

        // 홈 "자녀를 위한 365" 버튼은 ?view=user 로 온다 — 일반 보기 : 관리자여도 관리 버튼을 숨긴다
        bool userView = string.Equals(view, "user", StringComparison.OrdinalIgnoreCase);
        if (userView) md = todayMd;   // 일반 보기는 오늘 기도문만 — 주소에 ?md= 를 붙여도 다른 날로 가지 않는다

        // 허락 전(StaffOnly)에는 로그인한 회원만 — 로그인하지 않은 사람에게는 기도문을 읽지도 않고 "준비 중" 만 보여 준다 (2026-10-11)
        bool closed = Cfg.StaffOnly && !IsStaff && User.Identity?.IsAuthenticated != true;
        if (closed)
        {
            // 로그인하지 않았으면 로그인 화면으로 — 로그인 후 이 기도문으로 돌아온다
            var back = Uri.EscapeDataString(Request.Path + Request.QueryString);
            return Redirect("/Identity/Account/Login?ReturnUrl=" + back);
        }
        var vm = new Prayer365PageVM
        {
            MonthDay = md!,
            IsToday  = md == todayMd,
            Today    = today,
            Item     = closed ? null : await _svc.GetAsync(md!),
            IsStaff  = IsStaff && !userView,   // 화면의 관리 기능 표시 여부
            IsClosed = closed,
            UserView = userView,
            IsAdminUser = IsStaff,
            SignedIn = User.Identity?.IsAuthenticated == true,
        };

        // 오늘 기도 했어요 — 로그인한 사람은 개인별 기록으로 이미 눌렀는지 본다 (다른 기기에서 눌러도 같게)
        if (vm.Item != null && User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(User.Identity.Name))
        {
            try
            {
                vm.PrayedToday = await _svc.HasPrayedAsync(md!, User.Identity.Name, today);
                // "나의 기도 기록" 통계 — 오늘(KST) 기준
                vm.Stats = Prayer365PrayStats.Compute(await _svc.GetPrayDatesAsync(User.Identity.Name), today);
            }
            catch { /* 기록 조회 실패로 기도문을 막지 않는다 */ }
        }

        // 관리자 참여자 요약 — SuperAdmin 역할 · Basic 아이디에게만 (2026-10-18 홈 카드에서 옮김)
        if (PrayManagerHelper.CanSeePrayParticipants(User))
        {
            try { vm.Participants = Prayer365ParticipantsVM.Build(await _svc.GetAllPrayDaysAsync(), today, "recent"); }
            catch { vm.Participants = null; }   // 기록 표 문제면 그 줄만 빼고 기도문은 그대로
        }

        // 조회 수 — 기도 관리자("Basic" · "SuperAdmin" 계정)는 관리자 보기든 일반 보기든 세지 않는다 (2026-10-11)
        bool noCount = IsStaff;
        if (vm.Item != null && !(IsStaff && !userView) && !noCount)
        {
            try { vm.Item.ViewCount = await _svc.AddViewAsync(md!); }
            catch { /* 조회 수 실패로 기도문을 막지 않는다 */ }
        }
        return View(vm);
    }

    // ── 오늘 기도 했어요 — 이 기도문을 보는 누구나 (화면에서 ajax) ─────
    //  같은 사람이 여러 번 누르는 것은 브라우저(localStorage)에서 막고,
    //  서버는 같은 세션에서 같은 기도문에 하루 한 번만 반영한다 (2026-10-17)
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Pray(string? md)
    {
        if (!Cfg.TryParse(md, out _, out _))
            return Json(new { ok = false, msg = "잘못된 요청입니다." });

        // 허락 전(StaffOnly)에는 로그인한 회원 · 관리자만 — 기도문을 볼 수 있는 사람만 누른다
        if (Cfg.StaffOnly && !IsStaff && User.Identity?.IsAuthenticated != true)
            return Json(new { ok = false, msg = "로그인한 뒤에 눌러 주세요." });

        var today = DateHelper.GetKoreaToday();
        string? userName = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null;

        // 로그인 안 한 사람은 개인 기록이 없으니 세션으로 하루 한 번을 막는다
        // (로그인한 사람은 개인별 기록표 Prayer365PrayLog 가 하루 한 번을 막는다 — 2026-10-17)
        var sessionKey = $"prayed365:{md}:{today:yyyyMMdd}";
        if (userName == null && HttpContext.Session.GetString(sessionKey) == "1")
        {
            int cur = 0;
            try { cur = await _svc.GetPrayCountAsync(md!); } catch { }
            return Json(new { ok = true, already = true, count = cur });
        }

        (bool added, int count) res;
        try
        {
            string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            res = await _svc.AddPrayAsync(md!, userName, userId, today);
        }
        catch { return Json(new { ok = false, msg = "잠시 후 다시 눌러 주세요." }); }

        if (userName == null) HttpContext.Session.SetString(sessionKey, "1");
        return Json(new { ok = true, already = !res.added, count = res.count });
    }

    // ── 관리 : 기도 참여자 (365 기도 화면 맨 아래 관리자 줄) — 기도 관리자 + SuperAdmin 역할 · Basic 아이디 ─────
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Participants(string? sort)
    {
        if (!IsStaff && !PrayManagerHelper.CanSeePrayParticipants(User)) return RedirectToAction(nameof(Index));
        var rows = await _svc.GetAllPrayDaysAsync();
        return View(Prayer365ParticipantsVM.Build(rows, DateHelper.GetKoreaToday(), sort ?? "recent"));
    }

    // ── 관리 : 365일 달력판 ─────────────────────────────────
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Admin(string? f, string? q)
    {
        if (!IsStaff) return RedirectToAction(nameof(Index));
        var vm = new Prayer365AdminVM
        {
            Map     = await _svc.GetStatusMapAsync(),
            Filter  = f is "empty" or "check" or "done" ? f : "all",
            Q       = q,
            TodayMd = Cfg.Md(DateHelper.GetKoreaToday()),
        };
        if (!string.IsNullOrWhiteSpace(q)) vm.Found = await _svc.SearchAsync(q);
        return View(vm);
    }

    // ── 관리 : 확인 · 오타 수정 ─────────────────────────────
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(string? md)
    {
        if (!IsStaff) return RedirectToAction(nameof(Index));
        if (!Cfg.TryParse(md, out _, out _)) md = Cfg.Md(DateHelper.GetKoreaToday());
        return View(new Prayer365EditVM
        {
            MonthDay = md!,
            Item     = await _svc.GetAsync(md!),
            Toast    = TempData["P365Toast"] as string,
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string md, string? title, string? verseRef, string? verseText, string? body, bool isReviewed)
    {
        if (!IsStaff) return Forbid();
        if (!Cfg.TryParse(md, out _, out _)) return BadRequest();

        // 고친 곳 기록(Claude 교정)은 그대로 둔다 — 관리자가 고친 것은 본문에 바로 반영
        var old = await _svc.GetAsync(md);
        var item = new Prayer365Item
        {
            MonthDay   = md,
            Title      = Trim(title, 200),
            VerseRef   = Trim(verseRef, 100),
            VerseText  = Trim(verseText, 1000),
            Body       = (body ?? "").Replace("\r\n", "\n").Trim(),
            Fixes      = old?.Fixes ?? new(),
            IsReviewed = isReviewed,
        };
        if (string.IsNullOrWhiteSpace(item.Body))
        {
            TempData["P365Toast"] = "본문이 비어 있어 저장하지 않았어요.";
            return RedirectToAction(nameof(Edit), new { md });
        }
        await _svc.SaveAsync(item, User.Identity?.Name);
        TempData["P365Toast"] = $"{Cfg.Label(md)} 기도문을 저장했어요.";
        return RedirectToAction(nameof(Edit), new { md });
    }

    private static string Trim(string? s, int max)
    {
        s = (s ?? "").Trim();
        return s.Length > max ? s[..max] : s;
    }
}

using BulletinBoard.Enums;
using BulletinBoard.Models.FamilyPhoto;
using BulletinBoard.Services.FamilyPhoto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Cfg = BulletinBoard.Models.FamilyPhoto.FamilyPhotoContestConfig;

namespace BulletinBoard.Controllers;

/// <summary>
///  추석맞이 우리 가족 사진 콘테스트.
///  로그인 없이 쓴다 — 기기는 쿠키(fpc_dev), 내 가족은 가족명 + PIN 확인 뒤 보호된 쿠키(fpc_entry).
///  관리자 화면만 Admin · SuperAdmin.
/// </summary>
public class FamilyPhotoController : Controller
{
    private readonly IFamilyPhotoService _svc;
    private readonly IDataProtector _protector;

    public FamilyPhotoController(IFamilyPhotoService svc, IDataProtectionProvider dp)
    {
        _svc       = svc;
        _protector = dp.CreateProtector("FamilyPhoto.Entry");
    }

    // ── 쿠키 ─────────────────────────────────────────────────
    private string DeviceKey()
    {
        var v = Request.Cookies[Cfg.DeviceCookie];
        if (string.IsNullOrWhiteSpace(v) || v.Length > 64)
        {
            v = Guid.NewGuid().ToString("N");
            Response.Cookies.Append(Cfg.DeviceCookie, v, new CookieOptions
            {
                HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(120), Path = "/",
            });
        }
        return v;
    }

    private int? MyEntryId()
    {
        var v = Request.Cookies[Cfg.EntryCookie];
        if (string.IsNullOrWhiteSpace(v)) return null;
        try { return int.TryParse(_protector.Unprotect(v), out var id) ? id : null; }
        catch { return null; }
    }

    private void SetMyEntry(int entryId)
        => Response.Cookies.Append(Cfg.EntryCookie, _protector.Protect(entryId.ToString()), new CookieOptions
        {
            HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/",
        });

    private void ClearMyEntry() => Response.Cookies.Delete(Cfg.EntryCookie, new CookieOptions { Path = "/" });

    private string? Ip()
    {
        var fwd = Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(fwd)) return fwd.Split(',')[0].Trim();
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private bool IsStaff => User?.Identity?.IsAuthenticated == true
                            && (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.SuperAdmin)));

    // ══════════════════════════════════════════════════════════
    //  안내(Welcome) — 홈 배너를 누르면 오는 곳.  일정 · 참여 방법 · 갤러리 가기
    // ══════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Welcome()
    {
        await _svc.EnsureTablesAsync();
        var vm = await _svc.GetBannerAsync();
        ViewBag.IsStaff = IsStaff;
        // 카톡 · 메신저 공유 미리보기(Open Graph)는 절대 주소가 필요하다
        ViewBag.Domain  = $"{Request.Scheme}://{Request.Host}";
        ViewBag.FullURL = $"{Request.Scheme}://{Request.Host}/FamilyPhoto/Welcome";
        return View(vm);
    }

    // ══════════════════════════════════════════════════════════
    //  갤러리
    // ══════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await _svc.EnsureTablesAsync();
        var vm = await _svc.GetGalleryAsync(DeviceKey(), MyEntryId().HasValue, IsStaff);   // 관리자는 투표 시작 전에도 좋아요
        vm.Toast = TempData["FpToast"] as string;
        ViewBag.IsStaff = IsStaff;
        return View(vm);
    }

    /// <summary>좋아요 켜기/끄기 — fetch 로 부른다</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(int photoId)
    {
        await _svc.EnsureTablesAsync();
        var (ok, msg, liked) = await _svc.ToggleLikeAsync(photoId, DeviceKey(), Ip(), IsStaff);
        return Json(new { ok, msg, liked });
    }

    /// <summary>AI 리뷰 보드 스티커 붙이기 · 떼기 · 바꾸기 — fetch 로 부른다</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sticker(int photoId, int kind)
    {
        var (ok, msg, mine, counts) = await _svc.SetStickerAsync(photoId, kind, DeviceKey(), Ip());
        return Json(new { ok, msg, mine, counts });
    }

    // ══════════════════════════════════════════════════════════
    //  참가 · 내 가족
    // ══════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Enter(bool fresh = false)
    {
        await _svc.EnsureTablesAsync();
        // 이미 확인된 가족이 있으면 바로 사진 화면으로
        if (!fresh && MyEntryId() is int id && await _svc.GetManageAsync(id, DeviceKey()) != null)
            return RedirectToAction(nameof(Manage));

        ViewBag.Error = TempData["FpError"] as string;
        ViewBag.CanUpload = Cfg.CanUploadAt(BulletinBoard.Helpers.DateHelper.GetKoreaNow());
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enter(string familyName, string pin)
    {
        await _svc.EnsureTablesAsync();
        var (ok, msg, entryId) = await _svc.EnterAsync(familyName, pin, Ip());
        if (!ok)
        {
            TempData["FpError"] = msg;
            return RedirectToAction(nameof(Enter), new { fresh = true });
        }
        SetMyEntry(entryId);
        return RedirectToAction(nameof(Manage));
    }

    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        await _svc.EnsureTablesAsync();
        if (MyEntryId() is not int id) return RedirectToAction(nameof(Enter), new { fresh = true });
        var vm = await _svc.GetManageAsync(id, DeviceKey());
        if (vm == null) { ClearMyEntry(); return RedirectToAction(nameof(Enter), new { fresh = true }); }
        vm.Toast = TempData["FpToast"] as string;
        vm.Error = TempData["FpError"] as string;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(12 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 12 * 1024 * 1024)]
    public async Task<IActionResult> Upload(int slot, IFormFile? file, string? caption)
    {
        bool ajax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        if (MyEntryId() is not int id)
            return ajax ? Json(new { ok = false, msg = "다시 들어와 주세요.", redirect = "/FamilyPhoto/Enter?fresh=true" })
                        : RedirectToAction(nameof(Enter), new { fresh = true });
        if (file == null)
        {
            if (ajax) return Json(new { ok = false, msg = "사진을 골라 주세요." });
            TempData["FpError"] = "사진을 골라 주세요.";
            return RedirectToAction(nameof(Manage));
        }
        var (ok, msg) = await _svc.UploadAsync(id, slot, file, caption);
        if (ajax)
        {
            // 진행 카드가 있는 화면(XHR)에서는 JSON 으로 답하고, 화면이 알아서 다시 불러온다
            if (ok) TempData["FpToast"] = msg;
            return Json(new { ok, msg });
        }
        TempData[ok ? "FpToast" : "FpError"] = msg;
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Caption(int slot, string? caption)
    {
        if (MyEntryId() is not int id) return RedirectToAction(nameof(Enter), new { fresh = true });
        var (ok, msg) = await _svc.SetCaptionAsync(id, slot, caption);
        TempData[ok ? "FpToast" : "FpError"] = msg;
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int slot)
    {
        if (MyEntryId() is not int id) return RedirectToAction(nameof(Enter), new { fresh = true });
        var (ok, msg) = await _svc.RemovePhotoAsync(id, slot);
        TempData[ok ? "FpToast" : "FpError"] = msg;
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEntry()
    {
        if (MyEntryId() is not int id) return RedirectToAction(nameof(Enter), new { fresh = true });
        var (ok, msg) = await _svc.DeleteEntryAsync(id);
        if (ok) ClearMyEntry();
        TempData[ok ? "FpToast" : "FpError"] = msg;
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Manage));
    }

    /// <summary>다른 가족으로 들어가기 (쿠키만 지운다)</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Leave()
    {
        ClearMyEntry();
        return RedirectToAction(nameof(Enter), new { fresh = true });
    }

    // ══════════════════════════════════════════════════════════
    //  결과
    // ══════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Result(bool preview = false)
    {
        await _svc.EnsureTablesAsync();
        // 2026-10-06 — 발표 뒤엔 결과 보기 · AI 심사평을 "AI 리뷰 보드"(Judge) 하나로 합쳤다.  발표 전 화면은 그대로
        if (Cfg.IsAnnouncedAt(BulletinBoard.Helpers.DateHelper.GetKoreaNow(), await _svc.IsPublishedAsync()))   // 홈 카드와 같은 기준 (발표일이 지났거나 발표 스위치)
            return RedirectToAction(nameof(Judge));
        var vm = await _svc.GetResultAsync(preview && IsStaff);
        ViewBag.IsStaff = IsStaff;
        ViewBag.Banner  = await _svc.GetBannerAsync();   // 발표 전 화면 — 가족 수 · 사진 수 · 흐린 썸네일 (1분 캐시)
        return View(vm);
    }

    // ══════════════════════════════════════════════════════════
    //  관리자
    // ══════════════════════════════════════════════════════════
    [HttpGet]
    [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin))]
    public async Task<IActionResult> Admin()
    {
        await _svc.EnsureTablesAsync();
        var vm = await _svc.GetAdminAsync();
        vm.Toast = TempData["FpToast"] as string;
        return View(vm);
    }

    /// <summary>AI 리뷰 보드 (2026-10-06) — 등수 없이 사진마다 AI 칭찬 · 별명 배지 · 좋아요 수 · 스티커.
    ///  예전 "결과 보기"(Result)와 "AI 심사평"을 합쳤다.  주소(/FamilyPhoto/Judge)는 카톡 공유 때문에 그대로</summary>
    [HttpGet]
    public async Task<IActionResult> Judge()
    {
        await _svc.EnsureTablesAsync();
        var vm = await _svc.GetAdminAsync();   // 숨긴 사진까지 — 화면에서 관리자가 아니면 숨긴 사진은 뺀다
        ViewBag.Stickers = await _svc.GetStickersAsync(DeviceKey());
        // 카카오톡 공유 미리보기(Open Graph)는 절대 주소가 필요하다
        ViewBag.Domain  = $"{Request.Scheme}://{Request.Host}";
        ViewBag.FullURL = $"{Request.Scheme}://{Request.Host}/FamilyPhoto/Judge";
        ViewBag.IsStaff = IsStaff;   // 관리자 참고 메모 · 관리 링크
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin))]
    public async Task<IActionResult> AdminEntryHidden(int entryId, bool hidden)
    {
        var (_, msg) = await _svc.SetEntryHiddenAsync(entryId, hidden, User.Identity?.Name ?? "");
        TempData["FpToast"] = msg;
        return RedirectToAction(nameof(Admin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin))]
    public async Task<IActionResult> AdminPhotoHidden(int photoId, bool hidden)
    {
        var (_, msg) = await _svc.SetPhotoHiddenAsync(photoId, hidden, User.Identity?.Name ?? "");
        TempData["FpToast"] = msg;
        return RedirectToAction(nameof(Admin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin))]
    public async Task<IActionResult> AdminResetPin(int entryId)
    {
        var (_, msg, _) = await _svc.ResetPinAsync(entryId, User.Identity?.Name ?? "");
        TempData["FpToast"] = msg;
        return RedirectToAction(nameof(Admin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin))]
    public async Task<IActionResult> AdminPublish(bool published)
    {
        var (_, msg) = await _svc.SetPublishedAsync(published, User.Identity?.Name ?? "");
        TempData["FpToast"] = msg;
        return RedirectToAction(nameof(Admin));
    }

    /// <summary>테스트 자료 전체 초기화 — 되돌릴 수 없어서 SuperAdmin 만, 확인 문구 "초기화" 를 받아야 한다</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = nameof(Roles.SuperAdmin))]
    public async Task<IActionResult> AdminResetAll(string confirm)
    {
        if (!string.Equals(confirm?.Trim(), "초기화", StringComparison.Ordinal))
        {
            TempData["FpToast"] = "확인 칸에 '초기화' 라고 적어야 지워져요.";
            return RedirectToAction(nameof(Admin));
        }
        var (_, msg) = await _svc.ResetAllAsync(User.Identity?.Name ?? "");
        TempData["FpToast"] = msg;
        return RedirectToAction(nameof(Admin));
    }
}

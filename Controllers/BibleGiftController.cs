using BibleReadingThrough.Services.BibleGift;
using BulletinBoard.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibleReadingThrough.Controllers;

/// <summary>도전! 성경통독 완독 선물 — 고르기 / 관리자 / 이력</summary>
[Authorize]
public class BibleGiftController : Controller
{
    private readonly IBibleGiftService _svc;
    public BibleGiftController(IBibleGiftService svc) => _svc = svc;

    // 관리 화면을 볼 수 있는 계정 (성경통독 허브 통계와 같은 기준)
    private static readonly HashSet<string> AdminUsers =
        new(StringComparer.OrdinalIgnoreCase) { "Basic", "SuperAdmin", "Admin" };

    private bool IsAdmin => AdminUsers.Contains(User.Identity?.Name ?? "");
    private string MyId  => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private string MyName => User.Identity?.Name ?? "";

    // ── 회차별 선물 목록 (구경 전용) ───────────────────────
    //  선물을 고르는 곳은 통합 선물함(/StudentPointDashboard4/MyGiftBox3) 한 곳으로 모았다.
    //  이 화면은 비로그인·비대상자에게 "통독하면 이런 선물을 받아요"를 보여 주는 용도로 남긴다.
    //  view=true 는 통합 선물함의 '고른 선물 보기'로 들어온 경우라 되돌려 보내지 않는다.
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Select(int round, bool view = false)
    {
        BibleReadingThrough.Models.BibleGift.BibleGiftSelectVM? vm = null;
        if (User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(MyId))
            vm = await _svc.GetSelectPageAsync(MyId, round);

        // 고를 자격이 있는 사람은 통합 선물함으로 보낸다
        if (vm != null && !view)
            return Redirect("/StudentPointDashboard4/MyGiftBox3");

        vm ??= await _svc.GetBrowsePageAsync(round);
        if (vm == null) return RedirectToAction("Index", "Home");

        // 화면에는 아이디(UserName)가 아니라 이름(예: 강세은)을 보여 준다
        vm.UserFullName = vm.IsReadOnly ? "" : (await _svc.GetFullNameAsync(MyName) ?? MyName);
        ViewData["Title"] = $"{round}독 완독 선물";
        return View(vm);
    }

    // returnTo=hub 로 들어오면 통합 선물함으로 돌려보낸다 (회차별 전용 화면은 그대로 둔다)
    private IActionResult Back(int round, string? returnTo)
        => string.Equals(returnTo, "hub", StringComparison.Ordinal)
           ? Redirect("/StudentPointDashboard4/MyGiftBox3")
           : RedirectToAction(nameof(Select), new { round });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pick(int round, int productId, string? returnTo = null)
    {
        try
        {
            var (ok, msg) = await _svc.SelectAsync(MyId, MyName, round, productId);
            TempData[ok ? "GiftOk" : "GiftErr"] = msg;
        }
        catch (Exception ex)
        {
            TempData["GiftErr"] = "선물을 저장하지 못했어요. 다시 해 주세요. (" + ex.Message + ")";
        }
        return Back(round, returnTo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int round, string? returnTo = null)
    {
        try
        {
            var (ok, msg) = await _svc.CancelAsync(MyId, round);
            TempData[ok ? "GiftOk" : "GiftErr"] = msg;
        }
        catch (Exception ex)
        {
            TempData["GiftErr"] = "취소하지 못했어요. 다시 해 주세요. (" + ex.Message + ")";
        }
        return Back(round, returnTo);
    }

    // ── 관리자 ────────────────────────────────────────────
    //  선물 관리는 통합 화면 한 곳(/StudentPointDashboard4/AdminPurchaseList)으로 모았다.
    //  예전 주소로 들어와도 그 화면의 '성경통독' 칸으로 보내 준다.
    [HttpGet]
    public IActionResult Admin(string tab = "picked")
        => RedirectToAction("AdminPurchaseList", "StudentPointDashboard4", new { kind = "Bible" });

    /// <summary>강제 삭제 — 그 사람의 그 회차 선물 기록과 이력을 모두 지운다</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceDelete(string userId, int round, string tab = "all")
    {
        // 되돌릴 수 없는 작업이라 SuperAdmin 만
        if (!User.IsInRole("SuperAdmin")) return Forbid();
        try
        {
            var (ok, msg) = await _svc.ForceDeleteAsync(userId, round, MyName);
            TempData["GiftToast"] = msg;
        }
        catch (Exception ex)
        {
            TempData["GiftToast"] = "지우지 못했어요. (" + ex.Message + ")";
        }
        return RedirectToAction("AdminPurchaseList", "StudentPointDashboard4", new { kind = "Bible" });
    }
}

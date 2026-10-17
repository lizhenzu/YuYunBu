using BulletinBoard.Common;
using BulletinBoard.Helpers;
using BulletinBoard.Models.DailyMission2;
using BulletinBoard.Services.DailyMission2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [Authorize]
    public class DailyMission2AwardController : Controller
    {
        private readonly IAwardDashboardService _service;

        public DailyMission2AwardController(IAwardDashboardService svc)
        {
            _service = svc;
        }

        private bool IsAdmin()
        {
            return User.Identity != null
                && User.Identity.IsAuthenticated
                && User.IsInRole("Admin");
        }

        // ============================================================
        // ✅ 새 기능: 단계형 선물 선택 (낮은 레벨부터 1개씩)
        // GET: /DailyMission2Award/PickGifts?year=2026&startLevel=100(optional)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> PickGifts(int? year, int? startLevel)
        {
            string userName = User?.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(userName))
                return RedirectToAction("Login", "Account");

            int cYear = year ?? DateHelper.GetKoreaNow().Year;

            ViewBag.IsAdmin = IsAdmin();

            var (hasPending, awardLevel, amountOfMoney) =
                await _service.GetNextPendingAwardAsync(userName, cYear, startLevel);

            // 더 이상 미선택 레벨이 없으면 완료 화면
            if (!hasPending)
                return RedirectToAction(nameof(PickGiftsDone), new { year = cYear });

            var vm = new AwardPickGiftsVM
            {
                cYear = cYear,
                AwardLevel = awardLevel,
                AmountOfMoney = amountOfMoney,
                IsAdmin = IsAdmin(),
                Items = await _service.GetItemsForAwardLevelAsync(awardLevel)
            };

            return View("~/Views/DailyMission2Award/PickGifts.cshtml", vm);
        }

        // ============================================================
        // ✅ 선택 즉시 저장 + 다음 레벨로 자동 이동
        // POST: /DailyMission2Award/PickGiftsSelect
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PickGiftsSelect(int cYear, int awardLevel, int productId)
        {
            string userName = User?.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(userName))
                return RedirectToAction("Login", "Account");

            bool ok = await _service.SaveGiftSelectionAsync(userName, cYear, awardLevel, productId);

            if (!ok)
            {
                TempData["DM2_OK_POPUP"] = "선물 선택 저장에 실패했어요.\n잠시 후 다시 시도해 주세요.";
                return RedirectToAction(nameof(PickGifts), new { year = cYear, startLevel = awardLevel });
            }

            // 다음 레벨로 자동 이동 (없으면 완료 화면)
            return RedirectToAction(nameof(PickGifts), new { year = cYear });
        }

        // ============================================================
        // ✅ 완료 전용 화면 + 레벨별 재선택 버튼(Delivered 잠금)
        // GET: /DailyMission2Award/PickGiftsDone?year=2026
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> PickGiftsDone(int? year)
        {
            string userName = User?.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(userName))
                return RedirectToAction("Login", "Account");

            int cYear = year ?? DateHelper.GetKoreaNow().Year;

            var vm = new AwardPickGiftsDoneVM
            {
                cYear = cYear,
                IsAdmin = IsAdmin(),
                Claims = await _service.GetMyAwardClaimsWithItemsAsync(userName, cYear)
            };

            return View("~/Views/DailyMission2Award/PickGiftsDone.cshtml", vm);
        }

        // ============================================================
        // ✅ 특정 레벨만 재선택(Delivered는 불가)
        // POST: /DailyMission2Award/ResetGiftSelection
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetGiftSelection(int cYear, int awardLevel)
        {
            string userName = User?.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(userName))
                return RedirectToAction("Login", "Account");

            bool ok = await _service.ResetGiftSelectionAsync(userName, cYear, awardLevel);
            if (!ok)
            {
                TempData["DM2_OK_POPUP"] = "이미 전달 완료(Delivered)이거나, 재선택할 수 없는 상태예요.";
                return RedirectToAction(nameof(PickGiftsDone), new { year = cYear });
            }

            // 방금 리셋한 레벨로 바로 이동
            return RedirectToAction(nameof(PickGifts), new { year = cYear, startLevel = awardLevel });
        }

        // ✅ 시상 대상자 대시보드 (Delivered 제외)
        [HttpGet]
        public async Task<IActionResult> Dashboard(int? year)
        {
            int cYear = year ?? DateHelper.GetKoreaNow().Year;

            var model = await _service.GetDashboardAsync(cYear);

            ViewBag.cYear = cYear;
            ViewBag.IsAdmin = IsAdmin();

            return View("~/Views/DailyMission2Award/Dashboard.cshtml", model);
        }

        // ✅ 관리자: 시상 완료 처리
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDelivered(string userName, int cYear, int awardLevel)
        {
            if (!IsAdmin())
                return Forbid();

            await _service.MarkDeliveredAsync(userName, cYear, awardLevel);
            return RedirectToAction(nameof(Dashboard), new { year = cYear });
        }

        // ✅ 전체 시상 히스토리
        [HttpGet]
        public async Task<IActionResult> Dashboard2(int? year)
        {
            int cYear = year ?? DateHelper.GetKoreaNow().Year;

            var model = await _service.GetDashboard2Async(cYear);

            ViewBag.cYear = cYear;
            ViewBag.IsAdmin = IsAdmin();

            return View("~/Views/DailyMission2Award/Dashboard2.cshtml", model);
        }

        // ✅ 내 시상 내역
        [HttpGet]
        public async Task<IActionResult> MyAwards()
        {
            string userName = User.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(userName))
                return RedirectToAction("Login", "Account");

            var model = await _service.GetMyAwardsAsync(userName);

            ViewBag.IsAdmin = IsAdmin();

            return View("~/Views/DailyMission2Award/MyAwards.cshtml", model);
        }
    }
}

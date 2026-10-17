using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Birthday;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [Authorize]
    [Route("Birthdays")]
    public class BirthdaysController : Controller
    {
        private readonly IBirthdayService _birthdayService;
        public BirthdaysController(IBirthdayService svc) => _birthdayService = svc;

        // /Birthdays
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var m = DateHelper.GetKoreaNow().Month;
            var vm = await _birthdayService.GetMonthAsync(m);
            return View("~/Views/Birthdays/BirthdayMonth.cshtml", vm);
        }
        [HttpGet("NoBirthday")]
        public async Task<IActionResult> NoBirthday()
        {
            var vm = await _birthdayService.GetNoBirthdayAsync();
            return View("~/Views/Birthdays/BirthdayNoBirthday.cshtml", vm);
        }
        // /Birthdays/Month?month=1
        [HttpGet("Month")]
        public async Task<IActionResult> Month([FromQuery] int month)
        {
            if (month < 1 || month > 12) month = DateHelper.GetKoreaNow().Month;
            var vm = await _birthdayService.GetMonthAsync(month);
            return View("~/Views/Birthdays/BirthdayMonth.cshtml", vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin))]
        [HttpPost("UpdateGiftState")]
        public async Task<IActionResult> UpdateGiftState(UpdateGiftStateVM vm)
        {
            bool hasUserId  = vm != null && !string.IsNullOrWhiteSpace(vm.UserId);
            bool hasPersonId = vm != null && vm.PersonId > 0;
            bool hasMemo    = vm != null && !string.IsNullOrWhiteSpace(vm.Memo);

            // ── 특수 케이스: UserId 없고 PersonId + Memo만 있으면 메모만 갱신
            if (!hasUserId && hasPersonId && hasMemo)
            {
                await _birthdayService.UpsertMemoAsync(vm!.PersonId, vm.Memo!.Trim());
                TempData["Toast"] = "✅ 메모를 저장했어요.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { ok = true });

                var referer0 = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrWhiteSpace(referer0))
                    return Redirect(referer0);

                return RedirectToAction("Month", new { month = DateHelper.GetKoreaNow().Month });
            }

            // ── 일반 케이스: UserId 필수
            if (!hasUserId)
            {
                TempData["Toast"] = "❌ 저장 실패: UserId가 비어 있어요.";
                return RedirectToAction("Month", new { month = DateHelper.GetKoreaNow().Month });
            }

            // 2) 선물 상태 UPDATE 수행 (INSERT 금지) - 기존 기록이 없으면 affected=0
            var affectedGift = await _birthdayService.UpdateGiftStateAsync(
                vm!.UserId,
                vm.IsActive,
                vm.IsSent
            );

            // 3) 문자(SMS) 발송 여부 업서트
            var affectedSms = await _birthdayService.UpdatePickSmsStateAsync(
                vm.UserId,
                vm.PickSmsSent
            );

            // 4) 메모 업서트: BirthdayGiftPickMeMo (PersonID 키)
            var affectedMemo = 0;
            if (hasPersonId)
            {
                affectedMemo = await _birthdayService.UpsertMemoAsync(
                    vm.PersonId,
                    hasMemo ? vm.Memo!.Trim() : null
                );
            }

            // 5) 토스트 메시지 구성
            if (affectedGift <= 0)
            {
                TempData["Toast"] = (affectedSms > 0 || affectedMemo > 0)
                    ? "✅ 문자 발송 여부 / 메모를 저장했어요. (선물 선택 기록은 아직 없어요)"
                    : "⚠️ 저장할 데이터가 없어요.";
            }
            else
            {
                TempData["Toast"] = "✅ 선물 상태를 저장했어요.";
            }

            // 6) 돌아가기: AJAX면 JSON, 아니면 Referer 리디렉트
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { ok = true });

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer))
                return Redirect(referer);

            return RedirectToAction("Month", new { month = DateHelper.GetKoreaNow().Month });
        }


        /// <summary>
        /// ✅ 이번 달 생일자 목록을 조회하여 ViewBag에 담아주는 Action
        /// ViewBag.ThisMonthBirthdays : List<BirthdayCardVM>
        /// ViewBag.ThisMonth : int (1~12)
        /// </summary>
        [HttpGet("ThisMonth")]
        public async Task<IActionResult> ThisMonth()
        {
            // ✅ KST 기준 현재 월 (프로젝트에 DateHelper가 있으면 그걸 사용)
            var nowKst = DateHelper.GetKoreaToday();
            int month = nowKst.Month;

            // ✅ 기존 서비스 재사용
            BirthdayListPageVM vm = await _birthdayService.GetMonthAsync(month);

            // ✅ ViewBag에 담기 (월 화면은 Items)
            ViewBag.ThisMonth = month;
            ViewBag.ThisMonthBirthdays = vm?.Items ?? new List<BirthdayCardVM>();

            // 필요에 따라:
            // 1) 별도 화면이 있으면 View() 유지
            // 2) 다른 화면에서 호출만 할 거면 EmptyResult/PartialView로 바꿔도 됨
            return View();
        }

        // /Birthdays/Year
        [HttpGet("Year")]
        public async Task<IActionResult> Year()
        {
            var vm = await _birthdayService.GetYearAsync();
            return View("~/Views/Birthdays/BirthdayYear.cshtml", vm);
        }

        // /Birthdays/Edit/123  (현재는 “보기+수정 화면” 이름이 Edit)
        [HttpGet("Edit/{personId:int}")]
        public async Task<IActionResult> Edit(int personId)
        {
            var vm = await _birthdayService.GetEditAsync(personId);
            if (vm == null) return NotFound();
            return View("~/Views/Birthdays/BirthdayEdit.cshtml", vm);
        }
    }
}

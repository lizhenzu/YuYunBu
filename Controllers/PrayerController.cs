using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace BulletinBoard.Controllers
{
        [LogActionFilter]
    public class PrayerController : BaseController
    {
        private readonly IPrayerService _svc;
        public PrayerController(IPrayerService svc) { _svc = svc; }


        // 오늘(또는 쿼리스트링 day로 특정 일자) 한 개만 노출
        // /Prayer/Index 또는 /Prayer?day=5
        public async Task<IActionResult> Index(int? day)
        {
            PrayerArrow? model;
            if (day.HasValue)
            {
                var idx = ((Math.Max(1, day.Value) - 1) % 31) + 1; // 안전 순환
                model = await _svc.GetByDayIndexAsync(idx);
            }
            else
            {
                model = await _svc.GetTodayAsync();
            }
            return View(model);
        }


        // 관리자용: 단일 상세 + 수정 이동
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _svc.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PrayerArrow input)
        {
            if (!ModelState.IsValid) return View(input);
            await _svc.UpdateAsync(input);
            TempData["msg"] = "저장되었습니다.";
            return RedirectToAction(nameof(Index), new { day = input.DayIndex });
        }
    }
}



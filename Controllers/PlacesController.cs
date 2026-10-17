using BulletinBoard.BusinessLayer;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
        [LogActionFilter]
    public class PlacesController : Controller
    {
        private readonly IPlaceService _svc;
        public PlacesController(IPlaceService svc) => _svc = svc;

        // 메인: 카테고리 3개 큰 카드로 진입
        [HttpGet]
        public async Task<IActionResult> Main()
        {
            var counts = await _svc.GetCountsAsync();
            ViewBag.Counts = counts;
            return View();
        }

        // 목록: /Places/List?category=맛집&q=검색어
        [HttpGet]
        public async Task<IActionResult> List(string category = "맛집", string? q = null)
        {
            var items = await _svc.GetListAsync(category, q);
            ViewBag.Category = category;
            ViewBag.Q = q;
            return View(items);
        }

        [HttpGet]
        public IActionResult Create(string? category = "맛집")
        {
            return View(new PlaceItemInput { Category = CategoryOptions.Normalize(category) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlaceItemInput input)
        {
            if (!ModelState.IsValid) return View(input);
            var id = await _svc.CreateAsync(input);
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _svc.GetAsync(id);
            if (item is null) return NotFound();

            var vm = new PlaceItemInput
            {
                PlaceId = item.PlaceId,
                Category = item.Category,
                Name = item.Name,
                ExternalUrl = item.ExternalUrl,
                Address = item.Address,
                Tags = item.Tags,
                ThumbnailUrl = item.ThumbnailUrl,
                IsActive = item.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlaceItemInput input)
        {
            if (!ModelState.IsValid) return View(input);
            await _svc.UpdateAsync(input);
            return RedirectToAction(nameof(List), new { category = input.Category });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string category)
        {
            await _svc.DeleteAsync(id);
            return RedirectToAction(nameof(List), new { category });
        }
    }
}

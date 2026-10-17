using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System; // DateTime
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using YunYunBu2024.Services.Shorts;
    [LogActionFilter]
public class ShortsController : Controller
{
    private readonly IShortsRepository _repo;

    private readonly IAuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;
    public ShortsController(IAuthService authService, IShortsRepository repo,
        UserManager<ApplicationUser> userManager)
    {
        _repo = repo;
        _authService = authService;
        _userManager = userManager;
    }

    //public async Task<IActionResult> Index(string? q)
    //{
    //    var items = await _repo.GetListAsync(string.IsNullOrWhiteSpace(q) ? null : q.Trim());
    //    ViewBag.q = q; // 👉 검색어를 ViewBag에 담아 뷰로 전달
    //    return View(items);
    //}

    public async Task<IActionResult> Main()
    {
        var counts = await _repo.GetCountsByVideoKindAsync(onlyActive: true);
        ViewBag.VideoKindCounts = counts; // 뷰에서 사용

        return View();
    }
        public async Task<IActionResult> Index(string? q, VideoKindOption? param )
    {
        // 1) 목록 가져오기(검색어 있으면 필터), 정렬은 고정 기준으로
        var list = (await _repo.GetListAsync(string.IsNullOrWhiteSpace(q) ? null : q.Trim(), param))
                   .OrderBy(x => x.ShortId)   // 생성일/정렬 기준이 따로 있다면 그 필드로 교체
                   .ToList();

        ViewBag.q = q;

        // 2) 비어있으면 그대로 반환
        if (list.Count == 0) return View(list);

        if (await _authService.IsSignedIn(User) && await _authService.IsInRole(await _authService.GetSignedUser(User), nameof(Roles.SuperAdmin)))
        {
            return View(list);
        }
        else
        {
            // 3) 오늘 날짜(KST) 기준으로 라운드 로빈 인덱스 계산
            TimeZoneInfo kst;
            try { kst = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"); }
            catch { kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time"); } // Windows 호환
            var todayKst = TimeZoneInfo.ConvertTime(DateTime.UtcNow, kst).Date;

            // 기준일은 고정(변경 가능). 날짜 차이의 '일'을 이용해 순환
            var baseDate = new DateTime(2025, 8, 23);
            var dayNumber = (int)(todayKst - baseDate).TotalDays;

            var idx = ((dayNumber % list.Count) + list.Count) % list.Count; // 안전 모듈로
            var one = new List<ShortVideo> { list[idx] };

            // 4) 뷰에는 선택된 1개만 전달
            return View(one);
        }


    }

    public IActionResult Create() => View(new ShortVideoInput());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShortVideoInput input)
    {
        if (!ModelState.IsValid) return View(input);
        if (await _repo.ExistsByYoutubeIdAsync(input.YoutubeId))
        {
            ModelState.AddModelError(nameof(input.YoutubeId), "이미 존재하는 Shorts ID 입니다.");
            return View(input);
        }
        var id = await _repo.CreateAsync(input);
        TempData["msg"] = id > 0 ? "등록되었습니다." : "등록 실패";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _repo.FindAsync(id);
        if (entity == null) return NotFound();
        var vm = new ShortVideoInput
        {
            ShortId = entity.ShortId,
            YoutubeId = entity.YoutubeId,
            Title = entity.Title,
            IsActive = entity.IsActive
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ShortVideoInput input)
    {
        if (!ModelState.IsValid) return View(input);
        if (input.ShortId == null) return BadRequest();
        if (await _repo.ExistsByYoutubeIdAsync(input.YoutubeId, input.ShortId))
        {
            ModelState.AddModelError(nameof(input.YoutubeId), "다른 항목에서 이미 사용 중인 Shorts ID 입니다.");
            return View(input);
        }
        var ok = await _repo.UpdateAsync(input);
        TempData["msg"] = ok ? "수정되었습니다." : "수정 실패";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        TempData["msg"] = ok ? "삭제되었습니다." : "삭제 실패";
        return RedirectToAction(nameof(Index));
    }
}
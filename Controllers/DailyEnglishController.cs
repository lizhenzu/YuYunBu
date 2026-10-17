using Microsoft.AspNetCore.Mvc;
using StockBulletinBoard.Models;
using StockBulletinBoard.Services;

namespace StockBulletinBoard.Controllers
{
    public class DailyEnglishController : Controller
    {
        private readonly IDailyEnglishService _dailyEnglishService;

        // ✅ DI 주입 방식 (필수)
        public DailyEnglishController(IDailyEnglishService dailyEnglishService)
        {
            _dailyEnglishService = dailyEnglishService;
        }

        // 사용자 화면 — id 지정 시 해당 항목, 없으면 가장 적게 읽힌 항목
        public IActionResult Index(int? id)
        {
            var item = (id.HasValue && id.Value > 0)
                ? _dailyEnglishService.GetById(id.Value)
                : _dailyEnglishService.GetNext();

            if (item != null)
            {
                // ✅ 화면에 보이는 즉시 IsRead +1
                _dailyEnglishService.IncrementIsRead(item.Id);

                // 이전/다음 ID + 통계
                ViewBag.PrevId = _dailyEnglishService.GetPrevId(item.Id);
                ViewBag.NextId = _dailyEnglishService.GetNextId(item.Id);
                var (pos, total, unread) = _dailyEnglishService.GetPositionAndTotal(item.Id);
                ViewBag.CurrentPos  = pos;
                ViewBag.TotalCount  = total;
                ViewBag.UnreadCount = unread;
            }

            return View(item);
        }

        // 진도 API (상단 진도바용) — 미읽음 기준
        [HttpGet]
        public IActionResult Progress()
        {
            var all    = _dailyEnglishService.GetAll();
            var total  = all.Count;
            var unread = all.Count(x => x.IsRead == 0);
            return Json(new { unread, total });
        }

        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            _dailyEnglishService.MarkAsRead(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Reset()
        {
            _dailyEnglishService.Reset();
            return RedirectToAction("Index");
        }

        // 관리자 목록
        public IActionResult AdminList()
        {
            var list = _dailyEnglishService.GetAll();
            return View(list);
        }

        // 등록 화면
        public IActionResult Create()
        {
            return View(new DailyEnglish());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DailyEnglish item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            _dailyEnglishService.Insert(item);
            return RedirectToAction("AdminList");
        }

        // 수정 화면
        public IActionResult Edit(int id)
        {
            var item = _dailyEnglishService.GetById(id);

            if (item == null)
            {
                return NotFound(); // ✅ Core 방식
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DailyEnglish item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            _dailyEnglishService.Update(item);
            return RedirectToAction("AdminList");
        }

        // 삭제
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _dailyEnglishService.Delete(id);
            return RedirectToAction("AdminList");
        }

        // ─────────────────────────────────────────────
        // ✅ YouTube 영상 관리
        // ─────────────────────────────────────────────

        // 목록 + 등록 폼
        public IActionResult Video()
        {
            var list = _dailyEnglishService.GetAllVideos();
            return View("~/Views/DailyEnglish/Video.cshtml", list);
        }

        // 영상 등록
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VideoAdd(string youTubeId, string title)
        {
            // URL 전체를 붙여넣어도 ID만 추출
            var id = ExtractYouTubeId(youTubeId ?? "");
            if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(title))
                _dailyEnglishService.InsertVideo(id, title);

            return RedirectToAction("Video");
        }

        // 영상 삭제
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VideoDelete(int id)
        {
            _dailyEnglishService.DeleteVideo(id);
            return RedirectToAction("Video");
        }

        // 재생 페이지
        public IActionResult VideoPlay(int id)
        {
            var video = _dailyEnglishService.GetVideoById(id);
            if (video == null) return RedirectToAction("Video");
            return View("~/Views/DailyEnglish/VideoPlay.cshtml", video);
        }

        // ✅ YouTube ID 추출 헬퍼 (전체 URL / 단순 ID 둘 다 지원)
        private static string ExtractYouTubeId(string input)
        {
            input = input.Trim();
            // youtu.be/XXXX
            var m = System.Text.RegularExpressions.Regex.Match(input, @"youtu\.be/([A-Za-z0-9_-]{11})");
            if (m.Success) return m.Groups[1].Value;
            // ?v=XXXX or &v=XXXX
            m = System.Text.RegularExpressions.Regex.Match(input, @"[?&]v=([A-Za-z0-9_-]{11})");
            if (m.Success) return m.Groups[1].Value;
            // /embed/XXXX
            m = System.Text.RegularExpressions.Regex.Match(input, @"/embed/([A-Za-z0-9_-]{11})");
            if (m.Success) return m.Groups[1].Value;
            // 11자리 ID 그대로
            if (System.Text.RegularExpressions.Regex.IsMatch(input, @"^[A-Za-z0-9_-]{11}$"))
                return input;
            return "";
        }
    }
}
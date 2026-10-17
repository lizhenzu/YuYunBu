// Controllers/BandBoardController.cs
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class BandBoardController : Controller
    {
        private readonly IBandBoardService _bandBoardService;
        private readonly IWebHostEnvironment _env;

        // ✅ 여기 PageSize 상수 선언 (다른 곳에서 사용 가능)
        private const int PageSize = 5;

        public BandBoardController(IBandBoardService bandBoardService, IWebHostEnvironment env)
        {
            _bandBoardService = bandBoardService;
            _env = env;
        }

        /// <summary>
        /// 공용 게시판 엔트리
        /// - 기본: BandPosts / "게시판"
        /// - 예: /BandBoard?boardTableName=BandJuboPosts&boardName=주보게시판
        /// - 예: /BandBoard?boardTableName=BandAPosts&boardName=A게시판
        /// </summary>
        public async Task<IActionResult> Index(
        string boardTableName = "BandPosts",
        string boardName = "게시판")
        {
            // ✅ 특정 테이블이면 강제 이동
            if (boardTableName == "BandPosts")
            {
                return Redirect("/ReplayBoard/Details/6?tableName=NoticeBoard");
            }

            var result = await _bandBoardService.GetPostsPageAsync(boardTableName, 1, PageSize);
            var counts = await _bandBoardService.GetPostCountsAsync(boardTableName);

            ViewBag.HasMore = result.HasMore;
            ViewBag.NextPage = result.NextPage;
            ViewBag.BoardTableName = boardTableName;
            ViewBag.BoardName = boardName;
            ViewBag.TotalCount = counts.Total;
            ViewBag.TodayCount = counts.Today;

            return View(result.Posts);
        }

        /// <summary>
        /// 무한 스크롤용 추가 로드
        /// </summary>
        public async Task<IActionResult> LoadMore(string boardTableName, string boardName, int page)
        {
            // ✅ 여기도 boardTableName, page, PageSize 3개 인자 전달
            var result = await _bandBoardService.GetPostsPageAsync(boardTableName, page, PageSize);

            ViewBag.HasMore = result.HasMore;
            ViewBag.NextPage = result.NextPage;

            ViewBag.BoardTableName = boardTableName;
            ViewBag.BoardName = boardName;

            return PartialView("_PostListPartial", result.Posts);
        }

        /// <summary>
        /// 등록 폼 페이지 (GET)
        /// </summary>
        [HttpGet]
        [Authorize]
        public IActionResult Create(string boardTableName = "BandPosts", string boardName = "게시판")
        {
            ViewBag.BoardTableName = boardTableName;
            ViewBag.BoardName      = boardName;
            return View();
        }

        /// <summary>
        /// 글 + 사진 업로드
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(
            string boardTableName,
            string boardName,
            string writerName,
            string content,
            List<IFormFile> imageFiles)
        {
            bool hasText = !string.IsNullOrWhiteSpace(content);
            bool hasImage = imageFiles != null && imageFiles.Any(f => f != null && f.Length > 0);

            if (!hasText && !hasImage)
            {
                TempData["Error"] = "텍스트나 사진 중 한 가지는 반드시 입력해 주세요.";
                return RedirectToAction(nameof(Index), new { boardTableName, boardName });
            }

            // 1) 물리 파일 저장 (wwwroot/uploads/band)
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "band");
            Directory.CreateDirectory(uploadsRoot);

            var imagePaths = new List<string>();

            if (hasImage)
            {
                foreach (var file in imageFiles.Where(f => f != null && f.Length > 0))
                {
                    var ext = Path.GetExtension(file.FileName);
                    var fileName = $"{DateHelper.GetKoreaNow():yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsRoot, fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                    }

                    imagePaths.Add($"/uploads/band/{fileName}");
                }
            }

            // 2) DB 등록 (게시판별 테이블로)
            await _bandBoardService.CreatePostAsync(boardTableName, writerName, content, imagePaths);

            return RedirectToAction(nameof(Index), new { boardTableName, boardName });
        }

        /// <summary>
        /// 좋아요 (AJAX)
        /// JS에서: /BandBoard/Like?boardTableName=... 으로 호출
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Like(string boardTableName, int id)
        {
            var likeCount = await _bandBoardService.IncrementLikeAsync(boardTableName, id);
            return Json(new { likeCount });
        }

        /// <summary>
        /// 댓글 등록 (AJAX)
        /// JS에서: /BandBoard/AddComment?boardTableName=... 으로 호출
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddComment(string boardTableName, int postId, string writerName, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("댓글 내용을 입력해 주세요.");

            var comment = await _bandBoardService.AddCommentAsync(boardTableName, postId, writerName, content);

            return Json(new
            {
                id = comment.Id,
                writerName = comment.WriterName,
                content = comment.Content,
                createdAt = comment.CreatedAt.ToString("yyyy.MM.dd HH:mm")
            });
        }

        /// <summary>
        /// 게시글 삭제 (DB + 관련 댓글/이미지 삭제)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(string boardTableName, string boardName, int id)
        {
            await _bandBoardService.DeletePostAsync(boardTableName, id);
            return RedirectToAction(nameof(Index), new { boardTableName, boardName });
        }
    }
}

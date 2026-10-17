using BulletinBoard.BusinessLayer;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Notice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static BulletinBoard.Services.NoticeService;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class NoticeController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly INoticeService _noticeService;
        private readonly IBandBoardService _bandBoardService;

        // ✅ 업로드 폴더(원하시면 BulletinBoard로 바꾸는 규칙 그대로 적용 가능)
        // wwwroot/uploads/notice/{noticeId}/...
        private const string NOTICE_UPLOAD_ROOT = "uploads/notice";

        public NoticeController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            INoticeService noticeService,
            IBandBoardService bandBoardService,
            IWebHostEnvironment hostingEnvironment
        ) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _noticeService = noticeService;
            _bandBoardService = bandBoardService;
        }

        // ----------------------------
        // 목록
        // ----------------------------
       public async Task<IActionResult>  Index(int page = 1, string keyword = null)
        {
            const int pageSize = 10;
            var vm = _noticeService.GetNoticeList(page, pageSize, keyword);
            return View(vm);
        } // ----------------------------
          // 목록
          // ----------------------------
        public async Task<IActionResult> Branch()
        {
            SetLatestNotices();
            await BindBandBoardTopAsync();
            return View();
        }


        private async Task BindBandBoardTopAsync()
        {
            var bandBoard = await _bandBoardService.GetPostsPageAsync("BandPosts", 1, 1);

            if (bandBoard?.Posts != null && bandBoard.Posts.Any())
            {
                ViewBag.BandBoardMessage = bandBoard.Posts.First();
            }
            else
            {
                ViewBag.BandBoardMessage = "게시판 데이터가 없습니다."; 
            }
        }


        // ----------------------------
        // 상세
        // ----------------------------
        public IActionResult Details(int id)
        {
            var notice = _noticeService.GetNoticeAndIncreaseViewCount(id);
            if (notice == null) return NotFound();

            // ✅ 반드시 추가
            ViewBag.Attachments = _noticeService.GetAttachments(id);

            return View(notice);
        }

        // ----------------------------
        // Create (GET)
        // ----------------------------
        [Authorize]
        public IActionResult Create()
        {
            var vm = new NoticeEditVm
            {
                AuthorName = User.Identity.IsAuthenticated ? User.Identity.Name : "",
                // ✅ 팝업 기본값(원하시면 false로)
                // IsPopup = false,
                // PopupUntil = null
            };
            return View(vm);
        }

        // ----------------------------
        // Create (POST) : 다중 파일 업로드 지원
        // ----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Create(NoticeEditVm vm, List<IFormFile> files)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // 1️⃣ 공지 먼저 생성
            var newId = _noticeService.CreateNotice(vm);

            // 2️⃣ 파일 저장 + DTO 생성
            var attachments = SaveNoticeFilesAndBuildDtos(newId, files);

            // 3️⃣ 🔥 첨부가 있으면 반드시 UpdateNotice 호출
            if (attachments.Count > 0)
            {
                vm.Id = newId;
                _noticeService.UpdateNotice(vm, attachments, deleteAttachmentIds: null);
            }

            return RedirectToAction("Details", new { id = newId });
        }

        // ----------------------------
        // Edit (GET)
        // ----------------------------
        [Authorize]
        public IActionResult Edit(int id)
        {
            var vm = _noticeService.GetNoticeForEdit(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // ----------------------------
        // Edit (POST) : 첨부 추가 + 기존 첨부 삭제 지원
        // ----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(
            NoticeEditVm vm,
            List<IFormFile> files,
            List<long> deleteAttachmentIds // Edit.cshtml에서 체크된 첨부 Id들이 바인딩되게 name 맞춰야 함
        )
        {
            if (!ModelState.IsValid)
                return View(vm);

            // 1) 새 파일 저장
            var newAttachments = SaveNoticeFilesAndBuildDtos(vm.Id.Value, files);

            // 2) DB 반영 (공지 수정 + 첨부 추가/삭제)
            var ok = _noticeService.UpdateNotice(vm, newAttachments, deleteAttachmentIds);
            if (!ok) return NotFound();

            // 3) (선택) 실제 파일 삭제까지 하려면, deleteAttachmentIds로 StoredName을 조회해서 지우는 로직 추가 가능
            // 지금은 DB soft delete만 처리(서비스 기준). 실제 파일 삭제까지 원하시면 말씀해 주세요.

            return RedirectToAction("Details", new { id = vm.Id });
        }

        // ----------------------------
        // 첨부 다운로드
        // ----------------------------
        public IActionResult DownloadAttachment(long id)
        {
            // 서비스에서 attachment 조회 필요: GetAttachment(id)
            var att = _noticeService.GetAttachment(id);
            if (att == null || att.IsDeleted) return NotFound();

            var fullPath = GetStoredFileFullPath(att.NoticeId, att.StoredName);
            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var contentType = string.IsNullOrWhiteSpace(att.ContentType)
                ? "application/octet-stream"
                : att.ContentType;

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, contentType, att.OriginalName);
        }

        // ----------------------------
        // Delete (POST)
        // ----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var ok = _noticeService.SoftDeleteNotice(id);
            if (!ok) return NotFound();

            // (선택) 실제 파일 폴더도 지우고 싶으면 여기서 폴더 삭제 처리 가능
            // var dir = GetNoticeUploadDir(id);
            // if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);

            return RedirectToAction("Index");
        }

        // ==========================================================
        // Helpers
        // ==========================================================
        private List<NoticeAttachmentCreateDto> SaveNoticeFilesAndBuildDtos(int noticeId, List<IFormFile> files)
        {
            var list = new List<NoticeAttachmentCreateDto>();
            if (files == null || files.Count == 0) return list;

            var dir = GetNoticeUploadDir(noticeId);
            Directory.CreateDirectory(dir);

            foreach (var f in files)
            {
                if (f == null || f.Length <= 0) continue;

                // (선택) 크기 제한/확장자 제한 원하면 여기서 체크
                // if (f.Length > 20 * 1024 * 1024) continue;

                var originalName = Path.GetFileName(f.FileName);

                // 저장 파일명: GUID + 확장자
                var ext = Path.GetExtension(originalName);
                var storedName = $"{Guid.NewGuid():N}{ext}";

                var fullPath = Path.Combine(dir, storedName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    f.CopyTo(stream);
                }

                list.Add(new NoticeAttachmentCreateDto
                {
                    OriginalName = originalName,
                    StoredName = storedName,
                    ContentType = f.ContentType,
                    FileSize = f.Length
                });
            }

            return list;
        }

        private string GetNoticeUploadDir(int noticeId)
        {
            // wwwroot/uploads/notice/{noticeId}
            var webRoot = _hostingEnvironment.WebRootPath;
            return Path.Combine(webRoot, NOTICE_UPLOAD_ROOT, noticeId.ToString());
        }

        private string GetStoredFileFullPath(int noticeId, string storedName)
        {
            return Path.Combine(GetNoticeUploadDir(noticeId), storedName);
        }


        private void SetLatestNotices()
        {
            // (deprecated) 기존 단일 최신 공지 로직
            SetPinnedNotices();
        }

        // ✅ 공지 배너: IsPinned=true 여러 건을 서비스에서 선별해서 전달
        private void SetPinnedNotices()
        {
            var nowKst = DateHelper.GetKoreaNow();
            var pinned = _noticeService.GetPinnedNoticesForBanner(nowKst, maxN: 20);

            if (pinned == null || pinned.Count == 0)
            {
                ViewBag.PinnedNotices = null;
                ViewBag.LatestNotice = null;
                ViewBag.LatestAttachments = null;
                return;
            }

            ViewBag.PinnedNotices = pinned;
            ViewBag.LatestNotice = pinned.FirstOrDefault();
            ViewBag.LatestAttachments = null;
        }


    }
}

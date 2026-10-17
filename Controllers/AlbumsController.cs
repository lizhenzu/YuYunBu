using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YunYunBu2024.Services;

namespace BulletinBoard.Controllers
{
        [LogActionFilter]
    public class AlbumsController : BaseController
    {
        private readonly IAlbumService _svc;
        private readonly IFileStorage _storage;
        private readonly IZipService _zip;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public AlbumsController(
            IAlbumService svc,
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor http,
            IFileStorage storage,
            IWebHostEnvironment hostingEnvironment,
            IZipService zip) : base(authService, userManager, hostingEnvironment)
        {
            _svc = svc;
            _storage = storage;
            _zip = zip;
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }



        // GET: /Albums — 앨범 홈 (월별 묶음 · 검색 · 연도 · 정렬)
        public async Task<IActionResult> Index()
        {
            var albums = await _svc.GetAlbumSummariesAsync();
            return View(albums);
        }

        // GET: /Albums
        public async Task<IActionResult> Branch()
        {
            return View();
        }

        // GET: /Albums/Create
        [Authorize]
        public IActionResult Create() => View();

        // POST: /Albums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(string name, string description)
        {
            if (await _authService.IsSignedIn(User))
            {
                await SetUserInfo();
            }

            //// 로그인 사용자 세션 ID
            //var uerFullName = _http.HttpContext?.Session
            //    .GetString(HttpContextSessionCategory.UserFullName.ToString());

            // 로그인 정보 세팅
      
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("name", "앨범 이름은 필수입니다.");
                return View();
            }

            var album = await _svc.CreateAlbumAsync(name.Trim(), description?.Trim(), this.UserFullName);
            // 만들자마자 사진 올리기 창을 연다
            return RedirectToAction(nameof(Details), new { id = album.Id, upload = 1 });
        }

        // ==========================================================
        // 새 앨범 화면용 (화면을 다시 불러오지 않고 처리 · JSON)
        // ==========================================================

        // POST: /Albums/UpdateAlbum — 이름 · 설명 고치기
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateAlbum(int id, string name, string description)
        {
            name = (name ?? "").Trim();
            if (id <= 0 || name.Length == 0) return Json(new { ok = false, message = "앨범 이름을 넣어 주세요." });
            if (name.Length > 200) name = name.Substring(0, 200);
            description = (description ?? "").Trim();
            if (description.Length > 1000) description = description.Substring(0, 1000);
            var ok = await _svc.UpdateAlbumAsync(id, name, description);
            return Json(new { ok, name, description, message = ok ? "" : "앨범을 찾지 못했어요." });
        }

        // POST: /Albums/DeletePhotosApi — 사진 여러 장 지우기
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeletePhotosApi(int albumId, int[] photoIds)
        {
            if (albumId <= 0 || photoIds == null || photoIds.Length == 0) return Json(new { ok = false, message = "지울 사진을 골라 주세요." });
            try
            {
                await _svc.DeletePhotosAsync(albumId, photoIds);
                var (album, _) = await _svc.GetAlbumWithPhotosAsync(albumId, 1);
                return Json(new { ok = true, deleted = photoIds.Length, coverPhotoId = album?.CoverPhotoId });
            }
            catch (Exception ex) { return Json(new { ok = false, message = ex.Message }); }
        }

        // POST: /Albums/SetCoverApi — 커버 사진 정하기
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> SetCoverApi(int albumId, int photoId)
        {
            try { await _svc.SetCoverAsync(albumId, photoId); return Json(new { ok = true, coverPhotoId = photoId }); }
            catch (Exception ex) { return Json(new { ok = false, message = ex.Message }); }
        }

        // GET: /Albums/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var (album, photos) = await _svc.GetAlbumWithPhotosAsync(id);
            if (album == null) return NotFound();

            ViewBag.Album = album;
            return View(photos);
        }

        // GET: /Albums/Details/5
        public async Task<IActionResult> MyPhotoPick()
        {
            // 1) 선택된 사진 정보 가져오기
            ManageAlbum manageAlbum = new ManageAlbum();
            List<UserInfoBase2> urlInfo = manageAlbum.getMyPhotoPick();

            // 🔴 여기 중요: 반드시 UserInfos 리스트를 뷰에 넘긴다
            return View(urlInfo);
        }

        // POST: /Albums/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Upload(int id, List<Microsoft.AspNetCore.Http.IFormFile> files)
        {

            if (await _authService.IsSignedIn(User))
            {
                await SetUserInfo();
            }

            var saved = await _svc.UploadAsync(id, files, this.UserFullName);
            TempData["msg"] = $"업로드 완료: {saved}개";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Albums/DeletePhoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeletePhoto(int photoId, int albumId)
        {
            await _svc.DeletePhotoAsync(photoId);
            TempData["msg"] = "사진이 삭제되었습니다.";
            return RedirectToAction(nameof(Details), new { id = albumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAlbumAsync(id);
            TempData["msg"] = "앨범과 내부 사진이 모두 삭제되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Albums/SetCover
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> SetCover(int albumId, int photoId)
        {
            await _svc.SetCoverAsync(albumId, photoId);
            return RedirectToAction(nameof(Details), new { id = albumId });
        }

        // GET: /Albums/DownloadAlbum/5  -> 앨범 전체 ZIP
        public async Task<IActionResult> DownloadAlbum(int id)
        {
            var (album, photos) = await _svc.GetAlbumWithPhotosAsync(id);
            if (album == null) return NotFound();
            if (photos == null || photos.Count == 0) return NotFound();

            var streams = new List<(string EntryName, Stream Source)>();
            try
            {
                foreach (var p in photos)
                {
                    var s = await _storage.OpenReadAsync(p.StoragePath);
                    // ZIP 안에서는 원본 파일명으로 넣어줌 (동명이 파일 충돌 가능 시 Id 접미어 등 추가 권장)
                    var entry = string.IsNullOrWhiteSpace(p.OriginalName)
                        ? p.FileName
                        : p.OriginalName;
                    streams.Add((entry, s));
                }

                var zip = await _zip.CreateZipAsync(streams);
                var fileName = $"Album_{(string.IsNullOrWhiteSpace(album.Name) ? id.ToString() : album.Name)}.zip";
                return File(zip, "application/zip", fileName);
            }
            finally
            {
                // ZipService에서 Source를 dispose하지 않으므로 여기서 정리
                foreach (var (_, src) in streams)
                    src?.Dispose();
            }
        }
        [HttpPost]
        [Route("Albums/DeleteSelected")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteSelected(int albumId, int[] photoIds)
        {
            if (photoIds == null || photoIds.Length == 0)
            {
                TempData["msg"] = "삭제할 사진이 선택되지 않았습니다.";
                return RedirectToAction(nameof(Details), new { id = albumId });
            }

            await _svc.DeletePhotosAsync(albumId, photoIds);
            TempData["msg"] = $"{photoIds.Length}개의 사진을 삭제했습니다.";
            return RedirectToAction(nameof(Details), new { id = albumId });
        }


        // POST: /Albums/DownloadSelected -> 선택 ZIP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadSelected(int albumId, int[] photoIds)
        {
            if (photoIds == null || photoIds.Length == 0)
                return RedirectToAction(nameof(Details), new { id = albumId });

            var (album, allPhotos) = await _svc.GetAlbumWithPhotosAsync(albumId);
            if (album == null) return NotFound();

            var photos = allPhotos.Where(p => photoIds.Contains(p.Id)).ToList();
            if (photos.Count == 0)
                return RedirectToAction(nameof(Details), new { id = albumId });

            var streams = new List<(string EntryName, Stream Source)>();
            try
            {
                foreach (var p in photos)
                {
                    var s = await _storage.OpenReadAsync(p.StoragePath);
                    var entry = string.IsNullOrWhiteSpace(p.OriginalName)
                        ? p.FileName
                        : p.OriginalName;
                    streams.Add((entry, s));
                }

                var zip = await _zip.CreateZipAsync(streams);
                var fileName = $"Album_{(string.IsNullOrWhiteSpace(album.Name) ? albumId.ToString() : album.Name)}_Selected.zip";
                return File(zip, "application/zip", fileName);
            }
            finally
            {
                foreach (var (_, src) in streams)
                    src?.Dispose();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UploadAjax(int id, Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (await _authService.IsSignedIn(User))
            {
                await SetUserInfo();
            }
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { ok = false, message = "빈 파일이거나 잘못된 파일입니다." });
            }

            try
            {
                // 기존 UploadAsync 재사용 (하나짜리 리스트로 전달)
                var saved = await _svc.UploadAsync(id, new[] { file }, this.UserFullName);

                return Json(new
                {
                    ok = true,
                    saved,
                    fileName = file.FileName
                });
            }
            catch (Exception ex)
            { 
                return StatusCode(500, new { ok = false, message = ex.Message });
            }
        }

    }
}

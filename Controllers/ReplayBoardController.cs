using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services.ReplayBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Globalization;

namespace BulletinBoard.Controllers;

/// <summary>
/// ✅ ReplayBoardController (확장판)
/// 변경 사항:
/// - ✅ boardName 파라미터 제거
/// - ✅ tableName(=PostTable)만 받고, dbo.BoardRegistry에서 BoardName/ReplyTable을 조회하여 BoardContext 구성
/// </summary>
public sealed class ReplayBoardController : Controller
{
    private readonly ReplayBoardRepository _repo;
    private readonly IWebHostEnvironment _env;
    private readonly IAuthService _auth;

    // ✅ 파일 정책(원하면 변경)
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",".jpg",".jpeg",".gif",".webp",
        ".pdf",
        ".doc",".docx",".xls",".xlsx",".ppt",".pptx",
        ".txt",".zip"
    };

    private static readonly HashSet<string> ImageExt = new(StringComparer.OrdinalIgnoreCase)
    { ".png", ".jpg", ".jpeg", ".gif", ".webp" };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private const int MaxFilesPerPost = 10;

    // 썸네일: 최대 폭(px)
    private const int ThumbMaxWidth = 520;

    public ReplayBoardController(ReplayBoardRepository repo, IWebHostEnvironment env, IAuthService auth)
    {
        _repo = repo;
        _env = env;
        _auth = auth;
    }

    // ✅ 공통: board(테이블/게시판명) 해석 + 검증
    // 이제 boardName은 받지 않고 PostTable(tableName)만으로 BoardRegistry에서 조회합니다.
    private async Task<BoardContext> MustResolveAsync(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new InvalidOperationException("tableName 이 필요합니다.");

        var board = await _repo.ResolveBoardAsync(tableName); // ✅ 변경: (tableName) only
        if (board == null)
            throw new InvalidOperationException($"허용되지 않은 게시판입니다. table={tableName}");

        return board;
    }

    private async Task<(ApplicationUser? user, bool isSignedIn, bool isAdmin)> GetAuthAsync()
    {
        var isSignedIn = await _auth.IsSignedIn(User);
        if (!isSignedIn) return (null, false, false);

        var u = await _auth.GetSignedUser(User);

        var isSuper = await _auth.IsInRole(u, nameof(Roles.SuperAdmin));
        var isAdmin = await _auth.IsInRole(u, nameof(Roles.Admin));
        var isMod = await _auth.IsInRole(u, nameof(Roles.Moderator));
        return (u, true, isSuper || isAdmin || isMod);
    }

    private static string BuildDisplayName(ApplicationUser u)
    {
        var fn = (u.FirstName ?? "").Trim();
        var ln = (u.LastName ?? "").Trim();

        if (!string.IsNullOrWhiteSpace(fn) && !string.IsNullOrWhiteSpace(ln))
            return $"{ln}{fn}"; // 한국식(성+이름) 스냅샷
        if (!string.IsNullOrWhiteSpace(fn)) return fn;
        if (!string.IsNullOrWhiteSpace(ln)) return ln;

        return (u.UserName ?? u.Email ?? "사용자").Trim();
    }

    // ✅ 화면(datetime-local) 값(yyyy-MM-ddTHH:mm)을 타임존 변환 없이 그대로 DB(datetime2)에 저장하기 위한 파서
    private static DateTime? ParsePopupUntilExact(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;

        s = s.Trim();

        // 대부분의 브라우저 datetime-local: "yyyy-MM-ddTHH:mm"
        if (DateTime.TryParseExact(s, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);

        // 일부 환경에서 초까지 올 수 있음: "yyyy-MM-ddTHH:mm:ss"
        if (DateTime.TryParseExact(s, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);

        return null;
    }


    private static string SafeFileName(string name)
    {
        name = Path.GetFileName(name);
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    private static string ThumbNameFromStored(string storedFileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(storedFileName);
        // ✅ 썸네일은 webp로 통일
        return $"{baseName}_thumb.webp";
    }

    // =========================================================
    // Index / Search
    // =========================================================

    // /ReplayBoard/Index?tableName=NoticeBoard&page=1&q=키워드
    public async Task<IActionResult> Index(string tableName, int page = 1, int pageSize = 15, string? q = null)
    {
        var board = await MustResolveAsync(tableName);
        var (user, isSignedIn, isAdmin) = await GetAuthAsync();

        var (items, total) = await _repo.ListAsync(board, page, pageSize, q, viewerUserId: user?.Id);

        ViewBag.Board = board;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;
        ViewBag.Q = q ?? "";
        ViewBag.IsSignedIn = isSignedIn;
        ViewBag.IsAdmin = isAdmin;

        return View(items);
    }

    // =========================================================
    // Details
    // =========================================================

    public async Task<IActionResult> Details(string tableName, int id)
    {
        var board = await MustResolveAsync(tableName);
        var (user, isSignedIn, isAdmin) = await GetAuthAsync();

        var post = await _repo.GetAsync(board, id, increaseView: true, viewerUserId: user?.Id);
        if (post == null) return NotFound();

        var replies = await _repo.ListRepliesAsync(board, id);
        var files = await _repo.ListFilesAsync(board, id);

        // ✅ 권한(작성자/관리자)
        var canEdit = false;
        if (isSignedIn)
        {
            if (isAdmin) canEdit = true;
            else if (!string.IsNullOrWhiteSpace(post.AuthorUserId) && post.AuthorUserId == user!.Id) canEdit = true;
        }

        var vm = new PostDetailVM
        {
            Board = board,
            Post = post,
            Replies = replies
        };

        ViewBag.Files = files;
        ViewBag.IsSignedIn = isSignedIn;
        ViewBag.IsAdmin = isAdmin;
        ViewBag.CanEdit = canEdit;

        return View(vm);
    }

    // =========================================================
    // Create
    // =========================================================

    [HttpGet]
    [Authorize] // ✅ 글쓰기: 로그인 필요
    public async Task<IActionResult> Create(string tableName)
    {
        var board = await MustResolveAsync(tableName);
        var (user, _, isAdmin) = await GetAuthAsync();

        ViewBag.Board = board;
        ViewBag.Writer = user == null ? "" : BuildDisplayName(user);
        ViewBag.IsAdmin = isAdmin;
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string tableName,
        string title,
        string content,
        bool isNotice,
        bool isPinned,
        string? popupUntil,
        List<IFormFile>? files)
    {
        var board = await MustResolveAsync(tableName);
        var (user, isSignedIn, isAdmin) = await GetAuthAsync();

        if (!isSignedIn || user == null) return Forbid();

        title = (title ?? "").Trim();
        content = (content ?? "").Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            ViewBag.Board = board;
            ViewBag.Writer = BuildDisplayName(user);
            ViewBag.IsAdmin = isAdmin;
            ViewBag.Error = "제목/내용을 입력해 주세요.";
            return View();
        }

        // ✅ 공지는 관리자만
        if (!isAdmin) isNotice = false;

        // ✅ (NoticeBoard 전용) 상단고정/팝업도 관리자만
        if (!isAdmin) { isPinned = false; popupUntil = null; }
        if (!isPinned) popupUntil = null;
string? popupUntilRaw = null;
        if (isPinned)
        {
            popupUntilRaw = string.IsNullOrWhiteSpace(popupUntil) ? null : popupUntil.Trim();
        }

        var writerSnapshot = BuildDisplayName(user);

        // ✅ 게시글 + 메타 생성(작성자 UserId 포함)
        var newId = await _repo.CreateAsync(board, title, content, writerSnapshot, user.Id, isNotice, isPinned, popupUntilRaw);

        // ✅ 첨부 처리 + 썸네일 생성
        if (files != null && files.Count > 0)
        {
            foreach (var f in files.Take(MaxFilesPerPost))
            {
                if (f == null || f.Length <= 0) continue;
                if (f.Length > MaxFileSize) continue;

                var ext = Path.GetExtension(f.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext))
                    continue;

                var original = SafeFileName(f.FileName);
                var stored = $"{Guid.NewGuid():N}{ext}";
                var relDir = $"/uploads/{board.PostTable}/{newId}";
                var absDir = Path.Combine(_env.WebRootPath, "uploads", board.PostTable, newId.ToString());

                Directory.CreateDirectory(absDir);

                var absPath = Path.Combine(absDir, stored);
                await using (var fs = System.IO.File.Create(absPath))
                {
                    await f.CopyToAsync(fs);
                }

                // ✅ 이미지면 썸네일 생성 (webp)
                if (ImageExt.Contains(ext))
                {
                    try
                    {
                        var thumbName = ThumbNameFromStored(stored);
                        var thumbAbsPath = Path.Combine(absDir, thumbName);

                        using var image = await Image.LoadAsync(absPath);
                        var ratio = (double)ThumbMaxWidth / image.Width;
                        if (ratio < 1.0)
                        {
                            var newW = ThumbMaxWidth;
                            var newH = (int)Math.Round(image.Height * ratio);
                            image.Mutate(x => x.Resize(newW, newH));
                        }

                        var encoder = new WebpEncoder { Quality = 75 };
                        await image.SaveAsWebpAsync(thumbAbsPath, encoder);
                    }
                    catch
                    {
                        // 썸네일 실패해도 본 파일은 저장되어 있으므로 무시
                    }
                }

                var relPath = $"{relDir}/{stored}";
                var contentType = string.IsNullOrWhiteSpace(f.ContentType) ? "application/octet-stream" : f.ContentType;

                await _repo.AddFileAsync(board, newId, original, stored, contentType, f.Length, relPath);
            }
        }

        return RedirectToAction(nameof(Details), new { tableName, id = newId });
    }

    // =========================================================
    // Edit / Delete (작성자만, 관리자 예외)
    // =========================================================

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(string tableName, int id)
    {
        var board = await MustResolveAsync(tableName);
        var (user, isSignedIn, isAdmin) = await GetAuthAsync();
        if (!isSignedIn || user == null) return Forbid();

        var post = await _repo.GetAsync(board, id, increaseView: false, viewerUserId: user.Id);
        if (post == null) return NotFound();

        var canEdit = isAdmin || (!string.IsNullOrWhiteSpace(post.AuthorUserId) && post.AuthorUserId == user.Id);
        if (!canEdit) return Forbid();

        ViewBag.Board = board;
        ViewBag.Files = await _repo.ListFilesAsync(board, id);
        ViewBag.IsAdmin = isAdmin;

        return View(post);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string tableName,
        int id,
        string title,
        string content,
        bool isPinned,
        string? popupUntil,
        List<IFormFile>? files)
    {
        var board = await MustResolveAsync(tableName);
        var (user, isSignedIn, isAdmin) = await GetAuthAsync();
        if (!isSignedIn || user == null) return Forbid();

        var post = await _repo.GetAsync(board, id, increaseView: false, viewerUserId: user.Id);
        if (post == null) return NotFound();

        var canEdit = isAdmin || (!string.IsNullOrWhiteSpace(post.AuthorUserId) && post.AuthorUserId == user.Id);
        if (!canEdit) return Forbid();

        title = (title ?? "").Trim();
        content = (content ?? "").Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            ViewBag.Board = board;
            ViewBag.Error = "제목/내용을 입력해 주세요.";
            ViewBag.Files = await _repo.ListFilesAsync(board, id);
            ViewBag.IsAdmin = isAdmin;

            post.Title = title;
            post.Content = content;
            return View(post);
        }

        // ✅ (NoticeBoard 전용) 상단고정/팝업은 관리자만 변경 가능
        string? popupUntilRaw;
        if (!isAdmin)
        {
            // 비관리자는 기존 값을 그대로 유지(DB 값을 그대로 재전송)
            isPinned = post.IsPinned;
            popupUntilRaw = post.PopupUntil.HasValue ? post.PopupUntil.Value.ToString("yyyy-MM-dd") : null;
        }
        else
        {
            if (!isPinned) popupUntil = null;
            popupUntilRaw = string.IsNullOrWhiteSpace(popupUntil) ? null : popupUntil.Trim();
        }

        await _repo.UpdateAsync(board, id, title, content, isPinned, popupUntilRaw);

        // ✅ 첨부 추가 업로드 + 썸네일
        if (files != null && files.Count > 0)
        {
            foreach (var f in files.Take(MaxFilesPerPost))
            {
                if (f == null || f.Length <= 0) continue;
                if (f.Length > MaxFileSize) continue;

                var ext = Path.GetExtension(f.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext))
                    continue;

                var original = SafeFileName(f.FileName);
                var stored = $"{Guid.NewGuid():N}{ext}";
                var relDir = $"/uploads/{board.PostTable}/{id}";
                var absDir = Path.Combine(_env.WebRootPath, "uploads", board.PostTable, id.ToString());

                Directory.CreateDirectory(absDir);

                var absPath = Path.Combine(absDir, stored);
                await using (var fs = System.IO.File.Create(absPath))
                {
                    await f.CopyToAsync(fs);
                }

                if (ImageExt.Contains(ext))
                {
                    try
                    {
                        var thumbName = ThumbNameFromStored(stored);
                        var thumbAbsPath = Path.Combine(absDir, thumbName);

                        using var image = await Image.LoadAsync(absPath);
                        var ratio = (double)ThumbMaxWidth / image.Width;
                        if (ratio < 1.0)
                        {
                            var newW = ThumbMaxWidth;
                            var newH = (int)Math.Round(image.Height * ratio);
                            image.Mutate(x => x.Resize(newW, newH));
                        }

                        var encoder = new WebpEncoder { Quality = 75 };
                        await image.SaveAsWebpAsync(thumbAbsPath, encoder);
                    }
                    catch { }
                }

                var relPath = $"{relDir}/{stored}";
                var contentType = string.IsNullOrWhiteSpace(f.ContentType) ? "application/octet-stream" : f.ContentType;

                await _repo.AddFileAsync(board, id, original, stored, contentType, f.Length, relPath);
            }
        }

        return RedirectToAction(nameof(Details), new { tableName, id });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string tableName, int id)
    {
        var board = await MustResolveAsync(tableName);
        var (user, isSignedIn, isAdmin) = await GetAuthAsync();
        if (!isSignedIn || user == null) return Forbid();

        var post = await _repo.GetAsync(board, id, increaseView: false, viewerUserId: user.Id);
        if (post == null) return NotFound();

        var canDelete = isAdmin || (!string.IsNullOrWhiteSpace(post.AuthorUserId) && post.AuthorUserId == user.Id);
        if (!canDelete) return Forbid();

        // ✅ DB 삭제 전에 파일 실제 경로 먼저 정리(게시글에 딸린 첨부 전부)
        var files = await _repo.ListFilesAsync(board, id);
        foreach (var f in files)
        {
            var absPath = Path.Combine(_env.WebRootPath,
                f.RelativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (System.IO.File.Exists(absPath))
                System.IO.File.Delete(absPath);

            // 썸네일도 함께 삭제(있으면)
            var thumbAbs = Path.Combine(Path.GetDirectoryName(absPath)!, ThumbNameFromStored(Path.GetFileName(absPath)));
            if (System.IO.File.Exists(thumbAbs))
                System.IO.File.Delete(thumbAbs);
        }

        await _repo.DeleteAsync(board, id);

        // ✅ 폴더 정리
        var absDir = Path.Combine(_env.WebRootPath, "uploads", board.PostTable, id.ToString());
        if (Directory.Exists(absDir))
        {
            try { Directory.Delete(absDir, recursive: true); } catch { }
        }

        return RedirectToAction(nameof(Index), new { tableName });
    }

    // =========================================================
    // Replies (로그인 사용자 작성자 자동)
    // =========================================================

    [HttpPost]
 //   [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReply(
        string tableName,
        int postId,
        string replyContent,
        string replyWriter,
        string clientTime)
    {
        var board = await MustResolveAsync(tableName);
        //var (user, isSignedIn, _) = await GetAuthAsync();
        //if (!isSignedIn || user == null) return Forbid();

        replyContent = (replyContent ?? "").Trim();
        replyWriter = (replyWriter ?? "익명").Trim();  //BuildDisplayName(user)
        if (!string.IsNullOrWhiteSpace(replyContent))
        {
            var createdAt = DateHelper.GetKoreaNow();
            await _repo.CreateReplyAsync(board, postId, replyContent, replyWriter, createdAt);
        }

        return RedirectToAction(nameof(Details), new { tableName, id = postId });
    }

    /// <summary>
    /// 댓글 수정 · 삭제 권한 (2026-09-27) — Admin · SuperAdmin 역할, 예외로 "Basic" 계정(사용자 이름).
    /// ⚠️ Moderator 는 넣지 않는다.  Roles.Basic 역할(일반 회원)이 아니라 계정 이름 "Basic" 이다.
    /// </summary>
    private async Task<bool> CanManageRepliesAsync()
    {
        if (!await _auth.IsSignedIn(User)) return false;
        if (string.Equals(User.Identity?.Name, "Basic", StringComparison.OrdinalIgnoreCase)) return true;
        var u = await _auth.GetSignedUser(User);
        if (u == null) return false;
        return await _auth.IsInRole(u, nameof(Roles.SuperAdmin)) || await _auth.IsInRole(u, nameof(Roles.Admin));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditReply(string tableName, int postId, int replyId, string replyWriter, string replyContent)
    {
        var board = await MustResolveAsync(tableName);
        if (!await CanManageRepliesAsync()) return Forbid();

        replyContent = (replyContent ?? "").Trim();
        replyWriter  = (replyWriter ?? "").Trim();
        if (string.IsNullOrWhiteSpace(replyWriter)) replyWriter = "익명";
        if (replyContent.Length > 1000) replyContent = replyContent[..1000];
        if (replyWriter.Length > 60)    replyWriter  = replyWriter[..60];

        if (!string.IsNullOrWhiteSpace(replyContent))
            await _repo.UpdateReplyAsync(board, postId, replyId, replyContent, replyWriter);

        return Redirect(Url.Action(nameof(Details), new { tableName, id = postId }) + "#reply-" + replyId);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReply(string tableName, int postId, int replyId)
    {
        var board = await MustResolveAsync(tableName);
        if (!await CanManageRepliesAsync()) return Forbid();

        await _repo.DeleteReplyAsync(board, replyId);
        return RedirectToAction(nameof(Details), new { tableName, id = postId });
    }

    // =========================================================
    // Files
    // =========================================================

    public async Task<IActionResult> DownloadFile(string tableName, int fileId)
    {
        var board = await MustResolveAsync(tableName);
        var file = await _repo.GetFileAsync(board, fileId);
        if (file == null) return NotFound();

        var absPath = Path.Combine(
            _env.WebRootPath,
            file.RelativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
        );

        if (!System.IO.File.Exists(absPath)) return NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(absPath);
        return File(bytes, file.ContentType, file.OriginalFileName);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFile(string tableName, int postId, int fileId)
    {
        var board = await MustResolveAsync(tableName);
        var (user, isSignedIn, isAdmin) = await GetAuthAsync();
        if (!isSignedIn || user == null) return Forbid();

        var post = await _repo.GetAsync(board, postId, increaseView: false, viewerUserId: user.Id);
        if (post == null) return NotFound();

        var canEdit = isAdmin || (!string.IsNullOrWhiteSpace(post.AuthorUserId) && post.AuthorUserId == user.Id);
        if (!canEdit) return Forbid();

        var file = await _repo.GetFileAsync(board, fileId);
        if (file != null)
        {
            var absPath = Path.Combine(
                _env.WebRootPath,
                file.RelativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (System.IO.File.Exists(absPath))
                System.IO.File.Delete(absPath);

            // 썸네일도 삭제
            var thumbAbs = Path.Combine(Path.GetDirectoryName(absPath)!, ThumbNameFromStored(Path.GetFileName(absPath)));
            if (System.IO.File.Exists(thumbAbs))
                System.IO.File.Delete(thumbAbs);

            await _repo.DeleteFileAsync(board, fileId);
        }

        return RedirectToAction(nameof(Details), new { tableName, id = postId });
    }

    // =========================================================
    // Like (추천)
    // =========================================================

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike(string tableName, int postId)
    {
        var board = await MustResolveAsync(tableName);
        var (user, isSignedIn, _) = await GetAuthAsync();
        if (!isSignedIn || user == null) return Forbid();

        await _repo.ToggleLikeAsync(board, postId, user.Id);

        return RedirectToAction(nameof(Details), new { tableName, id = postId });
    }

    // =========================================================
    // Notice (공지 고정) - 관리자/모더레이터
    // =========================================================

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleNotice(string tableName, int postId, bool isNotice)
    {
        var board = await MustResolveAsync(tableName);
        var (_, isSignedIn, isAdmin) = await GetAuthAsync();
        if (!isSignedIn) return Forbid();
        if (!isAdmin) return Forbid();

        await _repo.SetNoticeAsync(board, postId, isNotice);

        return RedirectToAction(nameof(Details), new { tableName, id = postId });
    }
}
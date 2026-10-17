using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient; // DataAccessModule 과 같은 SqlParameter 형식
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class StoryController : BaseController
    {
        private const string StaffRoles = "Admin,SuperAdmin,Moderator";

        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public StoryController(IAuthService authService,
            UserManager<ApplicationUser> userManager) : base(authService, userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        // GET: /Story/Index — 활동 영상 목록
        public async Task<IActionResult> Index()
        {
            ManageStoryElementary123 manageStoryElementary123 = new ManageStoryElementary123();
            List<StoryElementary123s> values = manageStoryElementary123.getStoryElementary123(string.Empty, 0);
            return View(values);
        }

        // ==========================================================
        // 영상 관리 (교사만) — 추가 · 고치기 · 지우기
        // ⚠️ 목록(Index)은 contentsURL 에 "유튜브 영상 ID(11자)"만 있다고 보고 썸네일·재생 주소를 만든다.
        //    기존 ManageStoryElementary123.Create 는 embed 전체 주소를 넣어 목록이 깨지므로 쓰지 않고,
        //    여기서 ID 만 뽑아 매개변수 SQL 로 저장한다.
        // ==========================================================

        // 유튜브 주소 · ID → 11자 영상 ID (못 찾으면 null)
        public static string? ExtractVideoId(string? input)
        {
            var u = (input ?? "").Trim();
            if (u.Length == 0) return null;
            foreach (var p in new[] { @"[?&]v=([A-Za-z0-9_-]{11})", @"youtu\.be/([A-Za-z0-9_-]{11})", @"/embed/([A-Za-z0-9_-]{11})", @"/shorts/([A-Za-z0-9_-]{11})", @"/live/([A-Za-z0-9_-]{11})" })
            {
                var m = Regex.Match(u, p);
                if (m.Success) return m.Groups[1].Value;
            }
            return Regex.IsMatch(u, @"^[A-Za-z0-9_-]{11}$") ? u : null;
        }

        [HttpGet]
        [Authorize(Roles = StaffRoles)]
        public IActionResult Manage()
        {
            var values = new ManageStoryElementary123().getStoryElementary123(string.Empty, 0);
            return View("~/Views/Story/Manage.cshtml", values);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaffRoles)]
        public IActionResult Add(string url, string title)
        {
            var vid = ExtractVideoId(url);
            if (vid == null) { TempData["err"] = "유튜브 주소를 알아볼 수 없어요. 유튜브에서 '공유 → 링크 복사'한 주소를 넣어 주세요."; TempData["url"] = url; TempData["title"] = title; return RedirectToAction(nameof(Manage)); }
            title = (title ?? "").Trim();
            if (title.Length == 0) { TempData["err"] = "영상 제목을 넣어 주세요."; TempData["url"] = url; return RedirectToAction(nameof(Manage)); }
            if (title.Length > 200) title = title.Substring(0, 200);

            DataAccessModule.executeNonQuery(@"
INSERT INTO [dbo].[StoryElementary123] (contentsURL, contentsText, kind, [visited], UpdateDT, InsertDT)
VALUES (@url, @text, @kind, 0, DATEADD(HOUR, @diff, GETDATE()), DATEADD(HOUR, @diff, GETDATE()));",
                new List<SqlParameter> {
                    new("@url", vid), new("@text", title), new("@kind", ""), new("@diff", ConstatsCommon.timeDifference16)
                });
            TempData["msg"] = $"「{title}」 영상을 추가했어요.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaffRoles)]
        public IActionResult Edit(int id, string url, string title)
        {
            var vid = ExtractVideoId(url);
            title = (title ?? "").Trim();
            if (id <= 0 || vid == null || title.Length == 0) { TempData["err"] = "주소나 제목이 비어 있거나 올바르지 않아요."; return RedirectToAction(nameof(Manage)); }
            if (title.Length > 200) title = title.Substring(0, 200);

            DataAccessModule.executeNonQuery(@"
UPDATE [dbo].[StoryElementary123]
SET contentsURL = @url, contentsText = @text, UpdateDT = DATEADD(HOUR, @diff, GETDATE())
WHERE Id = @id;",
                new List<SqlParameter> {
                    new("@url", vid), new("@text", title), new("@diff", ConstatsCommon.timeDifference16), new("@id", id)
                });
            TempData["msg"] = $"「{title}」 영상을 고쳤어요.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaffRoles)]
        public IActionResult Delete(int id, string? title)
        {
            if (id > 0)
            {
                DataAccessModule.executeNonQuery("DELETE FROM [dbo].[StoryElementary123] WHERE Id = @id;", new List<SqlParameter> { new("@id", id) });
                TempData["msg"] = $"「{title}」 영상을 목록에서 지웠어요.";
            }
            return RedirectToAction(nameof(Manage));
        }
    }
}

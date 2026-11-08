using BulletinBoard.BusinessLayer;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    /// <summary>AI 버튼이 복사하는 공용 프람프트 관리 (SuperAdmin 롤 · "Basic" 계정만)</summary>
    [Authorize]
    [LogActionFilter]
    public class QuizAiPromptController : BaseController
    {
        private new readonly IAuthService _authService;

        public QuizAiPromptController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
        }

        // 정보 관리 메뉴의 "SB" 와 같은 기준
        private async Task<bool> IsStaffAsync()
        {
            if (!await _authService.IsSignedIn(User)) return false;
            var user = await _authService.GetSignedUser(User);
            if (await _authService.IsInRole(user, nameof(BulletinBoard.Enums.Roles.SuperAdmin))) return true;
            return string.Equals(User.Identity?.Name, "Basic", System.StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> Index()
        {
            if (!await IsStaffAsync()) return Forbid();
            var manage = new ManageQuizAiPrompt();
            ViewBag.PromptLibrary = manage.GetLibrary();
            return View(manage.Get());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string? promptText)
        {
            if (!await IsStaffAsync()) return Forbid();

            var manage = new ManageQuizAiPrompt();
            string text = QuizAiPromptHelper.Normalize(promptText);

            var missing = QuizAiPromptHelper.MissingTokens(text);
            if (missing.Count > 0)
            {
                ViewBag.PromptError = $"{string.Join(", ", missing)} 자리가 빠져서 저장하지 않았어요. 이 자리에 그날 구절과 날짜가 들어가요.";
                return View("Index", WithText(manage, text));
            }

            try
            {
                manage.Save(text, User.Identity?.Name ?? "");
            }
            catch (SqlException ex)
            {
                ViewBag.PromptError = $"저장하지 못했어요. 표(dbo.QuizAiPrompt)를 만들었는지 확인해 주세요. ({ex.Message})";
                return View("Index", WithText(manage, text));
            }

            TempData["PromptSaved"] = "저장했어요. AI 버튼이 있는 모든 화면에 바로 적용돼요.";
            return RedirectToAction(nameof(Index));
        }

        // 저장하지 못했을 때 입력한 글을 그대로 둔다
        private QuizAiPromptInfo WithText(ManageQuizAiPrompt manage, string text)
        {
            ViewBag.PromptLibrary = manage.GetLibrary();
            var info = manage.Get();
            info.PromptText = text;
            return info;
        }
    }
}

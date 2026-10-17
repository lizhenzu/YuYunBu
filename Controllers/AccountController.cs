using BulletinBoard.BusinessLayer;
using BulletinBoard.Models;
using BulletinBoard.Models.AccountViewModels;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountController(IAuthService authService,
            SignInManager<ApplicationUser> signInManager)
        {
            _authService = authService;
            _signInManager = signInManager;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult Login(string returnUrl = null)
        //{
        //    ViewData["ReturnUrl"] = returnUrl;
        //    return View();
        //}

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            // Identity Razor Page 로그인으로 넘겨버리기
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Content("~/");
            }

            return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _authService.Login(model.UserName, model.Password, model.RememberMe);
            if (result)
            {
                // 로그인 성공 시, returnUrl 이 비어 있으면 기본 페이지로 대체
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    returnUrl = Url.Content("~/Home/IndexBranch");
                }

                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "잘못된 사용자이름(영문명)이나 비밀번호 입니다.");
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid register attempt.");
                return View(model);
            }

            bool result = await _authService.Register(model.UserName, model.Email, model.Password, true);

            if (result)
            {
                ManageAccount manageAccount = new ManageAccount();
                manageAccount.UpdateAspNetUsers(model);

                var user = await _authService.GetSignedUser(User);

                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    returnUrl = Url.Content("~/Home/IndexBranch");
                }

                return RedirectToLocal(returnUrl);
            }

            ModelState.TryAddModelError(string.Empty, "This email address is already taken.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.Logout();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}

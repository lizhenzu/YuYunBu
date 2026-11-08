using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using BulletinBoard.Models;
using System.Net.Mail;
using BulletinBoard.Common;
using Microsoft.AspNetCore.Http;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services;

namespace BulletinBoard.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IAuthService _authService;
        public LoginModel(SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            IAuthService authService,
            UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        // ── 관리자로 로그인 (2026-11-08 시안 확정) ──
        //   화면에는 아이디를 넣지 않는다 — "관리자 모드" 표시만 오고, 아이디는 서버에서 채운다 (페이지 소스에 관리자 아이디가 보이지 않게)
        private const string AdminUserName = "superadmin";

        [BindProperty]
        public bool AdminMode { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "이메일")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "비밀번호")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "로그인 정보 기억")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }
        private async Task<string> GetUserName()
        {
            string userName = string.Empty;
            if (await _authService.IsSignedIn(User))
            {
                userName = await _authService.GetUserName(User);
            }

            return userName;
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (AdminMode)
            {
                // 관리자 모드 — 아이디 칸은 비어서 온다. 서버에서 채우고 그 칸의 "필수" 오류를 지운다
                Input ??= new InputModel();
                Input.Email = AdminUserName;
                ModelState.Remove("Input.Email");
            }

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var userName = Input.Email;
                var userID = string.Empty;
                var userFullName = string.Empty;
                if (AdminMode)
                {
                    var admin = await _userManager.FindByNameAsync(AdminUserName);
                    if (admin != null)
                    {
                        userName = admin.UserName;
                        userID = admin.Id;
                        userFullName = $"{admin.LastName}{admin.FirstName}";
                    }
                }
                else if (IsValidEmail(Input.Email))
                {
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                    {
                        userName = user.UserName;
                        userID = user.Id;
                        userFullName = $"{user.LastName}{user.FirstName}";
                    }
                }
                var result = await _signInManager.PasswordSignInAsync(userName, Input.Password,  true, lockoutOnFailure: false);
              //  var result = await _signInManager.PasswordSignInAsync(userName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    HttpContext.Session.SetString(HttpContextSessionCategory.UserName.ToString(), userName);
                    HttpContext.Session.SetString(HttpContextSessionCategory.UserId.ToString(), userID);
                    HttpContext.Session.SetString(HttpContextSessionCategory.UserFullName.ToString(), userFullName);
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    if (AdminMode)
                    {
                        Input.Email = "";   // 다시 그린 화면에 관리자 아이디가 실리지 않게
                        ModelState.Remove("Input.Email");
                        ModelState.AddModelError(string.Empty, "관리자 비밀번호가 맞지 않아요");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "이메일 또는 비밀번호 오류");
                    }
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        public bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}

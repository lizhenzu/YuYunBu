using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using BulletinBoard.Models;
using BulletinBoard.Helpers;

namespace BulletinBoard.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
      //  private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
           // _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "이름(예: '길동')")]
            public string FirstName { get; set; }
            [Required]
            [Display(Name = "성(예: '홍')")]
            public string LastName { get; set; }
            [Required]
            [EmailAddress]
            [Display(Name = "이메일")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "비밀번호 최소 3자리 이상 입력하세요.", MinimumLength = 3)]
            [DataType(DataType.Password)]
            [Display(Name = "비밀번호")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "재확인 비밀번호")]
            [Compare("Password", ErrorMessage = "비밀번호와 재확인 비밀번호가 같지 않습니다.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                MailAddress address = new MailAddress(Input.Email);
                string userName = address.User;
                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    InsertDT = DateHelper.GetKoreaNow()
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _userManager.AddToRoleAsync(user, Enums.Roles.Basic.ToString());
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        // 사람 명부(PersonMaster_2026): 같은 한글 이름이 없으면 자동으로 만든다(UserName 포함).
                        // 같은 이름이 있으면 가입 마법사에서 "나인지" 고르게 하고, 관리자 승인 때 연결한다. 실패해도 가입은 막지 않는다.
                        try
                        {
                            var fullName = (Input.LastName + Input.FirstName).Trim();
                            var match = new BulletinBoard.DataAccessLayer.ControlPersonMatch();
                            var same = await match.FindByFullNameAsync(fullName);
                            if (same.Count == 0)
                                await match.CreateAsync(fullName, user.UserName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "가입 후 PersonMaster_2026 생성 실패");
                        }

                        // 가입 마법사 — 역할(학생 · 교사 · 학부모)과 나머지 정보 입력 → 관리자 승인 요청
                        return LocalRedirect(Url.Content("~/SignupWizard") + "?returnUrl=" + Uri.EscapeDataString(returnUrl));
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}

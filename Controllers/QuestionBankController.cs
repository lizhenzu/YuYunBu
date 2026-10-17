using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    //[Authorize(Roles = nameof(Roles.Admin))]
    public class QuestionBankController : BaseController
    {
        private readonly IAuthService _authService;
        private string queryCode = "";
        private readonly UserManager<ApplicationUser> _userManager;
        public QuestionBankController(IAuthService authService,
            UserManager<ApplicationUser> userManager) : base(authService, userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        // queryCode는 class의 필드/프로퍼티(int)라고 가정
        private async Task SetQueryCodeAsync()
        {
            var user = await _authService.GetSignedUser(User);
            var isSignedIn = await _authService.IsSignedIn(User);
            var isSuperAdmin = isSignedIn && await _authService.IsInRole(user, nameof(Roles.SuperAdmin));
            var isAdmin = isSignedIn && await _authService.IsInRole(user, nameof(Roles.Admin));
            var isBasic = isSignedIn && await _authService.IsInRole(user, nameof(Roles.Basic));

            if (isAdmin)
            {
                this.queryCode = "20250921";
            }
            else
            {
                this.queryCode = "20250928";
            }
        }


        public ActionResult Index()
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();

            return View(manageQuestionBank.getQuestionBankOrg(0, string.Empty));
        }

        public async Task<IActionResult> IndexByCode()
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return View("Index", manageQuestionBank.getQuestionBankOrg(0, this.queryCode));
        }

        public async Task<IActionResult> IndexSlide()
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return View("IndexSlide", manageQuestionBank.getQuestionBankOrg(0, this.queryCode));
        }

        public async Task<IActionResult> IndexSlideView()
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return View("IndexSlideView", manageQuestionBank.getQuestionBankOrg(0, this.queryCode));
        }

        public async Task<IActionResult> IndexAnswer()
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return View(manageQuestionBank.getQuestionBankOrg(0, string.Empty));
        }

        public async Task<IActionResult> DetailQuestionBank(int Id)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return View("Index", manageQuestionBank.getQuestionBankOrg(Id, string.Empty));
        }

        public async Task<IActionResult> ModifyQuestionBank(int Id)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();

            List<QuestionBankData> value = manageQuestionBank.getQuestionBankOrg(Id, string.Empty);

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return View(value[0]);
        }


        
        [HttpPost]
        public async Task<IActionResult> ModifyQuestionBankProcess(QuestionBankData param)
        {
            string userName = string.Empty;

            if (await _authService.IsSignedIn(User))
            {
                userName = await _authService.GetUserName(User);
            }

            ManageQuestionBank manageQuestionBank = new ManageQuestionBank(); 

            ModelState.Remove("AnswerQ");
            ModelState.Remove("QuestionTypeDesc");
            ModelState.Remove("UserName");
            ModelState.Remove("WeekDayKor");

            if (param.QuestionType == "s")
            {
                ModelState.Remove("Question1");
                ModelState.Remove("Question2");
                ModelState.Remove("Question3");
                ModelState.Remove("Question4");
            }

            if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
                return View("ModifyQuestionBank", param);
            }
            else
            {
                manageQuestionBank.UpdateQuestionBank(param);
            }

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return RedirectToAction("DetailQuestionBank", new {Id = param.Id});
        }


        public async Task<IActionResult> VerifyQuestionBank(int Id)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();
            manageQuestionBank.VerifyQuestionBank(Id);

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UpdatePrevRecordUpdateDT(int Id)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();
            manageQuestionBank.UpdatePrevRecordUpdateDT(Id);

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UpdateNextRecordUpdateDT(int Id)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();
            manageQuestionBank.UpdateNextRecordUpdateDT(Id);

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MinifyQuestionBank(int Id)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();
            manageQuestionBank.MinifyQuestionBank(Id);

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> DeleteQuestionBank(int Id)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();
            manageQuestionBank.DeleteQuestionBank(Id);

            await SetQueryCodeAsync();
            ViewBag.queryCode = this.queryCode;

            return RedirectToAction("Index");
        }
    }
}

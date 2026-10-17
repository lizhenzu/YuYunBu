using BulletinBoard.BusinessLayer;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class SurveyController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public SurveyController(IAuthService authService,
            UserManager<ApplicationUser> userManager) : base(authService, userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }


        // GET: JobCategory
        public async Task<IActionResult> CreateSurvey()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessSurvey(SurveyData param)
        {         
            ManageSurvey manageSurvey = new ManageSurvey();           
            manageSurvey.CreateSurveyResult(param);             
            return RedirectToLocal("~/Survey/ProcessSurveyResult");
        }
        // GET: JobCategory
        public async Task<IActionResult> ProcessSurveyResult()
        {
            return View();
        } 
         
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(SurveyController.CreateSurvey), "Survey");
        }
    }
}



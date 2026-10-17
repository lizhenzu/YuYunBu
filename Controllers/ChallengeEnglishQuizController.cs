using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize]
    public class ChallengeEnglishQuizController : BaseController
    {
        private readonly string controllName = "ChallengeEnglishQuiz";
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ChallengeEnglishQuizController(IAuthService authService,
        UserManager<ApplicationUser> userManager,
          IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }
        // GET: JobCategory
        public async Task<IActionResult> Index()
        {
            ManageEnglishBibleQuiz manageEnglishBibleQuiz = new ManageEnglishBibleQuiz();

            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;
            List<EnglishBibleQuiz> data =   manageEnglishBibleQuiz.getEnglishBibleQuizCount(); ;

            ViewBag.EnglishBibleQuizCount = data.Count;
            ViewBag.MissionSuccess = manageEnglishBibleQuiz.getEnglishBibleQuizSuccessCount(UserName, true, 0);
            ViewBag.MissionFailed = manageEnglishBibleQuiz.getEnglishBibleQuizSuccessCount(UserName, false, 0);
            ViewBag.Remaining = ViewBag.EnglishBibleQuizCount - ViewBag.MissionSuccess - ViewBag.MissionFailed;
            return View();
        }


        public async Task<IActionResult> RankList()
        {
            ManageEnglishBibleQuiz manageEnglishBibleQuiz = new ManageEnglishBibleQuiz();

            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;
            List<EnglishBibleQuizRank> data = manageEnglishBibleQuiz.getEnglishBibleQuizRank(_hostingEnvironment); ;
            return View(data);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Intro()
        {
            return View();
        }

        public async Task<IActionResult> DetailView()
        {

            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

            ManageEnglishBibleQuiz manageEnglishBibleQuiz = new ManageEnglishBibleQuiz();

            DateTime currentDate = SharedComm.GetToday();

            int Id = manageEnglishBibleQuiz.getClickHistoryForEnglishBibleQuiz(UserName, currentDate);
            
            if(Id> 0)
            {
                ViewBag.MissionMessage1 = "You’ve already completed today’s mission.";
                ViewBag.MissionMessage2 = "Try again tomorrow!";
            }
            else
            {
                ViewBag.MissionMessage1 = string.Empty;
                ViewBag.MissionMessage2 = string.Empty;
            }

            List<EnglishBibleQuiz> data = null; //;manageEnglishBibleQuiz.getEnglishBibleQuiz(userID, Id);
            if (data.Count == 0)
                return RedirectToAction(nameof(LearningCenterController.Index), controllName);
            return View(data[0]);
        }

        public async Task<IActionResult> AnswerQuestion(int Id,string selectedOption)
        {
            ManageEnglishBibleQuiz manageEnglishBibleQuiz = new ManageEnglishBibleQuiz();
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;
            manageEnglishBibleQuiz.CreateClickHistoryForEnglishBibleQuiz(UserName, Id, selectedOption);

            ViewBag.MissionSuccess = manageEnglishBibleQuiz.getEnglishBibleQuizSuccessCount(UserName, true, Id);
            return View("PassFailResult");
        }

        public async Task<IActionResult> DeleteView(int Id)
        {
            ManageOpic manageOpic = new ManageOpic();
            manageOpic.DeleteOpic(Id);
            return RedirectToAction(nameof(LearningCenterController.List), controllName);
        }

        public async Task<IActionResult> EditView(int Id)
        {
            ManageOpic manageOpic = new ManageOpic();
            var data = manageOpic.getOpic(Id, string.Empty, string.Empty, string.Empty);
            return View(data[0]);
        }

        public async Task<IActionResult> EditViewProcess(OpicData param)
        {
            ManageOpic manageOpic = new ManageOpic();
            manageOpic.UpdateOpic(param);
            return RedirectToAction(nameof(LearningCenterController.DetailView), controllName, new { Id = param.Id });
        }
        public async Task<IActionResult> EnrollView()
        {
            OpicData OpicData = new OpicData();
            return View(OpicData);
        }


        public async Task<IActionResult> EnrollViewProcess(OpicData param)
        {
            ManageOpic manageOpic = new ManageOpic();
            manageOpic.CreateOpic(param);
            return RedirectToAction(nameof(LearningCenterController.List), controllName);
        }

        // GET: JobCategory
        public async Task<IActionResult> List(string sortBy, string query)
        {
            ManageOpic manageOpic = new ManageOpic();

            return View(manageOpic.getOpic(0, string.Empty, sortBy, query)); 
        }

        public async Task<IActionResult> MarkView(int Id)
        {
            ManageOpic manageOpic = new ManageOpic();
            manageOpic.WorkVisitedOpic(Id);
            return RedirectToAction(nameof(LearningCenterController.List), controllName);
        }

        // GET: JobCategory
        public async Task<IActionResult> SearchQuery(string query)
        {
            return RedirectToAction(nameof(LearningCenterController.List), controllName, new { query = query });
        }


    }
}



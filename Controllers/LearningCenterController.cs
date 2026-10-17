using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class LearningCenterController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public LearningCenterController(IAuthService authService,
            UserManager<ApplicationUser> userManager) : base(authService, userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }
        // GET: JobCategory
        public async Task<IActionResult> Index()
        {
            return View();
        }


        public async Task<IActionResult> DetailView(int Id, string ExposeDate)
        {

            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";
            if (Id > 0)
            {
            }
            else
            {
                if (string.IsNullOrEmpty(ExposeDate))
                {
                    ExposeDate = SharedComm.GetTodayString();
                }
            }
            ManageOpic manageOpic = new ManageOpic();
            var data = manageOpic.getOpic(Id, ExposeDate, string.Empty, string.Empty);

            if (data.Count == 0)
                return RedirectToAction(nameof(LearningCenterController.List), "LearningCenter");

            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

            if (!SharedComm.isAdmin(UserName))
            {
                manageOpic.VisitedOpic(data[0].Id);
            }


            return View(data[0]);

        }


        public async Task<IActionResult> DeleteView(int Id)
        {
            ManageOpic manageOpic = new ManageOpic();
            manageOpic.DeleteOpic(Id);
            return RedirectToAction(nameof(LearningCenterController.List), "LearningCenter");
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
            return RedirectToAction(nameof(LearningCenterController.DetailView), "LearningCenter", new { Id = param.Id });
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
            return RedirectToAction(nameof(LearningCenterController.List), "LearningCenter");
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
            return RedirectToAction(nameof(LearningCenterController.List), "LearningCenter");
        }

        // GET: JobCategory
        public async Task<IActionResult> SearchQuery(string query)
        {
            return RedirectToAction(nameof(LearningCenterController.List), "LearningCenter", new { query = query });
        }


    }
}



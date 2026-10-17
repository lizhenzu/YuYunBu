using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class WeeklyReportController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public WeeklyReportController(IAuthService authService,
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
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;
            var UserId = HttpContext.Session.GetString(HttpContextSessionCategory.UserId.ToString()) as string;            

            TodayCollectWeeklyWordToRead todayCollectWeeklyWordToRead = SharedComm.ReadTodayWeeklyWordToReadMission(Category.All, 0, UserName, GetWorkDT(), "ko", UserId, DateHelper.GetKoreaToday().Year);

            return View(todayCollectWeeklyWordToRead);
        }        // GET: JobCategory
    
        public async Task<IActionResult> IndexByID(int Id)
        {
            var UserId = HttpContext.Session.GetString(HttpContextSessionCategory.UserId.ToString()) as string;
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());

            TodayCollectWeeklyWordToRead todayCollectWeeklyWordToRead = SharedComm.ReadTodayWeeklyWordToReadMission(Category.All, Id, UserName, GetWorkDT(),"ko", UserId, DateHelper.GetKoreaToday().Year);
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            HttpContext.Session.SetString(HTTPSessionKey.WeeklyWordToReadID.ToString(), Id.ToString());
             
            if (!SharedComm.isAdmin(UserName))
            {
                manageWeeklyWordToRead.HitWeeklyWordToRead(Id);
            }
             

            return View(todayCollectWeeklyWordToRead);
        }

             public async Task<IActionResult> IndexByID_3(int Id)
        {
            var UserId = HttpContext.Session.GetString(HttpContextSessionCategory.UserId.ToString()) as string;
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());

            TodayCollectWeeklyWordToRead todayCollectWeeklyWordToRead = SharedComm.ReadTodayWeeklyWordToReadMission(Category.All, Id, UserName, GetWorkDT(),"ko", UserId, DateHelper.GetKoreaToday().Year);
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            HttpContext.Session.SetString(HTTPSessionKey.WeeklyWordToReadID.ToString(), Id.ToString());
             
            if (!SharedComm.isAdmin(UserName))
            {
                manageWeeklyWordToRead.HitWeeklyWordToRead(Id);
            }
             

            return View(todayCollectWeeklyWordToRead);
        }
        public JsonResult GetDetails(string bibleText , int  fromJang  , int fromZul  , int toJang , int toZul)
        { 
            ManageeBible manageeBible = new ManageeBible();
            var UserId = HttpContext.Session.GetString(HttpContextSessionCategory.UserId.ToString()) as string;            
            return Json(manageeBible.geteBible2(bibleText, fromJang, fromZul, toJang, toZul,"ko", UserId));
        }
        private DateTime GetWorkDT()
        {
            DateTime workDT = SharedComm.GetToday();
            var workDTInside = HttpContext.Session.GetString(DateKind.WorkDT.ToString());
            if (workDTInside != null)
            {
                workDT = DateTime.Parse(workDTInside);
            }

            return workDT;
        }
 

    }
}
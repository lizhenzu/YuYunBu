using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
   
    public class PrayHomeController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public PrayHomeController(IAuthService authService,
                IWebHostEnvironment hostingEnvironment,
            UserManager<ApplicationUser> userManager) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<IActionResult> Index()
		{
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";
            return View();
        }

        public async Task<IActionResult> Branch()
        {
            return View();
        }
        public async Task<IActionResult> Intercessory()
        {
            return View();
        }
        public async Task<IActionResult> coClass()
        {
            return View();
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> TodayPray()
        {

            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";

            ManagePray _ManagePray = new ManagePray();

            List<TodayPray> ddd = _ManagePray.TodayPray(true);

            return View(ddd[0]);
        }

        [AllowAnonymous]
        public async Task<IActionResult> TodayPrayList()
        {

            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";

            ManagePray _ManagePray = new ManagePray();

            List<TodayPray> ddd = _ManagePray.TodayPray(false);

            return View(ddd);
        }



        // GET: PrayHome/Edit/1
        public async Task<IActionResult> ModifyPray(int PrayID)
        {
            ManagePray _ManagePray = new ManagePray();

            List<TodayPray> ddd = _ManagePray.TodayPrayByID(PrayID);

            return View(ddd[0]);
        }


        [HttpPost]
        public async Task<IActionResult> ModifyPrayProcess(TodayPray model)
        {
            ManagePray _ManagePray = new ManagePray();

            _ManagePray.UpdateTodayPray(model.PrayID, model.Subject, model.PrayTEXTData, model.BibleText, model.BibleTextData, model.publishYN);
        
            return RedirectToAction("TodayPrayList"); // 기도 목록 페이지
        }

        public async Task<IActionResult> PrayBan()
        {
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";
            UserInfo _UserInfos = null;

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
                ManageHumanResource manageHumanResource = new ManageHumanResource();

                List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

                if (userInfo.Count > 0)
                {
                    HttpContext.Session.SetString(HttpContextSessionCategory.DeptCode.ToString(), userInfo[0].DeptCode);
                    HttpContext.Session.SetString(HttpContextSessionCategory.DeptNM.ToString(), userInfo[0].DeptNM);
                    _UserInfos = ViewBag.UserInfos = userInfo[0];
                }

            }

            if (_UserInfos != null)
            {
                ManagePray panagePray = new ManagePray();
                List<UserInfosPray> userInfosPray = panagePray.getPrayHistory(_hostingEnvironment, _UserInfos.ClassYear, _UserInfos.ClassBan, UserId);
                return View(userInfosPray);
            }
            else
            {
                return View();
            }

        }

        public async Task<IActionResult> CreatePrayHistory(string personPraying, string prayerRecipient)
        {
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";

            ManagePray panagePray = new ManagePray();
            panagePray.CreatePrayHistory(personPraying, prayerRecipient);

            return RedirectToLocal("~/PrayHome/PrayBan");
        }

        [AllowAnonymous]
        public async Task<IActionResult> WhatTick()
        {
            return View();
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



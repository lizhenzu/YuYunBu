using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class RhapsodyOfRealityController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public RhapsodyOfRealityController(IAuthService authService,
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

        public void GetLangCDMethod()
        {
            ManageeCommon manageeCommon = new ManageeCommon();

            List<UserSettings> va = manageeCommon.fetchUserSettings(UserId);
            if (va != null)
            {
                if (va.Count > 0)
                {
                     if(string.IsNullOrEmpty(va[0].LangCD))
                    {
                        va[0].LangCD = "ko";
					}
                    HttpContext.Session.SetString(HttpContextSessionCategory.LangCD.ToString(), va[0].LangCD);
                    HttpContext.Session.SetString(HttpContextSessionCategory.PaesForView.ToString(), va[0].PagesForView.ToString());
                }
            }
            else
            {
                HttpContext.Session.SetString(HttpContextSessionCategory.LangCD.ToString(), "ko");
                HttpContext.Session.SetString(HttpContextSessionCategory.PaesForView.ToString(), "100");
            }
        }

        public async Task<IActionResult> DetailWords(int Id, string ExposeDate, string methodTEXT)
        {
          
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }


            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";
            ViewBag.methodTEXT = methodTEXT;
            ManageRhapsodyOfReality manageRhapsodyOfReality = new ManageRhapsodyOfReality();

            if (Id > 0)
            {
            }
            else
            {
                if (string.IsNullOrEmpty(ExposeDate))
                {
                //   ExposeDate = SharedComm.GetTodayString();
                }
            }

            var data = manageRhapsodyOfReality.getRhapsodyOfReality1(Id, ExposeDate);

            if(data.Count == 0)
                return RedirectToAction(nameof(RhapsodyOfRealityController.DetailList), "RhapsodyOfReality");

            var userFullName = HttpContext.Session.GetString(HttpContextSessionCategory.UserFullName.ToString()) as string;
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;
            var UserId = HttpContext.Session.GetString(HttpContextSessionCategory.UserId.ToString()) as string;

            if (!SharedComm.isAdmin(UserName))
            {
                manageRhapsodyOfReality.HitRhapsodyOfReality(data[0].Id);
            }             
             
            ManageeBible manageeBible = new ManageeBible();
            
            RhapsodyOfRealityComposite rhapsodyOfRealityCompositeData = new RhapsodyOfRealityComposite();

            List<EveryBibles> EveryBiblesList =  manageRhapsodyOfReality.getEveryBible(data[0].ExposeDate);

            
            foreach (var va in EveryBiblesList)
            {
                eBibles da = manageeBible.geteBible2(va.fullWord, va.chapter, va.startVerse, va.chapter, va.endVerse,"ko" , UserId);
                rhapsodyOfRealityCompositeData.eBibleDatas.Add(da);
            }

            rhapsodyOfRealityCompositeData.RhapsodyOfRealitysData = data[0];

            ViewBag.DisableLangTransBtton = "y";

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());


            GetLangCDMethod();

            return View(rhapsodyOfRealityCompositeData);        
        }
        public async Task<IActionResult> DetailWordsP(int Id, string ExposeDate, string methodTEXT)
        {

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }


            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";
            ViewBag.methodTEXT = methodTEXT;
            ManageRhapsodyOfReality manageRhapsodyOfReality = new ManageRhapsodyOfReality();

            if (Id > 0)
            {
            }
            else
            {
                if (string.IsNullOrEmpty(ExposeDate))
                {
                    //   ExposeDate = SharedComm.GetTodayString();
                }
            }

            var data = manageRhapsodyOfReality.getRhapsodyOfReality1(Id, ExposeDate);

            if (data.Count == 0)
                return RedirectToAction(nameof(RhapsodyOfRealityController.DetailList), "RhapsodyOfReality");

            var userFullName = HttpContext.Session.GetString(HttpContextSessionCategory.UserFullName.ToString()) as string;
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;
            var UserId = HttpContext.Session.GetString(HttpContextSessionCategory.UserId.ToString()) as string;

            if (!SharedComm.isAdmin(UserName))
            {
                manageRhapsodyOfReality.HitRhapsodyOfReality(data[0].Id);
            }

            ManageeBible manageeBible = new ManageeBible();

            RhapsodyOfRealityComposite rhapsodyOfRealityCompositeData = new RhapsodyOfRealityComposite();

            List<EveryBibles> EveryBiblesList = manageRhapsodyOfReality.getEveryBible(data[0].ExposeDate);


            foreach (var va in EveryBiblesList)
            {
                eBibles da = manageeBible.geteBible2(va.fullWord, va.chapter, va.startVerse, va.chapter, va.endVerse, "ko", UserId);
                rhapsodyOfRealityCompositeData.eBibleDatas.Add(da);
            }

            rhapsodyOfRealityCompositeData.RhapsodyOfRealitysData = data[0];

            ViewBag.DisableLangTransBtton = "y";

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());


            GetLangCDMethod();

            return View(rhapsodyOfRealityCompositeData);
        }

        public async Task<IActionResult> DeleteView(int Id)
        {
            ManageRhapsodyOfReality manageRhapsodyOfReality = new ManageRhapsodyOfReality();
 
            manageRhapsodyOfReality.DeleteRhapsodyOfReality(Id);
            return RedirectToAction(nameof(RhapsodyOfRealityController.DetailList), "RhapsodyOfReality");
        }

        public async Task<IActionResult> ModifyView(int Id)
        {
            ManageRhapsodyOfReality manageRhapsodyOfReality = new ManageRhapsodyOfReality();
             
            var data = manageRhapsodyOfReality.getRhapsodyOfReality1(Id, string.Empty);
            return View(data[0]);
        }

        public async Task<IActionResult> EnrollView()
        {
            RhapsodyOfReality rhapsodyOfReality = new RhapsodyOfReality();
            rhapsodyOfReality.ExposeDate = SharedComm.GetTodayString();
            return View(rhapsodyOfReality);
        }


        public async Task<IActionResult> ModifyProcess(RhapsodyOfReality param)
        {
            ManageRhapsodyOfReality manageRhapsodyOfReality = new ManageRhapsodyOfReality();
            manageRhapsodyOfReality.UpdateRhapsodyOfReality(param);
            return RedirectToAction(nameof(RhapsodyOfRealityController.DetailList), "RhapsodyOfReality");
        }

        public async Task<IActionResult> EnrollProcess(RhapsodyOfReality param)
        {
            ManageRhapsodyOfReality manageRhapsodyOfReality = new ManageRhapsodyOfReality();
            manageRhapsodyOfReality.CreateRhapsodyOfReality(param);

            return RedirectToAction(nameof(RhapsodyOfRealityController.DetailWords), "RhapsodyOfReality");
        }

         
        public async Task<IActionResult> DetailList()
        {
            ManageRhapsodyOfReality manageRhapsodyOfReality = new ManageRhapsodyOfReality();

            return View(manageRhapsodyOfReality.getRhapsodyOfReality1(0, string.Empty));
        } 

    }
}



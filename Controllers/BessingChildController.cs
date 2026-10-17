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
    public class BessingChildController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public BessingChildController(IAuthService authService,
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
 
            return View();
        }
        public async Task<IActionResult> DetailWords(int Id, string ExposeDate, string forWhom)
        {
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";

            ManageBessingChild manageBessingChild = new ManageBessingChild();

            //if (Id > 0)
            //{
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(ExposeDate))
            //    {
            //       ExposeDate = SharedComm.GetTodayString();
            //    }
            //}

            var data = manageBessingChild.getBessingChild(Id, ExposeDate);

            if (data.Count == 0)
                return RedirectToAction(nameof(BessingChildController.DetailList), "BessingChild");
                        
            var userFullName = HttpContext.Session.GetString(HttpContextSessionCategory.UserFullName.ToString()) as string;
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

            if (!SharedComm.isAdmin(UserName))
            {
                manageBessingChild.HitBessingChild(data[0].Id);
            }

            //if (!string.IsNullOrEmpty(forWhom))
            //{
            //    string stringData = data[0].PrayText;


            //    // 앞 글자 제거 ("홍길동" -> "길동")
            //    string replacedPart = forWhom.Substring(1);

            //    // "**길동**(이)" 로 변환
            //    string formatted = $"**{replacedPart}**(이)";

            //    // stringData 안의 "00(이)" 부분을 교체
            //    string result = stringData.Replace("00(이)", formatted);

            //    data[0].PrayText = result;
            //}


            if (!string.IsNullOrWhiteSpace(forWhom))
            {
                var name = forWhom.Trim();

                // 길이가 3자 미만이면 아무 것도 하지 않음
                if (name.Length >= 3)
                {
                    string stringData = data[0].PrayText ?? string.Empty;

                    // 앞 글자 제거 ("홍길동" -> "길동")
                    string replacedPart = name.Substring(1);

                    // "**길동**(이)" 로 변환
                    string formatted = $"**{replacedPart}**(이)";

                    // stringData 안의 "00(이)" 부분을 교체
                    data[0].PrayText = stringData.Replace("00(이)", formatted);
                }
                else  
                {
                    string stringData = data[0].PrayText ?? string.Empty;

                    // 앞 글자 제거 ("홍길동" -> "길동")
                    string replacedPart = name;

                    // "**길동**(이)" 로 변환
                    string formatted = $"**{replacedPart}**(이)";

                    // stringData 안의 "00(이)" 부분을 교체
                    data[0].PrayText = stringData.Replace("00(이)", formatted);
                }
            }

            return View(data[0]);
        
        }

        public async Task<IActionResult> DeleteView(int Id)
        {
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";

            ManageBessingChild manageBessingChild = new ManageBessingChild();

            manageBessingChild.DeleteBessingChild(Id);
            return RedirectToAction(nameof(BessingChildController.DetailList), "BessingChild");
        }

        public async Task<IActionResult> DecreasetHitWeeklyWordToRead(int Id)
        {
            ManageBessingChild manageBessingChild = new ManageBessingChild();

            manageBessingChild.DecreasetHitWeeklyWordToRead(Id);
            return RedirectToAction(nameof(BessingChildController.DetailList), "BessingChild");
        }

        public async Task<IActionResult> ModifyView(int Id)
        {
            ManageBessingChild manageBessingChild = new ManageBessingChild();
             
            var data = manageBessingChild.getBessingChild(Id, string.Empty);
            return View(data[0]);
        }

        public async Task<IActionResult> EnrollView()
        {
            BessingChildData bessingChildData = new BessingChildData();
            bessingChildData.FromDate = SharedComm.GetTodayString();
            bessingChildData.ToDate   = SharedComm.GetTodayString(3);
            return View(bessingChildData);
        }


        public async Task<IActionResult> ModifyProcess(BessingChildData param)
        {
            ManageBessingChild manageBessingChild = new ManageBessingChild();
            manageBessingChild.UpdateBessingChild(param);
            return RedirectToAction(nameof(BessingChildController.DetailWords), "BessingChild", new {Id = param.Id });
        }

        public async Task<IActionResult> EnrollProcess(BessingChildData param)
        {
            ManageBessingChild manageBessingChild = new ManageBessingChild();
            manageBessingChild.CreateBessingChild(param);

            return RedirectToAction(nameof(BessingChildController.DetailList), "BessingChild");
        }

         
        public async Task<IActionResult> DetailList()
        {
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";

            ManageBessingChild manageBessingChild = new ManageBessingChild();

            return View(manageBessingChild.getBessingChild(999, string.Empty));
        } 
    }
}



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
using Microsoft.CodeAnalysis; 
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize]
    public class CheckPointController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly BulletinBoard.Services.GiftCatalog.IGiftCatalogService _giftCatalog;   // ✅ 선물 품목 조회

        public CheckPointController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
                 IWebHostEnvironment hostingEnvironment,
                 BulletinBoard.Services.GiftCatalog.IGiftCatalogService giftCatalog) : base(authService, userManager, hostingEnvironment)
        {
            _giftCatalog = giftCatalog;
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: JobCategory
        public async Task<IActionResult> Index()
        {
            string userName = string.Empty;

            if(await _authService.IsSignedIn(User))
            {                
                userName = await _authService.GetUserName(User);
            } 

            return View();
        }

		public async Task<IActionResult> GetGroupByUsernameDataByClass()
		{
            int ClassYear = 0;
            int ClassBan = 0;
			await SetDept();
			ManageCheckPoint manageCheckPoint = new ManageCheckPoint();

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();

                ManageHumanResource manageHumanResource = new ManageHumanResource();

                List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);
                if (userInfo != null)
                {
                    ClassYear = userInfo[0].ClassYear;
                    ClassBan = userInfo[0].ClassBan;
                }
            }
            return View("CheckPointDetailList", manageCheckPoint.getGroupByUsernameDataByClass(ClassYear, ClassBan, "DC_Total", _hostingEnvironment));
		}

		public async Task<IActionResult> ChampionList()
        {
            await SetDept();

             ManageCheckPoint manageCheckPoint = new ManageCheckPoint();
            RankQueryOption groupNameEnum = RankQueryOption.None;
            var DeptCode = HttpContext.Session.GetString(HttpContextSessionCategory.DeptCode.ToString()) as string;
            return View(manageCheckPoint.getGroupByUsernameData(groupNameEnum, string.Empty, "DC_Total", DeptCode, _hostingEnvironment));
        }

        // 퀴즈 답변 정정(목록 · 보정)은 SuperAdmin 전용 — 2026-10-04
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> MismatchClickHistoryForQuestion()
        {
            await SetDept();

            ManageClickHistoryForQuestion manageClickHistoryForQuestion = new ManageClickHistoryForQuestion();
            return View(manageClickHistoryForQuestion.MismatchClickHistoryForQuestion());
        }

        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> CorrectMismatchClickHistoryForQuestion(int Id, int QuestionId, string UserName)
        {
            ManageClickHistoryForQuestion manageClickHistoryForQuestion = new ManageClickHistoryForQuestion();
            manageClickHistoryForQuestion.CorrectMismatchClickHistoryForQuestion(Id, QuestionId, UserName);
            return RedirectToLocal("~/CheckPoint/MismatchClickHistoryForQuestion");
        }

        [HttpPost]
        public async Task<IActionResult> updateAwardPoint([FromBody] AwardUpdateData param)
        {

            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();

            manageCheckPoint.SetAwardData(param.UserName, param.Value);

            //return View("TodayWordToReadResult", todayCheckCompositeData);
            return Json("");

        }

         
        public async Task<IActionResult> GetThisWeekRewardingAward(string viewOption)
        {
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";

            await SetDept();

            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();
            var DeptCode = HttpContext.Session.GetString(HttpContextSessionCategory.DeptCode.ToString()) as string;

            DeptCode = SharedComm.DefaultDeptCode(DeptCode);

            ViewBag.viewOption = viewOption;

            return View(manageCheckPoint.getGetThisWeekRewardingByAward(DeptCode, _hostingEnvironment));
        }



        public async Task<IActionResult> AwardItemList(string option, int myPoint, int ProductID = 0)  //금주 시상
        {
            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();

            
            List<PointByUserName> _PointByUserName = null;

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
                ManageHumanResource manageHumanResource = new ManageHumanResource();

                List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

                ViewData["userInfo"] = userInfo[0];

                var DeptCode = userInfo[0].DeptCode;
                _PointByUserName = manageCheckPoint.getGetThisWeekRewarding(DeptCode, _hostingEnvironment);

            }

            if(string.IsNullOrEmpty(option))
            {
                option = "none";
            } 
             

            var userData = _PointByUserName.FirstOrDefault(p => p.UserId == UserId);

            int tempPoint = (option == "choice" && userData != null) ? userData.DC_Total : 0;

      
            int tempPoint2 = (option != "a" && userData != null) ? userData.Total : 0;
 
            if (tempPoint > 0)
            {
                option = "choice";
            }

            List<AwardBase> _AwardBase = manageCheckPoint.getAwardBase();

            ViewBag._AwardBase = _AwardBase;

     

            ProductDisplayOpton productDisplayOpton;

            if (Enum.TryParse(option, out productDisplayOpton))
            {
               // await DbLog1ger.LogInfoAsync($"before2 : {option} {productDisplayOpton.ToString()}");
            }
            else
            {
                productDisplayOpton = ProductDisplayOpton.none;
              //  await DbLogg11er.LogInfoAsync($"before3 : {option} {productDisplayOpton.ToString()}");
            }

           // await DbLogg1er.LogInfoAsync($"before4 : {option} {productDisplayOpton.ToString()}");

            ViewBag.option = productDisplayOpton.ToString();

            // ProductID 가 있으면 그 한 건만 (옵션·점수는 무시)
            var awardItems = ProductID > 0
                ? await _giftCatalog.GetAsync(BulletinBoard.Services.GiftCatalog.GiftCatalogQuery.ById(ProductID, includeHidden: true))
                : await _giftCatalog.GetAsync(new BulletinBoard.Services.GiftCatalog.GiftCatalogQuery
                  {
                      Option  = productDisplayOpton,
                      MyPoint = tempPoint2
                  });

            return View(awardItems);
        }



        public async Task<IActionResult> AwardItemPick(string UserId)  //금주 시상
        {
            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();
            List<AwardItemPick> data = manageCheckPoint.getAwardItemPick(UserId);
            return View(data[0]);
        }



        public async Task<IActionResult> UpdateAwardItem(int ProductID = 0)  //금주 시상
        {

            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();

            var item = await _giftCatalog.GetByIdAsync(ProductID, includeHidden: true);
            if (item == null) return RedirectToAction("AwardItemList");
            return View(item);
        }


        public async Task<IActionResult> UpsertAwardItemPick(string userId, int productId)
        {

            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();

            manageCheckPoint.UpsertAwardItemPick(userId, productId);
            List<AwardItemByUser> data = manageCheckPoint.getAwardItemPickAll(UserId);

            return View("AwardItemPick", data[0]);

        }

		public async Task<IActionResult> MyAwardItemPick()
		{

			ManageCheckPoint manageCheckPoint = new ManageCheckPoint();


			if (await _authService.IsSignedIn(User))
			{
				var result = await SetUserInfo();
			}

			List<AwardItemByUser> _AwardItemByUser = manageCheckPoint.getAwardItemPickAll(UserId);

            var __AwardItemByUser = _AwardItemByUser.FirstOrDefault(p => p.UserId == UserId);

            return View("AwardItemPick", __AwardItemByUser);

		}

		[HttpPost]
        public async Task<IActionResult> ProcessUpdateAwardItem(AwardItem param)
        {

            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();
            List<AwardBase> _AwardBase = manageCheckPoint.getAwardBase();
            ViewBag._AwardBase = _AwardBase;

            manageCheckPoint.UpdateAwardItem(param);
            var displayItems = await _giftCatalog.GetAsync(new BulletinBoard.Services.GiftCatalog.GiftCatalogQuery
            {
                Option = ProductDisplayOpton.display
            });
            return View("AwardItemList", displayItems); 

        }

        public async Task<IActionResult> ManagePoint(string UserId, string rankQueryOption = "TeacherOnly", string orderBy = "DC_Total")
        {
            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();
             
            RankQueryOption groupNameEnum = (RankQueryOption)Enum.Parse(typeof(RankQueryOption), "TeacherOnly");
            var DeptCode = HttpContext.Session.GetString(HttpContextSessionCategory.DeptCode.ToString()) as string;
            List<AwardBase> data = manageCheckPoint.getAwardBase();
            ViewBag._AwardBase = data;

            return View(manageCheckPoint.getGroupByUsernameData(groupNameEnum, UserId, orderBy, DeptCode, _hostingEnvironment));
        }

        public async Task<IActionResult> SetPoint(string userName, int value)
        {
            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();

            manageCheckPoint.SetAwardData(userName, value);
            return RedirectToLocal("~/Admin/IndexPoint");
        }


        public async Task<IActionResult> CheckPointDetailList2(string rankQueryOption = "TeacherOnly", string orderBy = "DC_Total")  //학생만 , 교사만
        {
            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();
            ViewBag.Kind = rankQueryOption;
            RankQueryOption groupNameEnum = (RankQueryOption)Enum.Parse(typeof(RankQueryOption), rankQueryOption);
            var DeptCode = HttpContext.Session.GetString(HttpContextSessionCategory.DeptCode.ToString()) as string;
            ViewBag.RankQueryOption = groupNameEnum;


            List<AwardBase> _AwardBase = manageCheckPoint.getAwardBase();

            var maxAward = _AwardBase
                        .OrderByDescending(a => a.AwardLevel)
                        .FirstOrDefault();

            if (maxAward != null)
            {
                ViewBag.maxAwardLevel = maxAward.AwardLevel;
            }
            else
            {
                ViewBag.maxAwardLevel = 1000;
            }

            return View("CheckPointDetailList2", manageCheckPoint.getGroupByUsernameData(groupNameEnum, string.Empty, orderBy, DeptCode, _hostingEnvironment));
        }








        public async Task<IActionResult> ChangeCheckPoint(string userName, string optionData, int value=100)
        {
            ManageCheckPoint manageCheckPoint = new ManageCheckPoint();
            manageCheckPoint.UpOrDownAwardData(userName, optionData, value);

            return RedirectToLocal("~/Admin/IndexPoint"); 
        }
        //public async Task<IActionResult> CheckPointDown(string userName)
        //{
        //    ManageCheckPoint manageCheckPoint = new ManageCheckPoint();
        //    manageCheckPoint.UpOrDownAwardData(userName, "down");

        //    return RedirectToLocal("~/CheckPoint/CheckPointDetailStudentOnlyListAwardLevel");
        //}
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public async Task<IActionResult> SetDept()
        {
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();

                //ManageHumanResource manageHumanResource = new ManageHumanResource();

                //List<UserInfos> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

                //if (userInfo.Count > 0)
                //{
                //    HttpContext.Session.SetString(HttpContextSessionCategory.DeptCode.ToString(), userInfo[0].DeptCode);
                //    HttpContext.Session.SetString(HttpContextSessionCategory.DeptNM.ToString(), userInfo[0].DeptNM);
                //}
            }

            // 기본 가짜 데이터 반환
            return Json(new { success = false, message = "Set Dept." });

        }
    }
}

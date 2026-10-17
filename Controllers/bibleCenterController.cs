using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize]
    public class bibleCenterController : BaseController
    {
        //private readonly IAuthService _authService;
        //private readonly UserManager<ApplicationUser> _userManager;
        //private readonly IWebHostEnvironment _hostingEnvironment;
        //public bibleCenterController(IAuthService authService,
        //    UserManager<ApplicationUser> userManager) : base(authService, userManager)
        //{
        //    _authService = authService;
        //    _userManager = userManager;
        //}

        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public bibleCenterController(IAuthService authService,
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
            ViewBag.ab = "";
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            return View(manageeBulkInputBible.getBibleYouTuBuEnrollList(string.Empty, UserId));
        }
        // GET: JobCategory
        [Authorize]
        public async Task<IActionResult> DoneReadBibleOneVerse(string LangCD, int TopValue, int CurrntBibleID, int CurrntBibleIDLoop, string UserName)
        {
            string userName = "guest";
            if (await _authService.IsSignedIn(User))
            {
                userName = await _authService.GetUserName(User);
            }

            ManageeBible _ManageeBible = new ManageeBible();
            _ManageeBible.UpsertBibleStampRange(CurrntBibleID, CurrntBibleIDLoop, userName);

            return RedirectToAction(nameof(bibleCenterController.ReadingTheBibleOneVerse), "bibleCenter" ,new { LangCD  = LangCD , TopValue  = TopValue });
        }

        public async Task<IActionResult> MyDashboard(string userId)
        {

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();

                if(!string.IsNullOrEmpty(userId))
                {
                    UserId = userId;
                }                 

                ManageReadBible manageReadBible = new ManageReadBible();
                List<Testaments> Testamentss = manageReadBible.GetBibleReadingStats(UserId);
                List<TestamentReadSpeed> TestamentReadSpeeds = manageReadBible.GetReadingProgressEstimate(UserId);

                List<BibleLastReadInfo> BibleLastReadInfos = manageReadBible.GetLastReadVerse(UserId , 0 , 0);

                if (BibleLastReadInfos.Count > 0)
                {
                    ViewBag.BibleLastReadInfo = BibleLastReadInfos[0];
                }
                else
                {
                    ViewBag.BibleLastReadInfo = null;
                }

                ViewBag.LastReadDate = manageReadBible.GetLastReadDate(UserId);
                ViewBag.TotalProgressRate = manageReadBible.GetTotalBibleProgressRate(UserId);

                if (Testamentss.Count > 0)
                {
                    ViewBag.Testament = Testamentss[0];
                }
                else
                {
                    ViewBag.Testament = null;
                }


                if (TestamentReadSpeeds.Count > 0)
                {
                    ViewBag.TestamentReadSpeed = TestamentReadSpeeds[0];
                }
                else
                {
                    ViewBag.Testament = null;
                }

                ManageHumanResource manageHumanResource = new ManageHumanResource();
                List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

                if (userInfo.Count > 0)
                {
                    ViewData["ProfilePicturePath"] = userInfo[0].ProfilePicturePath; 
                    ViewData[HttpContextSessionCategory.UserId.ToString()] = userInfo[0].UserId;
                    ViewData[HttpContextSessionCategory.UserFullName.ToString()] = userInfo[0].FullName;
                }
                else
                {
                    ViewData["ProfilePicturePath"] = ConstatsCommon.representFaceImage;
                    ViewData["UserId"] = UserId;
                }

                return View(manageReadBible.GetUnfinishedBooks(UserId, string.Empty));

            }



            return View();
        }



        public async Task<IActionResult> bibleBookMarkDetail(int VerseId)
        {

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());


            ManageeBible _manageeBible = new ManageeBible();

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();

            }
            List<eBibleBase> va = _manageeBible.getBookMark(this.UserId,  VerseId);

            return View(va[0]);
        }

        public async Task<IActionResult> bibleBookMark()
        {

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());


            ManageeBible _manageeBible = new ManageeBible();

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();

            }
            List<eBibleBase> va = _manageeBible.getBookMark(this.UserId, 0);

            return View(va);
        }


        public async Task<IActionResult> DeleteBookMark(string VerseId)
        {
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();

            }
            ManageeBible _manageeBible = new ManageeBible();
            _manageeBible.deleteBookMark(this.UserId, VerseId);
            List<eBibleBase> va = _manageeBible.getBookMark(this.UserId, 0);
            return View("bibleBookMark", va);
        }

        public async Task<IActionResult> MyDashboardList(string bilbeProcessOption)
        {
            ManageReadBible manageReadBible = new ManageReadBible();

            BilbeProcessOption groupNameEnum = BilbeProcessOption.TotalProgressRate;

            if (!string.IsNullOrEmpty(bilbeProcessOption))
            {
                groupNameEnum = (BilbeProcessOption)Enum.Parse(typeof(BilbeProcessOption), bilbeProcessOption);
            }

            List<BibleLastReadRateByUserSimple> BibleLastReadInfos = manageReadBible.GetUserTotalProgressRatesByUser(_hostingEnvironment, groupNameEnum);

            return View(BibleLastReadInfos);
        }
        [AllowAnonymous]
        public async Task<IActionResult> MyDashboardList2()
        {
            ManageReadBible manageReadBible = new ManageReadBible();
            List<BibleLastReadRateByUserCompleted> BibleLastReadInfos = manageReadBible.GetUserTotalProgressRatesByUserCompleted(_hostingEnvironment);

            return View(BibleLastReadInfos);
        }

        public async Task<IActionResult> BibleOneList(string BibleText)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            return View(manageeBulkInputBible.getBibleMissingListB_KO_EN_SU());
        }

        public async Task<IActionResult> BibleAllList()
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            return View(manageeBulkInputBible.getBibleMissingListB_KO_EN_SU());
        }

        public UserSettings SetLangCDMethod(string UserId, string LangCDMethod, int paesForView)
        {
            ManageeCommon manageeCommon = new ManageeCommon();

            if (!string.IsNullOrEmpty(LangCDMethod))
            {
                manageeCommon.UpsertUserSettings(UserId, langCD:LangCDMethod);
            }
            if (paesForView > 0)
            {
                manageeCommon.UpsertUserSettings(UserId, pagesForView:paesForView);
            }

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
                    return va[0];
                }
            }
            return new UserSettings() { LangCD = "ko", PagesForView = 5 };
        }

        // GET: JobCategory
        [Authorize]
        public async Task<IActionResult> ReadingTheBibleOneVerse(int BibleBookSerialId, int BibleId, int Jang, string LangCD, int PagesForView, string MoveDirection  ,string BibleText)
        {       
            ViewBag.ab = ""; 

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            ManageReadBible manageReadBible = new ManageReadBible();

            if (!string.IsNullOrEmpty(BibleText) && Jang > 0)
            {
                ManageeBible manageeBible = new ManageeBible();
                List<eBibleBase>  data1 = manageeBible.getBibleBaseJang(BibleText, Jang);

                int min = data1[0].Id;
                int max = data1[data1.Count - 1].Id;            

                List<BibleLastReadInfo> BibleLastReadInfos = manageReadBible.GetLastReadVerse(UserId, min, max);

                if (BibleLastReadInfos.Count > 0)
                {
                    BibleBookSerialId = BibleLastReadInfos[0].BibleBookSerialId;
                }
                else
                {
                    BibleBookSerialId = min;
                }
            }

            if (BibleBookSerialId == 0)
            {
                List<BibleLastReadInfo> BibleLastReadInfos = manageReadBible.GetLastReadVerse(UserId, 0, 0);

                if (BibleLastReadInfos.Count > 0)
                {
                    BibleBookSerialId = BibleLastReadInfos[0].BibleBookSerialId;
                }
            }


            UserSettings userSettings = SetLangCDMethod(UserId, LangCD, PagesForView);

            if (!string.IsNullOrEmpty(MoveDirection))
            {
                if (MoveDirection.ToLower() == "p")
                {
                    BibleBookSerialId = BibleBookSerialId - userSettings.PagesForView;
                }
                if (MoveDirection.ToLower() == "n")
                {
                    BibleBookSerialId = BibleBookSerialId + userSettings.PagesForView;
                }
            }


            ManageeBible _ManageeBible = new ManageeBible();

            ViewBag.TotalProgressRate = manageReadBible.GetTotalBibleProgressRate(UserId);

            List<eBible2> data = _ManageeBible.getBibleDataByBibleID(BibleBookSerialId, userSettings.PagesForView, userSettings.LangCD, UserId);
            if (data != null && data.Count > 0)
            {
                eBible2 first = data[0];
                eBible2 last = data[data.Count - 1];

                ViewBag.BibleText = data[0].BibleText;
                ViewBag.BibleSerialJangZul = SharedComm.MakeBibleSerialJangZul(first.BibleText, first.Jang, first.zul, last.Jang, last.zul); 
                
                ViewBag.BibleBookSerialId = BibleBookSerialId - ( userSettings.PagesForView - data.Count) ;
                ViewBag.eBibleBaseFirst = first;
                ViewBag.eBibleBaseLast = last;

                List<TestamentReading> testamentReadings = manageReadBible.GetUnfinishedBooks(UserId, ViewBag.BibleText);

                if (testamentReadings.Count > 0)
                {
                    ViewBag.TestamentReading = testamentReadings[0];
                }
                else
                {
                    ViewBag.TestamentReading = null;
                }
            };
            ViewBag.TotalProgressRate = manageReadBible.GetTotalBibleProgressRate(UserId);


            return View(data);
        }

        // GET: JobCategory
        public async Task<IActionResult> IndexByAB(string ab)
        { 
            ViewBag.ab = ab;
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();

            List<BibleBibleGroup> data = manageeBulkInputBible.getBibleGroupByName(ab);

            List<BibleYouTuBuEnrollsList> _BibleYouTuBuEnrollsList = new List<BibleYouTuBuEnrollsList>();

            foreach (BibleBibleGroup group in data)
            {
                BibleYouTuBuEnrollsList dd= new BibleYouTuBuEnrollsList();
                dd._BibleBibleGroup = group;
                dd._BibleYouTuBuEnrolls = manageeBulkInputBible.getBibleYouTuBuEnrollList(group.Kind2, UserId);
                _BibleYouTuBuEnrollsList.Add(dd);
            }
 
            return View("Index", _BibleYouTuBuEnrollsList);
        }

        // GET: JobCategory
        public async Task<IActionResult> IndexByID(int Id, string BibleText)
        {
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());
                
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();

            List<BibleBasicInfos> value = manageeBulkInputBible.getBibleBasicListByBibleText(Id, BibleText);
            if (value != null && value.Count > 0)
            {
                ViewBag.BibleText = value[0].BibleText;
            }

            var data = manageeBulkInputBible.getBibleYouTuBuList(Id, BibleText , UserId);

            return View(data);
        }

        public async Task<IActionResult> IndexByMap(int Id, string BibleText)
        {
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());

            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();

            List<BibleBasicInfos> value = manageeBulkInputBible.getBibleBasicListByBibleText(Id, BibleText);
            if (value != null && value.Count > 0)
            {
                ViewBag.BibleText = value[0].BibleText;
            }

            var data = manageeBulkInputBible.getBibleYouTuBuList(Id, BibleText, UserId);

            return View(data);
        }


        public async Task<IActionResult> HitBibleYouTuBuHit(string videoId)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();

            string userName = "guest";

            if (await _authService.IsSignedIn(User))
            {
                userName = await _authService.GetUserName(User);
            }

            manageeBulkInputBible.SaveBibleYouTubeHit(videoId, userName);

            return Json(new { success = true, message = "success" });
        }

        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> SaveOrUpdateBibleYouTuBu(string BibleText, int Jang)
        { 
            var data =new BibleYouTuBu();

            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            List<BibleBasicInfos> value = manageeBulkInputBible.getBibleBasicListByBibleText(0, BibleText);
            if (value != null && value.Count > 0)
            {
                ViewBag.Id = value[0].Id;
            }

            data.BibleText = BibleText;
            data.Jang = Jang;

            return View(data);
        }
        [HttpPost]
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> ProcessSaveOrUpdateBibleYouTuBu(BibleYouTuBu bibleYouTuBu)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();

            int newID =1 ;
         
        // if (!ModelState.IsValid)
        {
            // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
            //    return View("ModifyJuManNa", juManNa);
        }
            //  else
            {
                manageeBulkInputBible.SaveOrUpdateBibleYouTuBu(bibleYouTuBu);
            }

            List<BibleBasicInfos> value = manageeBulkInputBible.getBibleBasicListByBibleText(0, bibleYouTuBu.BibleText);
            if(value != null && value.Count > 0)
            {
                newID = value[0].Id;
            }
            return RedirectToAction(nameof(JuManNaController.IndexByID), "bibleCenter", new { Id = newID });
        }


    }
}
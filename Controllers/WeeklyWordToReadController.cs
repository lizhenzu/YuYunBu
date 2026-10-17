using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class WeeklyWordToReadController : BaseController
    { 

        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public WeeklyWordToReadController(IAuthService authService,       
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
             string userName = string.Empty;

            if(await _authService.IsSignedIn(User))
            {                
                userName = await _authService.GetUserName(User);
            }
     
            return View();
        }

        // GET: JobCategory
        public async Task<IActionResult> Branch()
        {
             

            return View();
        }

        public async Task<IActionResult> EnrollWeeklyWordToRead()
        {
            WeeklyWordToRead WeeklyWordToRead = new WeeklyWordToRead();
            WeeklyWordToRead.DateFrom = SharedComm.GetToday();
            WeeklyWordToRead.DateTo = SharedComm.GetToday(7);

            return View(WeeklyWordToRead);
        }
        [HttpPost]
        public IActionResult SaveUploadData(UploadDataWithID uploadDataWithID, string action)
        {
            ManageUploadData manageUploadData = new ManageUploadData();

            if (action == "delete")
            {
                foreach (var va in uploadDataWithID.UploadDatas)
                {
                    if (va.checkData)
                    {
                        va.URLData = va.URLData.Replace("/", "\\").TrimStart('\\');
                        var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, va.URLData);

                        if (System.IO.File.Exists(uploadPath))
                        {
                            System.IO.File.Delete(uploadPath);
                            manageUploadData.DeleteUploadData(va.UploadID);
                        }
                    }
                }
            }
            else
            {
                manageUploadData.CreateUploadWeeklyWordToReadAssoc(uploadDataWithID.SelectedCategory.ToString(), uploadDataWithID.WeeklyWordToReadID, uploadDataWithID.UploadDatas);
            }
            return RedirectToAction(nameof(WeeklyWordToReadController.ModifyUploadData), "WeeklyWordToRead", new {category = uploadDataWithID.SelectedCategory.ToString(), WeeklyWordToReadID = uploadDataWithID.WeeklyWordToReadID, afterProcess ="procesed"});
        }

        //[HttpPost]
        //public IActionResult SaveBibleGeomorphological(BibleGeomorphologicalWithID bibleGeomorphologicalWithID)
        //{

        //    ManageBibleGeomorphological manageBibleGeomorphological = new ManageBibleGeomorphological();

        //    manageBibleGeomorphological.CreateBiblePeopleWeeklyWordToReadAssoc(bibleGeomorphologicalWithID.WeeklyWordToReadID, bibleGeomorphologicalWithID.BibleGeomorphologicals);

        //    return RedirectToAction(nameof(WeeklyWordToReadController.WeeklyWordToReadDetailList), "WeeklyWordToRead");
        //}


        public async Task<IActionResult> ModifyUploadData(Category category, int WeeklyWordToReadID, string afterProcess)
        {
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();
            UploadDataWithID uploadDataWithID = new UploadDataWithID();

            uploadDataWithID.UploadDatas = manageWeeklyWordToRead.getUploadDataWeeklyWordToReadAssoc(category, WeeklyWordToReadID);


            uploadDataWithID.UploadDatas = uploadDataWithID.UploadDatas
            .OrderBy(data => data.CategoryData)
            .ThenBy(data => data.TEXTData)
            .ToList();

            uploadDataWithID.WeeklyWordToReadID = WeeklyWordToReadID;
            uploadDataWithID.SelectedCategory = category;

            if(!string.IsNullOrEmpty(afterProcess))
                ViewBag.Message = "파일 업로드 성공 (Save)";

            return View(uploadDataWithID);
        } 
         
        //

        public async Task<IActionResult> ModifyWeeklyWordToRead(int Id)
        {
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            List<WeeklyWordToReads> value = manageWeeklyWordToRead.getWeeklyWordToRead(Id, false, DateHelper.GetKoreaToday(), DateHelper.GetKoreaToday().Year);
            WeeklyWordToRead WeeklyWordToRead = new WeeklyWordToRead();
            WeeklyWordToRead = value[0];
            return View(WeeklyWordToRead);
        }

        public async Task<IActionResult> MakePublishYn(int Id, string publishYn)
        {
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            manageWeeklyWordToRead.MakePublishYn(Id, publishYn);

            return RedirectToAction(nameof(WeeklyWordToReadController.WeeklyWordToReadDetailList), "WeeklyWordToRead");
        }

        public async Task<IActionResult> DecreasetHitWeeklyWordToRead(int Id)
        {
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();
            manageWeeklyWordToRead.DecreasetHitWeeklyWordToRead(Id);
            return RedirectToAction(nameof(WeeklyReportController.IndexByID), "WeeklyReport", new { Id = Id });
        }


        [HttpPost]
        public async Task<IActionResult> CreateWeeklyWordToRead(WeeklyWordToRead WeeklyWordToRead)
        {
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
                return View("EnrollWeeklyWordToRead", WeeklyWordToRead);
            }
            else
            {
                manageWeeklyWordToRead.CreateWeeklyWordToRead(WeeklyWordToRead);
            }        
            return RedirectToAction("WeeklyWordToReadDetailList");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWeeklyWordToRead(WeeklyWordToRead WeeklyWordToRead)
        {
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
                return View("ModifyWeeklyWordToRead", WeeklyWordToRead);
            }
            else
            {
                manageWeeklyWordToRead.UpdateWeeklyWordToRead(WeeklyWordToRead);
            }
            return RedirectToAction(nameof(WeeklyReportController.IndexByID), "WeeklyReport", new { Id = WeeklyWordToRead.Id});
        }         

        public async Task<IActionResult> WeeklyWordToReadDetailList()
        {
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";


            var ChurchName = HttpContext.Session.GetString(CommonMetaDataCategory.ChurchName.ToString()) as string;

            if (string.IsNullOrEmpty(ChurchName))
            {
                ManageeCommon manageeCommon = new ManageeCommon();
                List<CommonMetaData> commonMetaData = manageeCommon.getCommonMetaData();

                foreach (var va in commonMetaData)
                {
                    ViewData[va.Kind.ToString()] = va.KindDesc;
                    HttpContext.Session.SetString(va.Kind.ToString(), va.KindDesc);

                }
            }



            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            return View(manageWeeklyWordToRead.getWeeklyWordToRead(0, false, SharedComm.GetToday(), DateHelper.GetKoreaToday().Year));
        }

        public async Task<IActionResult> DeleteWeeklyWordToRead(int Id)
        {
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            manageWeeklyWordToRead.DeleteWeeklyWordToRead(Id);

            return RedirectToAction("WeeklyWordToReadDetailList");
        }

        public async Task<IActionResult> IntroBook()
        {
            return View();
        }
    }
}

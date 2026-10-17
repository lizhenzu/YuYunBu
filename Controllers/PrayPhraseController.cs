using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [Authorize]
    [LogActionFilter]
    public class PrayPhraseController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public PrayPhraseController(IAuthService authService,
            UserManager<ApplicationUser> userManager) : base(authService, userManager)
        {
            _authService = authService;
            _userManager = userManager;
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
        public async Task<IActionResult> EnrollPrayPhrase()
        {
            PrayPhrase prayPhrase = new PrayPhrase();
            prayPhrase.PraryBeginMesssage = "";
            prayPhrase.PraryEndMesssage = "예수님의 이름으로 기도드립니다. 아멘!"; 
            return View(prayPhrase);
        }

        public async Task<IActionResult> ModifyPrayPhrase(int Id)
        {
            ManagePrayPhrase managePrayPhrase = new ManagePrayPhrase();

            List<PrayPhrases> value = managePrayPhrase.getPrayPhrases(Id, false, SharedComm.GetToday());
            PrayPhrase PrayPhrase = new PrayPhrase();
            PrayPhrase = value[0];
            return View(PrayPhrase);
        } 

        [HttpPost]
        public async Task<IActionResult> CreatePrayPhrase(PrayPhrase PrayPhrase)
        {
            ManagePrayPhrase managePrayPhrase = new ManagePrayPhrase();

            if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
                return View("EnrollPrayPhrase", PrayPhrase);
            }
            else
            {
                managePrayPhrase.CreatePrayPhrase(PrayPhrase);
            }        
            return RedirectToAction("PrayPhraseDetailList");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePrayPhrase(PrayPhrase PrayPhrase)
        {
            ManagePrayPhrase managePrayPhrase = new ManagePrayPhrase();

            if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
                return View("ModifyPrayPhrase", PrayPhrase);
            }
            else
            {
                managePrayPhrase.UpdatePrayPhrase(PrayPhrase);
            }
            return RedirectToAction("PrayPhraseDetailList");
        }

        //

        public async Task<IActionResult> PrayPhraseDetailList()
        {
            ManagePrayPhrase managePrayPhrase = new ManagePrayPhrase();
            ManageeBible manageeBible = new ManageeBible();
        
            List<PrayPhrases> prayList = managePrayPhrase.getPrayPhrases(0, false, GetWorkDT());

            PrayPhrasesDetails prayPhrasesDetails = new PrayPhrasesDetails();

            var UserId = HttpContext.Session.GetString(HttpContextSessionCategory.UserId.ToString()) as string;            

            foreach (PrayPhrases va in prayList)
            {
                eBibles eBiblesData = manageeBible.geteBible2(va.BibleTextToPray, va.BibleTextToPrayFromJang, va.BibleTextToPrayFromZul, va.BibleTextToPrayToJang, va.BibleTextToPrayToZul, "ko", UserId);

                prayPhrasesDetails.PrayPhrasesDetailList.Add(new PrayPhrasesDetail { PrayPhrasesData = va, eBiblesData = eBiblesData });
            }

            ViewBag.DisableLangTransBtton = "y";

            return View("PrayPhraseDetailList", prayPhrasesDetails);
        }

        public async Task<IActionResult> DeletePrayPhrase(int Id)
        {
            ManagePrayPhrase managePrayPhrase = new ManagePrayPhrase();

            managePrayPhrase.DeletePrayPhrase(Id);

            return RedirectToAction("PrayPhraseDetailList");
        }
        public async Task<IActionResult> RestorPrayPhrase(int YYear, int MMonth)
        {
            ManagePrayPhrase managePrayPhrase = new ManagePrayPhrase();

            managePrayPhrase.RestorPrayPhrase(YYear, MMonth);

            return RedirectToAction("PrayPhraseDetailList");
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

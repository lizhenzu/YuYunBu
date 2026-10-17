using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class JuManNaController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public JuManNaController(IAuthService authService,
            UserManager<ApplicationUser> userManager) : base(authService, userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }  
        
        // 목록 · 수정 · 저장은 SuperAdmin 전용.  상세(IndexByID)는 QT 홈에서 일반 사용자도 들어온다
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> Index()
        {
            ManageJuManNa manageJuManNa = new ManageJuManNa();
            List<JuManNas> values = manageJuManNa.getJuManNa(0, false, GetWorkDT());

            return View(values);
        }

        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> IndexGroupByYYYYMM()
        {
            ManageJuManNa manageJuManNa = new ManageJuManNa();
            List<JuManNaYYYYMM> values = manageJuManNa.getJuManNaGroupByYYYYMM();

            return View(values);
        }
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> IndexQueryByYYYYMM(string option,  string YYYYMM)
        {
            ManageJuManNa manageJuManNa = new ManageJuManNa();
            List<JuManNas> values = manageJuManNa.getJuManNaByYYYYMM(YYYYMM);
            ViewBag.option = option.ToLower();
            ViewBag.YYYYMM = YYYYMM;   // 화면 위 보기 방식 칩(본문만 · 주제까지 · 전체 내용 · 영상 구간)이 같은 달로 이동할 때 쓴다

            if (option == "DetailSerail")
            {

                return View("IndexGroupByYYYYMMSerial", values);
            }
            else
            {
                return View("Index", values);
            }

        }

        // 월별 일괄 입력 — 주제 · 성경 · 장절만.  있는 달이면 수정, 없는 날은 저장할 때 새로 넣는다
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public IActionResult MonthlyBulk(string YYYYMM)
        {
            if (!TryParseMonth(YYYYMM, out var firstDay))
            {
                return RedirectToAction(nameof(IndexGroupByYYYYMM));
            }

            ManageJuManNa manageJuManNa = new ManageJuManNa();
            return View(manageJuManNa.getJuManNaMonthly(firstDay));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public IActionResult ProcessMonthlyBulk(JuManNaMonthly model)
        {
            if (model == null || !TryParseMonth(model.YYYYMM, out var firstDay))
            {
                return RedirectToAction(nameof(IndexGroupByYYYYMM));
            }

            ManageJuManNa manageJuManNa = new ManageJuManNa();
            var (updated, inserted) = manageJuManNa.SaveJuManNaMonthly(firstDay, model.Days);

            TempData["MonthlySaved"] = inserted > 0 ? $"새로 {inserted}일 추가 · {updated}일 수정했어요"
                                     : updated > 0 ? $"{updated}일 수정했어요"
                                     : "바뀐 날이 없어요";
            return RedirectToAction(nameof(MonthlyBulk), new { YYYYMM = firstDay.ToString("yyyy-MM") });
        }

        // 영상 구간 편집 — 한 달 전체.  영상 창에서 시작 · 종료를 누르면 SaveSegment 로 바로 저장(화면 그대로)
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public IActionResult SegmentEditor(string YYYYMM)
        {
            if (!TryParseMonth(YYYYMM, out var firstDay))
            {
                return RedirectToAction(nameof(IndexGroupByYYYYMM));
            }
            return View(new ManageJuManNa().getJuManNaSegMonth(firstDay));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public IActionResult SaveSegment(int id, int slot, string kind, int sec)
        {
            if (kind != "start" && kind != "end")
            {
                return Json(new { ok = false, msg = "잘못된 요청이에요" });
            }
            try
            {
                var (ok, msg, seg) = new ManageJuManNa().SaveJuManNaSegPoint(id, slot, kind == "start", sec);
                if (!ok) return Json(new { ok = false, msg });
                return Json(new { ok = true, start = seg.StartSec, end = seg.EndSec, len = seg.LenSec });
            }
            catch (Exception)
            {
                return Json(new { ok = false, msg = "저장하지 못했어요. 다시 눌러 주세요" });
            }
        }

        private static bool TryParseMonth(string YYYYMM, out DateTime firstDay)
        {
            return DateTime.TryParseExact((YYYYMM ?? "") + "-01", "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out firstDay)
                && firstDay.Year >= 2000 && firstDay.Year <= 2100;
        }

        public async Task<IActionResult> IndexByID(int Id)
        { 
            ManageJuManNa manageJuManNa = new ManageJuManNa();
            List<JuManNas> values =  manageJuManNa.getJuManNa(Id, true, GetWorkDT());

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());


            var userFullName = HttpContext.Session.GetString(HttpContextSessionCategory.UserFullName.ToString()) as string;
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

            if (!SharedComm.isAdmin(UserName))
            {
                manageJuManNa.HitJuManNa(values[0].Id);
            } 
           
            return View(values[0]);
        }


        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> ModifyJuManNa(int Id)
        {
            ManageJuManNa manageJuManNa = new ManageJuManNa();
            List<JuManNas> values = manageJuManNa.getJuManNa(Id, false, GetWorkDT());

            return View(values[0]);
        }
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> ModifyJuManNabyDate(DateTime dt)
        {
            ManageJuManNa manageJuManNa = new ManageJuManNa();
            List<JuManNas> values = manageJuManNa.getJuManNa(0, true, dt);

            return View("ModifyJuManNa", values[0]);
        }

        [HttpPost]
        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> ProcessModifyJuManNa(JuManNa juManNa)
        {
            ManageJuManNa manageJuManNa = new ManageJuManNa();

           // if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
            //    return View("ModifyJuManNa", juManNa);
            }
          //  else
            {
                manageJuManNa.UpdateJuManNa(juManNa);
            }
            return RedirectToAction(nameof(JuManNaController.IndexByID), "JuManNa", new { Id = juManNa.Id });
        }



        public async Task<JsonResult> GetDetails(string bibleText , int  fromJang  , int fromZul  , int toJang , int toZul, string langCD = "ko")
        { 
            ManageeBible manageeBible = new ManageeBible(); 

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            return Json(manageeBible.geteBible2(bibleText, fromJang, fromZul, toJang, toZul, langCD, UserId));
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
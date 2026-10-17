using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class BessingACoupleController : BaseController
    {

        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public BessingACoupleController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
                 IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        // 2026-10-04: Kind 는 표 이름(Bessing{Kind})에 그대로 들어가므로 Wife · Husband 만 받는다 (SQL 인젝션 차단)
        //             삭제 · 등록 · 조회수 감소는 SuperAdmin 전용.  보기(DetailWords · DetailList)는 누구나.
        // 2026-10-11: 기도 관리자 = 로그인 아이디 "Basic" · "SuperAdmin" 두 계정 (역할은 보지 않는다 — PrayManagerHelper)
        //             → 읽음 수 안 올림 · 수정(ModifyView · ModifyProcess).  Admin 은 일반 사용자와 같다.
        //             ※ Roles.Basic 은 일반 회원 기본 역할이라 역할로 판정하면 안 된다.
        private bool IsPrayManager => PrayManagerHelper.IsPrayManager(User);
        private static readonly string[] AllowedKinds = { "Wife", "Husband" };
        private static string? NormalizeKind(string? kind)
            => AllowedKinds.FirstOrDefault(k => string.Equals(k, kind?.Trim(), StringComparison.OrdinalIgnoreCase));

        // GET: JobCategory
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> DetailWords(string Kind, int Id, string ExposeDate)
        {
            Kind = NormalizeKind(Kind);
            if (Kind == null) return BadRequest();

            ManageBessingACouple manageBessingACouple = new ManageBessingACouple();
            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}/{ViewBag.ControllerName}/{ViewBag.ActionName}";
            ViewBag.Kind = Kind;
            ViewBag.VerifyReady = IsPrayManager && manageBessingACouple.IsVerifyReady(Kind);   // 검증 칸이 있고 관리자일 때만
            ViewBag.CanAddPray = CanAddPray;   // 새 기도문 버튼
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

            var data = manageBessingACouple.getBessingACouple(Kind, Id, ExposeDate);

            if (data.Count == 0)
                return RedirectToAction(nameof(BessingACoupleController.DetailList), "BessingACouple", new { Kind = Kind });

            DateTime currentDate = SharedComm.GetToday();
            int index = SharedComm.CalculateIndex(data.Count, currentDate);


            var userFullName = HttpContext.Session.GetString(HttpContextSessionCategory.UserFullName.ToString()) as string;
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

            // 기도 관리자(Basic · SuperAdmin 계정)는 읽음 수를 올리지 않는다 (Admin 계정은 일반 사용자처럼 센다)
            if (!IsPrayManager)
            {
                manageBessingACouple.HitBessingACouple(Kind, data[index].Id);
            }              

            return View(data[index]);
        }

        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> DeleteView(string Kind, int Id)
        {
            Kind = NormalizeKind(Kind);
            if (Kind == null) return BadRequest();

            ManageBessingACouple manageBessingACouple = new ManageBessingACouple();
            ViewBag.Kind = Kind;
            manageBessingACouple.DeleteBessingACouple(Kind, Id);
            return RedirectToAction(nameof(BessingACoupleController.DetailList), "BessingACouple", new { Kind = Kind });
        }

        [Authorize(Roles = nameof(Roles.SuperAdmin))]
        public async Task<IActionResult> DecreasetHitACouple(string Kind, int Id)
        {
            Kind = NormalizeKind(Kind);
            if (Kind == null) return BadRequest();

            ManageBessingACouple manageBessingACouple = new ManageBessingACouple();
            ViewBag.Kind = Kind;
            manageBessingACouple.DecreasetHitACouple(Kind, Id);
            return RedirectToAction(nameof(BessingACoupleController.DetailList), "BessingACouple", new { Kind = Kind });
        }

        [Authorize]
        public async Task<IActionResult> ModifyView(string Kind,int Id)
        {
            if (!IsPrayManager) return RedirectToAction(nameof(DetailWords), new { Kind = NormalizeKind(Kind) ?? "Husband", Id });

            Kind = NormalizeKind(Kind);
            if (Kind == null) return BadRequest();

            ManageBessingACouple manageBessingACouple = new ManageBessingACouple();
            ViewBag.Kind = Kind;
            var data = manageBessingACouple.getBessingACouple(Kind, Id, string.Empty);
            return View(data[0]);
        }

        // 새 기도문 쓰기 — SuperAdmin 역할 · Basic 계정 (2026-10-15, PrayManagerHelper.CanAddCouplePray)
        private bool CanAddPray => PrayManagerHelper.CanAddCouplePray(User);

        [Authorize]
        public async Task<IActionResult> EnrollView(string Kind)
        {
            Kind = NormalizeKind(Kind);
            if (Kind == null) return BadRequest();
            if (!CanAddPray) return RedirectToAction(nameof(DetailWords), new { Kind });

            var mgr = new ManageBessingACouple();
            BessingACoupleData bessingACoupleData = new BessingACoupleData();
            ViewBag.Kind = Kind;
            ViewBag.NextPageNo = mgr.GetNextPageNo(Kind);   // "45번째로 들어가요" 안내용
            return View(bessingACoupleData);
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyProcess(string Kind, BessingACoupleData param)
        {
            if (!IsPrayManager) return Forbid();

            Kind = NormalizeKind(Kind);
            if (Kind == null) return BadRequest();

            ManageBessingACouple manageBessingACouple = new ManageBessingACouple();
            ViewBag.Kind = Kind;
            manageBessingACouple.UpdateBessingACouple(Kind, param);
            // 2026-10-11: 고친 기도문을 바로 확인할 수 있게 기도문 화면으로 돌아간다 (예전에는 목록)
            return RedirectToAction(nameof(BessingACoupleController.DetailWords), "BessingACouple", new { Kind = Kind, Id = param.Id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollProcess(string Kind, BessingACoupleData param)
        {
            Kind = NormalizeKind(Kind);
            if (Kind == null) return BadRequest();
            if (!CanAddPray) return Forbid();

            if (param == null || string.IsNullOrWhiteSpace(param.PrayTitle) || string.IsNullOrWhiteSpace(param.PrayText))
            {
                // 제목 · 본문이 비면 저장하지 않고 쓰던 화면으로 돌려보낸다 (쓴 내용은 그대로)
                ViewBag.Kind = Kind;
                ViewBag.NextPageNo = new ManageBessingACouple().GetNextPageNo(Kind);
                ViewBag.Error = "제목과 기도문 본문은 꼭 적어 주세요.";
                return View("EnrollView", param ?? new BessingACoupleData());
            }

            ManageBessingACouple manageBessingACouple = new ManageBessingACouple();
            ViewBag.Kind = Kind;
            param.PageNo = 0;   // 번호는 화면에서 받지 않는다 — 맨 끝 번호(+1)로 자동
            manageBessingACouple.CreateBessingACouple(Kind, param);

            // 방금 쓴 기도문 화면으로 바로 간다 (찾지 못하면 목록)
            int newId = manageBessingACouple.GetMaxId(Kind);
            if (newId > 0)
                return RedirectToAction(nameof(DetailWords), new { Kind, Id = newId });
            return RedirectToAction(nameof(BessingACoupleController.DetailList), "BessingACouple", new { Kind  = Kind });
        }
                 
        public async Task<IActionResult> DetailList(string Kind)
        {
            Kind = NormalizeKind(Kind);
            if (Kind == null) return BadRequest();

            ManageBessingACouple manageBessingACouple = new ManageBessingACouple();
            ViewBag.Kind = Kind;
            ViewBag.VerifyReady = IsPrayManager && manageBessingACouple.IsVerifyReady(Kind);
            ViewBag.CanAddPray = CanAddPray;

            return View(manageBessingACouple.getBessingACouple(Kind, 0, string.Empty));
        }

        // 검증 완료 표시 — 기도 관리자(Basic · SuperAdmin 계정)만. 이미 검증된 기도문은 그대로 둔다.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Verify(string Kind, int Id)
        {
            Kind = NormalizeKind(Kind);
            if (Kind == null) return BadRequest();
            if (!IsPrayManager) return Forbid();

            if (Id > 0)
            {
                new ManageBessingACouple().VerifyBessingACouple(Kind, Id, User.Identity?.Name);
            }
            return RedirectToAction(nameof(DetailWords), new { Kind = Kind, Id = Id });
        }

        // 검증 취소 — "검증 전" 으로 되돌린다 (기도 관리자만)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Unverify(string Kind, int Id)
        {
            Kind = NormalizeKind(Kind);
            if (Kind == null) return BadRequest();
            if (!IsPrayManager) return Forbid();

            if (Id > 0)
            {
                new ManageBessingACouple().UnverifyBessingACouple(Kind, Id);
            }
            return RedirectToAction(nameof(DetailWords), new { Kind = Kind, Id = Id });
        }
    }
}



using BulletinBoard.BusinessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [Authorize(Roles = nameof(Roles.SuperAdmin))]
    [LogActionFilter]
    public class BibleBulkController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public BibleBulkController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
                 IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Role
        public async Task<IActionResult> Index()
        {
            BibleBulk bibleBulk = new BibleBulk();
            ViewBag.screenName = "성경말씀등록";
            ViewBag.LangCD = "ko";
            return View(bibleBulk);
        }      // GET: Role     // GET: Role
        public async Task<IActionResult> IndexEN()
        {
            BibleBulk bibleBulk = new BibleBulk();
            ViewBag.screenName = "성경말씀등록";
            ViewBag.LangCD = "EN";
            return View("Index", bibleBulk);
        }      // GET: Role
        public async Task<IActionResult> IndexEN_NIV(string BibleText_K, int Jang_K)
        {
            BibleBulk bibleBulk = new BibleBulk();
            ViewBag.screenName = "성경말씀등록";
            ViewBag.LangCD = "EN_NIV";
            ViewBag.BibleText_K = BibleText_K;
            ViewBag.Jang_K = Jang_K;
            return View("Index", bibleBulk);
        }      // GET: Role
        public async Task<IActionResult> EnrollBulkQuiz(string screenName)
        {
            BibleBulkQuiz bibleBulkQuiz = new BibleBulkQuiz();
            ViewBag.screenName = screenName;
            return View(bibleBulkQuiz);
        }

        //
        // GET: Role
        public async Task<IActionResult> EnrollView(string BibleText, int chapter, string LangCD)
        {
            ViewBag.screenName = "성경말씀등록(" + LangCD + ")";
            BibleBulk bibleBulk = new BibleBulk();
            bibleBulk.BibleTitle = BibleText;
            bibleBulk.BibleJang = chapter;
            ViewBag.LangCD = LangCD;
            return View("Index", bibleBulk);
        }

        [AllowAnonymous]
        public async Task<IActionResult> BibleSheet2(string BibleText, int Jang, string langCD)
        {
            ManageeBible manageeBible =new ManageeBible();

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            eBibles eBibleDatas = manageeBible.geteBible3(BibleText, Jang,langCD, UserId);
            return View(eBibleDatas);
        }

        public async Task<IActionResult> BibleEnrollList(string screenName)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            ViewBag.screenName = screenName;
            return View(manageeBulkInputBible.getBibleEnrollList());
        }
        public async Task<IActionResult> BibleBasicList(string screenName)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            ViewBag.screenName = screenName;
            return View(manageeBulkInputBible.getBibleBasicList());
        }
        public async Task<IActionResult> BibleDetailList(string BibleText, int Jang)
        { 
            ViewBag.Jang = Jang;
            ViewBag.BibleText = BibleText;

            int inputValue = Jang; // 3으로 변경하면 1, 2, 3의 데이터가 생성됩니다.
            List<VirtualA> virtualATable = CreateVirtualATable(inputValue);

            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();

            List<BibleQuizEnrollGroupInfo> bTable = manageeBulkInputBible.getBibleQuizEnrollGroupList(BibleText);


            var result = from a in virtualATable
                         join b in bTable on a.Id equals b.Jang into abGroup
                         from b in abGroup.DefaultIfEmpty()
                         select new
                         {
                             AId = a.Id,
                             BId = b?.Jang,
                             BText = b?.BibleText,
                             BCount = b?.DCount
                         };
            List<BibleQuizEnrollGroupInfo2> bTable1 = new List<BibleQuizEnrollGroupInfo2>();
            // 출력
            foreach (var row in result)
            {
                bTable1.Add(new BibleQuizEnrollGroupInfo2 { Id = row.AId, Jang = row.AId, BibleText = row.BText , DCount = row.BCount }); 
            }


            return View(bTable1);
        }

        public async Task<IActionResult> DeleteQuestionBank(int Id, string BibleText, int Jang, int MaxJang )
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();
            manageQuestionBank.DeleteQuestionBank(Id);
            return RedirectToAction("BibleQuizEnrollList", new { BibleText = BibleText, Jang= Jang, MaxJang = MaxJang });
        }

        public async Task<IActionResult> BibleQuizEnrollList(string BibleText, int Jang, int MaxJang)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            ViewBag.Jang = Jang;
            ViewBag.MaxJang = MaxJang;
            ViewBag.BibleText = BibleText;
            return View(manageeBulkInputBible.getBibleQuizEnrollList(BibleText, Jang));
        }

        public async Task<IActionResult> BibleNullList(string BibleText, int Jang)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            ViewBag.Jang = Jang;
            ViewBag.BibleText = BibleText;
            return View(manageeBulkInputBible.getBibleNullList(BibleText, Jang));
        }


        public async Task<IActionResult> BibleMissingList(string screenName)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            ViewBag.screenName = screenName;
            return View(manageeBulkInputBible.getBibleMissingList());
        }

        public async Task<IActionResult> BibleMissingListByLang(string screenName, string KO_BibleText)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            ViewBag.screenName = screenName;
            return View(manageeBulkInputBible.getBibleMissingListByLang(KO_BibleText));
        }
        public async Task<IActionResult> BibleMissingListB_KO_EN_SU()
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            return View(manageeBulkInputBible.getBibleMissingListB_KO_EN_SU());
        }
        public async Task<IActionResult> BibleMissingListB_SU(string BibleText)
        {
            ViewBag.BibleText = BibleText;
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            return View(manageeBulkInputBible.getBibleMissingListB_SU(BibleText));
        }
        public async Task<IActionResult> SyncENtoNIC()
        { 
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            manageeBulkInputBible.SyncENtoNIC();

            return View("Index");
        }

        public async Task<IActionResult> BibleMissingListB_KJV(string BibleText)
        {
            ViewBag.BibleText = BibleText;
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            return View(manageeBulkInputBible.getBibleMissingListB_KJV(BibleText));
        }
        public async Task<IActionResult> BibleMissingList_NIV(string BibleText)
        {
            ViewBag.BibleText = BibleText;
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            return View(manageeBulkInputBible.getBibleMissingList_NIV(BibleText));
        }


        public async Task<IActionResult> BibleMissingListByGroupByLang(string screenName)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();
            ViewBag.screenName = screenName;
            return View(manageeBulkInputBible.getBibleMissingListByGroupByLang());
        }


        // POST: Role/ChangeRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBulkInputBible(BibleBulk bibleBulk)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();

            manageeBulkInputBible.CreateBulkInputBible(bibleBulk);

            return RedirectToAction("BibleMissingListB_KO_EN_SU");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBulkQuiz(BibleBulkQuiz param)
        {
            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();

            manageeBulkInputBible.CreateBulkQuiz(param);

            return RedirectToAction("EnrollBulkQuiz");
        }

        static List<VirtualA> CreateVirtualATable(int count)
        {
            List<VirtualA> table = new List<VirtualA>();
            for (int i = 1; i <= count; i++)
            {
                table.Add(new VirtualA { Id = i });
            }
            return table;
        }

    }
    class VirtualA
    {
        public int Id { get; set; }
    }

}
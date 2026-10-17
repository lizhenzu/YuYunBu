using BulletinBoard.DataAccessLayer.Worldview;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services.WorldView.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Controllers
{
    [Authorize] // 필요시 [Authorize(Roles="Admin")]
    [Route("WorldviewAdmin")]
    public class WorldviewAdminController : BaseController
    {
        private readonly IWorldviewAdminService _adminService;
        private readonly WorldviewLessonRepository _repo;
        public WorldviewAdminController(
            IAuthService authService,
            IWorldviewAdminService adminService,
              WorldviewLessonRepository repo,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostingEnvironment
        ) : base(authService, userManager, hostingEnvironment)
        {
            _repo = repo;
            _adminService = adminService;
        }


        // GET /WorldviewAdmin/SubmitHistory?lessonNo=1
        [HttpGet("SubmitHistory")]
        public IActionResult SubmitHistory([FromQuery] int? lessonNo = null)
        {
            var rows = _repo.GetSubmitRankRows(lessonNo);
            ViewBag.LessonNo = lessonNo;
            return View(rows);
        }
    

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var cards = _adminService.GetHeaderTitleCards();
            ViewBag.ActiveLessonNo = _adminService.GetActiveLessonNo();
            return View(cards);
        }
        // GET /WorldviewAdmin/SubmitRank?lessonNo=1
        [HttpGet("SubmitRank")]
        public IActionResult SubmitRank([FromQuery] int? lessonNo = null)
        {
            var rows = _repo.GetSubmitRankByUser(lessonNo);
            ViewBag.LessonNo = lessonNo;
            return View("~/Views/WorldviewAdmin/SubmitRank.cshtml", rows);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("SetActive")]
        public IActionResult SetActive(int lessonNo)
        {
            var userName = User?.Identity?.Name ?? "";
            _adminService.SetActiveLessonNo(lessonNo, userName);
            TempData["Msg"] = $"활성 과정(lessonNo)을 {lessonNo}로 변경했습니다.";
            return RedirectToAction(nameof(Index));
        }
    }
}

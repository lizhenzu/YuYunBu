using BulletinBoard.BusinessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Xml;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    
    public class SummerCampController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public SummerCampController(IAuthService authService,
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

        public async Task<IActionResult> Index2024()
        {
            return View();
        }

        // GET: JobCategory
        public async Task<IActionResult> Invitation()
        {
            return View();
        }
        public async Task<IActionResult> TimeTable()
        {
            return View();
        }
        public async Task<IActionResult> Study()
        {
            return View();
        }
        public async Task<IActionResult> intro()
        {
            return View();
        }
        public async Task<IActionResult> TrainVideo()
        {
            return View();
        }
        public async Task<IActionResult> gw02()
        {
            return View();
        }
        public async Task<IActionResult> gw03()
        {
            return View();
        }
        public async Task<IActionResult> studyslide_00()
        {
            return View();
        }
        public async Task<IActionResult> studyslide_01()
        {
            return View();
        }
        public async Task<IActionResult> studyslide_02()
        {
            return View();
        }
        public async Task<IActionResult> studyslide_03()
        {
            return View();
        }

        public async Task<IActionResult> books_01()
        {
            return View();
        }

        public async Task<IActionResult> books_02()
        {
            return View();
        }

        public async Task<IActionResult> books_03()
        {
            return View();
        } 

    }
}




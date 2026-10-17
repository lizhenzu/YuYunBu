using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using BulletinBoard.Models;
using BulletinBoard.Services;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class ExerciseController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IExerciseRepository _repo;


        public ExerciseController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
                 IWebHostEnvironment hostingEnvironment, IExerciseRepository repo) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _repo = repo;
            _hostingEnvironment = hostingEnvironment;

        }
        //public IActionResult Index()
        //{
        //    ExerciseService _ExerciseService = new ExerciseService();
        //    return View(_ExerciseService.GetExerciseDetail(2));
        //}    

        // 목록 + 필터
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 전체 목록: 필터 없이 가져오기
            var (items, total) = await _repo.GetExercisesAsync(new ExerciseFilter { Page = 1, PageSize = 1000 });
            ViewBag.Total = total;
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var vm = await _repo.GetExerciseDetailAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }
    }

    
}



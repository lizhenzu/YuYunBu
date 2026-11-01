using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class PrayBranchController : BaseController
    {
        private new readonly IAuthService _authService;
        private new readonly UserManager<ApplicationUser> _userManager;
        private new readonly IWebHostEnvironment _hostingEnvironment;
        public PrayBranchController(IAuthService authService,
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
            return View();
        }
        
    }
}



using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.DataAccessLayer;
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
using System.IO;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize]
    public class PrayFriendsController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PrayFriendsController(IAuthService authService,
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
      
    
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> PrayFriend()
        {
			var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

			ManagePrayFriends managePrayFriends = new ManagePrayFriends();

            List<PrayFriend> userInfo = managePrayFriends.getPrayFriend(_hostingEnvironment);

			if (!SharedComm.isAdmin(UserName))
			{
				managePrayFriends.HitPrayFreind();
			}

			return View(userInfo);
        }

    }
}



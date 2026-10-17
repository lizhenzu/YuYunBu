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

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class YoutubeShortsController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment; 

        public YoutubeShortsController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
                 IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;

        }
        public IActionResult Index()
        {
            ManageYoutubeShorts manageYoutubeShorts = new ManageYoutubeShorts();

            return View(manageYoutubeShorts.getYoutubeShorts());
        }
        public IActionResult IndexByName(string ContentsText)
        {
            ManageYoutubeShorts manageYoutubeShorts = new ManageYoutubeShorts();

            return View("Index", manageYoutubeShorts.getYoutubeShorts(string.Empty, ContentsText));
        }

        //IndexByName
        public IActionResult IndexByGroup()
        {
            ManageYoutubeShorts manageYoutubeShorts = new ManageYoutubeShorts();

            return View(manageYoutubeShorts.getYoutubeShortsByGroup());
        }


        public IActionResult EnrollYoutubeShorts()
        {
            YoutubeShort youtubeShort = new YoutubeShort();

            return View(youtubeShort);
        }
        public IActionResult ModifyYoutubeShorts(string ContentsURL)
        {
            ManageYoutubeShorts manageYoutubeShorts = new ManageYoutubeShorts();
            List<YoutubeShort> data = manageYoutubeShorts.getYoutubeShorts(ContentsURL);             

            return View("IndexByGroup", data[0]);
        }
        public IActionResult DeleteYoutubeShorts(int id)
        {
            ManageYoutubeShorts manageYoutubeShorts = new ManageYoutubeShorts();
            manageYoutubeShorts.DeleteUploadData(id);
              return RedirectToAction("IndexByGroup");
        }

        [HttpPost]
        public async Task<IActionResult> EnrollProcessYoutubeShorts(YoutubeShort param)
        {
            ManageYoutubeShorts manageYoutubeShorts = new ManageYoutubeShorts();
            manageYoutubeShorts.upsertYoutubeShorts(param.ContentsURL, param.ContentsText);

            return RedirectToAction("IndexByGroup");
        }


        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        } 
    }
}



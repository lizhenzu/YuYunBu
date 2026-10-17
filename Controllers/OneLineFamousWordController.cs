using BulletinBoard.BusinessLayer;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using BulletinBoard.Enums;
using Microsoft.AspNetCore.Authorization;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize(Roles = nameof(Roles.Admin))]
    public class OneLineFamousWordController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public OneLineFamousWordController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
                 IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            ManageOneLineFamousWord manageOneLineFamousWord = new ManageOneLineFamousWord();

            return View(manageOneLineFamousWord.getOneLineFamousWord(0));
        }

        public async Task<IActionResult> CreateOneLineFamousWord(int Id)
        {
            OneLineFamousWords oneLineFamousWords = new OneLineFamousWords();

            return View(oneLineFamousWords);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOneLineFamousWordProcess(OneLineFamousWords param)
        {
            ManageOneLineFamousWord manageOneLineFamousWord = new ManageOneLineFamousWord();

            manageOneLineFamousWord.CreateOneLineFamousWord(param);

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> DeleteOneLineFamousWord(int Id)
        {
            ManageOneLineFamousWord manageOneLineFamousWord = new ManageOneLineFamousWord();

            manageOneLineFamousWord.DeleteOneLineFamousWord(Id);

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> ModifyOneLineFamousWordProcess(OneLineFamousWords param)
        {
            ManageOneLineFamousWord manageOneLineFamousWord = new ManageOneLineFamousWord();

            manageOneLineFamousWord.UpdateOneLineFamousWord(param);

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> ModifyOneLineFamousWord(int Id)
        {
            ManageOneLineFamousWord manageOneLineFamousWord = new ManageOneLineFamousWord();

            List<OneLineFamousWords> value = manageOneLineFamousWord.getOneLineFamousWord(Id);
            return View(value[0]);
        }
 

    }
}



using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize]
    public class AIExperienceController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public AIExperienceController(IAuthService authService,
            UserManager<ApplicationUser> userManager,
                 IWebHostEnvironment hostingEnvironment) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<IActionResult> Index()
        { 
            return View();
        }
        public async Task<IActionResult> ChatWithAI(string meth)
        {
            ViewBag.Meth = meth;
            return View();
        }
        public async Task<IActionResult> AIAskHistory()
        {
            ManageAIExperience manageAIExperience = new ManageAIExperience();
            return View(manageAIExperience.getAIAskHistory(0));
        }
        [AllowAnonymous]
        public async Task<IActionResult> WhatAI()
        { 
            return View();
        }

        //
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(AIExperienceController.Index), "Home");
        }         
    }
}



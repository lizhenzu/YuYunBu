using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services.WorldView.Admin;
using BulletinBoard.ViewModels.Worldview;
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
   
    public class ResourceBoxController : BaseController
    {
      

        public ResourceBoxController()
        {
            
        }

        public IActionResult Index() => View();
        public IActionResult worldView2025() => View();
        public IActionResult TeacherSeminarfortheFirstHalfof2026() => View();
    }
}
 

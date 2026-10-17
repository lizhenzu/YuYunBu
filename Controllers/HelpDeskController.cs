using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class HelpDeskController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public HelpDeskController(IAuthService authService,
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
         
        [HttpPost] 
        public async Task<IActionResult> CreateRepairHomePage([FromBody] RepairHomePageData param)
        {
            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

            ManageHelpDesk manageHelpDesk = new ManageHelpDesk();

            if(string.IsNullOrEmpty(param.UserName))
            {
                param.UserName = UserName;
            }
            manageHelpDesk.CreateRepairHomePage(param);

            //return RedirectToAction(nameof(DailyMissionController.TodayQuestion ), "DailyMission");
            return Json("OK");
        }


        //public async Task<IActionResult> HelpDeskDetailList2()
        //{
        //    ManageHelpDesk manageHelpDesk = new ManageHelpDesk();

        //    return View(manageHelpDesk.getRepairHomePage(0));
        //}

        public async Task<IActionResult> DeleteRepairHomePage(int Id)
        {
            ManageHelpDesk manageHelpDesk = new ManageHelpDesk();

            manageHelpDesk.DeleteRepairHomePage(Id);

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> ModifyRepairHomePage(int Id)
        {
            ManageHelpDesk manageHelpDesk = new ManageHelpDesk();

            List<RepairHomePageDatas> value = manageHelpDesk.getRepairHomePage(Id);
            RepairHomePageData data = value[0];
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> ModifyRepairHomePageProcess(RepairHomePageData param)
        {
            ManageHelpDesk manageHelpDesk = new ManageHelpDesk();

            manageHelpDesk.UpdateRepairHomePage(param);

            return RedirectToAction("Index");
        }
   
          
    }
}



using BulletinBoard.BusinessLayer;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class PraiseHomeController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public PraiseHomeController(IAuthService authService,
            UserManager<ApplicationUser> userManager) : base(authService, userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        // GET: JobCategory
        public async Task<IActionResult> Index()
        {
            ManagePraiseContents managePraiseContents = new ManagePraiseContents();
            List<PraiseContents> values = managePraiseContents.getPraiseContents("conti", 0);
            return View(values); 
        }

        // GET: JobCategory
        public async Task<IActionResult> IndexBranchOld()
        {
            return View();
        }
        // GET: JobCategory
        public async Task<IActionResult> IndexBranch()
        {
            return View();
        }

        // GET: JobCategory
        public async Task<IActionResult> IndexQuery(string Kind)
        {
            ManagePraiseContents managePraiseContents = new ManagePraiseContents();
            List<PraiseContents> values = managePraiseContents.getPraiseContents(Kind, 0 );
            ViewBag.Kind = Kind;
            return View(values);
        }

        public async Task<IActionResult> EnrollView(string kind)
        {
            PraiseContent praiseContent = new PraiseContent();
            ViewBag.Kind = kind;
            return View(praiseContent);
        }

        public async Task<IActionResult> ModifyView(int Id, string Kind)
        {
            ManagePraiseContents managePraiseContents = new ManagePraiseContents();
            var data = managePraiseContents.getPraiseContents(string.Empty, Id);
            ViewBag.Kind = Kind;
            return View(data[0]);
        }


        public async Task<IActionResult> ModifyViewProcess(PraiseContent param)
        {
            ManagePraiseContents managePraiseContents = new ManagePraiseContents();
            managePraiseContents.UpdatePraiseContents(param);
            return RedirectToAction(nameof(PraiseHomeController.IndexQuery), "PraiseHome", new { Kind = param.Kind });
        }


        public async Task<IActionResult> EnrollViewProcess(PraiseContent param)
        {
            ManagePraiseContents managePraiseContents = new ManagePraiseContents();
            managePraiseContents.CreatePraiseContents(param);

            return RedirectToAction(nameof(PraiseHomeController.IndexQuery), "PraiseHome", new { Kind = param.Kind});
        }

        public async Task<IActionResult> DeleteView(int Id, string Kind)
        {
            ManagePraiseContents managePraiseContents = new ManagePraiseContents();
            managePraiseContents.DeletePraiseContents(Id);
            return RedirectToAction(nameof(PraiseHomeController.IndexQuery), "PraiseHome", new { Kind = Kind });
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(PraiseHomeController.Index), "PraiseContents");
        }
    }
}



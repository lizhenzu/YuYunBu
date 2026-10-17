using System.Linq;
using System.Threading.Tasks;
using BulletinBoard.DataAccessLayer.SuperAdmin;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Models.SuperAdmin;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.Controllers
{
    [Authorize(Roles = nameof(Roles.SuperAdmin))]
    [LogActionFilter]
    public class SuperAdminAnomalyController : BaseController
    {
        private readonly AnomalyRepository _repo;

        public SuperAdminAnomalyController(
            IConfiguration config,
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostingEnvironment
        ) : base(authService, userManager, hostingEnvironment)
        {
            _repo = new AnomalyRepository(config);
        }

        public async Task<IActionResult> Index()
        {
            var identity = await _repo.GetIdentityDuplicateNamesAsync();
            var pm = await _repo.GetPersonMasterDuplicateFullNamesAsync();

            var vm = new AnomalyDashboardVM
            {
                IdentityDupRows = identity,
                PersonMasterDupRows = pm,

                IdentityDupGroupCount = identity.Select(x => x.FullNameKey).Distinct().Count(),
                IdentityDupUserCount = identity.Count,

                PersonMasterDupGroupCount = pm.Select(x => x.FullName).Distinct().Count(),
                PersonMasterDupRowCount = pm.Count
            };

            return View(vm);
        }
    }
}

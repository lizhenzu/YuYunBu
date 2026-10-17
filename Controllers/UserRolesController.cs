using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Common;
using Microsoft.AspNetCore.Http;

namespace BulletinBoard.Controllers
{
    [Authorize(Roles = nameof(Roles.SuperAdmin))]
    public class UserRolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserRolesController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _roleManager   = roleManager;
            _userManager   = userManager;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderBy(x => (x.LastName + x.FirstName)).ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (ApplicationUser user in users)
            {
                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.FirstName = user.FirstName;
                thisViewModel.LastName = user.LastName;
                thisViewModel.Roles = await GetUserRoles(user);
                userRolesViewModel.Add(thisViewModel);
                thisViewModel.InsertDT = user.InsertDT;   // ✅ 추가
                thisViewModel.PhoneNumber = user.PhoneNumber;   // ✅ 추가
            }
            return View(userRolesViewModel);
        }

        public async Task<IActionResult> Delete(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return Redirect("/UserRoles/Index");
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        public async Task<IActionResult> ImpersonateUser(string userId)
        {
            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                return NotFound();

            // 실제 인증 쿠키를 대상 유저로 교체 (비영속 = 브라우저 닫으면 자동 해제)
            await _signInManager.SignInAsync(targetUser, isPersistent: false);

            // 기존 세션 사용자 정보 초기화 → BaseController가 새 Identity 클레임으로 재설정
            HttpContext.Session.Remove(HttpContextSessionCategory.UserFullName.ToString());
            HttpContext.Session.Remove(HttpContextSessionCategory.UserId.ToString());
            HttpContext.Session.Remove(HttpContextSessionCategory.UserName.ToString());
            HttpContext.Session.Remove(HttpContextSessionCategory.DeptCode.ToString());
            HttpContext.Session.Remove(HttpContextSessionCategory.DeptNM.ToString());

            return Redirect("/Home/Index");
        }


        public async Task<IActionResult> Manage(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            ViewBag.FullName = $"{user.LastName}{user.FirstName}".Trim();
            ViewBag.Email = user.Email;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("Index");
        }
        private async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }
    }
}
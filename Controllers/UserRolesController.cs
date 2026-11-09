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
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public UserRolesController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _roleManager   = roleManager;
            _userManager   = userManager;
            _signInManager = signInManager;
            _env           = env;
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
                thisViewModel.UserName = user.UserName;
            }
            // 인별 소속 — Common/UserAffiliation (역할은 UserRole 기준)
            var aff = UserAffiliation.LoadAll();
            foreach (var vm in userRolesViewModel)
            {
                if (!aff.TryGetValue(vm.UserId ?? "", out var a)) a = new UserAffiliationInfo();
                vm.Kind = a.Kind; vm.RoleNM = a.RoleNM; vm.ClassYear = a.ClassYear; vm.ClassBan = a.ClassBan;
                vm.TeachClasses = a.TeachClasses; vm.Pending = a.Pending; vm.AffLabel = a.Label;
            }

            // 인별 최종 접속 — dbo.loginHistory (사용자별 마지막 기록 1건, 시각은 한국 시간으로 저장됨)
            var lastAccess = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var ds = DataAccessModule.commonQuery("select UserName, max(InsertDT) LastAt from [dbo].[loginHistory] group by UserName");
                if (ds.Tables.Count > 0)
                    foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                        if (row["UserName"] is string un && row["LastAt"] is DateTime at) lastAccess[un.Trim()] = at;
            }
            catch { /* 접속 기록은 부가 정보 — 못 읽어도 화면은 그대로 */ }
            foreach (var vm in userRolesViewModel)
                if (lastAccess.TryGetValue((vm.UserName ?? "").Trim(), out var at)) vm.LastAccessAt = at;

            // 도전 성경말씀 가족팀 · 친구팀 — 활동 중인 팀의 활동 중인 구성원만
            try
            {
                var ds = DataAccessModule.commonQuery(@"
select m.UserName, ft.FamilyTeamId, ft.TeamName, isnull(ft.TeamType, N'FAMILY') TeamType, isnull(ft.CreatorUserName, N'') CreatorUserName, m.RoleCode
from dbo.DM2_FamilyMember m
join dbo.DM2_FamilyTeam ft on ft.FamilyTeamId = m.FamilyTeamId and ft.IsActive = 1
where m.IsActive = 1
order by ft.FamilyTeamId, m.RoleCode, m.RelationCode, m.SortNo");
                if (ds.Tables.Count > 0)
                {
                    var byUser = userRolesViewModel.Where(v => !string.IsNullOrWhiteSpace(v.UserName))
                        .GroupBy(v => v.UserName.Trim(), StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
                    var rows = ds.Tables[0].Rows.Cast<System.Data.DataRow>().Select(r => new
                    {
                        UserName = Convert.ToString(r["UserName"])?.Trim() ?? "",
                        TeamId   = Convert.ToInt32(r["FamilyTeamId"]),
                        TeamName = Convert.ToString(r["TeamName"]) ?? "",
                        TeamType = Convert.ToString(r["TeamType"]) ?? "FAMILY",
                        Creator  = Convert.ToString(r["CreatorUserName"]) ?? "",
                        Rel      = BulletinBoard.Services.FamilyTeamService.TypeFriend == (Convert.ToString(r["TeamType"]) ?? "")
                                   ? "친구"
                                   : BulletinBoard.Services.FamilyTeamService.LabelOf(
                                         BulletinBoard.Services.FamilyTeamService.KeyOfRole(Convert.ToInt32(r["RoleCode"]))),
                    }).ToList();

                    foreach (var team in rows.GroupBy(r => r.TeamId))
                    {
                        foreach (var me in team)
                        {
                            if (!byUser.TryGetValue(me.UserName, out var vm) || vm.TeamName.Length > 0) continue;
                            vm.TeamName = me.TeamName;
                            vm.TeamType = me.TeamType;
                            vm.TeamRelation = me.Rel;
                            vm.TeamIsOwner = string.Equals(me.Creator, me.UserName, StringComparison.OrdinalIgnoreCase);
                            vm.TeamMates = team.Where(o => !string.Equals(o.UserName, me.UserName, StringComparison.OrdinalIgnoreCase))
                                .Select(o =>
                                {
                                    string nm = byUser.TryGetValue(o.UserName, out var ov) ? $"{ov.LastName}{ov.FirstName}".Trim() : "";
                                    if (nm.Length == 0) nm = o.UserName;
                                    return $"{nm}({o.Rel})";
                                }).ToList();
                        }
                    }
                }
            }
            catch { /* 팀 정보는 부가 정보 — 못 읽어도 화면은 그대로 */ }

            return View(userRolesViewModel);
        }

        // ── 계정 삭제 (2026-11-08) — 탈퇴 승인(/Withdraw/Decide)과 같은 범위로 지운다 ──
        //   ① 기록(한 트랜잭션, ControlWithdraw.DeleteDataAsync — 가족팀 방장 이어받기 포함) → ② 프로필 사진 → ③ 로그인 계정
        //   중간에 실패하면 계정이 남아 다시 지울 수 있다. 내 계정 · SuperAdmin 계정은 못 지운다.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId ?? "");
            if (user == null)
            {
                TempData["UrMsg"] = "그 계정을 찾지 못했어요.";
                return RedirectToAction(nameof(Index));
            }
            if (string.Equals(user.Id, _userManager.GetUserId(User), StringComparison.Ordinal))
            {
                TempData["UrMsg"] = "내 계정은 탈퇴 처리할 수 없어요.";
                return RedirectToAction(nameof(Index));
            }
            if (await _userManager.IsInRoleAsync(user, nameof(Roles.SuperAdmin)))
            {
                TempData["UrMsg"] = "SuperAdmin 계정은 탈퇴 처리할 수 없어요.";
                return RedirectToAction(nameof(Index));
            }

            string name = $"{user.LastName}{user.FirstName}".Trim();
            try
            {
                var wd = new BulletinBoard.DataAccessLayer.ControlWithdraw();
                var pendingReq = await wd.GetPendingOfAsync(user.Id);   // 탈퇴 요청이 걸려 있으면 같이 처리 표시
                await wd.DeleteDataAsync(user.Id, user.UserName ?? "");
                BulletinBoard.DataAccessLayer.ControlWithdraw.DeleteProfileFiles(_env.WebRootPath, user.Id);

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    TempData["UrMsg"] = "기록은 지웠지만 로그인 계정을 지우지 못해 탈퇴가 끝나지 않았어요: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index));
                }
                if (pendingReq != null)
                {
                    try { await wd.SetStatusAsync(pendingReq.Id, "APPROVED", User.Identity?.Name); } catch { }
                }
                _ = BulletinBoard.Helpers.DbLogger.LogDebugAsync($"UserRoles Delete by {User.Identity?.Name} → {user.UserName}");
                TempData["UrMsg"] = $"{name} 님을 탈퇴 처리했어요. 계정과 기록을 모두 지웠어요.";
            }
            catch (Exception ex)
            {
                TempData["UrMsg"] = "탈퇴 처리하지 못했어요: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
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
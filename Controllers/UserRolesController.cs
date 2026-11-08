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
            FillAffiliation(userRolesViewModel);
            return View(userRolesViewModel);
        }

        // ── 인별 소속 (2026-11-08) — 역할은 Common/UserRole.RoleCodeSql 한 곳 기준 ──
        //   학생: 몇 학년 몇 반 · 교사: 담당 반(담임 · 부담임) · 임원: 직분 이름 · 기타 · 학부모
        private static void FillAffiliation(List<UserRolesViewModel> list)
        {
            var byId = list.Where(x => !string.IsNullOrEmpty(x.UserId)).ToDictionary(x => x.UserId);
            try
            {
                string sql = $@"
SELECT u.[Id] AS uid, x.rc,
       ISNULL((SELECT TOP 1 cd.[MinorNM] FROM [dbo].[CodeData] cd WHERE cd.[MajorCD] = 'T001' AND cd.[MinorCD] = x.rc), N'') AS rnm,
       ISNULL(s.[ClassYear], 0) AS cy, ISNULL(s.[ClassBan], 0) AS cb,
       CASE WHEN x.rc = N'' AND EXISTS (SELECT 1 FROM [dbo].[SignupApplication] sa WHERE sa.[UserName] = u.[UserName] AND sa.[Status] = N'PENDING' AND sa.[ReqType] = N'JOIN')
            THEN (SELECT TOP 1 sa.[RoleKind] FROM [dbo].[SignupApplication] sa WHERE sa.[UserName] = u.[UserName] AND sa.[Status] = N'PENDING' AND sa.[ReqType] = N'JOIN' ORDER BY sa.[Id] DESC)
            ELSE N'' END AS pk
FROM [Identity].[User] u
CROSS APPLY (SELECT {UserRole.RoleCodeSql("u.[Id]")} AS rc) x
OUTER APPLY (SELECT TOP 1 c.[ClassYear], c.[ClassBan] FROM [dbo].[view_UserCompositeInfo_2026] c
             WHERE c.[UserName] = u.[UserName] AND ISNULL(c.[ClassYear], 0) > 0 ORDER BY c.[ClassYear], c.[ClassBan]) s;

SELECT DISTINCT ap.[userid] AS uid, m.[ClassYear] AS cy, m.[ClassBan] AS cb,
       CASE WHEN m.[TeacherPersonId] = ap.[personid] THEN 0 ELSE 1 END AS sub
FROM [dbo].[ClassTeacherMap_2026] m
JOIN [dbo].[view_All_Person_2026] ap ON ap.[personid] IN (m.[TeacherPersonId], m.[SubTeacherPersonId])
WHERE ISNULL(m.[IsActive], 0) = 1 AND ISNULL(ap.[personid], 0) <> 0 AND ap.[userid] IS NOT NULL
ORDER BY cy, cb;";
                var ds = DataAccessModule.commonQuery(sql);
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (System.Data.DataRow r in ds.Tables[0].Rows)
                    {
                        if (!byId.TryGetValue(Convert.ToString(r["uid"]) ?? "", out var vm)) continue;
                        string rc = Convert.ToString(r["rc"]) ?? "";
                        string pk = (Convert.ToString(r["pk"]) ?? "").Trim();
                        vm.Kind = UserRole.KindOf(rc);
                        if (rc.Trim().Length == 0 && pk.Length > 0)
                        {
                            vm.Pending = true;
                            vm.Kind = pk switch { "S" => UserRoleKind.Student, "T" => UserRoleKind.Teacher, "E" => UserRoleKind.Etc, _ => UserRoleKind.Parent };
                        }
                        vm.RoleNM = Convert.ToString(r["rnm"]) ?? "";
                        vm.ClassYear = Convert.ToInt32(r["cy"]);
                        vm.ClassBan = Convert.ToInt32(r["cb"]);
                    }
                }
                if (ds != null && ds.Tables.Count > 1)
                {
                    foreach (System.Data.DataRow r in ds.Tables[1].Rows)
                    {
                        if (!byId.TryGetValue(Convert.ToString(r["uid"]) ?? "", out var vm)) continue;
                        string t = $"{Convert.ToInt32(r["cy"])}학년 {Convert.ToInt32(r["cb"])}반 {(Convert.ToInt32(r["sub"]) == 1 ? "부담임" : "담임")}";
                        if (!vm.TeachClasses.Contains(t)) vm.TeachClasses.Add(t);
                    }
                }
            }
            catch (Exception ex)
            {
                BulletinBoard.Helpers.DbLogger.LogDebugAsync("UserRoles 소속 조회 실패: " + ex.Message);
            }

            foreach (var vm in list)
            {
                string lbl = vm.Kind switch
                {
                    UserRoleKind.Student => vm.ClassYear > 0 ? $"학생 · {vm.ClassYear}학년 {vm.ClassBan}반" : "학생 · 반 없음",
                    UserRoleKind.Teacher => vm.TeachClasses.Count > 0 ? $"교사 · {string.Join(", ", vm.TeachClasses)}" : "교사",
                    UserRoleKind.Officer => (string.IsNullOrWhiteSpace(vm.RoleNM) ? "임원" : $"임원 · {vm.RoleNM}")
                                            + (vm.TeachClasses.Count > 0 ? $" · {string.Join(", ", vm.TeachClasses)}" : ""),
                    _ => UserRole.LabelOf(vm.Kind),
                };
                vm.AffLabel = vm.Pending ? lbl + " (승인 대기)" : lbl;
            }
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
                TempData["UrMsg"] = "내 계정은 삭제할 수 없어요.";
                return RedirectToAction(nameof(Index));
            }
            if (await _userManager.IsInRoleAsync(user, nameof(Roles.SuperAdmin)))
            {
                TempData["UrMsg"] = "SuperAdmin 계정은 삭제할 수 없어요.";
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
                    TempData["UrMsg"] = "기록은 지웠지만 계정을 지우지 못했어요: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index));
                }
                if (pendingReq != null)
                {
                    try { await wd.SetStatusAsync(pendingReq.Id, "APPROVED", User.Identity?.Name); } catch { }
                }
                _ = BulletinBoard.Helpers.DbLogger.LogDebugAsync($"UserRoles Delete by {User.Identity?.Name} → {user.UserName}");
                TempData["UrMsg"] = $"{name} 님의 계정과 기록을 모두 지웠어요.";
            }
            catch (Exception ex)
            {
                TempData["UrMsg"] = "삭제하지 못했어요: " + ex.Message;
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
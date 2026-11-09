using Microsoft.AspNetCore.Mvc;
using BulletinBoard.Services;
using BulletinBoard.Models.DailyMission2;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace BulletinBoard.Controllers
{
    [Authorize(Roles = nameof(BulletinBoard.Enums.Roles.SuperAdmin))]   // 2026-10-04: 가족팀 관리(보기 · 만들기 · 수정 · 삭제)는 SuperAdmin 전용
    public class DailyMission2FamilyAdminController : Controller
    {
        private readonly DailyMission2FamilyAdminService _svc;
        private readonly FamilyTeamService _teamSvc;

        public DailyMission2FamilyAdminController(DailyMission2FamilyAdminService svc, FamilyTeamService teamSvc)
        {
            _svc = svc;
            _teamSvc = teamSvc;
        }

        // GET: /DailyMission2FamilyAdmin
        public async Task<IActionResult> Index(int? teamId)
        {
            var vm = new FamilyAdminVM();
            vm.Teams = await _svc.GetTeamsAsync();

            if (teamId.HasValue)
            {
                vm.SelectedTeamId = teamId.Value;
                vm.SelectedTeam = await _svc.GetTeamAsync(teamId.Value);
                if (vm.SelectedTeam != null)
                {
                    vm.Members = await _svc.GetMembersAsync(teamId.Value);
                    vm.Requests = await _svc.GetRequestsAsync(teamId.Value);   // 대기 중인 가입 신청 · 초대
                }
            }

            ViewBag.Error = TempData["Error"] as string;
            ViewBag.Info = TempData["Info"] as string;
            ViewBag.AwardLockEnabled = await _teamSvc.GetAwardLockEnabledAsync();

            return View(vm);
        }

        /// <summary>시상 잠금 스위치 켜기/끄기 (시상 기록이 있는 팀의 나가기 · 내보내기 · 역할 바꾸기)</summary>
        [HttpPost]
        public async Task<IActionResult> SetAwardLock(bool enabled)
        {
            bool ok = await _teamSvc.SetAwardLockEnabledAsync(enabled);
            if (ok) TempData["Info"] = enabled ? "시상 잠금을 켰어요." : "시상 잠금을 껐어요.";
            else TempData["Error"] = "설정 표가 아직 없어요. SQL/TeamAward_20261109.sql 을 먼저 실행해 주세요.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam(string teamName, string? memo)
        {
            if (string.IsNullOrWhiteSpace(teamName))
                return RedirectToAction(nameof(Index));

            var id = await _svc.CreateTeamAsync(teamName.Trim(), memo);

            BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext); // 홈 가족 마일스톤 캐시 즉시 비움
            TempData["Info"] = "팀이 추가되었습니다.";
            return RedirectToAction(nameof(Index), new { teamId = id });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTeam(int teamId, string teamName, bool isActive, string? memo)
        {
            if (teamId <= 0) return RedirectToAction(nameof(Index));
            if (string.IsNullOrWhiteSpace(teamName)) teamName = "가족팀";

            await _svc.UpdateTeamAsync(teamId, teamName.Trim(), isActive, memo);

            BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext); // 홈 가족 마일스톤 캐시 즉시 비움
            TempData["Info"] = "팀 정보가 저장되었습니다.";
            return RedirectToAction(nameof(Index), new { teamId });
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(int teamId, string userName, int roleCode, int sortNo, int relationCode)
        {
            if (teamId <= 0) return RedirectToAction(nameof(Index));
            if (string.IsNullOrWhiteSpace(userName)) return RedirectToAction(nameof(Index), new { teamId });

            try
            {
                await _svc.AddMemberAsync(teamId, userName.Trim(), roleCode, sortNo, relationCode);
                BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext); // 홈 가족 마일스톤 캐시 즉시 비움
                TempData["Info"] = "구성원이 추가되었습니다.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "저장 실패: " + FriendlySqlError(ex);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = "저장 실패: " + ex.Message;   // 역할 기준에 맞지 않음
            }

            return RedirectToAction(nameof(Index), new { teamId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMember(int teamId, string userName, int roleCode, int sortNo, bool isActive, int relationCode)
        {
            if (teamId <= 0) return RedirectToAction(nameof(Index));
            if (string.IsNullOrWhiteSpace(userName)) return RedirectToAction(nameof(Index), new { teamId });

            try
            {
                await _svc.UpdateMemberAsync(teamId, userName.Trim(), roleCode, sortNo, isActive, relationCode);
                BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext); // 홈 가족 마일스톤 캐시 즉시 비움
                TempData["Info"] = "구성원 정보가 저장되었습니다.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "저장 실패: " + FriendlySqlError(ex);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = "저장 실패: " + ex.Message;   // 역할 기준에 맞지 않음
            }

            return RedirectToAction(nameof(Index), new { teamId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMember(int teamId, string userName)
        {
            if (teamId <= 0) return RedirectToAction(nameof(Index));
            if (string.IsNullOrWhiteSpace(userName)) return RedirectToAction(nameof(Index), new { teamId });

            await _svc.DeleteMemberAsync(teamId, userName.Trim());

            BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext); // 홈 가족 마일스톤 캐시 즉시 비움
            TempData["Info"] = "비활성 처리되었습니다.";
            return RedirectToAction(nameof(Index), new { teamId });
        }

        // 가입 신청 수락 — 팀원 대신 관리자가 받아 준다 (초대는 초대받은 본인이 수락해야 해서 지우기만 가능)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int teamId, int requestId)
        {
            if (teamId <= 0) return RedirectToAction(nameof(Index));

            var res = await _teamSvc.AdminAcceptRequestAsync(teamId, requestId);
            if (res.Ok)
            {
                BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext);
                TempData["Info"] = res.Message;
            }
            else TempData["Error"] = res.Message;
            return RedirectToAction(nameof(Index), new { teamId });
        }

        // 가입 신청 거절 · 초대 취소 — 신청(초대) 한 건을 지운다
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRequest(int teamId, int requestId)
        {
            if (teamId <= 0) return RedirectToAction(nameof(Index));

            if (await _teamSvc.AdminRemoveRequestAsync(teamId, requestId)) TempData["Info"] = "신청을 지웠어요.";
            else TempData["Error"] = "이미 처리됐거나 사라진 신청이에요.";
            return RedirectToAction(nameof(Index), new { teamId });
        }

        // 팀 강제 삭제 — 일반 사용자는 팀을 지울 수 없고 관리자만 할 수 있다
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            if (teamId <= 0) return RedirectToAction(nameof(Index));

            await _svc.DeleteTeamAsync(teamId);

            BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext);
            TempData["Info"] = "팀을 삭제했습니다.";
            return RedirectToAction(nameof(Index));
        }

        // 방장 바꾸기
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCreator(int teamId, string userName)
        {
            if (teamId <= 0) return RedirectToAction(nameof(Index));

            bool ok = !string.IsNullOrWhiteSpace(userName) && await _svc.SetCreatorAsync(teamId, userName);
            if (ok) TempData["Info"] = "방장을 바꿨습니다.";
            else TempData["Error"] = "이 팀의 활성 구성원만 방장으로 정할 수 있습니다.";
            return RedirectToAction(nameof(Index), new { teamId });
        }

        private static string FriendlySqlError(SqlException ex)
        {
            // 팀 내 본인(RelationCode=0) 중복 방지 인덱스가 있을 경우 흔히 2601/2627 발생
            if (ex.Number == 2601 || ex.Number == 2627)
                return "본인(RelationCode=0)은 한 팀에 1명만 설정할 수 있습니다.";

            return ex.Message;
        }
    }
}

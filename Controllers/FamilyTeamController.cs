using BulletinBoard.Helpers;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Controllers
{
    /// <summary>
    /// 가족팀 직접 구성 (카톡 방식) — 팀 만들기 · 가입 신청 · 초대 · 수락 · 탈퇴 · 내보내기
    /// 규칙은 FamilyTeamService 머리말 참고. 화면: Views/FamilyTeam (Index = 팀 없음, Team = 팀 있음)
    /// </summary>
    [Authorize]
    public class FamilyTeamController : Controller
    {
        private readonly FamilyTeamService _svc;
        private readonly IStudentPointGiftService _gift;

        public FamilyTeamController(FamilyTeamService svc, IStudentPointGiftService gift)
        {
            _svc = svc;
            _gift = gift;
        }

        private string Me => User.Identity?.Name ?? "";

        // GET: /FamilyTeam  — 팀이 있으면 우리 가족팀, 없으면 팀 만들기 · 찾기
        [HttpGet]
        public async Task<IActionResult> Index(string? type = null)
        {
            ViewBag.Msg = TempData["FamilyMsg"] as string;
            ViewBag.MsgOk = TempData["FamilyMsgOk"] is bool b && b;
            ViewBag.Relations = FamilyTeamService.Relations;

            try
            {
                var detail = await _svc.GetTeamDetailAsync(Me);
                if (detail != null)
                {
                    if (detail.IsFriend)
                    {
                        try { ViewBag.FriendBoard = await _gift.GetMyFriendGiftBoardAsync(Me, DateHelper.GetKoreaNow().Year); }
                        catch { ViewBag.FriendBoard = null; }
                        return View("FriendTeam", detail);
                    }

                    try { ViewBag.FamilyTree = await _gift.GetMyFamilyTreeAsync(Me, DateHelper.GetKoreaNow().Year); }
                    catch { ViewBag.FamilyTree = null; }
                    return View("Team", detail);
                }

                bool isStudent = await _svc.IsStudentAsync(Me);
                ViewBag.IsStudent = isStudent;
                ViewBag.Invites = await _svc.GetIncomingInvitesAsync(Me);

                // 팀이 없으면 먼저 종류를 고른다. 친구팀은 학생만
                string? mode = NormalizeType(type);
                if (mode == null || (mode == FamilyTeamService.TypeFriend && !isStudent))
                    return View("Choose");

                ViewBag.Mode = mode;
                ViewBag.Teams = await _svc.ListOpenTeamsAsync(Me, mode);
                return View("Index");
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                // 표(DM2_FamilyRequest / DM2_FamilyNotice)를 아직 만들지 않았을 때
                ViewBag.Msg = "가족팀 기능 준비 중이에요. 잠시 후 다시 들어와 주세요.";
                ViewBag.MsgOk = false;
                ViewBag.IsStudent = false;
                ViewBag.Invites = new List<FamilyRequestInfo>();
                return View("Choose");
            }
        }

        // GET: /FamilyTeam/Search?q=  — 초대할 사람 이름 검색 (JSON)
        [HttpGet]
        public async Task<IActionResult> Search(string? q)
        {
            var hits = await _svc.SearchUsersAsync(Me, q ?? "");
            return Json(hits.Select(h => new
            {
                teamType = h.TeamType,
                isFull = h.IsFull,
                isStudent = h.IsStudent,
                userName = h.UserName,
                fullName = h.FullName,
                state = h.State,
                teamId = h.TeamId,
                teamName = h.TeamName,
                members = h.MemberNames,
                requested = h.Requested,
            }));
        }

        private static string? NormalizeType(string? type)
            => string.Equals(type, "friend", StringComparison.OrdinalIgnoreCase) || string.Equals(type, FamilyTeamService.TypeFriend, StringComparison.OrdinalIgnoreCase) ? FamilyTeamService.TypeFriend
             : string.Equals(type, "family", StringComparison.OrdinalIgnoreCase) || string.Equals(type, FamilyTeamService.TypeFamily, StringComparison.OrdinalIgnoreCase) ? FamilyTeamService.TypeFamily
             : null;

        private IActionResult Done(FamilyTeamResult r, string? type = null)
        {
            TempData["FamilyMsg"] = r.Message;
            TempData["FamilyMsgOk"] = r.Ok;
            if (r.Ok) HomeCacheHelper.InvalidateFamilyMilestones(HttpContext);   // 팀 구성 · 이름이 바뀌면 홈 가족 마일스톤도 새로
            return RedirectToAction(nameof(Index), new { type });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? relation, string? type) => Done(await _svc.CreateTeamAsync(Me, relation, NormalizeType(type) ?? FamilyTeamService.TypeFamily), type);

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int teamId, string? relation, string? type) => Done(await _svc.ApplyAsync(Me, teamId, relation), type);

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int requestId, string? type) => Done(await _svc.CancelAsync(Me, requestId), type);

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int requestId) => Done(await _svc.AcceptRequestAsync(Me, requestId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineRequest(int requestId) => Done(await _svc.DeclineRequestAsync(Me, requestId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(string targetUserName, string? relation) => Done(await _svc.InviteAsync(Me, targetUserName, relation));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptInvite(int requestId) => Done(await _svc.AcceptInviteAsync(Me, requestId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineInvite(int requestId) => Done(await _svc.DeclineInviteAsync(Me, requestId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave() => Done(await _svc.LeaveAsync(Me));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Kick(string targetUserName) => Done(await _svc.KickAsync(Me, targetUserName));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRelation(string targetUserName, string relation) => Done(await _svc.ChangeRelationAsync(Me, targetUserName, relation));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Rename(string? teamName) => Done(await _svc.RenameTeamAsync(Me, teamName));
    }
}

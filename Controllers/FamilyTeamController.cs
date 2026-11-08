using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

        private readonly UserManager<ApplicationUser> _userManager;

        public FamilyTeamController(FamilyTeamService svc, IStudentPointGiftService gift, UserManager<ApplicationUser> userManager)
        {
            _svc = svc;
            _gift = gift;
            _userManager = userManager;
        }

        private string Me => User.Identity?.Name ?? "";

        /// <summary>화면 맨 위 인사 — "안녕하세요 / ○○님 / 유년부에 오심을 환영해요" (홈 · 미션 허브와 같은 문구)</summary>
        private async Task SetGreetingAsync()
        {
            try
            {
                var me = await _userManager.GetUserAsync(User);
                ViewBag.HiName = me == null ? "" : ((me.LastName ?? "") + (me.FirstName ?? "")).Trim();

                var key = CommonMetaDataCategory.DepartmentName.ToString();
                var dept = HttpContext.Session.GetString(key);
                if (string.IsNullOrEmpty(dept))
                {
                    foreach (var m in new ManageeCommon().getCommonMetaData())
                        HttpContext.Session.SetString(m.Kind.ToString(), m.KindDesc);
                    dept = HttpContext.Session.GetString(key);
                }
                ViewBag.HiDept = dept ?? "";
            }
            catch { ViewBag.HiName = ""; ViewBag.HiDept = ""; }
        }

        // GET: /FamilyTeam  — 팀이 있으면 우리 가족팀, 없으면 팀 만들기 · 찾기
        [HttpGet]
        public async Task<IActionResult> Index(string? type = null, string? create = null, string? step = null)
        {
            // 팀이 없을 때의 흐름 (2026-11-08)
            //   1단계 (주소에 아무것도 없음) : 아직 소속된 팀이 없어요 → [기존 팀에 참가] / [새 팀 만들기]
            //   ?step=join                   : 이름으로 팀 찾기 (가족팀 · 친구팀 같이) → 가입 신청
            //   ?step=make                   : 어떤 팀을 만들까요? (가족팀 / 친구팀)
            //   ?type=family&create=true     : 관계 고르고 팀 만들기
            // ?create=1 · true 둘 다 받는다 — bool 로 받으면 "1" 을 못 읽어 눌러도 같은 화면이 다시 떴다.
            ViewBag.Create = string.Equals(create, "1", StringComparison.Ordinal)
                          || string.Equals(create, "true", StringComparison.OrdinalIgnoreCase);
            ViewBag.Msg = TempData["FamilyMsg"] as string;
            ViewBag.MsgOk = TempData["FamilyMsgOk"] is bool b && b;
            ViewBag.Relations = FamilyTeamService.Relations;
            await SetGreetingAsync();

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

                // 역할 기준 (2026-11-08, Common/UserRole.cs) — 고를 수 있는 관계는 역할이 정한다. 교사 · 임원은 팀에 못 들어간다
                var kind = await _svc.GetRoleKindAsync(Me);
                bool isStudent = kind == UserRoleKind.Student;
                ViewBag.IsStudent = isStudent;
                ViewBag.Relations = FamilyTeamService.RelationsFor(kind);
                ViewBag.Invites = await _svc.GetIncomingInvitesAsync(Me);
                if (!UserRole.CanJoinFamily(kind))
                {
                    ViewBag.RoleBlocked = UserRole.LabelOf(kind);
                    return View("Choose");
                }

                // 친구팀은 학생만
                string? mode = NormalizeType(type);
                if (mode == FamilyTeamService.TypeFriend && !isStudent) mode = null;
                bool wantCreate = ViewBag.Create is bool cr && cr;
                string st = (step ?? "").Trim().ToLowerInvariant();

                // 만들기 — 종류를 정했으면 관계 고르고 만들기, 아직이면 종류부터
                if (st == "make" || wantCreate)
                {
                    if (wantCreate && mode != null)
                    {
                        ViewBag.Mode = mode;
                        ViewBag.PendingTeam = (await _svc.ListOpenTeamsAsync(Me, mode)).FirstOrDefault(t => t.RequestId != 0)?.Name;
                        return View("Index");
                    }
                    return View("MakeType");
                }

                // 참가 — 이름으로 찾기 (옛 주소 ?type=… 도 여기로)
                if (st == "join" || mode != null)
                {
                    ViewBag.TeamsFamily = await _svc.ListOpenTeamsAsync(Me, FamilyTeamService.TypeFamily);
                    ViewBag.TeamsFriend = isStudent
                        ? await _svc.ListOpenTeamsAsync(Me, FamilyTeamService.TypeFriend)
                        : new List<FamilyTeamBrief>();
                    return View("Join");
                }

                return View("Choose");
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
                roleKind = (int)h.RoleKind,   // 0 학부모 · 1 학생 · 2 교사 · 3 임원 (Common/UserRole.cs)
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

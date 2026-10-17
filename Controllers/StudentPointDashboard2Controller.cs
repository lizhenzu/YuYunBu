using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
// ✅ 아래 2개는 여러분 프로젝트에 이미 만든 타입들(없으면 실제 위치로 using 수정)
using BulletinBoard.Models.Gift; // PopupActionType, GiftMilestone, PopupActionRequest 등
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services.DailyMission2;
using BulletinBoard.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize]
    public class StudentPointDashboard2Controller : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly IAttendanceService _attendanceService;
        private readonly IDevotionPointsService _devotion;
        private readonly IStudentPointGiftService _gift;
        private readonly IDailyMission2Service _iDailyMission2Service;
        private readonly IStudentPointTotalRankService _totalRankService;

        // ✅ Worldview(미션북) 점수 서비스
        private readonly IWorldviewLessonService _IWorldviewLessonService;
        private readonly IAwardItemService _awardItemService;
        private readonly BulletinBoard.Services.GiftCatalog.IGiftCatalogService _giftCatalog;   // ✅ 선물 목록 조회용

        public StudentPointDashboard2Controller(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IStudentPointTotalRankService totalRankService,
            IWebHostEnvironment hostingEnvironment,
            IAttendanceService attendanceService,
            IDailyMission2Service iDailyMission2Service,
            IWorldviewLessonService lessonService,
            IDevotionPointsService devotionPointsService,
            IStudentPointGiftService giftService,
            IAwardItemService awardItemService,
            BulletinBoard.Services.GiftCatalog.IGiftCatalogService giftCatalog
        ) : base(authService, userManager, hostingEnvironment)
        {
            _giftCatalog = giftCatalog;
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;

            _attendanceService = attendanceService;
            _iDailyMission2Service = iDailyMission2Service;
            _totalRankService = totalRankService;
            _IWorldviewLessonService = lessonService;

            _devotion = devotionPointsService;
            _gift = giftService;
            _awardItemService = awardItemService;
            // ✅ Controller는 더 이상 Repo를 직접 호출하지 않습니다.
        }

        /// <summary>
        /// ✅ (공통) 서버 정답 통합점수 계산
        /// Bible(연누적) + MissionBook(대시보드 Score 합) + Thanks(총점)
        /// </summary>
        private async Task<int> ComputeIntegratedTotalAsync(int year)
        {
            // BaseController.SetUserInfo()로 UserId/UserName 채워졌다는 전제
            int bible = 0;
            int attendancePoint = 0;
            int mission = 0;
            int thanks = 0;
            int evangelism = 0;

            try
            {
                // ✅ GetYearPointAsync 사용 → DailyMission2/Index와 동일한 계산 방식으로 통일
                var yearPt = await _iDailyMission2Service.GetYearPointAsync(this.UserName, year);
                bible = yearPt?.YearTotalPoint ?? 0;
            }
            catch { bible = 0; }

            try
            {
                var attendanceCount = await _attendanceService.GetMyAttendanceCountYearToDateAsync(this.UserId);
                attendancePoint = Math.Max(0, attendanceCount) * ConstatsCommon.attendancePoint;
            }
            catch { attendancePoint = 0; }

            try
            {
                var map = await _IWorldviewLessonService.GetDashboardStatusMapAsync(this.UserId, this.UserName);
                mission = (map == null) ? 0 : map.Values.Sum(x => x == null ? 0 : x.Score);
            }
            catch { mission = 0; }

            try
            {
                thanks = await _devotion.GetMyTotalPointsAsync(this.UserId);
            }
            catch { thanks = 0; }

            try
            {
                evangelism = await _attendanceService.GetEvangelismCountByUserIdAsync(this.UserId);
            }
            catch { evangelism = 0; }

            return bible + attendancePoint + mission + thanks + (evangelism * 50);
        }

        [HttpGet]
        public async Task<IActionResult> Guide()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ✅ 로그인 사용자면 BaseController.SetUserInfo로 this.UserName / this.UserId 채우기
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            // ✅ 방어 (Authorize지만 운영 중 예외 케이스 대비)
            if (string.IsNullOrWhiteSpace(this.UserId) || string.IsNullOrWhiteSpace(this.UserName))
                return Unauthorized();

            // 기존 Branch() 패턴 유지
            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(),
                this.ControllerContext.RouteData.Values["action"]?.ToString() ?? "");
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(),
                this.ControllerContext.RouteData.Values["controller"]?.ToString() ?? "");

            var vm = new StudentPointDashboardVM
            {
                BiblePoint = 0,
                BibleReadPercent = 0,
                AttendanceCount = 0,
                AttendancePoint = 0,
                AttendanceWeeksTotal = 0,
                MissionBookPoint = 0,
                EvangelismCount = 0,
                ThanksDiaryCount = 0,
                ThanksPointTotal = 0,
                IntegratedTotal = 0,

                ShowGiftPopup = false,
                PopupMilestone = 0,
                EarnedCredit = 0,
                SpentCredit = 0,
                AvailableCredit = 0
            };

            var now = DateHelper.GetKoreaNow();

            // ✅ 1) 도전 성경말씀 포인트(연 누적)
            //    GetYearPointAsync 사용 → DailyMission2/Index 와 동일한 계산 방식으로 통일
            try
            {
                var yearPt = await _iDailyMission2Service.GetYearPointAsync(this.UserName, now.Year);
                vm.BiblePoint = yearPt?.YearTotalPoint ?? 0;
            }
            catch { vm.BiblePoint = 0; }

            // ✅ 2) 도전 성경완독(%)
            try
            {
                var manageReadBible = new ManageReadBible();
                var p = manageReadBible.GetTotalBibleProgressRate(this.UserId);
                vm.BibleReadPercent = ClampInt((double)p, 0, 100);
            }
            catch { vm.BibleReadPercent = 0; }

            // ✅ 3) 나의 출석카운트/출석포인트(올해 누적)
            try
            {
                vm.AttendanceCount = Math.Max(0, await _attendanceService.GetMyAttendanceCountYearToDateAsync(this.UserId));
                vm.AttendanceWeeksTotal = Math.Max(0, await _attendanceService.GetMyAttendanceWeeksYearToDateAsync(this.UserId));
                vm.AttendancePoint = vm.AttendanceCount * ConstatsCommon.attendancePoint;
            }
            catch
            {
                vm.AttendanceCount = 0;
                vm.AttendanceWeeksTotal = 0;
                vm.AttendancePoint = 0;
            }

            // ✅ 4) 미션북 점수(과별 Score 합)
            try
            {
                var map = await _IWorldviewLessonService.GetDashboardStatusMapAsync(this.UserId, this.UserName);
                vm.MissionBookPoint = (map == null)
                    ? 0
                    : map.Values.Sum(x => x == null ? 0 : x.Score);
            }
            catch { vm.MissionBookPoint = 0; }

            // ✅ 5) 전도 횟수
            try
            {
                vm.EvangelismCount = await _attendanceService.GetEvangelismCountByUserIdAsync(this.UserId);
            }
            catch { vm.EvangelismCount = 0; }

            // ✅ 6) 감사 총점(서버 정답)
            try
            {
                vm.ThanksPointTotal = await _devotion.GetMyTotalPointsAsync(this.UserId);
            }
            catch { vm.ThanksPointTotal = 0; }

            // ✅ 7) 통합점수(서버 정답) — 전도상: 1명당 50점
            vm.IntegratedTotal = vm.BiblePoint + vm.AttendancePoint + vm.MissionBookPoint + vm.ThanksPointTotal + (vm.EvangelismCount * 50);

            // ✅ 8) 팝업/credit 평가
            // - 평가 로직은 _gift.EvaluateAsync 유지 (현재 프로젝트 구조 최소 변경)
            try
            {
                var eval = await _gift.EvaluateAsync(this.UserName, vm.IntegratedTotal);
                vm.ShowGiftPopup = eval.Show;
                vm.PopupMilestone = eval.Milestone;
                vm.lastSeen = eval.lastSeen;
                vm.EarnedCredit = eval.EarnedCredit;
                vm.SpentMilestone = eval.spentMilestone;
                vm.SpentCredit = eval.SpentCredit;
                vm.AvailableCredit = eval.AvailableCredit;
            }
            catch
            {
                vm.ShowGiftPopup = false;
                vm.PopupMilestone = 0;
                vm.EarnedCredit = 0;
                vm.lastSeen = 0;
                vm.SpentMilestone = 0;
                vm.SpentCredit = 0;
                vm.AvailableCredit = 0;
            }
            ViewBag.ShowGiftList = await _gift.IsStudentAsync(this.UserName);
            // vm.ShowGiftPopup = await _gift.IsStudentAsync(this.UserName);

            return View("Dashboard", vm); // Views/StudentPointDashboard2/Dashboard.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PopupAction([FromBody] PopupActionRequest req)
        {
            // ✅ POST + AntiForgery로만 통일 (GET로 상태 변경 금지)
            if (await _authService.IsSignedIn(User)) await SetUserInfo();

    

            // payload 방어
            if (req == null)
                return BadRequest(new { ok = false, message = "Invalid payload" });

            // milestone 방어
            if (req.Milestone < GiftMilestone.Min ||
                req.Milestone > GiftMilestone.Max ||
                req.Milestone % GiftMilestone.Step != 0)
                return BadRequest(new { ok = false, message = "Invalid milestone" });

            // ✅ Controller → Service 단일 진입점
            await _gift.HandlePopupActionAsync(this.UserName, req.Milestone, req.ActionType);

            return Json(new { ok = true });
        }

        [HttpGet]
        public async Task<IActionResult> MyGiftBox()
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserId) || string.IsNullOrWhiteSpace(this.UserName))
                return Unauthorized();

            var now = DateHelper.GetKoreaNow();
            var integrated = await ComputeIntegratedTotalAsync(now.Year);

            var vm = await _gift.GetMyGiftBoxAsync(this.UserName, integrated);

            // ✅ Admin/SuperAdmin이면 가족팀 선물 탭 데이터도 전달
            var user       = await _authService.GetSignedUser(User);
            var isAdmin    = await _authService.IsInRole(user, nameof(Roles.Admin));
            var isSuperAdmin = await _authService.IsInRole(user, nameof(Roles.SuperAdmin));
            bool isStaff   = isAdmin || isSuperAdmin;

            if (isStaff)
            {
                // 내 가족팀 학생 UserName 목록
                var myStudents  = await _gift.GetMyFamilyStudentNamesAsync(this.UserName);
                var allRows     = await _gift.GetAdminPurchaseListAsync(includeHidden: true);

                // 내 가족팀과 관련된 선물만 필터링
                var myTeamGifts = allRows
                    .Where(r => r.IsTeamGift && myStudents.Contains(r.UserName ?? ""))
                    .ToList();

                ViewBag.TeamGiftRows = myTeamGifts;
            }
            ViewBag.IsStaff = isStaff;

            return View("~/Views/StudentPointDashboard2/MyGiftBox.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectGift(int productId)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserId) || string.IsNullOrWhiteSpace(this.UserName))
                return Unauthorized();

            var now = DateHelper.GetKoreaNow();
            var integrated = await ComputeIntegratedTotalAsync(now.Year);

            await _gift.SelectGiftAsync(this.UserName, productId, integrated);
            return RedirectToAction(nameof(MyGiftBox));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelGift(int orderId)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserId) || string.IsNullOrWhiteSpace(this.UserName))
                return Unauthorized();

            // ✅ 취소 = 완전 삭제 (현재 선택 DRAFT만 삭제)
            await _gift.CancelSelectedGiftAsync(this.UserName, orderId);

            return RedirectToAction(nameof(MyGiftBox));
        }
        // 선물 관리는 통합 화면 한 곳으로 모았다 — 예전 주소로 와도 그리로 보낸다.
        [Authorize]
        [HttpGet]
        public IActionResult AdminPurchaseList()
            => RedirectToAction("AdminPurchaseList", "StudentPointDashboard4");

        // ─────────────────────────────────────────────────────────────
        // ✅ 가족팀 선물 지급 (ParticipantTeams 페이지에서 호출)
        // ─────────────────────────────────────────────────────────────

        /// <summary>가족팀 선물 취소 (Admin/SuperAdmin)</summary>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeamGiftCancel(string studentUserName, int milestone)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            if (string.IsNullOrWhiteSpace(studentUserName))
                return Json(new { ok = false, message = "학생 정보가 없습니다." });

            try
            {
                await _gift.CancelTeamGiftAsync(studentUserName, milestone);
                return Redirect("/DailyMission2/ParticipantTeams");
            }
            catch (Exception ex)
            {
                TempData["TeamGiftMsg"] = $"❌ 취소 오류: {ex.Message}";
                return Redirect("/DailyMission2/ParticipantTeams");
            }
        }

        /// <summary>선물 선택 페이지 (선물 지급 버튼 클릭 시 이동)</summary>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> TeamGiftSelect(string studentUserName, string teamName, int milestone = 1000)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var items = await _awardItemService.GetAllAsync(BulletinBoard.Common.ProductDisplayOpton.none, includeHidden: false);

            ViewBag.StudentUserName = studentUserName ?? "";
            ViewBag.TeamName        = teamName ?? "";
            ViewBag.Milestone       = milestone;
            return View("~/Views/StudentPointDashboard2/TeamGiftSelect.cshtml", items);
        }

        /// <summary>가족팀 선물 즉시 지급 (Status=HIDDEN)</summary>
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeamGiftGrant(string studentUserName, int productId, string teamName, int milestone = 1000)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            if (string.IsNullOrWhiteSpace(studentUserName) || productId <= 0)
                return Json(new { ok = false, message = "잘못된 요청입니다." });

            try
            {
                // includeHidden:true — 숨김 상품도 팀 선물로 지급 가능
                var item = await _awardItemService.GetByIdAsync(productId, includeHidden: true);
                if (item == null)
                    return Json(new { ok = false, message = $"상품 ID {productId}를 찾을 수 없습니다." });

                await _gift.InsertTeamGiftAsync(
                    studentUserName: studentUserName,
                    productId:       productId,
                    unitPrice:       item.UnitPrice,
                    teamName:        teamName ?? "",
                    adminId:         this.UserId ?? "",
                    milestone:       milestone
                );
                // fetch에서는 redirect response를 자동 추적하므로 redirect 유지
                return Redirect("/DailyMission2/ParticipantTeams");
            }
            catch (Exception ex)
            {
                // fetch의 .catch()에서 잡힘
                Response.StatusCode = 500;
                return Json(new { ok = false, message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // ✅ 가족 합산 선물 선택/취소 (로그인 가족이면 누구나)
        // ─────────────────────────────────────────────────────────────

        /// <summary>가족 합산 선물 선택 페이지 (GET) — threshold 포함</summary>
        [HttpGet]
        public async Task<IActionResult> FamilyGiftSelect(string teamName, string topScorer, int teamTotal, int threshold = 1000)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserName)) return Unauthorized();

            // 마일스톤 점수별 선물 옵션 (1000→2K, 3000→3K, 4000→4K, 5000→5K). 선물 없는 점수(2000 등)는 홈으로.
            var familyOption = BulletinBoard.Common.ProductDisplayOptonHelper.FamilyTeamOption(threshold);
            if (familyOption == null)
            {
                TempData["FamilyGiftMsg"] = "이 단계는 선물이 없는 축하 단계예요.";
                return RedirectToAction("Index", "Home");
            }
            var items = await _giftCatalog.GetAsync(new BulletinBoard.Services.GiftCatalog.GiftCatalogQuery
            {
                Option = familyOption.Value
            });
            ViewBag.FamilyOption     = familyOption.Value.ToString();
            ViewBag.FamilyPriceLimit = BulletinBoard.Common.ProductDisplayOptonHelper.FamilyTeamPriceLimit(familyOption.Value) ?? 0;
            ViewBag.TeamName   = teamName ?? "";
            ViewBag.TopScorer  = topScorer ?? "";
            ViewBag.TeamTotal  = teamTotal;
            ViewBag.Threshold  = threshold;

            var user    = await _authService.GetSignedUser(User);
            var isAdmin = user != null && (await _authService.IsInRole(user, nameof(Roles.Admin)) || await _authService.IsInRole(user, nameof(Roles.SuperAdmin)));
            ViewBag.IsAdmin = isAdmin;

            return View("~/Views/StudentPointDashboard2/FamilyGiftSelect.cshtml", items);
        }

        /// <summary>가족 합산 선물 선택 처리 (POST) — threshold 포함</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FamilyGiftSelectPost(int productId, string teamName, string topScorerUserName, int teamTotal, int threshold = 1000)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserName)) return Unauthorized();

            // 서버에서도 한 번 더 확인 — 이 단계의 가격 상한을 넘는 선물은 막는다
            var postOption = BulletinBoard.Common.ProductDisplayOptonHelper.FamilyTeamOption(threshold);
            if (postOption == null)
            {
                TempData["FamilyGiftMsg"] = "❌ 이 단계는 선물을 고를 수 없어요.";
                return RedirectToAction("Index", "Home");
            }
            var allowed = await _giftCatalog.CanPickAsync(productId,
                new BulletinBoard.Services.GiftCatalog.GiftCatalogQuery { Option = postOption.Value });
            if (!allowed)
            {
                TempData["FamilyGiftMsg"] = "❌ 이 단계에서 고를 수 없는 선물이에요. 목록에서 다시 골라 주세요.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                await _gift.SelectFamilyGiftForMilestoneAsync(topScorerUserName, productId, teamTotal, teamName, threshold);
                BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext); // 홈 가족 마일스톤 캐시 즉시 비움
                TempData["FamilyGiftMsg"] = "✅ 가족 합산 선물이 신청되었어요! 관리자가 확인 후 처리됩니다.";
            }
            catch (Exception ex)
            {
                TempData["FamilyGiftMsg"] = $"❌ 오류: {ex.Message}";
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>가족 합산 선물 취소 (POST) — threshold 포함</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FamilyGiftCancel(string teamName, string topScorerUserName, int threshold = 1000)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserName)) return Unauthorized();

            try
            {
                await _gift.CancelFamilyGiftForMilestoneAsync(teamName, threshold);
                BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext); // 홈 가족 마일스톤 캐시 즉시 비움
                TempData["FamilyGiftMsg"] = "🗑️ 가족 합산 선물 신청이 취소되었어요.";
            }
            catch (Exception ex)
            {
                TempData["FamilyGiftMsg"] = $"❌ 오류: {ex.Message}";
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>APPLAUSE 마일스톤 숨기기 (POST) — 팀 단위</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FamilyMilestoneHide(int milestoneConfigId, int familyTeamId)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserName)) return Unauthorized();

            // SuperAdmin이 아니면 해당 가족팀 소속인지 확인
            var currentUser = await _authService.GetSignedUser(User);
            bool isSuperAdmin = await _authService.IsInRole(currentUser, nameof(BulletinBoard.Enums.Roles.SuperAdmin));
            if (!isSuperAdmin)
            {
                bool isMember = await _gift.IsUserInFamilyTeamAsync(this.UserName, familyTeamId);
                if (!isMember) return Forbid();
            }

            try
            {
                await _gift.HideApplauseMilestoneAsync(milestoneConfigId, familyTeamId);
                BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext); // 홈 가족 마일스톤 캐시 즉시 비움
            }
            catch { /* 중복 insert 무시 */ }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> TotalRank(int? year = null)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            int cYear = year ?? DateHelper.GetKoreaNow().Year;

            var vm = await _totalRankService.GetTotalRankAsync(cYear);
            return View("~/Views/StudentPointDashboard2/TotalRank.cshtml", vm);
        }

        private static int ClampInt(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        private static int ClampInt(double v, int min, int max)
        {
            var iv = (int)Math.Round(v, 0);
            if (iv < min) return min;
            if (iv > max) return max;
            return iv;
        }
    }
}

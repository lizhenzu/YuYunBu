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
    public class StudentPointDashboard3Controller : BaseController
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

        // 통합 선물함에서 도전! 성경통독 완독 선물도 같이 보여 주기 위해 필요하다
        private readonly BibleReadingThrough.Services.BibleGift.IBibleGiftService _bibleGift;

        // 생일 선물 — 내 생일이 가까울 때만 선물함에 칸을 띄운다
        private readonly IPresentPickService _presentPick;

        // 선물 관리 통합 창구 (도토리 · 가족팀 · 가족합산 · 성경통독을 한 화면에서)
        private readonly BulletinBoard.Services.GiftAdmin.IGiftAdminFacade _giftAdmin;

        public StudentPointDashboard3Controller(
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
            BulletinBoard.Services.GiftCatalog.IGiftCatalogService giftCatalog,
            BibleReadingThrough.Services.BibleGift.IBibleGiftService bibleGift,
            IPresentPickService presentPick,
            BulletinBoard.Services.GiftAdmin.IGiftAdminFacade giftAdmin
        ) : base(authService, userManager, hostingEnvironment)
        {
            _giftCatalog = giftCatalog;
            _bibleGift = bibleGift;
            _presentPick = presentPick;
            _giftAdmin = giftAdmin;
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
                ViewBag.GiftHasAffordable = eval.HasAffordableGift;
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

            return View("Dashboard", vm); // Views/StudentPointDashboard3/Dashboard.cshtml
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
            ViewBag.IsStudent = await _gift.IsStudentAsync(this.UserName); // 학생만 선물 선택 가능

            return View("~/Views/StudentPointDashboard3/MyGiftBox.cshtml", vm);
        }

        /// <summary>
        ///  통합 선물함 — 받을 수 있는 선물 네 가지를 한 칸씩 보여 준다.
        ///  (1 나의 도토리 · 2 도전! 성경통독 완독 · 3 생일 · 4 우리 가족)
        ///  선물 목록은 여기서 보여 주지 않는다. 고르기는 GiftSelect 로 넘긴다.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyGiftBox3()
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserId) || string.IsNullOrWhiteSpace(this.UserName))
                return Unauthorized();

            var now        = DateHelper.GetKoreaNow();
            var integrated = await ComputeIntegratedTotalAsync(now.Year);

            var page = new GiftBoxPageVM
            {
                FullName  = this.UserFullName,
                IsStudent = await _gift.IsStudentAsync(this.UserName)
            };

            // 네 가지를 각각 모아 온다. 한 곳이 실패해도 나머지 칸은 그대로 보여 준다.
            var box = await _gift.GetMyGiftBoxAsync(this.UserName, integrated);

            var rounds = new List<BibleReadingThrough.Models.BibleGift.BibleGiftSelectVM>();
            try { rounds = await _bibleGift.GetMyRoundsAsync(this.UserId) ?? new(); } catch { }

            UpcomingBirthdayRowVM? birth = null;
            try
            {
                var up = await _presentPick.GetUpcomingBirthdaysAsync(30, "all", _hostingEnvironment);
                birth = up?.Rows?.FirstOrDefault(
                    r => string.Equals(r.User?.UserId, this.UserId, StringComparison.OrdinalIgnoreCase));
            }
            catch { }

            var fam = new List<FamilyMilestoneDisplayVM>();
            FamilyTeamGiftVM? famTeam = null;
            try { fam     = await _gift.GetMyFamilyMilestonesAsync(this.UserName, now.Year) ?? new(); } catch { }
            try { famTeam = await _gift.GetFamilyTeamGiftStatusAsync(this.UserName, now.Year, 1000); } catch { }

            // 전달 완료(받았음)면 선택 영역에서는 완전히 뺀다 — 지난 선물(History)에는 그대로 남는다
            page.Cards.Add(BuildPointCard(box, integrated, page.IsStudent));
            AddIfAny(page.Cards, BuildBibleCard(rounds));
            AddIfAny(page.Cards, BuildBirthCard(birth));
            AddIfAny(page.Cards, BuildFamilyCard(fam, famTeam));

            page.History = BuildGiftHistory(box, rounds, birth, fam);

            ViewBag.Toast = TempData["GiftToast"]?.ToString()
                         ?? TempData["GiftErr"]?.ToString()
                         ?? TempData["GiftOk"]?.ToString()
                         ?? TempData["FamilyGiftMsg"]?.ToString()
                         ?? "";

            return View("~/Views/StudentPointDashboard3/MyGiftBox3.cshtml", page);
        }

        /// <summary>
        ///  선물 고르기 — 네 가지가 화면 하나를 같이 쓴다. type 값에 따라 머리말과 목록만 달라진다.
        ///  저장은 기존 액션(SelectGift · BibleGift/Pick · Present/PickBirth · FamilyGiftSelectPost)을 그대로 쓴다.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GiftSelect(string type, int round = 0, int threshold = 0, string? returnTo = null)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserId) || string.IsNullOrWhiteSpace(this.UserName))
                return Unauthorized();

            var now = DateHelper.GetKoreaNow();
            var vm  = new GiftPickPageVM { Type = (type ?? "").ToLowerInvariant() };

            switch (vm.Type)
            {
                case "point":
                {
                    int integrated = await ComputeIntegratedTotalAsync(now.Year);
                    var box = await _gift.GetMyGiftBoxAsync(this.UserName, integrated);

                    vm.Kicker = $"{ConstatsCommon.myDoToRi} 선물";
                    vm.Title  = $"{Won(box.Milestone)}점을 모았어요";
                    vm.Why    = $"{ConstatsCommon.MainKeyTitle} · {ConstatsCommon.MissionBibleWithparent} · 감사 · 출석을 합해 100점마다 받아요.";
                    vm.Items  = await _giftCatalog.GetAsync(
                        BulletinBoard.Services.GiftCatalog.GiftCatalogQuery.ByLimit(box.AvailableCredit));
                    vm.PickedProductId = box.CurrentDraft?.ProductID;
                    vm.PostAction = "/StudentPointDashboard3/SelectGift";
                    // 홈 카드에서 바로 들어왔으면(returnTo=home) 저장 뒤 홈으로 돌아간다
                    vm.PostFields = new() { ["returnTo"] = string.Equals(returnTo, "home", StringComparison.OrdinalIgnoreCase) ? "home" : "3" };
                    // 크레딧이 없어 고를 게 없을 때 화면이 비지 않도록 안내 문구를 넘긴다
                    if (vm.Items.Count == 0)
                        vm.EmptyText = box.AvailableCredit > 0
                                       ? "지금 고를 수 있는 선물이 없어요. 선생님께 말씀해 주세요."
                                       : "아직 고를 수 있는 선물이 없어요. 100점을 모으면 선물을 고를 수 있어요.";
                    break;
                }

                case "bible":
                {
                    var page = await _bibleGift.GetSelectPageAsync(this.UserId, round);
                    if (page == null) return RedirectToAction(nameof(MyGiftBox3));

                    vm.Kicker = $"{ConstatsCommon.ReadBibleAllTitleButton} 선물";
                    vm.Title  = $"성경 {page.RoundNo}독을 마쳤어요";
                    vm.Why    = $"성경을 한 번 다 읽으면 받는 선물이에요. {page.CompletedAt:yyyy-MM-dd}에 마쳤어요.";
                    vm.Items  = (await _giftCatalog.GetAsync(BulletinBoard.Services.GiftCatalog.GiftCatalogQuery.All()))
                                .Where(a => page.MaxUnitPrice <= 0 || a.UnitPrice <= page.MaxUnitPrice)
                                .ToList();
                    vm.PickedProductId = page.PickedProductId;
                    vm.PostAction = "/BibleGift/Pick";
                    vm.PostFields = new() { ["round"] = page.RoundNo.ToString(), ["returnTo"] = "hub" };
                    break;
                }

                case "birth":
                {
                    var up  = await _presentPick.GetUpcomingBirthdaysAsync(30, "all", _hostingEnvironment);
                    var row = up?.Rows?.FirstOrDefault(
                        r => string.Equals(r.User?.UserId, this.UserId, StringComparison.OrdinalIgnoreCase));
                    if (row == null || row.DDay < 0 || row.DDay > 30) return RedirectToAction(nameof(MyGiftBox3));

                    vm.Kicker = "생일 선물";
                    vm.Title  = "생일 축하해요";
                    vm.Why    = $"{row.NextBirthdayDate:M월 d일}이 생일이에요. 한 해에 한 번 받는 선물이에요.";
                    vm.Items  = await _giftCatalog.GetAsync(
                        new BulletinBoard.Services.GiftCatalog.GiftCatalogQuery { Option = ProductDisplayOpton.birth });
                    vm.PickedProductId = row.ProductId;
                    vm.PostAction = "/Present/PickBirth";
                    vm.PostFields = new() { ["targetUserId"] = this.UserId, ["returnTo"] = "hub" };
                    break;
                }

                case "family":
                {
                    var fam = await _gift.GetMyFamilyMilestonesAsync(this.UserName, now.Year);
                    var m   = fam.FirstOrDefault(x => x.PointThreshold == threshold) ?? fam.FirstOrDefault();
                    if (m == null) return RedirectToAction(nameof(MyGiftBox3));

                    // 선물 대상자 본인만 고를 수 있다
                    if (!string.Equals(m.RecipientUserName, this.UserName, StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["FamilyGiftMsg"] = $"가족 합산 선물은 {m.RecipientFullName}님이 골라요.";
                        return RedirectToAction(nameof(MyGiftBox3));
                    }

                    var opt = BulletinBoard.Common.ProductDisplayOptonHelper.FamilyTeamOption(m.PointThreshold);
                    if (opt == null) return RedirectToAction(nameof(MyGiftBox3));

                    vm.Kicker = "우리 가족 선물";
                    vm.Title  = $"가족이 {Won(m.PointThreshold)}점을 모았어요";
                    vm.Why    = $"{m.TeamName} 합산 {Won(m.TeamTotalDm2)}점이에요. 선물은 {m.RecipientFullName}님께 드려요.";
                    vm.Items  = await _giftCatalog.GetAsync(
                        new BulletinBoard.Services.GiftCatalog.GiftCatalogQuery { Option = opt.Value });
                    vm.PickedProductId = m.ProductId;
                    vm.PostAction = "/StudentPointDashboard3/FamilyGiftSelectPost";
                    vm.PostFields = new()
                    {
                        ["teamName"]          = m.TeamName,
                        ["topScorerUserName"] = m.RecipientUserName,
                        ["teamTotal"]         = m.TeamTotalDm2.ToString(),
                        ["threshold"]         = m.PointThreshold.ToString(),
                        ["returnTo"]          = "hub"
                    };
                    break;
                }

                default:
                    return RedirectToAction(nameof(MyGiftBox3));
            }

            return View("~/Views/StudentPointDashboard3/GiftSelect.cshtml", vm);
        }

        // ══════════════════════════════════════════════════════════
        //  선물함 칸 만들기 — 종류가 달라도 화면에서는 같은 모양으로 그린다
        // ══════════════════════════════════════════════════════════

        private static string Won(int v) => v.ToString("N0");
        private static void AddIfAny(List<GiftBoxCardVM> list, GiftBoxCardVM? c) { if (c != null) list.Add(c); }

        /// <summary>01 나의 도토리 선물</summary>
        private GiftBoxCardVM BuildPointCard(GiftBoxVM box, int integrated, bool isStudent)
        {
            int credit = box?.AvailableCredit ?? 0;
            var draft  = box?.CurrentDraft;

            var c = new GiftBoxCardVM
            {
                Type       = "point",
                Title      = $"{ConstatsCommon.myDoToRi} 선물",
                Reason     = $"{ConstatsCommon.MainKeyTitle} · {ConstatsCommon.MissionBibleWithparent} · 감사 · 출석을 합해 100점마다",
                RightLabel = "쓸 수 있는",
                RightValue = Won(credit),
                RightUnit  = "credit",
                SelectUrl  = Url.Action(nameof(GiftSelect), new { type = "point" }) ?? "",
                IsOpen     = isStudent && credit > 0
            };

            if (!isStudent)
            {
                c.IsOpen   = false;
                c.LockHint = "이 선물은 학생만 받을 수 있어요.";
            }
            else if (credit <= 0)
            {
                int next = (integrated / 100 + 1) * 100;
                c.LockHint = $"{ConstatsCommon.MainKeyTitle} · {ConstatsCommon.MissionBibleWithparent} · 감사 · 출석 점수를 모아 100점이 될 때마다 선물을 고를 수 있어요. "
                           + $"지금 {Won(integrated)}점이고, 다음 {Won(next)}점까지 {Won(Math.Max(0, next - integrated))}점 남았어요.";
                c.RightLabel = "지금 점수";
                c.RightValue = Won(integrated);
                c.RightUnit  = "점";
            }

            if (draft != null)
            {
                c.PickedProductId = draft.ProductID;
                c.PickedName      = draft.ProductName;
                c.PickedMeta      = $"{Won(draft.CreditCostSnapshot)} credit · 선택 중";
                c.CanChange       = true;
                c.CancelAction    = "/StudentPointDashboard3/CancelGift";
                c.CancelFields    = new() { ["orderId"] = draft.OrderId.ToString(), ["returnTo"] = "3" };
            }
            else
            {
                // 관리자가 구매 완료로 바꾸면 더 이상 DRAFT 가 아니라 '고른 선물'에서 빠진다.
                // 학생 화면에서 선물이 사라져 보이므로, 준비 중인 것으로 그대로 보여 준다 (바꾸지는 못한다).
                var buying = box?.History?.FirstOrDefault(
                    h => string.Equals(h.Status, "PURCHASED", StringComparison.OrdinalIgnoreCase));

                if (buying != null)
                {
                    c.PickedProductId = buying.ProductID;
                    c.PickedName      = buying.ProductName;
                    c.PickedMeta      = "선생님이 선물을 준비하고 있어요";
                    c.CanChange       = false;
                }
            }
            return c;
        }

        /// <summary>
        ///  02 도전! 성경통독 완독 선물 — 아직 안 고른 회차를 먼저 보여 준다.
        ///  받을 회차가 아예 없으면(모두 완독도 안 했으면) 잠긴 칸으로, 모든 회차가
        ///  이미 전달 완료면 null — 선택 영역에서 완전히 빠지고 지난 선물에만 남는다.
        /// </summary>
        private GiftBoxCardVM? BuildBibleCard(List<BibleReadingThrough.Models.BibleGift.BibleGiftSelectVM> rounds)
        {
            var c = new GiftBoxCardVM
            {
                Type   = "bible",
                Title  = $"{ConstatsCommon.ReadBibleAllTitleButton} 선물",
                Reason = "성경을 한 번 다 읽으면 받아요",
            };

            if (rounds.Count > 0 && rounds.All(r => r.IsDelivered))
                return null;   // 완독한 회차를 전부 받았다 — 더 보여 줄 게 없다

            // 안 고른 회차 우선, 없으면 아직 전달 안 된(구매 대기·선택 중) 가장 최근 회차
            var target = rounds.FirstOrDefault(r => r.PickedProductId == null)
                      ?? rounds.LastOrDefault(r => !r.IsDelivered);
            if (target == null)
            {
                c.IsOpen     = false;
                c.LockHint   = "성경을 처음부터 끝까지 한 번 다 읽으면 선물을 고를 수 있어요.";
                c.RightLabel = "완독";
                c.RightValue = "0";
                c.RightUnit  = "독";
                return c;
            }

            int waiting = rounds.Count(r => r.PickedProductId == null);
            c.IsOpen     = !target.Locked;
            c.Reason     = $"{target.RoundNo}독 완독 · {target.CompletedAt:yyyy-MM-dd} 마침"
                         + (waiting > 1 ? $" · 고를 회차 {waiting}개" : "");
            c.RightLabel = "선물 한도";
            c.RightValue = Won(target.MaxUnitPrice);
            c.RightUnit  = "credit";
            c.SelectUrl  = Url.Action(nameof(GiftSelect), new { type = "bible", round = target.RoundNo }) ?? "";

            var picked = target.Items.FirstOrDefault(i => i.IsPicked);
            if (picked != null)
            {
                c.PickedProductId = picked.ProductID;
                c.PickedName      = picked.ProductName;
                c.PickedMeta      = target.IsDelivered ? "선물을 전해 드렸어요"
                                  : target.Locked      ? "선물을 준비하고 있어요"
                                                       : $"{target.RoundNo}독 · 선택 중";
                c.CanChange       = !target.Locked;
                c.CancelAction    = "/BibleGift/Cancel";
                c.CancelFields    = new() { ["round"] = target.RoundNo.ToString(), ["returnTo"] = "hub" };
            }
            return c;
        }

        /// <summary>
        ///  03 생일 선물 — 생일 30일 전부터.
        ///  이미 전달 완료(발송)면 null — 선택 영역에서 완전히 빠지고 지난 선물에만 남는다.
        /// </summary>
        private GiftBoxCardVM? BuildBirthCard(UpcomingBirthdayRowVM? row)
        {
            if (row != null && row.HasPickedGift && row.IsSent)
                return null;   // 이미 전해 드렸다 — 더 보여 줄 게 없다

            var c = new GiftBoxCardVM
            {
                Type   = "birth",
                Title  = "생일 선물",
                Reason = "한 해에 한 번, 생일에 받아요",
            };

            if (row == null)
            {
                c.IsOpen     = false;
                c.LockHint   = "생일이 등록되어 있지 않아요. 선생님께 생일을 알려 주세요.";
                c.RightLabel = "생일";
                c.RightValue = "-";
                return c;
            }

            // 올해 생일이 지났으면 다음 생일까지 남은 날로 바꿔 보여 준다
            int dday = row.DDay >= 0
                ? row.DDay
                : (int)(row.NextBirthdayDate.AddYears(1) - DateHelper.GetKoreaToday()).TotalDays;

            c.Reason     = $"{row.NextBirthdayDate:M월 d일} 생일";
            c.RightLabel = "생일까지";
            c.RightValue = dday == 0 ? "오늘" : $"D-{dday}";
            c.IsOpen     = row.DDay >= 0 && row.DDay <= 30 && !row.IsPurchased;
            c.SelectUrl  = Url.Action(nameof(GiftSelect), new { type = "birth" }) ?? "";

            if (!c.IsOpen && !row.IsPurchased)
                c.LockHint = $"{row.NextBirthdayDate:M월 d일}이 생일이에요. 생일 30일 전부터 선물을 고를 수 있어요.";

            if (row.HasPickedGift)
            {
                c.PickedProductId = row.ProductId;
                c.PickedName      = row.ProductName ?? "";
                c.PickedMeta      = row.IsSent      ? "선물을 전해 드렸어요"
                                  : row.IsPurchased ? "선물을 준비하고 있어요"
                                                    : "선택 중";
                c.CanChange       = !row.IsPurchased;
                c.CancelAction    = "/Present/CancelBirth";
                c.CancelFields    = new() { ["targetUserId"] = this.UserId, ["returnTo"] = "hub" };
            }
            return c;
        }

        /// <summary>
        ///  04 우리 가족 선물 — 선물 대상자 본인만 고를 수 있다.
        ///  달성한 단계를 전부 이미 전달 완료했으면 null — 선택 영역에서 완전히 빠지고 지난 선물에만 남는다.
        /// </summary>
        private GiftBoxCardVM? BuildFamilyCard(List<FamilyMilestoneDisplayVM> fam, FamilyTeamGiftVM? team)
        {
            bool IsDelivered(FamilyMilestoneDisplayVM m)
                => string.Equals(m.GiftStatus, "HIDDEN", StringComparison.OrdinalIgnoreCase);

            if (fam.Count > 0 && fam.All(IsDelivered))
                return null;   // 달성한 단계를 전부 이미 전달 받았다 — 더 보여 줄 게 없다

            var c = new GiftBoxCardVM
            {
                Type   = "family",
                Title  = "우리 가족 선물",
                Reason = "가족 점수를 모두 더해 단계마다 받아요",
            };

            // 안 고른 단계 우선, 없으면 아직 전달 안 된(구매 대기·선택 중) 가장 최근 단계
            var target = fam.FirstOrDefault(m => string.IsNullOrWhiteSpace(m.GiftStatus))
                      ?? fam.LastOrDefault(m => !IsDelivered(m));
            if (target == null)
            {
                c.IsOpen     = false;
                c.RightLabel = "가족 합산";
                c.RightValue = team != null ? Won(team.TeamTotalDm2) : "0";
                c.RightUnit  = "점";
                c.LockHint   = team != null
                    ? $"{team.TeamName}의 지금 합산은 {Won(team.TeamTotalDm2)}점이에요. 1,000점이 되면 선물을 고를 수 있어요."
                    : "가족 점수를 모두 더해 1,000점이 되면 선물을 고를 수 있어요.";
                return c;
            }

            bool isOwner = string.Equals(target.RecipientUserName, this.UserName, StringComparison.OrdinalIgnoreCase);
            bool locked  = string.Equals(target.GiftStatus, "PURCHASED", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(target.GiftStatus, "HIDDEN",    StringComparison.OrdinalIgnoreCase);

            c.Reason     = $"{target.TeamName} 합산 {Won(target.TeamTotalDm2)}점 · 받는 사람 {target.RecipientFullName}";
            c.RightLabel = "달성 단계";
            c.RightValue = Won(target.PointThreshold);
            c.RightUnit  = "점";
            c.IsOpen     = isOwner && !locked;
            c.SelectUrl  = Url.Action(nameof(GiftSelect), new { type = "family", threshold = target.PointThreshold }) ?? "";

            if (!isOwner)
            {
                c.NotOwnerText = $"{target.RecipientFullName}님이 골라요";
                c.LockHint     = $"가족 합산 선물은 가장 많이 모은 {target.RecipientFullName}님이 골라요.";
            }

            if (!string.IsNullOrWhiteSpace(target.GiftStatus))
            {
                c.PickedProductId = target.ProductId;
                c.PickedName      = target.ProductName ?? "";
                c.PickedMeta      = locked ? "선물을 준비하고 있어요" : "선택 중";
                c.CanChange       = isOwner && !locked;
                c.CancelAction    = "/StudentPointDashboard3/FamilyGiftCancel";
                c.CancelFields    = new()
                {
                    ["teamName"]          = target.TeamName,
                    ["topScorerUserName"] = target.RecipientUserName,
                    ["teamTotal"]         = target.TeamTotalDm2.ToString(),
                    ["threshold"]         = target.PointThreshold.ToString(),
                    ["returnTo"]          = "hub"
                };
            }
            return c;
        }

        /// <summary>지난 선물 — 네 가지 기록을 한 목록에 모아 최근 순으로</summary>
        private List<GiftHistoryRowVM> BuildGiftHistory(
            GiftBoxVM box,
            List<BibleReadingThrough.Models.BibleGift.BibleGiftSelectVM> rounds,
            UpcomingBirthdayRowVM? birth,
            List<FamilyMilestoneDisplayVM> fam)
        {
            var list = new List<GiftHistoryRowVM>();

            foreach (var h in box?.History ?? new List<GiftOrderVM>())
            {
                var up = (h.Status ?? "").ToUpperInvariant();
                if (up is "CANCELED_BY_RESELECT" or "CANCELED_BY_UPGRADE") continue;

                list.Add(new GiftHistoryRowVM
                {
                    Date        = h.PurchasedAt ?? h.CreatedAt,
                    ProductId   = h.ProductID,
                    ProductName = h.ProductName,
                    Source      = h.Milestone > 0 ? $"{ConstatsCommon.myDoToRi} {Won(h.Milestone)}점" : ConstatsCommon.myDoToRi,
                    Status      = up == "HIDDEN" ? "받음" : up == "PURCHASED" ? "준비 중" : "선택 중"
                });
            }

            foreach (var r in rounds)
            {
                var picked = r.Items.FirstOrDefault(i => i.IsPicked);
                if (picked == null) continue;

                list.Add(new GiftHistoryRowVM
                {
                    Date        = r.SelectedAt ?? r.CompletedAt,
                    ProductId   = picked.ProductID,
                    ProductName = picked.ProductName,
                    Source      = $"{ConstatsCommon.ReadBibleAllTitleButton} {r.RoundNo}독",
                    Status      = r.IsDelivered ? "받음" : r.Locked ? "준비 중" : "선택 중"
                });
            }

            if (birth?.HasPickedGift == true)
            {
                list.Add(new GiftHistoryRowVM
                {
                    Date        = birth.PurchasedAt ?? birth.NextBirthdayDate,
                    ProductId   = birth.ProductId,
                    ProductName = birth.ProductName ?? "",
                    Source      = "생일",
                    Status      = birth.IsSent ? "받음" : birth.IsPurchased ? "준비 중" : "선택 중"
                });
            }

            foreach (var m in fam.Where(x => !string.IsNullOrWhiteSpace(x.GiftStatus)))
            {
                var up = (m.GiftStatus ?? "").ToUpperInvariant();
                list.Add(new GiftHistoryRowVM
                {
                    ProductId   = m.ProductId,
                    ProductName = m.ProductName ?? "",
                    Source      = $"우리 가족 {Won(m.PointThreshold)}점",
                    Status      = up == "HIDDEN" ? "받음" : up == "PURCHASED" ? "준비 중" : "선택 중"
                });
            }

            return list.OrderByDescending(x => x.Date).ToList();
        }

        // 선택·취소 뒤 어느 화면으로 돌아갈지.
        //   home → 홈(카드에서 바로 들어온 경우) · 3 → 새 선물함 · 그 밖 → 예전 선물함
        private IActionResult BackToGiftBox(string? returnTo)
            => string.Equals(returnTo, "home", StringComparison.OrdinalIgnoreCase)
               ? RedirectToAction("Index", "Home")
               : string.Equals(returnTo, "3", StringComparison.Ordinal)
                 ? RedirectToAction(nameof(MyGiftBox3))
                 : RedirectToAction(nameof(MyGiftBox));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectGift(int productId, string? returnTo = null)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserId) || string.IsNullOrWhiteSpace(this.UserName))
                return Unauthorized();

            var now = DateHelper.GetKoreaNow();
            var integrated = await ComputeIntegratedTotalAsync(now.Year);

            if (string.Equals(returnTo, "3", StringComparison.Ordinal))
            {
                // 새 화면에서는 실패 사유를 문구로 보여 준다. (예: 크레딧 부족)
                try { await _gift.SelectGiftAsync(this.UserName, productId, integrated); }
                catch (InvalidOperationException ex) { TempData["GiftToast"] = ex.Message; }
            }
            else
            {
                await _gift.SelectGiftAsync(this.UserName, productId, integrated);
            }

            return BackToGiftBox(returnTo);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelGift(int orderId, string? returnTo = null)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserId) || string.IsNullOrWhiteSpace(this.UserName))
                return Unauthorized();

            // ✅ 취소 = 완전 삭제 (현재 선택 DRAFT만 삭제)
            await _gift.CancelSelectedGiftAsync(this.UserName, orderId);

            return BackToGiftBox(returnTo);
        }
        // ══════════════════════════════════════════════════════════
        //  선물 관리 통합 화면 — 도토리 · 가족팀 · 가족합산 · 성경통독을 한 곳에서
        // ══════════════════════════════════════════════════════════

        /// <summary>
        ///  관리 권한 — Admin · SuperAdmin 역할, 그리고 예외로 "Basic" 계정.
        ///  ("Basic" 은 성경통독 관리 화면을 쓰던 기존 계정이라 그대로 살려 둔다)
        /// </summary>
        private async Task<(bool canManage, bool isSuperAdmin)> GetGiftAdminRightsAsync()
        {
            var user         = await _authService.GetSignedUser(User);
            bool isAdmin     = await _authService.IsInRole(user, nameof(Roles.Admin));
            bool isSuperAdmin = await _authService.IsInRole(user, nameof(Roles.SuperAdmin));
            bool isBasic     = string.Equals(User.Identity?.Name, "Basic", StringComparison.OrdinalIgnoreCase);

            return (isAdmin || isSuperAdmin || isBasic, isSuperAdmin);
        }

        /// <summary>kind 는 전달 완료 묶음에만 거는 거르개다. 미처리는 언제나 전부 보여 준다.</summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AdminPurchaseList(string? kind = null)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var (canManage, isSuperAdmin) = await GetGiftAdminRightsAsync();
            if (!canManage) return RedirectToAction("Index", "Home");

            var vm = await _giftAdmin.GetPageAsync(ParseKind(kind), isSuperAdmin);
            return View("~/Views/StudentPointDashboard3/AdminPurchaseList.cshtml", vm);
        }

        private static BulletinBoard.Services.GiftAdmin.GiftKind? ParseKind(string? v)
            => Enum.TryParse<BulletinBoard.Services.GiftAdmin.GiftKind>(v, true, out var k) ? k : null;

        /// <summary>통합 화면의 상태 바꾸기 — 종류(kind)를 보고 원래 시스템으로 넘긴다</summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminGiftAction(
            string op, BulletinBoard.Services.GiftAdmin.GiftKind kind, int orderId,
            string? kindFilter = null)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var (canManage, _) = await GetGiftAdminRightsAsync();
            if (!canManage) return Forbid();

            var adminId = kind == BulletinBoard.Services.GiftAdmin.GiftKind.Bible
                          ? (User.Identity?.Name ?? this.UserId)   // 성경통독은 이름으로 기록해 왔다
                          : this.UserId;

            var (_, msg) = op switch
            {
                "purchase"     => await _giftAdmin.PurchaseAsync(kind, orderId, adminId),
                "undoPurchase" => await _giftAdmin.UndoPurchaseAsync(kind, orderId, adminId),
                "deliver"      => await _giftAdmin.DeliverAsync(kind, orderId, adminId),
                "undoDeliver"  => await _giftAdmin.UndoDeliverAsync(kind, orderId, adminId),
                _              => (false, "알 수 없는 동작이에요.")
            };

            TempData["GiftToast"] = msg;

            // 선물 상태가 홈 가족 마일스톤 카드에 보이므로 캐시를 비운다
            BulletinBoard.Helpers.HomeCacheHelper.InvalidateFamilyMilestones(HttpContext);

            return RedirectToAction(nameof(AdminPurchaseList), new { kind = kindFilter });
        }

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
                return Redirect("/DailyMission3/ParticipantTeams");
            }
            catch (Exception ex)
            {
                TempData["TeamGiftMsg"] = $"❌ 취소 오류: {ex.Message}";
                return Redirect("/DailyMission3/ParticipantTeams");
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
            return View("~/Views/StudentPointDashboard3/TeamGiftSelect.cshtml", items);
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
                return Redirect("/DailyMission3/ParticipantTeams");
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

        /// <summary>
        ///  가족 합산 선물을 다룰 수 있는 사람인지 — 그 가족 구성원과 관리자만.
        ///  예전에는 로그인만 하면 남의 가족 선물도 고르고 취소할 수 있었다.
        /// </summary>
        private async Task<bool> CanHandleFamilyGiftAsync(string teamName)
        {
            var user = await _authService.GetSignedUser(User);
            if (user != null && (await _authService.IsInRole(user, nameof(Roles.Admin))
                              || await _authService.IsInRole(user, nameof(Roles.SuperAdmin))))
                return true;

            return await _gift.IsUserInFamilyTeamByNameAsync(this.UserName, teamName ?? "");
        }

        /// <summary>가족 합산 선물 선택 페이지 (GET) — threshold 포함</summary>
        [HttpGet]
        public async Task<IActionResult> FamilyGiftSelect(string teamName, string topScorer, int teamTotal, int threshold = 1000)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserName)) return Unauthorized();

            if (!await CanHandleFamilyGiftAsync(teamName))
            {
                TempData["FamilyGiftMsg"] = "우리 가족의 선물만 고를 수 있어요.";
                return RedirectToAction("Index", "Home");
            }

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

            // 이미 고른 선물 — 무엇을 골랐는지 보여 주고 취소할 수 있게 한다
            ViewBag.FamilyPick = await _gift.GetFamilyGiftPickAsync(teamName ?? "", threshold);
            ViewBag.FamilyMsg  = TempData["FamilyGiftMsg"]?.ToString() ?? "";

            return View("~/Views/StudentPointDashboard3/FamilyGiftSelect.cshtml", items);
        }

        /// <summary>가족 합산 선물 선택 처리 (POST) — threshold 포함</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FamilyGiftSelectPost(int productId, string teamName, string topScorerUserName, int teamTotal, int threshold = 1000,
                                                              string? returnTo = null)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserName)) return Unauthorized();

            if (!await CanHandleFamilyGiftAsync(teamName))
            {
                TempData["FamilyGiftMsg"] = "❌ 우리 가족의 선물만 고를 수 있어요.";
                return RedirectToAction("Index", "Home");
            }

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

            if (string.Equals(returnTo, "hub", StringComparison.Ordinal))
                return RedirectToAction(nameof(MyGiftBox3));

            return RedirectToAction("Index", "Home");
        }

        /// <summary>가족 합산 선물 취소 (POST) — threshold 포함</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FamilyGiftCancel(string teamName, string topScorerUserName, int threshold = 1000,
                                                          int teamTotal = 0, string? returnTo = null)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();
            if (string.IsNullOrWhiteSpace(this.UserName)) return Unauthorized();

            if (!await CanHandleFamilyGiftAsync(teamName))
            {
                TempData["FamilyGiftMsg"] = "❌ 우리 가족의 선물만 취소할 수 있어요.";
                return RedirectToAction("Index", "Home");
            }

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

            if (string.Equals(returnTo, "hub", StringComparison.Ordinal))
                return RedirectToAction(nameof(MyGiftBox3));

            // 고르는 화면에서 취소했으면 그 화면으로 돌아가 바로 다시 고를 수 있게 한다
            if (string.Equals(returnTo, "select", StringComparison.Ordinal))
                return RedirectToAction(nameof(FamilyGiftSelect),
                    new { teamName, topScorer = topScorerUserName, teamTotal, threshold });

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
            return View("~/Views/StudentPointDashboard3/TotalRank.cshtml", vm);
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

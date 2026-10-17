using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BibleReadingThrough.Data;
using BulletinBoard.DataAccessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services.DailyMission2;
using BulletinBoard.Services.ReplayBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class HomeController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly INoticeService _noticeService;
        private readonly IBandBoardService _bandBoardService;
        private readonly IPresentPickService _presentPickService;
        private readonly IVBSParticipationSurveyService _VBSParticipationSurveyService;
        private readonly IAttendanceService _attendanceService;
        private readonly IUserUiPreferenceService _prefSvc;
        private readonly ReplayBoardRepository _replayRepo;

        // ✅ StudentPointDashboard summary for Home/Index
        private readonly IDailyMission2Service _dailyMission2Service;
        private readonly IWorldviewLessonService _worldviewLessonService;
        private readonly IDevotionPointsService _devotionPointsService;
        private readonly IStudentPointGiftService _studentPointGiftService;
        private readonly AppDbContext _db;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

        // ── 홈 화면 공용 캐시 ─────────────────────────────
        // 아래 항목들은 "로그인한 사람이 누구든 같은 값"이라 사용자별로 다시 조회할 이유가 없다.
        // 홈은 접속이 가장 잦은 화면이므로 짧게 캐시해 DB 왕복을 줄인다.
        // ⚠️ 날짜가 바뀌면 내용도 바뀌는 항목은 키에 "그날 날짜"를 붙인다.
        //    그래야 자정이 지나는 순간 키가 달라져 새 내용이 바로 나온다.
        //    (예전에는 고정 키 + 15분이라, 새 글을 넣거나 날이 바뀌어도 한동안 옛 것이 보였다)
        // 생일 수정 화면에서 바로 비울 수 있도록 키를 HomeCacheHelper 와 공유한다
        private const string CK_Birthdays  = HomeCacheHelper.BirthdaysKey;    // + 날짜
        private const string CK_BandTop    = "home:bandtop:v1";
        // 가족 마일스톤은 조건(구성원·점수·설정)이 바뀌면 바로 보여야 해서 캐시하지 않는다. (키는 남겨 둠)
        private const string CK_Milestones = HomeCacheHelper.FamilyMilestonesKey;
        private const string CK_MsConfigs  = HomeCacheHelper.FamilyMsConfigsKey;
        private const string CK_Jubo       = "home:jubo:v1";
        // ⚠ 완독자 목록(TotalProgressRate/readBibleAllCount)은 캐시하지 않는다 — 아래 참조.
        private const string CK_Menus      = "home:menus:v1";
        private const string CK_Award      = "home:award:v1";
        private const string CK_OneLine    = "home:oneline:v1";
        private const string CK_Rhapsody   = "home:rhapsody:v2:";     // + 날짜
        private const string CK_Pinned     = "home:pinned:v1";
        private const string CK_Attendance = "home:attendance:v1:";   // + 기준일
        private const string CK_BoardTops  = "home:boardtops:v1";
        private const string CK_GuestWord  = "home:guestword:v1:";    // + 날짜
        private const string CK_TodayJoin  = "home:todayjoin:v1:";    // + 날짜
        private const string CK_MenuPolicy = "home:menupolicy:v1";

        private static readonly TimeSpan TTL_Birthdays  = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan TTL_BandTop    = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan TTL_Milestones = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan TTL_Short      = TimeSpan.FromMinutes(2);    // 자주 바뀌는 것
        private static readonly TimeSpan TTL_Medium     = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan TTL_Long       = TimeSpan.FromMinutes(15);   // 하루 단위로 바뀌는 것

        // Index() 에서 미리 시작해 두고 IndexCore() 에서 회수하는 조회들.
        // (예전에는 IndexCore 에 들어가서야 시작해, 앞 구간이 끝날 때까지 놀고 있었다)
        private Task<UpcomingBirthdayListVM> _tBirthdaysPre;
        private Task<string> _tBandTopPre;

        /// <summary>
        /// 캐시에 있으면 그대로, 없으면 factory 로 채운다.
        /// 조회가 실패하면 캐시에 남기지 않아 다음 요청에서 다시 시도한다.
        /// </summary>
        private async Task<T> CachedAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory)
        {
            if (_cache != null && _cache.TryGetValue(key, out T hit))
                return hit;

            T value;
            try { value = await factory(); }
            catch { return default; }

            if (_cache != null && value != null)
                _cache.Set(key, value, ttl);

            return value;
        }

        public HomeController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            INoticeService noticeService,
            IAttendanceService attendanceService,
            IVBSParticipationSurveyService VBSParticipationSurveyService,
            IPresentPickService presentPickService,
            IUserUiPreferenceService prefSvc,
            IBandBoardService bandBoardService,
            IWebHostEnvironment hostingEnvironment,

            // ✅ NoticeBoard(ReplayBoard) pinned banner
            ReplayBoardRepository replayRepo,

            // ✅ StudentPointDashboard deps
            IDailyMission2Service dailyMission2Service,
            IWorldviewLessonService worldviewLessonService,
            IDevotionPointsService devotionPointsService,
            IStudentPointGiftService studentPointGiftService,
            AppDbContext db,
            Microsoft.Extensions.Caching.Memory.IMemoryCache cache
        ) : base(authService, userManager, hostingEnvironment)
        {
            _authService = authService;
            _userManager = userManager;
            _prefSvc = prefSvc;
            _presentPickService = presentPickService;
            _noticeService = noticeService;
            _bandBoardService = bandBoardService;
            _VBSParticipationSurveyService = VBSParticipationSurveyService;
            _attendanceService = attendanceService;
            _hostingEnvironment = hostingEnvironment;
            _replayRepo = replayRepo;

            _dailyMission2Service = dailyMission2Service;
            _worldviewLessonService = worldviewLessonService;
            _devotionPointsService = devotionPointsService;
            _studentPointGiftService = studentPointGiftService;
            _db = db;
            _cache = cache;
        }

        // GET: JobCategory
        public async Task<IActionResult> IndexBranch()
        {
            ManageOneLineFamousWord manageOneLineFamousWord = new ManageOneLineFamousWord();
            List<OneLineFamousWords> values = manageOneLineFamousWord.getOneLineFamousWord(0);

            DateTime currentDate = SharedComm.GetToday();
            int index = SharedComm.CalculateIndex(values.Count, currentDate);

            ViewData["OneLineText"] = values[index].OneLineText;
            ViewData["OneLineTextID"] = values[index].Id;

            if (await _authService.IsSignedIn(User))
            {
                ManageeCommon manageeCommon = new ManageeCommon();
                manageeCommon.DeleteClickHistory("guest");
                return RedirectToLocal("~/Home/MissionMainBoard");
            }

            return View();
        }

        public async Task<IActionResult> HowToInstall() => View();
        public async Task<IActionResult> HowToInstall_Android() => View();
        public async Task<IActionResult> HowToInstall_iPhone() => View();

        // ✅ 메인(간단 화면)
        public async Task<IActionResult> Index()
        {
            // 📊 방문자 집계는 Startup.cs 의 미들웨어에서 모든 화면 진입을 대상으로 처리한다 (2026-10-11).

            // ⏳ 처음 들어오면 가벼운 대기 화면을 먼저 보내고, 그 화면이 곧바로 진짜 홈(?_full=1)을 불러온다.
            //    홈을 만드는 데 시간이 걸려도 하얀 화면 대신 '잠시만 기다려 주세요' 카드가 보인다.
            if (ShouldShowHomeSplash())
                return View("~/Views/Home/IndexSplash.cshtml");

            // ✅ 로그인 사용자면 BaseController.SetUserInfo로 this.UserName 채우기
            //    (IsSignedIn 은 아래에서도 쓰므로 한 번만 호출해 재사용합니다)
            bool __signedIn = await _authService.IsSignedIn(User);
            if (__signedIn)
                await SetUserInfo();

            // ⚡ 성능: 비로그인 화면에 필요한 조회를 여기서 미리 시작합니다.
            //    (기존에는 아래 else 블록에서 순차로 await 해 약 2초를 썼습니다)
            Task<List<BulletinBoard.Services.Abstract.FamilyMilestoneDisplayVM>> tGuestMilestones = null;
            Task<List<BulletinBoard.Services.Abstract.FamilyMilestoneConfigVM>>  tGuestConfigs    = null;
            Task<BulletinBoard.Models.DailyMission2.TodayWordDetailResult>       tGuestWord       = null;

            if (!__signedIn)
            {
                int cYearGuestPre  = DateHelper.GetKoreaNow().Year;
                var kTodayGuestPre = DateHelper.GetKoreaNow().Date;

                tGuestMilestones = _studentPointGiftService.GetHomepageFamilyMilestonesAsync(cYearGuestPre);
                tGuestConfigs    = _studentPointGiftService.GetMilestoneConfigsAsync(cYearGuestPre);
                // 비로그인용 오늘 말씀은 사용자 정보가 없으므로 그대로 캐시할 수 있다
                tGuestWord       = CachedAsync(CK_GuestWord + kTodayGuestPre.ToString("yyyyMMdd"), TTL_Long,
                                       () => _dailyMission2Service.GetTodayWordDetailAsync(kTodayGuestPre, "KR", ""));
            }

            // ⚡ 성능: SetPinnedNotices() / SetLatestNoticeBoardTopsAsync() 는
            //    아래 IndexCore() 에서 같은 ViewBag 키를 다시 채웁니다.
            //    여기서 부르면 DB 왕복만 3회 더 생기고 결과는 버려지므로 제거했습니다.

            // ✅ "설정키"는 FullName이 아니라 UserName(로그인 아이디)
            var loginUserName = string.IsNullOrWhiteSpace(this.UserName) ? null : this.UserName;

            // ✅ 로그인한 경우: 익명(IPKey) 설정을 UserName으로 승격(없을 때만 복사)
            if (!string.IsNullOrWhiteSpace(loginUserName))
                _prefSvc.PromoteIfNeeded(HttpContext, loginUserName);

            // 홈 카드의 수치(5분설교 조회수 · 오늘 참여자 수)는
            // basic 계정과 SuperAdmin 에게만 보여 준다.
            bool __showHomeStats = false;
            if (__signedIn)
            {
                if (string.Equals(loginUserName, "basic", StringComparison.OrdinalIgnoreCase))
                {
                    __showHomeStats = true;
                }
                else
                {
                    try
                    {
                        var __u = await _authService.GetSignedUser(User);
                        __showHomeStats = __u != null
                            && await _authService.IsInRole(__u, nameof(Roles.SuperAdmin));
                    }
                    catch { __showHomeStats = false; }
                }
            }
            ViewBag.ShowHomeStats = __showHomeStats;

            // 모든 사용자에게 전체 메뉴 표시 (PickUserType 리다이렉트 없음)
            // ⚡ 저장 값이 항상 'A' 로 고정인데 매 요청 DB 쓰기를 하고 있었다(약 150ms).
            //    같은 세션·같은 계정이면 한 번만 저장한다.
            var __uiKey = "uiTypeSaved:" + (loginUserName ?? "");
            if (HttpContext.Session.GetString(__uiKey) != "A")
            {
                try
                {
                    _prefSvc.SaveUserTypeForCurrent(HttpContext, loginUserName, 'A');
                    HttpContext.Session.SetString(__uiKey, "A");
                }
                catch { /* 저장 실패해도 화면은 그대로 보여 준다 */ }
            }
            ViewBag.UserType = 'A';

            // ✅ 메뉴 정책 로드해서 View로 전달 (거의 바뀌지 않으므로 캐시)
            ViewBag.MenuPolicyMap = await CachedAsync(CK_MenuPolicy, TTL_Long,
                () => Task.FromResult(_prefSvc.GetMenuPolicyMap()));

            // =========================================================
            // ✅ StudentPointDashboardController 요약값을 Home/Index에서도 사용
            // =========================================================
            int integratedTotal = 0;
            bool giftAvailable = false;
            bool milestoneJustReached = false;
            int giftCredit = 0;              // 쓸 수 있는 선물 크레딧

            // ⚡ 성능: 로그인 여부와 무관한 조회는 여기서 미리 시작해 두고
            //    아래 분기 처리가 끝난 뒤 한 번에 회수합니다. (순차 → 병렬)
            var tJuboPre = CachedAsync(CK_Jubo, TTL_Short,
                () => _bandBoardService.GetPostsPageAsync("BandJuboPosts", 1, 1));

            // ⚠ 캐시를 쓰지 않는다 — 이 값은 "지금 이 순간 100% 인 사람"을 실시간으로 걸러내는
            //    조회라, 여러 사람이 공유하는 캐시로 몇 분만 묵혀도 방금 완독한 사람이
            //    한동안 축하 카드에 안 뜨는 문제가 생긴다. 매 요청마다 새로 조회한다.
            var tBibleRatesPre = Task.Run(() =>
            {
                var mgr = new ManageReadBible();
                return mgr.GetUserTotalProgressRatesByUser(
                    _hostingEnvironment, BilbeProcessOption.TotalProgressRate);
            });

            var tBibleDonePre = Task.Run(() =>
            {
                var mgr = new ManageReadBible();
                return mgr.GetUserTotalProgressRatesByUserCompleted(_hostingEnvironment);
            });

            // 오늘 "매일성경 말씀"을 수행한 사람 수 (로그인 여부와 무관하게 표시)
            var __kToday = DateHelper.GetKoreaNow().Date;
            var tTodayJoinPre = CachedAsync(CK_TodayJoin + __kToday.ToString("yyyyMMdd"), TTL_Short,
                () => _dailyMission2Service.GetTodayWordParticipantCountAsync(__kToday));

            // ⚡ IndexCore 에서 쓸 무거운 조회 2건을 여기서 미리 시작한다.
            //    아래 마일스톤 대기와 겹쳐 돌아가므로 그만큼 시간이 줄어든다.
            _tBirthdaysPre = GetUpcomingBirthdaysVmAsync();
            _tBandTopPre   = GetBandTopMessageAsync();

            if (__signedIn)   // ⚡ 위에서 이미 확인한 값 재사용 (중복 호출 제거)
            {
                if (!string.IsNullOrWhiteSpace(this.UserId) && !string.IsNullOrWhiteSpace(this.UserName))
                {
                    try
                    {
                        int cYear = DateHelper.GetKoreaNow().Year;
                        string capturedUserId   = this.UserId;
                        string capturedUserName = this.UserName;

                        // ── 1차: 서로 독립적인 쿼리 1~5, 7, 8, 9 + EvaluateAsync 사전조회(preload) 동시 실행 ──
                        var t1       = _dailyMission2Service.GetUserTotalPointsThisYearAsync(capturedUserName, cYear);
                        var t2       = _attendanceService.GetMyAttendanceCountYearToDateAsync(capturedUserId);
                        var t3       = _worldviewLessonService.GetDashboardStatusMapAsync(capturedUserId, capturedUserName);
                        var t4       = _devotionPointsService.GetMyTotalPointsAsync(capturedUserId);
                        var t5       = _attendanceService.GetEvangelismCountByUserIdAsync(capturedUserId)
                                           .ContinueWith(t => t.IsFaulted ? 0 : t.Result);
                        var t7       = _studentPointGiftService.GetAdminPurchaseListAsync(includeHidden: true);
                        var t8       = _studentPointGiftService.GetFamilyTeamGiftStatusAsync(capturedUserName, cYear, 1000);
                        // 마일스톤/설정은 사용자와 무관한 공통 값이라 비로그인 화면과 캐시를 공유한다
                        var t9       = _studentPointGiftService.GetHomepageFamilyMilestonesAsync(cYear);
                        var t10      = _studentPointGiftService.GetMilestoneConfigsAsync(cYear);
                        var t11      = _studentPointGiftService.GetMyFamilyTreeAsync(capturedUserName, cYear)
                                           .ContinueWith(t => t.IsFaulted ? null : t.Result);
                        var tPreload = _studentPointGiftService.PreloadEvalStateAsync(capturedUserName);
                        var kToday   = DateHelper.GetKoreaNow().Date;
                        var tWord    = _dailyMission2Service.GetTodayWordDetailAsync(kToday, "KR", capturedUserId)
                                           .ContinueWith(t => t.IsFaulted ? null : t.Result);
                        var tDoneMap = _dailyMission2Service.GetCalendarAllDoneMapAsync(capturedUserName, kToday)
                                           .ContinueWith(t => t.IsFaulted ? null : t.Result);

                        await Task.WhenAll(t1, t2, t3, t4, t5, t7, t8, t9, t10, tPreload, tWord, tDoneMap);
                        await t11;

                        // ── 1~5 결과로 나의 도토리 계산 ──
                        int biblePoint      = t1.Result;
                        int attendancePoint = Math.Max(0, t2.Result) * ConstatsCommon.attendancePoint;
                        int missionPoint    = t3.Result?.Sum(x => x.Value?.Score ?? 0) ?? 0;
                        int thanksPoint     = t4.Result;
                        int evangelismCount = t5.Result;

                        integratedTotal = biblePoint + attendancePoint + missionPoint + thanksPoint + (evangelismCount * 50);

                        // ── 2차: integratedTotal 필요한 쿼리 6 (preload 재사용으로 DB 조회 2개 절감) ──
                        var eval = await _studentPointGiftService.EvaluateAsync(capturedUserName, integratedTotal, tPreload.Result);
                        giftAvailable       = eval?.HasAffordableGift ?? false;
                        milestoneJustReached = eval?.Show ?? false;
                        giftCredit          = eval?.AvailableCredit ?? 0;

                        // ✅ 이미 고른(아직 관리자 승인 대기중인) 선물이 있으면 홈 카드에서 바꿀 곳을 다르게 안내한다
                        try
                        {
                            var giftBox = await _studentPointGiftService.GetMyGiftBoxAsync(capturedUserName, integratedTotal);
                            ViewBag.GiftCurrentDraft = giftBox.CurrentDraft;
                        }
                        catch { ViewBag.GiftCurrentDraft = null; }

                        // ── 7번 결과 적용 ──
                        var giftList = t7.Result;
                        if (giftList != null)
                        {
                            ViewBag.GiftRecipientList = giftList
                                .Where(r => !string.IsNullOrEmpty(r.Status)
                                    && (string.Equals(r.Status, "DRAFT",     StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(r.Status, "PURCHASED", StringComparison.OrdinalIgnoreCase)))
                                .OrderByDescending(r => r.PurchasedAt.HasValue ? r.PurchasedAt.Value : r.CreatedAt)
                                .ToList();
                        }
                        else
                        {
                            ViewBag.GiftRecipientList = null;
                        }

                        // ── 홈 "축하해 주세요~~" — 아이마다 몇 점에 무슨 선물을 신청했는지 + 최종 상태 ──
                        //    (가족팀·가족합산 선물은 가족 마일스톤 카드에서 보여 주므로 뺀다)
                        try
                        {
                            // ⚠️ "다음 선물 · 남은 점수"는 보여 주지 않는다 — 선물 점수 단계가 100점마다(100~2,000)라
                            //    다음 단계가 실제로 고를 수 있는 다음 선물과 맞지 않는다 (고를 수 있는지는 쌓인 credit · 선물 가격으로 정해짐)
                            bool IsSt(string? s, string v) => string.Equals(s ?? "", v, StringComparison.OrdinalIgnoreCase);

                            var kids = (giftList ?? new List<AdminPurchaseRowVM>())
                                .Where(r => !r.IsTeamGift && !r.IsFamilyCombinedGift
                                         && (IsSt(r.Status, "DRAFT") || IsSt(r.Status, "PURCHASED") || IsSt(r.Status, "HIDDEN")))
                                .GroupBy(r => r.UserName, StringComparer.OrdinalIgnoreCase)
                                .Select(g =>
                                {
                                    var f = g.First();
                                    return new BulletinBoard.ViewModels.Home.GiftTimelineKidVM
                                    {
                                        UserName    = f.UserName,
                                        DisplayName = string.IsNullOrWhiteSpace(f.UserFullName) ? f.UserName : f.UserFullName!,
                                        Photo       = string.IsNullOrWhiteSpace(f.UserPhotoUrl) ? "/images/icon/people/people_01.webp" : f.UserPhotoUrl!,
                                        ClassLabel  = (f.ClassYear != null && f.ClassBan != null && (f.RoleCode ?? "") == "T1001") ? $"{f.ClassYear}학년 {f.ClassBan}반" : null,
                                        RoleCode    = f.RoleCode,
                                        Total       = f.IntegratedTotal,
                                        LastAt      = g.Max(x => x.PurchasedAt ?? x.CreatedAt),
                                        Items       = g.OrderBy(x => x.MileStoneVal).ThenBy(x => x.CreatedAt)
                                                       .Select(x => new BulletinBoard.ViewModels.Home.GiftTimelineItemVM
                                                       {
                                                           Milestone   = x.MileStoneVal,
                                                           ProductID   = x.ProductID,
                                                           ProductName = x.ProductName,
                                                           Status      = (x.Status ?? "").ToUpperInvariant(),
                                                           CreatedAt   = x.CreatedAt,
                                                           PurchasedAt = x.PurchasedAt,
                                                           DeliveredAt = x.DeliveredAt,
                                                           CreditCost  = x.CreditCostSnapshot
                                                       }).ToList()
                                    };
                                }).ToList();

                            // 나 — 주문이 없어도, 닿았는데 아직 안 고른 단계가 있으면 보여 준다
                            var me = kids.FirstOrDefault(k => string.Equals(k.UserName, capturedUserName, StringComparison.OrdinalIgnoreCase));
                            if (me == null && giftAvailable)
                                me = new BulletinBoard.ViewModels.Home.GiftTimelineKidVM { UserName = capturedUserName, DisplayName = string.IsNullOrWhiteSpace(this.UserFullName) ? capturedUserName : this.UserFullName };
                            if (me != null)
                            {
                                me.IsMe = true;
                                me.Total = integratedTotal;
                                if (giftAvailable && !me.Items.Any(i => i.Status == "DRAFT"))
                                    me.PendingMilestone = eval?.Milestone;
                            }
                            ViewBag.MyGiftTimeline = me;
                            // 친구들 — 선물을 신청해서 아직 전달 전(신청중·구매완료)인 아이만, 최근 활동 순 (나는 "나의 선물함" 카드로)
                            //   ⚠️ 전달완료만 있는 아이는 넣지 않는다 — "받은 사람 명단"이 아니라 "지금 신청한 친구" 목록
                            ViewBag.GiftTimelineKids = kids
                                .Where(k => !k.IsMe && k.Items.Any(i => i.Status == "DRAFT" || i.Status == "PURCHASED"))
                                .OrderByDescending(k => k.LastAt)
                                .ToList();
                        }
                        catch (Exception exTl)
                        {
                            try { DbLogger.LogErrorAsync($"[Home/Index] 선물 타임라인 실패: {exTl.Message}"); } catch { }
                            ViewBag.GiftTimelineKids = null;
                            ViewBag.MyGiftTimeline = null;
                        }

                        // ── 8번 결과 적용 ──
                        ViewBag.FamilyTeamGift = t8.Result;
                        ViewBag.MyUserName     = capturedUserName;

                        // ── 홈 "가족 합산 시상" 카드 — 신청일·구매일 · 지난 가족 선물 · 다음 가족 단계 (2026-09-26) ──
                        //    가족 선물 VM 에는 날짜가 없어 이미 읽어 온 선물 목록(t7)에서 같은 OrderId 로 찾는다 (DB 조회 추가 없음)
                        try
                        {
                            var fg = t8.Result;
                            if (fg != null)
                            {
                                var fgRow = fg.OrderId > 0 ? giftList?.FirstOrDefault(r => r.OrderId == fg.OrderId) : null;
                                ViewBag.FamilyGiftCreatedAt   = fgRow?.CreatedAt;
                                ViewBag.FamilyGiftPurchasedAt = fgRow?.PurchasedAt;

                                // 메모 "가족합산:팀이름" = 1,000점 단계, "가족합산:팀이름:3000" = 3,000점 단계
                                string fgKey = "가족합산:" + fg.TeamName;
                                int FgMs(string memo) => memo.Length > fgKey.Length + 1 && int.TryParse(memo.Substring(fgKey.Length + 1), out var v) ? v : 1000;
                                ViewBag.FamilyGiftPast = (giftList ?? new List<AdminPurchaseRowVM>())
                                    .Where(r => r.Memo != null && (r.Memo == fgKey || r.Memo.StartsWith(fgKey + ":"))
                                             && string.Equals(r.Status, "HIDDEN", StringComparison.OrdinalIgnoreCase))
                                    .Select(r => new BulletinBoard.ViewModels.Home.GiftTimelineItemVM
                                    {
                                        Milestone = FgMs(r.Memo!), ProductID = r.ProductID, ProductName = r.ProductName,
                                        Status = "HIDDEN", CreatedAt = r.CreatedAt, PurchasedAt = r.PurchasedAt
                                    })
                                    .OrderBy(x => x.Milestone).ToList();

                                // 다음 가족 선물 단계 (AWARD) — 막대는 "도달한 마지막 단계 → 다음 단계"
                                var awards = (t10.Result ?? new List<FamilyMilestoneConfigVM>())
                                    .Where(c => string.Equals(c.MilestoneType, "AWARD", StringComparison.OrdinalIgnoreCase))
                                    .Select(c => c.PointThreshold).Distinct().OrderBy(p => p).ToList();
                                ViewBag.FamilyGiftNext = awards.FirstOrDefault(p => p > fg.TeamTotalDm2);   // 0 = 모든 단계 달성
                                ViewBag.FamilyGiftFrom = awards.Where(p => p <= fg.TeamTotalDm2).DefaultIfEmpty(0).Max();
                            }
                        }
                        catch (Exception exFg)
                        {
                            try { DbLogger.LogErrorAsync($"[Home/Index] 가족 합산 카드 부가정보 실패: {exFg.Message}"); } catch { }
                        }

                        // ── 9번 결과 적용 ── (마일스톤 표시 목록)
                        ViewBag.FamilyMilestones       = t9.Result;
                        ViewBag.FamilyMilestoneConfigs = t10.Result;
                        ViewBag.FamilyTree             = t11.IsCompletedSuccessfully ? t11.Result : null;
                        ViewBag.DM2TodayWord           = tWord.Result;
                        ViewBag.DM2DoneMap             = tDoneMap.Result;
                        ViewBag.DM2BiblePoint          = biblePoint;

                        // ── 성경통독 마지막 읽은 구절 + 통계 (/BIBLE 동일 로직) ──
                        try
                        {
                            var _bMgr  = new ManageReadBible();
                            var _bList = _bMgr.GetLastReadVerse(capturedUserId, 0, 0);
                            ViewBag.BibleLastReadInfo = _bList.Count > 0 ? _bList[0] : null;

                            // 장 메타 (책+장 별 전체 절 수)
                            var _bChapterMeta = await _db.EBibles
                                .GroupBy(e => new { e.BibleText, e.Jang })
                                .Select(g => new { BibleText = g.Key.BibleText ?? "", Jang = g.Key.Jang, TotalVerses = g.Count() })
                                .ToListAsync();

                            // 사용자가 읽은 구절 (책+장+절id)
                            var _bUserProgress = await _db.UserBibleProgresses
                                .Where(p => p.UserId == capturedUserId)
                                .Join(_db.EBibles, p => p.VerseId, e => e.Id,
                                      (p, e) => new { BibleText = e.BibleText ?? "", e.Jang, p.VerseId })
                                .ToListAsync();

                            // 완독 장 집합 (해당 장의 모든 절을 읽어야 완독)
                            var _bReadSet = _bUserProgress
                                .GroupBy(x => new { x.BibleText, x.Jang })
                                .Where(g => {
                                    var meta = _bChapterMeta.FirstOrDefault(c => c.BibleText == g.Key.BibleText && c.Jang == g.Key.Jang);
                                    return meta != null && meta.TotalVerses == g.Select(x => x.VerseId).Distinct().Count();
                                })
                                .Select(g => $"{g.Key.BibleText}_{g.Key.Jang}")
                                .ToHashSet();

                            int _bTotalChaps = _bChapterMeta.Count;
                            int _bReadChaps  = _bReadSet.Count;
                            double _bRate    = _bTotalChaps > 0 ? Math.Round(_bReadChaps * 100.0 / _bTotalChaps, 1) : 0;

                            ViewBag.BibleReadChapters  = _bReadChaps;
                            ViewBag.BibleTotalChapters = _bTotalChaps;
                            ViewBag.BibleProgressRate  = _bRate;
                        }
                        catch { ViewBag.BibleLastReadInfo = null; }
                    }
                    catch (Exception exHome)
                    {
                        // ⚠️ 위 12개 조회 중 하나만 실패해도 여기로 온다.
                        //    예전에는 가족 마일스톤까지 null 로 비워, 로그인 사용자에게만 카드가 사라졌다.
                        //    마일스톤은 사용자와 무관한 공통 값이므로 따로 다시 채운다.
                        try { DbLogger.LogErrorAsync($"[Home/Index] 로그인 사용자 조회 실패: {exHome.Message}"); } catch { }

                        ViewBag.GiftRecipientList = null;
                        ViewBag.FamilyTeamGift    = null;
                        ViewBag.FamilyTree        = null;
                        ViewBag.MyUserName        = this.UserName;

                        int cYearFb = DateHelper.GetKoreaNow().Year;
                        try
                        {
                            ViewBag.FamilyMilestones       = await _studentPointGiftService.GetHomepageFamilyMilestonesAsync(cYearFb);
                            ViewBag.FamilyMilestoneConfigs = await _studentPointGiftService.GetMilestoneConfigsAsync(cYearFb);
                        }
                        catch
                        {
                            ViewBag.FamilyMilestones       = null;
                            ViewBag.FamilyMilestoneConfigs = null;
                        }
                    }
                }

                ViewBag.IntegratedTotal      = integratedTotal;
                ViewBag.GiftAvailable        = giftAvailable;
                ViewBag.MilestoneJustReached = milestoneJustReached;
                ViewBag.GiftCredit           = giftCredit;
            }
            else
            {
                // 비로그인 안전값(뷰에서 파싱 편하게)
                ViewBag.IntegratedTotal = 0;
                ViewBag.GiftAvailable = false;
                ViewBag.MilestoneJustReached = false;
                ViewBag.GiftCredit = 0;

                // ⚡ 위에서 미리 시작해 둔 비로그인용 조회를 회수합니다.
                if (tGuestMilestones != null)
                {
                    // 가족 마일스톤은 캐시 없이 매번 조회하므로, 실패해도 화면이 죽지 않게 각각 받는다
                    try { await Task.WhenAll(tGuestMilestones, tGuestConfigs, tGuestWord); } catch { }
                    ViewBag.FamilyMilestones       = tGuestMilestones.IsCompletedSuccessfully ? tGuestMilestones.Result : null;
                    ViewBag.FamilyMilestoneConfigs = tGuestConfigs.IsCompletedSuccessfully    ? tGuestConfigs.Result    : null;
                    ViewBag.DM2TodayWord           = tGuestWord.IsCompletedSuccessfully       ? tGuestWord.Result       : null;
                }
                else
                {
                    ViewBag.FamilyMilestones = null;
                    ViewBag.FamilyMilestoneConfigs = null;
                    ViewBag.DM2TodayWord = null;
                }
            }


            // ⚡ 위에서 미리 시작해 둔 조회들을 한 번에 회수합니다.
            await Task.WhenAll(tJuboPre, tBibleRatesPre, tBibleDonePre, tTodayJoinPre);

            ViewBag.DM2TodayJoinCount = tTodayJoinPre.Result;

            // 주보 마지막 게시일
            ViewBag.JuboLastDate = tJuboPre.Result?.Posts?.FirstOrDefault()?.CreatedAt;


            // ✅ 성경 100% 완독자 목록 + 이전 완독 횟수(readBibleAllCount) 맵
            try
            {
                var allUsers = tBibleRatesPre.Result;
                ViewBag.BibleCompletedUsers = allUsers?
                    .Where(u => u.TotalProgressRate >= 100)
                    .ToList();

                // userId → readBibleAllCount (이전 완독 리셋 횟수)
                var completedList = tBibleDonePre.Result;
                ViewBag.BibleCompletedCountMap = completedList != null
                    ? completedList.GroupBy(c => c.UserId)
                                   .ToDictionary(g => g.Key, g => g.First().readBibleAllCount)
                    : new Dictionary<string, int>();
            }
            catch
            {
                ViewBag.BibleCompletedUsers = null;
                ViewBag.BibleCompletedCountMap = new Dictionary<string, int>();
            }

            // ✅ 완독 날짜(세로 타임라인용) — userId → [ (몇 독, 마친 날) ]
            //    표(BibleCompletionLog)가 아직 없으면 날짜 없이 지금처럼 보여 준다.
            try
            {
                var logs = await _db.BibleCompletionLogs
                    .OrderBy(l => l.UserId).ThenBy(l => l.RoundNo)
                    .Select(l => new { l.UserId, l.RoundNo, l.CompletedAt })
                    .ToListAsync();

                ViewBag.BibleCompletionMap = logs
                    .GroupBy(l => l.UserId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => (Round: x.RoundNo, Date: x.CompletedAt)).ToList());
            }
            catch
            {
                ViewBag.BibleCompletionMap = new Dictionary<string, List<(int Round, DateTime Date)>>();
            }

            // ✅ 도전! 성경통독 완독 선물 배너 — 완독한 본인에게만
            try
            {
                var giftSvc = HttpContext.RequestServices
                    .GetService(typeof(BibleReadingThrough.Services.BibleGift.IBibleGiftService))
                    as BibleReadingThrough.Services.BibleGift.IBibleGiftService;

                var myId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (giftSvc != null && !string.IsNullOrWhiteSpace(myId))
                {
                    // 배너에는 아이디(UserName)가 아니라 이름(예: 강세은)을 보여 준다
                    var myFullName = !string.IsNullOrWhiteSpace(this.UserFullName)
                        ? this.UserFullName
                        : await giftSvc.GetFullNameAsync(User.Identity?.Name) ?? User.Identity?.Name;

                    ViewBag.BibleGiftBanner = await giftSvc.GetBannerAsync(myId, myFullName);
                }

                // 완독 축하 카드에 "고른 선물"을 함께 보여 주기 위한 목록
                if (giftSvc != null)
                    ViewBag.BibleGiftPickMap = await giftSvc.GetCurrentPicksAsync();
            }
            catch
            {
                ViewBag.BibleGiftBanner = null;   // 표가 아직 없으면 배너 없이 간다
            }

            return await IndexCore(viewName: "Index");
        }

        // ⚡ 데이터만 돌려준다 (ViewBag 할당은 호출한 쪽에서). 사용자와 무관하므로 캐시한다.
        private Task<List<BoardPost>> GetNoticeBoardTopsAsync()
        {
            return CachedAsync(CK_BoardTops, TTL_Short, async () =>
            {
                // ✅ Korea 기준 오늘(자정)로 팝업 만료 판단
                var clientToday = DateHelper.GetKoreaNow().Date;

                // ✅ NoticeBoard(ReplayBoard) 컨텍스트 해석
                var board = await _replayRepo.ResolveBoardAsync("NoticeBoard");
                if (board == null) return null;

                // ✅ 최신 글 일부를 가져온 뒤(정렬: IsPinned DESC ...),
                //    pinned + 팝업만료조건을 통과한 것만 추립니다.
                var (items, _) = await _replayRepo.ListAsync(board, page: 1, pageSize: 50, q: null, viewerUserId: null);

                var tops = items
                    .Where(p => p.IsPinned)
                    .Where(p => !p.PopupUntil.HasValue || p.PopupUntil.Value.Date >= clientToday)
                    .ToList();

                return tops.Count == 0 ? null : tops;
            });
        }

        /// <summary>
        /// 홈 대기 화면을 보낼지 판단한다.
        /// · 브라우저로 처음 들어온 GET 요청만 (?_full=1 이 붙은 진짜 홈 요청은 제외)
        /// · 카카오톡 · 검색엔진 등 미리보기 수집기, ajax, 미리 불러오기(prefetch)는 제외 — 바로 진짜 홈을 준다
        /// </summary>
        private bool ShouldShowHomeSplash()
        {
            var req = Request;
            if (!HttpMethods.IsGet(req.Method)) return false;
            if (req.Query.ContainsKey("_full")) return false;

            // 사이트 안의 다른 화면에서 홈을 누른 경우는 그 화면이 이미 대기 카드를 보여 준다 → 대기 화면을 또 띄우지 않는다.
            var referer = req.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)
                && Uri.TryCreate(referer, UriKind.Absolute, out var refUri)
                && string.Equals(refUri.Host, req.Host.Host, StringComparison.OrdinalIgnoreCase))
                return false;

            var accept = req.Headers["Accept"].ToString();
            if (string.IsNullOrEmpty(accept) || accept.IndexOf("text/html", StringComparison.OrdinalIgnoreCase) < 0) return false;

            if (string.Equals(req.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)) return false;
            var purpose = req.Headers["Sec-Purpose"].ToString() + req.Headers["Purpose"].ToString();
            if (purpose.IndexOf("prefetch", StringComparison.OrdinalIgnoreCase) >= 0) return false;

            var ua = req.Headers["User-Agent"].ToString();
            if (string.IsNullOrEmpty(ua)) return false;
            string[] bots = { "bot", "spider", "crawl", "kakaotalk-scrap", "facebookexternalhit", "slurp", "yeti", "daum", "preview", "whatsapp", "telegram", "line/" };
            foreach (var b in bots)
                if (ua.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0) return false;

            return true;
        }

        // ✅ 메인(복잡 화면)
        public async Task<IActionResult> IndexComplex()
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var loginUserName = string.IsNullOrWhiteSpace(this.UserName) ? null : this.UserName;

            if (!string.IsNullOrWhiteSpace(loginUserName))
                _prefSvc.PromoteIfNeeded(HttpContext, loginUserName);

            // 모든 사용자에게 전체 메뉴 표시
            _prefSvc.SaveUserTypeForCurrent(HttpContext, loginUserName, 'A');
            ViewBag.UserType = 'A';

            // ✅ IndexComplex에도 동일하게 전달(누락 방지)
            ViewBag.MenuPolicyMap = _prefSvc.GetMenuPolicyMap();

            return await IndexCore(viewName: "IndexComplex");
        }

        private async Task<IActionResult> IndexCore(string viewName)
        {
            // ✅ 로그인 상태면 여기서 1번 확실히 채우기 (UserName 타이밍 꼬임 방지)
            var signedIn = await _authService.IsSignedIn(User);
            if (signedIn)
                await SetUserInfo();

            var urlInfo = UrlInfoHelper.GetUrlInfo(HttpContext);
            ViewBag.Domain = urlInfo.Domain;
            ViewBag.ControllerName = urlInfo.ControllerName;
            ViewBag.ActionName = urlInfo.ActionName;
            ViewBag.FullURL = $"{ViewBag.Domain}";


            var manageeCommon = new ManageeCommon();

            DateTime currentDate = SharedComm.GetToday();
            var baseDate = SharedComm.GetKoreaNow();
            var nowKst   = DateHelper.GetKoreaNow();

            // ⚡ 성능: 기존에 순차로 돌던 동기 DB 호출 4건을 데이터 조회/할당으로 분리해
            //    전부 동시에 시작합니다. ViewBag/ViewData 할당은 회수 후 한 곳에서만 합니다.
            //    (ViewBag 은 스레드 안전하지 않으므로 병렬 구간에서는 절대 건드리지 않습니다)
            // Index() 에서 이미 시작했으면 그대로 쓰고, 아니면(다른 진입 경로) 여기서 시작한다
            var tBirthdays       = _tBirthdaysPre ?? GetUpcomingBirthdaysVmAsync();
            var tBandBoard       = _tBandTopPre   ?? GetBandTopMessageAsync();
            var tAttendanceTrend = CachedAsync(CK_Attendance + baseDate.ToString("yyyyMMdd"), TTL_Long,
                () => _attendanceService.GetAdminTrend3WeeksAsync(baseDate));

            var tNoticeBoardTops = GetNoticeBoardTopsAsync();

            var tMenus = CachedAsync(CK_Menus, TTL_Long, () => Task.Run(() =>
                new ManageeCommon().GetMissionMenusByRole()));

            var tAward = CachedAsync(CK_Award, TTL_Medium, () => Task.Run(() =>
                new ManageCheckPoint().getGetThisWeekRewardingByAward(string.Empty, _hostingEnvironment)));

            var tOneLine = CachedAsync(CK_OneLine, TTL_Long, () => Task.Run(() =>
                new ManageOneLineFamousWord().getOneLineFamousWord(0)));

            // 5분설교는 날마다 새 글이 올라오므로 날짜별 키 + 짧은 유지시간을 쓴다
            var tRhapsody = CachedAsync(CK_Rhapsody + nowKst.ToString("yyyyMMdd"), TTL_Short, () => Task.Run(() =>
                new ManageRhapsodyOfReality().getRhapsodyOfReality1(0, string.Empty)));

            var tPinned = CachedAsync(CK_Pinned, TTL_Short, () => Task.Run(() =>
                _noticeService.GetPinnedNoticesForBanner(nowKst, maxN: 20)));

            await Task.WhenAll(tBirthdays, tAttendanceTrend, tBandBoard, tNoticeBoardTops,
                               tMenus, tAward, tOneLine, tRhapsody, tPinned);

            // ── 회수 후 ViewBag/ViewData 할당 (단일 스레드) ──
            var birthVm = tBirthdays.Result;
            ViewBag.BirthDayList7      = birthVm?.Rows;
            ViewBag.BirthDayWithinDays = birthVm?.WithinDays ?? 7;
            ViewBag.BirthDayTotalCount = birthVm?.TotalCount ?? 0;

            var bandMsg = tBandBoard.Result;
            ViewBag.BandBoardMessage = string.IsNullOrEmpty(bandMsg) ? "게시판 데이터가 없습니다." : bandMsg;

            ViewBag.NoticeBoardTopPosts = tNoticeBoardTops.Result;

            ViewData["missionMenuItem"] = tMenus.Result;
            ViewBag.PointAwardUser      = tAward.Result;

            var oneLineList = tOneLine.Result;
            if (oneLineList != null && oneLineList.Count > 0)
            {
                int idx = SharedComm.CalculateIndex(oneLineList.Count, currentDate);
                ViewData[HttpContextSessionCategory.OneLineText.ToString()] = oneLineList[idx].OneLineText;
                ViewData["OneLineTextID"] = oneLineList[idx].Id;
            }

            var rhapList = tRhapsody.Result;
            ViewBag.RhapsodyOfRealitys = (rhapList != null && rhapList.Count > 0) ? rhapList[0] : null;

            var pinned = tPinned.Result;
            if (pinned == null || pinned.Count == 0)
            {
                ViewBag.PinnedNotices = null;
                ViewBag.LatestNotice  = null;
                ViewBag.LatestAttachments = null;
            }
            else
            {
                ViewBag.PinnedNotices     = pinned;
                ViewBag.LatestNotice      = pinned.FirstOrDefault();
                ViewBag.LatestAttachments = null;
            }
            ViewBag.AttendanceAdminTrendVM = tAttendanceTrend.Result;

            // 로그인 사용자 처리
            if (signedIn)
            {
                await BindSignedUserHeaderAndMessagesAsync(currentDate);
            }

            // ✅ 비로그인: 팝업도 의미 없으니 false
            if (!signedIn || string.IsNullOrEmpty(UserName))
            {
                ViewBag.ShowVBSParticipationSurveyGuide = false;
                return View(viewName);
            }

            // ✅ 로그인 + UserName 있음: 겨울수련회 참여 팝업 노출 여부
            try
            {
                ViewBag.ShowVBSParticipationSurveyGuide =
                    await _VBSParticipationSurveyService.ShouldShowGuideAsync(this.UserName, this.UserId);
            }
            catch
            {
                ViewBag.ShowVBSParticipationSurveyGuide = false;
            }

            // 버튼 문구(뷰에서 사용)
            ViewBag.VBSParticipationSurveyGuideBtn_Now = "지금 참여하기";
            ViewBag.VBSParticipationSurveyGuideBtn_Later = "다음에 할래요";
            ViewBag.VBSParticipationSurveyGuideBtn_3d = "3일후 할래요";
            ViewBag.VBSParticipationSurveyGuideBtn_7d = "7일후 할래요";

            // ✅ 진행률/오늘미션/퀴즈 세팅 후 모델 반환
            var manageReadBible = new ManageReadBible();
            ViewBag.TotalProgressRate = manageReadBible.GetTotalBibleProgressRate(UserId);

            // ⚡ 같은 조회를 두 번 하고 있었다(ViewBag 용 1번, 모델용 1번). 한 번만 하고 나눠 쓴다.
            TodayCheckCompositeData value = manageeCommon.getTodayStatusData(UserName, GetWorkDT());
            ViewBag.TodayCheckCompositeData = value;

            TodayCollectQuestion todayCollectQuestion = SharedComm.ReadTodayQuestionMission(0, UserName, GetWorkDT());
            ViewBag.todayCollectQuestion = todayCollectQuestion;

            return View(viewName, value);
        }

        /// <summary>
        /// ✅ 겨울수련회 팝업 유예 저장
        /// days=0 => "다음에 할래요"(저장 안 함)
        /// days=3 => 3일 유예
        /// days=7 => 7일 유예
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeferVBSParticipationSurveyGuide(int days)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            if (string.IsNullOrWhiteSpace(this.UserName))
                return Json(new { ok = false, msg = "UserName missing" });

            await _VBSParticipationSurveyService.DeferGuideAsync(this.UserName, days);
            return Json(new { ok = true });
        }

        private async Task BindSignedUserHeaderAndMessagesAsync(DateTime currentDate)
        {
            await SetUserInfo();

            var manageHumanResource = new ManageHumanResource();
            List<UserInfo> userInfo = manageHumanResource.getUserInfos(
                _hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

            if (userInfo.Count > 0)
            {
                ViewData["ProfilePicturePath"] = userInfo[0].ProfilePicturePath;
                ViewData["UserId"] = userInfo[0].UserId;
                ViewData["UserInfo"] = userInfo[0];

                HttpContext.Session.SetString(HttpContextSessionCategory.DeptCode.ToString(), userInfo[0].DeptCode);
                HttpContext.Session.SetString(HttpContextSessionCategory.DeptNM.ToString(), userInfo[0].DeptNM);
            }
            else
            {
                ViewData["ProfilePicturePath"] = ConstatsCommon.representFaceImage;
                ViewData["UserId"] = UserId;
            }

            var managePopMessage = new ManageMessageCenter();
            ViewBag.PopMessages = managePopMessage.getPopMessage(
                _hostingEnvironment, 0, string.Empty, UserName, PopMessageReadMehod.CheckMyNessMessage, UserName);

            bool isSuperAdmin = await _authService.IsInRole(await _authService.GetSignedUser(User), nameof(Roles.SuperAdmin));
            if (isSuperAdmin)
            {
                var manageeCommon = new ManageeCommon();
                ViewBag.DB_NAME = manageeCommon.get_DB_NAME();

                List<VisitInfoData> visitInfo = manageeCommon.getVisitInfo();
                List<VisitDateInfoData> visitDateInfo = manageeCommon.getVisitDateInfo();

                foreach (var va in visitInfo)
                    ViewData[va.kind.ToString()] = va.visited;

                ViewBag.VisitDateInfoDatas = visitDateInfo;

                var manageBessing = new ManageBessingACouple();
                string exposeDate = SharedComm.GetTodayString();

                var dataHusband = manageBessing.getBessingACouple(Couple.Husband.ToString(), 0, exposeDate);
                var dataWife = manageBessing.getBessingACouple(Couple.Wife.ToString(), 0, exposeDate);

                int idxH = SharedComm.CalculateIndex(dataHusband.Count, currentDate);
                int idxW = SharedComm.CalculateIndex(dataWife.Count, currentDate);

                ViewData["Husband"] = dataHusband[idxH].visited;
                ViewData["Wife"] = dataWife[idxW].visited;
            }
        }

        private void BindOneLineFamousWord(DateTime currentDate)
        {
            var manageOneLine = new ManageOneLineFamousWord();
            List<OneLineFamousWords> values = manageOneLine.getOneLineFamousWord(0);
            int index = SharedComm.CalculateIndex(values.Count, currentDate);

            ViewData[HttpContextSessionCategory.OneLineText.ToString()] = values[index].OneLineText;
            ViewData["OneLineTextID"] = values[index].Id;
        }
 

        private void BindRhapsody()
        {
            var manageRhapsody = new ManageRhapsodyOfReality();
            var list = manageRhapsody.getRhapsodyOfReality1(0, string.Empty);
            ViewBag.RhapsodyOfRealitys = list[0];
        }

        // ⚡ 데이터만 돌려준다 (ViewBag 할당은 호출한 쪽에서)
        private Task<string> GetBandTopMessageAsync()
        {
            // 최신 글 1건 — 사용자와 무관하므로 짧게 캐시한다
            return CachedAsync(CK_BandTop, TTL_BandTop, async () =>
            {
                var bandBoard = await _bandBoardService.GetPostsPageAsync("BandPosts", 1, 1);
                return (bandBoard?.Posts != null && bandBoard.Posts.Any())
                    ? $"{bandBoard.Posts.First().Content}"
                    : "게시판 데이터가 없습니다.";
            });
        }

        private void SetLatestNotices()
        {
            // (deprecated) 기존 단일 최신 공지 로직
            SetPinnedNotices();
        }

        // ✅ 공지 배너: IsPinned=true 여러 건을 서비스에서 선별해서 전달
        private void SetPinnedNotices()
        {
            var nowKst = DateHelper.GetKoreaNow();

            var pinned = _noticeService.GetPinnedNoticesForBanner(nowKst, maxN: 20);

            if (pinned == null || pinned.Count == 0)
            {
                ViewBag.PinnedNotices = null;
                ViewBag.LatestNotice = null;
                ViewBag.LatestAttachments = null;
                return;
            }

            // ✅ 배너에서는 조회수 증가하지 않음(상세 진입 시에만 증가)
            ViewBag.PinnedNotices = pinned;

            // ✅ 기존 코드/뷰 호환(단일 최신 공지 참조가 남아있을 수 있어 첫 번째도 세팅)
            ViewBag.LatestNotice = pinned.FirstOrDefault();
            ViewBag.LatestAttachments = null;
        }

        // ✅ async void 제거
        // ⚡ 데이터만 돌려준다. ViewBag 할당은 호출한 쪽에서 단일 스레드로 한다.
        //    (병렬 구간에서 ViewBag 을 건드리면 안 되기 때문)
        private Task<UpcomingBirthdayListVM> GetUpcomingBirthdaysVmAsync()
        {
            // 누가 보든 같은 목록이라 공용 캐시를 쓴다 (홈에서 가장 무거운 조회 중 하나)
            // 생일 목록은 날이 바뀌면 달라지므로 키에 오늘 날짜를 붙인다
            return CachedAsync(CK_Birthdays + DateHelper.GetKoreaNow().ToString("yyyyMMdd"), TTL_Birthdays, async () =>
            {
                var v = await _presentPickService.GetUpcomingBirthdaysAsync(7, "all", _hostingEnvironment);

                if (v?.Rows != null)
                {
                    v.Rows = v.Rows
                        .Where(r => r.User != null &&
                            !string.Equals(r.User.birthDay_PublishYN, "h", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
                return v;
            });
        }

        public async Task<IActionResult> goToday()
        {
            string todayDT = SharedComm.GetTodayString();
            HttpContext.Session.SetString(DateKind.TodayDT.ToString(), todayDT);
            HttpContext.Session.SetString(DateKind.WorkDT.ToString(), todayDT);
            return RedirectToLocal("~/Home/MissionMainBoard");
        }

        public async Task<IActionResult> goWorkDT(DateTime? workDT)
        {
            DateTime workDate = SharedComm.GetToday();
            if (workDT != null) workDate = (DateTime)workDT;

            HttpContext.Session.SetString(DateKind.WorkDT.ToString(), workDate.ToString("yyyy-MM-dd"));
            return RedirectToLocal("~/Home/MissionMainBoard");
        }

        [Authorize]
        public async Task<IActionResult> MissionMainBoard(DateTime? naviDate)
        {
            DateTime WeekNaviDT = DateHelper.GetKoreaNow();

            var UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;

            if (naviDate == null)
            {
                WeekNaviDT = SharedComm.GetToday();
            }
            else
            {
                WeekNaviDT = (DateTime)naviDate;
            }

            ViewBag.PrevDateTime = SharedComm.GetTodayString(-7);
            ViewBag.NowDateTime = SharedComm.GetTodayString();
            ViewBag.NextDateTime = SharedComm.GetTodayString(7);

            ManageeCommon manageeCommon = new ManageeCommon();
            manageeCommon.DeleteClickHistory("guest");
            TodayCheckCompositeData value = manageeCommon.getTodayStatusData(UserName, GetWorkDT());

            TodayCollectQuestion todayCollectQuestion = SharedComm.ReadTodayQuestionMission(0, UserName, GetWorkDT());
            ViewBag.todayCollectQuestion = todayCollectQuestion;

            value.WeekStatusDatas = manageeCommon.getWeekStatusData(UserName, WeekNaviDT, false, 0);
            List<WeekStatusData> monthStatusData = manageeCommon.getWeekStatusData(UserName, SharedComm.GetToday(), true, 0);

            string monthStatusDataString = SharedComm.GetDaysWithDCountGreaterThanZero(monthStatusData);

            ViewData["MonString"] = monthStatusDataString;

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();

                ManageHumanResource manageHumanResource = new ManageHumanResource();

                List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, UserId, string.Empty, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

                if (userInfo.Count > 0)
                {
                    ViewData["ProfilePicturePath"] = userInfo[0].ProfilePicturePath;
                    ViewData["UserName"] = userInfo[0].UserName;
                    ViewData["UserId"] = userInfo[0].UserId;
                    ViewData["RoleCode"] = userInfo[0].RoleCode;
                }
                else
                {
                    ViewData["ProfilePicturePath"] = ConstatsCommon.representFaceImage;
                    ViewData["UserId"] = UserId;
                }

                ManageMessageCenter managePopMessage = new ManageMessageCenter();

                List<PopMessages> popMessages = managePopMessage.getPopMessage(_hostingEnvironment, 0, string.Empty, UserName, PopMessageReadMehod.CheckMyNessMessage, UserName);

                if (popMessages.Count > 0)
                {
                    ViewData["PM"] = popMessages[0].MessageText;
                    ViewData["PMID"] = popMessages[0].Id;
                }
                else
                {
                    ViewData["PM"] = string.Empty;
                }

                ManageCommonCookie __ManageCommonCookie = new ManageCommonCookie();

                List<UserSettingsCookie> UserSettingsCookies = __ManageCommonCookie.getUserCookieSettings(UserId, CookieKey.MissionMainBoard_HideDay.ToString());

                if (UserSettingsCookies.Count > 0)
                {
                    ViewBag._UserSettingsCookie = UserSettingsCookies[0];
                }
                else
                {
                    ViewBag.MissionMainBoard_HideDay = null;
                }

                ManageCheckPoint manageCheckPoint = new ManageCheckPoint();

                var DeptCode = userInfo[0].DeptCode;

                List<CheckPoint> _CheckPoint = manageCheckPoint.getViewAllPointInfo(UserId);
                var __CheckPoint = _CheckPoint.FirstOrDefault(p => p.UserId == UserId);
                ViewBag._CheckPoint = __CheckPoint;

                List<AwardBase> _AwardBase = manageCheckPoint.getAwardBase();

                var maxAward = _AwardBase
                            .OrderByDescending(a => a.AwardLevel)
                            .FirstOrDefault();

                if (maxAward != null)
                {
                    ViewBag.maxAwardLevel = maxAward.AwardLevel;
                }
                else
                {
                    ViewBag.maxAwardLevel = 1000;
                }

                List<PointAwardUser> PointAwardUsers = manageCheckPoint.getGetThisWeekRewardingByAward(string.Empty, _hostingEnvironment);

                ViewBag.PointAwardUserExists = false;
                ViewBag.PointAwardUserExists = PointAwardUsers.Any(p => p.UserName != null && p.UserName == this.UserName);
            }

            return View(value);
        }

        public async Task<IActionResult> DeletePopMessage(int Id, string MessageText)
        {
            ManageMessageCenter manageMessageCenter = new ManageMessageCenter();
            manageMessageCenter.DeletePopMessage(Id);
            return Json("OK");
        }

        public async Task<IActionResult> NaviMonth(string UserName)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;
            }

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
                ViewData["LoginUserName"] = this.UserName;
                ViewData["LoginFullName"] = this.UserFullName;
            }

            ManageeCommon manageeCommon = new ManageeCommon();

            List<AllStatusData> allStatusDatas = manageeCommon.getAllStatusData(UserName);
            ManageHumanResource manageHumanResource = new ManageHumanResource();
            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, string.Empty, UserName, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

            ViewData["userInfo"] = userInfo[0];
            return View(allStatusDatas);
        }

        public async Task<IActionResult> NaviHistory(string UserName)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;
            }

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
                ViewData["LoginUserName"] = this.UserName;
                ViewData["LoginFullName"] = this.UserFullName;
            }

            ManageeCommon manageeCommon = new ManageeCommon();

            List<AllStatusData> allStatusDatas = manageeCommon.getAllStatusData(UserName);
            ManageHumanResource manageHumanResource = new ManageHumanResource();
            List<UserInfo> userInfo = manageHumanResource.getUserInfos(_hostingEnvironment, string.Empty, UserName, 0, 0, 0, string.Empty, string.Empty, RankQueryOption.None);

            ViewData["userInfo"] = userInfo[0];
            return View(allStatusDatas);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserType()
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var loginUserName = string.IsNullOrWhiteSpace(this.UserName) ? null : this.UserName;
            _prefSvc.ClearUserTypeForCurrent(HttpContext, loginUserName);
            return RedirectToAction(nameof(PickUserType), new { force = true });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserType()
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var loginUserName = string.IsNullOrWhiteSpace(this.UserName) ? null : this.UserName;

            _prefSvc.ClearUserTypeForCurrent(HttpContext, loginUserName);

            // ✅ 무조건 선택화면 띄우기 (중요!)
            return RedirectToAction(nameof(PickUserType), new { force = true });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> PickUserType(bool force = false)
        { 
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var loginUserName = string.IsNullOrWhiteSpace(this.UserName) ? null : this.UserName;

            if (!string.IsNullOrWhiteSpace(loginUserName))
                _prefSvc.PromoteIfNeeded(HttpContext, loginUserName);

            var type = _prefSvc.GetEffectiveUserType(HttpContext, loginUserName);

            // ✅ force가 아니면 기존 정책 유지(한번 고르면 다시 못봄)
            if (!force && type != null)
                return RedirectToAction(nameof(Index));

            // ✅ force면 화면을 보여줌(유형 변경)
            ViewBag.UserType = type; // 화면에서 현재 선택값 표시용(선택)
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PickUserType(string userType)
        { 


            userType = (userType ?? "").Trim().ToUpperInvariant();
            if (userType != "A" && userType != "T" && userType != "P" && userType != "C")
                userType = "C";

            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var loginUserName = string.IsNullOrWhiteSpace(this.UserName) ? null : this.UserName;

            _prefSvc.SaveUserTypeForCurrent(HttpContext, loginUserName, userType[0]);

            return RedirectToAction(nameof(Index));
        }

        private DateTime GetWorkDT()
        {
            DateTime workDT = DateHelper.GetKoreaToday();
            var workDTInside = HttpContext.Session.GetString(DateKind.WorkDT.ToString());
            if (workDTInside != null)
                workDT = DateTime.Parse(workDTInside);
            return workDT;
        }
    }
}

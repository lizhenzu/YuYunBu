using BibleReadingThrough.Data;
using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Models.DailyMission2;
using BulletinBoard.Services;
using BulletinBoard.Services.Abstract;
using BulletinBoard.Services.DailyMission2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize]
    public class DailyMission4Controller : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IDailyMission2Service _iDailyMission2Service;
        private readonly AppDbContext _db;
        private readonly string addedMessage = "🚀 포인트가 쑥 올라갔어요!";

        // ✅ AWARD: IAwardDashboardService 안에 "포인트 도달 처리 서비스"까지 포함된 상태
        private readonly IAwardDashboardService _awardService;
        private readonly IStudentPointGiftService _giftService;   // ✅ 팀 선물 지급 이력 조회용

        // 선물 크레딧을 MyGiftBox 와 똑같이 계산하려면 통합 점수가 필요하다.
        // (미션 + 출석 + 세계관 + 감사 + 전도)
        private readonly IAttendanceService _attendanceService;
        private readonly IWorldviewLessonService _worldviewService;
        private readonly IDevotionPointsService _devotionService;

        public DailyMission4Controller(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IDailyMission2Service dailyMission2Service,
            IUserSettingsService userSettingsService,
            IAwardDashboardService awardService,
            IWebHostEnvironment hostingEnvironment,
            AppDbContext db,
            IStudentPointGiftService giftService,
            IAttendanceService attendanceService,
            IWorldviewLessonService worldviewService,
            IDevotionPointsService devotionService
        ) : base(authService, userManager, hostingEnvironment)
        {
            _attendanceService = attendanceService;
            _worldviewService  = worldviewService;
            _devotionService   = devotionService;
            _authService = authService;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _iDailyMission2Service = dailyMission2Service;
            _userSettingsService = userSettingsService;
            _awardService = awardService;
            _db = db;
            _giftService = giftService;
        }
  
        /// <summary>
        /// 통합 점수 — StudentPointDashboardController.ComputeIntegratedTotalAsync 와 같은 계산이다.
        /// 선물 크레딧은 이 점수를 기준으로 정해지므로, MyGiftBox 와 어긋나지 않도록 같은 식을 쓴다.
        ///   미션(연 누적) + 출석 + 세계관 수업 + 감사 + 전도×50
        /// </summary>
        private async Task<int> ComputeIntegratedTotalAsync(int year)
        {
            int bible = 0, attendancePoint = 0, mission = 0, thanks = 0, evangelism = 0;

            try
            {
                var yearPt = await _iDailyMission2Service.GetYearPointAsync(this.UserName, year);
                bible = yearPt?.YearTotalPoint ?? 0;
            }
            catch { bible = 0; }

            try
            {
                var cnt = await _attendanceService.GetMyAttendanceCountYearToDateAsync(this.UserId);
                attendancePoint = Math.Max(0, cnt) * ConstatsCommon.attendancePoint;
            }
            catch { attendancePoint = 0; }

            try
            {
                var map = await _worldviewService.GetDashboardStatusMapAsync(this.UserId, this.UserName);
                mission = (map == null) ? 0 : map.Values.Sum(x => x == null ? 0 : x.Score);
            }
            catch { mission = 0; }

            try { thanks = await _devotionService.GetMyTotalPointsAsync(this.UserId); }
            catch { thanks = 0; }

            try { evangelism = await _attendanceService.GetEvangelismCountByUserIdAsync(this.UserId); }
            catch { evangelism = 0; }

            return bible + attendancePoint + mission + thanks + (evangelism * 50);
        }

        [HttpGet]
        public async Task<IActionResult> Participants(int? year)
        {
            // ✅ 기존 URL 호환: 개인 참여자 페이지로 이동
            return await ParticipantPersons(year);
        }

        [HttpGet]
        public async Task<IActionResult> ParticipantPersons(int? year)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            int cYear = year ?? DateHelper.GetKoreaNow().Year;

            int targetPoint = await _iDailyMission2Service.GetTargetPointFromAwardBaseAsync();

            var rows = await _iDailyMission2Service.GetParticipantPersonCardsAsync(cYear);

            foreach (var r in rows)
            {
                if (targetPoint <= 0) r.AchievePercent = 0;
                else
                {
                    var pct = (int)Math.Round((r.YearPoint * 100.0) / targetPoint);
                    r.AchievePercent = Math.Max(0, Math.Min(100, pct));
                }
            }

            var vm = new ParticipantCardsVM
            {
                Year = cYear,
                TargetPoint = targetPoint,
                CurrentUserName = this.UserName ?? "", // ✅ 추가 (BaseController에 UserName이 있으니)
                Rows = rows
            };

            return View("~/Views/DailyMission4/ParticipantPersons.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> ParticipantTeams(int? year)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            int cYear = year ?? DateHelper.GetKoreaNow().Year;

            // ── 세 쿼리 병렬 실행 ──────────────────────────────────────
            var tTarget = _iDailyMission2Service.GetTargetPointFromAwardBaseAsync();
            var tRows   = _iDailyMission2Service.GetParticipantTeamCardsAsync(cYear);
            var tGift   = _giftService.GetFamilyCombinedGiftStatusAllAsync();

            await Task.WhenAll(tTarget, tRows, tGift);

            int targetPoint = tTarget.Result;
            var rows        = tRows.Result;

            foreach (var r in rows)
            {
                if (targetPoint <= 0) r.AchievePercent = 0;
                else
                {
                    var pct = (int)Math.Round((r.YearPoint * 100.0) / targetPoint);
                    r.AchievePercent = Math.Max(0, Math.Min(100, pct));
                }
            }

            var vm = new ParticipantCardsVM
            {
                Year            = cYear,
                TargetPoint     = targetPoint,
                CurrentUserName = this.UserName ?? "",   // 가계도에서 '본인'을 표시하는 데 쓴다.
                Rows            = rows
            };

            ViewBag.FamilyCombinedGiftInfo = tGift.IsCompletedSuccessfully
                ? tGift.Result
                : new System.Collections.Generic.Dictionary<string, (int, string, string)>();

            return View("~/Views/DailyMission4/ParticipantTeams.cshtml", vm);
        }
// ============================================================
        // ✅ 공통: lang 정규화 + (query로 왔으면 저장) + ViewBag.Lang 세팅
        // ============================================================
        private async Task<string> ResolveLangAsync(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
            {
                lang = await _userSettingsService.GetLangAsync(this.UserId);
            }

            lang = (lang ?? "ko").ToLowerInvariant();
            if (lang != "ko" && lang != "en" && lang != "al") lang = "ko";

            try
            {
                if (Request?.Query != null && Request.Query.ContainsKey("lang"))
                {
                    await _userSettingsService.SaveLangAsync(this.UserId, lang);
                }
            }
            catch { }

            ViewBag.Lang = lang;
            return lang;
        }

        // ============================================================
        // ✅ AWARD: 서비스가 리턴한 "팝업 결과"를 TempData로 반영
        // - IAwardDashboardService에 추가하신 새 메서드 사용 전제
        // ============================================================
        private void ApplyAwardPopupResult(object popupResultObj)
        {
            if (popupResultObj == null) return;

            // 타입을 강하게 참조할 수 있으면 아래 dynamic 대신 실제 타입으로 바꾸세요.
            // 예: AwardPopupResult popup = (AwardPopupResult)popupResultObj;
            dynamic popup = popupResultObj;

            try
            {
                bool hasNew = popup.HasNewAwards;
                if (!hasNew) return;

                // 기존 팝업이 이미 있으면(예: 저장 결과 안내) 덮어쓸지 정책 결정:
                // - "도달 축하" 팝업이 더 중요하면 덮어쓰기(현재는 덮어쓰기)
                // - 덮어쓰기 싫으면 아래 if로 방어하세요.
                // var existing = TempData.Peek("DM2_OK_POPUP")?.ToString();
                // if (!string.IsNullOrWhiteSpace(existing)) return;

                TempData["DM2_AWARD_URL"] = (string)popup.AwardUrl;
                TempData["DM2_AWARD_LEVELS"] = (string)popup.LevelsText;
                TempData["DM2_OK_POPUP"] = (string)popup.Message;
            }
            catch
            {
                // 팝업은 부가 기능이므로 실패해도 메인 흐름은 유지
            }
        }
        [HttpGet]
        public async Task<IActionResult> Branch()
        {

            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());


            var manageeCommon = new ManageeCommon();
            var manageReadBible = new ManageReadBible();
            // 로그인 + UserName 있음: 진행률/오늘미션/퀴즈 세팅 후 모델 반환
            if (await _authService.IsSignedIn(User))
            {
                await SetUserInfo();
                ViewBag.TotalProgressRate = manageReadBible.GetTotalBibleProgressRate(UserId);
            }
            else
            {
                ViewBag.TotalProgressRate = 0;
            }

            ViewBag.TodayCheckCompositeData = manageeCommon.getTodayStatusData(this.UserName, DateHelper.GetKoreaToday());

            // 도전!!! 성경말씀 연간 점수 — 홈·미션홈과 같은 계산(GetYearPointAsync)으로 통일
            try { ViewBag.YearTotalPoint = string.IsNullOrWhiteSpace(this.UserName) ? 0 : (await _iDailyMission2Service.GetYearPointAsync(this.UserName, DateHelper.GetKoreaNow().Year)).YearTotalPoint; }
            catch { ViewBag.YearTotalPoint = null; }


            // 이번주 시상
            var manageCheckPoint = new ManageCheckPoint();
            ViewBag.PointAwardUser = manageCheckPoint.getGetThisWeekRewardingByAward(string.Empty, _hostingEnvironment);


            List<BibleLastReadInfo> BibleLastReadInfos = manageReadBible.GetLastReadVerse(UserId, 0, 0);

            if (BibleLastReadInfos.Count > 0)
            {
                ViewBag.BibleLastReadInfo = BibleLastReadInfos[0];
            }
            else
            {
                ViewBag.BibleLastReadInfo = null;
            }



            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Index(string date, string lang = "")
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            await ResolveLangAsync(lang);

            var vm = await BuildDashboardVM(this.UserName, date);

            // ================================
            // 🔎 DEBUG 1: 현재 포인트 확인
            // ================================
            TempData["DM2_DEBUG_POINT"] =
                $"Year={vm.SelectedDate.Year}, TotalPoint={vm.YearPoint?.YearTotalPoint}";

            // ✅ AWARD: 도달한 레벨 Eligible 보장
            await _awardService.EnsureEligibleAwardsByTotalAsync(
                this.UserName,
                vm.SelectedDate.Year,
                vm.YearPoint?.YearTotalPoint ?? 0
            );

            ViewBag.UserName = this.UserName;
            // ✅ Bug fix: Index 액션에서도 YearTotalPoint를 ViewBag에 세팅
            //    → _OkPopup.cshtml의 @ViewBag.YearTotalPoint 가 값을 표시할 수 있도록
            ViewBag.YearTotalPoint = vm.YearPoint?.YearTotalPoint ?? 0;

            // ================================
            // 🔎 DEBUG 2: 기존 팝업 존재 여부
            // ================================
            var existingPopup = TempData.Peek("DM2_OK_POPUP")?.ToString();
            TempData["DM2_DEBUG_EXISTING_POPUP"] =
                string.IsNullOrWhiteSpace(existingPopup)
                    ? "NO existing popup"
                    : $"EXISTING POPUP = {existingPopup}";

            int currentYearPoint = vm.YearPoint?.YearTotalPoint ?? 0;

            if (string.IsNullOrWhiteSpace(existingPopup))
            {
                var (hasPending, pendingLevel) =
                    await _awardService.HasPendingAwardSelectionAsync(
                        this.UserName,
                        vm.SelectedDate.Year,
                        currentYearPoint   // ✅ Bug fix: 실제 포인트 전달 → 이 값 이하의 레벨만 팝업
                    );

                // ================================
                // 🔎 DEBUG 3: Pending Award 상태
                // ================================
                TempData["DM2_DEBUG_PENDING"] =
                    $"hasPending={hasPending}, pendingLevel={pendingLevel}";

                if (hasPending)
                {
                    const string key = "DM2_AwardPopupHide";
                    bool suppressed =
                        await _userSettingsService.IsCookieSuppressedAsync(this.UserId, key);

                    // ================================
                    // 🔎 DEBUG 4: Suppress 여부
                    // ================================
                    TempData["DM2_DEBUG_SUPPRESS"] =
                        $"suppressed={suppressed}";

                    if (!suppressed)
                    {
                        TempData["DM2_AWARD_URL"] =
                            $"/DailyMission2Award/PickGifts?year={vm.SelectedDate.Year}";

          TempData["DM2_OK_POPUP"] =
    $"🎁 축하해요!\n" +
    $"시상 포인트 {pendingLevel}점을 달성했어요!\n" +
    $"지금 선물을 골라볼까요?";

                        // ================================
                        // 🔎 DEBUG 5: 팝업 생성 확인
                        // ================================
                        TempData["DM2_DEBUG_POPUP_CREATED"] = "POPUP CREATED";
                    }
                }
            }

            ViewBag.Toast = TempData["Toast"]?.ToString() ?? "";

            // ── 선물 : MyGiftBox 와 같은 기준으로 계산해 넘긴다 ──
            //   통합 점수 → 100점 단위 마일스톤 → 쌓인 크레딧(원) − 쓴 금액 = 쓸 수 있는 크레딧
            try
            {
                int integrated = await ComputeIntegratedTotalAsync(vm.SelectedDate.Year);
                var eval = await _giftService.EvaluateAsync(this.UserName, integrated);

                // 선물은 학생만 고를 수 있다. 학생이 아니면 선물 관련 문구를 아예 내보내지 않는다.
                bool isStudent = await _giftService.IsStudentAsync(this.UserName);

                ViewBag.GiftIsStudent       = isStudent;
                ViewBag.GiftIntegrated      = integrated;
                ViewBag.GiftMilestone       = eval.Milestone;
                ViewBag.GiftAvailableCredit = eval.AvailableCredit;
                ViewBag.GiftHasAffordable   = eval.HasAffordableGift;
            }
            catch
            {
                ViewBag.GiftIsStudent       = false;
                ViewBag.GiftIntegrated      = 0;
                ViewBag.GiftMilestone       = 0;
                ViewBag.GiftAvailableCredit = 0;
                ViewBag.GiftHasAffordable   = false;
            }

            if (!string.IsNullOrWhiteSpace(this.UserName))
            {
                var tFamilyTree = _giftService.GetMyFamilyTreeAsync(this.UserName, vm.SelectedDate.Year)
                    .ContinueWith(t => t.IsFaulted ? null : t.Result);
                var tWordDetail = _iDailyMission2Service.GetTodayWordDetailAsync(vm.SelectedDate, "KR", this.UserId)
                    .ContinueWith(t => t.IsFaulted ? null : t.Result);
                await Task.WhenAll(tFamilyTree, tWordDetail);
                ViewBag.FamilyTree        = tFamilyTree.IsCompletedSuccessfully ? tFamilyTree.Result : null;
                ViewBag.TodayWordToReadRow = tWordDetail.Result;
            }

            try
            {
                var prayPhrase = SharedComm.GetTodayCollectPrayPhrase(vm.SelectedDate, "ko", this.UserId);
                ViewBag.TodayPrayPhraseRow = prayPhrase;
            }
            catch { ViewBag.TodayPrayPhraseRow = null; }

            return View("~/Views/DailyMission4/Dashboard.cshtml", vm);
        }

        // ============================================================
        // ✅ AWARD POPUP: 숨김 처리(1일/3일/7일)
        // POST: /DailyMission2/AwardPopupHide
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AwardPopupHide(int days, string returnUrl = "")
        {
            if (days != 1 && days != 3 && days != 7) days = 1;

            const string key = "DM2_AwardPopupHide";

            var todayStr = DateHelper.GetKoreaNow().ToString("yyyy-MM-dd");
            await _userSettingsService.UpsertCookieSuppressionAsync(this.UserId, key, todayStr, days);

            if (string.IsNullOrWhiteSpace(returnUrl))
                return RedirectToAction(nameof(Index));

            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction(nameof(Index));

            return Redirect(returnUrl);
        }

        // ============================================================
        // GET: /DailyMission2/Word
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Word(string date, string lang = "", int YearTotalPoint = 0)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var vm = await BuildDashboardVM(this.UserName, date);
            var userLang = await ResolveLangAsync(lang);

            var detail = await _iDailyMission2Service.GetTodayWordDetailAsync(vm.SelectedDate, userLang, this.UserId);
            ViewBag.TodayWordToReadRow = detail;

            ViewBag.Toast = TempData["Toast"]?.ToString() ?? "";
            ViewBag.YearTotalPoint = YearTotalPoint;
            return View("~/Views/DailyMission4/Word.cshtml", vm);
        }

        public async Task<IActionResult> NaviMonth(string UserName)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                UserName = HttpContext.Session.GetString(HttpContextSessionCategory.UserName.ToString()) as string;
            }

            if (await _authService.IsSignedIn(User))
            {
                await SetUserInfo();
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
                await SetUserInfo();
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

        // ============================================================
        // GET: /DailyMission2/Quiz
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Quiz(string date, string lang = "", int YearTotalPoint = 0)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var vm = await BuildDashboardVM(this.UserName, date);
            var userLang = await ResolveLangAsync(lang);

            var q = await _iDailyMission2Service.GetTodayQuestionAsync(vm.SelectedDate);
            ViewBag.TodayQuestionRow = q;

            ViewBag.QuizCorrect = vm.DayStatus?.QuizCorrect ?? false;
            ViewBag.QuizTryCnt = vm.DayStatus?.QuizTryCntMax ?? 0;

            // 퀴즈를 끝낸 뒤에도 "내가 고른 자리"를 보여 주려면
            // TempData 가 아니라 실제 기록(ClickHistoryForQuestion)을 읽어야 한다.
            int _qid = q?.QuestionId ?? 0;
            ViewBag.QuizMyAnswers = (_qid > 0)
                ? await _iDailyMission2Service.GetQuizMyAnswersAsync(this.UserName, vm.SelectedDate, _qid)
                : new List<string>();

            var detail = await _iDailyMission2Service.GetTodayWordDetailAsync(vm.SelectedDate, userLang, this.UserId);
            ViewBag.TodayWordToReadRow = detail;

            ViewBag.Toast = TempData["Toast"]?.ToString() ?? "";
            ViewBag.LastAnswerQ = TempData["DM2_LAST_ANSWER"]?.ToString();
            ViewBag.LastAnswerQid = TempData["DM2_LAST_QID"];
            ViewBag.YearTotalPoint = YearTotalPoint;
            return View("~/Views/DailyMission4/Quiz.cshtml", vm);
        }

        // ============================================================
        // POST: /DailyMission2/AnswerQuestion
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnswerQuestion(string date, int questionId, string answerQ, string lang = "")
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            lang = await ResolveLangAsync(lang);

            var selected = ParseDateOrToday(date);

            if (!IsActionableDate(selected))
            {
                TempData["DM2_OK_POPUP"] = "이 날짜는 수행할 수 없어요.\n(오늘/어제/그저께만 가능)";
                return RedirectToAction(nameof(Quiz), new { date = selected.ToString("yyyy-MM-dd"), lang });
            }

            if (questionId <= 0)
            {
                TempData["DM2_OK_POPUP"] = "오늘의 퀴즈가 없습니다.";
                return RedirectToAction(nameof(Quiz), new { date = selected.ToString("yyyy-MM-dd"), lang });
            }

            var before = await _iDailyMission2Service.GetDayStatusAsync(this.UserName, selected);

            var res = await _iDailyMission2Service.SaveQuizAsync(this.UserName, selected, questionId, answerQ);

            var after = await _iDailyMission2Service.GetDayStatusAsync(this.UserName, selected);

            bool isCorrect = after?.QuizCorrect ?? false;
            int tryCnt = after?.QuizTryCntMax ?? 0;

            var yearPoint = await _iDailyMission2Service.GetYearPointAsync(this.UserName, selected.Year);

            int added = (isCorrect && (before?.QuizCorrect ?? false) == false) ? 1 : 0;

            string msg;
            if (!res.Ok)
            {
                msg = res.Message ?? "처리할 수 없어요.";
            }
            else
            {
                if (isCorrect)
                {
                    msg = $"정답! {addedMessage}";
                }
                else if (tryCnt >= 2)
                {
                    msg = "오답이에요 🙂\n오늘은 2번 도전했어요.\n내일 새로운 퀴즈에 도전해 보세요!";
                }
                else
                {
                    msg = $"오답이에요. ({tryCnt}/2)\n한 번 더 도전해볼까요?";
                }
            }

            TempData["DM2_OK_POPUP"] = msg;
            TempData["DM2_LAST_ANSWER"] = answerQ;
            TempData["DM2_LAST_QID"] = questionId;

            // ✅ AWARD: (IAwardDashboardService 내부) 포인트 도달 처리 + 팝업 결과 리턴
            // ※ 메서드명은 IAwardDashboardService에 추가하신 이름과 동일해야 합니다.
            var popup = await _awardService.HandleAwardIfReachedAsync(this.UserName, selected.Year, yearPoint.YearTotalPoint, added);
            ApplyAwardPopupResult(popup);

            return RedirectToAction(nameof(Quiz), new { date = selected.ToString("yyyy-MM-dd"), lang = "" , YearTotalPoint = yearPoint.YearTotalPoint });
        }

        // ============================================================
        // GET: /DailyMission2/Pray
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Pray(string date, string lang = "", int YearTotalPoint = 0)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            var vm = await BuildDashboardVM(this.UserName, date);
            var userLang = await ResolveLangAsync(lang);

            try
            {
                var prayDetail = SharedComm.GetTodayCollectPrayPhrase(vm.SelectedDate, userLang, this.UserId);
                ViewBag.TodayPrayPhraseRow = prayDetail;
                ViewBag.TodayWordToReadRow = prayDetail.TodayeBiblesToPrayData;
            }
            catch { }

            ViewBag.Toast = TempData["Toast"]?.ToString() ?? "";
            ViewBag.YearTotalPoint = YearTotalPoint;
            return View("~/Views/DailyMission4/Pray.cshtml", vm);
        }

        // ============================================================
        // POST: /DailyMission2/SavePray
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePray(
            string date,
            int prayPhraseId,
            bool toMe = false,
            bool toParent = false,
            bool toFriend = false,
            string lang = ""
        )
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            lang = await ResolveLangAsync(lang);

            string userName = User?.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(userName))
                return RedirectToAction("Login", "Account");

            var selected = ParseDateOrToday(date);

            if (!IsActionableDate(selected))
            {
                TempData["DM2_OK_POPUP"] = "이 날짜는 수행할 수 없어요.\n(오늘/어제/그저께만 가능)";
                return RedirectToAction(nameof(Pray), new { date = selected.ToString("yyyy-MM-dd"), lang });
            }

            if (prayPhraseId <= 0)
            {
                TempData["DM2_OK_POPUP"] = "오늘의 기도문이 없습니다.";
                return RedirectToAction(nameof(Pray), new { date = selected.ToString("yyyy-MM-dd"), lang });
            }

            var before = await _iDailyMission2Service.GetDayStatusAsync(userName, selected);

            bool finalToMe = (toMe || (before?.Pray_ToMe ?? false));
            bool finalToParent = (toParent || (before?.Pray_ToParent ?? false));
            bool finalToFriend = (toFriend || (before?.Pray_ToFriend ?? false));

            var res = await _iDailyMission2Service.SavePrayAsync(
                userName, selected, prayPhraseId,
                finalToMe, finalToParent, finalToFriend
            );

            TempData["Toast"] = res.Message;

            if (!res.Ok)
            {
                TempData["DM2_OK_POPUP"] = string.IsNullOrWhiteSpace(res.Message) ? "저장에 실패했어요." : res.Message;
                return RedirectToAction(nameof(Pray), new { date = selected.ToString("yyyy-MM-dd"), lang });
            }

            var yearPoint = await _iDailyMission2Service.GetYearPointAsync(userName, selected.Year);

            int added =
                (toMe && (before?.Pray_ToMe ?? false) == false ? 1 : 0) +
                (toParent && (before?.Pray_ToParent ?? false) == false ? 1 : 0) +
                (toFriend && (before?.Pray_ToFriend ?? false) == false ? 1 : 0);

            string actionLabel =
                toMe ? "나를 위해 기도 했어요" :
                toParent ? "부모님이나 친구를 위해서 기도 했어요" :
                toFriend ? "친구를 위해 기도 했어요" :
                "기도 했어요";

            TempData["DM2_OK_POPUP"] =
                $"🎉 {actionLabel} 스티커 완료! (+{added})\n" +
                $"⭐ 올해 총 {yearPoint.YearTotalPoint}점 모았어요!";

            // ✅ AWARD: 포인트 도달 처리
            var popup = await _awardService.HandleAwardIfReachedAsync(userName, selected.Year, yearPoint.YearTotalPoint, added);
            ApplyAwardPopupResult(popup);

            return RedirectToAction(nameof(Pray), new { date = selected.ToString("yyyy-MM-dd"), lang = "", YearTotalPoint = yearPoint.YearTotalPoint });
        }

        // ============================================================
        // GET: /DailyMission2/Recite
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Recite(string date, string lang = "", int YearTotalPoint = 0)
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            // 암송 구절은 한글로만 보여 줍니다. 주소에 ?lang=en 이 붙어도 무시합니다.
            // ⚠️ ResolveLangAsync 는 ?lang= 값을 사용자 설정에 저장하므로 여기서는 쓰지 않습니다.
            //    (썼다면 이 화면을 지나는 것만으로 다른 화면의 언어까지 바뀝니다)
            lang = "ko";
            ViewBag.Lang = lang;

            var vm = await BuildDashboardVM(this.UserName, date);
            ViewBag.Toast = TempData["Toast"]?.ToString() ?? "";

            var weeklyDetail = await _iDailyMission2Service.GetWeeklyWordDetailAsync(vm.SelectedDate, lang, this.UserId, vm.SelectedDate.Year);
            ViewBag.WeeklyWordDetail = weeklyDetail;
            ViewBag.WeeklyWordToReadData = weeklyDetail?.WeeklyWordToReadData;
            ViewBag.WeekeBiblesDataToRecite = weeklyDetail?.WeekeBiblesDataToRecite;
            ViewBag.YearTotalPoint = YearTotalPoint;
            return View("~/Views/DailyMission4/Recite.cshtml", vm);
        }

        // ============================================================
        // GET: /DailyMission2/BibleRead
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> BibleRead(string date, string lang = "")
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            lang = await ResolveLangAsync(lang);

            var vm = await BuildDashboardVM(this.UserName, date);

            // 인-페이지 언어 전환을 위해 항상 양언어(al) 조회, lang은 초기 표시 용도
            var detail = await _iDailyMission2Service.GetTodayWordDetailAsync(vm.SelectedDate, "al", this.UserId);
            ViewBag.TodayWordToReadRow = detail;

            var wordData = await _iDailyMission2Service.GetTodayWordAsync(0, true, vm.SelectedDate);

            if (wordData.Count > 0)
            {
                ViewBag.TodayWordToReadData     = wordData[0];
                ViewBag.TodayWordToReadDataList = wordData;   // ✅ 다중 장 순차 재생을 위해 전체 목록 전달

                var wordDataSummary = await _iDailyMission2Service.GetTodayBibleSummaryAsync(wordData[0].BibleText);

                if (wordDataSummary._BibleBasicInfos2 != null)
                    ViewBag._BibleBasicInfos2 = wordDataSummary._BibleBasicInfos2;

                if (wordDataSummary._BibleBookOverviewDetail != null)
                    ViewBag._BibleBookOverviewDetail = wordDataSummary._BibleBookOverviewDetail;
            }

            // 현대인성경 (eBibleHD) — 본문 범위에 해당하는 절 Id → 본문 Dictionary
            if (detail != null && !string.IsNullOrWhiteSpace(detail.BibleText))
            {
                var bt  = detail.BibleText;
                var fj  = detail.FromJang;
                var fz  = detail.FromZul;
                var tj  = detail.ToJang;
                var tz  = detail.ToZul;
                try
                {
                    var modernMap = await _db.EBibleHDs
                        .Where(e => e.BibleText == bt &&
                            ((e.Jang > fj && e.Jang < tj) ||
                             (e.Jang == fj && e.Jang == tj && e.Zul >= fz && e.Zul <= tz) ||
                             (e.Jang == fj && fj != tj && e.Zul >= fz) ||
                             (e.Jang == tj && fj != tj && e.Zul <= tz)))
                        .ToDictionaryAsync(e => e.Id, e => e.BibleData ?? "");
                    ViewBag.ModernMap = modernMap;
                }
                catch { ViewBag.ModernMap = new System.Collections.Generic.Dictionary<int, string>(); }
            }
            else
            {
                ViewBag.ModernMap = new System.Collections.Generic.Dictionary<int, string>();
            }

            // 성경 전체 통독률
            var manageReadBible = new ManageReadBible();
            ViewBag.TotalProgressRate    = !string.IsNullOrWhiteSpace(this.UserId)
                ? manageReadBible.GetTotalBibleProgressRate(this.UserId)
                : (decimal)0;
            ViewBag.TodayReadVerseCount  = !string.IsNullOrWhiteSpace(this.UserId)
                ? manageReadBible.GetTodayReadVerseCount(this.UserId)
                : 0;

            ViewBag.Toast = TempData["Toast"]?.ToString() ?? "";
            return View("~/Views/DailyMission4/BibleRead.cshtml", vm);
        }

        // ============================================================
        // GET: /DailyMission4/BibleReadRange
        //   날짜가 아니라 구절 범위를 받아 BibleRead 화면으로 펼친다.
        //   예) ?BibleText=로마서&FromJang=8&FromZul=18&ToJang=8&ToZul=22&from=weekly&returnId=35
        //   · 점수와 무관한 읽기 — 화면은 ViewBag.RangeMode 로 날짜 이동·말씀 점수 버튼을 숨긴다
        //   · 잘못된 범위면 오늘 말씀(BibleRead)으로 보낸다
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> BibleReadRange(string BibleText, int FromJang, int FromZul, int ToJang, int ToZul,
                                                        string from = "", int returnId = 0, string label = "", string lang = "")
        {
            if (await _authService.IsSignedIn(User))
                await SetUserInfo();

            lang = await ResolveLangAsync(lang);

            var detail = await _iDailyMission2Service.GetWordDetailByRangeAsync(
                BibleText, FromJang, FromZul, ToJang, ToZul, "al", this.UserId);

            if (detail == null)
            {
                TempData["Toast"] = "성경 구절 정보가 올바르지 않아 오늘 말씀으로 이동했어요";
                return RedirectToAction(nameof(BibleRead));
            }

            // 화면 공통 모델 (오늘 날짜 기준 — 네비·상단바용)
            var vm = await BuildDashboardVM(this.UserName, DateHelper.GetKoreaNow().ToString("yyyy-MM-dd"));

            ViewBag.TodayWordToReadRow = detail;
            ViewBag.RangeMode          = true;
            ViewBag.RangeFrom          = from ?? "";
            ViewBag.RangeLabel         = string.IsNullOrWhiteSpace(label) ? "" : label.Trim();
            ViewBag.RangeReturnUrl     = (string.Equals(from, "weekly", StringComparison.OrdinalIgnoreCase) && returnId > 0)
                                         ? $"/WeeklyReport/IndexByID?Id={returnId}"
                                         : "/DailyMission4/Index";
            ViewBag.RangeReturnText    = (string.Equals(from, "weekly", StringComparison.OrdinalIgnoreCase) && returnId > 0)
                                         ? "주일공과로" : "미션홈으로";

            // 성경 요약 (책 소개)
            try
            {
                var summary = await _iDailyMission2Service.GetTodayBibleSummaryAsync(detail.BibleText);
                if (summary?._BibleBasicInfos2 != null)        ViewBag._BibleBasicInfos2        = summary._BibleBasicInfos2;
                if (summary?._BibleBookOverviewDetail != null) ViewBag._BibleBookOverviewDetail = summary._BibleBookOverviewDetail;
            }
            catch { /* 요약이 없어도 본문은 보인다 */ }

            // 현대인성경 대조 — 절을 0 으로 넘기면 그 장 전체
            {
                var bt = detail.BibleText;
                var fj = detail.FromJang;
                var fz = detail.FromZul <= 0 ? 1   : detail.FromZul;
                var tj = detail.ToJang;
                var tz = detail.ToZul   <= 0 ? 999 : detail.ToZul;
                try
                {
                    var modernMap = await _db.EBibleHDs
                        .Where(e => e.BibleText == bt &&
                            ((e.Jang > fj && e.Jang < tj) ||
                             (e.Jang == fj && e.Jang == tj && e.Zul >= fz && e.Zul <= tz) ||
                             (e.Jang == fj && fj != tj && e.Zul >= fz) ||
                             (e.Jang == tj && fj != tj && e.Zul <= tz)))
                        .ToDictionaryAsync(e => e.Id, e => e.BibleData ?? "");
                    ViewBag.ModernMap = modernMap;
                }
                catch { ViewBag.ModernMap = new System.Collections.Generic.Dictionary<int, string>(); }
            }

            // 성경 전체 통독률
            var manageReadBible = new ManageReadBible();
            ViewBag.TotalProgressRate   = !string.IsNullOrWhiteSpace(this.UserId)
                ? manageReadBible.GetTotalBibleProgressRate(this.UserId)
                : (decimal)0;
            ViewBag.TodayReadVerseCount = !string.IsNullOrWhiteSpace(this.UserId)
                ? manageReadBible.GetTodayReadVerseCount(this.UserId)
                : 0;

            ViewBag.Toast = TempData["Toast"]?.ToString() ?? "";
            return View("~/Views/DailyMission4/BibleRead.cshtml", vm);
        }

        // ============================================================
        // POST: /DailyMission2/MarkVerseRead
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkVerseRead(string date, string lang, string bibleText, int jang, int zul, int Id)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();

            lang = await ResolveLangAsync(lang);

            var selected = ParseDateOrToday(date);

            var res = await _iDailyMission2Service.MarkVerseReadAsync(this.UserId, selected, lang, bibleText, jang, zul, Id);

            bool? isRead = TryGetBool(res, "IsRead");

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    ok = res.Ok,
                    message = res.Message,
                    verse = new { jang, zul, id = Id },
                    isRead = isRead ?? true
                });
            }

            TempData["Toast"] = res.Message;
            return RedirectToAction(nameof(BibleRead), new { date = selected.ToString("yyyy-MM-dd"), lang });
        }

        // ============================================================
        // POST: /DailyMission2/MarkVerseUnread
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkVerseUnread(string date, string lang, string bibleText, int jang, int zul, int Id)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();

            lang = await ResolveLangAsync(lang);

            var selected = ParseDateOrToday(date);

            var res = await _iDailyMission2Service.MarkVerseUnreadAsync(this.UserId, selected, lang, bibleText, jang, zul, Id);

            bool? isRead = TryGetBool(res, "IsRead");

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    ok = res.Ok,
                    message = res.Message,
                    verse = new { jang, zul, id = Id },
                    isRead = isRead ?? false
                });
            }

            TempData["Toast"] = res.Message;
            return RedirectToAction(nameof(BibleRead), new { date = selected.ToString("yyyy-MM-dd"), lang });
        }

        // ============================================================
        // POST: /DailyMission2/ToggleBookmark
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBookmark(string date, string lang, string bibleText, int jang, int zul, int Id)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();

            lang = await ResolveLangAsync(lang);

            var selected = ParseDateOrToday(date);

            var res = await _iDailyMission2Service.ToggleBookmarkAsync(this.UserId, selected, lang, bibleText, jang, zul, Id);

            bool? isBookmarked = TryGetBool(res, "IsBookmarked");

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    ok = res.Ok,
                    message = res.Message,
                    verse = new { jang, zul, id = Id },
                    isBookmarked = isBookmarked ?? true
                });
            }

            TempData["Toast"] = res.Message;
            return RedirectToAction(nameof(BibleRead), new { date = selected.ToString("yyyy-MM-dd"), lang });
        }

        // ============================================================
        // POST: /DailyMission2/ToggleBibleRange  (AJAX 전체 읽음↔취소)
        // ============================================================
        public class ToggleBibleRangeReq
        {
            public string BibleText { get; set; } = "";
            public int FromJang { get; set; }
            public int FromZul  { get; set; }
            public int ToJang   { get; set; }
            public int ToZul    { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBibleRange([FromBody] ToggleBibleRangeReq req)
        {
            if (!await _authService.IsSignedIn(User)) return Unauthorized();
            await SetUserInfo();

            var (isRead, readVerses, totalVerses) =
                await _iDailyMission2Service.ToggleBibleRangeAsync(
                    this.UserId, req?.BibleText ?? "", req?.FromJang ?? 0, req?.FromZul ?? 0,
                    req?.ToJang ?? 0, req?.ToZul ?? 0);

            return Json(new { ok = true, isRead, readVerses, totalVerses });
        }

        // ============================================================
        // GET: /DailyMission2/GetBibleProgressJson  (AJAX 통독률 갱신)
        // ============================================================
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GetBibleProgressJson()
        {
            if (string.IsNullOrWhiteSpace(this.UserId))
                return Json(new { ok = false });

            var manageReadBible = new ManageReadBible();
            decimal progressRate    = manageReadBible.GetTotalBibleProgressRate(this.UserId);
            int     todayReadVerses = manageReadBible.GetTodayReadVerseCount(this.UserId);

            return Json(new { ok = true, progressRate, todayReadVerseCount = todayReadVerses });
        }

        // ============================================================
        // POST: /DailyMission2/MarkPageRead
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPageRead(string date, string lang, string bibleText, int fromJang, int fromZul, int toJang, int toZul)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();

            lang = await ResolveLangAsync(lang);

            var selected = ParseDateOrToday(date);

            var res = await _iDailyMission2Service.MarkPageReadAsync(this.UserId, selected, lang, bibleText, fromJang, fromZul, toJang, toZul);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    ok = res.Ok,
                    message = res.Message
                });
            }

            TempData["Toast"] = res.Message;
            return RedirectToAction(nameof(BibleRead), new { date = selected.ToString("yyyy-MM-dd"), lang });
        }

        // ============================================================
        // POST: /DailyMission2/SaveWord
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWord(string date, int todayWordToReadId, bool toMe, bool toParent, string lang = "")
        {
            lang = await ResolveLangAsync(lang);

            var backAction = nameof(Word);

            if (await _authService.IsSignedIn(User)) await SetUserInfo();

            var selected = ParseDateOrToday(date);

            if (!IsActionableDate(selected))
            {
                TempData["Toast"] = "이 날짜는 수행할 수 없어요. (오늘/어제/그저께만 가능)";
                return RedirectToAction(backAction, new { date = selected.ToString("yyyy-MM-dd"), lang });
            }

            var before = await _iDailyMission2Service.GetDayStatusAsync(this.UserName, selected);

            var res = await _iDailyMission2Service.SaveWordAsync(this.UserName, selected, todayWordToReadId, toMe, toParent);
            TempData["Toast"] = res.Message;
            int YearTotalPoint = 0;
            if (res.Ok)
            {
                var yearPoint = await _iDailyMission2Service.GetYearPointAsync(this.UserName, selected.Year);
                
                YearTotalPoint = yearPoint.YearTotalPoint;

                int added =
                    (toMe && (before?.Word_ToMe ?? false) == false ? 1 : 0) +
                    (toParent && (before?.Word_ToParent ?? false) == false ? 1 : 0);

                TempData["DM2_OK_POPUP"] =
                    $"말씀 미션 수행완료했요. (+{added})\n" +
                    addedMessage; 

                // ✅ AWARD: 포인트 도달 처리
                var popup = await _awardService.HandleAwardIfReachedAsync(this.UserName, selected.Year, yearPoint.YearTotalPoint, added);
                ApplyAwardPopupResult(popup);
            }
            return RedirectToAction(backAction, new { date = selected.ToString("yyyy-MM-dd"), lang = "", YearTotalPoint = YearTotalPoint });
        }

        // ============================================================
        // POST: /DailyMission2/SaveRecite
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRecite(string date, int weeklyWordToReadId, string lang = "ko")
        {
            lang = await ResolveLangAsync(lang);

            if (!DateTime.TryParse(date, out var selectedDate))
            {
                TempData["DM2_OK_POPUP"] = "날짜 형식이 올바르지 않아요.";
                return RedirectToAction(nameof(Recite), new { date, lang });
            }

            if (!IsActionableDate(selectedDate))
            {
                TempData["DM2_OK_POPUP"] = "이 날짜는 수행할 수 없어요.\n(오늘/어제/그저께만 가능)";
                return RedirectToAction(nameof(Recite), new { date = selectedDate.ToString("yyyy-MM-dd"), lang });
            }

            var res = await _iDailyMission2Service.SaveReciteAsync(this.UserName, selectedDate, weeklyWordToReadId);
            int YearTotalPoint = 0;
            if (res != null && res.Ok)
            {
                var yearPoint = await _iDailyMission2Service.GetYearPointAsync(this.UserName, selectedDate.Year);
                
                YearTotalPoint = yearPoint.YearTotalPoint;

                TempData["DM2_OK_POPUP"] =
                    $"암송 완료! +1점 🎉\n" +
                    addedMessage;

                // ✅ AWARD: 암송은 +1
                var popup = await _awardService.HandleAwardIfReachedAsync(this.UserName, selectedDate.Year, yearPoint.YearTotalPoint, 1);
                ApplyAwardPopupResult(popup);
            }
            else
            {
                var msg = res?.Message;
                TempData["DM2_OK_POPUP"] = string.IsNullOrWhiteSpace(msg)
                    ? "저장에 실패했어요. 잠시 후 다시 시도해 주세요."
                    : msg;
            }

            return RedirectToAction(nameof(Recite), new { date = selectedDate.ToString("yyyy-MM-dd"), lang = "", YearTotalPoint = YearTotalPoint });
        }

        // ============================================================
        // ✅ 공통: Ajax 요청인지 판별
        // ============================================================
        private bool IsAjaxRequest()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                   || (Request.Headers["Accept"].ToString()?.Contains("application/json") ?? false);
        }

        // ============================================================
        // 내부 헬퍼
        // ============================================================
        private async Task<DailyMission2DashboardVM> BuildDashboardVM(string userName, string date)
        {
            var today = DateHelper.GetKoreaNow().Date;
            var selected = ParseDateOrToday(date);

            var vm = new DailyMission2DashboardVM
            {
                UserName = userName,
                Today = today,
                SelectedDate = selected,
                IsToday = selected == today,
                IsOtherDate = selected != today,
                IsActionable = IsActionableDate(selected),
                NoticeMessage = (selected != today) ? "📌 다른 날짜 미션을 보고 있습니다" : ""
            };

            vm.Content = await _iDailyMission2Service.GetContentIdsAsync(selected);
            vm.DayStatus = await _iDailyMission2Service.GetDayStatusAsync(userName, selected);
            vm.YearPoint = await _iDailyMission2Service.GetYearPointAsync(userName, selected.Year);
            vm.CalendarAllDoneMap = await _iDailyMission2Service.GetCalendarAllDoneMapAsync(userName, selected);
            // vm.CalendarDayPointMap = await _iDailyMission2Service.GetCalendarDayPointMapAsync(userName, selected);
            vm.CalendarDayPointMap = await BuildCalendarDayPointMapByUserDailyPointsAsync(userName, selected);


            return vm;
        }

        // ============================================================
        // ✅ 월별 일자 포인트 맵 생성 (UserMonthlyPoints 로직 기반)
        // - key: "yyyy-MM-dd"
        // - value: TotalPoint (Word+Quiz+Pray+Recite 합계)
        // ============================================================
        private async Task<Dictionary<string, int>> BuildCalendarDayPointMapByUserDailyPointsAsync(string userName, DateTime selected)
        {
            var now = DateHelper.GetKoreaNow().Date;

            int year = selected.Year;
            int month = selected.Month;

            var monthFirst = new DateTime(year, month, 1);
            var endOfMonth = monthFirst.AddMonths(1).AddDays(-1);
            // ✅ 2026-10-03 : "최근 7일" 막대가 지난달 끝(예: 10/3 이면 9/27~9/30)까지 걸치므로,
            //    선택일 6일 전부터 읽는다.  달력은 다른 달 칸에 점수를 쓰지 않으므로 영향 없음.
            var fromDate = selected.Date.AddDays(-6) < monthFirst ? selected.Date.AddDays(-6) : monthFirst;

            // ✅ 현재 월이면 "오늘"까지만 (UserMonthlyPoints와 동일 정책)
            var toDate = (year == now.Year && month == now.Month) ? now : endOfMonth;

            if (toDate < monthFirst) toDate = monthFirst;

            // ✅ 이게 "정답 데이터" (UserMonthlyPoints에서 쓰는 것)
            var rows = await _iDailyMission2Service.GetUserDailyPointsAsync(userName, fromDate, toDate);

            // ✅ rows(List<UserDailyPointRow>) -> CalendarDayPointMap(Dictionary<string,int>) 변환
            //    중복 날짜가 올 가능성은 낮지만, 방어로 합산 처리.
            var map = new Dictionary<string, int>(StringComparer.Ordinal);

            if (rows != null)
            {
                foreach (var r in rows)
                {
                    var key = r.DateBy.Date.ToString("yyyy-MM-dd");
                    if (map.ContainsKey(key))
                        map[key] += r.TotalPoint;
                    else
                        map[key] = r.TotalPoint;
                }
            }

            return map;
        }

        // GET: /DailyMission2/UserMonthlyPoints?userName=aaa&year=2026&month=4
        public async Task<IActionResult> UserMonthlyPoints(string userName, int year = 0, int month = 0)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();

            // 기본값 = 현재 년/월
            var now = DateHelper.GetKoreaNow();
            if (year <= 0) year = now.Year;
            if (month <= 0) month = now.Month;
            if (month < 1) month = 1;
            if (month > 12) month = 12;

            var fromDate = new DateTime(year, month, 1);
            var endOfMonth = fromDate.AddMonths(1).AddDays(-1);

            // 현재 월이면 "오늘"까지만 보여주기(원하면 이 줄 제거하면 월말까지 다 나옵니다)
            var toDate = (year == now.Year && month == now.Month) ? now.Date : endOfMonth;

            // 방어
            if (toDate < fromDate) toDate = fromDate;

            var rows = await _iDailyMission2Service.GetUserDailyPointsAsync(userName, fromDate, toDate);

            var vm = new UserDailyPointsVM
            {
                IsAdmin = false,
                UserName = userName,
                FromDate = fromDate,
                ToDate = toDate,
                Rows = rows
            };

            return View("~/Views/DailyMission4/UserMonthlyPoints.cshtml", vm);
        }


        // GET: /DailyMission2/UserDailyPoints?userName=aaa&from=2026-01-01&to=2026-02-03
        public async Task<IActionResult> UserDailyPoints(string userName)
        {
            if (await _authService.IsSignedIn(User)) await SetUserInfo();

            DateTime fromDate = ParseYmdOrDefault("", new DateTime(DateHelper.GetKoreaNow().Year, DateHelper.GetKoreaNow().Month, 1));
            DateTime toDate = ParseYmdOrDefault("", DateHelper.GetKoreaNow());

            if (toDate < fromDate)
            {
                // 뒤집혔으면 스왑
                var tmp = fromDate;
                fromDate = toDate;
                toDate = tmp;
            }

            var rows = await _iDailyMission2Service.GetUserDailyPointsAsync(userName, fromDate, toDate);

            var vm = new UserDailyPointsVM
            {
                IsAdmin = false,
                UserName = userName,
                FromDate = fromDate,
                ToDate = toDate,
                Rows = rows
            };

            return View("~/Views/DailyMission4/UserDailyPoints.cshtml", vm);
        }

        private static DateTime ParseYmdOrDefault(string s, DateTime def)
        {
            if (DateTime.TryParseExact(s ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d.Date;
            return def.Date;
        }

        private static DateTime ParseDateOrToday(string date)
        {
            if (DateTime.TryParseExact(date ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d.Date;

            return DateHelper.GetKoreaNow().Date;
        }

        private static bool IsActionableDate(DateTime selected)
        {
            var today = DateHelper.GetKoreaNow().Date;
            var diff = (today - selected.Date).TotalDays;
            return diff >= 0 && diff <= 2; // D, D-1, D-2
        }
        [HttpGet]
        public async Task<IActionResult> DeleteClickHistory()
        {
            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            ManageeCommon manageeCommon = new ManageeCommon();
            manageeCommon.DeleteClickHistory(UserName);
            return RedirectToAction(nameof(DailyMission4Controller.Index), "DailyMission4");
        }
        // ============================================================
        // ✅ SaveResult에서 bool 상태값(IsRead/IsBookmarked)을 꺼내오는 헬퍼
        // ============================================================
        private static bool? TryGetBool(object res, string propName)
        {
            try
            {
                var p = res.GetType().GetProperty(propName);
                if (p == null) return null;

                var v = p.GetValue(res);
                if (v == null) return null;

                if (v is bool b) return b;

                var s = v.ToString()?.Trim().ToLowerInvariant();
                if (s == "true" || s == "t" || s == "y" || s == "1") return true;
                if (s == "false" || s == "f" || s == "n" || s == "0") return false;

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}

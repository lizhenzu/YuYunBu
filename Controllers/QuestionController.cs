using BulletinBoard.BusinessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BulletinBoard.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using BulletinBoard.Helpers;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    [Authorize(Roles = nameof(Roles.Admin))]
    public class QuestionController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        public QuestionController(IAuthService authService,
            UserManager<ApplicationUser> userManager) : base(authService, userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        //// GET: JobCategory
        public async Task<IActionResult> Index11()
        {
            string userName = string.Empty;

            if (await _authService.IsSignedIn(User))
            {
                userName = await _authService.GetUserName(User);
            }

            return View();
        }
        public ActionResult Index(DateTime? selectedDate, string? KindData)
        {
            DateTime workDT = DateHelper.GetKoreaNow();
            ManageQuestion manageQuestion = new ManageQuestion();
            ManageeCommon manageeCommon = new ManageeCommon();

            ViewBag.KindData = KindData;
            if (selectedDate == null)
            {               
                List<UserSettings> va = manageeCommon.fetchUserSettings(UserId);
                if (va != null)
                {
                    if (va.Count > 0)
                    {
                        workDT = va[0].QuestionWorkDate;
                    }
                    else
                    {
                        workDT = SharedComm.GetToday();
                    }
                }
            }
            else
            {
                workDT = (DateTime)selectedDate;
            }

            manageeCommon.UpsertUserSettings(UserId, questionWorkDate: workDT);

            List<QuestionTypeDataObjects> data = manageQuestion.getQuestionObject(0, false, "O", true, false, workDT, KindData);

            List<Event> Events = new List<Event>();     
            
            foreach (var va in data)
            {
                Events.Add(new Event { Id = va.DayData, Date = va.ExposeDate, Title = $"{va.ExposeDate.ToString("yyyy-MM-dd")}({va.WeekDayKor})", Description = va.QuestionQ,  QuestionData = va });
            }

            var model = new CalendarViewModel
            {
                CurrentDate = workDT,
                Events = Events
            };

            ViewBag.PrevDateTime = workDT.AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.NowDateTime = SharedComm.GetTodayString();
            ViewBag.NextDateTime = workDT.AddMonths(1).ToString("yyyy-MM-dd");
            return View(model);
        }

        public void SetLangCDMethod(string UserId)
        {
            ManageeCommon manageeCommon = new ManageeCommon();

            List<UserSettings> va = manageeCommon.fetchUserSettings(UserId);
            if (va != null)
            {
                if (va.Count > 0)
                {
                    if(string.IsNullOrEmpty(va[0].LangCD))
                    {
                        va[0].LangCD = "ko";
					}
                    HttpContext.Session.SetString(HttpContextSessionCategory.LangCD.ToString(), va[0].LangCD);
                    HttpContext.Session.SetString(HttpContextSessionCategory.PaesForView.ToString(), va[0].PagesForView.ToString());
                    HttpContext.Session.SetString(HttpContextSessionCategory.Notice_1_Collapse.ToString(), va[0].Notice_1_Collapse.ToString());
                }
            }
        }
        public JsonResult GetEvents(DateTime date)
        {
            ManageQuestion manageQuestion = new ManageQuestion();
            List<QuestionTypeDataObjects> data = manageQuestion.getQuestionObject(0, false, "O", true, false, date, string.Empty);

            List<Event> Events = new List<Event>();

            foreach (var va in data)
            {
                Events.Add(new Event { Id = va.DayData, Date = va.ExposeDate, Title = va.WeekDayKor, Description = va.QuestionQ , QuestionData = va });
            }


            var events = Events.Where(e => e.Date.Date == date.Date).ToList();
            return Json(events);
        }



        public async Task<IActionResult> createNewQuestion()
        {
            return View();
        }
        public async Task<IActionResult> BranchQuestionType(string WhenDate)
        {
            ViewBag.WhenDate = WhenDate;
            return View();
        }
        public async Task<IActionResult> BranchQuestionMethod(string WhenDate, string KindData)
        {
            ViewBag.WhenDate = WhenDate;
            ViewBag.KindData = KindData;
            return View();
        }

        public async Task<IActionResult> FromTemplate(string WhenDate)
        {
            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();

            if (false)
            {

                SetLangCDMethod(this.UserId);



                ManageTodayWordToRead manageTodayWordToRead = new ManageTodayWordToRead();
                List<TodayWordToReads> TodayWordToReads = manageTodayWordToRead.getTodayWordToRead(0, true, DateTime.Parse(WhenDate));

                ViewBag.TodayWordToReads = TodayWordToReads[0];
                ViewBag.DisableLangTransBtton = "y";  // 언어변화 버튼 안보이게 
                ManageeBible manageeBible = new ManageeBible();

                var UserId = HttpContext.Session.GetString(HttpContextSessionCategory.UserId.ToString()) as string;

                eBibles _eBibles = manageeBible.geteBible2(TodayWordToReads[0].BibleText, TodayWordToReads[0].FromJang, TodayWordToReads[0].FromZul, TodayWordToReads[0].ToJang, TodayWordToReads[0].ToZul, string.Empty, UserId);

                ViewBag._eBibles = _eBibles;
                ViewBag.WhenDate = WhenDate;
            }
            NewMethod(WhenDate); 

            List<QuestionTypeDataObjects> data = manageQuestionBank.getQuestionBank(0, WhenDate);

            return View(data);
        }

        public async Task<IActionResult> BranchQuestionType2(string QuestionType, DateTime? WhenDate)
        {
            if (QuestionType.ToLower() == "o")
            {
                QuestionTypeDataObject questionData = new QuestionTypeDataObject();
                if (WhenDate == null)
                {
                    questionData.ExposeDate = SharedComm.GetToday();
                }
                else
                {
                    questionData.ExposeDate = (DateTime)WhenDate;
                }
                questionData.CorrectAnswer = "1";
                return View("EnrollQuestionObject", questionData);
            }
            else
            {
                QuestionTypeData questionData = new QuestionTypeData();
                questionData.ExposeDate = SharedComm.GetToday();
                return View("EnrollQuestionSubject", questionData);
            }
        }

        public async Task<IActionResult> CreateQuizByBank(int Id, string WhenDate)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();

            List<QuestionTypeDataObjects> data = manageQuestionBank.getQuestionBank(Id, WhenDate);


            ViewBag.WhenDate = WhenDate;
            return View(data[0]);
        }

        public async Task<IActionResult> ModifyQuiz(int Id)
        {
            ManageQuestion manageQuestion = new ManageQuestion();

            List<QuestionTypeDataObjects> value = manageQuestion.getQuestionObject(Id, false, "o", false, false, SharedComm.GetToday(), string.Empty);

            ViewBag.WhenDate = value[0].ExposeDate.ToString("yyyy-MM-dd");

            return View(value[0]);
        }

        public async Task<IActionResult> ModifyQuizByDate(DateTime dt)
        {
            ManageQuestion manageQuestion = new ManageQuestion();

            List<QuestionTypeDataObjects> value = manageQuestion.getQuestionObject(0, true, "o", false, false, dt, string.Empty);

            ViewBag.WhenDate = value[0].ExposeDate.ToString("yyyy-MM-dd");

            return View("ModifyQuiz", value[0]);
        }

         

        public async Task<IActionResult> CreateQuizBlank(string QuestionType, string WhenDate)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();
            ViewBag.WhenDate = WhenDate;

            QuestionTypeDataObjects data = new QuestionTypeDataObjects();

            if (WhenDate == null)
            {
                data.ExposeDate = SharedComm.GetToday();
            }
            else
            {
                if (DateTime.TryParseExact(WhenDate, "yyyy-MM-dd",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime parsedDate))
                {
                    data.ExposeDate = parsedDate;
                }
            }

            data.QuestionType = QuestionType;


            HttpContext.Session.SetString(MVCInfomation.ActionSource.ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
            HttpContext.Session.SetString(MVCInfomation.ControllerSource.ToString(), this.ControllerContext.RouteData.Values["controller"].ToString());

            if (await _authService.IsSignedIn(User))
            {
                var result = await SetUserInfo();
            }

            NewMethod(WhenDate);



            //            ViewBag.WhenDate = data.ExposeDate.ToString("yyyy-MM-dd");
            return View("CreateQuizByBank", data);
        }

        private void NewMethod(string WhenDate)
        {
            SetLangCDMethod(this.UserId);


            ManageTodayWordToRead manageTodayWordToRead = new ManageTodayWordToRead();
            List<TodayWordToReads> TodayWordToReads = manageTodayWordToRead.getTodayWordToRead(0, true, DateTime.Parse(WhenDate));

            ViewBag.TodayWordToReads = TodayWordToReads[0];
            ViewBag.DisableLangTransBtton = "y";  // 언어변화 버튼 안보이게 
            ManageeBible manageeBible = new ManageeBible();

            var UserId = HttpContext.Session.GetString(HttpContextSessionCategory.UserId.ToString()) as string;

            eBibles _eBibles = manageeBible.geteBible2(TodayWordToReads[0].BibleText, TodayWordToReads[0].FromJang, TodayWordToReads[0].FromZul, TodayWordToReads[0].ToJang, TodayWordToReads[0].ToZul, string.Empty, UserId);

            ViewBag._eBibles = _eBibles;
            ViewBag.WhenDate = WhenDate;
        }

        public async Task<IActionResult> ModifyQuestionSubject(int Id)
        {
            ManageQuestion manageQuestion = new ManageQuestion();

            List<QuestionTypeDatas> value = manageQuestion.getQuestionSubject(Id, false, "S", false, false, SharedComm.GetToday(), string.Empty);
            QuestionTypeData questionData = new QuestionTypeData();
            questionData = value[0];
            return View(questionData);
        }


        public async Task<IActionResult> ModifyQuestionObject(int Id)
        {
            ManageQuestion manageQuestion = new ManageQuestion();

            List<QuestionTypeDataObjects> value = manageQuestion.getQuestionObject(Id, false, "O", false, false, SharedComm.GetToday(), string.Empty);
            QuestionTypeDataObject questionData = new QuestionTypeDataObject();
            questionData = value[0];
            return View(questionData);
        }


        [HttpPost]
        public async Task<IActionResult> ModifyQuestionProcess(QuestionTypeDataObjects questionTypeDataObject)
        {
            string userName = string.Empty;

            if (await _authService.IsSignedIn(User))
            {
                userName = await _authService.GetUserName(User);
            }

            ManageQuestion manageQuestion = new ManageQuestion();

            ModelState.Remove("AnswerQ");
            ModelState.Remove("QuestionTypeDesc");
            ModelState.Remove("UserName");
            ModelState.Remove("WeekDayKor");

            if (questionTypeDataObject.QuestionType == "s")
            {
                ModelState.Remove("Question1");
                ModelState.Remove("Question2");
                ModelState.Remove("Question3");
                ModelState.Remove("Question4");
            }

            if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
                return View("ModifyQuiz", questionTypeDataObject);
            }
            else
            {
                questionTypeDataObject.UserName = userName;

                manageQuestion.UpdateQuestionObject(questionTypeDataObject);
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> CreateQuestionProcess(QuestionTypeDataObjects questionTypeDataObject)
        {
			string userName = string.Empty;

			if (await _authService.IsSignedIn(User))
			{
				userName = await _authService.GetUserName(User);
			}

			ManageQuestion manageQuestion = new ManageQuestion();

            ModelState.Remove("AnswerQ"); 
            ModelState.Remove("QuestionTypeDesc");
            ModelState.Remove("UserName");
            ModelState.Remove("WeekDayKor");

            if(questionTypeDataObject.QuestionType == "s" || questionTypeDataObject.QuestionType == "a")
            {
                ModelState.Remove("Question1");
                ModelState.Remove("Question2");
                ModelState.Remove("Question3");
                ModelState.Remove("Question4");
            }

            if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시

                NewMethod(questionTypeDataObject.ExposeDate.ToString("yyyy-MM-dd"));
                return View("CreateQuizByBank", questionTypeDataObject);
            }
            else
            {
                questionTypeDataObject.UserName = userName;

				manageQuestion.CreateQuestionObject(questionTypeDataObject);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestionSubject(QuestionTypeData questionTypeData)
        {
            ManageQuestion manageQuestion = new ManageQuestion();
			string userName = string.Empty;

			if (await _authService.IsSignedIn(User))
			{
				userName = await _authService.GetUserName(User);
			}


			ModelState.Remove("AnswerQ");


            if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
                return View("EnrollQuestionSubject", questionTypeData);
            }
            else
            {
                questionTypeData.UserName = userName;

				manageQuestion.CreateQuestionSubject(questionTypeData);
            }
            return RedirectToAction("QuestionDetailList");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuestionSubject(QuestionTypeData questionTypeData)
        {
            ManageQuestion manageQuestion = new ManageQuestion();

            ModelState.Remove("AnswerQ");

            if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
                return View("ModifyWeeklyWordToRead", questionTypeData);
            }
            else
            {
                manageQuestion.UpdateQuestionSubject(questionTypeData);
            }
            return RedirectToAction("QuestionDetailList");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuestionObject(QuestionTypeDataObject questionTypeDataObject)
        {
            ManageQuestion manageQuestion = new ManageQuestion();

            ModelState.Remove("AnswerQ");

            if (!ModelState.IsValid)
            {
                // 유효성 검사에 실패한 경우, 같은 뷰를 다시 렌더링하여 오류 메시지를 표시
                return View("ModifyQuestionObject", questionTypeDataObject);
            }
            else
            {
                manageQuestion.UpdateQuestionObject(questionTypeDataObject);
            }
            return RedirectToAction("QuestionDetailList");
        }

        public async Task<IActionResult> QuestionDetailList(string KindData)
        { 
            ManageQuestion manageQuestion = new ManageQuestion();
            ViewBag.KindData = KindData;

            List<QuestionTypeDataObjects> data = manageQuestion.getQuestionObject(0, false, "O", false, true, SharedComm.GetToday(), KindData);

            return View("QuestionDetailList", data);
        }


        public async Task<IActionResult> DeleteQuestion(int Id)
        {
            ManageQuestion manageQuestion = new ManageQuestion();
            manageQuestion.DeleteQuestion(Id);
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> DeleteQuestionBank(int Id, string WhenDate)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();
            manageQuestionBank.DeleteQuestionBank(Id);
            return RedirectToAction("FromTemplate", new { WhenDate = WhenDate });
        }

        public async Task<IActionResult> CreateQuizById(int Id, string WhenDate)
        {
            ManageQuestionBank manageQuestionBank = new ManageQuestionBank();
            manageQuestionBank.CreateQuizById(Id, WhenDate);
            return RedirectToAction("Index");
        }
    }
}

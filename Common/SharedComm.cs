using BulletinBoard.BusinessLayer;
 
using BulletinBoard.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using SkiaSharp;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;

namespace BulletinBoard.Common
{
    public static class SharedComm
    {

        // 🔁 선물 가격 규칙(UnitPriceRangeSQL · PriceListCase)은
        //    Services/GiftCatalog/GiftPriceRule.cs 로 옮겼습니다.

        public static string DDay()
        {
            DateTime baseDate = new DateTime(2025, 7, 28).AddHours(ConstatsCommon.timeDifference16);

            int diff = (GetToday() - baseDate).Days;

            string dDayLabel = diff == 0 ? "D-DAY"
                             : diff > 0 ? $"D+{diff}"
                             : $"D{diff}"; // 음수면 D-3 같은 형식

            return $"{dDayLabel}";
        }
        public static DateTime GetKoreaNow()
        {
            // Windows 서버: "Korea Standard Time"
            // (SmarterASP.NET 은 보통 Windows라서 이 ID 사용)
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }
        // varbinary 데이터를 base64로 변환하고 이미지를 압축하는 메서드
        public static string CompressAndConvertToBase64(byte[] imageData, int quality = 50)
        {
            using (var inputStream = new SKMemoryStream(imageData))
            {
                // SkiaSharp을 사용하여 이미지 로드
                using (var codec = SKCodec.Create(inputStream))
                using (var bitmap = SKBitmap.Decode(codec))
                {
                    // 압축할 이미지 생성
                    using (var compressedStream = new MemoryStream())
                    {
                        var image = SKImage.FromBitmap(bitmap);
                        var data = image.Encode(SKEncodedImageFormat.Jpeg, quality); // 50% 품질로 압축

                        // 압축된 이미지를 스트림에 저장
                        data.SaveTo(compressedStream);

                        // 압축된 이미지를 base64로 변환
                        var compressedImageBytes = compressedStream.ToArray();
                        return Convert.ToBase64String(compressedImageBytes);
                    }
                }
            }
        }
        public static string ExtractYouTubeId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            var regex = new Regex(@"(?:youtu\.be/|youtube\.com/(?:watch\?v=|embed/|v/))([\w\-]{11})", RegexOptions.IgnoreCase);
            var match = regex.Match(url);

            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }

            return null;
        }



        public static string DefaultDeptCode(string DeptCode)
        {
            if (string.IsNullOrEmpty(DeptCode))
            {
                return "D1001";
            };
            return DeptCode;
        }

        public static string MakeBibleSerialJangZul(string BibleText, int FromJang, int FromZul, int ToJang, int ToZul)
        {
            if (FromJang == 0)
                return string.Empty;

            if (FromJang == ToJang)
            {
                if (FromZul == ToZul)
                {
                    if (FromZul > 0)
                    {
                        return $"{FromJang}장 {FromZul}절";
                    }
                    else
                    {
                        return $"{FromJang}장";
                    }
                }
                else
                {
                    return $"{FromJang}장 {FromZul}~{ToZul}절";
                }
            }
            else
            {
                return $"{FromJang}장{FromZul}절 ~ {ToJang}장{ToZul}절";
            }
        }

        public static bool HasGeomorphologicalCategory(List<UploadData> uploadDatas, string categoryData)
        {
            if (uploadDatas == null || uploadDatas.Count == 0)
                return false;

            return uploadDatas.Any(data =>
                !string.IsNullOrEmpty(data.CategoryData) &&
                data.CategoryData.Equals(categoryData, StringComparison.OrdinalIgnoreCase));
        }

        public static string MakeBibleQueryString(string BibleText, int FromJang, int FromZul, int ToJang, int ToZul)
        {
            return $"&BibleText={BibleText}&FromJang={FromJang}&FromZul={FromZul}&ToJang={ToJang}&ToZul={ToZul}";
        }

        //";

        public static string appendSpecialChar(string value)
        {

            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("'", "''");

        }
        public static DataSet CheckDataSet(DataSet ds)
        {
            // DataSet이 null이거나 테이블이 없을 때
            if (ds == null || ds.Tables.Count == 0)
            {
                return null;
            }

            // 각 테이블에 데이터가 있는지 확인
            foreach (DataTable table in ds.Tables)
            {
                if (table.Rows.Count > 0)
                {
                    return ds;  // 데이터가 있는 경우 DataSet 반환
                }
            }

            // 데이터가 없는 경우 null 반환
            return null;
        }

        public static void AddToLog(string controllerName, string actionName, string userName, string requestIP)
        {

            if (userName.ToLower() == "anonymous")
                return;
            if (userName.ToLower() == "SuperAdmin".ToLower())
                return;
            if (userName.ToLower() == "Basic".ToLower())
                return;
            if (userName.ToLower() == "Admin".ToLower())
                return;


            #region log history
            if (true)
            {
                ManageLoginHistory manageLoginHistory = new ManageLoginHistory();
                LoginHistory loginHistory = new LoginHistory() { ControllerName = controllerName, ActionName = actionName, UserName = userName, requestIP = requestIP };
                manageLoginHistory.AddLoginHistory(loginHistory);
            }

            #endregion
        }

        public static TodayCollectWordToRead ReadTodayWordToReadMission(int TodayWordToReadId, string userName, DateTime workDT, string langCD, string UserId)
        {
            ManageeCommon manageeCommon = new ManageeCommon();
            TodayCollectWordToRead todayCollectWordToRead = GetTodayCollectWordToRead(TodayWordToReadId, workDT, langCD, UserId);

            todayCollectWordToRead.TodayCheckCompositeData = manageeCommon.getTodayStatusData(userName, workDT);
            return todayCollectWordToRead;
        }

        public static TodayCollectPrayPhrase ReadTodayPrayPhraseMission(string userName, DateTime workDT, string UserId)
        {
            ManageeCommon manageeCommon = new ManageeCommon();

            TodayCollectPrayPhrase todayCollectPrayPhrase = GetTodayCollectPrayPhrase(workDT, "ko", UserId);

            todayCollectPrayPhrase.TodayCheckCompositeData = manageeCommon.getTodayStatusData(userName, workDT);
            return todayCollectPrayPhrase;
        }

        public static TodayCollectQuestion ReadTodayQuestionMission(int QuestionDataId, string userName, DateTime workDT, string UserName = "", bool AddPoint = false)
        {
            ManageeCommon manageeCommon = new ManageeCommon();

            TodayCollectQuestion todayCollectQuestion = new TodayCollectQuestion();

            todayCollectQuestion.QuestionTypeDataObjectsData = SharedComm.CollectTodayQuestion(QuestionDataId, workDT, UserName, AddPoint); // Question Info.
            todayCollectQuestion.TodayCheckCompositeData = manageeCommon.getTodayStatusData(userName, workDT);
            todayCollectQuestion.questionProcessStatus = getQuestionProcessStatus(userName, workDT, todayCollectQuestion.QuestionTypeDataObjectsData.Id);

            return todayCollectQuestion;
        }

        public static QuestionProcessStatus getQuestionProcessStatus(string userName, DateTime workDT, int Id)
        {
            ManageClickHistoryForQuestion manageClickHistoryForQuestion = new ManageClickHistoryForQuestion();
            QuestionProcessStatus _QuestionProcessStatus = new QuestionProcessStatus();

            List<QuestionProcessStatus> questionProcessStatus = manageClickHistoryForQuestion.getCurrentStatus(userName, workDT, Id);

            if (questionProcessStatus.Count > 0)
            {
                _QuestionProcessStatus = questionProcessStatus[0];
            }
            else
            {
                _QuestionProcessStatus = new QuestionProcessStatus() { TryCnt = 0 };
            }

            return _QuestionProcessStatus;
        }

        public static WeeklyWordToRead getActiveWeeklyWordToRead(DateTime workDT,int cYear)
        {
            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            List<WeeklyWordToReads> weeklyWordToReads = manageWeeklyWordToRead.getWeeklyWordToRead(0, true, workDT,cYear);
            WeeklyWordToRead weeklyWordToRead = new WeeklyWordToRead();
            weeklyWordToRead = weeklyWordToReads[0];
            return weeklyWordToRead;
        }

        public static TodayCollectWordToRead GetTodayCollectWordToRead(int TodayWordToReadId, DateTime workDT, string langCD, string UserId)
        {
            ManageTodayWordToRead manageTodayWordToRead = new ManageTodayWordToRead();
            List<TodayWordToReads> TodayWordToReads = manageTodayWordToRead.getTodayWordToRead(TodayWordToReadId, true, workDT);

            TodayCollectWordToRead todayCollectWordToRead = new TodayCollectWordToRead();

            if (TodayWordToReads.Count > 0)
            {
                todayCollectWordToRead.TodayWordToReadData = TodayWordToReads[0];

                ManageeBible manageeBible = new ManageeBible();
                todayCollectWordToRead.TodayeBiblesData = manageeBible.geteBible2(TodayWordToReads[0].BibleText, TodayWordToReads[0].FromJang, TodayWordToReads[0].FromZul, TodayWordToReads[0].ToJang, TodayWordToReads[0].ToZul, langCD, UserId);
            }

            return todayCollectWordToRead;
        }

        /// <summary>
        /// 오늘의 퀴즈나 특정 일자의 퀴즈를 읽어 온다.
        /// </summary>
        /// <param name="QuestionDataId"></param>
        /// <param name="workDT"></param>
        /// <param name="UserName"></param>
        /// <param name="AddPoint"></param>
        /// <returns></returns>
        public static QuestionTypeDataObjects CollectTodayQuestion(int QuestionDataId, DateTime workDT, string UserName = "", bool AddPoint = false)
        {
            ManageQuestion manageQuestion = new ManageQuestion();
            List<QuestionTypeDataObjects> questionTypeDataObjects = manageQuestion.getQuestionObject(QuestionDataId, true, "O", false, false, workDT, string.Empty, UserName, AddPoint);

            QuestionTypeDataObjects questionTypeDataObject = new QuestionTypeDataObjects();
            if (questionTypeDataObjects.Count > 0)
            {
                questionTypeDataObject = questionTypeDataObjects[0];
            }

            return questionTypeDataObject;
        }

        public static TodayCollectPrayPhrase GetTodayCollectPrayPhrase(DateTime workDT, string langCD, string UserId)
        {
            ManagePrayPhrase managePrayPhrase = new ManagePrayPhrase();
            List<PrayPhrases> prayPhrases = managePrayPhrase.getPrayPhrases(0, true, workDT);
            ManageeBible manageeBible = new ManageeBible();

            TodayCollectPrayPhrase todayCollectPrayPhrase = new TodayCollectPrayPhrase();


            if (prayPhrases.Count > 0)
            {
                todayCollectPrayPhrase.PrayPhraseData = prayPhrases[0];
                todayCollectPrayPhrase.TodayeBiblesToPrayData = manageeBible.geteBible2(prayPhrases[0].BibleTextToPray, prayPhrases[0].BibleTextToPrayFromJang, prayPhrases[0].BibleTextToPrayFromZul, prayPhrases[0].BibleTextToPrayToJang, prayPhrases[0].BibleTextToPrayToZul, langCD, UserId);
            }
            return todayCollectPrayPhrase;
        }

        public static TodayCollectWeeklyWordToRead GetTodayCollectWeeklyWordToRead(int Id, DateTime workDT, string langCD, string UserId, int cYear)
        {
            ManageeBible manageeBible = new ManageeBible();

            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            List<WeeklyWordToReads> WeeklyWordToReads = manageWeeklyWordToRead.getWeeklyWordToRead(Id, true, workDT, cYear);

            TodayCollectWeeklyWordToRead todayCollectWeeklyWordToRead = new TodayCollectWeeklyWordToRead();

            if (WeeklyWordToReads.Count > 0)
            {
                todayCollectWeeklyWordToRead.WeeklyWordToReadData = WeeklyWordToReads[0];
                todayCollectWeeklyWordToRead.WeekeBiblesDataToRecite = manageeBible.geteBible2(WeeklyWordToReads[0].BibleTextToRecite, WeeklyWordToReads[0].BibleTextToReciteFromJang, WeeklyWordToReads[0].BibleTextToReciteFromZul, WeeklyWordToReads[0].BibleTextToReciteToJang, WeeklyWordToReads[0].BibleTextToReciteToZul, langCD, UserId);
                todayCollectWeeklyWordToRead.WeekeBiblesDataToRead = manageeBible.geteBible2(WeeklyWordToReads[0].BibleTextToRead, WeeklyWordToReads[0].BibleTextToReadFromJang, WeeklyWordToReads[0].BibleTextToReadFromZul, WeeklyWordToReads[0].BibleTextToReadToJang, WeeklyWordToReads[0].BibleTextToReadToZul, langCD, UserId);
                todayCollectWeeklyWordToRead.WeekeBiblesDataToRead_1 = manageeBible.geteBible2(WeeklyWordToReads[0].BibleTextToRead_1, WeeklyWordToReads[0].BibleTextToReadFromJang_1, WeeklyWordToReads[0].BibleTextToReadFromZul_1, WeeklyWordToReads[0].BibleTextToReadToJang_1, WeeklyWordToReads[0].BibleTextToReadToZul_1, langCD, UserId);
            }

            return todayCollectWeeklyWordToRead;
        }

        public static TodayCollectWeeklyWordToRead ReadTodayWeeklyWordToReadMission(Category CategoryData, int WeeklyWordToReadID, string userName, DateTime workDT, string langCD, string UserId, int cYear)
        {
            ManageeCommon manageeCommon = new ManageeCommon();

            TodayCollectWeeklyWordToRead todayCollectWeeklyWordToRead = SharedComm.GetTodayCollectWeeklyWordToRead(WeeklyWordToReadID, workDT, langCD, UserId, cYear);

            ManageWeeklyWordToRead manageWeeklyWordToRead = new ManageWeeklyWordToRead();

            todayCollectWeeklyWordToRead.UploadDatas = manageWeeklyWordToRead.getUploadDataWeeklyWordToReadAssoc2(CategoryData, WeeklyWordToReadID);

            todayCollectWeeklyWordToRead.UploadDatas = todayCollectWeeklyWordToRead.UploadDatas
            .OrderBy(u => u.CategoryData)
            .ThenBy(u => u.TEXTData)
            .ToList();

            //todayCollectWeeklyWordToRead.UploadDatasWelCome = manageWeeklyWordToRead.getUploadDataWeeklyWordToReadAssoc(Category.Welcome, WeeklyWordToReadID);
            //todayCollectWeeklyWordToRead.UploadDatasTeacher = manageWeeklyWordToRead.getUploadDataWeeklyWordToReadAssoc(Category.Teacher, WeeklyWordToReadID);

            todayCollectWeeklyWordToRead.TodayCheckCompositeData = manageeCommon.getTodayStatusData(userName, workDT);
            return todayCollectWeeklyWordToRead;
        }

        public static string GetKoreanDayOfWeek(DateTime date)
        {
            string[] koreanDays = { "일", "월", "화", "수", "목", "금", "토" };
            // DateTime.DayOfWeek는 0(일요일)부터 시작합니다.
            return koreanDays[(int)date.DayOfWeek];
        }

        public static bool isAdmin(string UserName)
        {
            if (string.IsNullOrEmpty(UserName)) { return false; }

            if (Roles.SuperAdmin.ToString() == UserName.ToString())
                return true;
            if (Roles.Admin.ToString() == UserName.ToString())
                return true;
            if (Roles.Basic.ToString() == UserName.ToString())
                return true;
            return false;
        }

        public static string HowMuchItTakes(int targetScore, int currentScore = 0)
        {

            // 점수 계산: 하루에 4점, 매주 1점 추가
            int dailyPoints = 4;
            int weeklyBonus = 1;

            // 점수 계산에 사용할 변수
            int totalScore = currentScore;
            int daysPassed = 0;

            // 목표 점수에 도달할 때까지 계산
            while (totalScore < targetScore)
            {
                totalScore += dailyPoints;

                // 매 7일마다 추가로 1점 획득
                if (daysPassed % 7 == 0 && daysPassed != 0)
                {
                    totalScore += weeklyBonus;
                }

                daysPassed++;
            }

            // 결과 출력
            DateTime dt = DateHelper.GetKoreaNow().AddDays(daysPassed);

            return $"{dt.Year}년{dt.Month}월{dt.Day}일 ({GetWeekKor(dt)})";

        }
        //$"{dt.Year}년{dt.Month}월{dt.Day}일 {GetWeekKor(dt)}요일";

        public static DateTime GetToday(int days = 0)
        {
            return DateHelper.GetKoreaNow();
        }
        public static string GetTodayString(int days = 0)
        {
            return GetToday(days).ToString("yyyy-MM-dd");
        }


        public static string GetDaysWithDCountGreaterThanZero(List<WeekStatusData> weekStatusDataList)
        {
            var daysWithDCount = weekStatusDataList
                .Where(data => data.DCount > 0)
                .Select(data => int.Parse(data.DDD).ToString())
                .ToList();

            return string.Join(",", daysWithDCount);
        }

        public static string GetTodayStringKor(int days = 0)
        {
            DateTime dt = GetToday(days);

            return $"{dt.Year}년{dt.Month}월{dt.Day}일 ({GetWeekKor(dt)})";
        }
        public static int GetDayNumber()
        {
            DateTime startDate = new DateTime(2025, 7, 14);
            int dayNumber = (GetToday().Date - startDate.Date).Days + 1;

            return dayNumber >= 1 ? (dayNumber -1) : 0;  // 기준일보다 빠른 날짜는 0으로 처리
        }


        public static string GetTodayAsString()
        {
            return GetToday().ToString("yyyyMMdd");
        }

        public static string GetTodayToStringKor(DateTime dt)
        {
            return $"{dt.Year}년 {dt.Month}월{dt.Day}일 ({GetWeekKor(dt)})";
        }


        public static bool showVictoryChallenge()
        {
            var today = GetToday();

            //.AddHours(ConstatsCommon.timeDifference16)
            DateTime dt1 = new DateTime(2025, 7, 14).AddHours(ConstatsCommon.timeDifference16);
            DateTime dt2 = new DateTime(2025, 7, 19).AddHours(ConstatsCommon.timeDifference16);

            DateTime dt3 = new DateTime(2025, 7, 21).AddHours(ConstatsCommon.timeDifference16);
            DateTime dt4 = new DateTime(2025, 7, 26).AddHours(ConstatsCommon.timeDifference16);

            var showVictoryChallenge =
            (today >= dt1) && today <= dt2 || (today >= dt3 && today <= dt4);

            return showVictoryChallenge;

        }

        public static string GetTodayToStringKor_MMdd(DateTime dt)
        {
            return $"{dt.Month}월{dt.Day}일 ({GetWeekKor(dt)})";
        }

        public static string GetTodayToStringKor_dd(DateTime dt)
        {
            return $"{dt.Day}일 ({GetWeekKor(dt)})";
        }

        public static string GetTodayStringKor2(int days = 0)
        {
            DateTime dt = GetToday(days);

            return $"{dt.Year}년{dt.Month}월";
        }

        public static string GetWeekKor(DateTime dt)
        {
            DayOfWeek dayOfWeek = dt.DayOfWeek;

            string dayInKorean = "";

            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    dayInKorean = "일";
                    break;
                case DayOfWeek.Monday:
                    dayInKorean = "월";
                    break;
                case DayOfWeek.Tuesday:
                    dayInKorean = "화";
                    break;
                case DayOfWeek.Wednesday:
                    dayInKorean = "수";
                    break;
                case DayOfWeek.Thursday:
                    dayInKorean = "목";
                    break;
                case DayOfWeek.Friday:
                    dayInKorean = "금";
                    break;
                case DayOfWeek.Saturday:
                    dayInKorean = "토";
                    break;
            }

            return $"{dayInKorean}";
        }


        public static string SqlSTatSnipWeekEngToKorShort()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("         WHEN 'Monday'     THEN '월' ");
            sb.AppendFormat("         WHEN 'Tuesday'    THEN '화' ");
            sb.AppendFormat("         WHEN 'Wednesday'  THEN '수' ");
            sb.AppendFormat("         WHEN 'Thursday'   THEN '목' ");
            sb.AppendFormat("         WHEN 'Friday'     THEN '금' ");
            sb.AppendFormat("         WHEN 'Saturday'   THEN '토' ");
            sb.AppendFormat("         WHEN 'Sunday'     THEN '일' ");
            return sb.ToString();
        }
        public static string SqlSTatSnipWeekEngToKorFull()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("         WHEN 'Monday'     THEN '월요일' ");
            sb.AppendFormat("         WHEN 'Tuesday'    THEN '화요일' ");
            sb.AppendFormat("         WHEN 'Wednesday'  THEN '수요일' ");
            sb.AppendFormat("         WHEN 'Thursday'   THEN '목요일' ");
            sb.AppendFormat("         WHEN 'Friday'     THEN '금요일' ");
            sb.AppendFormat("         WHEN 'Saturday'   THEN '토요일' ");
            sb.AppendFormat("         WHEN 'Sunday'     THEN '일요일' ");
            return sb.ToString();
        }


        public static int HowManyDaysElapse()
        {
            // 기준일 지정
            DateTime baseDay = new DateTime(2025, 7, 5).AddHours(ConstatsCommon.timeDifference16);

            // 날짜 차이 계산 (기준일 포함해서 계산하려면 +1)
            int dayDiff = (GetToday() - baseDay).Days + 1;

            return dayDiff;
        }

        public static string SqlSTatSnipWeekKorToKorFull()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("         WHEN '월요일'     THEN '월요일' ");
            sb.AppendFormat("         WHEN '화요일'     THEN '화요일' ");
            sb.AppendFormat("         WHEN '수요일'     THEN '수요일' ");
            sb.AppendFormat("         WHEN '목요일'     THEN '목요일' ");
            sb.AppendFormat("         WHEN '금요일'     THEN '금요일' ");
            sb.AppendFormat("         WHEN '토요일'     THEN '토요일' ");
            sb.AppendFormat("         WHEN '일요일'     THEN '일요일' ");
            return sb.ToString();
        }
        public static string SqlSTatSnipWeekKorToKorShort()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("         WHEN '월요일'     THEN '월' ");
            sb.AppendFormat("         WHEN '화요일'     THEN '화' ");
            sb.AppendFormat("         WHEN '수요일'     THEN '수' ");
            sb.AppendFormat("         WHEN '목요일'     THEN '목' ");
            sb.AppendFormat("         WHEN '금요일'     THEN '금' ");
            sb.AppendFormat("         WHEN '토요일'     THEN '토' ");
            sb.AppendFormat("         WHEN '일요일'     THEN '일' ");
            return sb.ToString();
        }

        public static string TrimString(string Data)
        {
            if (string.IsNullOrEmpty(Data))
                return string.Empty;
            return Data.Trim();
        }

        public static void MaxTime(int MM, int SS, int maxMM, int maxSS, out int outMM, out int outSS)
        {
            outMM = MM;
            outSS = SS;

            if (MM == 0 && SS == 00)
            {
                outMM = maxMM;
                outSS = maxSS;
            };
        }

        public static int CalculateIndex(int totalDataCount, DateTime date)
        {
            // 8월 1일부터 오늘까지의 일 수를 계산합니다
            DateTime startDate = new DateTime(2024, 8, 26);
            int daysSinceStart = (date - startDate).Days;

            // 데이터의 인덱스를 계산합니다 (0부터 시작)
            int index = daysSinceStart % totalDataCount;

            return index;
        }
        public static int CountUnderscorePatterns(string text)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '(')
                {
                    int j = i + 1;
                    while (j < text.Length && text[j] != ')')
                    {
                        j++;
                    }
                    if (j < text.Length && text[j] == ')')
                    {
                        string inside = text.Substring(i + 1, j - i - 1);
                        bool allUnderscores = true;
                        foreach (char c in inside)
                        {
                            if (c != '_')
                            {
                                allUnderscores = false;
                                break;
                            }
                        }
                        if (allUnderscores)
                        {
                            count++;
                        }
                        i = j;
                    }
                }
            }
            return count;
        }

        public static bool ContainsBracePattern(string input)
        {
            // 정규식: \( -> 여는 괄호, _+ -> 한 개 이상의 밑줄, \) -> 닫는 괄호
            string pattern = @"\(_+\)";
            return Regex.IsMatch(input, pattern);
        }
        public static int GetDateDifferenceInDays(DateTime date1, DateTime date2)
        {
            TimeSpan timeSpan = date2.Date - date1.Date;
            return Math.Abs(timeSpan.Days);
        }

        public static string FormatDateRange(DateTime date1, DateTime date2)
        {
            // 날짜 정렬
            if (date1 > date2)
            {
                var temp = date1;
                date1 = date2;
                date2 = temp;
            }

            if (date1.Year == date2.Year)
            {
                if (date1.Month == date2.Month)
                {
                    return $"{date1.Year}년{date1.Month}월{date1.Day}일~{date2.Day}일";
                }
                else
                {
                    return $"{date1.Year}년{date1.Month}월{date1.Day}일~{date2.Month}월{date2.Day}일";
                }
            }
            else
            {
                return $"{date1.Year}년{date1.Month}월{date1.Day}일~{date2.Year}년{date2.Month}월{date2.Day}일";
            }
        }
    }
}

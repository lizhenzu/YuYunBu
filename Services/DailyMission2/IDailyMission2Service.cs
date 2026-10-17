using BulletinBoard.Models;
using BulletinBoard.Models.DailyMission2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Services.DailyMission2
{
    public interface IDailyMission2Service
    {
        Task<DailyMission2ContentIds> GetContentIdsAsync(DateTime selectedDate);
        Task<DailyMission2DayStatus> GetDayStatusAsync(string userName, DateTime selectedDate);
        Task<DailyMission2YearPoint> GetYearPointAsync(string userName, int cYear);
        Task<Dictionary<string, bool>> GetCalendarAllDoneMapAsync(string userName, DateTime selectedDate);
        Task<int> GetTargetPointFromAwardBaseAsync();

        /// <summary>
        /// 시상 기준 점수(AwardBase.AwardLevel)를 작은 것부터 돌려 준다.
        /// "다음 선물까지 몇 점 남았는지" 를 계산하는 데 쓴다.
        /// </summary>
        Task<List<int>> GetAwardLevelsAsync();
        Task<SaveResult> SaveWordAsync(string userName, DateTime selectedDate, int todayWordToReadId, bool toMe, bool toParent);
        Task<SaveResult> SavePrayAsync(string userName, DateTime selectedDate, int prayPhraseId, bool toMe, bool toParent, bool toFriend);
        Task<SaveResult> SaveQuizAsync(string userName, DateTime selectedDate, int questionId, string answerQ, string? correctAnswer = null);

        /// <summary>
        /// 이 사람이 그 날 그 퀴즈에 실제로 무엇을 골랐는지 시도 순서대로 돌려 준다.
        /// 퀴즈를 끝낸 뒤 "내가 고른 자리"와 정답/오답을 화면에 표시하는 데 쓴다.
        /// </summary>
        Task<List<string>> GetQuizMyAnswersAsync(string userName, DateTime selectedDate, int questionId);
        Task<SaveResult> SaveReciteAsync(string userName, DateTime selectedDate, int weeklyWordToReadId);
        Task<List<ParticipantCardRow>> GetParticipantCardsAsync(int year);
        Task<List<ParticipantCardRow>> GetParticipantPersonCardsAsync(int year);
        Task<List<ParticipantCardRow>> GetParticipantTeamCardsAsync(int year);
        // ✅ Word.cshtml에서 TodayWordToRead 상세+본문 표시용
        Task<TodayWordDetailResult?> GetTodayWordDetailAsync(DateTime selectedDate, string langCD, string userId);

        /// <summary>
        /// 날짜가 아니라 구절 범위(성경·시작 장절·끝 장절)로 본문을 가져온다.
        /// DailyMission4/BibleReadRange 에서 쓴다. 잘못된 범위면 null.
        /// </summary>
        Task<TodayWordDetailResult?> GetWordDetailByRangeAsync(string bibleText, int fromJang, int fromZul, int toJang, int toZul, string langCD, string userId);
        Task<List<TodayWordToReads>> GetTodayWordAsync(int Id, bool activeData, DateTime workDT);
        Task<eBibles> GetTodayBibleSummaryAsync(string BibleText);
        Task<WeeklyWordDetailResult?> GetWeeklyWordDetailAsync(DateTime selectedDate, string langCD, string userId, int cYear);
        Task<SaveResult> MarkVerseReadAsync(string UserId, DateTime selectedDate, string langCD, string bibleText, int jang, int zul, int VerseId);
        Task<SaveResult> MarkVerseUnreadAsync(string UserId, DateTime selectedDate, string langCD, string bibleText, int jang, int zul, int VerseId);
        Task<SaveResult> ToggleBookmarkAsync(string UserId, DateTime selectedDate, string langCD, string bibleText, int jang, int zul, int VerseId);

        Task<int> GetUserTotalPointsAsync(string userName, DateTime fromDate, DateTime toDate);
        Task<int> GetUserTotalPointsThisYearAsync(string userName, int year);
        Task<SaveResult> MarkPageReadAsync(string UserId, DateTime selectedDate, string langCD, string bibleText, int fromJang, int fromZul, int toJang, int toZul);
        Task<SaveResult> MarkAllReadAsync(string UserId, DateTime selectedDate, string langCD, string bibleText, int fromJang, int fromZul, int toJang, int toZul);
        Task<(bool isRead, int readVerses, int totalVerses)> ToggleBibleRangeAsync(string userId, string bibleText, int fromJang, int fromZul, int toJang, int toZul);
        Task<DailyMission2QuizContent?> GetTodayQuestionAsync(DateTime selectedDate);

        Task<List<UserDailyPointRow>> GetUserDailyPointsAsync(string userName, DateTime fromDate, DateTime toDate);

        // 그날 오늘의 말씀 미션을 수행한 사람 수 (중복 제외)
        Task<int> GetTodayWordParticipantCountAsync(DateTime day);
    
}
}

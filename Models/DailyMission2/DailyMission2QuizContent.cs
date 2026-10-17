using System;

namespace BulletinBoard.Models.DailyMission2
{

    public class DailyMission2QuizContent
    {
        public int? QuestionId { get; set; }

        public string? QuestionType { get; set; }          // 예: "1" 등 (DB 정의대로 표시만)
        public string? QuestionQ { get; set; }             // 문제 본문
        public string? Question1 { get; set; }             // 보기 1
        public string? Question2 { get; set; }             // 보기 2
        public string? Question3 { get; set; }             // 보기 3
        public string? Question4 { get; set; }             // 보기 4
        public string? CorrectAnswer { get; set; }         // 정답(저장/판정용)
        public string? AnswerGuide { get; set; }           // 해설/힌트
        public string? QuestionBackground { get; set; }    // 배경(설명)
        public DateTime ExposeDate { get; set; }

        public bool HasChoices =>
            !string.IsNullOrWhiteSpace(Question1) ||
            !string.IsNullOrWhiteSpace(Question2) ||
            !string.IsNullOrWhiteSpace(Question3) ||
            !string.IsNullOrWhiteSpace(Question4);
    }
}

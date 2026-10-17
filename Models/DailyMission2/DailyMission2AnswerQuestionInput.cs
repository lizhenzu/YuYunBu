namespace BulletinBoard.Models.DailyMission2
{
    public class DailyMission2AnswerQuestionInput
    {
        public string date { get; set; } = "";     // "yyyy-MM-dd"
        public int questionId { get; set; }
        public string answerQ { get; set; } = "";  // 사용자가 선택/입력한 답
    }
}

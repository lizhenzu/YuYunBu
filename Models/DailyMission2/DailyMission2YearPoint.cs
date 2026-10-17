namespace BulletinBoard.Models.DailyMission2
{
    public class DailyMission2YearPoint
    {
        public string UserName { get; set; } = "";
        public int cYear { get; set; }

        public int WordPoint { get; set; }
        public int PrayPoint { get; set; }
        public int QuizPoint { get; set; }
        public int RecitePoint { get; set; }

        // ✅ 표준(권장)
        public int YearTotalPoint { get; set; }

        // ✅ 호환(기존 코드에서 Total을 쓰면 여기로)
        public int Total
        {
            get => YearTotalPoint;
            set => YearTotalPoint = value;
        }
    }
}

namespace BulletinBoard.ViewModels.Dashboard
{
    public class StudentPointTotalRankRowVM
    {
        public int Rank { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";

        public int BiblePoint { get; set; }         // 도전성경(2026)
        public int MissionBookPoint { get; set; }    // 미션북(2026)
        public int DevotionPoint { get; set; }       // 말씀&일기(2026)
        public int AttendancePoint { get; set; }     // 출석카운트 * 2
        public int EvangelismPoint { get; set; }     // 전도 횟수 * 50
        public int TotalPoint { get; set; }          // 합계

        public string RoleLabel { get; set; } = "기타";

        // 추가 정보
        public int? ClassYear { get; set; }          // 학생이면 학년
        public int? ClassBan  { get; set; }          // 학생이면 반
        public DateTime? LastActivityDate { get; set; } // 마지막 활동일
    }

    public class StudentPointTotalRankVM
    {
        public int Year { get; set; }
        public List<StudentPointTotalRankRowVM> Rows { get; set; } = new();
    }
}

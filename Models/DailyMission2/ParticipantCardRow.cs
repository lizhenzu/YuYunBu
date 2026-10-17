namespace BulletinBoard.Models.DailyMission2
{
    public class ParticipantCardRow
    {
        public string UserName { get; set; } = "";
        public string UserId { get; set; } = "";

        public string FullName { get; set; } = "";
        public string ProfilePicture { get; set; } = "";  // DB의 ProfilePicture (상대/절대 경로일 수 있음)

        // 역할 표시
        public bool IsStudent { get; set; }
        public bool IsTeacher { get; set; }
        public bool IsParent { get; set; }

        // 학생이면
        public int? ClassYear { get; set; }
        public int? ClassBan { get; set; }

        // 교사면
        public string DeptCode { get; set; } = "";
        public string RoleCode { get; set; } = "";
        public string RoleName { get; set; } = "";

        // 점수/달성
        public int YearPoint { get; set; }
        public DateTime? LastActionDate { get; set; }
        public int ConsecutiveDays { get; set; }   // 연속 수행일수

        // 화면용
        public string DisplayRoleText { get; set; } = "";   // 예: "교사(유년부/부장)" or "학생(3학년 2반)"
        public int AchievePercent { get; set; }            // 0~100 (기준은 질문드릴게요)

        public bool IsFamily { get; set; }                 // 가족 카드 여부

        public int? FamilyTeamId { get; set; }             // 가족팀 ID (개인이면 null)
        public string FamilyTeamName { get; set; } = "";   // 예: "김민지 가족"

        // 가족 구성원(학생/엄마/아빠) 목록
        public List<FamilyMemberRow> FamilyMembers { get; set; } = new();
    }


}

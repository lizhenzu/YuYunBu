namespace BulletinBoard.Models.DailyMission2
{
    public class FamilyTeamRow
    {
        public int FamilyTeamId { get; set; }
        public string TeamName { get; set; } = "";
        public bool IsActive { get; set; }
        public string? Memo { get; set; }
        public DateTime InsertDT { get; set; }
        public DateTime UpdateDT { get; set; }
        public int MemberCount { get; set; }      // 활성 구성원 수 (관리 화면 목록용)
        public string? CreatorUserName { get; set; }   // 만든 사람 (탈퇴 불가 · 내보내기 권한)
    }

    public class FamilyMemberRow2
    {
        public int FamilyTeamId { get; set; }
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";   // 실명 (없으면 UserName)
        public bool UserExists { get; set; } = true; // 가입 계정에 있는 아이디인지
        public int RoleCode { get; set; }   // 0=학생,1=엄마,2=아빠
        public int SortNo { get; set; }
        public bool IsActive { get; set; } = true;
        // ✅ 추가
        public int RelationCode { get; set; } = 9; // 0=본인,1=형,2=동생,3=언니,4=누나,9=미설정
    }

    public class FamilyAdminVM
    {
        public List<FamilyTeamRow> Teams { get; set; } = new();
        public int? SelectedTeamId { get; set; }
        public FamilyTeamRow? SelectedTeam { get; set; }
        public List<FamilyMemberRow2> Members { get; set; } = new();
        public List<FamilyRequestRow2> Requests { get; set; } = new();   // 이 팀의 대기 중인 가입 신청 · 초대
    }

    /// <summary>팀의 대기 중인 가입 신청(R) · 초대(I) 한 건 — 관리자 화면용</summary>
    public class FamilyRequestRow2
    {
        public int Id { get; set; }
        public string Kind { get; set; } = "";          // R = 가입 신청(신청자 = Target) · I = 초대(초대받은 사람 = Target)
        public string TargetUserName { get; set; } = "";
        public string TargetFullName { get; set; } = "";
        public string FromUserName { get; set; } = "";
        public string FromFullName { get; set; } = "";
        public string RelationKey { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}

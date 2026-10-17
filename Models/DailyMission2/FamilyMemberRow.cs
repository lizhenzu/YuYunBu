namespace BulletinBoard.Models.DailyMission2
{
      
    public class FamilyMemberRow
    {
        public string UserName { get; set; } = "";
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";

        // 0=학생, 1=엄마, 2=아빠
        public int RoleCode { get; set; }
        public string RoleLabel { get; set; } = ""; // "학생" / "엄마" / "아빠"

        // ✅ 형제관계(학생에만 의미 있음)
        // 0=본인, 1=형, 2=동생, 3=언니, 4=누나, 9=미설정
        public int RelationCode { get; set; } = 9;

        public int? ClassYear { get; set; }   // ✅ 학년 (학생만)
        public int? ClassBan  { get; set; }   // ✅ 반   (학생만)

        public int YearPoint { get; set; }
        public DateTime? LastActionDate { get; set; }

        public string ProfilePicture { get; set; } = "";
    }

}

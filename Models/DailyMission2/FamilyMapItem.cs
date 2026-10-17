namespace BulletinBoard.Models.DailyMission2
{

    public class FamilyMapItem
    {
        public int FamilyTeamId { get; set; }
        public string TeamName { get; set; } = "";
        public string UserName { get; set; } = "";
        public int RoleCode { get; set; }    // 0=학생,1=엄마,2=아빠
        public int SortNo { get; set; }

        // ✅ 학생 형제관계(기준자=RelationCode 0)
        // 0=본인, 1=형, 2=동생, 3=언니, 4=누나 (학생(RoleCode=0)에만 의미 있음)
        // 미설정은 9 권장
        public int RelationCode { get; set; } = 9;
    }


}

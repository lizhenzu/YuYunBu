namespace BulletinBoard.ViewModels.Worldview.Admin
{
    public class WorldviewSubmitRankRowVM
    {
        public int LessonNo { get; set; }
        public string? UserName { get; set; }
        public bool IsAllCorrect { get; set; }
        public int BasePoint { get; set; }
        public int BonusPoint { get; set; }
        public int TotalPoint { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? CheckedAt { get; set; }
    }
}

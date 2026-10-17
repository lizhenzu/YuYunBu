namespace BulletinBoard.Models.DailyMission2
{
    public class SaveResult
    {
        // ✅ 표준(권장)
        public bool Ok { get; set; }
        public string Message { get; set; } = "";

        // ✅ 호환(기존 코드에서 Success를 쓰면 여기로)
        public bool Success
        {
            get => Ok;
            set => Ok = value;
        }
        public bool? IsRead { get; set; }
        public bool? IsBookmarked { get; set; }
    }
}

namespace BulletinBoard.Models.Gift
{
    public class GiftPopupState
    {
        public string UserName { get; set; } = "";
        public int LastSeenMilestone { get; set; }
        public DateTime? BlockUntilUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}

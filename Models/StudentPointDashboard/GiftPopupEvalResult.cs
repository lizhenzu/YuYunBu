namespace BulletinBoard.ViewModels.Gift
{
    public class GiftPopupEvalResult
    {
        public bool ShowGiftPopup { get; set; }
        public int PopupMilestone { get; set; } // 0이면 없음
        public DateTime? BlockUntilUtc { get; set; }

        // credit 계산값(여기선 인터페이스만 둠)
        public int EarnedCredit { get; set; }
        public int SpentCredit { get; set; }
        public int AvailableCredit => EarnedCredit - SpentCredit;
    }
}

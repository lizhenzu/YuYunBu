namespace BulletinBoard.Enums
{
    /// <summary>
    /// 선물 팝업 버튼 액션 종류
    /// Dashboard.cshtml의 actionMap 숫자와 반드시 동일해야 함
    /// </summary>
    public enum PopupActionType
    {
        ACCUMULATE = 0,   // 누적(적립)
        LATER = 1,        // 다음에
        DEFER3 = 2,       // 3일 후
        DEFER7 = 3,       // 7일 후
        GO_PICK = 4       // 선물 고르러 가기
    }
}

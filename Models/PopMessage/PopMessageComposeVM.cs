namespace BulletinBoard.ViewModels.PopMessage
{
    public class PopMessageComposeVM
    {
        public string ToUserName { get; set; } = "";
        public string? ToFullName { get; set; }
        public string? ReturnUrl { get; set; }

        public string MessageText { get; set; } = "";
        public int MaxLen { get; set; } = 500; // 화면 제한(원하면 늘리세요)
    }
}

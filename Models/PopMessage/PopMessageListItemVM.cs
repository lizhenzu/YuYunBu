namespace BulletinBoard.ViewModels.PopMessage
{
    public class PopMessageListItemVM
    {
        public int Id { get; set; }
        public string? MessageText { get; set; }
        public string? MessagePreview { get; set; }

        public string? FromWhom { get; set; }   // UserName
        public string? ToWhom { get; set; }     // UserName

        public string? PeerUserName { get; set; } // 상대(UserName)
        public string? PeerFullName { get; set; } // 상대(fullName)

        public int Visited { get; set; } // 0=안읽음
        public string? InsertDT { get; set; } // 표시용(yyyy-MM-dd HH:mm)
    }
}

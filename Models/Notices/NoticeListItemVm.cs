using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Notice
{
    public class NoticeListItemVm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsPinned { get; set; }
        public int ViewCount { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPopup { get; set; }
        public DateTime? PopupUntil { get; set; }
    }
}

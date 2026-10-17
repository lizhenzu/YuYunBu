// 경로: /ViewModels/Notice/NoticeEditVm.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels.Notice
{
    public class Notice
    {
        public int Id { get; set; }

        public string Title { get; set; }      // NVARCHAR(200)
        public string Content { get; set; }    // NVARCHAR(MAX)
        public string AuthorName { get; set; } // NVARCHAR(50) NULL

        public bool IsPinned { get; set; }
        public int ViewCount { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsPopup { get; set; }
        public DateTime? PopupUntil { get; set; }
    }
}

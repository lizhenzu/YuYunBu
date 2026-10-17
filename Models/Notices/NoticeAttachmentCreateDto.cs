// 경로: /ViewModels/Notice/NoticeEditVm.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels.Notice
{
    public class NoticeAttachmentCreateDto
    {
        public string OriginalName { get; set; }
        public string StoredName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
    }
}

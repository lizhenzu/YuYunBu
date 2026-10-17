// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class NoticeAttachment
    {
        public long Id { get; set; }
        public int NoticeId { get; set; }

        public string OriginalName { get; set; }
        public string StoredName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

}

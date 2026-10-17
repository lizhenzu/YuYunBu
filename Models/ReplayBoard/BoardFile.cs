using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{

    public sealed class BoardFile
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string OriginalFileName { get; set; } = "";
        public string StoredFileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long FileSize { get; set; }
        public string RelativePath { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
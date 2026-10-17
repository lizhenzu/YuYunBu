// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public Album Album { get; set; }


        public string FileName { get; set; }
        public string UserName { get; set; }
        public string OriginalName { get; set; }
        public string ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public byte[] HashSha256 { get; set; }
        public string StoragePath { get; set; } // "/uploads/{albumId}/{file}"

        public string ThumbnailStoragePath { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

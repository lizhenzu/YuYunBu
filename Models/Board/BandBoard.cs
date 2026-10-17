// Models/BandPost.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels
{
    public class BandPost
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string WriterName { get; set; }

        [MaxLength(2000)]
        public string Content { get; set; }

        // C# 8(.NET Core 3.0) 방식: new() 대신 new List<T>()
        public List<BandPostImage> Images { get; set; } = new List<BandPostImage>();

        public int LikeCount { get; set; }

        public List<BandPostComment> Comments { get; set; } = new List<BandPostComment>();

        public DateTime CreatedAt { get; set; }
    }

    public class BandPostImage
    {
        public int Id { get; set; }
        public int BandPostId { get; set; }
        public string ImagePath { get; set; }
    }

    public class BandPostComment
    {
        public int Id { get; set; }
        public int BandPostId { get; set; }
        public string WriterName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

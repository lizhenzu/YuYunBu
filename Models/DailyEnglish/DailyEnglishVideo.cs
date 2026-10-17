using System;

namespace StockBulletinBoard.Models
{
    public class DailyEnglishVideo
    {
        public int    Id        { get; set; }
        public string YouTubeId { get; set; } = "";
        public string Title     { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        /// <summary>YouTube 썸네일 URL</summary>
        public string ThumbnailUrl => $"https://img.youtube.com/vi/{YouTubeId}/mqdefault.jpg";
    }
}

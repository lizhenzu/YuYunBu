using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class ExerciseDocListItem
    {
        public int ExerciseDocId { get; set; }
        public string Slug { get; set; }
        public string TitleKo { get; set; }
        public string Subtitle { get; set; }
        public string Audience { get; set; }
        public string LevelText { get; set; }
        public string Keywords { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int Visited { get; set; }
        
        public string CoverUrl { get; set; }

        public ExerciseDocListItem()
        {
            Slug = "";
            TitleKo = "";
            Subtitle = "";
            Audience = "";
            LevelText = "";
            Keywords = "";
            CoverUrl = "";
            Visited = 0;
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleYouTuBuEnroll
    {
        public int Id { get; set; }
        public string BibleText { get; set; }
        public int BibleJang { get; set; }
        public int YouTubePercentage { get; set; }
        public int DCount { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleYouTuBuBase
    {
        public int Id { get; set; }
        [Display(Name = "성경")]
        public string BibleText { get; set; }
        [Display(Name = "몇장")]
        public int Jang { get; set; }
        [Display(Name = "유투브")]
        public string URL { get; set; }
        [Display(Name = "Play Time")]
        public int TimeMM { get; set; }
        public int TimeSS { get; set; }
    }
} 
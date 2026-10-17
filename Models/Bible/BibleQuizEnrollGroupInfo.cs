using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleQuizEnrollGroupInfo
    {
        public string? BibleText { get; set; } 
        public int? Jang { get; set; }
        public int? DCount { get; set; }
    }
} 
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleQuizEnrollGroupInfo2
    {
        public int Id { get; set; }
        public string? BibleText { get; set; } 
        public int? Jang { get; set; }
        public int? DCount { get; set; }
    }
} 
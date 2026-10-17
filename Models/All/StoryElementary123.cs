using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class StoryElementary123
    {
        public int Id { get; set; }
        public string contentsURL { get; set; }
        public string contentsText { get; set; }
        public string kind { get; set; }
        public int visited { get; set; }
    }
} 
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class EveryBible
    {
        public int Id { get; set; }
        public string ExposeDate { get; set; }

        public string fullWord { get; set; }

        public int chapter { get; set; }
        public int startVerse { get; set; }
        public int endVerse { get; set; } 
    }
} 
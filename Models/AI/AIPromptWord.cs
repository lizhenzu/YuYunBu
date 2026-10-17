using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class AIPromptWord
    { 
        public int Id { get; set; }
        public string PromptText { get; set; }
        public DateTime updateDT { get; set; }
    }
}

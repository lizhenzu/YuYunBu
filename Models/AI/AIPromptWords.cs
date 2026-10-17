using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class AIPromptWords : AIPromptWord
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

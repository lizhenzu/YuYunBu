using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class StoryElementary123s : StoryElementary123
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

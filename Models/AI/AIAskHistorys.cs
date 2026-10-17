using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class AIAskHistorys : AIAskHistory
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

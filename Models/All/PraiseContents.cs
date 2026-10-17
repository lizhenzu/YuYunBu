using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PraiseContents : PraiseContent
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

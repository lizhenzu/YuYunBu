using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class QuestionTypeDataObjects : QuestionTypeDataObject
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

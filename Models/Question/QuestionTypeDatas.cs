using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class QuestionTypeDatas : QuestionTypeData
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

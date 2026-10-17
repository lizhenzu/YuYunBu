using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class MismatchClickHistoryForQuestions : MismatchClickHistoryForQuestion
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
    }
}

 
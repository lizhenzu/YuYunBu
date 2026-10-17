using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleQuizEnrollGroupInfos : BibleQuizEnrollGroupInfo
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

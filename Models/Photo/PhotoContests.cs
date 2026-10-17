using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PhotoContests : PhotoContest
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

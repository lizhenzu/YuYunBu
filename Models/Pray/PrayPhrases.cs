using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PrayPhrases : PrayPhrase
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
    }
}

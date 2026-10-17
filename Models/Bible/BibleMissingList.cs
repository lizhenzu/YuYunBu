using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleMissingList
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
        [Display(Name = "성경")]
        public string BibleTextToPray { get; set; }
        [Display(Name = "장")]
        public int BibleTextToPrayFromJang { get; set; }
        [Display(Name = "장")]
        public int Jang { get; set; }
    }
}
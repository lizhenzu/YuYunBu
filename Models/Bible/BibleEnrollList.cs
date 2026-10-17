using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleEnrollList
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
        [Display(Name = "성경")]
        public string BibleText { get; set; }
        [Display(Name = "장")]
        public int Jang { get; set; }
        [Display(Name = "절수")]
        public int DC { get; set; }
    }
}
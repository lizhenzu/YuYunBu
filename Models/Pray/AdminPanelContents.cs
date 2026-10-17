using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PrayerArrow
    {
        public int PrayerId { get; set; }


        [Range(1, 31)]
        [Display(Name = "일자(1~31)")]
        public int DayIndex { get; set; }


        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;


        [Required, StringLength(400)]
        [Display(Name = "1줄")]
        public string Line1 { get; set; } = string.Empty;


        [Required, StringLength(400)]
        [Display(Name = "2줄")]
        public string Line2 { get; set; } = string.Empty;


        [Required, StringLength(400)]
        [Display(Name = "3줄")]
        public string Line3 { get; set; } = string.Empty;


        public bool IsActive { get; set; } = true;


        public DateTime InsertDT { get; set; }
        public DateTime? UpdateDT { get; set; }
    }
}

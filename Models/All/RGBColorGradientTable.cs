using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class RGBColorGradientTable
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
        public int Id { get; set; }
        public string ColorName { get; set; }
        public string BackGroud1 { get; set; }
        public string BackGroud2 { get; set; }
        public string BackGroud3 { get; set; }
        public string FontColor { get; set; }      
    }
} 
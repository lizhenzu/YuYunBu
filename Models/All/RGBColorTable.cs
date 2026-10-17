using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class RGBColorTable
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
        public int Id { get; set; }
        public string ColorName { get; set; }
        public string BackGroud { get; set; }
        public string FontColor { get; set; }
        public string ColorType { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleLastReadRateByUsers : BibleLastReadRateByUser
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }

    }
}

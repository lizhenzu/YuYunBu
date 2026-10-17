using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleYouTuBuEnrollsList
    {
        public BibleBibleGroup _BibleBibleGroup { get; set; }
        
        public List<BibleYouTuBuEnrolls> _BibleYouTuBuEnrolls { get; set; }

        public BibleYouTuBuEnrollsList()
        {
            _BibleYouTuBuEnrolls = new List<BibleYouTuBuEnrolls>();
        }
    }
}

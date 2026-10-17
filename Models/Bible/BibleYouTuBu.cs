using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleYouTuBu : BibleYouTuBuBase
    {
        public int SeqNo { get; set; }
        public decimal ProgressRate { get; set; }
        public int BibleId { get; set; }        
        public string videoId { get; set; }        
    }
}

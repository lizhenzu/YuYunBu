using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PrayPhrasesDetail
    {
        public PrayPhrases PrayPhrasesData { get; set; }
        public eBibles eBiblesData { get; set; } 
    }
}

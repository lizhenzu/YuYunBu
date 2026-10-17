using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PrayPhrasesDetails
    { 
        public List<PrayPhrasesDetail> PrayPhrasesDetailList { get; set; } 
        public PrayPhrasesDetails() {
            PrayPhrasesDetailList = new List<PrayPhrasesDetail>();  
        }
    }
}

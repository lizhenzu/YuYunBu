using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class TodayCollectPrayPhrase
    {
        public PrayPhrase PrayPhraseData { get; set; }
        public eBibles TodayeBiblesData { get; set; }
        public eBibles TodayeBiblesToPrayData { get; set; }
        public TodayCheckCompositeData TodayCheckCompositeData { get; set; }

        public TodayCollectPrayPhrase()
        {
            PrayPhraseData = new PrayPhrase();
            TodayeBiblesData = new eBibles();
            TodayeBiblesToPrayData = new eBibles();
            TodayCheckCompositeData = new TodayCheckCompositeData();
        }
    }
}

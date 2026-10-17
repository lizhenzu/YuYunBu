using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class TodayCollectWordToRead
	{
        public TodayWordToRead TodayWordToReadData { get; set; }
        public eBibles TodayeBiblesData { get; set; }

        public TodayCheckCompositeData TodayCheckCompositeData { get; set; }

        public TodayCollectWordToRead()
        {
            TodayWordToReadData = new TodayWordToRead();
            TodayeBiblesData = new eBibles();
            TodayCheckCompositeData = new TodayCheckCompositeData();
        }
    }
}

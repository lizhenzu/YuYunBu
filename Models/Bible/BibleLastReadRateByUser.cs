using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class BibleLastReadRateByUser : view_UserCompositeInfo
    {
        public Decimal TotalProgressRate { get; set; }
        public DateTime ReadDate { get; set; }
        
    } 
}

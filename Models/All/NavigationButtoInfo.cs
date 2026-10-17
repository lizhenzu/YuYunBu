using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class NavigationButtoInfo
    {
        public string Title { get; set; }
        public int Total_All { get; set; }
        public TodayCheckCompositeData TodayCheckCompositeDataData { get; set; }
        
    }

}

using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class TodayPray
    {
        
        public int PrayID { get; set; }

        public string PrayTEXTData { get; set; }
        public string BibleText { get; set; }
        public string BibleTextData { get; set; }
        public string Subject { get; set; }
        public string publishYN { get; set; }        
        public DateTime ExposeDate { get; set; }
        public DateTime updateDT { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class BibleLastReadInfo
    {        
        public int BibleBookSerialId { get; set; }
        public int BibleBookId { get; set; }
        public string BibleText { get; set; }
        public int Jang { get; set; }
        public int Zul { get; set; }
        public DateTime ReadDate { get; set; }
    } 
}

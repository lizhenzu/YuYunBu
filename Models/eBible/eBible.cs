using System;
using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class eBible : eBibleBase
    {
        public string BibleKeyWord { get; set; }
        public string BibleSummary { get; set; }
        public int FromJang { get; set; }
        public int FromZul { get; set; }
        public int ToJang { get; set; }
        public int ToZul { get; set; }
        public string LangCD { get; set; }
        public string readDone { get; set; }
        public string bookmarkDone { get; set; }        
    }
} 
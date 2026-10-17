using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class eBibles
    {
        public List<eBible> eBibleDatas { get; set; }
        public string BibleText { get; set; } 
        public int FromJang { get; set; }
        public int FromZul { get; set; }
        public int ToJang { get; set; }
        public int ToZul { get; set; }
        public BibleBasicInfos2 _BibleBasicInfos2 { get; set; }
        public BibleBookOverviewDetail _BibleBookOverviewDetail { get; set; }

        public string JanZulString { get; set; }
        public string JanZulStringOnly { get; set; }
        public eBibles()
        {
            eBibleDatas = new List<eBible>();
            _BibleBasicInfos2 = new BibleBasicInfos2();
            _BibleBookOverviewDetail = new BibleBookOverviewDetail();
        }
    }
}

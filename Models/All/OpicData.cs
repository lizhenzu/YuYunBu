using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class OpicData
    {
        public int Id { get; set; }

        public int PageNo { get; set; }
        public string Question { get; set; } 
        public string Memo { get; set; }
        public string Vocabulary { get; set; }
        public string BodyContents { get; set; }

        [Display(Name = "요약1")]
        public string KeyWord1 { get; set; }
        [Display(Name = "요약2")]
        public string KeyWord2 { get; set; }
        [Display(Name = "요약3")]
        public string KeyWord3 { get; set; }
        public string KeyWord4 { get; set; }


        [Display(Name = "설명1")]
        public string KeyWord1Desc { get; set; }
        [Display(Name = "설명2")]
        public string KeyWord2Desc { get; set; }
        [Display(Name = "설명3")]
        public string KeyWord3Desc { get; set; }
        [Display(Name = "설명4")]
        public string KeyWord4Desc { get; set; }


        public int WorkVisited { get; set; }

        public int Visited { get; set; }
    }
}

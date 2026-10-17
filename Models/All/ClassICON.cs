using BulletinBoard.Common;
using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class ClassICON
    {
        public string ClassName { get; set; }
        public string DeptCode { get; set; }
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public RankQueryOption RankQueryOptionData { get; set; }

        public string imageURL { get; set; }   


    }
}
 
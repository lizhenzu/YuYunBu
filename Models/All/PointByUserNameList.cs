using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PointByUserNameList
    {
        public IEnumerable<PointByUserName> pointByUserNames { get; set; }
        public int FromPoint { get; set; }
        public int ToPoint { get; set; }
        public bool displayNone { get; set; }

        public PointByUserNameList()
        {
            pointByUserNames = new List<PointByUserName>();   
        }
    }
}

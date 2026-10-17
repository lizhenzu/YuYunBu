using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class CheckPoint
    {
        public int DC_Total { get; set; }
        public int AwardLevel { get; set; }
        public int ProductID { get; set; }
        public string UserId { get; set; }
    }
}
 
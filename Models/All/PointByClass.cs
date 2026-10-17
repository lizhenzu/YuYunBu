using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PointByClass
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; } 
        public int DC { get; set; }
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }      
    }
}
 
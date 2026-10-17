using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class AwardItemByUser : AwardItem
    {
        public string UserId { get; set; }
    }
} 
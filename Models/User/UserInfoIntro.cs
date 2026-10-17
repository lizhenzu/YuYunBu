using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulletinBoard.Models
{
    public class PrayFriend : UserInfoBase
    {
        public string PrayId { get; set; }
        public string PrayText { get; set; }
        public DateTime insertDT { get; set; }
    }
}
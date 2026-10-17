using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PrayFriendRequest
    {
        public int PrayId { get; set; }
        public string PrayText { get; set; }
    }
}
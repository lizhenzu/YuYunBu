using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class UserInfosPray : UserInfo
    {
        public string PersonPraying { get; set; }
        public string PrayerRecipient { get; set; }
        public int PrayCount { get; set; }
        public DateTime WhenPray { get; set; }
    }
}

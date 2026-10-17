using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class UserBannerPreference
    {
        public int Id { get; set; }
        public string GlobalClientKey { get; set; }
        public string ClientIp { get; set; }
        public string BannerCode { get; set; }
        public DateTime HideUntilUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

}

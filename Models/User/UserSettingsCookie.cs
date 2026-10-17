using System;

namespace BulletinBoard.Models
{
    public class UserSettingsCookie
    {
        public string UserId { get; set; } // NVARCHAR(1) -> string
        public string cookieName { get; set; } // NVARCHAR(1) -> string
        public string cookieValue { get; set; } // NVARCHAR(1) -> string
        public int cookieDays { get; set; } // NVARCHAR(1) -> string
    }
}


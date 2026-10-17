// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class GlobalCookieHideRequest
    {
        public string CookieKey { get; set; }   // 필수
        public string DurationCode { get; set; } // "today", "1week", "3week"
    }

 

}

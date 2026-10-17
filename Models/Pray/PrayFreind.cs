using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PrayFreind
    {
        public string UserId { get; set; }
        public string PrayText { get; set; }
        public string UserName { get; set; }
        public string WriteUserName { get; set; }
        public string WriteUserId { get; set; }
    }
}

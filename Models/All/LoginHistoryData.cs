using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class LoginHistoryData
    {
        public long sequenceNo { get; set; }
        public string UserName { get; set; }
        public int DC { get; set; }
        public DateTime minInsertDT { get; set; }
        public DateTime maxInsertDT { get; set; }
        public string FullName { get; set; }
        public string UserId { get; set; }
        public int ClassYear { get; set; }
        
        public int ClassBan { get; set; }
        public string RoleCode { get; set; }
        public string RoleNM { get; set; }
        public string ProfilePicturePath { get; set; }
    }
} 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleLastReadRateByUserSimple
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string UserId { get; set; }
        public string ProfilePicturePath { get; set; }
        public Decimal TotalProgressRate { get; set; }
        public DateTime ReadDate { get; set; }

        public int readBibleAllCount { get; set; }

        public string RoleCode { get; set; }
        public string RoleNM   { get; set; }
        public int    ClassYear { get; set; }
        public int    ClassBan  { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleLastReadRateByUserCompleted 
    {

        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string UserId { get; set; }
        public string ProfilePicturePath { get; set; }
        public int readBibleAllCount { get; set; }        
    }
}

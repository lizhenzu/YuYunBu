using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulletinBoard.Models
{
    public class PopMessage
    {
        public int Id { get; set; }
        public string MessageText { get; set; }
        public string readYN { get; set; }

        public string FromWhom { get; set; }
        public string FromWhomName { get; set; }
        public string FromWhomID { get; set; }

        public string ToWhom { get; set; }
        public string ToWhomName { get; set; }
        public string ToWhomID { get; set; }

        public int Visited { get; set; }
        [NotMapped]
        public string ProfilePicturePathFromWhom { get; set; }
        public string ProfilePicturePathToWhom { get; set; }

        public DateTime updateDT { get; set; }
    }
} 
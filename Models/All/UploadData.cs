using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class UploadData
    {
        public int UploadID { get; set; }
        public string CategoryData { get; set; }
        public string TEXTData { get; set; }
        public string URLData { get; set; }
        public bool checkData { get; set; }
    }
} 
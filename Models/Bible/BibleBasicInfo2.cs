using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleBasicInfo2 : BibleBasicInfo
    {
        public string OverView { get; set; }
        public string WhenWritten { get; set; }
        public string Author { get; set; }
        public string Purpose { get; set; }
        public string SummaryURL1 { get; set; }
        public string SummaryURL2 { get; set; }

    }
} 
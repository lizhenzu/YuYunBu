using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class view_NIV
    {
        public int Jang_K { get; set; }
        public int Jang_E { get; set; }
        public int Jang_N { get; set; }


        public string BibleText_K { get; set; }
        public string BibleText_E { get; set; }
        public string BibleText_N { get; set; }
        public int DC_K { get; set; }
        public int DC_E { get; set; }
        public int DC_N { get; set; }

    }
} 
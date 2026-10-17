using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleMissingListByLang
    {
        public string MA_BibleText { get; set; }
        public string KO_BibleText { get; set; }
        public string EN_BibleText { get; set; }
        public int MA_Jang { get; set; }
        public int KO_Jang { get; set; }
        public int EN_Jang { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleBulk
	{
        public string BibleTitle { get; set; }
        public int BibleJang { get; set; }
        public string BibleText { get; set; }
        public string LangCD { get; set; }
    }
}

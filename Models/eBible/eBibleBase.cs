using System;
using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class eBibleBase
    {
        public int Id { get; set; }
        public string BibleText { get; set; }
        public int Jang { get; set; }
        public int zul { get; set; }
        public string BibleData { get; set; }
    }
}  
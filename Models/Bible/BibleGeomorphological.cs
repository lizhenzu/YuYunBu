using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleGeomorphological
    {
        public int BibleGeomorphologicalID { get; set; }
        public string BibleGeomorphologicalText { get; set; }
        public string BibleGeomorphologicalURL { get; set; }
        public bool checkData { get; set; }
    }
}

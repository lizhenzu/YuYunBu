using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleGeomorphologicalWithID
    {
        public List<BibleGeomorphological> BibleGeomorphologicals { get; set; }

        public int WeeklyWordToReadID { get; set; } 

        public BibleGeomorphologicalWithID()
        {
            BibleGeomorphologicals = new List<BibleGeomorphological>(); 
        }
    }
}

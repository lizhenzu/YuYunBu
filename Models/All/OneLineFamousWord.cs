using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class OneLineFamousWord
    { 
        public int Id { get; set; }
        public string OneLineText { get; set; }
        public DateTime updateDT { get; set; }
    }
}

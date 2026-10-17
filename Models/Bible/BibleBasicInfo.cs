using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleBasicInfo
    {
        public int Id { get; set; }
        public string BibleText { get; set; }
        public int BibleJang { get; set; }        
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class view_KO_EN_SU
    {
        public int Id { get; set; }
        public string BibleText { get; set; }
        public string BibleText_EN { get; set; }
        public int BibleJang { get; set; }
        
        // eBible (Korean)
        public int Chapters_KO { get; set; }
        public int Verses_KO { get; set; }

        // eBibleEN (English)
        public int Chapters_EN { get; set; }
        public int Verses_EN { get; set; }

        // eBibleEN_NIV (NIV)
        public int Chapters_NIV { get; set; }
        public int Verses_NIV { get; set; }

        // eBibleSummary
        public int Summary_Count { get; set; }



    }
}
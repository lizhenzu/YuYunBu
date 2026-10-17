using System;

namespace BulletinBoard.Models
{
    public class UserSettings
    {
        public string LangCD { get; set; } // NVARCHAR(1) -> string
        public int PagesForView { get; set; } // NVARCHAR(1) -> string
        public DateTime QuestionWorkDate { get; set; } // NVARCHAR(1) -> string
        public string Notice_1_Collapse { get; set; } // NVARCHAR(1) -> string                
        public string BibleBookOverview { get; set; } // NVARCHAR(1) -> string                
    }
}

 
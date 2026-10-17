using System;

namespace BulletinBoard.Models
{
    public class AllStatusData
    {
        public DateTime DateBy { get; set; } // DATE -> DateTime
        public string word_ToMe { get; set; } // NVARCHAR(1) -> string
        public string word_ToParent { get; set; } // NVARCHAR(1) -> string
        public int word_Cnt { get; set; } // NVARCHAR(1) -> string
        public string quiz_result { get; set; } // NVARCHAR(1) -> string
        public int quiz_Cnt { get; set; } // NVARCHAR(1) -> string
        public string pray_ToMe { get; set; } // NVARCHAR(1) -> string
        public string pray_ToParent { get; set; } // NVARCHAR(1) -> string
        public int pray_Cnt { get; set; } // NVARCHAR(1) -> string
        public string Recite_yn { get; set; } // BIT -> bool
        public int Recite_Cnt { get; set; } // NVARCHAR(1) -> string
        public string chcekYN { get; set; } // NVARCHAR(10) -> string
    }
}

 
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class EnglishBibleQuizRank
    {
        public long sequenceNo { get; set; }
        public string userName { get; set; }
        public string HideName { get; set; }
        public string Id { get; set; }
        public int DC { get; set; }
        public int DC_Success { get; set; }
        public int DC_Fail { get; set; }
        public string ProfilePicturePath { get; set; }        
    }
}

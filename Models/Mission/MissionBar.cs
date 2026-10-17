using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class MissionBar
    {
        public string MissionText { get; set; }
        public string MissionURL { get; set; }
        public string MissionLangDec { get; set; }
        public MissionBar() {
            MissionLangDec = "한글";
        }
    }
}

using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.DailyMission2
{
    public class AwardPickGiftsVM
    {
        public int cYear { get; set; }
        public int AwardLevel { get; set; }
        public int AmountOfMoney { get; set; }     // AwardBase.AmountOfMoney
        public bool IsAdmin { get; set; }

        public List<AwardItemDTO> Items { get; set; } = new();
    }

   

}

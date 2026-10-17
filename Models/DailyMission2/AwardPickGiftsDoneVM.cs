using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.DailyMission2
{
    public class AwardPickGiftsDoneVM
    {
        public int cYear { get; set; }
        public bool IsAdmin { get; set; }
        public List<AwardClaimWithItemDTO> Claims { get; set; } = new();
    }
}

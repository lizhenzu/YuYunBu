// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.DailyMission2
{
    public class AwardDashboardHistoryRow : AwardDashboardRow
    {
        public DateTime? ClaimAt { get; set; } // AwardUserClaim에 있다고 가정(없으면 null)
    }

}

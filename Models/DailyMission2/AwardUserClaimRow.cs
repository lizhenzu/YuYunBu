// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.DailyMission2
{
    public class AwardUserClaimRow
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public int cYear { get; set; }
        public int AwardLevel { get; set; }
        public int? AwardBaseId { get; set; }
        public string Status { get; set; } = "Eligible";
        public int? SelectedProductID { get; set; }
        public DateTime? SelectedDT { get; set; }
        public DateTime InsertDT { get; set; }
        public DateTime UpdateDT { get; set; }
    }

}

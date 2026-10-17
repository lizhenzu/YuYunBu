using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.DailyMission2
{



    public class AwardClaimWithItemDTO
    {
        public int AwardLevel { get; set; }
        public string Status { get; set; } = "";
        public int? SelectedProductID { get; set; }
        public DateTime? SelectedDT { get; set; }

        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public int? UnitPrice { get; set; }        // 관리자만 표시
    }
}

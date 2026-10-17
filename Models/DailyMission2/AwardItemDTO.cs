using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.DailyMission2
{
    public class AwardItemDTO
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string? ProductDescription { get; set; }
        public int UnitPrice { get; set; }         // 관리자만 표시
    }
}

// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.DailyMission2
{
    public class AwardPopupResult
    {
        public bool HasNewAwards { get; set; }
        public string? LevelsText { get; set; }   // "1000, 1500"
        public string? AwardUrl { get; set; }     // 이동 URL
        public string? Message { get; set; }      // 팝업 메시지
    }


}

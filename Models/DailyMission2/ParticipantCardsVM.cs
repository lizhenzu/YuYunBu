using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.DailyMission2
{
    public class ParticipantCardsVM
    {
        public int Year { get; set; }
        public int TargetPoint { get; set; }               // 달성율 기준 목표점수
        public string CurrentUserName { get; set; } = "";  // ✅ 추가
        public List<ParticipantCardRow> Rows { get; set; } = new();
    }
}

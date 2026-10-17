// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class PresentBirthGiftVM
    {
        public PresentItemPick? MyPick { get; set; }

        // 기존 선물 리스트 (기존 AwardItem 그대로 사용)
        public List<AwardItem> Items { get; set; } = new();
    }
}

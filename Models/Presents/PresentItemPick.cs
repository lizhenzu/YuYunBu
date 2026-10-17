// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class PresentItemPick
    {
        public int PickId { get; set; }
        public string UserId { get; set; } = "";
        public int ProductId { get; set; }

        public string ProductName { get; set; } = "";
        public string? ProductDescription { get; set; }
        public string? SourceType { get; set; }        
        public DateTime PickedAt { get; set; }
        public bool IsActive { get; set; }

        public bool IsPurchased { get; set; }
        public DateTime? PurchasedAt { get; set; }
    }
}

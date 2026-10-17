// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class ActivePickInfo
    {
        public long PickId { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = "";
        public string ProductDescription { get; init; } = "";
        public string SourceType { get; init; } = "";        
        public bool IsSent { get; init; }
        public DateTime? SentAt { get; init; }
        public bool IsPurchased { get; init; }
        public DateTime? PurchasedAt { get; init; }
    }

}

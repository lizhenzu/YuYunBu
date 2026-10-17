// Models/PlaceItem.cs
using System;

namespace BulletinBoard.Models
{
    public class PlaceItem
    {
        public int PlaceId { get; set; }
        public string Category { get; set; } = "";
        public string Name { get; set; } = "";
        public string ExternalUrl { get; set; } = "";
        public string? Address { get; set; }
        public string? Tags { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime InsertDT { get; set; }
        public DateTime? UpdateDT { get; set; }
    }
}

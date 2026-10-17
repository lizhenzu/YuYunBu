// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class Album
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CoverPhotoId { get; set; }
        public Photo CoverPhoto { get; set; }
        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    }
}

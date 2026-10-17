using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    
    public sealed class BoardReply
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string Content { get; set; } = "";
        public string Writer { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
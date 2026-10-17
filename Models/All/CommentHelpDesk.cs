using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class CommentHelpDesk
    {
        public string UserName { get; set; }

        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }
    }
} 
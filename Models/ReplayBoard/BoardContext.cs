using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public sealed class BoardContext
    {
        public string BoardName { get; set; } = "";
        public string PostTable { get; set; } = "";
        public string ReplyTable { get; set; } = "";

        // ✅ 추가
        public string FileTable { get; set; } = "";
    }
}
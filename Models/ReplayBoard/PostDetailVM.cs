using System;
using System.ComponentModel.DataAnnotations;
using BulletinBoard.Models;
 
namespace BulletinBoard.Models
{ 
    public sealed class PostDetailVM
    {
        public BoardContext Board { get; set; } = new();
        public BoardPost Post { get; set; } = new();
        public List<BoardReply> Replies { get; set; } = new();

        // 댓글 작성 폼 바인딩
        public string ReplyWriter { get; set; } = "";
        public string ReplyContent { get; set; } = "";
    }
}
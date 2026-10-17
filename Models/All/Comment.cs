using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int BoardId { get; set; }
        public int? ParentCommentId { get; set; }
        public string Content { get; set; }
        public string UserName { get; set; }

        
        public DateTime CreatedDate { get; set; }

        public Board Board { get; set; }
        public Comment ParentComment { get; set; }

        // 대댓글 리스트를 포함하는 속성
        public List<Comment> ChildComments { get; set; }
    }
}

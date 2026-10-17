using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BoardViewModel
    {
        public Board Board { get; set; }
        public List<Comment> Comments { get; set; }
    }

}

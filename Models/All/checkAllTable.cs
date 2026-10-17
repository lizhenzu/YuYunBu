using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class checkAllTable
    {
      
        public string Kind { get; set; } 
        public string Content { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }

     
    }
}

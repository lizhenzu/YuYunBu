using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class ClickHistory
    {
        public string UserName { get; set; }
        public string Kind { get; set; }
    }
}
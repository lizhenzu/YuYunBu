using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class MissionCountInformation
    {
        public int Id { get; set; }
        public int TotalCount { get; set; }
        public int CountForId { get; set; }
    }
}

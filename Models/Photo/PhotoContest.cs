using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PhotoContest
    {
        public int Id { get; set; }
        public string SummaryText { get; set; }
        public string FullText { get; set; }
        public string URLData { get; set; }
        public int visited { get; set; }
    }
}

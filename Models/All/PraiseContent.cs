using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PraiseContent
    {
        public int Id { get; set; }

        public string contentsURL { get; set; }
        public string contentsText { get; set; }

        public string Kind { get; set; }
        public string Singer { get; set; }
    }
}

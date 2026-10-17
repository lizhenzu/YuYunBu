using System;

namespace BulletinBoard.Models
{
    public class RepairHomePageData
    {
        public int Id { get; set; }
        public string RequestContents { get; set; }
        public string AnswerContents { get; set; }
        public string UserName { get; set; }
        public DateTime InsertDT { get; set; }
    }
}

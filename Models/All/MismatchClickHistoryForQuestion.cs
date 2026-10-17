using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class MismatchClickHistoryForQuestion
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }

        public string UserName { get; set; }
        public string FullName { get; set; }

        public string AnswerQ { get; set; }
        public string CorrectAnswer { get; set; }

        public int TryCnt { get; set; }

        public DateTime ClickDate { get; set; }
        public DateTime InsertDT { get; set; }
    }
}

 
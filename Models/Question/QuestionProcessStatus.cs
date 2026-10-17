using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class QuestionProcessStatus
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }

        public string UserName { get; set; }
        public string AnswerQ { get; set; }
        public string CorrectAnswer { get; set; }
        public int TryCnt { get; set; }
        public string Result { get; set; }

        public DateTime ClickDate { get; set; }		
	}
}

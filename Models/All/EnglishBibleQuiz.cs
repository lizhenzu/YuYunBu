using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class EnglishBibleQuiz
    {
        public long sequenceNo { get; set; }
        public int Id { get; set; }
        public DateTime ExposeDate { get; set; }
        public string QuestionQ { get; set; }
        public string Question1 { get; set; }
        public string Question2 { get; set; }
        public string Question3 { get; set; }
        public string Question4 { get; set; }

        public string QuestionQ_KR { get; set; }
        public string Question1_KR { get; set; }
        public string Question2_KR { get; set; }
        public string Question3_KR { get; set; }
        public string Question4_KR { get; set; }


        public string CorrectAnswer { get; set; }
    }
}

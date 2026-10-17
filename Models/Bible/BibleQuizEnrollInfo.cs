using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BibleQuizEnrollInfo
    {
        public int Id { get; set; }
        public string QuestionType { get; set; }
        public string QuestionQ { get; set; }

        public string Question1 { get; set; }
        public string Question2 { get; set; }
        public string Question3 { get; set; }
        public string Question4 { get; set; }
        public string CorrectAnswer { get; set; }
        public string BibleText { get; set; } 
        public int Jang { get; set; }
        public int zul { get; set; }
    }
}
// , ,  , , 
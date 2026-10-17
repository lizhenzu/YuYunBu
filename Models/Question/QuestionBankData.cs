using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class QuestionBankData
    {


        //Id, QuestionType, QuestionQ, Question1, Question2, Question3, Question4, CorrectAnswer, BibleText, Jang, zul, QuestionTypeDesc, 
        public long sequenceNo { get; set; }

        public int Id { get; set; }
        public string QuestionType { get; set; }
        [Display(Name = "질문")]
        public string QuestionQ { get; set; }

        [Display(Name = "답변1")]
        public string Question1 { get; set; }
        [Display(Name = "답변2")]
        public string Question2 { get; set; }
        [Display(Name = "답변3")]
        public string Question3 { get; set; }
        [Display(Name = "답변4")]
        public string Question4 { get; set; }

        [Display(Name = "정답")]
        public string CorrectAnswer { get; set; }

        [Display(Name = "성경")]
        public string BibleText { get; set; }
        [Display(Name = "몇장")]
        public int Jang { get; set; }

        [Display(Name = "몇절")]
        public int Zul { get; set; }
        [Display(Name = "몇절")]
        public int Zul1 { get; set; }

        public string QuestionTypeDesc { get; set; }
        public string example1 { get; set; }
        public string example2 { get; set; }
        public string example3 { get; set; }
        public string example4 { get; set; }


        public DateTime updateDT { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class QuestionTypeData
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "날짜는 필수 입력 항목입니다.")]
        [DataType(DataType.Date)]
        [Display(Name = "시험날짜")]
        public DateTime ExposeDate { get; set; }

        public int DayData { get; set; }
        public string WeekDayKor { get; set; }

        [Required]
        [Display(Name = "질문유형")]
        public string QuestionType { get; set; }

        public string QuestionTypeDesc { get; set; }

        [Required]
        [Display(Name = "문제 배경")]
        public string QuestionBackground { get; set; }

        [Required]
        [Display(Name = "질문")]
        public string QuestionQ { get; set; }

        [Required]
        [Display(Name = "답변")]
        public string AnswerQ { get; set; }

        [Required]
        [Display(Name = "정답")]
        public string CorrectAnswer { get; set; }

        [Display(Name = "답변 가이드(힌트,형식) 기입")]
        public string AnswerGuide { get; set; }
        public string UserName { get; set; }

        public string BibleText { get; set; }
        public string appendedComments { get; set; }
        public int Jang { get; set; }
        public int Zul { get; set; }

    }
}

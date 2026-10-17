using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class QuestionTypeDataObject : QuestionTypeData
    {
        [Required]
        [Display(Name = "선택 #1")]
        public string Question1 { get; set; }

        [Required]
        [Display(Name = "선택 #2")]
        public string Question2 { get; set; }

        [Required]
        [Display(Name = "선택 #3")]
        public string Question3 { get; set; }

        [Required]
        [Display(Name = "선택 #4")]
        public string Question4 { get; set; }
    }
}

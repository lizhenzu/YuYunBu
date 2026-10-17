using BulletinBoard.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class FileUploadViewModel
    {

        [Required]
        [Display(Name = "카테고리1")]
        public Category SelectedCategory { get; set; }
        public IEnumerable<SelectListItem> Categorys { get; set; }

        [Display(Name = "Id")]
        public int  Id { get; set; }

        public int WeeklyWordToReadID { get; set; }

        [Display(Name = "파일 한글명")]
        public string FileKoreanName { get; set; }

        [Required]
        [Display(Name = "Year")]
        public int YYear { get; set; }

        [Required]
        [Display(Name = "Month")]
        public int MMonth { get; set; }
        [Required]
        [Display(Name = "Week")]
        public int WWeek { get; set; }

        [Required]
        [Display(Name = "파일 선택(1)")]
        public IFormFile UploadedFile1 { get; set; }      
        [Display(Name = "파일 선택(2)")]
        public IFormFile UploadedFile2 { get; set; }
        [Display(Name = "파일 선택(3)")]
        public IFormFile UploadedFile3 { get; set; }
        [Display(Name = "파일 선택(4)")]
        public IFormFile UploadedFile4 { get; set; }
        [Display(Name = "파일 선택(5)")]
        public IFormFile UploadedFile5 { get; set; }
        [Display(Name = "파일 선택(6)")]
        public IFormFile UploadedFile6 { get; set; }
        [Display(Name = "파일 선택(7)")]
        public IFormFile UploadedFile7 { get; set; }
    }
}


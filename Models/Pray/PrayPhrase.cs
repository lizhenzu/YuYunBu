using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PrayPhrase
    {
        public int Id { get; set; }
         
        [Required(ErrorMessage = "날짜는 필수 입력 항목입니다.")]
        [DataType(DataType.Date)]
        [Display(Name = "기도날짜")]
        public DateTime ExposeDate { get; set; }

        [Required]
        [Display(Name = "기도제목")]
        public string PrayTitle { get; set; }


        [Display(Name = "기도시작 멘트")]
        public string PraryBeginMesssage { get; set; }

    
        [Display(Name = "외울말씀(예,고전도전서)")]
        public string BibleTextToPray { get; set; }

        [Required]
        [Display(Name = "외울말씀 시작 절")]
        public int BibleTextToPrayFromJang { get; set; }

        [Required]
        [Display(Name = "외울말씀 시작 장")]
        public int BibleTextToPrayFromZul { get; set; }

        [Required]
        [Display(Name = "외울말씀 끝 장")]
        public int BibleTextToPrayToJang { get; set; }

        [Required]
        [Display(Name = "외울말씀 끝 절")]
        public int BibleTextToPrayToZul { get; set; }

        [Required]
        [Display(Name = "기도본문구절 #1")]
        public string PrayContent1 { get; set; }

        [Display(Name = "기도본문구절 #2")]
        public string PrayContent2 { get; set; }

        [Display(Name = "기도본문구절 #3")]
        public string PrayContent3 { get; set; }
         
        [Display(Name = "기도종료 멘트")]
        public string PraryEndMesssage { get; set; }
         
        [Display(Name = "기도할 대상자 이름")]
        public string PrayWrite { get; set; }

    }
}

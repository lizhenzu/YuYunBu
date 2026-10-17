using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class TodayWordToRead
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "게시날짜는 필수 입력 항목입니다.")]
        [DataType(DataType.Date)]
        [Display(Name = "게시날짜")]
        public DateTime ExposeDate { get; set; }

        [Required]
        [Display(Name = "말씀주제")]
        public string BibleSubject { get; set; }
        public string Synopsis { get; set; }

        [Display(Name = "말씀요약")]
        public string BibleSummary { get; set; }

        public int YouTuBuStartMM { get; set; }
        public int YouTuBuStartSS { get; set; }
        public int YouTuBuEndMM { get; set; }
        public int YouTuBuEndSS { get; set; }

        public int YouTuBuStartMM2 { get; set; }
        public int YouTuBuStartSS2 { get; set; }
        public int YouTuBuEndMM2 { get; set; }
        public int YouTuBuEndSS2 { get; set; }

        public int YouTuBuStartMM3 { get; set; }
        public int YouTuBuStartSS3 { get; set; }
        public int YouTuBuEndMM3 { get; set; }
        public int YouTuBuEndSS3 { get; set; }

        public int YouTuBuStartMM4 { get; set; }
        public int YouTuBuStartSS4 { get; set; }
        public int YouTuBuEndMM4 { get; set; }
        public int YouTuBuEndSS4 { get; set; }

        public int YouTuBuStartMM5 { get; set; }
        public int YouTuBuStartSS5 { get; set; }
        public int YouTuBuEndMM5 { get; set; }
        public int YouTuBuEndSS5 { get; set; }


        public string URL { get; set; }
        public int TimeMM { get; set; }
        public int TimeSS { get; set; }

        public string URL2 { get; set; }
        public int TimeMM2 { get; set; }
        public int TimeSS2 { get; set; }


        public string URL3 { get; set; }
        public int TimeMM3 { get; set; }
        public int TimeSS3 { get; set; }


        public string URL4 { get; set; }
        public int TimeMM4 { get; set; }
        public int TimeSS4 { get; set; }


        public string URL5 { get; set; }
        public int TimeMM5 { get; set; }
        public int TimeSS5 { get; set; }

        [Required]
        [Display(Name = "성경구절")]
        public string BibleText { get; set; }

        [Required]
        [Display(Name = "시작 장")]
        public int FromJang { get; set; }

        [Required]
        [Display(Name = "시작 절")]
        public int FromZul { get; set; }

        [Required]
        [Display(Name = "끝 장")]
        public int ToJang { get; set; }

        [Required]
        [Display(Name = "끝 절")]
        public int ToZul { get; set; }


    }
}

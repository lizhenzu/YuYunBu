using BulletinBoard.Common;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class ShortVideo
    {
        [Key]
        public int ShortId { get; set; }


        [Required, StringLength(16)]
        public string YoutubeId { get; set; } = string.Empty;
        public string VideoKind { get; set; } = "Life";


        [StringLength(200)]
        public string? Title { get; set; }


        public bool IsActive { get; set; } = true;
        public DateTime InsertDT { get; set; }
        public DateTime? UpdateDT { get; set; }
    }


    public class ShortVideoInput
    {
        public int? ShortId { get; set; }


        [Required(ErrorMessage = "유튜브 쇼츠 ID를 입력하세요."), StringLength(16)]
        public string YoutubeId { get; set; } = string.Empty;
        public string VideoKind { get; set; } = "Life";

        [StringLength(200)]
        public string? Title { get; set; }


        public bool IsActive { get; set; } = true;
    }

}

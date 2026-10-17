// Models/PlaceItemInput.cs  (등록/수정용 DTO)
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PlaceItemInput
    {
        public int? PlaceId { get; set; }

        [Required, Display(Name = "카테고리")]
        public string Category { get; set; } = "맛집";

        [Required, StringLength(200), Display(Name = "이름")]
        public string Name { get; set; } = "";

        [Required, Url, StringLength(500), Display(Name = "외부 링크(URL)")]
        public string ExternalUrl { get; set; } = "";

        [StringLength(300), Display(Name = "주소")]
        public string? Address { get; set; }

        [StringLength(200), Display(Name = "태그(쉼표구분)")]
        public string? Tags { get; set; }

        [Url, StringLength(500), Display(Name = "썸네일 URL(선택)")]
        public string? ThumbnailUrl { get; set; }

        [Display(Name = "사용")]
        public bool IsActive { get; set; } = true;
    }
}

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models.Presents
{
    public class AwardItemEditVM
    {
        public int ProductID { get; set; }

        [Required, StringLength(100)]
        public string ProductName { get; set; } = "";

        [StringLength(500)]
        public string? ProductDescription { get; set; }

        [Range(0, 10000000)]
        public int UnitPrice { get; set; }

        public bool IsHidden { get; set; }

        // ✅ SourceType: 카카오/쿠팡만 허용
        // - 화면에서는 select로 제한하지만, 서버에서도 한 번 더 검증(중요)
        [Required(ErrorMessage = "SourceType를 선택해 주세요.")]
        [StringLength(100)]
        [RegularExpression("^(카카오|쿠팡)$", ErrorMessage = "SourceType는 '카카오' 또는 '쿠팡'만 가능합니다.")]
        public string? SourceType { get; set; } = "카카오"; // ✅ 기본값(원하면 '쿠팡'으로 변경 가능)

        [StringLength(1000)]
        public string? ExternalUrl { get; set; }

        // crop 결과(정사각 jpeg)를 여기로 업로드
        public IFormFile? ImageFile { get; set; }
    }
}

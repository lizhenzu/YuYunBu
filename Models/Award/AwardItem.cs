using System;

namespace BulletinBoard.Models
{
    public class AwardItem
    {
        // DB: IDENTITY(1000,1) NOT NULL
        public int ProductID { get; set; }

        // DB: nvarchar(100) NOT NULL
        public string ProductName { get; set; } = "";

        // DB: nvarchar(500) NULL
        public string? ProductDescription { get; set; }

        // ✅ 요구사항: 숨김 여부 (DB에 bit 컬럼 추가)
        public bool IsHidden { get; set; }

        // DB: int NOT NULL
        public int UnitPrice { get; set; }

        // DB: datetime NULL DEFAULT(getdate())
        public DateTime? CreatedDate { get; set; }

        // DB: datetime NULL DEFAULT(getdate())
        public DateTime? ModifiedDate { get; set; }

        // DB: nvarchar(100) NULL DEFAULT('')
        public string? SourceType { get; set; }

        // DB: nvarchar(1000) NULL
        public string? ExternalUrl { get; set; }

        // ✅ 기존 코드 호환용(테이블에는 없으니 서비스에서는 건드리지 않음)
        public string? PointRequirement { get; set; }
    }
}

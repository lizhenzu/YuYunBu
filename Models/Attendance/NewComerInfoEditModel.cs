using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels.Attendance
{
    /// <summary>새친구 상세 페이지에서 기본정보(이름·연락처·생일·학년반·학교·출석유형) 수정용 모델</summary>
    public class NewComerInfoEditModel
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string? ChildName { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [Range(1, 12)]
        public int? BirthMonth { get; set; }

        [Range(1, 31)]
        public int? BirthDay { get; set; }

        [Range(1, 20)]
        public int? ClassYear { get; set; }

        [Range(1, 20)]
        public int? ClassBan { get; set; }

        [StringLength(200)]
        public string? School { get; set; }

        /// <summary>자진출석 여부 (true=자진출석, false=전도로 출석)</summary>
        public bool IsVoluntary { get; set; } = true;

        /// <summary>전도자 이름 (IsVoluntary=false 일 때 입력)</summary>
        [StringLength(100)]
        public string? EvangelistName { get; set; }
    }
}

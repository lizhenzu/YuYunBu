using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels.Attendance
{
    public class NewComerSaveModel
    {
        public int Id { get; set; }
        public int? PersonId { get; set; }

        [Required(ErrorMessage = "이름은 필수입니다.")]
        [StringLength(100)]
        public string ChildName { get; set; } = string.Empty;

        [Required(ErrorMessage = "첫 출석일은 필수입니다.")]
        public DateTime? FirstVisitDate { get; set; }

        [Required(ErrorMessage = "보호자명은 필수입니다.")]
        [StringLength(100)]
        public string ParentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "전화번호는 필수입니다.")]
        [StringLength(50)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "생년월은 필수입니다.")]
        [Range(1, 12)]
        public int? BirthMonth { get; set; }

        [Required(ErrorMessage = "생년일은 필수입니다.")]
        [Range(1, 31)]
        public int? BirthDay { get; set; }

        [Required(ErrorMessage = "학년은 필수입니다.")]
        [Range(1, 20)]
        public int? ClassYear { get; set; }

        [Required(ErrorMessage = "반은 필수입니다.")]
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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BulletinBoard.Models.Jubo
{
    public class JuboListViewModel
    {
        public List<JuboListItemViewModel> Items { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public int PublishedCount { get; set; }
        public int ThisMonthCount { get; set; }
    }

    public class JuboListItemViewModel
    {
        public int Id { get; set; }
        public DateTime JuboDate { get; set; }
        public string? Season { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TemplateType { get; set; } = "Normal";
        public bool IsPublished { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public int BlockCount { get; set; }
    }

    public class JuboCreateViewModel
    {
        [Required(ErrorMessage = "날짜를 선택해주세요.")]
        [Display(Name = "주일 날짜")]
        public DateTime JuboDate { get; set; } = GetNextSunday();

        [Display(Name = "절기")]
        [StringLength(100)]
        public string? Season { get; set; }

        [Required(ErrorMessage = "제목을 입력해주세요.")]
        [Display(Name = "주보 제목")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "템플릿")]
        public string TemplateType { get; set; } = "Normal";

        private static DateTime GetNextSunday()
        {
            var today = DateTime.Today;
            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7;
            return daysUntilSunday == 0 ? today : today.AddDays(daysUntilSunday);
        }
    }

    public class JuboEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public DateTime JuboDate { get; set; }

        [StringLength(100)]
        public string? Season { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string TemplateType { get; set; } = "Normal";
        public bool IsPublished { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BlockEditViewModel> Blocks { get; set; } = new();
    }

    public class BlockEditViewModel
    {
        public int Id { get; set; }
        public int JuboId { get; set; }
        public string BlockType { get; set; } = string.Empty;
        public string? BlockTitle { get; set; }
        public string? BlockContent { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; } = true;
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public bool IsRequired => JuboBlockTypes.RequiredTypes.Contains(BlockType);
    }

    public class JuboPreviewViewModel
    {
        public int Id { get; set; }
        public DateTime JuboDate { get; set; }
        public string? Season { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TemplateType { get; set; } = "Normal";
        public bool IsPublished { get; set; }
        public List<BlockEditViewModel> Blocks { get; set; } = new();
        public AttendanceSummaryViewModel? Attendance { get; set; }
        public List<AttendanceNewComerModel> NewComers { get; set; } = new();

        public BlockEditViewModel? GetBlock(string blockType)
            => Blocks.FirstOrDefault(b => b.BlockType == blockType && b.IsVisible);
    }

    public class StudentJuboViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
        public int YearPresentCount { get; set; }
        public bool IsNewComer { get; set; }
        public int NewComerVisitCount { get; set; }
    }

    public class AttendanceSummaryViewModel
    {
        public DateTime JuboDate { get; set; }
        public int WeeksCount { get; set; } = 1;
        public List<ClassAttendanceViewModel> Classes { get; set; } = new();
        public int TotalCount => Classes.Sum(c => c.TotalCount);
        public int TotalPresent => Classes.Sum(c => c.PresentCount);
        public int TotalNewComer => Classes.Sum(c => c.NewComerCount);
        public int TotalAbsent => TotalCount - TotalPresent;
    }

    public class ClassAttendanceViewModel
    {
        public int Id { get; set; }
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int PresentCount { get; set; }
        public int NewComerCount { get; set; }
        public string? Note { get; set; }
        public bool IsLocked { get; set; }
        public string? RecordedBy { get; set; }
        public DateTime RecordedAt { get; set; }
        public List<StudentJuboViewModel> Students { get; set; } = new();
    }

    public class AttendancePageViewModel
    {
        public int JuboId { get; set; }
        public DateTime JuboDate { get; set; }
        public string JuboTitle { get; set; } = string.Empty;
        public AttendanceSummaryViewModel Summary { get; set; } = new();
        public List<AttendanceNewComerModel> NewComers { get; set; } = new();
    }

    public class ClassAttendanceSaveItem
    {
        public int Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int PresentCount { get; set; }
        public int NewComerCount { get; set; }
        public string? Note { get; set; }
    }

    public class SaveNewComerViewModel
    {
        public DateTime AttendanceDate { get; set; }

        [Required(ErrorMessage = "이름을 입력해주세요.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? Age { get; set; }

        [StringLength(50)]
        public string? Grade { get; set; }

        [StringLength(100)]
        public string? ParentName { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }
    }
}

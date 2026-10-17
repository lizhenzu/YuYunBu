using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels.Attendance
{
    public class NewComerVisitSaveModel
    {
        [Required]
        public int NewComerId { get; set; }

        [Required(ErrorMessage = "출석일은 필수입니다.")]
        public DateTime? VisitDate { get; set; }
    }
}
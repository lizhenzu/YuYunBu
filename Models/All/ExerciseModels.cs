using System.Collections.Generic;

namespace BulletinBoard.Models
{
    /* 목록용 */
    public class ExerciseSummary
    {
        public int ExerciseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public bool IsAnaerobic { get; set; }
        public string Difficulty { get; set; } = "";
        public string? Description { get; set; }
        public int? RepsMin { get; set; }
        public int? RepsMax { get; set; }
        public int? SetsMin { get; set; }
        public int? SetsMax { get; set; }
        public int? RestSecMin { get; set; }
        public int? RestSecMax { get; set; }
        public string? Tempo { get; set; }
        public string? PrescriptionNotes { get; set; }
    }

    /* 필터/페이징 */
    public class ExerciseFilter
    {
        public string? Name { get; set; }
        public string? Difficulty { get; set; }
        public string? Category { get; set; }
        public bool? IsAnaerobic { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /* 상세 최종 DTO */
    public class ExerciseDetail
    {
        public ExerciseDetail()
        {
            Basic = new List<Exercise>();
            Steps = new List<ExerciseStep>();
            Muscles = new List<ExerciseMuscle>();
            Benefits = new List<ExerciseBenefit>();
            KidsInfo = new List<ExerciseKids>();
        }

        public List<Exercise> Basic { get; set; }
        public List<ExerciseStep> Steps { get; set; }
        public List<ExerciseMuscle> Muscles { get; set; }
        public List<ExerciseBenefit> Benefits { get; set; }
        public List<ExerciseKids> KidsInfo { get; set; }
    }

    /* Basic 한 행 */
    public class Exercise
    {
        public int ExerciseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public bool IsAnaerobic { get; set; }
        public string Difficulty { get; set; } = "";
        public string? Description { get; set; }
        public int? RepsMin { get; set; }
        public int? RepsMax { get; set; }
        public int? SetsMin { get; set; }
        public int? SetsMax { get; set; }
        public int? RestSecMin { get; set; }
        public int? RestSecMax { get; set; }
        public string? Tempo { get; set; }
        public string? PrescriptionNotes { get; set; }
    }

    public class ExerciseStep
    {
        public int StepNo { get; set; }
        public string Phase { get; set; } = "";
        public string Title { get; set; } = "";
        public string Instruction { get; set; } = "";
    }

    public class ExerciseMuscle
    {
        public string MuscleName { get; set; } = "";
        public string? Region { get; set; }
        public string Role { get; set; } = "";
        public int? SortOrder { get; set; }
    }

    public class ExerciseBenefit
    {
        public string EffectText { get; set; } = "";
    }

    public class ExerciseKids
    {
        public string Audience { get; set; } = "";
        public string Explanation { get; set; } = "";
    }
}

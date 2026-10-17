using System;

namespace BulletinBoard.Models
{
    public class TeacherBookVM
    {
        public int Id { get; set; }
        public string LessonKey { get; set; }
        public int? LessonNumber { get; set; }
        public string LessonLabel { get; set; }
        public string LessonTitle { get; set; }
        public int? UnitNumber { get; set; }
        public string UnitTitle { get; set; }
        public string BibleTextRef { get; set; }
        public string MemoryVerseRef { get; set; }
        public string MemoryVerseText { get; set; }
        public string LearningObjective { get; set; }
        public string ApplicationText { get; set; }
        public string TeacherStudyTitle { get; set; }
        public string TeacherStudyContent { get; set; }
        public string UnitIntroContent { get; set; }
        public string LearnContent { get; set; }
        public string WordGlassesText { get; set; }
        public string LiveContent { get; set; }
        public string TeacherTip { get; set; }
        public string ClosingContent { get; set; }
        public string PrayerText { get; set; }
        public string Page1ImagePath { get; set; }
        public string Page2ImagePath { get; set; }
        public string Page3ImagePath { get; set; }
        public string Page4ImagePath { get; set; }
        public int AcademicYear { get; set; }
        public int Semester { get; set; }

        // 이전/다음 과 탐색용
        public int? PrevLessonNumber { get; set; }
        public int? NextLessonNumber { get; set; }
        public string PrevLessonKey { get; set; }
        public string NextLessonKey { get; set; }
    }
}

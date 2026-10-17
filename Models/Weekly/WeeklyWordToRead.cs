using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class WeeklyWordToRead
    {
        public int Id { get; set; }

        [Display(Name = "단원")]
        public int WeekTitle_DW { get; set; }
        public string WeekTitle_DW_Title { get; set; }
        [Display(Name = "과")]
        public int WeekTitle_GUA { get; set; }

        public string BibleTitle { get; set; }
        public string OnePoint { get; set; }
        [Display(Name = "학습요점")]
        public string LearningObjectives1 { get; set; }
        [Display(Name = "적용")]
        public string LearningObjectives2 { get; set; }
        public string LearningObjectives3 { get; set; }


        [Display(Name = "읽을말씀")]
        public string BibleTextToRead { get; set; }
        [Display(Name = "장(시작)")]
        public int BibleTextToReadFromJang { get; set; }
        [Display(Name = "절(시작)")]
        public int BibleTextToReadFromZul { get; set; }
        [Display(Name = "장(끝)")]
        public int BibleTextToReadToJang { get; set; }
        [Display(Name = "절(끝)")]
        public int BibleTextToReadToZul { get; set; }


        [Display(Name = "읽을말씀1")]
        public string BibleTextToRead_1 { get; set; }
        [Display(Name = "장(시작)")]
        public int BibleTextToReadFromJang_1 { get; set; }
        [Display(Name = "절(시작)")]
        public int BibleTextToReadFromZul_1 { get; set; }
        [Display(Name = "장(끝)")]
        public int BibleTextToReadToJang_1 { get; set; }
        [Display(Name = "절(끝)")]
        public int BibleTextToReadToZul_1 { get; set; }


        public string ReadAndWriteOneWord { get; set; }
        public string PrayText { get; set; }

        



        [Display(Name = "외울말씀")]
        public string BibleTextToRecite { get; set; }
        [Display(Name = "장(시작)")]
        public int BibleTextToReciteFromJang { get; set; }
        [Display(Name = "절(시작)")]
        public int BibleTextToReciteFromZul { get; set; }
        [Display(Name = "장(끝)")]
        public int BibleTextToReciteToJang { get; set; }
        [Display(Name = "절(끝)")]
        public int BibleTextToReciteToZul { get; set; }

		[Display(Name = "이번주 공과영상 URL")]
		public string YoJulSongURL { get; set; }
		[Display(Name = "이번주 공과영상 Text")]
		public string YoJulSongText { get; set; }

		[Display(Name = "성품카드 영상보기 URL")]
		public string CharcaterMovingURL { get; set; }
		[Display(Name = "성품카드 영상보기 Text")]
		public string CharcaterMovingURLText { get; set; }

        [Display(Name = "성품카드 Text 1")]
        public string Charcatercard1Text { get; set; }
        [Display(Name = "성품카드 Text 2")]
        public string Charcatercard2Text { get; set; }
        [Display(Name = "성품카드 Text 3")]
        public string Charcatercard3Text { get; set; }

        public string verseText1 { get; set; }
        public string verseText2 { get; set; }
        public string verseText3 { get; set; }
        public string verseText4 { get; set; }
        public string verseText5 { get; set; }
        public string WelcomeA { get; set; }
        public string WelcomeB { get; set; }
        public string WelcomeTeacherGuide { get; set; } 
        public string Adventure { get; set; }
        public string AdventureTeacherGuide { get; set; }
        public string ValueData1 { get; set; }
        public string ValueData2 { get; set; }
        public string ValueData3 { get; set; }
        public string ValueTeacherGuide { get; set; }
        public string EncourageStudyPoint1 { get; set; }
        public string EncourageStudyPoint2 { get; set; }
        public string EncourageTeacherGuide { get; set; }
        public string ThisWeekMission { get; set; }
        public string publishYn { get; set; }
        public string relativeImagePath1 { get; set; }
        public string relativeImagePath2 { get; set; }
        public string relativeImagePath3 { get; set; }


        public int visited { get; set; }
        [Display(Name = "시작")]
        public DateTime DateFrom { get; set; }
        [Display(Name = "종료")]
        public DateTime DateTo { get; set; }

        public WeeklyWordToRead()
        {

            BibleTitle = string.Empty;
            OnePoint = string.Empty;
            LearningObjectives1 = string.Empty;
            LearningObjectives2 = string.Empty;
            LearningObjectives3 = string.Empty;

            BibleTextToRead = string.Empty;

            BibleTextToRecite = string.Empty;

            YoJulSongURL = string.Empty;

            YoJulSongText = string.Empty;
            verseText1 = string.Empty;
            verseText2 = string.Empty;
            verseText3 = string.Empty;
            verseText4 = string.Empty;
            verseText5 = string.Empty;
            WelcomeA = string.Empty;
            WelcomeB = string.Empty;
            WelcomeTeacherGuide = string.Empty;
            Adventure = string.Empty;
            AdventureTeacherGuide = string.Empty;
            ValueData1 = string.Empty;
            ValueData2 = string.Empty;
            ValueData3 = string.Empty;
            ValueTeacherGuide = string.Empty;
            EncourageStudyPoint1 = string.Empty;
            EncourageStudyPoint2 = string.Empty;
            EncourageTeacherGuide = string.Empty;
            ThisWeekMission = string.Empty;
            publishYn = string.Empty;
            relativeImagePath1 = string.Empty;
            relativeImagePath2 = string.Empty;
            relativeImagePath3 = string.Empty;


        }
    }
}

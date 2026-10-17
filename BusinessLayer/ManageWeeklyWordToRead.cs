using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageWeeklyWordToRead
    {

        public List<UploadData> getUploadDataWeeklyWordToReadAssoc(Category CategoryData, int WeeklyWordToReadID)
        {
            ControlWeeklyWordToRead controlWeeklyWordToRead = new ControlWeeklyWordToRead();
            DataSet ds = controlWeeklyWordToRead.fetchUploadDataWeeklyWordToReadAssoc(CategoryData, WeeklyWordToReadID);
            List<UploadData> value = ds.Tables[0].ToList<UploadData>();
            return value;
        }

        public List<UploadData> getUploadDataWeeklyWordToReadAssoc2(Category CategoryData, int WeeklyWordToReadID)
        {
            ControlWeeklyWordToRead controlWeeklyWordToRead = new ControlWeeklyWordToRead();
            DataSet ds = controlWeeklyWordToRead.fetchUploadDataWeeklyWordToReadAssoc2(CategoryData, WeeklyWordToReadID);
            List<UploadData> value = ds.Tables[0].ToList<UploadData>();
            return value;
        }

        public List<WeeklyWordToReads> getWeeklyWordToRead(int WeeklyWordToReadID, bool activeData, DateTime workDT, int cYear)
        {
            ControlWeeklyWordToRead controlWeeklyWordToRead = new ControlWeeklyWordToRead();
            DataSet ds = controlWeeklyWordToRead.fetchWeeklyWordToRead(WeeklyWordToReadID, activeData, workDT, cYear);
            List<WeeklyWordToReads> value = ds.Tables[0].ToList<WeeklyWordToReads>();
            return value;
        }
        public void CreateWeeklyWordToRead(WeeklyWordToRead param)
        {
            ControlWeeklyWordToRead controlWeeklyWordToRead = new ControlWeeklyWordToRead();

            controlWeeklyWordToRead.CreateWeeklyWordToRead(param.WeekTitle_DW, param.WeekTitle_GUA, param.BibleTitle, param.OnePoint, param.LearningObjectives1, param.LearningObjectives2, param.LearningObjectives3,  param.BibleTextToRead, param.BibleTextToReadFromJang, param.BibleTextToReadFromZul, param.BibleTextToReadToJang, param.BibleTextToReadToZul, param.BibleTextToRead_1, param.BibleTextToReadFromJang_1, param.BibleTextToReadFromZul_1, param.BibleTextToReadToJang_1, param.BibleTextToReadToZul_1, param.BibleTextToRecite, param.BibleTextToReciteFromJang, param.BibleTextToReciteFromZul, param.BibleTextToReciteToJang, param.BibleTextToReciteToZul, param.YoJulSongURL, param.YoJulSongText, param.CharcaterMovingURL, param.CharcaterMovingURLText, param.Charcatercard1Text, param.Charcatercard2Text, param.Charcatercard3Text, param.verseText1, param.verseText2, param.verseText3, param.verseText4, param.verseText5, param.WelcomeA, param.WelcomeB, param.WelcomeTeacherGuide, param.Adventure, param.AdventureTeacherGuide, param.ValueData1, param.ValueData2, param.ValueData3, param.ValueTeacherGuide, param.EncourageStudyPoint1, param.EncourageStudyPoint2, param.EncourageTeacherGuide, param.ThisWeekMission, param.DateFrom, param.DateTo);
        }

        public void MakePublishYn(int Id, string publishYn)
        {
            ControlWeeklyWordToRead controlWeeklyWordToRead = new ControlWeeklyWordToRead();

            controlWeeklyWordToRead.MakePublishYn(Id, publishYn);
        }

        public void HitWeeklyWordToRead(int Id)
        {
            ControlWeeklyWordToRead controlWeeklyWordToRead = new ControlWeeklyWordToRead();
            controlWeeklyWordToRead.HitWeeklyWordToRead(Id);
        }
        public void DecreasetHitWeeklyWordToRead(int Id)
        {
            ControlWeeklyWordToRead controlWeeklyWordToRead = new ControlWeeklyWordToRead();
            controlWeeklyWordToRead.DecreasetHitWeeklyWordToRead(Id);
        }

        public void UpdateWeeklyWordToRead_RelativeImagePath(int Id, string uploadPathSub, string fileNameA, string fileNameB, string fileNameC)
        {
            ControlWeeklyWordToRead controlWeeklyWordToRead = new ControlWeeklyWordToRead();

            controlWeeklyWordToRead.UpdateWeeklyWordToRead_RelativeImagePath(Id, uploadPathSub, fileNameA, fileNameB, fileNameC);
        }


        public void UpdateWeeklyWordToRead(WeeklyWordToRead param)
        {
            ControlWeeklyWordToRead controlWeeklyWordToRead = new ControlWeeklyWordToRead();

            controlWeeklyWordToRead.UpdateWeeklyWordToRead(param.Id, param.WeekTitle_DW, param.WeekTitle_GUA, param.BibleTitle, param.OnePoint, param.LearningObjectives1, param.LearningObjectives2, param.LearningObjectives3, param.BibleTextToRead, param.BibleTextToReadFromJang, param.BibleTextToReadFromZul, param.BibleTextToReadToJang, param.BibleTextToReadToZul, param.BibleTextToRead_1, param.BibleTextToReadFromJang_1, param.BibleTextToReadFromZul_1, param.BibleTextToReadToJang_1, param.BibleTextToReadToZul_1, param.BibleTextToRecite, param.BibleTextToReciteFromJang, param.BibleTextToReciteFromZul, param.BibleTextToReciteToJang, param.BibleTextToReciteToZul, param.YoJulSongURL, param.YoJulSongText, param.CharcaterMovingURL, param.CharcaterMovingURLText, param.Charcatercard1Text, param.Charcatercard2Text, param.Charcatercard3Text, param.verseText1, param.verseText2, param.verseText3, param.verseText4, param.verseText5, param.WelcomeA, param.WelcomeB, param.WelcomeTeacherGuide, param.Adventure, param.AdventureTeacherGuide, param.ValueData1, param.ValueData2, param.ValueData3, param.ValueTeacherGuide, param.EncourageStudyPoint1, param.EncourageStudyPoint2, param.EncourageTeacherGuide, param.ThisWeekMission, param.DateFrom, param.DateTo);
        }


        public void DeleteWeeklyWordToRead(int Id)
        {
            ControlWeeklyWordToRead controlWeeklyWordToRead = new ControlWeeklyWordToRead();

            controlWeeklyWordToRead.DeleteWeeklyWordToRead(Id);
        }

    }
}

using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageUploadData
    {
        public void CreateUploadData(string CategoryData, string TEXTData, string URLData)
        {
            ControleUploadData controleUploadData = new ControleUploadData();
            controleUploadData.CreateUploadData(CategoryData, TEXTData, URLData);
        }

        public void DeleteUploadData(int UploadID)
        {
            ControleUploadData controleUploadData = new ControleUploadData();
            controleUploadData.DeleteUploadData(UploadID);
        }


        //public List<eBiblePeople> geteBiblePeople(string Keyword)
        //{
        //    ControleUploadData controleUploadData = new ControleUploadData();
        //    DataSet ds = null; controleUploadData.fetcheBiblePeople(Keyword);           

        //    List<eBiblePeople> value = ds.Tables[0].ToList<eBiblePeople>();

        //    return value;
        //}
        public void CreateUploadWeeklyWordToReadAssoc(string CategoryData, int WeeklyWordToReadID, List<UploadData> selectedUploadData)
        {
            ControleUploadData controleUploadData = new ControleUploadData();
            controleUploadData.CreateUploadWeeklyWordToReadAssoc(CategoryData, WeeklyWordToReadID, selectedUploadData);
        }


        public void InsertLastOne(int WeeklyWordToReadID)
        {
            ControleUploadData controleUploadData = new ControleUploadData();
            controleUploadData.InsertLastOne(WeeklyWordToReadID);
        }

        //public void CreatePrayPhrase(PrayPhrase PrayPhrase)
        //{
        //    ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();

        //    controlPrayPhrase.CreatePrayPhrase(PrayPhrase.ExposeDate, PrayPhrase.PraryBeginMesssage, PrayPhrase.PrayContent1, PrayPhrase.PrayContent2, PrayPhrase.PrayContent3, PrayPhrase.PraryEndMesssage, PrayPhrase.PrayTitle, PrayPhrase.BiblePhrase);
        //}

        //public void UpdatePrayPhrase(PrayPhrase PrayPhrase)
        //{
        //    ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();

        //    controlPrayPhrase.UpdatePrayPhrase(PrayPhrase.Id, PrayPhrase.ExposeDate, PrayPhrase.PraryBeginMesssage, PrayPhrase.PrayContent1, PrayPhrase.PrayContent2, PrayPhrase.PrayContent3, PrayPhrase.PraryEndMesssage, PrayPhrase.PrayTitle, PrayPhrase.BiblePhrase);
        //}

        //public void DeletePrayPhrase(int Id)
        //{
        //    ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();

        //    controlPrayPhrase.DeletePrayPhrase(Id);
        //}

    }
}

using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManagePrayPhrase
    {
        public List<PrayPhrases> getPrayPhrases(int Id, bool activeData, DateTime workDT)
        {
            ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();
            DataSet ds = controlPrayPhrase.fetchPrayPhrase(Id, activeData, workDT);
            List<PrayPhrases> value = ds.Tables[0].ToList<PrayPhrases>();
            return value;
        }
        public void CreatePrayPhrase(PrayPhrase param)
        {
            ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();

            controlPrayPhrase.CreatePrayPhrase(param.ExposeDate, param.PrayTitle, param.PraryBeginMesssage, param.BibleTextToPray, param.BibleTextToPrayFromJang, param.BibleTextToPrayFromZul, param.BibleTextToPrayToJang, param.BibleTextToPrayToZul, param.PrayContent1, param.PrayContent2, param.PrayContent3, param.PraryEndMesssage);
        }

        public void UpdatePrayPhrase(PrayPhrase param)
        {
            ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();

            controlPrayPhrase.UpdatePrayPhrase(param.Id, param.ExposeDate, param.PrayTitle, param.PraryBeginMesssage, param.BibleTextToPray, param.BibleTextToPrayFromJang, param.BibleTextToPrayFromZul, param.BibleTextToPrayToJang, param.BibleTextToPrayToZul, param.PrayContent1, param.PrayContent2, param.PrayContent3, param.PraryEndMesssage);
        }

        public void DeletePrayPhrase(int Id)
        {
            ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();

            controlPrayPhrase.DeletePrayPhrase(Id);
        }
        public void RestorPrayPhrase(int YYear, int MMonth)
        {
            ControlPrayPhrase controlPrayPhrase = new ControlPrayPhrase();

            controlPrayPhrase.RestorPrayPhrase(YYear, MMonth);
        }

    }
}

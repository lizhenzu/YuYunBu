using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageeBible
    {
        public eBibles geteBible2(string BibleText, int FromJang, int FromZul, int ToJang, int ToZul, string langCD, string UserId)
        {
            ControleBible controleBible = new ControleBible();
            DataSet ds = controleBible.fetcheBible2(BibleText, FromJang, FromZul, ToJang, ToZul, langCD, UserId);

            eBibles eBibles = new eBibles();
            eBibles.BibleText = BibleText;
            eBibles.FromJang = FromJang;
            eBibles.FromZul = FromZul;
            eBibles.ToJang = ToJang;
            eBibles.ToZul = ToZul;

            string bibleSerialJangZul = SharedComm.MakeBibleSerialJangZul(BibleText, FromJang, FromZul, ToJang, ToZul);

            if (!string.IsNullOrEmpty(bibleSerialJangZul))
            {
                eBibles.JanZulString = $"{BibleText} " + bibleSerialJangZul;
            }
            eBibles.JanZulStringOnly = SharedComm.MakeBibleSerialJangZul(BibleText, FromJang, FromZul, ToJang, ToZul);

            eBibles.eBibleDatas = ds.Tables[0].ToList<eBible>();


            ManageeBulkInputBible manageeBulkInputBible = new ManageeBulkInputBible();

            List<BibleBasicInfos2>  data = manageeBulkInputBible.getBibleBasicListByBibleText2(0, BibleText);

            if (data.Count > 0)
            {
                eBibles._BibleBasicInfos2 = data[0];
            }

            List<BibleBookOverviewDetail> _BibleBookOverviewDetail = manageeBulkInputBible.getBibleBasicListByBibleText3(0, BibleText);

            if (_BibleBookOverviewDetail.Count > 0)
            {
                eBibles._BibleBookOverviewDetail = _BibleBookOverviewDetail[0];
            }

            return eBibles;
        }

        public eBibles geteBible3(string BibleText, int Jang, string langCD, string UserId)
        {
            return geteBible2(BibleText, Jang, 0, Jang, 0, langCD, UserId);
        }

        //public int getMaxBibleID(string UserName)
        //{
        //    ControleBible controleBible = new ControleBible();
        //    return controleBible.fetchMaxBibleID(UserName);
        //}
        public int getMinBibleIDByBibleText(string BibleText, int Jang)
        {
            ControleBible controleBible = new ControleBible();
            return controleBible.fetchMinBibleIDByBibleText(BibleText, Jang,0);
        }

        //
        public void UpsertBibleStampRange(int CurrntBibleID, int CurrntBibleIDLoop, string UserName)
        {
            ControleBible controleBible = new ControleBible();
            controleBible.UpsertBibleStampRange(CurrntBibleID, CurrntBibleIDLoop, UserName);
        }


        //
        public bool InsertOrDeleteUserBibleProgress(string UserId, int VerseId)
        {
            ControleBible controleBible = new ControleBible();
            return (controleBible.InsertOrDeleteUserBibleProgress(UserId, VerseId));
        }

        //
        public bool InsertOrDeleteUserBookMark(string UserId, int VerseId)
        {
            ControleBible controleBible = new ControleBible();
            return (controleBible.InsertOrDeleteUserBookMark(UserId, VerseId));
        }

        public List<eBible2> getBibleDataByBibleID(int BibleBookSerialId, int PaesForView, string LangCD, string userId)
        {
            ControleBible controleBible = new ControleBible();

            DataSet ds = controleBible.FetchBibleDataByBibleID(BibleBookSerialId, PaesForView, LangCD, userId);

            List<eBible2> data = ds.Tables[0].ToList<eBible2>();

            // 데이터가 존재할 경우에만 필터링 진행
            if (data.Count > 0)
            {
                string firstBibleText = data[0].BibleText;
                data = data.Where(d => d.BibleText == firstBibleText).ToList();
            }

            return data;
        }


        public List<BibleYouTuBuBase> geteBibleYouTuBu(string BibleText, int Jang)
        {
            ControleBible controleBible = new ControleBible();
            DataSet ds = controleBible.fetcheBibleYouTuBu(BibleText, Jang);
            List<BibleYouTuBuBase> value = ds.Tables[0].ToList<BibleYouTuBuBase>();
            return value;
        }
        public List<eBibleBase> getBibleBaseJang(string BibleText, int Jang)
        {
            ControleBible controleBible = new ControleBible();
            DataSet ds = controleBible.fetcheBibleBaseJang(BibleText, Jang);
            List<eBibleBase> value = ds.Tables[0].ToList<eBibleBase>();
            return value;
        }

        public List<eBibleBase> geteBibleBaseVerseID(int VerseId)
        {
            ControleBible controleBible = new ControleBible();
            DataSet ds = controleBible.fetcheBibleBaseVerseID(VerseId);
            List<eBibleBase> value = ds.Tables[0].ToList<eBibleBase>();
            return value;
        }


        public List<eBibleBase> getBookMark(string UserId, int VerseId)
        {
            ControleBible controleBible = new ControleBible();
            DataSet ds = controleBible.fetchBookMark(UserId, VerseId);
            List<eBibleBase> value = ds.Tables[0].ToList<eBibleBase>();
            return value;
        }

        public void deleteBookMark(string UserId, string VerseId)
        {
            ControleBible controleBible = new ControleBible();
            controleBible.deleteBookMark(UserId, VerseId);
        }

        public void InsertUserBibleReadRange(string bibleText, int fromJang, int fromZul, int toJang, int toZul, string userId)
        {
            ControleBible controleBible = new ControleBible();
            controleBible.InsertUserBibleReadRange(bibleText, fromJang, fromZul, toJang, toZul, userId);
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

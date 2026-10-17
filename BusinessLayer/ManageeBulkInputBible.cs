using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Security.Policy;

namespace BulletinBoard.BusinessLayer
{
    public class ManageeBulkInputBible
	{
        

        public List<BibleMissingList> getBibleMissingList()
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleMissingList();
            List<BibleMissingList> value = ds.Tables[0].ToList<BibleMissingList>();
            return value;
        }


        public List<BibleMissingListByLang> getBibleMissingListByLang(string KO_BibleText)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleMissingListByLang(KO_BibleText);
            List<BibleMissingListByLang> value = ds.Tables[0].ToList<BibleMissingListByLang>();
            return value;
        }

        public List<view_KO_EN_SU> getBibleMissingListB_KO_EN_SU()
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleMissingListB_KO_EN_SU();
            List<view_KO_EN_SU> value = ds.Tables[0].ToList<view_KO_EN_SU>();
            return value;
        }
        public List<view_SU> getBibleMissingListB_SU(string BibleText)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleMissingListB_SU(BibleText);
            List<view_SU> value = ds.Tables[0].ToList<view_SU>();
            return value;
        }
        //
        public List<view_SU> getBibleMissingListB_KJV(string BibleText)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleMissingListB_KJV(BibleText);
            List<view_SU> value = ds.Tables[0].ToList<view_SU>();
            return value;
        }
        public List<view_NIV> getBibleMissingList_NIV(string BibleText)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleMissingList_NIV(BibleText);
            List<view_NIV> value = ds.Tables[0].ToList<view_NIV>();
            return value;
        }
        //

        public List<BibleMissingListByGroup> getBibleMissingListByGroupByLang()
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleMissingListByGroupByLang();
            List<BibleMissingListByGroup> value = ds.Tables[0].ToList<BibleMissingListByGroup>();
            return value;
        }
        public List<BibleEnrollList> getBibleEnrollList()
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleEnrollList();
            List<BibleEnrollList> value = ds.Tables[0].ToList<BibleEnrollList>();
            return value;
        }

        public List<BibleBasicInfos> getBibleBasicList()
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleBasicList();
            List<BibleBasicInfos> value = ds.Tables[0].ToList<BibleBasicInfos>();
            return value;
        }
        public List<BibleBibleGroup> getBibleGroupName(string ab)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleGroupName(ab);
            List<BibleBibleGroup> value = ds.Tables[0].ToList<BibleBibleGroup>();
            return value;
        }

        public List<BibleBibleGroup> getBibleGroupByName(string ab)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleGroupByName(ab);
            List<BibleBibleGroup> value = ds.Tables[0].ToList<BibleBibleGroup>();
            return value;
        }

        public List<BibleYouTuBuEnrolls> getBibleYouTuBuEnrollList(string ab, string userId)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleYouTuBuEnrollList(ab, userId);
            List<BibleYouTuBuEnrolls> value = ds.Tables[0].ToList<BibleYouTuBuEnrolls>();
            return value;
        }


        public List<BibleBasicInfos> getBibleBasicListByBibleText(int Id, string BibleText)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleBasicListByBibleText(Id, BibleText);
            List<BibleBasicInfos> value = ds.Tables[0].ToList<BibleBasicInfos>();
            return value;
        }
        public List<BibleBasicInfos2> getBibleBasicListByBibleText2(int Id, string BibleText)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleBasicListByBibleText2_(Id, BibleText);
            List<BibleBasicInfos2> value = ds.Tables[0].ToList<BibleBasicInfos2>();
            return value;
        }
        public List<BibleBookOverviewDetail> getBibleBasicListByBibleText3(int Id, string BibleText)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleBasicListByBibleText3_(Id, BibleText);
            List<BibleBookOverviewDetail> value = ds.Tables[0].ToList<BibleBookOverviewDetail>();
            return value;
        }


        

        public List<BibleYouTuBu> getBibleYouTuBuList(int Id,string BibleText, string UserId)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleYouTuBuList(Id, BibleText, UserId);
            List<BibleYouTuBu> value = ds.Tables[0].ToList<BibleYouTuBu>();
            return value;
        }

        public List<BibleQuizEnrollInfos> getBibleQuizEnrollList(string BibleText, int Jang)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleQuizEnrollList(BibleText, Jang);
            List<BibleQuizEnrollInfos> value = ds.Tables[0].ToList<BibleQuizEnrollInfos>();
            return value;
        }
        public void SaveOrUpdateBibleYouTuBu(BibleYouTuBu param)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            controlBulkInputBible.SaveOrUpdateBibleYouTuBu(param.BibleText, param.Jang, param.URL, param.TimeMM, param.TimeSS);
        }

        public void SaveBibleYouTubeHit(string videoId, string userName)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            controlBulkInputBible.SaveBibleYouTubeHit(videoId, userName);
        }


        public List<BibleNullInfo> getBibleNullList(string BibleText, int Jang)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleNullList(BibleText, Jang);
            List<BibleNullInfo> value = ds.Tables[0].ToList<BibleNullInfo>();
            return value;
        }

        //

        public List<BibleQuizEnrollGroupInfo> getBibleQuizEnrollGroupList(string BibleText)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            DataSet ds = controlBulkInputBible.fetchBibleQuizEnrollGroupList(BibleText);
            List<BibleQuizEnrollGroupInfo> value = ds.Tables[0].ToList<BibleQuizEnrollGroupInfo>();
            return value;
        }

        //

        public void CreateBulkInputBible(BibleBulk bibleBulk)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            controlBulkInputBible.CreateBulkInputBible(bibleBulk.BibleTitle, bibleBulk.BibleJang, bibleBulk.BibleText, bibleBulk.LangCD);

        }
        public void SyncENtoNIC()
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            controlBulkInputBible.SyncENtoNIC();

        }
        public void CreateBulkQuiz(BibleBulkQuiz param)
        {
            ControlBulkInputBible controlBulkInputBible = new ControlBulkInputBible();
            controlBulkInputBible.CreateBulkQuiz(param.SQLs);

        }

    
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

using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using BulletinBoard.Helpers;

namespace BulletinBoard.BusinessLayer
{
    public class ManageeCommon
    {
        public List<MissionMenuItem> GetMissionMenusByRole()
        {
            ControlCommon controlCommon = new ControlCommon();
            DataSet ds = controlCommon.FetchMissionMenusByRole();
            List<MissionMenuItem> value = ds.Tables[0].ToList<MissionMenuItem>();
            return value;
        }
        public List<VisitInfoData> getVisitInfo()
        {
            ControlCommon controlCommon = new ControlCommon();
            DataSet ds = controlCommon.fetcheVisitInfo();
            List<VisitInfoData> value = ds.Tables[0].ToList<VisitInfoData>();
            return value;
        }
        public List<CommonMetaData> getCommonMetaData()
        {
            ControlCommon controlCommon = new ControlCommon();
            DataSet ds = controlCommon.fetchCommonMetaData();
            List<CommonMetaData> value = ds.Tables[0].ToList<CommonMetaData>();
            return value;
        }

        public List<VisitDateInfoData> getVisitDateInfo()
        {
            ControlCommon controlCommon = new ControlCommon();
            DataSet ds = controlCommon.fetchVisitDateInfo();
            List<VisitDateInfoData> value = ds.Tables[0].ToList<VisitDateInfoData>();
            return value;
        }

        public string get_DB_NAME()
        {
            ControlCommon controlCommon = new ControlCommon();
            DataSet ds = controlCommon.fetch_DB_NAME();
            string DB_NAME = ds.Tables[0].Rows[0][0] as string;
            return DB_NAME;
        }


        public void DeleteClickHistory(string UserName)
        {
            ControlCommon controlCommon = new ControlCommon();
            TodayCheckCompositeData value = new TodayCheckCompositeData();

            controlCommon.DeleteClickHistory(UserName);
        }

        public List<AllStatusData> getAllStatusData(string UserName)
        {
            ControlCommon controlCommon = new ControlCommon();
            DataSet ds = controlCommon.fetchAllStatusData(UserName);
            List<AllStatusData> value = ds.Tables[0].ToList<AllStatusData>();
            return value;
        }
        public List<WeekStatusData> getWeekStatusData(string UserName, DateTime workDT, bool isMonth, int monthGap)
        {
            ControlCommon controlCommon = new ControlCommon();
            DataSet ds = controlCommon.fetchWeekStatusData(UserName, workDT, isMonth, monthGap);
            List<WeekStatusData> value = ds.Tables[0].ToList<WeekStatusData>();
            return value;
        }

        public TodayCheckCompositeData getTodayStatusData(string UserName, DateTime workDT)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                return null;
            }
            ControlCommon controlCommon = new ControlCommon();
            TodayCheckCompositeData value = new TodayCheckCompositeData();

            DataSet dsToday = controlCommon.fetchTodayStatusData(DateHelper.GetKoreaNow().Year, UserName, true, workDT);


            if (dsToday != null && dsToday.Tables.Count > 0 && dsToday.Tables[0].Rows.Count > 0)
            {
                value.TodayWordToReadCountToParent = int.Parse(dsToday.Tables[0].Rows[0]["TodayWordToReadToParent"].ToString());
                value.TodayWordToReadCountToMe = int.Parse(dsToday.Tables[0].Rows[0]["TodayWordToReadToMe"].ToString());
                value.WeeklyWordToReadCount = int.Parse(dsToday.Tables[0].Rows[0]["WeeklyWordToRead"].ToString());
                value.PrayPhraseCountToMe = int.Parse(dsToday.Tables[0].Rows[0]["PrayPhraseToMe"].ToString());
                value.PrayPhraseCountToParent = int.Parse(dsToday.Tables[0].Rows[0]["PrayPhraseToParent"].ToString());
                value.PrayPhraseCountToFriend = int.Parse(dsToday.Tables[0].Rows[0]["PrayPhraseToFriend"].ToString());
                value.QuestionCount = int.Parse(dsToday.Tables[0].Rows[0]["Question"].ToString());
            }

            DataSet dsTodayAddPoint = controlCommon.fetchTodayStatusDataForAddPoint(UserName, true, SharedComm.GetToday());
            value.QuestionCountAddPoint = int.Parse(dsTodayAddPoint.Tables[0].Rows[0]["Count"].ToString());

            DataSet dsAll = controlCommon.fetchTodayStatusData(DateHelper.GetKoreaNow().Year, UserName, false, workDT);


            if (dsAll != null && dsAll.Tables.Count > 0 && dsAll.Tables[0].Rows.Count > 0)
            {
                value.TodayWordToReadCountToParent_All = int.Parse(dsAll.Tables[0].Rows[0]["TodayWordToReadToParent"].ToString());
                value.TodayWordToReadCountToMe_All = int.Parse(dsAll.Tables[0].Rows[0]["TodayWordToReadToMe"].ToString());

                value.WeeklyWordToReadCount_All = int.Parse(dsAll.Tables[0].Rows[0]["WeeklyWordToRead"].ToString());

                value.PrayPhraseCountToMe_All = int.Parse(dsAll.Tables[0].Rows[0]["PrayPhraseToMe"].ToString());
                value.PrayPhraseCountToParent_All = int.Parse(dsAll.Tables[0].Rows[0]["PrayPhraseToParent"].ToString());
                value.PrayPhraseCountToFriend_All = int.Parse(dsAll.Tables[0].Rows[0]["PrayPhraseToFriend"].ToString());

                value.QuestionCount_All = int.Parse(dsAll.Tables[0].Rows[0]["Question"].ToString());
                value.SDC = int.Parse(dsAll.Tables[0].Rows[0]["SDC"].ToString());
            }

            value.Total_All = value.TodayWordToReadCountToParent_All + value.TodayWordToReadCountToMe_All + value.WeeklyWordToReadCount_All + value.PrayPhraseCountToMe_All + value.PrayPhraseCountToParent_All + value.PrayPhraseCountToFriend_All + value.QuestionCount_All + value.SDC;

            return value;
        }

        public List<UserSettings> fetchUserSettings(string Id)
        {
            ControlCommon controlCommon = new ControlCommon();
            DataSet ds = controlCommon.fetchUserSettings(Id);
            List<UserSettings> value = ds.Tables[0].ToList<UserSettings>();
            return value;
        }

        public void UpsertUserSettings(string ID, string? langCD = null, int? pagesForView = null, string? notice_1_Collapse = null, string? bibleBookOverview = null, DateTime? questionWorkDate = null)
        {
            ControlCommon controlCommon = new ControlCommon();
            controlCommon.UpsertUserSettings(ID, langCd: langCD, pagesForView: pagesForView, notice_1_Collapse: notice_1_Collapse, bibleBookOverview: bibleBookOverview, questionWorkDate: questionWorkDate);
        } 
    }
}

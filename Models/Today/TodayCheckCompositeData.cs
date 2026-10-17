using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class TodayCheckCompositeData
    {
        public int TodayWordToReadCountToMe { get; set; }
        public int TodayWordToReadCountToParent { get; set; }

        public int WeeklyWordToReadCount { get; set; }

        public int PrayPhraseCountToMe { get; set; }
        public int PrayPhraseCountToParent { get; set; }
        public int PrayPhraseCountToFriend { get; set; }

        public int QuestionCount { get; set; }
        public int QuestionCountAddPoint { get; set; }

        //public int TodayWordToReadCountToMe_Month { get; set; }
        //public int TodayWordToReadCountToParent_Month { get; set; }
        //public int WeeklyWordToReadCount_Month { get; set; }
        //public int PrayPhraseCountToMe_Month { get; set; }
        //public int PrayPhraseCountToParent_Month { get; set; }
        //public int PrayPhraseCountToFriend_Month { get; set; }
        //public int QuestionCount_Month { get; set; }

        public int TodayWordToReadCountToMe_All { get; set; }
        public int TodayWordToReadCountToParent_All { get; set; }

        public int WeeklyWordToReadCount_All { get; set; }

        public int PrayPhraseCountToMe_All { get; set; }
        public int PrayPhraseCountToParent_All { get; set; }
        public int PrayPhraseCountToFriend_All { get; set; }

        public int QuestionCount_All { get; set; }

        public int Total_All { get; set; }
        public int SDC { get; set; }


        public List<WeekStatusData> WeekStatusDatas { get; set; } 
        public TodayCheckCompositeData()
        {
            TodayWordToReadCountToMe = 0;
            TodayWordToReadCountToParent = 0;
            WeeklyWordToReadCount = 0;
            PrayPhraseCountToMe = 0;
            PrayPhraseCountToParent = 0;
            PrayPhraseCountToFriend = 0;
            QuestionCount = 0;
            SDC = 0;

            //TodayWordToReadCountToMe_Month = 0;
            //TodayWordToReadCountToParent_Month = 0;
            //WeeklyWordToReadCount_Month = 0;
            //PrayPhraseCountToMe_Month = 0;
            //PrayPhraseCountToParent_Month = 0;
            //PrayPhraseCountToFriend_Month = 0;
            //QuestionCount_Month = 0;

            TodayWordToReadCountToMe_All = 0;
            TodayWordToReadCountToParent_All = 0;
            WeeklyWordToReadCount_All = 0;
            PrayPhraseCountToMe_All = 0;
            PrayPhraseCountToParent_All = 0;
            PrayPhraseCountToFriend_All = 0;
            QuestionCount_All = 0;
            WeekStatusDatas = new List<WeekStatusData>();
        }
    }
}

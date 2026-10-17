using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageClickHistoryForPrayPhrase
    {
        public int CountClickHistoryForPrayPhrase(string UserName, int PrayPhraseId, bool TodayQuery, string ToWhom, DateTime workDT)
        {
            ControlClickHistoryForPrayPhrase controlClickHistory = new ControlClickHistoryForPrayPhrase();

            return controlClickHistory.CountClickHistoryForPrayPhrase(UserName, PrayPhraseId, TodayQuery, ToWhom, workDT);
        }

        public void CreateClickHistoryForPrayPhrase(string UserName , int PrayPhraseId, string ToWhom, DateTime workDT,int cYear)
        {
            ControlClickHistoryForPrayPhrase controlClickHistory = new ControlClickHistoryForPrayPhrase();

            controlClickHistory.CreateClickHistoryPrayPhrase(UserName, PrayPhraseId, ToWhom, workDT, cYear);
        }
    }
}

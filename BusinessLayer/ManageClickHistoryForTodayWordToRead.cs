using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageClickHistoryForTodayWordToRead
    {
        public int CountClickHistoryForTodayWordToRead(string UserName, int TodayWordToReadId, bool TodayQuery, string ToWhom, DateTime workDT)
        {
            ControlClickHistoryForTodayWordToRead controlClickHistory = new ControlClickHistoryForTodayWordToRead();

            return controlClickHistory.CountClickHistoryForTodayWordToRead(UserName, TodayWordToReadId, TodayQuery, ToWhom, workDT);
        }

        public void CreateClickHistoryForRecitedVers(string UserName , int TodayWordToReadId, string ToWhom, DateTime workDT, int cYear)
        {
            ControlClickHistoryForTodayWordToRead controlClickHistory = new ControlClickHistoryForTodayWordToRead();

            controlClickHistory.CreateClickHistoryTodayWordToRead(UserName, TodayWordToReadId, ToWhom, workDT, cYear);
        }
    }
}

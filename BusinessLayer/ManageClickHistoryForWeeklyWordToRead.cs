using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageClickHistoryForWeeklyWordToRead
    {
        public int CountClickHistoryForWeeklyWordToRead(string UserName, int WeeklyWordToReadId, bool todayQuery, DateTime workDT)
        {
            ControlClickHistoryForWeeklyWordToRead controlClickHistory = new ControlClickHistoryForWeeklyWordToRead();

            return controlClickHistory.CountClickHistoryForWeeklyWordToRead(UserName, WeeklyWordToReadId, todayQuery, workDT);
        }

        public void CreateClickHistoryForRecitedVers(string UserName , int WeeklyWordToReadId, DateTime workDT,int cYear)
        {
            ControlClickHistoryForWeeklyWordToRead controlClickHistory = new ControlClickHistoryForWeeklyWordToRead();

            controlClickHistory.CreateClickHistoryWeeklyWordToRead(UserName, WeeklyWordToReadId, workDT, cYear);
        }
    }
}

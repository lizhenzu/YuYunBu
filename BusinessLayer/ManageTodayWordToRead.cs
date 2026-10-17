using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageTodayWordToRead
    {        

        public List<TodayWordToReads> getTodayWordToRead(int Id, bool activeData, DateTime workDT)
        {
            ControlTodayWordToRead controlTodayWordToRead = new ControlTodayWordToRead();
            DataSet ds = controlTodayWordToRead.fetchTodayWordToRead(Id, activeData, workDT);
            List<TodayWordToReads> value = ds.Tables[0].ToList<TodayWordToReads>();
            return value;
        }
        

    }
}

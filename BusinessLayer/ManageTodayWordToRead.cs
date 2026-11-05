using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    /// <summary>퀴즈 달력에 보여 줄 그날 말씀 (제목 · 구절)</summary>
    public class MonthWordSummary
    {
        public string Subject { get; set; } = "";
        public string Range { get; set; } = "";
    }

    public class ManageTodayWordToRead
    {        
        /// <summary>그달 말씀을 "yyyy-MM-dd" 날짜별로 돌려준다. 같은 날이 여럿이면 첫 번째만 쓴다</summary>
        public Dictionary<string, MonthWordSummary> getMonthWordSummaries(DateTime workDT)
        {
            var result = new Dictionary<string, MonthWordSummary>();
            DataSet ds = new ControlTodayWordToRead().fetchMonthWordSummaries(workDT);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string date = Convert.ToDateTime(row["ExposeDate"]).ToString("yyyy-MM-dd");
                if (result.ContainsKey(date)) continue;

                result[date] = new MonthWordSummary
                {
                    Subject = row["BibleSubject"].ToString(),
                    Range = $"{row["BibleText"]} {row["FromJang"]}장 {row["FromZul"]}절 ~ {row["ToJang"]}장 {row["ToZul"]}절"
                };
            }
            return result;
        }

        public List<TodayWordToReads> getTodayWordToRead(int Id, bool activeData, DateTime workDT)
        {
            ControlTodayWordToRead controlTodayWordToRead = new ControlTodayWordToRead();
            DataSet ds = controlTodayWordToRead.fetchTodayWordToRead(Id, activeData, workDT);
            List<TodayWordToReads> value = ds.Tables[0].ToList<TodayWordToReads>();
            return value;
        }
        

    }
}

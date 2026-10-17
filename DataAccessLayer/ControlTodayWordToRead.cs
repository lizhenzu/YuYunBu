using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using BulletinBoard.Helpers;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlTodayWordToRead
    {

        public DataSet fetchTodayWordToRead(int TodayWordToReadId, bool activeData, DateTime workDT)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 5 ROW_NUMBER() OVER(ORDER BY  ExposeDate )  sequenceNo ,");
            sb.AppendFormat(" MA.Id, ExposeDate, BibleSubject,Synopsis,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" isnull(BibleSummary, '') BibleSummary, "); sb.Append(Environment.NewLine);

            //sb.AppendFormat(" Id_A, ExposeDate_A, BibleText_A, FromJang_A, FromZul_A, ToJang_A, ToZul_A, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" US.YouTuBuStartMM , US.YouTuBuStartSS , US.YouTuBuEndMM , US.YouTuBuEndSS , US.URL , US.TimeMM , US.TimeSS , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" US.YouTuBuStartMM2, US.YouTuBuStartSS2, US.YouTuBuEndMM2, US.YouTuBuEndSS2, US.URL2, US.TimeMM2, US.TimeSS2, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" US.YouTuBuStartMM3, US.YouTuBuStartSS3, US.YouTuBuEndMM3, US.YouTuBuEndSS3, US.URL3, US.TimeMM3, US.TimeSS3, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" US.YouTuBuStartMM4, US.YouTuBuStartSS4, US.YouTuBuEndMM4, US.YouTuBuEndSS4, US.URL4, US.TimeMM4, US.TimeSS4, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" US.YouTuBuStartMM5, US.YouTuBuStartSS5, US.YouTuBuEndMM5, US.YouTuBuEndSS5, US.URL5, US.TimeMM5, US.TimeSS5, "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" MA.BibleText, FromJang, FromZul, ToJang, ToZul  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[TodayWordToRead] MA "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join [view_TodayWordToRead_eBibleYouTuBu2] US  on MA.Id =  US.[Id_A] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            if (TodayWordToReadId > 0)
            {
                sb.AppendFormat(" and MA.[Id] =  {0} ", TodayWordToReadId); sb.Append(Environment.NewLine);
            }
            else if (activeData)
            {
                sb.AppendFormat($" and CONVERT(VARCHAR, ExposeDate, 23)   = '{workDT.ToString("yyyy-MM-dd")}'  "); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());

            DbLogger.LogDebugAsync($"YouTuBuStartMM4 : {sb.ToString()}");
            return ds;
        }


        public DataSet fetchTodayWordTo2222Read2(int TodayWordToReadId, bool activeData, DateTime workDT)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 5 ROW_NUMBER() OVER(ORDER BY  ExposeDate )  sequenceNo ,");
            sb.AppendFormat(" MA.Id, ExposeDate, BibleSubject,Synopsis,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" isnull(BibleSummary, '') BibleSummary, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" YouTuBuStartMM, YouTuBuStartSS, "); sb.Append(Environment.NewLine);

            sb.AppendFormat("   CASE "); sb.Append(Environment.NewLine);
            sb.AppendFormat("         WHEN YouTuBuEndMM IS NULL OR YouTuBuEndMM = 0 THEN TimeMM "); sb.Append(Environment.NewLine);
            sb.AppendFormat("         ELSE YouTuBuEndMM "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     END AS YouTuBuEndMM,"); sb.Append(Environment.NewLine);
            sb.AppendFormat("     CASE "); sb.Append(Environment.NewLine);
            sb.AppendFormat("         WHEN YouTuBuEndSS IS NULL OR YouTuBuEndSS = 0 THEN TimeSS "); sb.Append(Environment.NewLine);
            sb.AppendFormat("         ELSE YouTuBuEndSS "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     END AS YouTuBuEndSS,"); sb.Append(Environment.NewLine);


            sb.AppendFormat(" [URL], TimeMM, TimeSS, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" MA.BibleText, FromJang, FromZul, ToJang, ToZul  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[TodayWordToRead] MA "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join [eBibleYouTuBu] US on MA.BibleText = US.BibleText and MA.FromJang = US.Jang  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            if (TodayWordToReadId > 0)
            {
                sb.AppendFormat(" and MA.[Id] =  {0} ", TodayWordToReadId); sb.Append(Environment.NewLine);
            }
            else if (activeData)
            {
                sb.AppendFormat($" and CONVERT(VARCHAR, ExposeDate, 23)   = '{workDT.ToString("yyyy-MM-dd")}'  "); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
    }
}

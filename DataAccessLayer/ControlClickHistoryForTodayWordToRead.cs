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
    public class ControlClickHistoryForTodayWordToRead
    {

        public int CountClickHistoryForTodayWordToRead(string UserName, int TodayWordToReadId, bool TodayQuery, string ToWhom, DateTime workDT)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select count(*) from [dbo].[ClickHistoryForTodayWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and [UserName] = '{0}' ", UserName); sb.Append(Environment.NewLine);
            if (TodayWordToReadId > 0)
            {
                sb.AppendFormat(" and [TodayWordToReadId] = {0} ", TodayWordToReadId); sb.Append(Environment.NewLine);
            }
            if (TodayQuery)
            {
                sb.AppendFormat($" and CONVERT(VARCHAR, ClickDate, 23)   = CONVERT(VARCHAR, DATEADD(HOUR,{ConstatsCommon.timeDifference16},'{workDT.ToString("yyyy-MM-dd")}'), 23)  "); sb.Append(Environment.NewLine);
            }
            if(!string.IsNullOrEmpty(ToWhom))
            {
                sb.AppendFormat(" and [{0}] = 'y' ", ToWhom); sb.Append(Environment.NewLine);

            }
            return DataAccessModule.executeScalar(sb.ToString());
        }

        public void CreateClickHistoryTodayWordToRead(string UserName, int TodayWordToReadId, string ToWhom, DateTime workDT,int cYear)
        {
            if (!ConstatsCommon.addPoint)
                return;

            if (CountClickHistoryForTodayWordToRead(UserName, TodayWordToReadId, true, string.Empty,workDT) > 0)
            {
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(" insert into [dbo].[ClickHistoryForTodayWordToRead] "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
                sb.AppendFormat("   UserName, TodayWordToReadId, cYear, "); sb.Append(Environment.NewLine);
                sb.AppendFormat("      ClickDate, [UpdateDT], [InsertDT] "); sb.Append(Environment.NewLine);
                sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
                sb.AppendFormat($" values('{UserName}',{TodayWordToReadId}, {cYear}, "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"            CONVERT(DATE, DATEADD(HOUR,{ConstatsCommon.timeDifference16},'{workDT.ToString("yyyy-MM-dd")}')), "); sb.Append(Environment.NewLine);
				sb.AppendFormat($"                          '{DateHelper.GetKoreaToday().ToString("yyyy-MM-dd")}',                         "); sb.Append(Environment.NewLine);
				sb.AppendFormat($"                          '{DateHelper.GetKoreaToday().ToString("yyyy-MM-dd")}'                          "); sb.Append(Environment.NewLine);
				sb.AppendFormat($"        )"); sb.Append(Environment.NewLine);

                DbLogger.LogDebugAsync(sb.ToString());

                DataAccessModule.executeNonQuery(sb.ToString());
            }
            if(true)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(" update [dbo].[ClickHistoryForTodayWordToRead] "); sb.Append(Environment.NewLine);
				sb.AppendFormat("  set {0}        = 'y', ", ToWhom); sb.Append(Environment.NewLine);
				sb.AppendFormat($"      [UpdateDT] = '{DateHelper.GetKoreaToday().ToString("yyyy-MM-dd")}' "); sb.Append(Environment.NewLine);
				sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" and [UserName]          = '{0}' ", UserName); sb.Append(Environment.NewLine);
                sb.AppendFormat(" and [TodayWordToReadId] = {0} ", TodayWordToReadId); sb.Append(Environment.NewLine);
                DbLogger.LogDebugAsync(sb.ToString());
                DataAccessModule.executeNonQuery(sb.ToString());
            }
        }
    }
}

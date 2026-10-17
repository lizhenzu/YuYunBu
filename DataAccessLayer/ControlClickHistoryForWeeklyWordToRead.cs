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
    public class ControlClickHistoryForWeeklyWordToRead
    {

        public int CountClickHistoryForWeeklyWordToRead(string UserName, int WeeklyWordToReadId, bool todayQuery, DateTime workDT)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select count(*) from [dbo].[ClickHistoryForWeeklyWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and [UserName] = '{0}' ", UserName); sb.Append(Environment.NewLine);
            if (WeeklyWordToReadId > 0)
            {
                sb.AppendFormat(" and [WeeklyWordToReadId] = {0} ", WeeklyWordToReadId); sb.Append(Environment.NewLine);
            }
            if (todayQuery)
            {
                sb.AppendFormat($" and CONVERT(VARCHAR, ClickDate, 23)   = CONVERT(VARCHAR, DATEADD(HOUR,{ConstatsCommon.timeDifference16},'{workDT.ToString("yyyy-MM-dd")}'), 23)  "); sb.Append(Environment.NewLine);
            }
            return DataAccessModule.executeScalar(sb.ToString());
        }

        public void CreateClickHistoryWeeklyWordToRead(string UserName, int WeeklyWordToReadId, DateTime workDT,int cYear)
        {
            if (!ConstatsCommon.addPoint)
                return;
            if (CountClickHistoryForWeeklyWordToRead(UserName, WeeklyWordToReadId, true, workDT) > 0)
            {
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(" insert into [dbo].[ClickHistoryForWeeklyWordToRead] "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
                sb.AppendFormat("      UserName, WeeklyWordToReadId, cYear, "); sb.Append(Environment.NewLine);
                sb.AppendFormat("      ClickDate, [InsertDT] "); sb.Append(Environment.NewLine);
                sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
                sb.AppendFormat($" values('{UserName}',{WeeklyWordToReadId}, {cYear},"); sb.Append(Environment.NewLine);
                sb.AppendFormat($"            CONVERT(DATE, DATEADD(HOUR,{ConstatsCommon.timeDifference16},'{workDT.ToString("yyyy-MM-dd")}')), "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"                          '{DateHelper.GetKoreaToday().ToString("yyyy-MM-dd")}'                         "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"        )"); sb.Append(Environment.NewLine);
                DataAccessModule.executeNonQuery(sb.ToString());
            }
        }
    }
}

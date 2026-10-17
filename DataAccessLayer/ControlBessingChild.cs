using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BulletinBoard.Models;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlBessingChild
    {
        
        public DataSet fetchBessingChild(int Id, string ExposeDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  [updateDT] )  sequenceNo ,");
            sb.AppendFormat("  Id, ");

            sb.AppendFormat("     CASE DATENAME(WEEKDAY, convert(datetime, FromDate,20)) ");

            if (ConstatsCommon.timeDifference16 == 16)
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekEngToKorShort());
            }
            else
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekKorToKorShort());
            }
            sb.AppendFormat("     END AS FromDateWeekDayKor, ");


            sb.AppendFormat("     CASE DATENAME(WEEKDAY, convert(datetime, ToDate,20)) ");
            if (ConstatsCommon.timeDifference16 == 16)
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekEngToKorShort());
            }
            else
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekKorToKorShort());
            }
            sb.AppendFormat("     END AS ToDateWeekDayKor, ");
            sb.AppendFormat("  FromDate        , ToDate,   "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(PrayTitle,'')    PrayTitle    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(PrayText,'')     PrayText     , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(PrayText_Eng,'') PrayText_Eng , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(HolyWord,'')     HolyWord     , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" [visited], "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" [updateDT] "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from [dbo].[BessingChild] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);

            if (Id > 100)
            {
                 
            }
            else if (Id > 0 && Id < 100)
            {
                sb.AppendFormat(" and Id = {0} ", Id); sb.Append(Environment.NewLine);
            }
            else if (!string.IsNullOrEmpty(ExposeDate))
            {
                sb.AppendFormat($" and CONVERT(DATE,'{ExposeDate}') between [FromDate] and [ToDate] "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($" and  Id = day( DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()))   "); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }



        public void CreateBessingChild(string FromDate, string ToDate, string PrayTitle,string PrayText, string PrayText_Eng, string HolyWord)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" insert into [dbo].[BessingChild] (FromDate, ToDate, PrayTitle,  PrayText, PrayText_Eng, HolyWord, [InsertDT], [updateDT]) "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" Values(");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(FromDate).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(ToDate).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(PrayTitle).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(PrayText).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(PrayText_Eng).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(HolyWord).Trim()}', ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()),  ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())  ");
            sb.AppendFormat($" )"); sb.Append(Environment.NewLine);

            DataAccessModule.executeNonQuery(sb.ToString()); 
        }

      
        public void UpdateBessingChild(int Id, string FromDate, string ToDate, string PrayTitle,  string PrayText, string PrayText_Eng, string HolyWord)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update [dbo].[BessingChild] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set  ", FromDate.Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     FromDate     = '{0}', ", SharedComm.appendSpecialChar(FromDate).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     ToDate       = '{0}', ", SharedComm.appendSpecialChar(ToDate).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     PrayTitle    = '{0}', ", SharedComm.appendSpecialChar(PrayTitle).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     PrayText     = '{0}', ", SharedComm.appendSpecialChar(PrayText).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     PrayText_Eng = '{0}', ", SharedComm.appendSpecialChar(PrayText_Eng).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     HolyWord     = '{0}', ", SharedComm.appendSpecialChar(HolyWord).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    [updateDT]    = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id]    =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void HitBessingChild(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update [dbo].[BessingChild] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set visited = visited + 1 ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void DecreasetHitWeeklyWordToRead(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update [dbo].[BessingChild] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set visited = visited - 1 ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
        public void DeleteBessingChild(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[BessingChild] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}
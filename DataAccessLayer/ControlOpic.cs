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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlOpic
    {
        
        public DataSet fetchOpic(int Id, string ExposeDate, string sortBy, string query)
        {
            StringBuilder sb = new StringBuilder();

            if (sortBy == "WorkVisited")
            {
                sb.AppendFormat($" select top 400 ROW_NUMBER() OVER(ORDER BY [WorkVisited])  sequenceNo ,");
            }
            else if (sortBy == "Visited")
            {
                sb.AppendFormat(" select top 400 ROW_NUMBER() OVER(ORDER BY [Visited])  sequenceNo ,");
            }
            else
            {
                sb.AppendFormat(" select top 400 ROW_NUMBER() OVER(ORDER BY [PageNo] )  sequenceNo ,");
            }

            sb.AppendFormat("  Id, ");
            sb.AppendFormat("  PageNo, ");
            sb.AppendFormat("  isnull(Question    ,'') Question     , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(Memo   ,'') Memo    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(Vocabulary  ,'') Vocabulary   , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(BodyContents     ,'') BodyContents      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(KeyWord1     ,'') KeyWord1      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(KeyWord2     ,'') KeyWord2      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(KeyWord3     ,'') KeyWord3      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(KeyWord4     ,'') KeyWord4      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(KeyWord1Desc ,'') KeyWord1Desc  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(KeyWord2Desc ,'') KeyWord2Desc  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(KeyWord3Desc ,'') KeyWord3Desc  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(KeyWord4Desc ,'') KeyWord4Desc  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" [WorkVisited]                           , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" [visited]                                 "); sb.Append(Environment.NewLine);

            sb.AppendFormat($" from [dbo].[Opic] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);

          //  sb.AppendFormat(" and BodyContents like '%{0}%'", "배경"); sb.Append(Environment.NewLine);

            if (!string.IsNullOrEmpty(query))
            {
                sb.AppendFormat(" and ("); sb.Append(Environment.NewLine);
                sb.AppendFormat("      Question like '%{0}%'", query); sb.Append(Environment.NewLine);
                sb.AppendFormat(" or   BodyContents like '%{0}%'", query); sb.Append(Environment.NewLine);
                sb.AppendFormat(" or   KeyWord1Desc like '%{0}%'", query); sb.Append(Environment.NewLine);
                sb.AppendFormat(" or   KeyWord2Desc like '%{0}%'", query); sb.Append(Environment.NewLine);
                sb.AppendFormat(" or   KeyWord3Desc like '%{0}%'", query); sb.Append(Environment.NewLine);
                sb.AppendFormat(" or   KeyWord4Desc like '%{0}%'", query); sb.Append(Environment.NewLine);
                sb.AppendFormat("      )"); sb.Append(Environment.NewLine);
            }

            if (Id > 0)
            {
                sb.AppendFormat(" and Id = {0} ", Id); sb.Append(Environment.NewLine);
            }
             

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }



        public void CreateOpic(int PageNo, string Question,
            string KeyWord1, string KeyWord2, string KeyWord3, string KeyWord4,
            string KeyWord1Desc, string KeyWord2Desc, string KeyWord3Desc, string KeyWord4Desc, string Memo, string Vocabulary, string BodyContents)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" insert into [dbo].[Opic] (PageNo, Question, KeyWord1, KeyWord2, KeyWord3, KeyWord4, KeyWord1Desc, KeyWord2Desc,KeyWord3Desc,KeyWord4Desc, Memo, Vocabulary,BodyContents, UpdateDT, InsertDT) "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" Values(");
            sb.AppendFormat($" {PageNo}, ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(Question).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(KeyWord1).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(KeyWord2).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(KeyWord3).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(KeyWord4).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(KeyWord1Desc).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(KeyWord2Desc).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(KeyWord3Desc).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(KeyWord4Desc).Trim()}', "); 
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(Memo).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(Vocabulary).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(BodyContents).Trim()}', ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()),  ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())  ");
            sb.AppendFormat($" )"); sb.Append(Environment.NewLine);

            DataAccessModule.executeNonQuery(sb.ToString()); 
        }

      
        public void UpdateOpic(int Id, int PageNo, string Question,
            string KeyWord1, string KeyWord2, string KeyWord3, string KeyWord4, 
            string KeyWord1Desc, string KeyWord2Desc, string KeyWord3Desc, string KeyWord4Desc, string Memo, string Vocabulary, string BodyContents)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" update [dbo].[Opic] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set PageNo       =  {0} , ", PageNo); sb.Append(Environment.NewLine);
            sb.AppendFormat("     Question     = '{0}', ", SharedComm.appendSpecialChar(Question).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     KeyWord1 = '{0}', ", SharedComm.appendSpecialChar(KeyWord1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     KeyWord2 = '{0}', ", SharedComm.appendSpecialChar(KeyWord2).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     KeyWord3 = '{0}', ", SharedComm.appendSpecialChar(KeyWord3).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     KeyWord4 = '{0}', ", SharedComm.appendSpecialChar(KeyWord4).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     KeyWord1Desc = '{0}', ", SharedComm.appendSpecialChar(KeyWord1Desc).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     KeyWord2Desc = '{0}', ", SharedComm.appendSpecialChar(KeyWord2Desc).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     KeyWord3Desc = '{0}', ", SharedComm.appendSpecialChar(KeyWord3Desc).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     KeyWord4Desc = '{0}', ", SharedComm.appendSpecialChar(KeyWord4Desc).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     Memo    = '{0}', ", SharedComm.appendSpecialChar(Memo).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     Vocabulary   = '{0}', ", SharedComm.appendSpecialChar(Vocabulary).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     BodyContents      = '{0}', ", SharedComm.appendSpecialChar(BodyContents).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    [updateDT]   = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id]       =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void VisitedOpic(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" update [dbo].[Opic] "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" set [Visited]    = Visited + 1 ,                                            ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     [updateDT]   = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void WorkVisitedOpic(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" update [dbo].[Opic] "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" set [WorkVisited] = [WorkVisited] + 1  ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }


        public void DeleteOpic(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($" delete from [dbo].[Opic] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}
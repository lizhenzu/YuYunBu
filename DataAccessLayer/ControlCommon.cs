using BulletinBoard.Common;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using BulletinBoard.Common;
using System;
using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data;
using System.Reflection;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using BulletinBoard.Helpers;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlCommon
	{
        public DataSet FetchMissionMenusByRole()
        {
            StringBuilder sb = new StringBuilder();

            //  sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Jang, zul )  sequenceNo ,");
            sb.AppendFormat($" select * from dbo.MissionMenuConfig where [publishYN] = 'y' and expireDate > GETDATE()  order by [DisplayOrder] ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
        public DataSet fetcheVisitInfo()
        {
            StringBuilder sb = new StringBuilder();

            //  sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Jang, zul )  sequenceNo ,");
            sb.AppendFormat($" select * from [GetVisitInfo]({ConstatsCommon.timeDifference16}) ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
        public DataSet fetchCommonMetaData()
        {
            StringBuilder sb = new StringBuilder();

            //  sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Jang, zul )  sequenceNo ,");
            sb.AppendFormat($" select * from [CommonMetaData] ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
        public DataSet fetchVisitDateInfo()
        {
            StringBuilder sb = new StringBuilder();

            //  sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Jang, zul )  sequenceNo ,");
            sb.AppendFormat($" select * from [GetVisitDateInfo]() order by createddate desc ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
        public DataSet fetch_DB_NAME()
        {
            StringBuilder sb = new StringBuilder();

            //  sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Jang, zul )  sequenceNo ,");
            sb.AppendFormat($" SELECT DB_NAME() ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void DeleteClickHistory(string UserName)
        {
            if (!string.IsNullOrEmpty(UserName))
            {
                DataAccessModule.executeNonQuery($"usp_user_click_history '{UserName}'");
            }
        }

        public DataSet fetchAllStatusData(string UserName)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" select [DateBy], [word_ToMe], [word_ToParent], [word_Cnt], [quiz_result], [quiz_Cnt], [pray_ToMe], [pray_ToParent],[pray_Cnt], [Recite_yn], [Recite_Cnt], [chcekYN] "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" from [dbo].[ufn_GetUserClickHistory] ('{UserName}') "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  order by [DateBy] "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;

        }

        public DataSet fetchWeekStatusData(string UserName, DateTime workDT, bool isMonth, int monthGap)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select [DayByDay], [ClickDate], [WeekDayName] , isnull(DCount ,0) DCount, substring([DayByDay],1,4) YYYY, substring([DayByDay],6,2) MMM , substring([DayByDay],9,2) DDD , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("        CASE WHEN DCount > 0 THEN 'true' ELSE 'false' END AS boolDdata  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ( "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     select  CONVERT(varchar, [DayByDay], 23) [DayByDay]  ,[WeekDayName] "); sb.Append(Environment.NewLine);

            if (isMonth)
            {
                sb.AppendFormat($"  FROM dbo.GetMonthCalendarSerialList('{workDT.ToString("yyyy-MM-dd")}',{monthGap})  "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($"  FROM dbo.GetWeekCalendarSerialList('{workDT.ToString("yyyy-MM-dd")}')  "); sb.Append(Environment.NewLine);
            }

            sb.AppendFormat(" ) A "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join ( "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" 				select    [ClickDate],   DCount "); sb.Append(Environment.NewLine);
            if (isMonth)
            {
                sb.AppendFormat($"  from [GetMonthClickDayByUser]('{workDT.ToString("yyyy-MM-dd")}', '{UserName}', {monthGap} )"); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($"  from [GetWeekClickDayByUser]('{workDT.ToString("yyyy-MM-dd")}', '{UserName}' )"  ); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat(" 				) B on A.[DayByDay] = B.[ClickDate] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" order by [DayByDay] "); sb.Append(Environment.NewLine);

            DbLogger.LogDebugAsync(sb.ToString());

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetchTodayStatusData(int cYear, string UserName, bool queryToday, DateTime workDT)
        {
            StringBuilder sb = new StringBuilder();

            if (queryToday)
            {
                sb.AppendFormat($" select A.*, isnull( SDC, 0) SDC   from [dbo].[ufn_UserAreaPointsByDate_ByYear]('{UserName}', '{workDT.ToString("yyyy-MM-dd")}',{cYear}) A   "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($" select A.*, isnull( SDC, 0) SDC   from [dbo].[ufn_UserAreaPointsByDate_ByYear]('{UserName}',NULL,{cYear}) A   "); sb.Append(Environment.NewLine);

            }
            sb.AppendFormat($"   left outer join   "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" 	  (  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" 			 SELECT   "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" 				username AS userName,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" 				SUM(point) AS SDC,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" 				MIN(clickDate) AS A,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" 				MAX(clickDate) AS B  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" 			FROM ClickHistoryForPoint  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" 			GROUP BY username  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" 	  )	    b on a.UserName = b.userName  "); sb.Append(Environment.NewLine);
 
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

     
        public DataSet fetchTodayStatusDataForAddPoint(string UserName, bool queryToday, DateTime workDT)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("    SELECT  COUNT(*) AS Count           "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    FROM [dbo].[ClickHistoryForQuestion] CA                    "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    JOIN [dbo].[QuestionData] QD ON CA.[QuestionId] = QD.Id    "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    WHERE 1 = 1                                             "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    and LOWER(CA.[UserName]) = LOWER('{0}')       ", UserName); sb.Append(Environment.NewLine);
            sb.AppendFormat("    and       CA.[AnswerQ]   = CA.[CorrectAnswer]      "); sb.Append(Environment.NewLine);
            //sb.AppendFormat("    and   CONVERT(varchar,CA.InsertDT, 23) = CONVERT(varchar,GETDATE(), 23)    "); sb.Append(Environment.NewLine);
            if (queryToday)
            {
                sb.AppendFormat($"     and   CONVERT(VARCHAR, CA.InsertDT, 23) = '{DateHelper.GetKoreaToday().ToString("yyyy-MM-dd")}'   "); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat("    and   CONVERT(varchar,ClickDate, 23) != CONVERT(varchar,CA.InsertDT, 23)      "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetchUserSettings(string Id)
        {
            StringBuilder sb = new StringBuilder();

            //  sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Jang, zul )  sequenceNo ,");
            sb.AppendFormat($" select LangCD , isnull(PagesForView,3) PagesForView ,");
            sb.AppendFormat($"                 isnull([Notice_1_Collapse], 'n') Notice_1_Collapse,");
            sb.AppendFormat($"                 isnull([BibleBookOverview], 'n') BibleBookOverview,");
            sb.AppendFormat($"                 ISNULL(QuestionWorkDate, '{DateHelper.GetKoreaToday().ToString("yyyy-MM-dd")}') AS QuestionWorkDate ");
            sb.AppendFormat($" from  [dbo].[UserSettings] ");
            sb.AppendFormat($" where  Id = '{Id}' ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }


        public void UpsertUserSettings(string id, string? langCd = null, int? pagesForView = null, string? notice_1_Collapse = null, string? bibleBookOverview = null, DateTime? questionWorkDate = null)
        {
            if (langCd == null && pagesForView == null && notice_1_Collapse == null && bibleBookOverview == null && questionWorkDate == null)
                throw new ArgumentException("langCd, pagesForView, Notice_1_Collapse, bibleBookOverview, questionWorkDate 중 최소 하나는 지정해야 합니다.");

            const string sql = @"
                                    MERGE [dbo].[UserSettings] AS target
                                    USING (VALUES (@Id, @LangCD, @PagesForView, @Notice_1_Collapse, @BibleBookOverview, @QuestionWorkDate)) AS source ([Id], [LangCD], [PagesForView], [Notice_1_Collapse], [BibleBookOverview], [QuestionWorkDate])
                                        ON target.[Id] = source.[Id]
                                    WHEN MATCHED THEN
                                        UPDATE SET
                                            LangCD            = COALESCE(source.[LangCD], target.[LangCD]),
                                            PagesForView      = COALESCE(source.[PagesForView], target.[PagesForView]),
                                            Notice_1_Collapse = COALESCE(source.[Notice_1_Collapse], target.[Notice_1_Collapse]),
                                            BibleBookOverview = COALESCE(source.[BibleBookOverview], target.[BibleBookOverview]),
                                            [updateDT]        = GETDATE(),
                                            QuestionWorkDate  = COALESCE(source.[QuestionWorkDate], target.[QuestionWorkDate])
                                    WHEN NOT MATCHED THEN
                                        INSERT ([Id], [LangCD], [PagesForView], [Notice_1_Collapse], [BibleBookOverview], [QuestionWorkDate])
                                        VALUES (source.[Id], source.[LangCD], source.[PagesForView], source.[Notice_1_Collapse], source.[BibleBookOverview], source.[QuestionWorkDate]);";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@Id",               SqlDbType.NVarChar, 450) { Value = id },
                new SqlParameter("@LangCD",           SqlDbType.NVarChar,   2) { Value = (object?)langCd           ?? DBNull.Value },
                new SqlParameter("@PagesForView",     SqlDbType.Int)          { Value = (object?)pagesForView     ?? DBNull.Value },
                new SqlParameter("@Notice_1_Collapse",SqlDbType.NVarChar,1)          { Value = (object?)notice_1_Collapse     ?? DBNull.Value },
                new SqlParameter("@BibleBookOverview",SqlDbType.NVarChar,1)          { Value = (object?)bibleBookOverview     ?? DBNull.Value },
                new SqlParameter("@QuestionWorkDate", SqlDbType.DateTime)     { Value = (object?)questionWorkDate ?? DBNull.Value }
            };

            DataAccessModule.executeNonQuery(sql, parameters);
        }

    }
}

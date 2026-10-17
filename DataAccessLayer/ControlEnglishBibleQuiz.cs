using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlEnglishBibleQuiz
    {

        public DataSet fetchEnglishBibleQuiz(string UserName,int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Id asc)  sequenceNo ,");
            sb.AppendFormat(" Id, ExposeDate, ");
            sb.AppendFormat(" QuestionQ   , Question1   , Question2   , Question3   , Question4   , ");
            sb.AppendFormat(" QuestionQ_KR, Question1_KR, Question2_KR, Question3_KR, Question4_KR, ");
            sb.AppendFormat(" CorrectAnswer "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from [dbo].[EnglishBibleQuiz] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");

            if(Id> 0)
            {
                sb.AppendFormat($"and Id  = {Id} "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($"and id not in ( "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"                 select QuestionId  "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"			       from [dbo].[ClickHistoryForEnglishBibleQuiz]  "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"				   where [UserName] = '{UserName}'       "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"				   and  ( [AnswerQ] = [CorrectAnswer] ) "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"				 )                                       "); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetchEnglishBibleQuizCount()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select ROW_NUMBER() OVER(ORDER BY  ExposeDate asc)  sequenceNo ,");
            sb.AppendFormat(" Id, ExposeDate, ");
            sb.AppendFormat(" QuestionQ   , Question1   , Question2   , Question3   , Question4   , ");
            sb.AppendFormat(" QuestionQ_KR, Question1_KR, Question2_KR, Question3_KR, Question4_KR, ");
            sb.AppendFormat(" CorrectAnswer "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from [dbo].[EnglishBibleQuiz] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }


        public DataSet fetchEnglishBibleQuizRank()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select ROW_NUMBER() OVER(ORDER BY  DC desc)  sequenceNo ,");
            sb.AppendFormat(" userName, DC, DC_Success, DC_Fail, HideName, Id "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from view_EnglishBibleQuiz_Rank "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }



        public int fetchEnglishBibleQuizSuccessCount(string UserName, bool SuccessOrNot, int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" select count(*)                                "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" from [dbo].[ClickHistoryForEnglishBibleQuiz] A "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" join [dbo].[EnglishBibleQuiz]                B "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" on A.[QuestionId] = B.Id"); sb.Append(Environment.NewLine);

            if (Id > 0)
            {
                sb.AppendFormat($" and   A.[QuestionId] = {Id} "); sb.Append(Environment.NewLine);
            }


            if (SuccessOrNot)
            {
                sb.AppendFormat($" and  A.[AnswerQ] = B.CorrectAnswer"); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($" and  A.[AnswerQ] != B.CorrectAnswer"); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat($" where A.[UserName] = '{UserName}'                "); sb.Append(Environment.NewLine);

            int count = DataAccessModule.executeScalar(sb.ToString());
            return count;
        }


        public DataSet fetchTodayClickHistoryForEnglishBibleQuiz(string UserName ,DateTime workDT)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select QuestionId "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[ClickHistoryForEnglishBibleQuiz] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            sb.AppendFormat($" and [UserName] = '{UserName}'       "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" and CONVERT(VARCHAR, ClickDate, 23) = CONVERT(VARCHAR, DATEADD(HOUR,{ConstatsCommon.timeDifference16},'{workDT.ToString("yyyy-MM-dd")}'), 23)  "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void CreateClickHistoryForEnglishBibleQuiz(string UserName, int QuestionId, string AnswerQ)
        {
           
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(" insert into [dbo].[ClickHistoryForEnglishBibleQuiz] "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
                sb.AppendFormat("   UserName, QuestionId, AnswerQ, "); sb.Append(Environment.NewLine);
                sb.AppendFormat("      ClickDate, [InsertDT] "); sb.Append(Environment.NewLine);
                sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
                sb.AppendFormat($" values('{UserName}',{QuestionId},'{AnswerQ}', ");
                sb.AppendFormat($"        CONVERT(VARCHAR,  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()), 23) , "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"                          DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())        "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"        )"); sb.Append(Environment.NewLine);
                DataAccessModule.executeNonQuery(sb.ToString());         
        }

    }
}

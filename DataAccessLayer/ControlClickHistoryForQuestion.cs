using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using BulletinBoard.BusinessLayer;
using BulletinBoard.Models;
using System.Collections.Generic;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlClickHistoryForQuestion
    {

        public DataSet fetchCurrentStatus(string UserName, DateTime workDT, int Id = 0)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" SELECT CA.* ,                                            "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Result =  CASE                                           "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WHEN  CA.[AnswerQ] = CA.[CorrectAnswer]  THEN 't'        "); sb.Append(Environment.NewLine);
            sb.AppendFormat("      ELSE 'f'  END                                       "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" FROM [dbo].[ClickHistoryForQuestion] CA                  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" JOIN [dbo].[QuestionData] QD ON CA.[QuestionId] = QD.Id  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WHERE 1 = 1        "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" and   LOWER(CA.[UserName]) = LOWER('{UserName}')     "); sb.Append(Environment.NewLine);
            if (Id > 0)
            {
                sb.AppendFormat($" and CA.[QuestionId] = {Id} "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($" and   CONVERT(VARCHAR, QD.ExposeDate, 23) = '{workDT.ToString("yyyy-MM-dd")}' "); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet MismatchClickHistoryForQuestion()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select ROW_NUMBER() OVER(ORDER BY ClickDate desc  )  sequenceNo ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Id, QuestionId, UserName,  [dbo].[GetFullName] (UserName) FullName,  AnswerQ, CorrectAnswer, TryCnt, ClickDate, InsertDT "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[ClickHistoryForQuestion] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and answerQ != CorrectAnswer "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void CorrectMismatchClickHistoryForQuestion(int Id, int QuestionId, string UserName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update [dbo].[ClickHistoryForQuestion] "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  set [AnswerQ]      = [CorrectAnswer]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and [Id]         =  {0}  ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and [QuestionId] =  {0}  ", QuestionId); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and [UserName]   = '{0}' ", UserName); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public int CountClickHistoryForQuestion(string UserName, int QuestionId, bool todayQuery, DateTime workDT)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select count(*) from [dbo].[ClickHistoryForQuestion] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and [UserName] = '{0}' ", UserName); sb.Append(Environment.NewLine);
            if (QuestionId > 0)
            {
                sb.AppendFormat(" and [QuestionId] = '{0}' ", QuestionId); sb.Append(Environment.NewLine);
            }
            if (todayQuery)
            {
                sb.AppendFormat($" and CONVERT(VARCHAR, ClickDate, 23)   = CONVERT(VARCHAR, DATEADD(HOUR,{ConstatsCommon.timeDifference16},'{workDT.ToString("yyyy-MM-dd")}'), 23)  "); sb.Append(Environment.NewLine);
            }

            return DataAccessModule.executeScalar(sb.ToString());
        }


 public void CreateClickHistoryForQuestion(
    int QuestionId,
    string UserName,
    string AnswerQ,
    string CorrectAnswer,
    DateTime workDT,
    int cYear // ✅ 추가
)
{
    if (!ConstatsCommon.addPoint)
        return;

    // ✅ 안전: 작은따옴표 이스케이프 (기존 문자열 빌드 방식 유지)
    string Safe(string? s) => (s ?? "").Replace("'", "''");

    var sb = new StringBuilder();
    sb.AppendLine("MERGE INTO [dbo].[ClickHistoryForQuestion] AS target");
    sb.AppendLine("USING (SELECT ");
    sb.AppendFormat("    '{0}' AS UserName     , ", Safe(UserName)).AppendLine();
    sb.AppendFormat("     {0}  AS QuestionId   , ", QuestionId).AppendLine();
    sb.AppendFormat("    '{0}' AS AnswerQ      , ", Safe(AnswerQ)).AppendLine();
    sb.AppendFormat("    '{0}' AS CorrectAnswer, ", Safe(CorrectAnswer)).AppendLine();
    sb.AppendFormat("     {0}  AS cYear        , ", cYear).AppendLine(); // ✅ 추가
    sb.AppendFormat("    CONVERT(DATE, DATEADD(HOUR,{0},'{1}')) AS ClickDate, ",
                    ConstatsCommon.timeDifference16,
                    workDT.ToString("yyyy-MM-dd")).AppendLine();
    sb.AppendFormat("    DATEADD(HOUR,{0}, GETDATE()) AS InsertDT ",
                    ConstatsCommon.timeDifference16).AppendLine();
    sb.AppendLine(") AS source");
    sb.AppendLine("ON target.QuestionId = source.QuestionId AND target.UserName = source.UserName AND target.ClickDate = source.ClickDate");

    sb.AppendLine("WHEN MATCHED THEN");
    sb.AppendLine("    UPDATE SET ");
    sb.AppendLine("        AnswerQ = source.AnswerQ, ");
    sb.AppendLine("        CorrectAnswer = source.CorrectAnswer, ");
    sb.AppendLine("        cYear = source.cYear, ");         // ✅ 추가(갱신)
    sb.AppendLine("        TryCnt = target.TryCnt + 1 ");

    sb.AppendLine("WHEN NOT MATCHED THEN");
    sb.AppendLine("    INSERT (UserName, QuestionId, AnswerQ, CorrectAnswer, cYear, ClickDate, InsertDT)"); // ✅ 추가
    sb.AppendLine("    VALUES (source.UserName, source.QuestionId, source.AnswerQ, source.CorrectAnswer, source.cYear, source.ClickDate, source.InsertDT);"); // ✅ 추가

    DataAccessModule.executeNonQuery(sb.ToString());
}


        

        public void DeleteClickHistoryForQuestion(int QuestionId, string UserName)
        {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(" delete from [dbo].[ClickHistoryForQuestion] "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" and [QuestionId] = {0} ", QuestionId); sb.Append(Environment.NewLine);
                sb.AppendFormat(" and [UserName]  = '{0}' ", UserName); sb.Append(Environment.NewLine);
                DataAccessModule.executeNonQuery(sb.ToString());
           
        }
    }
}

using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
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

        // 2026-11-09: 퀴즈 기록은 통합 표 DM2_QuizAttempt(시도) + DM2_MissionActivity(그날 정답 1점)를 쓴다.
        //   옛 ClickHistoryForQuestion 은 백업으로만 남는다.

        public DataSet fetchCurrentStatus(string UserName, DateTime workDT, int Id = 0)
        {
            string u = (UserName ?? "").Replace("'", "''");
            StringBuilder sb = new StringBuilder();

            // 옛 칸 이름 그대로 돌려준다. 맞힌 시도 → 마지막 시도 순으로 첫 줄이 지금 상태가 되게.
            sb.AppendLine(" SELECT 0 AS Id, CA.QuestionId, CA.UserName, CA.AnswerText AS AnswerQ, CA.CorrectAnswerText AS CorrectAnswer, ");
            sb.AppendLine("        CAST(CA.AttemptNo AS INT) AS TryCnt, CAST(CA.MissionDate AS DATETIME) AS ClickDate, ");
            sb.AppendLine("        CASE WHEN CA.IsCorrect = 1 THEN 't' ELSE 'f' END AS Result ");
            sb.AppendLine(" FROM [dbo].[DM2_QuizAttempt] CA ");
            sb.AppendLine(" JOIN [dbo].[QuestionData] QD ON CA.[QuestionId] = QD.Id ");
            sb.AppendLine($" WHERE LOWER(CA.[UserName]) = LOWER('{u}') ");
            if (Id > 0)
                sb.AppendLine($" and CA.[QuestionId] = {Id} ");
            else
                sb.AppendLine($" and CONVERT(VARCHAR, QD.ExposeDate, 23) = '{workDT.ToString("yyyy-MM-dd")}' ");
            sb.AppendLine(" ORDER BY CA.IsCorrect DESC, CA.AttemptNo DESC ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        /// <summary>
        /// 시도 한 줄을 가리키는 번호 — 통합 표에는 자동 번호가 없어 날짜와 시도 순서로 만든다 (yyyyMMdd * 10 + 시도 순서).
        /// 정정 도구가 이 번호와 문제 번호 · 아이디로 그 시도를 다시 찾는다.
        /// </summary>
        private const string AttemptKeySql = "(YEAR(MissionDate) * 100000 + MONTH(MissionDate) * 1000 + DAY(MissionDate) * 10 + AttemptNo)";

        public DataSet MismatchClickHistoryForQuestion()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(" select ROW_NUMBER() OVER(ORDER BY MissionDate desc, AttemptedAt desc) sequenceNo, ");
            sb.AppendLine($" {AttemptKeySql} AS Id, QuestionId, UserName, [dbo].[GetFullName](UserName) FullName, ");
            sb.AppendLine("  AnswerText AS AnswerQ, CorrectAnswerText AS CorrectAnswer, CAST(AttemptNo AS INT) AS TryCnt, ");
            sb.AppendLine("  CAST(MissionDate AS DATETIME) AS ClickDate, CAST(AttemptedAt AS DATETIME) AS InsertDT ");
            sb.AppendLine(" from [dbo].[DM2_QuizAttempt] ");
            sb.AppendLine(" where IsCorrect = 0 ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        /// <summary>틀린 시도를 맞은 것으로 고친다 — 그날 퀴즈 정답 1점이 없으면 함께 넣는다</summary>
        public void CorrectMismatchClickHistoryForQuestion(int Id, int QuestionId, string UserName)
        {
            string u = (UserName ?? "").Replace("'", "''");
            string sql = $@"
DECLARE @d DATE, @n TINYINT;
SELECT TOP 1 @d = MissionDate, @n = AttemptNo
FROM [dbo].[DM2_QuizAttempt]
WHERE [UserName] = '{u}' AND [QuestionId] = {QuestionId} AND {AttemptKeySql} = {Id};
IF @d IS NOT NULL
BEGIN
    UPDATE [dbo].[DM2_QuizAttempt] SET [AnswerText] = [CorrectAnswerText], [IsCorrect] = 1
    WHERE [UserName] = '{u}' AND [QuestionId] = {QuestionId} AND [MissionDate] = @d AND [AttemptNo] = @n;
    IF NOT EXISTS (SELECT 1 FROM [dbo].[DM2_MissionActivity] WHERE [UserName] = '{u}' AND [MissionDate] = @d AND [MissionItem] = 'Quiz.Correct')
        INSERT INTO [dbo].[DM2_MissionActivity] ([UserName], [MissionDate], [MissionItem], [Points], [PerformedAt])
        VALUES ('{u}', @d, 'Quiz.Correct', 1, SYSDATETIME());
END";
            DataAccessModule.executeNonQuery(sql);
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

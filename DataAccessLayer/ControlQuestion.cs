using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlQuestion
    {
        
        public DataSet fetchQuestion(int QuestionDataId, bool activeData, string questionType, bool queryByMonth, bool queryMoreThan, DateTime workDT, string KindData, string UserName = "" , bool AddPoint = false)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  ExposeDate asc)  sequenceNo ,");
            sb.AppendFormat("  Id, CONVERT(DATETIME,  ExposeDate, 102)  ExposeDate, ");
            sb.AppendFormat("  DAY(ExposeDate) DayData, ");

            sb.AppendFormat("  CASE DATENAME(WEEKDAY, ExposeDate) ");

            if (ConstatsCommon.timeDifference16 == 16)
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekEngToKorShort());
            }
            else
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekKorToKorShort());
            }
            sb.AppendFormat("     END AS WeekDayKor, ");
            sb.AppendFormat("    QuestionType, ");


            sb.AppendFormat("  CASE LOWER(QuestionType)  WHEN 'o' THEN '객관식' ELSE '주관식' END QuestionTypeDesc, QuestionBackground, QuestionQ, "); sb.Append(Environment.NewLine);

            if (questionType.ToLower() == "o")
            {
                sb.AppendFormat("  Question1, Question2, Question3, Question4, "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat(" "); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat(" CorrectAnswer, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" isnull(AnswerGuide,'...') AnswerGuide, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" AnswerQ, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" '' BibleText, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" isnull(appendedComments,'') appendedComments , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" 0 Jang, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" 0 Zul, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" UserName "); sb.Append(Environment.NewLine);

			sb.AppendFormat(" from [dbo].[QuestionData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");

            if(AddPoint)
            {
                sb.AppendFormat($"and id not in ( "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"                 select QuestionId  "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"			       from [dbo].[ClickHistoryForQuestion]  "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"				   where [UserName] = '{UserName}'       "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"				   and  ( [TryCnt] > 1 or [AnswerQ] = [CorrectAnswer] ) "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"				 )                                       "); sb.Append(Environment.NewLine);
            }
            else
            {
               
                //if(string.IsNullOrEmpty(KindData))
                //{
                //    sb.AppendFormat($" and  rtrim(ltrim(KindData)) = '' ");
                //}
                //else
                //{
                //    sb.AppendFormat($" and  KindData = '{KindData}' ");
                //}

                if (QuestionDataId > 0)
                {
                    sb.AppendFormat(" and [Id] =  {0} ", QuestionDataId); sb.Append(Environment.NewLine);
                }
                else if (activeData)
                {
                    sb.AppendFormat($" and  CONVERT(VARCHAR, ExposeDate, 23)   = '{workDT.ToString("yyyy-MM-dd")}' "); sb.Append(Environment.NewLine);
                }
                else if (queryByMonth)
                {
                    sb.AppendFormat($" and  YEAR (ExposeDate) = YEAR (CONVERT(DATE,'{workDT.ToString("yyyy-MM-dd")}')) ");
                    sb.AppendFormat($" and  MONTH(ExposeDate) = MONTH(CONVERT(DATE,'{workDT.ToString("yyyy-MM-dd")}')) ");
                }
                else if (queryMoreThan)
                {
                    sb.AppendFormat($" and  CONVERT(VARCHAR, ExposeDate, 23)   >= '{workDT.AddDays(-3).ToString("yyyy-MM-dd")}' "); sb.Append(Environment.NewLine);
                }


            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void CreateQuestionObject(DateTime ExposeDate, string QuestionType, string QuestionBackground, string QuestionQ, string Question1, string Question2, string Question3, string Question4, string AnswerQ, string CorrectAnswer, string AnswerGuide, string UserName)
        {
            StringBuilder sb = new StringBuilder();

            QuestionBackground = SharedComm.appendSpecialChar(QuestionBackground);
            QuestionQ = SharedComm.appendSpecialChar(QuestionQ);
            AnswerQ = SharedComm.appendSpecialChar(AnswerQ);
            CorrectAnswer = SharedComm.appendSpecialChar(CorrectAnswer);
            AnswerGuide = SharedComm.appendSpecialChar(AnswerGuide);
            Question1 = SharedComm.appendSpecialChar(Question1);
            Question2 = SharedComm.appendSpecialChar(Question2);
            Question3 = SharedComm.appendSpecialChar(Question3);
            Question4 = SharedComm.appendSpecialChar(Question4);

            sb.AppendFormat(" insert into [dbo].[QuestionData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("     ExposeDate, QuestionType, QuestionBackground, QuestionQ, Question1, Question2, Question3, Question4, AnswerQ, CorrectAnswer, AnswerGuide, [UserName]"); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values('{ExposeDate.ToString("yyyy-MM-dd")}','{QuestionType}',N'{QuestionBackground}',N'{QuestionQ}',N'{Question1}',N'{Question2}',N'{Question3}',N'{Question4}',N'{AnswerQ}',N'{CorrectAnswer}',N'{AnswerGuide}','{UserName}')"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void CreateQuestionSubject(DateTime ExposeDate, string QuestionType, string QuestionBackground, string QuestionQ, string AnswerQ, string CorrectAnswer, string AnswerGuide, string UserName)
        {
            StringBuilder sb = new StringBuilder();

            QuestionBackground = SharedComm.appendSpecialChar(QuestionBackground);
            QuestionQ = SharedComm.appendSpecialChar(QuestionQ);
            AnswerQ = SharedComm.appendSpecialChar(AnswerQ);
            CorrectAnswer = SharedComm.appendSpecialChar(CorrectAnswer);
            AnswerGuide = SharedComm.appendSpecialChar(AnswerGuide); 

            sb.AppendFormat(" insert into [dbo].[QuestionData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("     ExposeDate, QuestionType, QuestionBackground, QuestionQ, AnswerQ, CorrectAnswer, AnswerGuide, [UserName] "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values('{ExposeDate.ToString("yyyy-MM-dd")}','{QuestionType}',N'{QuestionBackground}'N'{QuestionQ}',N'{AnswerQ}',N'{CorrectAnswer}',N'{AnswerGuide}','{UserName}')"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void UpdateQuestionObject(int Id, DateTime ExposeDate, string QuestionType, string QuestionBackground, string QuestionQ, string Question1, string Question2, string Question3, string Question4, string AnswerQ, string CorrectAnswer, string AnswerGuide , string appendedComments)
        {
            StringBuilder sb = new StringBuilder();

            QuestionBackground = SharedComm.appendSpecialChar(QuestionBackground);
            QuestionQ = SharedComm.appendSpecialChar(QuestionQ);
            appendedComments = SharedComm.appendSpecialChar(appendedComments);
            AnswerQ = SharedComm.appendSpecialChar(AnswerQ);
            CorrectAnswer = SharedComm.appendSpecialChar(CorrectAnswer);
            AnswerGuide = SharedComm.appendSpecialChar(AnswerGuide);
            Question1 = SharedComm.appendSpecialChar(Question1);
            Question2 = SharedComm.appendSpecialChar(Question2);
            Question3 = SharedComm.appendSpecialChar(Question3);
            Question4 = SharedComm.appendSpecialChar(Question4);
            

            sb.AppendFormat(" update  [dbo].[QuestionData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ExposeDate =  '{0}', ", ExposeDate.ToString("yyyy-MM-dd")); sb.Append(Environment.NewLine);
            sb.AppendFormat(" QuestionBackground  =  N'{0}', ", QuestionBackground.Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" QuestionQ  =  N'{0}', ", QuestionQ.Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" appendedComments  =  N'{0}', ", appendedComments); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Question1  =  N'{0}', ", Question1.Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Question2  =  N'{0}', ", Question2.Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Question3  =  N'{0}', ", Question3.Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Question4  =  N'{0}', ", Question4.Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" AnswerQ    =  N'{0}', ", AnswerQ); sb.Append(Environment.NewLine);
            sb.AppendFormat(" CorrectAnswer =  N'{0}', ", CorrectAnswer); sb.Append(Environment.NewLine);
            sb.AppendFormat(" AnswerGuide   =  N'{0}'  ", AnswerGuide); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
  
        public void UpdateQuestionSubject(int Id, DateTime ExposeDate, string QuestionType, string QuestionBackground, string QuestionQ, string AnswerQ, string CorrectAnswer,string AnswerGuide, string appendedComments)
        {
            StringBuilder sb = new StringBuilder();

            QuestionBackground = SharedComm.appendSpecialChar(QuestionBackground);
            QuestionQ = SharedComm.appendSpecialChar(QuestionQ);
            appendedComments = SharedComm.appendSpecialChar(appendedComments);
            AnswerQ = SharedComm.appendSpecialChar(AnswerQ);
            CorrectAnswer = SharedComm.appendSpecialChar(CorrectAnswer);
            AnswerGuide = SharedComm.appendSpecialChar(AnswerGuide); 

            sb.AppendFormat(" update  [dbo].[QuestionData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ExposeDate =  '{0}', ", ExposeDate.ToString("yyyy-MM-dd")); sb.Append(Environment.NewLine);
            sb.AppendFormat(" QuestionBackground  = N'{0}', ", QuestionBackground.Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" QuestionQ  = N'{0}', ", QuestionQ.Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" appendedComments  =  N'{0}', ", appendedComments); sb.Append(Environment.NewLine);
            sb.AppendFormat(" AnswerQ    =  N'{0}', ", AnswerQ); sb.Append(Environment.NewLine);
            sb.AppendFormat(" CorrectAnswer    =  N'{0}', ", CorrectAnswer); sb.Append(Environment.NewLine);
            sb.AppendFormat(" AnswerGuide   =  N'{0}'  ", AnswerGuide); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void DeleteQuestion(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[QuestionData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}

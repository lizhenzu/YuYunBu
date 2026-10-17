using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlQuestionBank
    {
        public DataSet fetchQuestionBankOrg(int Id , string queryCode)
        { 
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select top 1000 ROW_NUMBER() OVER(ORDER BY  seqNo)  sequenceNo , Id, ");
            sb.AppendFormat(" QuestionType, QuestionQ, Question1, Question2, Question3, Question4, CorrectAnswer, BibleText, Jang, zul, zul1, updateDT, ");
            sb.AppendFormat(" example1, example2,example3,example4, ");

            sb.AppendFormat(" CASE LOWER(QuestionType)  WHEN 'o' THEN '객관식' ELSE '주관식' END QuestionTypeDesc "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from QuestionBankData "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);

            if (Id > 0)
            {
                sb.AppendFormat(" and Id = {0} ", Id); sb.Append(Environment.NewLine);
            }
            else if (!string.IsNullOrEmpty(queryCode))
            {
                sb.AppendFormat($" and queryCode = '{queryCode}' "); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetchQuestionBank(int Id, string ExposeDate)
        {

            DataSet checkDS = fetchQuestionBankSub(ExposeDate);

            if (SharedComm.CheckDataSet(checkDS) == null)
            {
                return null;
            }

            DataRow firstRow = checkDS.Tables[0].Rows[0];

            int FromJang = 0;

            if (firstRow["FromJang"] != DBNull.Value)
            {
                if (int.TryParse(firstRow["FromJang"].ToString(), out FromJang))
                {

                }
            }

            int ToJang = 0;

            if (firstRow["ToJang"] != DBNull.Value)
            {
                if (int.TryParse(firstRow["ToJang"].ToString(), out ToJang))
                {

                }
            }



            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  A.Jang, A.Zul)  sequenceNo ,");
            sb.AppendFormat("  A.Id, CONVERT(DATETIME,  ExposeDate, 102)  ExposeDate, ");
            sb.AppendFormat("  DAY(ExposeDate) DayData, ");

            sb.AppendFormat("     CASE DATENAME(WEEKDAY, ExposeDate) ");
            if (ConstatsCommon.timeDifference16 == 16)
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekEngToKorShort());
            }
            else
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekKorToKorShort());
            }
            sb.AppendFormat(" END AS WeekDayKor, ");
            sb.AppendFormat(" QuestionType, ");

            sb.AppendFormat(" CASE LOWER(A.QuestionType)  WHEN 'o' THEN '객관식' ELSE '주관식' END QuestionTypeDesc, "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" A.QuestionQ + ' (' + A.BibleText + cast( A.jang as varchar(3)) + '장'+ cast( A.zul as varchar(3)) + '절)' QuestionQ, ");

            sb.AppendFormat(" A.Question1, A.Question2, A.Question3, A.Question4, "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" A.CorrectAnswer, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" A.BibleText, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" A.Jang, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" A.Zul, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" '' AnswerGuide, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" '' AnswerQ, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" '' UserName "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from QuestionBankData A "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" join ( select * from [dbo].[TodayWordToRead]  where CONVERT(VARCHAR, ExposeDate, 23) = '{0}'  ) B ", ExposeDate); sb.Append(Environment.NewLine);
            sb.AppendFormat(" on A.BibleText =  B.BibleText  "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" AND (  "); sb.Append(Environment.NewLine);

            if (FromJang == ToJang)
            {
                sb.AppendFormat("     -- Jang이 같을 때 "); sb.Append(Environment.NewLine);
                sb.AppendFormat("     (A.Jang = B.FromJang AND A.Zul >= B.FromZul AND A.Zul <= B.ToZul)  "); sb.Append(Environment.NewLine);
                sb.AppendFormat("     -- Jang이 다를 때 (FromJang에서 ToJang에 걸쳐 있을 때) "); sb.Append(Environment.NewLine);
                sb.AppendFormat("  ) "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat("        (A.Jang = B.ToJang AND A.Zul <= B.ToZul) "); sb.Append(Environment.NewLine);
                sb.AppendFormat("     OR (A.Jang = B.FromJang AND A.Zul >= B.FromZul) "); sb.Append(Environment.NewLine);
                sb.AppendFormat("     OR (A.Jang > B.FromJang AND A.Jang < B.ToJang) "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" ) "); sb.Append(Environment.NewLine);
            }



            if (Id > 0)
            {
                sb.AppendFormat(" where A.Id = {0} ", Id); sb.Append(Environment.NewLine);
            }
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }



        public DataSet fetchQuestionBankSub(string ExposeDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select *   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[TodayWordToRead]   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where CONVERT(VARCHAR, ExposeDate, 23) = '{0}'    ", ExposeDate); sb.Append(Environment.NewLine);
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());

            return ds;
        }

        public void CreateQuizById(int Id, string WhenDate)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" insert into [dbo].[QuestionData](   ExposeDate , QuestionType, QuestionQ  , Question1, Question2, Question3, Question4, CorrectAnswer ) "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" select ExposeDate,  A.QuestionType, A.QuestionQ + ' (' + A.BibleText + cast( A.jang as varchar(3)) + '장'+ cast( A.zul as varchar(3)) + '절)', A.Question1, A.Question2, A.Question3, A.Question4, A.CorrectAnswer  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from QuestionBankData A "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" join ( select * from [dbo].[TodayWordToRead]  where CONVERT(VARCHAR, ExposeDate, 23) = '{0}'  ) B ", WhenDate); sb.Append(Environment.NewLine);
            sb.AppendFormat(" on FromJang = Jang "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and Zul between FromZul and ToZul "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where A.Id = {0} ", Id); sb.Append(Environment.NewLine);

            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void UpdateQuestionBankData(int Id, string QuestionType, string QuestionQ, string Question1, string Question2, string Question3, string Question4, string CorrectAnswer, string BibleText, int Jang, int Zul, int Zul1)
        {
            StringBuilder sb = new StringBuilder();

            QuestionQ = SharedComm.appendSpecialChar(QuestionQ);
            CorrectAnswer = SharedComm.appendSpecialChar(CorrectAnswer);
            Question1 = SharedComm.appendSpecialChar(Question1);
            Question2 = SharedComm.appendSpecialChar(Question2);
            Question3 = SharedComm.appendSpecialChar(Question3);
            Question4 = SharedComm.appendSpecialChar(Question4);


            sb.AppendFormat(" update  [dbo].[QuestionBankData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" QuestionQ     =  '{0}', ", QuestionQ); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Question1     =  '{0}', ", Question1); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Question2     =  '{0}', ", Question2); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Question3     =  '{0}', ", Question3); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Question4     =  '{0}', ", Question4); sb.Append(Environment.NewLine);
            sb.AppendFormat(" CorrectAnswer =  '{0}', ", CorrectAnswer); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleText     =  '{0}', ", BibleText); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Jang          =   {0} , ", Jang); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Zul           =   {0},   ", Zul); sb.Append(Environment.NewLine);
            sb.AppendFormat(" Zul1          =   {0},   ", Zul1); sb.Append(Environment.NewLine);
            sb.AppendFormat($" updateDT    = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
        public void VerifyQuestionBank(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update  [dbo].[QuestionBankData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" updateDT    = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void MinifyQuestionBank(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" UpdateQuestionBankData  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void UpdatePrevRecordUpdateDT(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" [UpdatePreviousRecordUpdateDT]  {0}", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
        public void UpdateNextRecordUpdateDT(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" [UpdateNextRecordUpdateDT]  {0}", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void DeleteQuestionBank(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[QuestionBankData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}
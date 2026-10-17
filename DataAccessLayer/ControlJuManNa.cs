using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using BulletinBoard.Models;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlJuManNa
    {
        public DataSet fetchJuManNaGroupByYYYYMM()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("  SELECT  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    CONCAT(YEAR(ExposeDate), '년', MONTH(ExposeDate), '월') AS YYYYMMDsiaplyString,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    COUNT(*) AS DC,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    FORMAT(ExposeDate, 'yyyy-MM') AS YYYYMMCodeString  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  FROM  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    TodayWordToRead  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  GROUP BY  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    YEAR(ExposeDate),  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    MONTH(ExposeDate),  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    FORMAT(ExposeDate, 'yyyy-MM')  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  ORDER BY  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    YYYYMMCodeString  "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetchJuManNa(int Id, bool activeData, DateTime workDT)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(MakeSQLStats()); sb.Append(Environment.NewLine);

            if (Id > 0)
            {
                sb.AppendFormat(" and MA.[Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            }
            else if (activeData)
            {
                sb.AppendFormat($" and CONVERT(VARCHAR, ExposeDate, 23)   = '{workDT.ToString("yyyy-MM-dd")}'  "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($" and CONVERT(VARCHAR, ExposeDate, 23)   >= '{workDT.AddDays(-1).ToString("yyyy-MM-dd")}'  "); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetchJuManNaByYYYYMM(string YYYYMM)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(MakeSQLStats()); sb.Append(Environment.NewLine);

            sb.AppendFormat($" and FORMAT(ExposeDate, 'yyyy-MM') = '{YYYYMM}' "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public string MakeSQLStats()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 80 ROW_NUMBER() OVER(ORDER BY  ExposeDate )  sequenceNo ,");
            sb.AppendFormat(" MA.Id, ExposeDate, synopsis ,  "); sb.Append(Environment.NewLine);

            sb.AppendFormat("     CASE DATENAME(WEEKDAY, convert(datetime, ExposeDate,20)) ");

            if (ConstatsCommon.timeDifference16 == 16)
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekEngToKorShort());
            }
            else
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekKorToKorShort());
            }

            sb.AppendFormat("     END AS WeekDayKor, ");

            sb.AppendFormat(" BibleSubject, MA.BibleText, FromJang, FromZul, ToJang, ToZul, ");
            sb.AppendFormat(" isnull(BibleSummary,'') BibleSummary, ");

            sb.AppendFormat(" note1Keyword, note1, note1Question1, note1Question2, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" note2Keyword, note2, note2Question1, note2Question2,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" todayPrayForPerson, todayPrayForHome,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" todayDescribe1, todayDescribe2, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" todayDescribe1KeyWord, todayDescribe2KeyWord, "); sb.Append(Environment.NewLine);

            //sb.AppendFormat(" Id_A, ExposeDate_A, BibleText_A, FromJang_A, FromZul_A, ToJang_A, ToZul_A, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" US.YouTuBuStartMM , US.YouTuBuStartSS , US.YouTuBuEndMM , US.YouTuBuEndSS , US.URL , US.TimeMM , US.TimeSS , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" US.YouTuBuStartMM2, US.YouTuBuStartSS2, US.YouTuBuEndMM2, US.YouTuBuEndSS2, US.URL2, US.TimeMM2, US.TimeSS2, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" US.YouTuBuStartMM3, US.YouTuBuStartSS3, US.YouTuBuEndMM3, US.YouTuBuEndSS3, US.URL3, US.TimeMM3, US.TimeSS3, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" US.YouTuBuStartMM4, US.YouTuBuStartSS4, US.YouTuBuEndMM4, US.YouTuBuEndSS4, US.URL4, US.TimeMM4, US.TimeSS4, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" US.YouTuBuStartMM5, US.YouTuBuStartSS5, US.YouTuBuEndMM5, US.YouTuBuEndSS5, US.URL5, US.TimeMM5, US.TimeSS5, "); sb.Append(Environment.NewLine);



            sb.AppendFormat(" [visited] "); sb.Append(Environment.NewLine);

            sb.AppendFormat("    "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[TodayWordToRead] MA "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join [view_TodayWordToRead_eBibleYouTuBu2] US  on MA.Id =  US.[Id_A] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");

            return sb.ToString();
        }


        public void UpdaetJuManNa(JuManNa param)
        { 
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update  [dbo].[TodayWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);



            sb.AppendFormat("BibleSubject	    =  '{0}', ", SharedComm.appendSpecialChar(param.BibleSubject).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleText	        =  '{0}', ", SharedComm.appendSpecialChar(param.BibleText).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("Synopsis	        =  '{0}', ", SharedComm.appendSpecialChar(param.Synopsis).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("BibleSummary	    =  '{0}', ", SharedComm.appendSpecialChar(param.BibleSummary).Trim()); sb.Append(Environment.NewLine);

            sb.AppendFormat("FromJang	       =   {0} , ", param.FromJang); sb.Append(Environment.NewLine);
            sb.AppendFormat("FromZul	       =   {0} , ", param.FromZul); sb.Append(Environment.NewLine);
            sb.AppendFormat("ToJang	           =   {0} , ", param.ToJang); sb.Append(Environment.NewLine);
            sb.AppendFormat("ToZul	           =   {0} , ", param.ToZul); sb.Append(Environment.NewLine);


            //sb.AppendFormat("note1Keyword	    =  '{0}', ", SharedComm.appendSpecialChar(note1Keyword).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("note1	            =  '{0}', ", SharedComm.appendSpecialChar(note1).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("note1Question1	    =  '{0}', ", SharedComm.appendSpecialChar(note1Question1).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("note1Question2	    =  '{0}', ", SharedComm.appendSpecialChar(note1Question2).Trim()); sb.Append(Environment.NewLine);

            //sb.AppendFormat("note2Keyword	    =  '{0}', ", SharedComm.appendSpecialChar(note2Keyword).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("note2	            =  '{0}', ", SharedComm.appendSpecialChar(note2).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("note2Question1	    =  '{0}', ", SharedComm.appendSpecialChar(note2Question1).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("note2Question2	    =  '{0}', ", SharedComm.appendSpecialChar(note2Question2).Trim()); sb.Append(Environment.NewLine);

            //sb.AppendFormat("todayPrayForPerson =  '{0}', ", SharedComm.appendSpecialChar(todayPrayForPerson).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("todayPrayForHome   =  '{0}', ", SharedComm.appendSpecialChar(todayPrayForHome).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("todayDescribe1	    =  '{0}', ", SharedComm.appendSpecialChar(todayDescribe1).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("todayDescribe1KeyWord	    =  '{0}', ", SharedComm.appendSpecialChar(todayDescribe1KeyWord).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("todayDescribe2	    =  '{0}', ", SharedComm.appendSpecialChar(todayDescribe2).Trim()); sb.Append(Environment.NewLine);
            //sb.AppendFormat("todayDescribe2KeyWord =  '{0}', ", SharedComm.appendSpecialChar(todayDescribe2KeyWord).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuStartMM	       =   {0} , ", param.YouTuBuStartMM); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuStartSS	       =   {0} , ", param.YouTuBuStartSS); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuEndMM	       =   {0} , ", param.YouTuBuEndMM); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuEndSS	       =   {0} , ", param.YouTuBuEndSS); sb.Append(Environment.NewLine);

            sb.AppendFormat("YouTuBuStartMM2	   =   {0} , ", param.YouTuBuStartMM2); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuStartSS2	   =   {0} , ", param.YouTuBuStartSS2); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuEndMM2	       =   {0} , ", param.YouTuBuEndMM2); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuEndSS2	       =   {0} , ", param.YouTuBuEndSS2); sb.Append(Environment.NewLine);

            sb.AppendFormat("YouTuBuStartMM3	   =   {0} , ", param.YouTuBuStartMM3); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuStartSS3	   =   {0} , ", param.YouTuBuStartSS3); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuEndMM3	       =   {0} , ", param.YouTuBuEndMM3); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuEndSS3	       =   {0} , ", param.YouTuBuEndSS3); sb.Append(Environment.NewLine);

            sb.AppendFormat("YouTuBuStartMM4	   =   {0} , ", param.YouTuBuStartMM4); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuStartSS4	   =   {0} , ", param.YouTuBuStartSS4); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuEndMM4	       =   {0} , ", param.YouTuBuEndMM4); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuEndSS4	       =   {0} , ", param.YouTuBuEndSS4); sb.Append(Environment.NewLine);

            sb.AppendFormat("YouTuBuStartMM5	   =   {0} , ", param.YouTuBuStartMM5); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuStartSS5	   =   {0} , ", param.YouTuBuStartSS5); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuEndMM5	       =   {0} , ", param.YouTuBuEndMM5); sb.Append(Environment.NewLine);
            sb.AppendFormat("YouTuBuEndSS5	       =   {0} , ", param.YouTuBuEndSS5); sb.Append(Environment.NewLine);

            sb.AppendFormat($" updateDT         = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            
            sb.AppendFormat(" where [Id] =  {0} ", param.Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
            DataAccessModule.executeNonQuery(sb.ToString());
        }



        /// <summary>
        /// 월별 일괄 저장 — 한 번에(트랜잭션) 실행.
        /// updates: 이미 있는 날(Id) 의 주제 · 성경 · 장절만 바꾼다.
        /// inserts: 표에 없는 날을 새로 넣는다(같은 날짜가 이미 있으면 건너뜀).  영상 구간 칸은 0
        /// </summary>
        public void SaveJuManNaMonthly(List<JuManNaMonthlyDay> updates, List<JuManNaMonthlyDay> inserts)
        {
            if (updates.Count == 0 && inserts.Count == 0) return;

            StringBuilder sb = new StringBuilder();
            List<SqlParameter> ps = new List<SqlParameter>();
            string now = $"DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())";

            sb.AppendLine(" SET XACT_ABORT ON; BEGIN TRAN; ");

            int i = 0;
            foreach (var d in updates)
            {
                AddDayParams(ps, i, d);
                ps.Add(new SqlParameter($"@id{i}", SqlDbType.Int) { Value = d.Id });
                sb.AppendLine($" update [dbo].[TodayWordToRead] set BibleSubject = @s{i}, BibleText = @t{i}, FromJang = @fj{i}, FromZul = @fz{i}, ToJang = @tj{i}, ToZul = @tz{i}, updateDT = {now} where [Id] = @id{i}; ");
                i++;
            }

            const string yt = "YouTuBuStartMM, YouTuBuStartSS, YouTuBuEndMM, YouTuBuEndSS, YouTuBuStartMM2, YouTuBuStartSS2, YouTuBuEndMM2, YouTuBuEndSS2, YouTuBuStartMM3, YouTuBuStartSS3, YouTuBuEndMM3, YouTuBuEndSS3, YouTuBuStartMM4, YouTuBuStartSS4, YouTuBuEndMM4, YouTuBuEndSS4, YouTuBuStartMM5, YouTuBuStartSS5, YouTuBuEndMM5, YouTuBuEndSS5";
            const string ytZero = "0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0";
            foreach (var d in inserts)
            {
                AddDayParams(ps, i, d);
                ps.Add(new SqlParameter($"@d{i}", SqlDbType.DateTime) { Value = d.ExposeDate.Date });
                sb.AppendLine($" if not exists (select 1 from [dbo].[TodayWordToRead] where CONVERT(date, ExposeDate) = CONVERT(date, @d{i})) ");
                sb.AppendLine($"   insert into [dbo].[TodayWordToRead] (ExposeDate, BibleSubject, BibleText, FromJang, FromZul, ToJang, ToZul, InsertDT, updateDT, {yt}) ");
                sb.AppendLine($"   values (@d{i}, @s{i}, @t{i}, @fj{i}, @fz{i}, @tj{i}, @tz{i}, {now}, {now}, {ytZero}); ");
                i++;
            }

            sb.AppendLine(" COMMIT; ");

            DataAccessModule.executeNonQuery(sb.ToString(), ps);
        }

        private static void AddDayParams(List<SqlParameter> ps, int i, JuManNaMonthlyDay d)
        {
            object Txt(string v) => string.IsNullOrWhiteSpace(v) ? DBNull.Value : v.Trim();
            object Num(int? v) => v.HasValue && v.Value > 0 ? v.Value : DBNull.Value;

            ps.Add(new SqlParameter($"@s{i}", SqlDbType.NVarChar, -1) { Value = Txt(d.BibleSubject) });
            ps.Add(new SqlParameter($"@t{i}", SqlDbType.NVarChar, -1) { Value = Txt(d.BibleText) });
            ps.Add(new SqlParameter($"@fj{i}", SqlDbType.Int) { Value = Num(d.FromJang) });
            ps.Add(new SqlParameter($"@fz{i}", SqlDbType.Int) { Value = Num(d.FromZul) });
            ps.Add(new SqlParameter($"@tj{i}", SqlDbType.Int) { Value = Num(d.ToJang) });
            ps.Add(new SqlParameter($"@tz{i}", SqlDbType.Int) { Value = Num(d.ToZul) });
        }

        public void HitJuManNa(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update [dbo].[TodayWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set visited = visited + 1 ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

    }
}

using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using BulletinBoard.Models;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlRhapsodyOfReality
    {
        public DataSet fetchEveryBible(string ExposeDate)
        {   
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  fullWord desc)  sequenceNo ,");
            sb.AppendFormat(" Id, ExposeDate, fullWord, chapter, startVerse, endVerse  "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from [dbo].[view_everyBible] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);

            if (!string.IsNullOrEmpty(ExposeDate))
            {
                sb.AppendFormat(" and ExposeDate = '{0}' ", ExposeDate); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetchRhapsodyOfReality1(int Id, string ExposeDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  ExposeDate desc)  sequenceNo ,");
            sb.AppendFormat("  Id,    ExposeDate, ");
            sb.AppendFormat("  DAY(convert(datetime, ExposeDate,20)) DayData, ");

            sb.AppendFormat("     CASE DATENAME(WEEKDAY, convert(datetime, ExposeDate,20)) ");

            if (ConstatsCommon.timeDifference16  == 16)
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekEngToKorShort());
            }
            else
            {
                sb.AppendFormat(SharedComm.SqlSTatSnipWeekKorToKorShort());
            }

            sb.AppendFormat("     END AS WeekDayKor, ");
            sb.AppendFormat(" visited, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WordTitle, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" YouTuBuURL , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" EndMinute, EndSecond , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WordPhrase,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" WordPray, HolyWord "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from [dbo].[RhapsodyOfRealities] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);

            if (Id > 0)
            {
                sb.AppendFormat(" and Id = {0} ", Id); sb.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(ExposeDate))
            {
                sb.AppendFormat(" and ExposeDate = '{0}' ", ExposeDate); sb.Append(Environment.NewLine);
            }

            // ⚠️ 정렬이 ROW_NUMBER() 안에만 있고 바깥에는 없었다.
            //    그러면 실제 돌아오는 행 순서를 SQL Server 가 보장하지 않는다.
            //    화면은 첫 행([0])을 "오늘의 5분설교"로 쓰므로 최신순으로 고정해야 한다.
            sb.AppendFormat(" order by ExposeDate desc "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void CreateRhapsodyOfReality(RhapsodyOfReality param)
        {
            StringBuilder sb = new StringBuilder();
                  
            sb.AppendFormat(" insert into [dbo].[RhapsodyOfRealities] (ExposeDate, WordTitle,YouTuBuURL, EndMinute, EndSecond, WordPhrase, WordPray, HolyWord) "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" Values(");
            sb.AppendFormat($"'{SharedComm.appendSpecialChar(param.ExposeDate).Trim()}',");
            sb.AppendFormat($"'{SharedComm.appendSpecialChar(param.WordTitle).Trim()}' ,");
            sb.AppendFormat($"'{SharedComm.appendSpecialChar(param.YouTuBuURL).Trim()}' ,");
            sb.AppendFormat($" {param.EndMinute} ,");
            sb.AppendFormat($" {param.EndSecond} ,");
            sb.AppendFormat($"'{SharedComm.appendSpecialChar(param.WordPhrase).Trim()}',");
            sb.AppendFormat($"'{SharedComm.appendSpecialChar(param.WordPray).Trim()}'  ,");
            sb.AppendFormat($"'{SharedComm.appendSpecialChar(param.HolyWord).Trim()}'  )"); sb.Append(Environment.NewLine);

            DataAccessModule.executeNonQuery(sb.ToString());
            CreateRhapsodyOfRealityBible(param.ExposeDate, param.HolyWord);
        }

        public void CreateRhapsodyOfRealityBible(string ExposeDate, string HolyWord)
        {
          //  string input = "엡 4:11-14, 히 10:24-25, 골 3:16, 살전 5:11";

            // 정규식 패턴 수정
            string pattern = @"([가-힣]+)\s(\d+):(\d+)(?:-(\d+))?,?\s?";

            if (string.IsNullOrEmpty(HolyWord))
                return;

            // 정규식 객체 생성
            Regex regex = new Regex(pattern);

            // 매치 찾기
            MatchCollection matches = regex.Matches(HolyWord);

            DataAccessModule.executeNonQuery($"delete from everyBible where ExposeDate = '{ExposeDate}' ");

            // 매치된 결과 출력
            foreach (Match match in matches)
            {
                if (match.Groups.Count == 5)
                {
                    string book = match.Groups[1].Value;
                    int chapter = int.Parse(match.Groups[2].Value);
                    int startVerse = int.Parse(match.Groups[3].Value);
                    int endVerse = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : startVerse;

                    string sql = $"insert into [dbo].[everyBible]([ExposeDate], BibleText, chapter, startVerse, endVerse) values('{ExposeDate}', '{book}', {chapter}, {startVerse}, {endVerse})";
                    DataAccessModule.executeNonQuery(sql);
                    // Console.WriteLine($"책: {book}, 장: {chapter}, 절: {startVerse}-{endVerse}");
                }
            }
        }

        public void UpdateRhapsodyOfReality(RhapsodyOfReality param)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update [dbo].[RhapsodyOfRealities] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set WordTitle   = '{0}', ", SharedComm.appendSpecialChar(param.WordTitle).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     YouTuBuURL  = '{0}', ", SharedComm.appendSpecialChar(param.YouTuBuURL).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     EndMinute   =  {0} , ", param.EndMinute); sb.Append(Environment.NewLine);
            sb.AppendFormat("     EndSecond   =  {0} , ", param.EndSecond); sb.Append(Environment.NewLine);
            sb.AppendFormat("     WordPhrase  = '{0}', ", SharedComm.appendSpecialChar(param.WordPhrase).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     WordPray    = '{0}', ", SharedComm.appendSpecialChar(param.WordPray).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     HolyWord    = '{0}', ", SharedComm.appendSpecialChar(param.HolyWord).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     ExposeDate  = '{0}'  ", SharedComm.appendSpecialChar(param.ExposeDate).Trim()); sb.Append(Environment.NewLine); 
            sb.AppendFormat(" where [Id] =  {0} ", param.Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
            CreateRhapsodyOfRealityBible(param.ExposeDate, param.HolyWord);
        }

        public void HitRhapsodyOfReality(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update [dbo].[RhapsodyOfRealities] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set visited = visited + 1 ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }


        public void DeleteRhapsodyOfReality(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[RhapsodyOfRealities] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}
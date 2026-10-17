using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlPhotoContest
    {

        public DataSet fetchPhotoContest(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 500 ROW_NUMBER() OVER(ORDER BY  InsertDT )  sequenceNo ,");
            sb.AppendFormat(" Id, SummaryText, FullText, URLData, InsertDT, updateDT, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" [visited] "); sb.Append(Environment.NewLine);

            sb.AppendFormat("    "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[PhotoContest] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            if (Id > 0)
            {
                sb.AppendFormat(" and [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            }
           

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }



        public void UpdaetJuManNa(int Id, string BibleSubject, string note1Keyword, string note1, string note1Question1, string note1Question2,
            string note2Keyword, string note2, string note2Question1, string note2Question2,
            string todayPrayForPerson, string todayPrayForHome, string todayDescribe1, string todayDescribe1KeyWord, string todayDescribe2, string todayDescribe2KeyWord)
        { 
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update  [dbo].[TodayWordToRead] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);



            sb.AppendFormat("BibleSubject	    =  '{0}', ", SharedComm.appendSpecialChar(BibleSubject).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("note1Keyword	    =  '{0}', ", SharedComm.appendSpecialChar(note1Keyword).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("note1	            =  '{0}', ", SharedComm.appendSpecialChar(note1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("note1Question1	    =  '{0}', ", SharedComm.appendSpecialChar(note1Question1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("note1Question2	    =  '{0}', ", SharedComm.appendSpecialChar(note1Question2).Trim()); sb.Append(Environment.NewLine);

            sb.AppendFormat("note2Keyword	    =  '{0}', ", SharedComm.appendSpecialChar(note2Keyword).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("note2	            =  '{0}', ", SharedComm.appendSpecialChar(note2).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("note2Question1	    =  '{0}', ", SharedComm.appendSpecialChar(note2Question1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("note2Question2	    =  '{0}', ", SharedComm.appendSpecialChar(note2Question2).Trim()); sb.Append(Environment.NewLine);

            sb.AppendFormat("todayPrayForPerson =  '{0}', ", SharedComm.appendSpecialChar(todayPrayForPerson).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("todayPrayForHome   =  '{0}', ", SharedComm.appendSpecialChar(todayPrayForHome).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("todayDescribe1	    =  '{0}', ", SharedComm.appendSpecialChar(todayDescribe1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("todayDescribe1KeyWord	    =  '{0}', ", SharedComm.appendSpecialChar(todayDescribe1KeyWord).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("todayDescribe2	    =  '{0}', ", SharedComm.appendSpecialChar(todayDescribe2).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("todayDescribe2KeyWord	    =  '{0}', ", SharedComm.appendSpecialChar(todayDescribe2KeyWord).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat($" updateDT         = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
            DataAccessModule.executeNonQuery(sb.ToString());
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

using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlPrayPhrase
    {

        public DataSet fetchPrayPhrase(int Id, bool activeData, DateTime workDT)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY ExposeDate )  sequenceNo ,");
            sb.AppendFormat("  Id, ExposeDate, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  PrayTitle, PraryBeginMesssage, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  BibleTextToPray, BibleTextToPrayFromJang, BibleTextToPrayFromzul, BibleTextToPrayToJang, BibleTextToPrayTozul, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  PrayContent1, PrayContent2, PrayContent3, PrayWrite, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  PraryEndMesssage "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[PrayPhrase] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            if (Id > 0)
            {
                sb.AppendFormat(" and [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            }

            if (activeData)
            {
                sb.AppendFormat($" and CONVERT(VARCHAR, ExposeDate, 23) = CONVERT(VARCHAR, DATEADD(HOUR,{ConstatsCommon.timeDifference16},'{workDT.ToString("yyyy-MM-dd")}'), 23)  "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($" and  MONTH(ExposeDate) = MONTH(CONVERT(DATE,'{workDT.ToString("yyyy-MM-dd")}')) ");
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString()); 
            return ds;
        }
         
        public void CreatePrayPhrase(DateTime ExposeDate, string PrayTitle, string PraryBeginMesssage, string BibleTextToPray, int BibleTextToPrayFromJang, int BibleTextToPrayFromZul, int BibleTextToPrayToJang, int BibleTextToPrayToZul,
            string PrayContent1, string PrayContent2, string PrayContent3, string PraryEndMesssage)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[PrayPhrase] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("     ExposeDate, PrayTitle, PraryBeginMesssage,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     [BibleTextToPray], BibleTextToPrayFromJang, BibleTextToPrayFromZul, BibleTextToPrayToJang, BibleTextToPrayToZul "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     PrayContent1, PrayContent2, PrayContent3, PraryEndMesssage "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values('{ExposeDate.ToString("yyyy-MM-dd")}','{PrayTitle}''{PraryBeginMesssage}',{BibleTextToPrayFromJang},{BibleTextToPrayFromZul},{BibleTextToPrayToJang},{BibleTextToPrayToZul},'{PrayContent1}','{PrayContent2}','{PrayContent3}','{PraryEndMesssage}')"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void UpdatePrayPhrase(int Id, DateTime ExposeDate, string PrayTitle, string PraryBeginMesssage, string BibleTextToPray, int BibleTextToPrayFromJang, int BibleTextToPrayFromZul, int BibleTextToPrayToJang, int BibleTextToPrayToZul, 
            string PrayContent1, string PrayContent2, string PrayContent3, string PraryEndMesssage)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update  [dbo].[PrayPhrase] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);
        //    sb.AppendFormat(" ExposeDate               =  '{0}', ", ExposeDate.ToString("yyyy-MM-dd")); sb.Append(Environment.NewLine);
            sb.AppendFormat(" PrayTitle                = N'{0}', ", SharedComm.appendSpecialChar(PrayTitle).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" PraryBeginMesssage       = N'{0}', ", SharedComm.appendSpecialChar(PraryBeginMesssage).Trim()); sb.Append(Environment.NewLine);

            sb.AppendFormat(" BibleTextToPray          = N'{0}', ", SharedComm.appendSpecialChar(BibleTextToPray).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTextToPrayFromJang  =   {0}, ", BibleTextToPrayFromJang); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTextToPrayFromZul   =   {0}, ", BibleTextToPrayFromZul); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTextToPrayToJang    =   {0}, ", BibleTextToPrayToJang); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BibleTextToPrayToZul     =   {0}, ", BibleTextToPrayToZul); sb.Append(Environment.NewLine);
            sb.AppendFormat(" PrayContent1             = N'{0}', ", SharedComm.appendSpecialChar(PrayContent1).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" PrayContent2             = N'{0}', ", SharedComm.appendSpecialChar(PrayContent2).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" PrayContent3             = N'{0}', ", SharedComm.appendSpecialChar(PrayContent3).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat(" PraryEndMesssage         = N'{0}', ", SharedComm.appendSpecialChar(PraryEndMesssage).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat($" [updateDT]              = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());

            DataAccessModule.executeNonQuery($"UpdatePrayPhraseTemplateByExposeDate '{ExposeDate.ToString("yyyy-MM-dd")}'");
        }

        public void DeletePrayPhrase(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[PrayPhrase] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
        public void RestorPrayPhrase(int YYear, int MMonth)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[sp_InsertPrayPhrase] {0} , {1} ", YYear, MMonth); sb.Append(Environment.NewLine);
                DataAccessModule.executeNonQuery(sb.ToString());
            }
            catch(Exception e)
            {

            }
        }
    }
}

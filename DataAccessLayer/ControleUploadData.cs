using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text; 
using BulletinBoard.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace BulletinBoard.DataAccessLayer
{
    public class ControleUploadData
    {
        public void CreateUploadData(string CategoryData, string TEXTData, string URLData)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[UploadData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat(" CategoryData, TEXTData, URLData "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values(N'{CategoryData}',N'{TEXTData}',N'{URLData}')"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void DeleteUploadData(int UploadID)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[UploadData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1        = 1 "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and [UploadID] = {0} ", UploadID); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void CreateUploadWeeklyWordToReadAssoc(string CategoryData, int WeeklyWordToReadID, List<UploadData> selectedUploadData)
        {
            DeleteUploadWeeklyWordToReadAssoc(CategoryData, WeeklyWordToReadID);

            foreach (var uploadData in selectedUploadData.Where(p => p.checkData))
            {               
                if (uploadData.checkData)
                {
                    NewMethod(WeeklyWordToReadID, uploadData);
                }
            }
        }

        private static void NewMethod(int WeeklyWordToReadID, UploadData uploadData)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[UploadWeeklyWordToReadAssoc] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("   WeeklyWordToReadID, UploadID "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values({WeeklyWordToReadID},{uploadData.UploadID})"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public  void InsertLastOne(int WeeklyWordToReadID)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[UploadWeeklyWordToReadAssoc] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("   WeeklyWordToReadID, UploadID "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values({WeeklyWordToReadID},(select max(uploadid) from [dbo].[UploadData]))"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }


        public void DeleteUploadWeeklyWordToReadAssoc(string CategoryData, int WeeklyWordToReadID)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from  [dbo].[UploadWeeklyWordToReadAssoc]");
            sb.AppendFormat("  where 1 = 1 ", WeeklyWordToReadID); sb.Append(Environment.NewLine);
            sb.AppendFormat("  and [UploadID] in ( "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                      select [UploadID] "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                      from [dbo].[UploadData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                      where CategoryData = '{0}') ", CategoryData); sb.Append(Environment.NewLine);
            sb.AppendFormat("  and [WeeklyWordToReadID] =  {0}  ", WeeklyWordToReadID); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    } 
}

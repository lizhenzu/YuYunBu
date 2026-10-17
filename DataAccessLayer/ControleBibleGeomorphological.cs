using BulletinBoard.Common;
using BulletinBoard.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControleBibleGeomorphological
    {
        //public DataSet fetcheBibleGeomorphological(string  Keyword)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY [KeyOrder])  sequenceNo ,");
        //    sb.AppendFormat(" [PhraseData]  "); sb.Append(Environment.NewLine);
        //    sb.AppendFormat(" from [dbo].[BibleGeomorphological] "); sb.Append(Environment.NewLine);
        //    sb.AppendFormat(" where 1=1 ", Keyword); sb.Append(Environment.NewLine);
        //    sb.AppendFormat(" and  Keyword = '{0}' ", Keyword); sb.Append(Environment.NewLine);

        //    DataSet ds = DataAccessModule.commonQuery(sb.ToString());
        //    return ds;
        //}

        public void CreateBibleGeomorphological(string bibleGeomorphologicalTEXT, string bibleGeomorphologicalURL)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[BibleGeomorphological] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("  bibleGeomorphologicalTEXT, bibleGeomorphologicalURL "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values('{bibleGeomorphologicalTEXT}','{bibleGeomorphologicalURL}')"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void CreateBibleGeomorphologicalWeeklyWordToReadAssoc(int WeeklyWordToReadID,List<BibleGeomorphological> selectedBibleGeomorphological)
        {
            DeleteBibleGeomorphologicalWeeklyWordToReadAssoc(WeeklyWordToReadID);

            foreach (var Geomorphological in selectedBibleGeomorphological.Where(p => p.checkData))
            {               
                if (Geomorphological.checkData)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat(" insert into [dbo].[BibleGeomorphologicalWeeklyWordToReadAssoc] "); sb.Append(Environment.NewLine);
                    sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
                    sb.AppendFormat("   WeeklyWordToReadID, BibleGeomorphologicalID "); sb.Append(Environment.NewLine);
                    sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
                    sb.AppendFormat($" values({WeeklyWordToReadID},{Geomorphological.BibleGeomorphologicalID})"); sb.Append(Environment.NewLine);
                    DataAccessModule.executeNonQuery(sb.ToString());
                }
            }
        }
        public void DeleteBibleGeomorphologicalWeeklyWordToReadAssoc(int WeeklyWordToReadID)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from  [dbo].[BibleGeomorphologicalWeeklyWordToReadAssoc]       where [WeeklyWordToReadID] = {0} ", WeeklyWordToReadID); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    } 
}

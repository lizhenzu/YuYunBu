using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlAIPromptWord
    {

        public DataSet fetchAIPromptWord(int Id)
        {

         //   DataAccessModule.executeNonQuery(" usp_AIPromptWord_updateDT ");

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY [InsertDT] )  sequenceNo ,");
            sb.AppendFormat("  [Id],          "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  [PromptText], "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  [updateDT]     "); sb.Append(Environment.NewLine);
			sb.AppendFormat(" from [dbo].[AIPromptWord] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            
            if (Id > 0)
            {
                sb.AppendFormat(" and [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            }
        
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void CreateAIPromptWord(string PromptText)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[AIPromptWord] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("     PromptText, InsertDT, updateDT"); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values('{SharedComm.appendSpecialChar(PromptText).Trim()}',DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()),DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()))"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
        public void DeleteAIPromptWord(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[AIPromptWord] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void UpdateAIPromptWord(int Id, string PromptText)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update [dbo].[AIPromptWord] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set  [PromptText]  = '{0}', ", SharedComm.appendSpecialChar(PromptText).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     [updateDT]    = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}

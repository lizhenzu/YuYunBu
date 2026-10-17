using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlOneLineFamousWord
    {

        public DataSet fetchOneLineFamousWord(int Id)
        {

         //   DataAccessModule.executeNonQuery(" usp_OneLineFamousWord_updateDT ");

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select ROW_NUMBER() OVER(ORDER BY [InsertDT] )  sequenceNo ,");
            sb.AppendFormat("  [Id],          "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  [OneLineText], "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  [updateDT]     "); sb.Append(Environment.NewLine);
			sb.AppendFormat(" from [dbo].[OneLineFamousWord] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            
            if (Id > 0)
            {
                sb.AppendFormat(" and [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            }
        
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void CreateOneLineFamousWord(string OneLineText)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[OneLineFamousWord] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("     OneLineText, InsertDT, updateDT"); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values('{SharedComm.appendSpecialChar(OneLineText).Trim()}',DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()),DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()))"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
        public void DeleteOneLineFamousWord(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[OneLineFamousWord] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void UpdateOneLineFamousWord(int Id, string OneLineText)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update [dbo].[OneLineFamousWord] "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" set  [OneLineText]  = '{SharedComm.appendSpecialChar(OneLineText).Trim()}', "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     [updateDT]     = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}

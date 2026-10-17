using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlHelpDesk
    {

        public DataSet fetchRepairHomePage(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY [InsertDT] desc )  sequenceNo ,");
            sb.AppendFormat("  Id,     "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(RequestContents, '') RequestContents ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(AnswerContents , '') AnswerContents  ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  InsertDT, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" UserName "); sb.Append(Environment.NewLine);

			sb.AppendFormat(" from [dbo].[RepairHomePage] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            if (Id > 0)
            {
                sb.AppendFormat(" and [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void CreateRepairHomePage(string RequestContents, string UserName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[RepairHomePage] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("     RequestContents, UserName"); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values('{RequestContents}','{UserName}')"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
        public void DeleteRepairHomePage(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[RepairHomePage] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void UpdateRepairHomePage(int Id, string AnswerContents)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update [dbo].[RepairHomePage] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set [AnswerContents]  = '{0}', ", AnswerContents); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     [updateDT]     = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}

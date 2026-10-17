using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlPraiseContents
    {
     
        public DataSet fetchPraiseContents(string kind, int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY [contentsText] )  sequenceNo ,");
            sb.AppendFormat("  Id,     "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  contentsURL, contentsText,Kind, Singer    "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[PraiseContents]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
            if (Id > 0)
            {
                sb.AppendFormat("and Id   = {0}", Id); sb.Append(Environment.NewLine);
            }
            if (!string.IsNullOrEmpty(kind))
            {
                sb.AppendFormat("and kind   =  '{0}'", kind); sb.Append(Environment.NewLine);
            }
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
        public void CreatePraiseContents(string contentsURL, string contentsText, string Kind, string Singer)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" insert into [dbo].[PraiseContents] ( contentsURL, contentsText, kind ,[visited] , singer, UpdateDT, InsertDT) "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" Values(");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(contentsURL).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(contentsText).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(Kind).Trim()}', ");
            sb.AppendFormat($" 0, ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(Singer).Trim()}', ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()),  ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())  ");
            sb.AppendFormat($" )"); sb.Append(Environment.NewLine);

            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void UpdatePraiseContents(int Id, string contentsURL, string contentsText, string kind, string Singer)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" update [dbo].[PraiseContents] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set contentsURL  = '{0}', ", SharedComm.appendSpecialChar(contentsURL).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     contentsText = '{0}', ", SharedComm.appendSpecialChar(contentsText).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     kind         = '{0}', ", SharedComm.appendSpecialChar(kind).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     Singer       = '{0}', ", SharedComm.appendSpecialChar(Singer).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    [updateDT]   = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id]       =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void deletePraiseContents(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($" delete from [dbo].[PraiseContents] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

    }
}

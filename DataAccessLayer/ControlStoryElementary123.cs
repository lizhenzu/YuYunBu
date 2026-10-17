using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlStoryElementary123
    {
     
        public DataSet fetchStoryElementary123(string kind, int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY [InsertDT]  desc)  sequenceNo ,");
            sb.AppendFormat("  Id,     "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  contentsURL, contentsText, kind, visited    "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[StoryElementary123]  "); sb.Append(Environment.NewLine);
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
        public void CreateStoryElementary123(string contentsURL, string contentsText, string Kind)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" insert into [dbo].[StoryElementary123] ( contentsURL, contentsText, kind ,[visited] , UpdateDT, InsertDT) "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" Values(");
            sb.AppendFormat($" 'https://www.youtube.com/embed/{SharedComm.appendSpecialChar(contentsURL).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(contentsText).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(Kind).Trim()}', ");
            sb.AppendFormat($" 0, ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()),  ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())  ");
            sb.AppendFormat($" )"); sb.Append(Environment.NewLine);

            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void UpdateStoryElementary123(int Id, string contentsURL, string contentsText, string kind)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" update [dbo].[StoryElementary123] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set contentsURL  = '{0}', ", SharedComm.appendSpecialChar(contentsURL).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     contentsText = '{0}', ", SharedComm.appendSpecialChar(contentsText).Trim()); sb.Append(Environment.NewLine);
            sb.AppendFormat("     kind         = '{0}', ", SharedComm.appendSpecialChar(kind).Trim()); sb.Append(Environment.NewLine);         
            sb.AppendFormat($"    [updateDT]   = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id]       =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void deleteStoryElementary123(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($" delete from [dbo].[StoryElementary123] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

    }
}

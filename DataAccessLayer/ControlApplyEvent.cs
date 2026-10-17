using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlApplyEvent
    {
        public DataSet fetchEventInfo(int EventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select  EventId, EventTitle, EventDesc, processStatus, applyYN,EventDayFrom,ForWhom, EventTimeFrom, EventTimeTo, EventPlace, EventDayTo, URL,URLDesc, EventCountLimitStudent, EventCountLimitTeacher "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[EventMater] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where EventPublishYN = 'y' "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and   EventDayFrom > (GETDATE() - 2 ) "); sb.Append(Environment.NewLine);
            if (EventId > 0)
            {
                sb.AppendFormat("and EventId = {0} ", EventId); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat(" order by EventDayFrom asc "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
        public DataSet fetchEventAppyInfo(string UserId, int EventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select count(*) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from  [dbo].[EventApplyInfo] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where UserId  = '{0}' ", UserId); sb.Append(Environment.NewLine);
            sb.AppendFormat(" And   EventId =  {0}  ", EventId); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void EnrollEventApplyInfo(string UserId, int EventId)
        {
            StringBuilder sb = new StringBuilder();
            //sb.AppendFormat(" insert into [dbo].[EventApplyInfo] "); sb.Append(Environment.NewLine);
            //sb.AppendFormat(" (UserId, EventId, insertDT, updateDT)"); sb.Append(Environment.NewLine);
            //sb.AppendFormat($" values('{UserId}',{EventId}, "); sb.Append(Environment.NewLine);
            //sb.AppendFormat($"        DATEADD(HOUR,{ConstatsCommon.timeDifference17}, GETDATE()) ,"); sb.Append(Environment.NewLine);
            //sb.AppendFormat($"        DATEADD(HOUR,{ConstatsCommon.timeDifference17}, GETDATE())   )"); sb.Append(Environment.NewLine);


            sb.AppendFormat(" IF NOT EXISTS ( "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     SELECT 1  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     FROM [dbo].[EventApplyInfo]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     WHERE UserId = '{UserId}' "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     and  EventId = {EventId} "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" BEGIN "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     INSERT INTO [dbo].[EventApplyInfo]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("         (UserId, EventId, insertDT, updateDT) "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     VALUES "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"         ('{UserId}', {EventId},  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("         DATEADD(HOUR, 17, GETDATE()),  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("         DATEADD(HOUR, 17, GETDATE())) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" END "); sb.Append(Environment.NewLine);



            DataAccessModule.executeNonQuery(sb.ToString());
        }
        public void CancelEnrollEvent(string UserId, int EventId)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from EventApplyInfo  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" where UserId  = '{UserId}' "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" and   EventId =  {EventId} "); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}

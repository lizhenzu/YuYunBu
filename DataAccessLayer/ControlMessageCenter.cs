using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using System.ComponentModel;
using BulletinBoard.Enums;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlMessageCenter
    {

        public DataSet fetchPopMessage(int Id, string FromWhom, string ToWhom, PopMessageReadMehod pmrm, string currentUserId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY [updateDT] desc )  sequenceNo ,");
            sb.AppendFormat("  PM.Id,     "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(MessageText, '') MessageText       , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(readYN     , '') readYN            , "); sb.Append(Environment.NewLine);

            sb.AppendFormat("  isnull(ToWhom     , '') ToWhom            , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(dbo.ufn_UserFullMame(ToWhom)       , '') ToWhomName      , "); sb.Append(Environment.NewLine);

            sb.AppendFormat("  isnull(ToWhom   , '') ToWhom          , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull([dbo].[ufn_UserFullMame](ToWhom)     , '') ToWhomName  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull([dbo].[ufnIdentity_User_ID](ToWhom)  , '') ToWhomID  , "); sb.Append(Environment.NewLine);

            sb.AppendFormat("  isnull(FromWhom   , '') FromWhom          , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull([dbo].[ufn_UserFullMame](FromWhom)     , '') FromWhomName  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull([dbo].[ufnIdentity_User_ID](FromWhom)  , '') FromWhomID  , "); sb.Append(Environment.NewLine);

            sb.AppendFormat("  isnull(readYN     , '') readYN            , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull([Visited]  , 0 ) [Visited]         , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' ProfilePicturePathFromWhom             , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' ProfilePicturePathToWhom               , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  updateDT                                    "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from [dbo].[PopMessage] PM "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join [Identity].[User] US on PM.FromWhom = US.UserName "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");

            if(pmrm == PopMessageReadMehod.CheckMyNessMessage)
            {
                sb.AppendFormat(" and [Visited] = 0 "); sb.Append(Environment.NewLine);
            }

            if (Id > 0)
            {
                sb.AppendFormat(" and [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            }
            else
            {

                if (Roles.SuperAdmin.ToString().ToLower() == currentUserId.ToLower() && pmrm == PopMessageReadMehod.ReadAllMessage)
                {
                }
                else
                {
                    if (!string.IsNullOrEmpty(FromWhom))
                    {
                        sb.AppendFormat(" and lower([FromWhom]) = '{0}' ", FromWhom.ToLower()); sb.Append(Environment.NewLine);
                    }
                    else if (!string.IsNullOrEmpty(ToWhom))
                    {
                        sb.AppendFormat(" and lower([ToWhom])   = '{0}' ", ToWhom.ToLower()); sb.Append(Environment.NewLine);
                    }
                }
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void CreatePopMessage(string MessageText, string FromWhom, string ToWhom)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[PopMessage] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("     MessageText, ToWhom, FromWhom, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     updateDT,InsertDT "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values(N'{MessageText}','{ToWhom}','{FromWhom}', "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"        DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()), "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"        DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())   )"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    

        public void UpdatePopMessage(int Id, string MessageText, string FromWhom, string ToWhom)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update [dbo].[PopMessage] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set [MessageText]  = N'{0}', ", MessageText); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    [ToWhom]       =  '{0}', ", ToWhom); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    [FromWhom]     =  '{0}', ", FromWhom); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    [updateDT]     = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }



        public void UpdateHitPopMessage(string ToWhom)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update [dbo].[PopMessage] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set [Visited]  = [Visited] + 1 , "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    [updateDT] = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [ToWhom]  =  '{0}' ", ToWhom); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and   [Visited] = 0 "); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }



        public void DeletePopMessage(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[PopMessage] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}

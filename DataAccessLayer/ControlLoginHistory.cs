using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using BulletinBoard.Helpers;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlLoginHistory
    {      
        public void AddLoginHistory(string ControllerName, string ActionName, string UserName, string requestIP)
        {
            // 2026-10-05: 사용자별 최종 기록 1건만 유지 (있으면 갱신, 없으면 추가) — 디스크 절약
            string c  = BulletinBoard.Common.SharedComm.appendSpecialChar(ControllerName);
            string a  = BulletinBoard.Common.SharedComm.appendSpecialChar(ActionName);
            string u  = BulletinBoard.Common.SharedComm.appendSpecialChar(UserName);
            string ip = BulletinBoard.Common.SharedComm.appendSpecialChar(requestIP);
            string now = DateHelper.GetKoreaNowString();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update [dbo].[loginHistory] "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    set ControllerName = '{c}', ActionName = '{a}', requestIP = '{ip}', InsertDT = '{now}' "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"  where UserName = '{u}' "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" if @@ROWCOUNT = 0 "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   insert into [dbo].[loginHistory] (ControllerName, ActionName, UserName, requestIP, InsertDT) "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"   values('{c}','{a}','{u}','{ip}','{now}') "); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public DataSet fetchLoginHistory()
        {
            StringBuilder sb = new StringBuilder();
             
            sb.AppendFormat(" select top 1005 ROW_NUMBER() OVER(ORDER BY maxInsertDT desc )  sequenceNo , ");
//            sb.AppendFormat(" ma.UserName, mb.Id UserId, ma.DC, ma.minInsertDT, ma.maxInsertDT, FullName, ClassYear, ClassBan, UserId, RoleCode, RoleNM "); sb.Append(Environment.NewLine);


            sb.AppendFormat(" ma.UserName, mb.Id UserId, ma.DC, ma.minInsertDT, ma.maxInsertDT, FullName,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" isnull(ClassYear,0) ClassYear, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" isnull(ClassBan ,0) ClassBan, "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" UserId, RoleCode, RoleNM "); sb.Append(Environment.NewLine);


            sb.AppendFormat(" from  [dbo].[view_loginHistory_groupBy]        ma "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" join  [Identity].[User]                        mb on ma.UserName = mb.UserName "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join [dbo].[view_UserCompositeInfo_2026] mc on ma.UserName = mc.UserName "); sb.Append(Environment.NewLine);
            return DataAccessModule.commonQuery(sb.ToString());
        }
        public void DeleteLoginHistory(string UserName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[loginHistory]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [UserName] =  '{0}' ", BulletinBoard.Common.SharedComm.appendSpecialChar(UserName)); sb.Append(Environment.NewLine);   // 2026-10-04: 따옴표 이스케이프(SQL 인젝션 방지)
            DataAccessModule.executeNonQuery(sb.ToString());
        }

    }
}

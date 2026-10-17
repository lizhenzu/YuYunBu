using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlAccount
    {  
        public void UpdateAspNetUsers(string Email, string UserNameKOR, string roleST)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update  [dbo].[AspNetUsers] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" [UserNameKOR] = '{0}', ", UserNameKOR); sb.Append(Environment.NewLine);
            sb.AppendFormat(" [roleST]      = '{0}'  ", roleST); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Email] = '{0}'  ", Email); sb.Append(Environment.NewLine); 
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public DataSet GetAspNetUserRoles(string UserName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select [roleST]   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from   [dbo].[AspNetUsers] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where  [UserName] = '{0}'  ", UserName); sb.Append(Environment.NewLine); 
            return DataAccessModule.commonQuery(sb.ToString());
        }


    }
}

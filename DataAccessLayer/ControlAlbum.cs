using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using BulletinBoard.Helpers;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlAlbum
    {

        public DataSet fetchMyPhotoPick()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select A.UserName,  B.id UserId, LastName, FirstName ,  (B.LastName + FirstName) FullName "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[MyPhotoPick] A "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" join [Identity].[User]   B on A.UserName = B.UserName "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" order by (B.LastName + FirstName) "); sb.Append(Environment.NewLine);
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            DbLogger.LogDebugAsync(sb.ToString());
            return ds;
        }


    }
}

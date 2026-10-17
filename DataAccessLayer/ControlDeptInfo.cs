using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlDeptInfo
    {
        public DataSet fetchDeptRoleUserAssococ(string UserId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" SELECT DeptCode, RoleCode, RA.UserId,   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" (select  MinorNM  from [dbo].[CodeData] where MajorCD = 'D001' and  MinorCD = DeptCode  ) DeptNM,   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" (select  MinorNM  from [dbo].[CodeData] where MajorCD = 'T001' and  MinorCD = RoleCode  ) RoleNM,   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ClassYear, ClassBan   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" FROM [dbo].[DeptRoleUserAssoc] RA   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" left outer join [dbo].[TeacherInfo] TI on RA.UserId = TI.UserId   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where RA.UserId = '{0}' ", UserId); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet fetchDeptInfo(string UserId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY [InsertDT] desc )  sequenceNo ,");
            sb.AppendFormat("  DeptCode,     "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  UserId        "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" UserName "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from [dbo].[DeptInfo] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            sb.AppendFormat(" and [UserId] =  '{0}' ", UserId); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
        public DataSet fetchCodeData(string MajorCD)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select ");
            sb.AppendFormat(" MinorCD, MinorNM "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[CodeData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 ");
            sb.AppendFormat(" and MajorCD = '{0}' ", MajorCD); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ORDER BY [MinorCD]  "); sb.Append(Environment.NewLine);
            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        //public void CreateDeptInfo(int DeptCode, string UserId)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendFormat(" insert into [dbo].[DeptInfo] "); sb.Append(Environment.NewLine);
        //    sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
        //    sb.AppendFormat("     [DeptCode], [UserId]"); sb.Append(Environment.NewLine);
        //    sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
        //    sb.AppendFormat($" values({DeptCode},'{UserId}')"); sb.Append(Environment.NewLine);
        //    DataAccessModule.executeNonQuery(sb.ToString());
        //}


        public void UpsertDeptInfo(int DeptCode, string UserId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("IF EXISTS (SELECT 1 FROM [dbo].[DeptInfo] WHERE [UserId] = '{0}')", UserId); sb.Append(Environment.NewLine);
            sb.AppendFormat("BEGIN"); sb.Append(Environment.NewLine);
            sb.AppendFormat("    UPDATE [dbo].[DeptInfo]"); sb.Append(Environment.NewLine);
            sb.AppendFormat("    SET [DeptCode] = {0}", DeptCode); sb.Append(Environment.NewLine);
            sb.AppendFormat("    WHERE [UserId] = '{0}';", UserId); sb.Append(Environment.NewLine);
            sb.AppendFormat("END"); sb.Append(Environment.NewLine);
            sb.AppendFormat("ELSE"); sb.Append(Environment.NewLine);
            sb.AppendFormat("BEGIN"); sb.Append(Environment.NewLine);
            sb.AppendFormat("    INSERT INTO [dbo].[DeptInfo] ([DeptCode], [UserId])"); sb.Append(Environment.NewLine);
            sb.AppendFormat("    VALUES ({0}, '{1}');", DeptCode, UserId); sb.Append(Environment.NewLine);
            sb.AppendFormat("END"); sb.Append(Environment.NewLine);

            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void DeleteDeptInfo(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[DeptInfo] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
         
    }
}

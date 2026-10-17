using BulletinBoard.Common;
using BulletinBoard.Common;
using BulletinBoard.Common;
using BulletinBoard.Helpers;
using BulletinBoard.Models; 
using Microsoft.Extensions.Logging;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Data;
using System.Data; 
using System.Data.SqlClient;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlCheckPoint
    {

        public DataSet fetchAwardItemPickAll(string UserId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select A.UserId , B.* , "); sb.Append(Environment.NewLine);

            sb.AppendFormat(BulletinBoard.Services.GiftCatalog.GiftPriceRule.PointRequirementCaseSql()); sb.Append(Environment.NewLine);

            sb.AppendFormat(" from [AwardItemPick] A "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" join AwardItem B on A.ProductID = B.ProductID "); sb.Append(Environment.NewLine);
            if (!string.IsNullOrEmpty(UserId))
            {
                sb.AppendFormat($" and [UserId] = '{UserId}'"); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
 
        public DataSet fetchAwardItemPick(string UserId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select * from [AwardItemPick] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1  "); sb.Append(Environment.NewLine);
            if (!string.IsNullOrEmpty(UserId))
            {
                sb.AppendFormat($" and [UserId] = '{UserId}'"); sb.Append(Environment.NewLine);
            }           

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
 


        public DataSet fetchGetThisWeekRewarding(string DeptCode)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select * from [GetThisWeekRewardingP]('{0}') order by DC_Total desc ", DeptCode); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }


        public DataSet fetchViewAllPointInfo(string UserId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" select UserId, AwardLevel , DC_Total, ProductID  from view_all_point_info where UserId = '{UserId}' "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }



        /// <summary>
        ///  //학생만 , 교사만
        /// </summary>
        /// <param name="rankQueryOption"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public DataSet fetchGroupByUsernameDataN(RankQueryOption rankQueryOption , string UserId, string DeptCode, string orderBy)
		{
			StringBuilder sb = new StringBuilder();



            if (rankQueryOption == RankQueryOption.maxClickDate)
            {
                orderBy = RankQueryOption.maxClickDate.ToString();
            }

            sb.AppendFormat("   select ROW_NUMBER() OVER(ORDER BY {0} desc) sequenceNo, *  , '' ProfilePicturePath ", orderBy); sb.Append(Environment.NewLine);
            sb.AppendFormat("   from [dbo].[view_Ranking_By_Person_ext]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   where 1 = 1  "); sb.Append(Environment.NewLine);
            

            if (!string.IsNullOrEmpty(DeptCode))
			{
				sb.AppendFormat("      and   DeptCode = '{0}' ", DeptCode); sb.Append(Environment.NewLine);
			}

			if (!string.IsNullOrEmpty(UserId))
			{
				sb.AppendFormat("   and UserId = '{0}'  ", UserId); sb.Append(Environment.NewLine);
			}
			else if (rankQueryOption == RankQueryOption.StudentOnly)
			{
				sb.AppendFormat("   and RoleCode = 'T1001'  "); sb.Append(Environment.NewLine);
			}
			else if (rankQueryOption == RankQueryOption.TeacherOnly)
			{
				sb.AppendFormat("   and RoleCode not in ('T1001')  "); sb.Append(Environment.NewLine);
			}
            else if (rankQueryOption == RankQueryOption.UnClassify)
            {
                sb.AppendFormat("   and (RoleCode = '' )  "); sb.Append(Environment.NewLine);
            }
            else if (rankQueryOption == RankQueryOption.All)
            {
            } 

			DataSet ds = DataAccessModule.commonQuery(sb.ToString());
			return ds;
		}

		public DataSet fetchGroupByUsernameDataByClass(int ClassYear, int ClassBan, string orderBy)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat(MakeSQL(orderBy));

            if (ClassYear > 0 && ClassBan > 0)
            {
                sb.AppendFormat($" and ClassYear = {ClassYear}  and ClassBan = {ClassBan}  "); sb.Append(Environment.NewLine);
            }
			sb.AppendFormat("    ) data   "); sb.Append(Environment.NewLine);

			DataSet ds = DataAccessModule.commonQuery(sb.ToString());
			return ds;
		}

		private string MakeSQL(string orderBy)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("   select top 100 ROW_NUMBER() OVER(ORDER BY {0} desc) sequenceNo, *  ", orderBy); sb.Append(Environment.NewLine);
			sb.AppendFormat("   from   "); sb.Append(Environment.NewLine);
			sb.AppendFormat("   (  "); sb.Append(Environment.NewLine);
			sb.AppendFormat("   select   (DC - isnull(AD.AwardLevel, 0)) DC ,  DC DC_Total, MA.userName  , MA.[LastName], MA.[FirstName] , MA.FullName  ,MA.ClassYear, MA.ClassBan, RoleCode, DeptCode ,RoleNM, DeptNM , "); sb.Append(Environment.NewLine);
			sb.AppendFormat("               [minclickDate], [maxclickDate] ,isnull(AD.AwardLevel, 0) AwardLevel, isnull(AwardLevel1000Value, 0) AwardLevel1000Value, UserId "); sb.Append(Environment.NewLine);
			sb.AppendFormat("      from [dbo].view_Ranking_By_Person  MA   "); sb.Append(Environment.NewLine);
			sb.AppendFormat("      left outer join [dbo].[awardData] AD on MA.userName = AD.userName  "); sb.Append(Environment.NewLine);
			sb.AppendFormat("      left outer join [dbo].[awardDataUnit1000] AD1000 on MA.UserId = AD1000.Award1000UserId  "); sb.Append(Environment.NewLine);
			sb.AppendFormat("      where 1 = 1   "); sb.Append(Environment.NewLine);

            return sb.ToString();
		}

		public DataSet fetchGroupByClassData(RankQueryOption rankQueryOption, int duration, string DeptCode)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("   select top 10 ROW_NUMBER() OVER(ORDER BY DC desc) sequenceNo, *  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   from   "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   (  "); sb.Append(Environment.NewLine);

            sb.AppendFormat("  select ClassYear, ClassBan , SUM(DC) DC "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  from [GetPointByDate] (16, -{0}) ", duration); sb.Append(Environment.NewLine);
            sb.AppendFormat("  where 1 = 1 "); sb.Append(Environment.NewLine);
            sb.AppendFormat("      and   DeptCode = '{0}' ", DeptCode); sb.Append(Environment.NewLine);

            if (rankQueryOption == RankQueryOption.StudentOnly)
            {
                sb.AppendFormat("   and RoleCode = 'T1001' "); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat("  group by ClassYear, ClassBan "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    ) data   "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }


        public DataSet fetchAwardBase()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("   select AwardLevel, AmountOfMoney "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   from [dbo].[AwardBase] "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   order by [Id] "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }





        public DataSet fetchExpectedAward()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("   select ROW_NUMBER() OVER(ORDER BY [DC_Projected] desc) sequenceNo, A.*,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   DC_Current, CurrentAward, DC_Projected, ProjectedAward, IsTotal, '' ProfilePicturePath "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   from [dbo].[view_Ranking_By_Person] A LEFT OUTER JOIN  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   view_AwardPayouts_WithTotal B on A.userid = b.userid "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   where userName not in ('test1000','test700','test400','test100' , 'admin') "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }





        public void UpOrDownAwardData(string userName, string optionData, int value)
        {
            if(existsAwardData(userName) == 0)
            {
                CreateawardData(userName);
            }

            StringBuilder sb = new StringBuilder(); 
            sb.AppendFormat(" update  [dbo].[awardData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);
            if (optionData == "up")
            {
                sb.AppendFormat($" AwardLevel =  AwardLevel + {value} "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($" AwardLevel =  AwardLevel - {value} "); sb.Append(Environment.NewLine);
            } 
            sb.AppendFormat(" where userName =  '{0}' ", userName); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void SetAwardData(string userName, int value)
        {
            if (existsAwardData(userName) == 0)
            {
                CreateawardData(userName);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update  [dbo].[awardData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" AwardLevel = {value} "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where userName =  '{0}' ", userName); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }


        public void UpsertAwardItemPick(string userId, int productId)
        {
            
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("IF EXISTS (SELECT 1 FROM [dbo].[AwardItemPick] WHERE UserId = @UserId)");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    UPDATE [dbo].[AwardItemPick]");
            sb.AppendLine("    SET");
            sb.AppendLine("        ProductID = @ProductID,");
            sb.AppendLine("        UpdateDT = GETDATE()");
            sb.AppendLine("    WHERE UserId = @UserId");
            sb.AppendLine("END");
            sb.AppendLine("ELSE");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    INSERT INTO [dbo].[AwardItemPick] (UserId, ProductID)");
            sb.AppendLine("    VALUES (@UserId, @ProductID)");
            sb.AppendLine("END");

   
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@ProductID", productId)            
            };



            DataAccessModule.executeNonQuery(sb.ToString(), parameters);
        }



        public void UpdateAwardItem(AwardItem item)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("UPDATE [dbo].[AwardItem]");
            sb.AppendLine("SET");
            sb.AppendLine($"    ProductName = '{item.ProductName.Replace("'", "''")}',");
            sb.AppendLine($"    ProductDescription = '{item.ProductDescription?.Replace("'", "''")}',");
            sb.AppendLine($"    UnitPrice = {item.UnitPrice}");
            sb.AppendLine($"WHERE ProductID = {item.ProductID}");

            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void DeleteAwardItemPick(string UserId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($" delete from [dbo].[AwardItemPick] where [UserId] = '{UserId}'");

            DataAccessModule.executeNonQuery(sb.ToString());
        }


        public int existsAwardData(string UserName)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select count(*) from [dbo].[awardData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" and [UserName] = '{0}' ", UserName); sb.Append(Environment.NewLine);
            
            return DataAccessModule.executeScalar(sb.ToString());
        }


        public void CreateawardData(string UserName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[awardData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("    AwardLevel, userName "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values(0 ,'{UserName}')"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

    }
}

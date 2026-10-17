using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlPrayFriends
    {


        public DataSet fetchPrayFreind()
        {
            StringBuilder sb = new StringBuilder();
            //Id, ,   ,      , LastName, ProfilePicture 
            sb.AppendFormat(" select top 1000 ROW_NUMBER() OVER(ORDER BY FullName )  sequenceNo ,");

            sb.AppendFormat("  MA.UserId              ,     "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  MA.UserName                  UserName     , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(Email           , '') Email        , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(FirstName       , '') FirstName    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(LastName        , '') LastName     , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(FullName        , '') FullName     , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(MA.PhoneNumber  , '') PhoneNumber  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(bMonth          , 0)  bMonth       , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(bDay            , 0)  bDay         , "); sb.Append(Environment.NewLine);

            sb.AppendFormat("  isnull(ClassYear, 0)   ClassYear                            , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(ClassBan , 0)   ClassBan                             , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  PM.PrayId           ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(PM.PrayText  , '') PrayText                             , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  ''                     ProfilePicturePath                      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  InsertDT                                                         "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[view_UserCompositeInfo_2026] MA  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" join [dbo].[PrayFreind]            PM on MA.UserId = PM.UserId "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void CreatePrayFreind(string PrayText, string UserName, string WriteUserName, string WriteUserId)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" insert into [dbo].[PrayFreind] (UserId, PrayText, WriteUserName, WriteUserId, UpdateDT, InsertDT) "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" Values(");
            sb.AppendFormat($"  [dbo].[ufnIdentity_User_ID]('{UserName}') , ");
            sb.AppendFormat($" N'{SharedComm.appendSpecialChar(PrayText).Trim()}', ");
            sb.AppendFormat($" N'{SharedComm.appendSpecialChar(WriteUserName).Trim()}', ");
            sb.AppendFormat($" N'{SharedComm.appendSpecialChar(WriteUserId).Trim()}', ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()),  ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())  ");
            sb.AppendFormat($" )"); sb.Append(Environment.NewLine);

            DataAccessModule.executeNonQuery(sb.ToString());
        }


        public void UpdatePrayFreind(int PrayId, string PrayText)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update  [dbo].[PrayFreind] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" PrayText      =  N'{SharedComm.appendSpecialChar(PrayText).Trim()}'"); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [PrayId] =  {0} ", PrayId); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void DeletePrayFreind(int PrayId)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[PrayFreind] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [PrayId] =  {0} ", PrayId); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }


        public void HitPrayFreind()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat($" update [dbo].[PrayFreind] "); sb.Append(Environment.NewLine);
			sb.AppendFormat($" set visitDT = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()), "); sb.Append(Environment.NewLine);
			sb.AppendFormat($"     visited = visited + 1 "); sb.Append(Environment.NewLine);
			DataAccessModule.executeNonQuery(sb.ToString());
		}

	}
}

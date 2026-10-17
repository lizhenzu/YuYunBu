using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlPray
    {

        public DataSet fetchPrayHistory(int ClassYear, int ClassBan, string PersonPraying)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 150 ROW_NUMBER() OVER(ORDER BY WhenPray )  sequenceNo , * from  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  ( select UserId, LastName, FirstName, FullName, UserName, bMonth, bDay, email, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("           PhoneNumber, PhoneNumber2, HomeAaddress, school, sex, ClassYear, ClassBan, PersonId, DeptCode, RoleCode, DeptNM, RoleNM, ProfilePicture  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    from view_UserCompositeInfo  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"   where 1 = 1 "); sb.Append(Environment.NewLine);
            //
            if (ClassYear > 0 && ClassBan> 0)
            {
                sb.AppendFormat($"   and ClassYear = {ClassYear} and ClassBan = {ClassBan} "); sb.Append(Environment.NewLine);
                sb.AppendFormat($"   and RoleCode = 'T1001'  "); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendFormat($"   and RoleCode != 'T1001'  and DeptCode = 'D1001' "); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat($"   ) A "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  left outer join ( "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                   select [PersonPraying],[PrayerRecipient], isnull(count(*), 0) PrayCount, max([WhenPray]) [WhenPray] "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  					from [PrayHistory] "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"                    where [PersonPraying] = '{PersonPraying}'  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                   group by [PersonPraying],[PrayerRecipient] "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  ) B  on A.UserId = B.[PrayerRecipient] "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString()); 
            return ds;
        }

        public void CreatePrayHistory(string personPraying, string prayerRecipient)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("INSERT INTO [dbo].[PrayHistory]");
            sb.AppendLine("(");
            sb.AppendLine("    PersonPraying,");
            sb.AppendLine("    PrayerRecipient,");
            sb.AppendLine("    WhenPray");
            sb.AppendLine(")");
            sb.AppendLine("VALUES");
            sb.AppendLine("(");
            sb.AppendFormat("    '{0}',", SharedComm.TrimString(personPraying)); sb.AppendLine();
            sb.AppendFormat("    '{0}', ", SharedComm.TrimString(prayerRecipient)); sb.AppendLine();
            sb.AppendFormat($"        DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            sb.AppendLine(");");

            DataAccessModule.executeNonQuery(sb.ToString());
        }


   

        public DataSet TodayPray(bool today)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT PrayID, Subject, [PrayTEXTData], ExposeDate , updateDT, ");
            sb.AppendLine(" isnull(publishYN, 'n') publishYN , ");
            sb.AppendLine(" isnull(BibleTextData, '') BibleTextData , ");
            sb.AppendLine(" isnull(BibleText, '') BibleText  ");
            sb.AppendLine("FROM [dbo].[CampPray]");

            sb.AppendLine("where  1 = 1 ");
            //sb.AppendFormat($"     and   CONVERT(VARCHAR, ExposeDate, 23) >= CONVERT(VARCHAR, DATEADD(DAY,0,GETDATE()), 23)        "); sb.Append(Environment.NewLine);

            //if (today)
            //{
            //    sb.AppendLine($"and CONVERT(DATE, ExposeDate) = CONVERT(DATE,  DATEADD(HOUR, " + ConstatsCommon.timeDifference16 + ", GETDATE())  )");
            //}
            sb.AppendLine("order by  ExposeDate desc ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public DataSet TodayPrayByID(int PrayID)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT PrayID, Subject, [PrayTEXTData], ExposeDate , updateDT ,  ");
            sb.AppendLine(" isnull(publishYN, 'n') publishYN , ");
            sb.AppendLine(" isnull(BibleTextData, '') BibleTextData , ");
            sb.AppendLine(" isnull(BibleText, '') BibleText  ");
            sb.AppendLine("FROM [dbo].[CampPray]");

            sb.AppendLine($"WHERE PrayID = {PrayID} ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void UpdateTodayPray(int PrayID, string Subject, string PrayTEXTData, string BibleText, string BibleTextData, string publishYN)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UPDATE [dbo].[CampPray]");
            sb.AppendFormat("SET [Subject] = '{0}', ", Subject.Replace("'", "''")); sb.AppendLine();
            sb.AppendFormat("    [PrayTEXTData] = '{0}', ", PrayTEXTData.Replace("'", "''")); sb.AppendLine();
            sb.AppendFormat("    [BibleText] = '{0}', ", BibleText.Replace("'", "''")); sb.AppendLine();
            sb.AppendFormat("    [BibleTextData] = '{0}', ", BibleTextData.Replace("'", "''")); sb.AppendLine();
            sb.AppendFormat("    [publishYN] = '{0}', ", publishYN.Replace("'", "''")); sb.AppendLine();
            sb.AppendFormat("    [updateDT] = DATEADD(HOUR, {0}, GETDATE()) ", ConstatsCommon.timeDifference16); sb.AppendLine();
            sb.AppendFormat("WHERE [PrayID] = {0};", PrayID); sb.AppendLine();

            DataAccessModule.executeNonQuery(sb.ToString());
        }

    }
}

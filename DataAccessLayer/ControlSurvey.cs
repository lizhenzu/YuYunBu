using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlSurvey
    {

        public DataSet fetchSurvey(int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY [InsertDT] )  sequenceNo ,");
            sb.AppendFormat("  Id,     "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(SurveyText , '') SurveyText  ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(question11 , '') question11  ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(question21 , '') question21  ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(question22 , '') question22  ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(question31 , '') question31  ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(question41 , '') question41  ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(question42 , '') question42  ,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(question51 , '') question51     "); sb.Append(Environment.NewLine);
             

			sb.AppendFormat(" from [dbo].[SurveyData]  "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
            if (Id > 0)
            {
                sb.AppendFormat(" and [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void CreateSurvey(string SurveyText, string question11, string question21, string question22, string question31, string question41, string question42, string question51)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[SurveyData] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("    SurveyText, question11, question21, question22, question31, question41, question42, question51, InsertDT "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);
            sb.AppendFormat($" values('{SurveyText}','{question11}','{question21}','{question22}','{question31}','{question41}','{question42}','{question51}',DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()))"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        } 
    }
}

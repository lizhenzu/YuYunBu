using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlDevelopRGBColorGradientTable
    {
        public DataSet fetchRGBColorGradientTable()
        {
            StringBuilder sb = new StringBuilder();

          //  sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Jang, zul )  sequenceNo ,");
            sb.AppendFormat(" select top 500 ROW_NUMBER() OVER(ORDER BY ColorName)  sequenceNo ,");
            sb.AppendFormat(" Id, ColorName, BackGroud1, BackGroud2, BackGroud3, FontColor "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[RGBColorGradientTable] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
         

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void createRGBColorGradientTable(string ColorName, string BackGroud1, string BackGroud2, string BackGroud3, string FontColor)
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" insert into [dbo].[RGBColorGradientTable](ColorName, BackGroud1, BackGroud2, BackGroud3, FontColor) "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" Values(");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(ColorName).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(BackGroud1).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(BackGroud2).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(BackGroud3).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(FontColor).Trim()}'  ");           
            sb.AppendFormat($" )"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void updateRGBColorGradientTable(int Id, string ColorName, string BackGroud1, string BackGroud2, string BackGroud3, string FontColor)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update [dbo].[RGBColorGradientTable] "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" set ColorName = '{ColorName}',"); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     BackGroud1 = '{BackGroud1}',"); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     BackGroud2 = '{BackGroud2}',"); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     BackGroud3 = '{BackGroud3}',"); sb.Append(Environment.NewLine); 
            sb.AppendFormat($"     FontColor  = '{FontColor}' "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void deleteRGBColorGradientTable(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[RGBColorGradientTable] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    } 
}

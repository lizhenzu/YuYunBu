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
    public class ControlDevelopRGBColorTable
    {
        public DataSet fetchRGBColorTable()
        {
            StringBuilder sb = new StringBuilder();

          //  sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Jang, zul )  sequenceNo ,");
            sb.AppendFormat(" select top 500 ROW_NUMBER() OVER(ORDER BY BackGroud)  sequenceNo ,");
            sb.AppendFormat(" Id, ColorName, BackGroud, FontColor, ColorType "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[RGBColorTable] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1                         ");
         

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        public void createRGBColorTable(string ColorName, string BackGroud, string FontColor, string ColorType)
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" insert into [dbo].[RGBColorTable](ColorName, BackGroud, FontColor,ColorType) "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" Values(");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(ColorName).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(BackGroud).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(FontColor).Trim()}', ");
            sb.AppendFormat($" '{SharedComm.appendSpecialChar(ColorType).Trim()}' ");
            sb.AppendFormat($" )"); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void updateRGBColorTable(int Id, string ColorName, string BackGroud, string FontColor, string ColorType)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(" update [dbo].[RGBColorTable] "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" set ColorName = '{ColorName}',"); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     BackGroud = '{BackGroud}',"); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     FontColor = '{FontColor}',"); sb.Append(Environment.NewLine);
            sb.AppendFormat($"     ColorType = '{ColorType}' "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void deleteRGBColorTable(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[RGBColorTable] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    } 
}

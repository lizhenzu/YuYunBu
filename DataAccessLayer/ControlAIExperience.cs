using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BulletinBoard.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlAIExperience
    {
        
        public DataSet fetchAIAskHistory(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  [InsertDT] desc )  sequenceNo ,");
            sb.AppendFormat("  Id, ");
            sb.AppendFormat("  UserName, ");
            sb.AppendFormat("  isnull(WhatAsk,'') WhatAsk , "); sb.Append(Environment.NewLine); 
            sb.AppendFormat("  isnull(dbo.ufn_UserFullMame(UserName),'')  UserNameKor , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(durationTime,0 ) durationTime , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(responseData,'') responseData , "); sb.Append(Environment.NewLine); 
            sb.AppendFormat(" [InsertDT]  "); sb.Append(Environment.NewLine);

            sb.AppendFormat($" from [dbo].[AIAskHistory] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);
     
            if (Id > 0)
            {
                sb.AppendFormat(" and Id = {0} ", Id); sb.Append(Environment.NewLine);
            }          

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        } 
    }
}
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
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Data.SqlClient;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlCheckHistory
    {
         
        public DataSet fetchALLTable()
        {
             
            string[] queries = new string[]
            {
            "select top 5  'BoardHelpDesk'    Kind, [Content], [UserName], [CreatedDate] from [dbo].BoardHelpDesk order by [CreatedDate] desc",
            "select top 5  'CommentHelpDesk'  Kind, [Content], [UserName], [CreatedDate] from [dbo].CommentHelpDesk order by [CreatedDate] desc",
            "select top 5  'BoardAnonymous'   Kind, [Content], [UserName], [CreatedDate] from [dbo].BoardAnonymous order by [CreatedDate] desc",
            "select top 10 'PopMessage'       Kind, (dbo.GetFullName([FromWhom]) + '->' + dbo.GetFullName([ToWhom])  ) [Content] ,  '' [UserName] , [InsertDT] [CreatedDate]  from [dbo].[PopMessage] order by [InsertDT] desc  ",
            "select top 5  'AIAskHistory'     Kind, [WhatAsk] [Content] ,  [UserName] , [InsertDT] [CreatedDate]  from [dbo].[AIAskHistory] order by [InsertDT] desc ",
            "select top 5  'CommentAnonymous' Kind, [Content], [UserName], [CreatedDate] from [dbo].[CommentAnonymous] order by [CreatedDate] desc"
            };

            DataTable mergedTable = new DataTable();
            mergedTable.Columns.Add("Kind", typeof(string));
            mergedTable.Columns.Add("Content", typeof(string));
            mergedTable.Columns.Add("UserName", typeof(string));
            mergedTable.Columns.Add("CreatedDate", typeof(DateTime));

            using (SqlConnection connection = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                connection.Open();
                foreach (string query in queries)
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DataRow row = mergedTable.NewRow();
                                row["Kind"] = reader["Kind"];
                                row["Content"] = reader["Content"];
                                row["UserName"] = reader["UserName"];
                                row["CreatedDate"] = reader["CreatedDate"];
                                mergedTable.Rows.Add(row);
                            }
                        }
                    }
                }
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(mergedTable);
            return ds;
        }
 
    }
}
using BulletinBoard.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace BulletinBoard.Common
{

    public static class DataAccessModule
    {
        // 접속 정보는 코드에 두지 않는다 (2026-11-09) — Startup 생성자가 설정 파일 값을 넣어 준다.
        //   운영: ConnectionStrings:DefaultConnection · 로컬: ConnectionStrings:LocalConnection
        public static void Configure(string? server, string? local)
        {
            connectionString_Server = server ?? "";
            connectionString_Local = local ?? "";
        }

        public static string connectionString_Server = "";

        public static string connectionString_Local = "";




        public static int executeScalar(string sqlStatements)
        {
            int count = 0;
            string connectionString = connectionString_Local;

            try
            {
                if (ConstatsCommon.timeDifference16 == 16)
                {
                    connectionString = connectionString_Server;
                }
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open(); 
                    SqlCommand cmd = new SqlCommand(sqlStatements, sqlConnection); 

                    //Since return type is System.Object, a typecast is must
                    count = (Int32)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                DbLogger.LogErrorAsync($"L Error : {ex.Message}");
                //throw;

            }
            return count;
        }

        public static void executeParameterizedSQL(SqlCommand sqlCommand, string commandText)
        {
            string connectionString = connectionString_Local;
            try
            {
                if (ConstatsCommon.timeDifference16 == 16)
                {
                    connectionString = connectionString_Server;
                }
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    // logger.LogDebug(connectionString);
                   
                    sqlCommand.CommandText = commandText;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.Connection = sqlConnection;
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                DbLogger.LogErrorAsync($"L Error : {ex.Message}");
                //throw;

            }
        }

        public static void executeStoredProcedure(SqlCommand sqlCommand, string commandText)
        {
            string connectionString = connectionString_Local;
            try
            {
                if (ConstatsCommon.timeDifference16 == 16)
                {
                    connectionString = connectionString_Server;
                }
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                { 
                    sqlCommand.CommandText = commandText;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.Connection = sqlConnection;
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                DbLogger.LogErrorAsync($"L Error : {ex.Message}");
                //throw;

            }
        }
        public static void executeNonQuery(string sqlStatements)
        {
            string connectionString = connectionString_Local;
            try
            {
                if (ConstatsCommon.timeDifference16 == 16)
                {
                    connectionString = connectionString_Server;
                }
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    // logger.LogDebug(connectionString);             
                    SqlCommand cmd = new SqlCommand(sqlStatements, sqlConnection);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            { 
                DbLogger.LogErrorAsync($"L Error : {sqlStatements}");
                DbLogger.LogErrorAsync($"L Error : {ex.Message}");
                throw;

            }
        }
        /// <summary>GO 로 나뉜 SQL 을 한 트랜잭션으로 실행한다 (SqlBatchRunner)</summary>
        public static int executeBatches(IReadOnlyList<string> batches)
        {
            string connectionString = ConstatsCommon.timeDifference16 == 16 ? connectionString_Server : connectionString_Local;
            return SqlBatchRunner.Run(connectionString, batches);
        }

        public static int executeNonQuery(string sql, List<SqlParameter> parameters)
        {
            string connectionString = connectionString_Local;

            if (ConstatsCommon.timeDifference16 == 16)
            {
                connectionString = connectionString_Server;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataSet commonQuery(string sqlStatements)
        {
            DbLogger.LogDebugAsync(sqlStatements);


            DataSet dataSet = new DataSet();
            string connectionString = connectionString_Local;
            try
            {
                if (ConstatsCommon.timeDifference16 == 16)
                {
                    connectionString = connectionString_Server;
                }
                using (SqlConnection connection =
                    new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlDataAdapter dadapter = new SqlDataAdapter();
                    
                    dadapter.SelectCommand = new SqlCommand(sqlStatements, connection);

                    dadapter.Fill(dataSet);
                }
            }
            catch (Exception ex)
            {
                DbLogger.LogErrorAsync($"L Error : {ex.Message}"); 
                //throw;
            }
            return dataSet;
        }
    }
}

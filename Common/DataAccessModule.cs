using BulletinBoard.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BulletinBoard.Common
{

    public static class DataAccessModule
    {
        public static string connectionString_Server = "Data Source=SQL5088.site4now.net;Initial Catalog=db_a64b16_youyunbu;User ID=db_a64b16_youyunbu_admin;Password=YouYunBu@2024;";

        public static string connectionString_Local = "Data Source=localhost;Initial Catalog=Bulletin;User ID=sa;Password=sds@2010;";


        //        public static string connectionString = "Data Source=SQL5061.site4now.net;Initial Catalog=db_a64b16_mystock;User Id=db_a64b16_mystock_admin;Password=unierp@2010";

        // ASP.NET  "Data Source=sql5083.site4now.net;Initial Catalog=DB_A64B16_ClothAlba  ;User Id=DB_A64B16_ClothAlba_admin  ;Password=sds@2010
        // ASP.NET  "Data Source=sql5083.site4now.net;Initial Catalog=DB_A64B16_ClothMaster;User Id=DB_A64B16_ClothMaster_admin;Password=sds@2010

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

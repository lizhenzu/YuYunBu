using BulletinBoard.BusinessLayer;
using BulletinBoard.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BulletinBoard.Common
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool IsSingleRecord(string query, SqlParameter[] parameters = null)
        {
            bool isSingleRecord = false;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    // COUNT 쿼리 실행 및 결과 확인
                    object result = command.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out int count))
                    {
                        isSingleRecord = (count == 1);
                    }
                }
            }

            return isSingleRecord;
        }
    }

}

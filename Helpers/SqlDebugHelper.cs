using System;
using Microsoft.Data.SqlClient;
using System.Text;

namespace BulletinBoard.Helpers
{
    public static class SqlDebugHelper
    {
        /// <summary>
        /// SqlCommand에 바인딩된 파라미터를 실제 값으로 치환한
        /// "실행된 것처럼 보이는 SQL" 문자열을 반환 (디버그/로그용)
        /// </summary>
        public static string ToDebugSql(SqlCommand cmd)
        {
            if (cmd == null) return string.Empty;

            string sql = cmd.CommandText;

            foreach (SqlParameter p in cmd.Parameters)
            {
                string value = FormatParameterValue(p.Value);
                sql = sql.Replace(p.ParameterName, value);
            }

            return sql;
        }

        private static string FormatParameterValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return "NULL";

            return value switch
            {
                DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
                string s => $"'{s.Replace("'", "''")}'",
                bool b => b ? "1" : "0",
                _ => value.ToString()
            };
        }
    }
}

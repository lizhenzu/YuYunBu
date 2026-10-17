using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BulletinBoard.Helpers
{
    public static class DbLogger
    {
        private static string _connectionString;

        public static void Configure(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// 공용 내부 로깅 메서드
        /// </summary>
        private static async Task WriteLogAsync(
            string level,
            string message,
            [CallerMemberName] string memberName = null,
            [CallerFilePath] string sourceFilePath = null)
        {
            return;
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                    throw new InvalidOperationException("DbLogger not configured. Call DbLogger.Configure() first.");

                string className = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);

                const string sql = @"
                INSERT INTO dbo.Logs(Level, Message, ClassName, MethodName)
                VALUES (@lvl, @msg, @cls, @mtd);";

                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@lvl", SqlDbType.NVarChar, 20) { Value = (object)level ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@msg", SqlDbType.NVarChar) { Value = (object)message ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@cls", SqlDbType.NVarChar, 200) { Value = (object)className ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@mtd", SqlDbType.NVarChar, 200) { Value = (object)memberName ?? DBNull.Value });

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// 에러 로그 기록
        /// </summary>
        public static Task LogErrorAsync(string message,
            [CallerMemberName] string memberName = null,
            [CallerFilePath] string sourceFilePath = null)
            => WriteLogAsync("ERROR", message, memberName, sourceFilePath);

        /// <summary>
        /// 일반 디버그/상황 기록용
        /// </summary>
        public static Task LogDebugAsync(string message,
            [CallerMemberName] string memberName = null,
            [CallerFilePath] string sourceFilePath = null)
            => WriteLogAsync("DEBUG", message, memberName, sourceFilePath);

        /// <summary>
        /// 그 외 범용 정보 로그용
        /// </summary>
        public static Task LogInfoAsync(string message,
            [CallerMemberName] string memberName = null,
            [CallerFilePath] string sourceFilePath = null)
            => WriteLogAsync("INFO", message, memberName, sourceFilePath);
    }
}

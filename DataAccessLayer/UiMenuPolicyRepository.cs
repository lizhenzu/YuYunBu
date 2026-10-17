using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.DataAccessLayer
{
    public class UiMenuPolicyRepository
    {
        private readonly string _connStr;

        public UiMenuPolicyRepository(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }

        public Dictionary<string, string> GetAllActivePolicies()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var conn = new SqlConnection(_connStr);
            conn.Open();

            var sql = @"
SELECT MenuUrl, AllowedTypes
FROM dbo.UiMenuPolicy
WHERE UseYN = 'Y';
";
            using var cmd = new SqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                var url = (rd["MenuUrl"] as string ?? "").Trim();
                var allowed = (rd["AllowedTypes"] as string ?? "").Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(url)) continue;
                dict[url] = allowed;
            }

            return dict;
        }
    }
}

// Services/PlaceService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;              // ✅ 통일: Microsoft.Data.SqlClient
using Microsoft.Extensions.Configuration;
using BulletinBoard.Models;
using BulletinBoard.Helpers;

namespace BulletinBoard.Services
{
    public class PlaceService : IPlaceService
    {
        private readonly string _conn;

        public PlaceService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        private SqlConnection Conn() => new SqlConnection(_conn);

        public async Task<IReadOnlyList<PlaceItem>> GetListAsync(string category, string? q = null)
        {
            category = CategoryOptions.Normalize(category);

            var sql = new StringBuilder(@"
SELECT PlaceId, Category, Name, ExternalUrl, Address, Tags, ThumbnailUrl, IsActive, InsertDT, UpdateDT
FROM dbo.PlaceItem
WHERE IsActive = 1 AND Category = @cat
");
            if (!string.IsNullOrWhiteSpace(q))
            {
                sql.AppendLine("AND (Name LIKE @q OR Address LIKE @q OR Tags LIKE @q)");
            }
            sql.AppendLine("ORDER BY PlaceId DESC");

            using var con = Conn();
            using var cmd = new SqlCommand(sql.ToString(), con);
            cmd.Parameters.Add(new SqlParameter("@cat", SqlDbType.NVarChar, 20) { Value = category });

            if (!string.IsNullOrWhiteSpace(q))
            {
                cmd.Parameters.Add(new SqlParameter("@q", SqlDbType.NVarChar, 210)
                {
                    Value = $"%{q.Trim()}%"
                });
            }

            await con.OpenAsync();

            var list = new List<PlaceItem>();
            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new PlaceItem
                {
                    PlaceId = rd.GetInt32(0),
                    Category = rd.GetString(1),
                    Name = rd.GetString(2),
                    ExternalUrl = rd.GetString(3),
                    Address = rd.IsDBNull(4) ? null : rd.GetString(4),
                    Tags = rd.IsDBNull(5) ? null : rd.GetString(5),
                    ThumbnailUrl = rd.IsDBNull(6) ? null : rd.GetString(6),
                    IsActive = rd.GetBoolean(7),
                    InsertDT = rd.GetDateTime(8),
                    UpdateDT = rd.IsDBNull(9) ? (DateTime?)null : rd.GetDateTime(9)
                });
            }

            return list;
        }

        public async Task<PlaceItem?> GetAsync(int placeId)
        {
            const string sql = @"
SELECT PlaceId, Category, Name, ExternalUrl, Address, Tags, ThumbnailUrl, IsActive, InsertDT, UpdateDT
FROM dbo.PlaceItem WHERE PlaceId=@id";

            using var con = Conn();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = placeId });

            await con.OpenAsync();

            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                return new PlaceItem
                {
                    PlaceId = rd.GetInt32(0),
                    Category = rd.GetString(1),
                    Name = rd.GetString(2),
                    ExternalUrl = rd.GetString(3),
                    Address = rd.IsDBNull(4) ? null : rd.GetString(4),
                    Tags = rd.IsDBNull(5) ? null : rd.GetString(5),
                    ThumbnailUrl = rd.IsDBNull(6) ? null : rd.GetString(6),
                    IsActive = rd.GetBoolean(7),
                    InsertDT = rd.GetDateTime(8),
                    UpdateDT = rd.IsDBNull(9) ? (DateTime?)null : rd.GetDateTime(9)
                };
            }

            return null;
        }

        public async Task<int> CreateAsync(PlaceItemInput input)
        {
            const string sql = @"
INSERT dbo.PlaceItem(Category, Name, ExternalUrl, Address, Tags, ThumbnailUrl, IsActive)
VALUES(@Category, @Name, @ExternalUrl, @Address, @Tags, @ThumbnailUrl, @IsActive);
SELECT SCOPE_IDENTITY();
";

            using var con = Conn();
            using var cmd = new SqlCommand(sql, con);
            Bind(cmd, input);

            await con.OpenAsync();

            // (선택) 로그도 남기고 싶으면 주석 해제
            // await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<int> UpdateAsync(PlaceItemInput input)
        {
            const string sql = @"
UPDATE dbo.PlaceItem SET
    Category=@Category, Name=@Name, ExternalUrl=@ExternalUrl, Address=@Address,
    Tags=@Tags, ThumbnailUrl=@ThumbnailUrl, IsActive=@IsActive, UpdateDT=SYSDATETIME()
WHERE PlaceId=@PlaceId;
";

            using var con = Conn();
            using var cmd = new SqlCommand(sql, con);
            Bind(cmd, input);
            cmd.Parameters.Add(new SqlParameter("@PlaceId", SqlDbType.Int) { Value = input.PlaceId });

            await con.OpenAsync();

            // (선택) 로그
            // await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteAsync(int placeId)
        {
            const string sql = "DELETE dbo.PlaceItem WHERE PlaceId=@id";

            using var con = Conn();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = placeId });

            await con.OpenAsync();

            // (선택) 로그
            // await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Dictionary<string, int>> GetCountsAsync()
        {
            const string sql = @"
SELECT Category, COUNT(*) FROM dbo.PlaceItem WHERE IsActive=1 GROUP BY Category";

            using var con = Conn();
            using var cmd = new SqlCommand(sql, con);

            await con.OpenAsync();

            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            using var rd = await cmd.ExecuteReaderAsync();
            var dict = new Dictionary<string, int>();

            while (await rd.ReadAsync())
            {
                dict[rd.GetString(0)] = rd.GetInt32(1);
            }

            // 카테고리가 없더라도 0 채움
            foreach (var c in CategoryOptions.All)
                if (!dict.ContainsKey(c)) dict[c] = 0;

            return dict;
        }

        private static void Bind(SqlCommand cmd, PlaceItemInput input)
        {
            cmd.Parameters.AddRange(new[]
            {
                new SqlParameter("@Category", SqlDbType.NVarChar, 20)
                {
                    Value = CategoryOptions.Normalize(input.Category)
                },
                new SqlParameter("@Name", SqlDbType.NVarChar, 200)
                {
                    Value = (input.Name ?? "").Trim()
                },
                new SqlParameter("@ExternalUrl", SqlDbType.NVarChar, 500)
                {
                    Value = (input.ExternalUrl ?? "").Trim()
                },
                new SqlParameter("@Address", SqlDbType.NVarChar, 300)
                {
                    Value = (object?)input.Address ?? DBNull.Value
                },
                new SqlParameter("@Tags", SqlDbType.NVarChar, 200)
                {
                    Value = (object?)input.Tags ?? DBNull.Value
                },
                new SqlParameter("@ThumbnailUrl", SqlDbType.NVarChar, 500)
                {
                    Value = (object?)input.ThumbnailUrl ?? DBNull.Value
                },
                new SqlParameter("@IsActive", SqlDbType.Bit)
                {
                    Value = input.IsActive
                }
            });
        }
    }
}
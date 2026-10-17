using BulletinBoard.Common;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using Microsoft.Data.SqlClient; // NuGet: Microsoft.Data.SqlClient
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using YunYunBu2024.Services.Shorts;

namespace YourApp.Services
{
    public class ShortsRepository : IShortsRepository
    {
        private readonly string _connStr;
        public ShortsRepository(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection")!;
        }

        private SqlConnection GetConn() => new SqlConnection(_connStr);

        public async Task<List<ShortVideo>> GetListAsync2(string? q)
        {
            var list = new List<ShortVideo>();
            using var conn = GetConn();
            await conn.OpenAsync();

            var sql = @"SELECT ShortId, YoutubeId, Title, IsActive, isnull(VideoKind,'Life') VideoKind, InsertDT, UpdateDT
                         FROM dbo.ShortVideo WITH (NOLOCK)
                         WHERE 1=1
                           AND (@q IS NULL OR Title LIKE '%' + @q + '%' OR YoutubeId LIKE '%' + @q + '%')
                         ORDER BY InsertDT DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@q", SqlDbType.NVarChar, 200) { Value = (object?)q ?? DBNull.Value });
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new ShortVideo
                {
                    ShortId = rd.GetInt32(0),
                    YoutubeId = rd.GetString(1),
                    Title = rd.IsDBNull(2) ? null : rd.GetString(2),
                    IsActive = rd.GetBoolean(3),
                    VideoKind = rd.GetString(4),
                    InsertDT = rd.GetDateTime(5),
                    UpdateDT = rd.IsDBNull(6) ? (DateTime?)null : rd.GetDateTime(6)
                });
            }
            return list;
        }


        public async Task<List<ShortVideo>> GetListAsync(string? q, VideoKindOption? kind)
        {
            var list = new List<ShortVideo>();

            using var conn = GetConn(); // 기존 커넥션 팩토리 사용
            await conn.OpenAsync();

            // VideoKind가 INT 또는 NVARCHAR 어느 쪽이어도 동작하도록 이중 조건 적용
            // - TRY_CONVERT(int, VideoKind) = @kind        → NVARCHAR 컬럼에도 안전(변환 실패 시 NULL)
            // - CONVERT(nvarchar(32), VideoKind) = @kindText → INT 컬럼에도 안전(정수값을 문자열로 캐스팅하여 비교)
            var sql = @"
SELECT
    ShortId,
    YoutubeId,
    Title,
    IsActive,
    VideoKind,
    InsertDT,
    UpdateDT
FROM dbo.ShortVideo WITH (NOLOCK)
WHERE (@q IS NULL OR Title LIKE '%' + @q + '%' OR YoutubeId LIKE '%' + @q + '%')
  AND (
        @kind IS NULL
        OR TRY_CONVERT(int, VideoKind) = @kind
        OR CONVERT(nvarchar(32), VideoKind) = @kindText
      )
ORDER BY InsertDT DESC;";

            using var cmd = new SqlCommand(sql, conn);

            // 검색어 q (NULL-safe)
            cmd.Parameters.Add(new SqlParameter("@q", SqlDbType.NVarChar, 200)
            { Value = (object?)q ?? DBNull.Value });

            // kind (nullable enum → INT)
            cmd.Parameters.Add(new SqlParameter("@kind", SqlDbType.Int)
            { Value = kind.HasValue ? (object)(int)kind.Value : DBNull.Value });

            // kind의 문자열 표현 (NVARCHAR 컬럼 대비)
            cmd.Parameters.Add(new SqlParameter("@kindText", SqlDbType.NVarChar, 32)
            { Value = kind.HasValue ? (object)kind.Value.ToString()! : DBNull.Value });

            using var rd = await cmd.ExecuteReaderAsync();

            int ordShortId = rd.GetOrdinal("ShortId");
            int ordYoutube = rd.GetOrdinal("YoutubeId");
            int ordTitle = rd.GetOrdinal("Title");
            int ordActive = rd.GetOrdinal("IsActive");
            int ordKind = rd.GetOrdinal("VideoKind");
            int ordIns = rd.GetOrdinal("InsertDT");
            int ordUpd = rd.GetOrdinal("UpdateDT");

            while (await rd.ReadAsync())
            {
                // 필수/옵션 컬럼 NULL 안전
                var item = new ShortVideo
                {
                    ShortId = rd.GetInt32(ordShortId),
                    YoutubeId = rd.GetString(ordYoutube),                            // NOT NULL 가정
                    Title = rd.IsDBNull(ordTitle) ? null : rd.GetString(ordTitle),
                    IsActive = rd.GetBoolean(ordActive),
                    InsertDT = rd.GetDateTime(ordIns),
                    UpdateDT = rd.IsDBNull(ordUpd) ? (DateTime?)null : rd.GetDateTime(ordUpd),
                    VideoKind = rd.IsDBNull(ordKind) ? "Life" : rd.GetString(ordKind)
                };

                list.Add(item);
            }

            return list;
        }

    

        public async Task<ShortVideo?> FindAsync(int id)
        {
            using var conn = GetConn();
            await conn.OpenAsync();
            var sql = @"SELECT ShortId, YoutubeId, Title, IsActive, VideoKind, InsertDT, UpdateDT
                        FROM dbo.ShortVideo WITH (NOLOCK)
                        WHERE ShortId=@id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = id });
            using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                return new ShortVideo
                {
                    ShortId = rd.GetInt32(0),
                    YoutubeId = rd.GetString(1),
                    Title = rd.IsDBNull(2) ? null : rd.GetString(2),
                    IsActive = rd.GetBoolean(3),
                    VideoKind = rd.GetString(4),
                    InsertDT = rd.GetDateTime(5),
                    UpdateDT = rd.IsDBNull(6) ? (DateTime?)null : rd.GetDateTime(6)
                };
            }
            return null;
        }

        public async Task<bool> ExistsByYoutubeIdAsync(string youtubeId, int? excludeId = null)
        {
            using var conn = GetConn();
            await conn.OpenAsync();
            var sql = @"SELECT 1 FROM dbo.ShortVideo WITH (NOLOCK)
                        WHERE YoutubeId=@id AND (@ex IS NULL OR ShortId<>@ex)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, 16) { Value = youtubeId });
            cmd.Parameters.Add(new SqlParameter("@ex", SqlDbType.Int) { Value = (object?)excludeId ?? DBNull.Value });
            var obj = await cmd.ExecuteScalarAsync();
            return obj != null;
        }

        public async Task<int> CreateAsync(ShortVideoInput input)
        {
            using var conn = GetConn();
            await conn.OpenAsync();
            var sql = @"INSERT INTO dbo.ShortVideo (YoutubeId, Title, IsActive, VideoKind)
                        OUTPUT INSERTED.ShortId
                        VALUES (@y, @t, @a, @k);";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@y", SqlDbType.NVarChar, 16) { Value = input.YoutubeId });
            cmd.Parameters.Add(new SqlParameter("@t", SqlDbType.NVarChar, 200) { Value = (object?)input.Title ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@a", SqlDbType.Bit) { Value = input.IsActive });
            cmd.Parameters.Add(new SqlParameter("@k", SqlDbType.NVarChar, 50) { Value = input.VideoKind });
            var newId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return newId;
        }

        public async Task<bool> UpdateAsync(ShortVideoInput input)
        {
            if (input.ShortId == null) return false;
            using var conn = GetConn();
            await conn.OpenAsync();
            var sql = @"UPDATE dbo.ShortVideo
                        SET YoutubeId=@y, Title=@t, IsActive=@a, VideoKind=@k, UpdateDT=@UpdateDT
                        WHERE ShortId=@id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@y", SqlDbType.NVarChar, 16) { Value = input.YoutubeId });
            cmd.Parameters.Add(new SqlParameter("@t", SqlDbType.NVarChar, 200) { Value = (object?)input.Title ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@a", SqlDbType.Bit) { Value = input.IsActive });
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = input.ShortId.Value });
            cmd.Parameters.Add(new SqlParameter("@k", SqlDbType.NVarChar, 50) { Value = input.VideoKind });
            cmd.Parameters.AddWithValue("@UpdateDT", DateHelper.GetKoreaNow());
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = GetConn();
            await conn.OpenAsync();
            var sql = @"DELETE FROM dbo.ShortVideo WHERE ShortId=@id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = id });
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<Dictionary<VideoKindOption, int>> GetCountsByVideoKindAsync(bool onlyActive = true)
        {
            var result = new Dictionary<VideoKindOption, int>();

            // ▶ 동적 SQL 구성
            var sb = new StringBuilder();
            sb.AppendLine("SELECT");
            sb.AppendLine("  ISNULL(VideoKind, '') AS VideoKind,");
            sb.AppendLine("  COUNT(1)              AS Cnt");
            sb.AppendLine("FROM [dbo].[ShortVideo] WITH (NOLOCK)");
            sb.AppendLine("WHERE 1 = 1");
            if (onlyActive)
                sb.AppendLine("  AND IsActive = 1");
            sb.AppendLine("GROUP BY ISNULL(VideoKind, '')");

            using (var conn = GetConn())
            using (var cmd = new SqlCommand(sb.ToString(), conn))
            {
                await conn.OpenAsync();
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        var vkRaw = rd["VideoKind"]?.ToString(); // "0","1","2","3","4"
                        var cnt = Convert.ToInt32(rd["Cnt"]);

                        if (int.TryParse(vkRaw, out var vkInt)
                            && Enum.IsDefined(typeof(VideoKindOption), vkInt))
                        {
                            var vk = (VideoKindOption)vkInt;
                            result[vk] = cnt;
                        }
                        // VideoKind가 NULL/공백이거나 범위 밖이면 무시
                    }
                }
            }
            // 누락된 enum은 0으로 채워 일관성 있게 반환
            foreach (VideoKindOption e in Enum.GetValues(typeof(VideoKindOption)))
                if (!result.ContainsKey(e)) result[e] = 0;

            return result;
        }

    }
}

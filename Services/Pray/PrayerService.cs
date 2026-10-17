using BulletinBoard.Helpers;
using BulletinBoard.Common;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.Data.SqlClient; // NuGet: Microsoft.Data.SqlClient
using System.Data;
using System.Text;

namespace YourApp.Services
{


    public class PrayerService : IPrayerService
    {
        private readonly string _connStr;
        public PrayerService(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }

        private static int DayToIndex(DateTime d)
        {
            var day = d.Day; // 1~28/29/30/31
            // 1~31 매핑, 31일보다 짧은 달도 안전하게 순환
            return ((day - 1) % 31) + 1;
        }

        public async Task<PrayerArrow?> GetTodayAsync(DateTime? now = null)
        {
                    var today = now ?? DateHelper.GetKoreaNow().AddHours(ConstatsCommon.timeDifference16).Date;


            today = today.AddHours(ConstatsCommon.timeDifference16);

            var idx = DayToIndex(today);
            return await GetByDayIndexAsync(idx);
        }

        public async Task<PrayerArrow?> GetByDayIndexAsync(int dayIndex)
        {
            const string sql = @"SELECT TOP 1 * FROM dbo.PrayerArrow WHERE DayIndex=@DayIndex AND IsActive=1";
            using var cnn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, cnn);
            cmd.Parameters.AddWithValue("@DayIndex", dayIndex);
            await cnn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                return Map(rd);
            }
            return null;
        }

        public async Task<List<PrayerArrow>> SearchAsync(int? dayIndex = null, string? keyword = null, bool? isActive = true, string? orderBy = null, int? page = null, int? pageSize = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM dbo.PrayerArrow WHERE 1=1");
            var prms = new List<SqlParameter>();

            if (dayIndex.HasValue)
            {
                sb.AppendLine("AND DayIndex = @DayIndex");
                prms.Add(new SqlParameter("@DayIndex", SqlDbType.Int) { Value = dayIndex.Value });
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sb.AppendLine("AND (Title LIKE @kw OR Line1 LIKE @kw OR Line2 LIKE @kw OR Line3 LIKE @kw)");
                prms.Add(new SqlParameter("@kw", SqlDbType.NVarChar, 400) { Value = "%" + keyword + "%" });
            }
            if (isActive.HasValue)
            {
                sb.AppendLine("AND IsActive = @isActive");
                prms.Add(new SqlParameter("@isActive", SqlDbType.Bit) { Value = isActive.Value });
            }

            // 동적 정렬 (화이트리스트)
            string order = orderBy?.ToLowerInvariant() switch
            {
                "dayindex" => "DayIndex",
                "title" => "Title",
                _ => "DayIndex"
            };
            sb.AppendLine($"ORDER BY {order}");

            // 페이징 (옵션)
            if (page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0)
            {
                int skip = (page.Value - 1) * pageSize.Value;
                sb.AppendLine("OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY");
                prms.Add(new SqlParameter("@skip", SqlDbType.Int) { Value = skip });
                prms.Add(new SqlParameter("@take", SqlDbType.Int) { Value = pageSize.Value });
            }

            var list = new List<PrayerArrow>();
            using var cnn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sb.ToString(), cnn);
            cmd.Parameters.AddRange(prms.ToArray());
            await cnn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(Map(rd));
            }
            return list;
        }

        public async Task<PrayerArrow?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM dbo.PrayerArrow WHERE PrayerId=@Id";
            using var cnn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, cnn);
            cmd.Parameters.AddWithValue("@Id", id);
            await cnn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync()) return Map(rd);
            return null;
        }

        public async Task<int> UpdateAsync(PrayerArrow item)
        {
            const string sql = @"UPDATE dbo.PrayerArrow
SET DayIndex=@DayIndex, Title=@Title, Line1=@Line1, Line2=@Line2, Line3=@Line3, IsActive=@IsActive, UpdateDT=sysdatetime()
WHERE PrayerId=@PrayerId";
            using var cnn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(sql, cnn);
            cmd.Parameters.AddWithValue("@PrayerId", item.PrayerId);
            cmd.Parameters.AddWithValue("@DayIndex", item.DayIndex);
            cmd.Parameters.AddWithValue("@Title", item.Title);
            cmd.Parameters.AddWithValue("@Line1", item.Line1);
            cmd.Parameters.AddWithValue("@Line2", item.Line2);
            cmd.Parameters.AddWithValue("@Line3", item.Line3);
            cmd.Parameters.AddWithValue("@IsActive", item.IsActive);
            await cnn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        private static PrayerArrow Map(IDataRecord r)
        {
            return new PrayerArrow
            {
                PrayerId = r.GetInt32(r.GetOrdinal("PrayerId")),
                DayIndex = r.GetInt32(r.GetOrdinal("DayIndex")),
                Title = r.GetString(r.GetOrdinal("Title")),
                Line1 = r.GetString(r.GetOrdinal("Line1")),
                Line2 = r.GetString(r.GetOrdinal("Line2")),
                Line3 = r.GetString(r.GetOrdinal("Line3")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
                InsertDT = r.GetDateTime(r.GetOrdinal("InsertDT")),
                UpdateDT = r.IsDBNull(r.GetOrdinal("UpdateDT")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("UpdateDT"))
            };
        }
    }
}
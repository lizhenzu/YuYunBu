using BulletinBoard.Helpers;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.AddressBook;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace BulletinBoard.Services
{
    public class AddressBookService : IAddressBookService
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _cache;

        private const string FallbackPhoto = "/images/icon/people/people_01.webp";
        private static readonly TimeSpan PhotoCacheTtl = TimeSpan.FromMinutes(20); // ✅ 10~30분

        public AddressBookService(IConfiguration config, IWebHostEnvironment env, IMemoryCache cache)
        {
            _config = config;
            _env = env;
            _cache = cache;
        }

        private SqlConnection OpenConn()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        // ✅ userId별로 "최종 PhotoUrl"을 캐싱 (디스크 I/O 최소화)
        private string ResolvePhotoUrl(string? userIdRaw)
        {
            var userId = (userIdRaw ?? "").Trim();
            if (string.IsNullOrWhiteSpace(userId)) return FallbackPhoto;

            var cacheKey = $"profile-url:{userId}";

            if (_cache.TryGetValue(cacheKey, out string cachedUrl) && !string.IsNullOrWhiteSpace(cachedUrl))
                return cachedUrl;

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // 폴더 케이스 2개 시도
            var folders = new[]
            {
                Path.Combine(webRoot, "images", "Profile"),
                Path.Combine(webRoot, "images", "profile"),
            };

            // ✅ 확장자 후보 (현장 대응)
            var exts = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            string found = FallbackPhoto;

            foreach (var folder in folders)
            {
                foreach (var ext in exts)
                {
                    var physical = Path.Combine(folder, userId + ext);
                    if (File.Exists(physical))
                    {
                        // URL은 요청하신 경로 기준으로 /images/Profile 로 통일
                        found = $"/images/Profile/{userId}{ext}";
                        goto DONE;
                    }
                }
            }

        DONE:
            _cache.Set(cacheKey, found, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PhotoCacheTtl,
                Priority = CacheItemPriority.Low
            });

            return found;
        }

        public List<AddressBookPersonCardVM> Search(string q, int take = 30)
        {
            q = (q ?? "").Trim();
            if (q.Length == 0) return new List<AddressBookPersonCardVM>();

            if (take < 1) take = 1;
            if (take > 50) take = 50;

            var list = new List<AddressBookPersonCardVM>();

            using var conn = OpenConn();
            conn.Open();

            var sql = @"
SELECT TOP (@Take)
    p.PersonId,
    p.UserId,
    ISNULL(p.fullName,'') AS FullName,
    p.UserName,
    p.email,
    p.PhoneNumber,
    p.bMonth,
    p.bDay,
    ci.ClassYear,
    ci.ClassBan
FROM dbo.view_PersonMaster_2026 p
OUTER APPLY (
    SELECT TOP 1 c.ClassYear, c.ClassBan
    FROM dbo.ClassInfo_2026 c
    WHERE c.PersonID = p.PersonId
    ORDER BY c.updateDT DESC, c.InsertDT DESC
) ci
WHERE
    (p.fullName LIKE @LikeQ ESCAPE '\'
     OR p.UserName LIKE @LikeQ ESCAPE '\')
ORDER BY
    CASE
        WHEN p.fullName LIKE @PrefixQ ESCAPE '\' THEN 0
        WHEN p.UserName LIKE @PrefixQ ESCAPE '\' THEN 0
        ELSE 1
    END,
    p.fullName;
";

            static string EscapeLike(string input)
                => input.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");

            var safe = EscapeLike(q);
            var likeQ = "%" + safe + "%";
            var prefixQ = safe + "%";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });
            cmd.Parameters.Add(new SqlParameter("@LikeQ", SqlDbType.NVarChar, 200) { Value = likeQ });
            cmd.Parameters.Add(new SqlParameter("@PrefixQ", SqlDbType.NVarChar, 200) { Value = prefixQ });
 
            DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                // ✅ NULL 가능: PersonId (User에만 있는 경우)
                int? personId = rd["PersonId"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["PersonId"]);

                // ✅ UserId는 Guid일 수도 있음 → string으로 안전 처리
                string? userId = rd["UserId"] == DBNull.Value ? null : Convert.ToString(rd["UserId"])?.Trim();

                list.Add(new AddressBookPersonCardVM
                {
                    // ✅ VM도 int?가 가장 좋음
                    PersonId = personId,

                    UserId = userId,
                    FullName = Convert.ToString(rd["FullName"]) ?? "",
                    UserName = rd["UserName"] == DBNull.Value ? null : Convert.ToString(rd["UserName"]),
                    Email = rd["email"] == DBNull.Value ? null : Convert.ToString(rd["email"]),

                    PhoneNumber = rd["PhoneNumber"] == DBNull.Value ? null : Convert.ToString(rd["PhoneNumber"]),
                    BMonth = rd["bMonth"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["bMonth"]),
                    BDay = rd["bDay"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["bDay"]),

                    ClassYear = rd["ClassYear"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["ClassYear"]),
                    ClassBan = rd["ClassBan"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["ClassBan"]),

                    PhotoUrl = ResolvePhotoUrl(userId)
                });
            }

            return list;
        }

    }
}

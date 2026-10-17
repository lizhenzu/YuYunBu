using BulletinBoard.ViewModels.Worldview;
using BulletinBoard.ViewModels.Worldview.Admin;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BulletinBoard.Services.WorldView.Admin
{
    public class WorldviewAdminRepository
    {
        private readonly IConfiguration _cfg;
        private readonly string _connStr;

        public WorldviewAdminRepository(IConfiguration cfg)
        {
            _cfg = cfg;
            _connStr = cfg.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' not found.");
        }

        private SqlConnection GetConn()
            => new SqlConnection(_cfg.GetConnectionString("DefaultConnection"));

        // ✅ Admin 카드 목록(표시용): HeaderTitle%
        public List<HeaderTitleCardRow> GetHeaderTitleCards()
        {
            const string sql = @"
SELECT ContentId, LessonNo, SectionKey, ContentText
FROM dbo.WorldviewLessonContent
WHERE SectionKey LIKE 'HeaderTitle%'
ORDER BY LessonNo;";

            var list = new List<HeaderTitleCardRow>();
            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new HeaderTitleCardRow
                {
                    ContentId = r.GetInt32(0),
                    LessonNo = r.GetInt32(1),   // ✅ 핵심
                    SectionKey = r.GetString(2),
                    Title = r.IsDBNull(3) ? "" : r.GetString(3)
                });
            }
            return list;
        }

        
        
        // WeeklyWordToRead의 DateFrom~DateTo 범위로 오늘 날짜에 해당하는 과 번호 반환
        public int GetActiveLessonNo()
        {
            var kstNow = GetKoreaNow().Date;

            const string sql = @"
SELECT TOP 1 WeekTitle_GUA
FROM dbo.WeeklyWordToRead
WHERE cYear = @Year
  AND DateFrom IS NOT NULL
  AND DateTo   IS NOT NULL
  AND CAST(DateFrom AS date) <= @Today
  AND CAST(DateTo   AS date) >= @Today
ORDER BY DateFrom ASC;";

            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Year",  SqlDbType.Int).Value      = kstNow.Year;
            cmd.Parameters.Add("@Today", SqlDbType.Date).Value     = kstNow;

            conn.Open();
            var obj = cmd.ExecuteScalar();
            return obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
        }

        private static DateTime GetKoreaNow()
        {
            var utc = DateTime.UtcNow;
            try
            {
                var kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utc, kst);
            }
            catch
            {
                return utc.AddHours(9);
            }
        }

        // ✅ (A안) 활성 LessonNo 변경
        public void SetActiveLessonNo(int lessonNo, string userName)
        {
            using var conn = GetConn();
            using var cmd = new SqlCommand("dbo.Worldview_SetActiveLessonNo", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@ActiveLessonNo", SqlDbType.Int).Value = lessonNo;
            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = userName ?? "";

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

}

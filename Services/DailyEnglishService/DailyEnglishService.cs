using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StockBulletinBoard.Models;

namespace StockBulletinBoard.Services
{
    public class DailyEnglishService : IDailyEnglishService
    {
        private readonly string _connStr;

        // ✅ .NET 8 방식 (DI)
        public DailyEnglishService(IConfiguration configuration)
        {
            _connStr = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ 가장 적게 읽힌 문장 반환 (IsRead 최솟값, 동점이면 Id 오름차순)
        public DailyEnglish GetNext()
        {
            DailyEnglish item = null;
            const string sql = @"
                SELECT TOP 1 *
                FROM DailyEnglish
                ORDER BY IsRead ASC, Id ASC";

            using var conn = new SqlConnection(_connStr);
            using var cmd  = new SqlCommand(sql, conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            if (r.Read()) item = Map(r);
            return item;
        }

        // ✅ 화면에 보일 때마다 IsRead +1
        public void IncrementIsRead(int id)
        {
            const string sql = "UPDATE DailyEnglish SET IsRead = IsRead + 1 WHERE Id = @Id";
            using var conn = new SqlConnection(_connStr);
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ✅ 읽음 버튼: IsRead +1 (버튼 클릭 추가 가중치)
        public void MarkAsRead(int id)
        {
            IncrementIsRead(id);  // 버튼 클릭 시 추가 +1
        }

        // ✅ 초기화: 모든 IsRead = 0
        public void Reset()
        {
            const string sql = "UPDATE DailyEnglish SET IsRead = 0";
            using var conn = new SqlConnection(_connStr);
            using var cmd  = new SqlCommand(sql, conn);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public List<DailyEnglish> GetAll()
        {
            var list = new List<DailyEnglish>();

            string sql = "SELECT * FROM DailyEnglish ORDER BY Id DESC";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(Map(reader));
                    }
                }
            }

            return list;
        }

        public DailyEnglish GetById(int id)
        {
            DailyEnglish item = null;

            string sql = "SELECT * FROM DailyEnglish WHERE Id = @Id";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item = Map(reader);
                    }
                }
            }

            return item;
        }

        public void Insert(DailyEnglish item)
        {
            string sql = @"
                INSERT INTO DailyEnglish
                (EnglishText, KoreanText, Pronunciation, Description, Example, ExampleKorean, VocabNote, IsRead, CreatedAt)
                VALUES
                (@EnglishText, @KoreanText, @Pronunciation, @Description, @Example, @ExampleKorean, @VocabNote, 0, GETDATE())
            ";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@EnglishText", item.EnglishText ?? "");
                cmd.Parameters.AddWithValue("@KoreanText", item.KoreanText ?? "");
                cmd.Parameters.AddWithValue("@Pronunciation", item.Pronunciation ?? "");
                cmd.Parameters.AddWithValue("@Description", item.Description ?? "");
                cmd.Parameters.AddWithValue("@Example", item.Example ?? "");
                cmd.Parameters.AddWithValue("@ExampleKorean", item.ExampleKorean ?? "");
                cmd.Parameters.AddWithValue("@VocabNote", item.VocabNote ?? "");

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(DailyEnglish item)
        {
            string sql = @"
                UPDATE DailyEnglish
                SET EnglishText=@EnglishText,
                    KoreanText=@KoreanText,
                    Pronunciation=@Pronunciation,
                    Description=@Description,
                    Example=@Example,
                    ExampleKorean=@ExampleKorean,
                    VocabNote=@VocabNote
                WHERE Id=@Id
            ";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", item.Id);
                cmd.Parameters.AddWithValue("@EnglishText", item.EnglishText ?? "");
                cmd.Parameters.AddWithValue("@KoreanText", item.KoreanText ?? "");
                cmd.Parameters.AddWithValue("@Pronunciation", item.Pronunciation ?? "");
                cmd.Parameters.AddWithValue("@Description", item.Description ?? "");
                cmd.Parameters.AddWithValue("@Example", item.Example ?? "");
                cmd.Parameters.AddWithValue("@ExampleKorean", item.ExampleKorean ?? "");
                cmd.Parameters.AddWithValue("@VocabNote", item.VocabNote ?? "");

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public (int position, int total, int unread) GetPositionAndTotal(int currentId)
        {
            const string sql = @"
SELECT
    (SELECT COUNT(*) FROM DailyEnglish WHERE Id <= @Id)      AS Position,
    (SELECT COUNT(*) FROM DailyEnglish)                       AS Total,
    (SELECT COUNT(*) FROM DailyEnglish WHERE IsRead = 0)      AS Unread;";
            using var conn = new SqlConnection(_connStr);
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", currentId);
            conn.Open();
            using var r = cmd.ExecuteReader();
            if (r.Read())
                return (Convert.ToInt32(r["Position"]), Convert.ToInt32(r["Total"]), Convert.ToInt32(r["Unread"]));
            return (0, 0, 0);
        }

        public int? GetPrevId(int currentId)
        {
            const string sql = "SELECT MAX(Id) FROM DailyEnglish WHERE Id < @Id";
            using var conn = new SqlConnection(_connStr);
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", currentId);
            conn.Open();
            var obj = cmd.ExecuteScalar();
            return (obj == null || obj == DBNull.Value) ? (int?)null : Convert.ToInt32(obj);
        }

        public int? GetNextId(int currentId)
        {
            const string sql = "SELECT MIN(Id) FROM DailyEnglish WHERE Id > @Id";
            using var conn = new SqlConnection(_connStr);
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", currentId);
            conn.Open();
            var obj = cmd.ExecuteScalar();
            return (obj == null || obj == DBNull.Value) ? (int?)null : Convert.ToInt32(obj);
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM DailyEnglish WHERE Id = @Id";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private DailyEnglish Map(SqlDataReader reader)
        {
            return new DailyEnglish
            {
                Id = Convert.ToInt32(reader["Id"]),
                EnglishText = reader["EnglishText"].ToString(),
                KoreanText = reader["KoreanText"].ToString(),
                Pronunciation = reader["Pronunciation"].ToString(),
                Description = reader["Description"].ToString(),
                Example = reader["Example"].ToString(),
                ExampleKorean = reader["ExampleKorean"] == DBNull.Value ? "" : reader["ExampleKorean"].ToString(),
                VocabNote     = reader["VocabNote"] == DBNull.Value ? "" : reader["VocabNote"].ToString(),
                IsRead = Convert.ToInt32(reader["IsRead"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }

        // ─────────────────────────────────────────────
        // ✅ YouTube 영상 관리
        // ─────────────────────────────────────────────

        public List<DailyEnglishVideo> GetAllVideos()
        {
            var list = new List<DailyEnglishVideo>();
            const string sql = "SELECT Id, YouTubeId, Title, CreatedAt FROM DailyEnglishVideo ORDER BY Id DESC";
            using var conn = new SqlConnection(_connStr);
            using var cmd  = new SqlCommand(sql, conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(MapVideo(r));
            return list;
        }

        public DailyEnglishVideo GetVideoById(int id)
        {
            const string sql = "SELECT Id, YouTubeId, Title, CreatedAt FROM DailyEnglishVideo WHERE Id = @Id";
            using var conn = new SqlConnection(_connStr);
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapVideo(r) : null;
        }

        public void InsertVideo(string youTubeId, string title)
        {
            const string sql = "INSERT INTO DailyEnglishVideo (YouTubeId, Title, CreatedAt) VALUES (@Y, @T, GETDATE())";
            using var conn = new SqlConnection(_connStr);
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Y", youTubeId.Trim());
            cmd.Parameters.AddWithValue("@T", title.Trim());
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void DeleteVideo(int id)
        {
            const string sql = "DELETE FROM DailyEnglishVideo WHERE Id = @Id";
            using var conn = new SqlConnection(_connStr);
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        private static DailyEnglishVideo MapVideo(SqlDataReader r) => new DailyEnglishVideo
        {
            Id        = Convert.ToInt32(r["Id"]),
            YouTubeId = r["YouTubeId"].ToString(),
            Title     = r["Title"].ToString(),
            CreatedAt = Convert.ToDateTime(r["CreatedAt"])
        };
    }
}
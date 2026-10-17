using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Notice;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.Services
{
    public class NoticeService : INoticeService
    {
        private readonly string _connString;

        public NoticeService(IConfiguration configuration)
        {
            _connString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection() => new SqlConnection(_connString);

        // ✅ "클라이언트 기준"을 현실적으로 맞추기 위한 KST Now
        // (서버 타임존이 달라도, 한국 사용자 기준으로 저장되게)
        private static DateTime GetClientNow()
        {
            // Windows 서버: "Korea Standard Time"
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        // ✅ 배너용: 상단고정(IsPinned=1) 공지 여러 건(조회수 증가 없음)
        public List<Notice> GetPinnedNoticesForBanner(DateTime now, int maxN = 20)
        {
            var list = new List<Notice>();

            // "클라이언트 오늘" 기준 만료 처리
            var clientToday = now.Date;

            using (var conn = GetConnection())
            {
                conn.Open();

                // ✅ PopupUntil이 null이면 기간 제한 없음
                // ✅ PopupUntil이 있으면 '오늘' 이상인 것만 통과
                var sql = @"
SELECT TOP (@topN)
    Id, Title, Content, AuthorName, IsPinned, ViewCount,
    IsDeleted, CreatedAt, UpdatedAt, IsPopup, PopupUntil
FROM [Notice]
WHERE IsDeleted = 0
  AND IsPinned = 1
  AND (PopupUntil IS NULL OR CAST(PopupUntil AS date) >= @today)
ORDER BY CreatedAt DESC, Id DESC;
";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@topN", maxN);
                    cmd.Parameters.AddWithValue("@today", clientToday);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new Notice
                            {
                                Id = r.GetInt32(0),
                                Title = r.GetString(1),
                                Content = r.GetString(2),
                                AuthorName = r["AuthorName"] as string,
                                IsPinned = r.GetBoolean(4),
                                ViewCount = r.GetInt32(5),
                                IsDeleted = r.GetBoolean(6),
                                CreatedAt = r.GetDateTime(7),
                                UpdatedAt = r["UpdatedAt"] as DateTime?,
                                IsPopup = r.GetBoolean(9),
                                PopupUntil = r["PopupUntil"] as DateTime?
                            });
                        }
                    }
                }
            }

            return list;
        }

        // 1) 목록 + 페이징 + 검색
        public NoticeListVm GetNoticeList(int page, int pageSize, string keyword)
        {
            var items = new List<NoticeListItemVm>();
            int totalCount;

            using (var conn = GetConnection())
            {
                conn.Open();

                var countSql = @"
                    SELECT COUNT(1)
                    FROM [Notice]
                    WHERE IsDeleted = 0
                ";
                if (!string.IsNullOrWhiteSpace(keyword))
                    countSql += " AND (Title LIKE @kw OR Content LIKE @kw)";

                using (var cmd = new SqlCommand(countSql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                        cmd.Parameters.AddWithValue("@kw", $"%{keyword}%");

                    totalCount = (int)cmd.ExecuteScalar();
                }

                var listSql = @"
                    SELECT Id, Title, IsPinned, ViewCount, AuthorName, CreatedAt, IsPopup, PopupUntil
                    FROM [Notice]
                    WHERE IsDeleted = 0
                ";
                if (!string.IsNullOrWhiteSpace(keyword))
                    listSql += " AND (Title LIKE @kw2 OR Content LIKE @kw2)";

                listSql += @"
                    ORDER BY IsPinned DESC, CreatedAt DESC
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
                ";

                using (var cmd = new SqlCommand(listSql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                        cmd.Parameters.AddWithValue("@kw2", $"%{keyword}%");

                    cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@pageSize", pageSize);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            items.Add(new NoticeListItemVm
                            {
                                Id = r.GetInt32(0),
                                Title = r.GetString(1),
                                IsPinned = r.GetBoolean(2),
                                ViewCount = r.GetInt32(3),
                                AuthorName = r["AuthorName"] as string,
                                CreatedAt = r.GetDateTime(5),
                                IsPopup = r.GetBoolean(6),
                                PopupUntil = r["PopupUntil"] as DateTime?
                            });
                        }
                    }
                }
            }

            return new NoticeListVm
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Keyword = keyword
            };
        }

        // 2) 상세 + 조회수 증가
        public Notice GetNoticeAndIncreaseViewCount(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(
                    "UPDATE [Notice] SET ViewCount = ViewCount + 1 WHERE Id=@id AND IsDeleted=0;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                var sql = @"
                    SELECT Id, Title, Content, AuthorName, IsPinned, ViewCount,
                           IsDeleted, CreatedAt, UpdatedAt, IsPopup, PopupUntil
                    FROM [Notice]
                    WHERE Id=@id AND IsDeleted=0;
                ";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        return new Notice
                        {
                            Id = r.GetInt32(0),
                            Title = r.GetString(1),
                            Content = r.GetString(2),
                            AuthorName = r["AuthorName"] as string,
                            IsPinned = r.GetBoolean(4),
                            ViewCount = r.GetInt32(5),
                            IsDeleted = r.GetBoolean(6),
                            CreatedAt = r.GetDateTime(7),
                            UpdatedAt = r["UpdatedAt"] as DateTime?,
                            IsPopup = r.GetBoolean(9),
                            PopupUntil = r["PopupUntil"] as DateTime?
                        };
                    }
                }
            }
        }

        // 3) Edit 화면용 조회 (공지 + 첨부)
        public NoticeEditVm GetNoticeForEdit(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                var sql = @"
                    SELECT Id, Title, Content, AuthorName, IsPinned, IsPopup, PopupUntil
                    FROM [Notice]
                    WHERE Id=@id AND IsDeleted=0;
                ";

                NoticeEditVm vm = null;

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        vm = new NoticeEditVm
                        {
                            Id = r.GetInt32(0),
                            Title = r.GetString(1),
                            Content = r.GetString(2),
                            AuthorName = r["AuthorName"] as string,
                            IsPinned = r.GetBoolean(4),
                            IsPopup = r.GetBoolean(5),
                            PopupUntil = r["PopupUntil"] as DateTime?
                        };
                    }
                }

                vm.Attachments = GetAttachmentsInternal(conn, id);
                return vm;
            }
        }

        // 4) Create (기존)
        public int CreateNotice(NoticeEditVm vm) => CreateNotice(vm, null);

        // 4-1) Create + attachments
        public int CreateNotice(NoticeEditVm vm, List<NoticeAttachmentCreateDto> attachments)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        var now = GetClientNow();

                        var insertSql = @"
                            INSERT INTO [Notice]
                                (Title, Content, AuthorName, IsPinned, IsPopup, PopupUntil, ViewCount, IsDeleted, CreatedAt)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@Title, @Content, @AuthorName, @IsPinned, @IsPopup, @PopupUntil, 0, 0, @CreatedAt);
                        ";

                        int newId;
                        using (var cmd = new SqlCommand(insertSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Title", vm.Title);
                            cmd.Parameters.AddWithValue("@Content", vm.Content);
                            cmd.Parameters.AddWithValue("@AuthorName", (object)vm.AuthorName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsPinned", vm.IsPinned);
                            cmd.Parameters.AddWithValue("@IsPopup", vm.IsPopup);
                            cmd.Parameters.AddWithValue("@PopupUntil", (object)vm.PopupUntil ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedAt", now);

                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (attachments != null && attachments.Count > 0)
                            InsertAttachments(conn, tx, newId, attachments, now);

                        tx.Commit();
                        return newId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // 5) Update (기존)
        public bool UpdateNotice(NoticeEditVm vm) => UpdateNotice(vm, null, null);

        // 5-1) Update + add attachments + delete attachments
        public bool UpdateNotice(NoticeEditVm vm, List<NoticeAttachmentCreateDto> attachments, List<long> deleteAttachmentIds)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        var now = GetClientNow();

                        var sql = @"
                            UPDATE [Notice]
                            SET Title=@Title,
                                Content=@Content,
                                AuthorName=@AuthorName,
                                IsPinned=@IsPinned,
                                IsPopup=@IsPopup,
                                PopupUntil=@PopupUntil,
                                UpdatedAt=@UpdatedAt
                            WHERE Id=@Id AND IsDeleted=0;
                        ";

                        int affected;
                        using (var cmd = new SqlCommand(sql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Title", vm.Title);
                            cmd.Parameters.AddWithValue("@Content", vm.Content);
                            cmd.Parameters.AddWithValue("@AuthorName", (object)vm.AuthorName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsPinned", vm.IsPinned);
                            cmd.Parameters.AddWithValue("@IsPopup", vm.IsPopup);
                            cmd.Parameters.AddWithValue("@PopupUntil", (object)vm.PopupUntil ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@UpdatedAt", now);
                            cmd.Parameters.AddWithValue("@Id", vm.Id.Value);

                            affected = cmd.ExecuteNonQuery();
                        }

                        if (affected <= 0)
                        {
                            tx.Rollback();
                            return false;
                        }

                        if (deleteAttachmentIds != null && deleteAttachmentIds.Count > 0)
                            SoftDeleteAttachments(conn, tx, vm.Id.Value, deleteAttachmentIds);

                        if (attachments != null && attachments.Count > 0)
                            InsertAttachments(conn, tx, vm.Id.Value, attachments, now);

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // 6) Soft Delete (공지 + 첨부 soft delete)
        public bool SoftDeleteNotice(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        var now = GetClientNow();

                        int affected;
                        using (var cmd = new SqlCommand(
                            "UPDATE [Notice] SET IsDeleted=1, UpdatedAt=@UpdatedAt WHERE Id=@id AND IsDeleted=0;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@UpdatedAt", now);
                            affected = cmd.ExecuteNonQuery();
                        }

                        if (affected > 0)
                        {
                            using (var cmd = new SqlCommand(
                                "UPDATE dbo.NoticeAttachment SET IsDeleted=1 WHERE NoticeId=@id AND IsDeleted=0;", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return affected > 0;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // 7) Attachment API
        public List<NoticeAttachment> GetAttachments(int noticeId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                return GetAttachmentsInternal(conn, noticeId);
            }
        }

        public NoticeAttachment GetAttachment(long attachmentId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                var sql = @"
                    SELECT Id, NoticeId, OriginalName, StoredName, ContentType, FileSize, UploadedAt, IsDeleted
                    FROM dbo.NoticeAttachment
                    WHERE Id=@id AND IsDeleted=0;
                ";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", attachmentId);

                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        return new NoticeAttachment
                        {
                            Id = r.GetInt64(0),
                            NoticeId = r.GetInt32(1),
                            OriginalName = r.GetString(2),
                            StoredName = r.GetString(3),
                            ContentType = r["ContentType"] as string,
                            FileSize = r.GetInt64(5),
                            UploadedAt = r.GetDateTime(6),
                            IsDeleted = r.GetBoolean(7)
                        };
                    }
                }
            }
        }

        // 8) Popup notices
        public List<Notice> GetActivePopupNotices(DateTime now, int topN = 5)
        {
            var list = new List<Notice>();

            using (var conn = GetConnection())
            {
                conn.Open();

                var sql = @"
                    SELECT TOP (@topN)
                           Id, Title, Content, AuthorName, IsPinned, ViewCount,
                           IsDeleted, CreatedAt, UpdatedAt, IsPopup, PopupUntil
                    FROM dbo.Notice
                    WHERE IsDeleted=0
                      AND IsPopup=1
                      AND (PopupUntil IS NULL OR PopupUntil >= @now)
                    ORDER BY IsPinned DESC, CreatedAt DESC;
                ";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@topN", topN);
                    cmd.Parameters.AddWithValue("@now", now);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new Notice
                            {
                                Id = r.GetInt32(0),
                                Title = r.GetString(1),
                                Content = r.GetString(2),
                                AuthorName = r["AuthorName"] as string,
                                IsPinned = r.GetBoolean(4),
                                ViewCount = r.GetInt32(5),
                                IsDeleted = r.GetBoolean(6),
                                CreatedAt = r.GetDateTime(7),
                                UpdatedAt = r["UpdatedAt"] as DateTime?,
                                IsPopup = r.GetBoolean(9),
                                PopupUntil = r["PopupUntil"] as DateTime?
                            });
                        }
                    }
                }
            }

            return list;
        }

        // ----------------- internal helpers -----------------

        private List<NoticeAttachment> GetAttachmentsInternal(SqlConnection conn, int noticeId)
        {
            var list = new List<NoticeAttachment>();

            var sql = @"
                SELECT Id, NoticeId, OriginalName, StoredName, ContentType, FileSize, UploadedAt, IsDeleted
                FROM dbo.NoticeAttachment
                WHERE NoticeId=@nid AND IsDeleted=0
                ORDER BY Id ASC;
            ";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@nid", noticeId);

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new NoticeAttachment
                        {
                            Id = r.GetInt64(0),
                            NoticeId = r.GetInt32(1),
                            OriginalName = r.GetString(2),
                            StoredName = r.GetString(3),
                            ContentType = r["ContentType"] as string,
                            FileSize = r.GetInt64(5),
                            UploadedAt = r.GetDateTime(6),
                            IsDeleted = r.GetBoolean(7)
                        });
                    }
                }
            }

            return list;
        }

        private void InsertAttachments(SqlConnection conn, SqlTransaction tx, int noticeId, List<NoticeAttachmentCreateDto> attachments, DateTime now)
        {
            var sql = @"
                INSERT INTO dbo.NoticeAttachment
                    (NoticeId, OriginalName, StoredName, ContentType, FileSize, UploadedAt, IsDeleted)
                VALUES
                    (@NoticeId, @OriginalName, @StoredName, @ContentType, @FileSize, @UploadedAt, 0);
            ";

            foreach (var a in attachments)
            {
                using (var cmd = new SqlCommand(sql, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@NoticeId", noticeId);
                    cmd.Parameters.AddWithValue("@OriginalName", a.OriginalName ?? "");
                    cmd.Parameters.AddWithValue("@StoredName", a.StoredName ?? "");
                    cmd.Parameters.AddWithValue("@ContentType", (object)a.ContentType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileSize", a.FileSize);
                    cmd.Parameters.AddWithValue("@UploadedAt", now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void SoftDeleteAttachments(SqlConnection conn, SqlTransaction tx, int noticeId, List<long> ids)
        {
            var paramNames = new List<string>();
            for (int i = 0; i < ids.Count; i++) paramNames.Add("@p" + i);

            var sql = $@"
                UPDATE dbo.NoticeAttachment
                SET IsDeleted=1
                WHERE NoticeId=@nid AND Id IN ({string.Join(",", paramNames)}) AND IsDeleted=0;
            ";

            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@nid", noticeId);
                for (int i = 0; i < ids.Count; i++)
                    cmd.Parameters.AddWithValue(paramNames[i], ids[i]);

                cmd.ExecuteNonQuery();
            }
        }
    }
}

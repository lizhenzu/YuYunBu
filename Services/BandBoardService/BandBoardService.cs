// Services/BandBoardService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BulletinBoard.Common;
using BulletinBoard.Models;
using BulletinBoard.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.Services
{
    public class BandBoardService : IBandBoardService
    {
        private readonly string _connectionString;

        public BandBoardService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection() => new SqlConnection(_connectionString);

        /// <summary>
        /// 게시판용 테이블 3종 이름 계산
        /// - postsTableName: Posts 테이블 이름 (예: BandPosts, BandJuboPosts, BandAPosts)
        /// - Comments/Images는 네이밍 규칙에 맞춰 자동 생성
        ///   * BandPosts      -> BandPostComments / BandPostImages
        ///   * BandJuboPosts  -> BandJuboPostComments / BandJuboPostImages
        ///   * BandAPosts     -> BandAPostComments / BandAPostImages
        /// </summary>
        private (string Posts, string Comments, string Images) GetBoardTableNames(string postsTableName)
        {
            if (string.IsNullOrWhiteSpace(postsTableName))
            {
                postsTableName = "BandPosts";
            }

            // 간단한 안전장치 (SQL Injection 방지용: 알파벳/숫자/_ 만 허용)
            if (!Regex.IsMatch(postsTableName, @"^[A-Za-z0-9_]+$"))
            {
                throw new ArgumentException("잘못된 게시판 테이블 이름입니다.", nameof(postsTableName));
            }

            string commentsTable;
            string imagesTable;

            if (postsTableName.EndsWith("Posts", StringComparison.OrdinalIgnoreCase))
            {
                var prefix = postsTableName.Substring(0, postsTableName.Length - "Posts".Length);
                commentsTable = prefix + "PostComments";
                imagesTable = prefix + "PostImages";
            }
            else
            {
                commentsTable = postsTableName + "Comments";
                imagesTable = postsTableName + "Images";
            }

            return (postsTableName, commentsTable, imagesTable);
        }

        /// <summary>
        /// 페이지 단위로 게시글 + 이미지 + 댓글 모두 조회
        /// </summary>
        public async Task<PagedPostsResult> GetPostsPageAsync(string boardTableName, int page, int pageSize)
        {
            if (page < 1) page = 1;
            int offset = (page - 1) * pageSize;

            var (postsTable, commentsTable, imagesTable) = GetBoardTableNames(boardTableName);
            var posts = new List<BandPost>();

            using (var conn = GetConnection())
            {
                await conn.OpenAsync();

                // 1) 게시글 페이지 조회
                string sqlPosts = $@"
SELECT Id, WriterName, Content, LikeCount, CreatedAt
FROM {postsTable}
ORDER BY CreatedAt DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
";

                using (var cmd = new SqlCommand(sqlPosts, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = offset });
                    cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        posts.Add(new BandPost
                        {
                            Id = reader.GetInt32(0),
                            WriterName = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Content = reader.IsDBNull(2) ? null : reader.GetString(2),
                            LikeCount = reader.GetInt32(3),
                            CreatedAt = reader.GetDateTime(4),
                            Images = new List<BandPostImage>(),
                            Comments = new List<BandPostComment>()
                        });
                    }
                }

                if (!posts.Any())
                {
                    return new PagedPostsResult
                    {
                        Posts = posts,
                        HasMore = false,
                        NextPage = 0
                    };
                }

                var ids = posts.Select(p => p.Id).ToList();
                string idList = string.Join(",", ids);

                // 2) 이미지 조회
                string sqlImages = $@"
SELECT Id, BandPostId, ImagePath
FROM {imagesTable}
WHERE BandPostId IN ({idList});
";

                using (var cmdImg = new SqlCommand(sqlImages, conn))
                using (var readerImg = await cmdImg.ExecuteReaderAsync())
                {
                    while (await readerImg.ReadAsync())
                    {
                        int bandPostId = readerImg.GetInt32(1);
                        var post = posts.FirstOrDefault(p => p.Id == bandPostId);
                        if (post == null) continue;

                        post.Images.Add(new BandPostImage
                        {
                            Id = readerImg.GetInt32(0),
                            BandPostId = bandPostId,
                            ImagePath = readerImg.IsDBNull(2) ? null : readerImg.GetString(2)
                        });
                    }
                }

                // 3) 댓글 조회
                string sqlComments = $@"
SELECT Id, BandPostId, WriterName, Content, CreatedAt
FROM {commentsTable}
WHERE BandPostId IN ({idList})
ORDER BY CreatedAt ASC;
";

                using (var cmdCmt = new SqlCommand(sqlComments, conn))
                using (var readerCmt = await cmdCmt.ExecuteReaderAsync())
                {
                    while (await readerCmt.ReadAsync())
                    {
                        int bandPostId = readerCmt.GetInt32(1);
                        var post = posts.FirstOrDefault(p => p.Id == bandPostId);
                        if (post == null) continue;

                        post.Comments.Add(new BandPostComment
                        {
                            Id = readerCmt.GetInt32(0),
                            BandPostId = bandPostId,
                            WriterName = readerCmt.IsDBNull(2) ? null : readerCmt.GetString(2),
                            Content = readerCmt.IsDBNull(3) ? null : readerCmt.GetString(3),
                            CreatedAt = readerCmt.GetDateTime(4)
                        });
                    }
                }

                // 4) 더 있는지 확인 (HasMore)
                int minId = posts.Min(p => p.Id);
                string sqlHasMore = $@"
SELECT CASE 
         WHEN EXISTS (SELECT 1 FROM {postsTable} WHERE Id < @MinId) 
         THEN 1 ELSE 0 
       END;
";

                bool hasMore;
                using (var cmdMore = new SqlCommand(sqlHasMore, conn))
                {
                    cmdMore.Parameters.Add(new SqlParameter("@MinId", SqlDbType.Int) { Value = minId });
                    var result = (int)await cmdMore.ExecuteScalarAsync();
                    hasMore = result == 1;
                }

                return new PagedPostsResult
                {
                    Posts = posts,
                    HasMore = hasMore,
                    NextPage = hasMore ? page + 1 : 0
                };
            }
        }

        /// <summary>
        /// 글 + 이미지 등록 (이미지는 미리 저장해둔 경로 리스트)
        /// </summary>
        public async Task<int> CreatePostAsync(string boardTableName, string writerName, string content, IEnumerable<string> imagePaths)
        {
            DateTime createdAt = SharedComm.GetKoreaNow();
            var (postsTable, _, imagesTable) = GetBoardTableNames(boardTableName);

            writerName = string.IsNullOrWhiteSpace(writerName) ? "익명" : writerName.Trim();
            content = string.IsNullOrWhiteSpace(content) ? null : content.Trim();
            imagePaths ??= Array.Empty<string>();

            int newId;

            using (var conn = GetConnection())
            {
                await conn.OpenAsync();
                using var tran = await conn.BeginTransactionAsync();

                try
                {
                    // 1) 게시글 INSERT
                    string sqlInsertPost = $@"
INSERT INTO {postsTable} (WriterName, Content, LikeCount, CreatedAt)
VALUES (@WriterName, @Content, 0, @CreatedAt);
SELECT CAST(SCOPE_IDENTITY() AS int);
";
                    using (var cmd = new SqlCommand(sqlInsertPost, conn, (SqlTransaction)tran))
                    {
                        cmd.Parameters.Add(new SqlParameter("@WriterName", SqlDbType.NVarChar, 50) { Value = (object?)writerName ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.NVarChar, -1) { Value = (object?)content ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = createdAt });

                        newId = (int)await cmd.ExecuteScalarAsync();
                    }

                    // 2) 이미지 INSERT
                    foreach (var path in imagePaths.Where(p => !string.IsNullOrWhiteSpace(p)))
                    {
                        string sqlInsertImg = $@"
INSERT INTO {imagesTable} (BandPostId, ImagePath)
VALUES (@BandPostId, @ImagePath);
";
                        using var cmdImg = new SqlCommand(sqlInsertImg, conn, (SqlTransaction)tran);
                        cmdImg.Parameters.Add(new SqlParameter("@BandPostId", SqlDbType.Int) { Value = newId });
                        cmdImg.Parameters.Add(new SqlParameter("@ImagePath", SqlDbType.NVarChar, 300) { Value = path });
                        await cmdImg.ExecuteNonQueryAsync();
                    }

                    await tran.CommitAsync();
                }
                catch
                {
                    await tran.RollbackAsync();
                    throw;
                }
            }

            return newId;
        }

        /// <summary>
        /// 좋아요 1 증가 후 현재 LikeCount 반환
        /// </summary>
        public async Task<int> IncrementLikeAsync(string boardTableName, int postId)
        {
            var (postsTable, _, _) = GetBoardTableNames(boardTableName);

            using var conn = GetConnection();
            await conn.OpenAsync();

            string sql = $@"
UPDATE {postsTable}
SET LikeCount = LikeCount + 1
WHERE Id = @Id;

SELECT LikeCount FROM {postsTable} WHERE Id = @Id;
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = postId });

            var result = await cmd.ExecuteScalarAsync();
            return result == null ? 0 : (int)result;
        }

        /// <summary>
        /// 댓글 추가 후 방금 추가된 댓글 객체 반환
        /// </summary>
        public async Task<BandPostComment> AddCommentAsync(string boardTableName, int postId, string writerName, string content)
        {
            DateTime createdAt = SharedComm.GetKoreaNow();
            var (_, commentsTable, _) = GetBoardTableNames(boardTableName);

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("댓글 내용이 비어 있습니다.", nameof(content));

            writerName = string.IsNullOrWhiteSpace(writerName) ? "익명" : writerName.Trim();
            content = content.Trim();

            using var conn = GetConnection();
            await conn.OpenAsync();

            string sql = $@"
INSERT INTO {commentsTable} (BandPostId, WriterName, Content, CreatedAt)
VALUES (@BandPostId, @WriterName, @Content, @CreatedAt);
SELECT CAST(SCOPE_IDENTITY() AS int);
";

            int newId;

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@BandPostId", SqlDbType.Int) { Value = postId });
                cmd.Parameters.Add(new SqlParameter("@WriterName", SqlDbType.NVarChar, 50) { Value = writerName });
                cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.NVarChar, 1000) { Value = content });
                cmd.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = createdAt });

                newId = (int)await cmd.ExecuteScalarAsync();
            }

            return new BandPostComment
            {
                Id = newId,
                BandPostId = postId,
                WriterName = writerName,
                Content = content,
                CreatedAt = createdAt
            };
        }

        /// <summary>
        /// 게시글 + 이미지 + 댓글 삭제
        /// </summary>
        public async Task DeletePostAsync(string boardTableName, int postId)
        {
            var (postsTable, commentsTable, imagesTable) = GetBoardTableNames(boardTableName);

            using var conn = GetConnection();
            await conn.OpenAsync();
            using var tran = await conn.BeginTransactionAsync();

            try
            {
                string sqlDelComments = $"DELETE FROM {commentsTable} WHERE BandPostId = @PostId;";
                string sqlDelImages = $"DELETE FROM {imagesTable}  WHERE BandPostId = @PostId;";
                string sqlDelPost = $"DELETE FROM {postsTable}      WHERE Id        = @PostId;";

                using (var cmdC = new SqlCommand(sqlDelComments, conn, (SqlTransaction)tran))
                {
                    cmdC.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
                    await cmdC.ExecuteNonQueryAsync();
                }

                using (var cmdI = new SqlCommand(sqlDelImages, conn, (SqlTransaction)tran))
                {
                    cmdI.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
                    await cmdI.ExecuteNonQueryAsync();
                }

                using (var cmdP = new SqlCommand(sqlDelPost, conn, (SqlTransaction)tran))
                {
                    cmdP.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
                    await cmdP.ExecuteNonQueryAsync();
                }

                await tran.CommitAsync();
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<(int Total, int Today)> GetPostCountsAsync(string boardTableName)
        {
            var (postsTable, _, _) = GetBoardTableNames(boardTableName);
            using var conn = GetConnection();
            await conn.OpenAsync();
            string sql = $@"
SELECT
    COUNT(*) AS Total,
    SUM(CASE WHEN CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS Today
FROM {postsTable};";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return (reader.GetInt32(0), reader.IsDBNull(1) ? 0 : reader.GetInt32(1));
            return (0, 0);
        }
    }
}

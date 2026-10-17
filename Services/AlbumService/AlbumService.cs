using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace YunYunBu2024.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly string _cs;
        private readonly IFileStorage _storage;

        public AlbumService(IConfiguration config, IFileStorage storage)
        {
            _cs = config.GetConnectionString("DefaultConnection");
            _storage = storage;
        }

        #region Thumbnail Helper

        /// <summary>
        /// 업로드된 원본 IFormFile로부터 썸네일 이미지를 생성하고
        /// /uploads/{albumId}/thumbs 아래에 JPEG로 저장한 후 가상 경로를 반환합니다.
        /// </summary>
        private async Task<string> CreateAndSaveThumbnailAsync(int albumId, IFormFile file)
        {
            const int thumbMaxSize = 400;

            await using var srcStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(srcStream);

            // 긴 변 기준으로 리사이즈 (비율 유지, 최대 400px)
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(thumbMaxSize, thumbMaxSize)
            }));

            var thumbName = $"{Guid.NewGuid():N}_thumb.jpg";

            var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms, new JpegEncoder
            {
                Quality = 80
            });

            ms.Position = 0;

            // /uploads/{albumId}/thumbs/{thumbName}
            var thumbPath = await _storage.SaveAsync(albumId, ms, thumbName, "thumbs");
            return thumbPath;
        }

        #endregion

        #region Public API

        public async Task<IReadOnlyList<Album>> GetAlbumsAsync()
        {
            var sql = @"
SELECT a.Id,
       a.Name,
       a.Description,
       a.UserName,
       a.CreatedAt,
       a.CoverPhotoId,
       p.Id                   AS P_Id,
       p.AlbumId              AS P_AlbumId,
       p.FileName             AS P_FileName,
       p.UserName             AS P_UserName,
       p.OriginalName         AS P_OriginalName,
       p.ContentType          AS P_ContentType,
       p.FileSizeBytes        AS P_FileSizeBytes,
       p.Width                AS P_Width,
       p.Height               AS P_Height,
       p.HashSha256           AS P_HashSha256,
       p.StoragePath          AS P_StoragePath,
       p.ThumbnailStoragePath AS P_ThumbnailStoragePath,
       p.UploadedAt           AS P_UploadedAt
FROM dbo.Albums a
LEFT JOIN dbo.Photos p ON p.Id = a.CoverPhotoId
ORDER BY a.CreatedAt DESC;";

            var list = new List<Album>();

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            await using (var cmd = new SqlCommand(sql, conn))
            await using (var r = await cmd.ExecuteReaderAsync())
            {
                while (await r.ReadAsync())
                {
                    var a = MapAlbum(r);

                    // 커버 사진이 존재하는 경우에만 매핑
                    if (!r.IsDBNull(r.GetOrdinal("P_Id")))
                    {
                        a.CoverPhoto = MapPhotoPrefixed(r);
                    }

                    list.Add(a);
                }
            }

            return list;
        }

        public async Task<Album> GetAlbumAsync(int id)
        {
            var sql = @"
SELECT Id, Name, UserName, Description, CreatedAt, CoverPhotoId
FROM dbo.Albums WHERE Id = @id;

SELECT Id,
       AlbumId,
       FileName,
       UserName,
       OriginalName,
       ContentType,
       FileSizeBytes,
       Width,
       Height,
       HashSha256,
       StoragePath,
       ThumbnailStoragePath,
       UploadedAt
FROM dbo.Photos
WHERE AlbumId = @id
ORDER BY Id DESC;";

            Album album = null;
            var photos = new List<Photo>();

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = id });

            await using var reader = await cmd.ExecuteReaderAsync();

            // Albums
            if (await reader.ReadAsync())
            {
                album = MapAlbum(reader);
            }

            // Photos
            if (album != null && await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                    photos.Add(MapPhoto(reader));

                album.Photos = photos;
            }

            return album;
        }

        public async Task<Album> CreateAlbumAsync(string name, string description, string UserName)
        {
            var sql = @"
INSERT INTO dbo.Albums(Name, Description, UserName, CreatedAt)
VALUES (@name, @desc,@UserName, @CreatedAt);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int newId;

            await using (var conn = new SqlConnection(_cs))
            {
                await conn.OpenAsync();
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 200) { Value = (object)name ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@desc", SqlDbType.NVarChar, 1000) { Value = (object)description ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 100) { Value = (object)UserName ?? DBNull.Value });
                cmd.Parameters.AddWithValue("@CreatedAt", DateHelper.GetKoreaNow());

                await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

                var obj = await cmd.ExecuteScalarAsync();
                newId = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            }

            return new Album
            {
                Id = newId,
                Name = name,
                UserName = UserName,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<int> UploadAsync(int albumId, IEnumerable<IFormFile> files, string UserName, bool avoidDuplicates = true)
        {
            if (!await AlbumExistsAsync(albumId))
                throw new InvalidOperationException("Album not found");

            int saved = 0;

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                foreach (var file in files.Where(f => f?.Length > 0))
                {
                    byte[] hash = null;

                    if (avoidDuplicates)
                    {
                        using var sha = SHA256.Create();
                        using var s = file.OpenReadStream();
                        hash = sha.ComputeHash(s);
                        s.Position = 0;
                    }

                    if (avoidDuplicates && hash != null)
                    {
                        var exists = await PhotoHashExistsAsync(conn, tran, albumId, hash);
                        if (exists) continue;
                    }

                    var ext = Path.GetExtension(file.FileName);
                    var newName = $"{Guid.NewGuid():N}{ext}";
                    var storagePath = await _storage.SaveAsync(albumId, file, newName);

                    // 썸네일 생성
                    string thumbPath = null;
                    try
                    {
                        thumbPath = await CreateAndSaveThumbnailAsync(albumId, file);
                    }
                    catch (Exception ex)
                    {
                        await DbLogger.LogErrorAsync($"Thumbnail fail: {ex}");
                    }

                    var insertSql = @"
INSERT INTO dbo.Photos
    (AlbumId,
     FileName,
     UserName,
     OriginalName,
     ContentType,
     FileSizeBytes,
     Width,
     Height,
     HashSha256,
     StoragePath,
     ThumbnailStoragePath,
     UploadedAt)
VALUES
    (@AlbumId,
     @FileName,
     @UserName,
     @OriginalName,
     @ContentType,
     @FileSizeBytes,
     NULL,
     NULL,
     @HashSha256,
     @StoragePath,
     @ThumbPath,
     @UploadedAt);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    using var cmd = new SqlCommand(insertSql, conn, tran);
                    cmd.Parameters.Add(new SqlParameter("@AlbumId", SqlDbType.Int) { Value = albumId });
                    cmd.Parameters.Add(new SqlParameter("@FileName", SqlDbType.NVarChar, 260) { Value = newName });
                    cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 260) { Value = UserName });
                    cmd.Parameters.Add(new SqlParameter("@OriginalName", SqlDbType.NVarChar, 260) { Value = (object)file.FileName ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@ContentType", SqlDbType.NVarChar, 100) { Value = (object)(file.ContentType ?? "application/octet-stream") });
                    cmd.Parameters.Add(new SqlParameter("@FileSizeBytes", SqlDbType.BigInt) { Value = file.Length });
                    cmd.Parameters.Add(new SqlParameter("@HashSha256", SqlDbType.VarBinary, 32) { Value = (object)hash ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@StoragePath", SqlDbType.NVarChar, 500) { Value = storagePath });
                    cmd.Parameters.Add(new SqlParameter("@ThumbPath", SqlDbType.NVarChar, 500) { Value = (object)thumbPath ?? DBNull.Value });
                    cmd.Parameters.AddWithValue("@UploadedAt", DateHelper.GetKoreaNow());

                    await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

                    var insertedIdObj = await cmd.ExecuteScalarAsync();
                    var insertedId = (insertedIdObj == null || insertedIdObj == DBNull.Value)
                        ? 0
                        : Convert.ToInt32(insertedIdObj);

                    // 커버 사진이 아직 없으면 지금 업로드된 사진을 커버로 지정
                    var coverCheckSql = @"
IF (SELECT CoverPhotoId FROM dbo.Albums WHERE Id=@aid) IS NULL
    UPDATE dbo.Albums SET CoverPhotoId=@pid WHERE Id=@aid;";
                    using var coverCmd = new SqlCommand(coverCheckSql, conn, tran);
                    coverCmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
                    coverCmd.Parameters.Add(new SqlParameter("@pid", SqlDbType.Int) { Value = insertedId });
                    await coverCmd.ExecuteNonQueryAsync();

                    saved++;
                }

                tran.Commit();
            }
            catch (Exception ex)
            {
                DbLogger.LogErrorAsync($"L Error : {ex.Message}");
                tran.Rollback();
                throw;
            }

            return saved;
        }

        public async Task DeletePhotosAsync(int albumId, IEnumerable<int> photoIds)
        {
            var ids = (photoIds ?? Enumerable.Empty<int>()).Distinct().ToArray();
            if (ids.Length == 0) return;

            await DbLogger.LogInfoAsync($"albumId : {albumId}");

            var targets = new List<(int Id, string StoragePath, string ThumbPath)>();
            int? coverPhotoId = null;

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                // 1) 현재 앨범의 커버 사진 Id 조회
                using (var coverCmd = new SqlCommand("SELECT CoverPhotoId FROM dbo.Albums WHERE Id=@aid;", conn, tran))
                {
                    coverCmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });

                    await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(coverCmd)}");

                    var obj = await coverCmd.ExecuteScalarAsync();
                    if (obj != null && obj != DBNull.Value)
                        coverPhotoId = Convert.ToInt32(obj);
                }

                // 2) 삭제 대상 사진(Id, StoragePath, ThumbnailStoragePath) 조회
                var idTableForSelect = string.Join(",", ids);
                var selectSql = $@"
SELECT Id, StoragePath, ThumbnailStoragePath
FROM dbo.Photos
WHERE AlbumId=@aid AND Id IN ({idTableForSelect});";

                using (var cmd = new SqlCommand(selectSql, conn, tran))
                {
                    cmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
                    using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                    {
                        var id = r.GetInt32(0);
                        var path = r.GetString(1);
                        var thumbPath = r.IsDBNull(2) ? null : r.GetString(2);
                        targets.Add((id, path, thumbPath));
                    }
                }

                if (targets.Count == 0)
                {
                    tran.Commit();
                    return;
                }

                var idTable = string.Join(",", targets.Select(t => t.Id));

                // 3) 커버가 삭제 대상에 포함되어 있으면, 삭제 전 다른 사진을 커버로 지정
                if (coverPhotoId.HasValue && targets.Any(t => t.Id == coverPhotoId.Value))
                {
                    var nextSql = $@"
DECLARE @nextId INT = (
    SELECT TOP(1) Id
    FROM dbo.Photos
    WHERE AlbumId=@aid AND Id NOT IN ({idTable})
    ORDER BY Id ASC
);
UPDATE dbo.Albums SET CoverPhotoId = @nextId WHERE Id=@aid;";
                    using var nextCmd = new SqlCommand(nextSql, conn, tran);
                    nextCmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
                    await nextCmd.ExecuteNonQueryAsync();
                }

                // 4) DB에서 사진 삭제
                var delSql = $@"
DELETE FROM dbo.Photos
WHERE AlbumId=@aid AND Id IN ({idTable});";

                using (var delCmd = new SqlCommand(delSql, conn, tran))
                {
                    delCmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
                    await delCmd.ExecuteNonQueryAsync();
                }

                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                await DbLogger.LogErrorAsync(ex.ToString());
                throw;
            }

            // 5) 트랜잭션 커밋 후 물리 파일 삭제 (원본 + 썸네일)
            foreach (var p in targets)
            {
                // 원본
                var physicalPath = _storage.MapPhysicalPath(p.StoragePath);
                if (File.Exists(physicalPath))
                {
                    try { _storage.Delete(p.StoragePath); }
                    catch (Exception ex) { await DbLogger.LogErrorAsync(ex.ToString()); }
                }

                // 썸네일
                if (!string.IsNullOrEmpty(p.ThumbPath))
                {
                    var thumbPhysical = _storage.MapPhysicalPath(p.ThumbPath);
                    if (File.Exists(thumbPhysical))
                    {
                        try { _storage.Delete(p.ThumbPath); }
                        catch (Exception ex) { await DbLogger.LogErrorAsync(ex.ToString()); }
                    }
                }
            }
        }

        public async Task DeleteAlbumAsync(int albumId)
        {
            // 0) 앨범 존재 확인
            if (!await AlbumExistsAsync(albumId))
                return;

            await DbLogger.LogInfoAsync($"albumId : {albumId}");

            var storagePaths = new List<string>();

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                // 1) 사진들의 StoragePath 조회 (원본 경로만)
                const string selectPhotosSql = @"SELECT StoragePath FROM dbo.Photos WHERE AlbumId=@aid;";
                using (var cmd = new SqlCommand(selectPhotosSql, conn, tran))
                {
                    cmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
                    using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                        storagePaths.Add(r.GetString(0));
                }

                // 1-1) 앨범의 CoverPhotoId 를 NULL 로 초기화 (FK 충돌 방지)
                const string clearCoverSql = @"UPDATE dbo.Albums SET CoverPhotoId = NULL WHERE Id = @aid;";
                using (var clearCmd = new SqlCommand(clearCoverSql, conn, tran))
                {
                    clearCmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
                    await clearCmd.ExecuteNonQueryAsync();
                }

                // 2) Photos 삭제
                const string deletePhotosSql = @"DELETE FROM dbo.Photos WHERE AlbumId=@aid;";
                using (var delPhotoCmd = new SqlCommand(deletePhotosSql, conn, tran))
                {
                    delPhotoCmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
                    await delPhotoCmd.ExecuteNonQueryAsync();
                }

                // 3) Albums 삭제
                const string deleteAlbumSql = @"DELETE FROM dbo.Albums WHERE Id=@aid;";
                using (var delAlbumCmd = new SqlCommand(deleteAlbumSql, conn, tran))
                {
                    delAlbumCmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
                    await delAlbumCmd.ExecuteNonQueryAsync();
                }

                tran.Commit();
            }
            catch (Exception ex)
            {
                DbLogger.LogErrorAsync($"L Error : {ex.Message}");
                tran.Rollback();
                throw;
            }

            // 5) DB 삭제 후 물리 파일 삭제 (폴더 통째 삭제도 아래에서 다시 수행)
            foreach (var sp in storagePaths)
            {
                try { _storage.Delete(sp); }
                catch { /* 필요 시 로그 */ }
            }

            // 6) 앨범 폴더 전체 삭제 (/uploads/{albumId} 안의 thumbs 포함)
            try
            {
                var albumVirtual = $"/uploads/{albumId}";
                var albumPhysical = _storage.MapPhysicalPath(albumVirtual);
                if (Directory.Exists(albumPhysical))
                    Directory.Delete(albumPhysical, recursive: true);
            }
            catch (Exception ex)
            {
                await DbLogger.LogErrorAsync(ex.ToString());
            }
        }

        public async Task DeletePhotoAsync(int photoId)
        {
            Photo p = null;
            int? albumId = null;
            int? coverPhotoId = null;

            await DbLogger.LogInfoAsync($"photoId : {photoId}");

            await using (var conn = new SqlConnection(_cs))
            {
                await conn.OpenAsync();

                var readSql = @"
SELECT p.Id,
       p.AlbumId,
       p.StoragePath,
       p.ThumbnailStoragePath,
       a.CoverPhotoId
FROM dbo.Photos p
JOIN dbo.Albums a ON a.Id = p.AlbumId
WHERE p.Id = @pid;";

                await using (var readCmd = new SqlCommand(readSql, conn))
                {
                    readCmd.Parameters.Add(new SqlParameter("@pid", SqlDbType.Int) { Value = photoId });
                    await using var r = await readCmd.ExecuteReaderAsync();
                    if (await r.ReadAsync())
                    {
                        albumId = r.GetInt32(r.GetOrdinal("AlbumId"));
                        coverPhotoId = r.IsDBNull(r.GetOrdinal("CoverPhotoId"))
                            ? (int?)null
                            : r.GetInt32(r.GetOrdinal("CoverPhotoId"));

                        p = new Photo
                        {
                            Id = r.GetInt32(r.GetOrdinal("Id")),
                            AlbumId = albumId.Value,
                            StoragePath = r.GetString(r.GetOrdinal("StoragePath")),
                            ThumbnailStoragePath = r.IsDBNull(r.GetOrdinal("ThumbnailStoragePath"))
                                ? null
                                : r.GetString(r.GetOrdinal("ThumbnailStoragePath"))
                        };
                    }
                }

                if (p == null) return;

                // 원본 삭제
                var physicalPath = _storage.MapPhysicalPath(p.StoragePath);
                if (File.Exists(physicalPath))
                {
                    try { _storage.Delete(p.StoragePath); }
                    catch (Exception ex) { await DbLogger.LogErrorAsync(ex.ToString()); }
                }

                // 썸네일 삭제
                if (!string.IsNullOrEmpty(p.ThumbnailStoragePath))
                {
                    var thumbPhysical = _storage.MapPhysicalPath(p.ThumbnailStoragePath);
                    if (File.Exists(thumbPhysical))
                    {
                        try { _storage.Delete(p.ThumbnailStoragePath); }
                        catch (Exception ex) { await DbLogger.LogErrorAsync(ex.ToString()); }
                    }
                }

                using var tran = conn.BeginTransaction();

                try
                {
                    var delSql = "DELETE FROM dbo.Photos WHERE Id=@pid;";
                    using (var delCmd = new SqlCommand(delSql, conn, tran))
                    {
                        delCmd.Parameters.Add(new SqlParameter("@pid", SqlDbType.Int) { Value = photoId });
                        await delCmd.ExecuteNonQueryAsync();
                    }

                    if (coverPhotoId == photoId)
                    {
                        var nextSql = @"
DECLARE @nextId INT = (
    SELECT TOP(1) Id
    FROM dbo.Photos
    WHERE AlbumId=@aid
    ORDER BY Id ASC
);
UPDATE dbo.Albums SET CoverPhotoId = @nextId WHERE Id=@aid;";
                        using var nextCmd = new SqlCommand(nextSql, conn, tran);
                        nextCmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
                        await nextCmd.ExecuteNonQueryAsync();
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    await DbLogger.LogErrorAsync(ex.ToString());
                    throw;
                }
            }
        }

        public async Task SetCoverAsync(int albumId, int photoId)
        {
            var sql = @"
IF EXISTS (SELECT 1 FROM dbo.Photos WHERE Id=@pid AND AlbumId=@aid)
    UPDATE dbo.Albums SET CoverPhotoId=@pid WHERE Id=@aid;
ELSE
    THROW 50000, 'Photo not in album', 1;";

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@pid", SqlDbType.Int) { Value = photoId });
            cmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<(Album album, IReadOnlyList<Photo> photos)> GetAlbumWithPhotosAsync(int id, int? take = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT Id, Name, UserName, Description, CreatedAt, CoverPhotoId FROM dbo.Albums WHERE Id=@id;");
            sb.Append("SELECT ");
            if (take.HasValue) sb.Append("TOP(@take) ");
            sb.AppendLine(@"
       Id,
       AlbumId,
       FileName,
       UserName,
       OriginalName,
       ContentType,
       FileSizeBytes,
       Width,
       Height,
       HashSha256,
       StoragePath,
       ThumbnailStoragePath,
       UploadedAt
FROM dbo.Photos
WHERE AlbumId=@id
ORDER BY Id DESC;");

            Album album = null;
            var photos = new List<Photo>();

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sb.ToString(), conn);
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = id });
            if (take.HasValue)
                cmd.Parameters.Add(new SqlParameter("@take", SqlDbType.Int) { Value = take.Value });

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                album = MapAlbum(reader);

            if (album != null && await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                    photos.Add(MapPhoto(reader));
            }

            return (album, photos);
        }

        public async Task<IReadOnlyList<AlbumSummary>> GetAlbumSummariesAsync()
        {
            // 커버가 지정 안 됐으면 가장 먼저 올린 사진을 커버로 보여 준다
            const string sql = @"
SELECT a.Id, a.Name, a.Description, a.UserName, a.CreatedAt,
       ISNULL(pc.Cnt, 0) AS PhotoCount, pc.LastAt,
       COALESCE(cp.ThumbnailStoragePath, cp.StoragePath, fp.ThumbnailStoragePath, fp.StoragePath) AS CoverUrl
FROM dbo.Albums a
LEFT JOIN dbo.Photos cp ON cp.Id = a.CoverPhotoId
OUTER APPLY (SELECT COUNT(*) AS Cnt, MAX(UploadedAt) AS LastAt FROM dbo.Photos p WHERE p.AlbumId = a.Id) pc
OUTER APPLY (SELECT TOP 1 ThumbnailStoragePath, StoragePath FROM dbo.Photos p WHERE p.AlbumId = a.Id ORDER BY p.Id) fp
ORDER BY a.CreatedAt DESC, a.Id DESC;";

            var list = new List<AlbumSummary>();
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new AlbumSummary
                {
                    Id = r.GetInt32(0),
                    Name = r.IsDBNull(1) ? "" : r.GetString(1),
                    Description = r.IsDBNull(2) ? null : r.GetString(2),
                    UserName = r.IsDBNull(3) ? null : r.GetString(3),
                    CreatedAt = r.GetDateTime(4),
                    PhotoCount = r.GetInt32(5),
                    LastUploadedAt = r.IsDBNull(6) ? null : r.GetDateTime(6),
                    CoverUrl = r.IsDBNull(7) ? null : r.GetString(7)
                });
            }
            return list;
        }

        public async Task<bool> UpdateAlbumAsync(int albumId, string name, string description)
        {
            const string sql = "UPDATE dbo.Albums SET Name = @name, Description = @desc WHERE Id = @id;";
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 200) { Value = name });
            cmd.Parameters.Add(new SqlParameter("@desc", SqlDbType.NVarChar, 1000) { Value = string.IsNullOrWhiteSpace(description) ? DBNull.Value : description });
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = albumId });
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        #endregion

        #region Helpers

        private static Album MapAlbum(SqlDataReader r)
        {
            return new Album
            {
                Id = r.GetInt32(r.GetOrdinal("Id")),
                Name = r.GetString(r.GetOrdinal("Name")),
                UserName = r.GetString(r.GetOrdinal("UserName")),
                Description = r.IsDBNull(r.GetOrdinal("Description"))
                    ? null
                    : r.GetString(r.GetOrdinal("Description")),
                CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                CoverPhotoId = r.IsDBNull(r.GetOrdinal("CoverPhotoId"))
                    ? (int?)null
                    : r.GetInt32(r.GetOrdinal("CoverPhotoId"))
            };
        }

        private static Photo MapPhoto(SqlDataReader r)
        {
            return new Photo
            {
                Id = r.GetInt32(r.GetOrdinal("Id")),
                AlbumId = r.GetInt32(r.GetOrdinal("AlbumId")),
                FileName = r.GetString(r.GetOrdinal("FileName")),
                UserName = r.GetString(r.GetOrdinal("UserName")),
                OriginalName = r.GetString(r.GetOrdinal("OriginalName")),
                ContentType = r.GetString(r.GetOrdinal("ContentType")),
                FileSizeBytes = r.GetInt64(r.GetOrdinal("FileSizeBytes")),
                Width = r.IsDBNull(r.GetOrdinal("Width")) ? (int?)null : r.GetInt32(r.GetOrdinal("Width")),
                Height = r.IsDBNull(r.GetOrdinal("Height")) ? (int?)null : r.GetInt32(r.GetOrdinal("Height")),
                HashSha256 = r.IsDBNull(r.GetOrdinal("HashSha256")) ? null : (byte[])r["HashSha256"],
                StoragePath = r.GetString(r.GetOrdinal("StoragePath")),
                ThumbnailStoragePath = r.IsDBNull(r.GetOrdinal("ThumbnailStoragePath"))
                    ? null
                    : r.GetString(r.GetOrdinal("ThumbnailStoragePath")),
                UploadedAt = r.GetDateTime(r.GetOrdinal("UploadedAt"))
            };
        }

        /// <summary>
        /// GetAlbumsAsync 에서 커버 사진 join 결과를 매핑 (P_ 접두어 컬럼)
        /// </summary>
        private static Photo MapPhotoPrefixed(SqlDataReader r)
        {
            // 호출 전에 P_Id null 체크는 이미 함
            return new Photo
            {
                Id = r.GetInt32(r.GetOrdinal("P_Id")),
                AlbumId = r.GetInt32(r.GetOrdinal("P_AlbumId")),
                FileName = r.GetString(r.GetOrdinal("P_FileName")),
                UserName = r.GetString(r.GetOrdinal("P_UserName")),
                OriginalName = r.GetString(r.GetOrdinal("P_OriginalName")),
                ContentType = r.GetString(r.GetOrdinal("P_ContentType")),
                FileSizeBytes = r.GetInt64(r.GetOrdinal("P_FileSizeBytes")),
                Width = r.IsDBNull(r.GetOrdinal("P_Width")) ? (int?)null : r.GetInt32(r.GetOrdinal("P_Width")),
                Height = r.IsDBNull(r.GetOrdinal("P_Height")) ? (int?)null : r.GetInt32(r.GetOrdinal("P_Height")),
                HashSha256 = r.IsDBNull(r.GetOrdinal("P_HashSha256")) ? null : (byte[])r["P_HashSha256"],
                StoragePath = r.GetString(r.GetOrdinal("P_StoragePath")),
                ThumbnailStoragePath = r.IsDBNull(r.GetOrdinal("P_ThumbnailStoragePath"))
                    ? null
                    : r.GetString(r.GetOrdinal("P_ThumbnailStoragePath")),
                UploadedAt = r.GetDateTime(r.GetOrdinal("P_UploadedAt"))
            };
        }

        private async Task<bool> AlbumExistsAsync(int albumId)
        {
            const string sql = "SELECT 1 FROM dbo.Albums WHERE Id=@id;";
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = albumId });

            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            var obj = await cmd.ExecuteScalarAsync();
            return obj != null;
        }

        private static async Task<bool> PhotoHashExistsAsync(SqlConnection conn, SqlTransaction tran, int albumId, byte[] hash)
        {
            const string sql = @"SELECT 1 FROM dbo.Photos WHERE AlbumId=@aid AND HashSha256=@h;";
            await using var cmd = new SqlCommand(sql, conn, tran);
            cmd.Parameters.Add(new SqlParameter("@aid", SqlDbType.Int) { Value = albumId });
            cmd.Parameters.Add(new SqlParameter("@h", SqlDbType.VarBinary, 32) { Value = hash });

            await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

            var obj = await cmd.ExecuteScalarAsync();
            return obj != null;
        }

        #endregion
    }
}

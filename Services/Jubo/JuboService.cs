using BulletinBoard.Models.Jubo;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Services.Jubo
{
    public class JuboService : IJuboService
    {
        private readonly string _connectionString;

        public JuboService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // ── helpers ────────────────────────────────────────────────────────────
        private static T? ReadNullable<T>(SqlDataReader r, string col) where T : struct
        {
            var v = r[col];
            return v is DBNull ? null : (T?)Convert.ChangeType(v, typeof(T));
        }
        private static string? ReadNullableString(SqlDataReader r, string col)
        {
            var v = r[col];
            return v is DBNull ? null : v.ToString();
        }

        public async Task EnsureTablesAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SundaySchoolJubo')
BEGIN
    CREATE TABLE [dbo].[SundaySchoolJubo] (
        [Id]             INT IDENTITY(1,1) PRIMARY KEY,
        [JuboDate]       DATE          NOT NULL,
        [Season]         NVARCHAR(100) NULL,
        [Title]          NVARCHAR(300) NOT NULL,
        [TemplateType]   NVARCHAR(50)  NOT NULL DEFAULT 'Normal',
        [IsPublished]    BIT           NOT NULL DEFAULT 0,
        [CreatedBy]      NVARCHAR(200) NULL,
        [CreatedAt]      DATETIME      NOT NULL DEFAULT GETDATE(),
        [LastModifiedBy] NVARCHAR(200) NULL,
        [LastModifiedAt] DATETIME      NULL
    );
    CREATE INDEX [IX_SundaySchoolJubo_JuboDate]
        ON [dbo].[SundaySchoolJubo] ([JuboDate] DESC);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SundaySchoolJuboBlock')
BEGIN
    CREATE TABLE [dbo].[SundaySchoolJuboBlock] (
        [Id]             INT IDENTITY(1,1) PRIMARY KEY,
        [JuboId]         INT           NOT NULL,
        [BlockType]      NVARCHAR(100) NOT NULL,
        [BlockTitle]     NVARCHAR(300) NULL,
        [BlockContent]   NVARCHAR(MAX) NULL,
        [SortOrder]      INT           NOT NULL DEFAULT 0,
        [IsVisible]      BIT           NOT NULL DEFAULT 1,
        [LastModifiedBy] NVARCHAR(200) NULL,
        [LastModifiedAt] DATETIME      NULL,
        CONSTRAINT [FK_JuboBlock_Jubo]
            FOREIGN KEY ([JuboId]) REFERENCES [SundaySchoolJubo]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_SundaySchoolJuboBlock_JuboId]
        ON [dbo].[SundaySchoolJuboBlock] ([JuboId]);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SundaySchoolJuboSnapshot')
BEGIN
    CREATE TABLE [dbo].[SundaySchoolJuboSnapshot] (
        [Id]           INT IDENTITY(1,1) PRIMARY KEY,
        [JuboId]       INT           NOT NULL,
        [SnapshotJson] NVARCHAR(MAX) NULL,
        [CreatedAt]    DATETIME      NOT NULL DEFAULT GETDATE(),
        [CreatedBy]    NVARCHAR(200) NULL
    );
    CREATE INDEX [IX_SundaySchoolJuboSnapshot_JuboId]
        ON [dbo].[SundaySchoolJuboSnapshot] ([JuboId]);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AttendanceCheck_2026')
BEGIN
    CREATE TABLE [dbo].[AttendanceCheck_2026] (
        [Id]            INT IDENTITY(1,1) PRIMARY KEY,
        [JuboDate]      DATE          NOT NULL,
        [ClassName]     NVARCHAR(100) NOT NULL,
        [TotalCount]    INT           NOT NULL DEFAULT 0,
        [PresentCount]  INT           NOT NULL DEFAULT 0,
        [NewComerCount] INT           NOT NULL DEFAULT 0,
        [Note]          NVARCHAR(500) NULL,
        [RecordedBy]    NVARCHAR(200) NULL,
        [RecordedAt]    DATETIME      NOT NULL DEFAULT GETDATE(),
        [IsLocked]      BIT           NOT NULL DEFAULT 0
    );
    CREATE INDEX [IX_AttendanceCheck_2026_JuboDate]
        ON [dbo].[AttendanceCheck_2026] ([JuboDate]);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AttendanceNewComer')
BEGIN
    CREATE TABLE [dbo].[AttendanceNewComer] (
        [Id]             INT IDENTITY(1,1) PRIMARY KEY,
        [AttendanceDate] DATE          NOT NULL,
        [Name]           NVARCHAR(100) NOT NULL,
        [Age]            INT           NULL,
        [Grade]          NVARCHAR(50)  NULL,
        [ParentName]     NVARCHAR(100) NULL,
        [PhoneNumber]    NVARCHAR(50)  NULL,
        [Address]        NVARCHAR(300) NULL,
        [Note]           NVARCHAR(500) NULL,
        [RecordedBy]     NVARCHAR(200) NULL,
        [RecordedAt]     DATETIME      NOT NULL DEFAULT GETDATE()
    );
    CREATE INDEX [IX_AttendanceNewComer_Date]
        ON [dbo].[AttendanceNewComer] ([AttendanceDate]);
END";

            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 60;
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<JuboListViewModel> GetJuboListAsync(int page = 1, int pageSize = 12)
        {
            var result = new JuboListViewModel { CurrentPage = page };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using (var cmdStats = new SqlCommand(@"
                SELECT
                    COUNT(*)                                                     AS TotalCount,
                    SUM(CASE WHEN IsPublished = 1 THEN 1 ELSE 0 END)            AS PublishedCount,
                    SUM(CASE WHEN YEAR(JuboDate)  = YEAR(GETDATE())
                              AND MONTH(JuboDate) = MONTH(GETDATE()) THEN 1 ELSE 0 END) AS ThisMonthCount
                FROM [dbo].[SundaySchoolJubo]", conn))
            {
                using var r = await cmdStats.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    result.TotalCount     = r["TotalCount"]     is DBNull ? 0 : (int)r["TotalCount"];
                    result.PublishedCount = r["PublishedCount"] is DBNull ? 0 : (int)r["PublishedCount"];
                    result.ThisMonthCount = r["ThisMonthCount"] is DBNull ? 0 : (int)r["ThisMonthCount"];
                }
            }

            result.TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);
            if (result.TotalPages < 1) result.TotalPages = 1;

            using var cmd = new SqlCommand(@"
                SELECT j.Id, j.JuboDate, j.Season, j.Title, j.TemplateType, j.IsPublished,
                       j.LastModifiedBy, j.LastModifiedAt, j.CreatedAt, j.CreatedBy,
                       (SELECT COUNT(*) FROM SundaySchoolJuboBlock WHERE JuboId = j.Id) AS BlockCount
                FROM [dbo].[SundaySchoolJubo] j
                ORDER BY j.JuboDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", conn);

            cmd.Parameters.AddWithValue("@Offset",   (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Items.Add(new JuboListItemViewModel
                {
                    Id             = (int)reader["Id"],
                    JuboDate       = (DateTime)reader["JuboDate"],
                    Season         = ReadNullableString(reader, "Season"),
                    Title          = (string)reader["Title"],
                    TemplateType   = (string)reader["TemplateType"],
                    IsPublished    = (bool)reader["IsPublished"],
                    LastModifiedBy = ReadNullableString(reader, "LastModifiedBy"),
                    LastModifiedAt = ReadNullable<DateTime>(reader, "LastModifiedAt"),
                    CreatedAt      = (DateTime)reader["CreatedAt"],
                    CreatedBy      = ReadNullableString(reader, "CreatedBy"),
                    BlockCount     = reader["BlockCount"] is DBNull ? 0 : (int)reader["BlockCount"]
                });
            }

            return result;
        }

        public async Task<JuboEditViewModel?> GetJuboForEditAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            JuboEditViewModel? jubo = null;

            using (var cmd = new SqlCommand(@"
                SELECT Id, JuboDate, Season, Title, TemplateType, IsPublished,
                       CreatedBy, CreatedAt, LastModifiedBy, LastModifiedAt
                FROM [dbo].[SundaySchoolJubo]
                WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    jubo = new JuboEditViewModel
                    {
                        Id             = (int)reader["Id"],
                        JuboDate       = (DateTime)reader["JuboDate"],
                        Season         = ReadNullableString(reader, "Season"),
                        Title          = (string)reader["Title"],
                        TemplateType   = (string)reader["TemplateType"],
                        IsPublished    = (bool)reader["IsPublished"],
                        CreatedBy      = ReadNullableString(reader, "CreatedBy"),
                        CreatedAt      = (DateTime)reader["CreatedAt"],
                        LastModifiedBy = ReadNullableString(reader, "LastModifiedBy"),
                        LastModifiedAt = ReadNullable<DateTime>(reader, "LastModifiedAt")
                    };
                }
            }

            if (jubo == null) return null;

            using (var cmd = new SqlCommand(@"
                SELECT Id, JuboId, BlockType, BlockTitle, BlockContent,
                       SortOrder, IsVisible, LastModifiedBy, LastModifiedAt
                FROM [dbo].[SundaySchoolJuboBlock]
                WHERE JuboId = @JuboId
                ORDER BY SortOrder", conn))
            {
                cmd.Parameters.AddWithValue("@JuboId", id);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    jubo.Blocks.Add(new BlockEditViewModel
                    {
                        Id             = (int)reader["Id"],
                        JuboId         = (int)reader["JuboId"],
                        BlockType      = (string)reader["BlockType"],
                        BlockTitle     = ReadNullableString(reader, "BlockTitle"),
                        BlockContent   = ReadNullableString(reader, "BlockContent"),
                        SortOrder      = (int)reader["SortOrder"],
                        IsVisible      = (bool)reader["IsVisible"],
                        LastModifiedBy = ReadNullableString(reader, "LastModifiedBy"),
                        LastModifiedAt = ReadNullable<DateTime>(reader, "LastModifiedAt")
                    });
                }
            }

            EnsureAllBlockTypes(jubo);
            return jubo;
        }

        private static void EnsureAllBlockTypes(JuboEditViewModel jubo)
        {
            foreach (var blockType in JuboBlockTypes.AllTypes)
            {
                if (!jubo.Blocks.Any(b => b.BlockType == blockType))
                {
                    jubo.Blocks.Add(new BlockEditViewModel
                    {
                        Id           = 0,
                        JuboId       = jubo.Id,
                        BlockType    = blockType,
                        BlockTitle   = JuboBlockTypes.GetDefaultTitle(blockType),
                        BlockContent = string.Empty,
                        SortOrder    = JuboBlockTypes.GetDefaultSortOrder(blockType),
                        IsVisible    = JuboBlockTypes.RequiredTypes.Contains(blockType)
                    });
                }
            }
            jubo.Blocks = jubo.Blocks.OrderBy(b => b.SortOrder).ToList();
        }

        public async Task<JuboPreviewViewModel?> GetJuboForPreviewAsync(int id)
        {
            var editVm = await GetJuboForEditAsync(id);
            if (editVm == null) return null;

            var attendance = await GetAttendanceSummaryAsync(editVm.JuboDate.AddDays(-7));
            var newComers  = await GetNewComersAsync(editVm.JuboDate.AddDays(-7));

            return new JuboPreviewViewModel
            {
                Id           = editVm.Id,
                JuboDate     = editVm.JuboDate,
                Season       = editVm.Season,
                Title        = editVm.Title,
                TemplateType = editVm.TemplateType,
                IsPublished  = editVm.IsPublished,
                Blocks       = editVm.Blocks,
                Attendance   = attendance,
                NewComers    = newComers
            };
        }

        public async Task<int> CreateJuboAsync(JuboCreateViewModel model, string userName)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                int juboId;
                using (var cmd = new SqlCommand(@"
                    INSERT INTO [dbo].[SundaySchoolJubo]
                        (JuboDate, Season, Title, TemplateType, IsPublished,
                         CreatedBy, CreatedAt, LastModifiedBy, LastModifiedAt)
                    VALUES (@JuboDate, @Season, @Title, @TemplateType, 0,
                            @CreatedBy, GETDATE(), @CreatedBy, GETDATE());
                    SELECT SCOPE_IDENTITY();", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@JuboDate",     model.JuboDate.Date);
                    cmd.Parameters.AddWithValue("@Season",       (object?)model.Season ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Title",        model.Title);
                    cmd.Parameters.AddWithValue("@TemplateType", model.TemplateType);
                    cmd.Parameters.AddWithValue("@CreatedBy",    userName);
                    juboId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                foreach (var blockType in JuboBlockTypes.AllTypes)
                {
                    bool isVisible = JuboBlockTypes.RequiredTypes.Contains(blockType);
                    using var cmdBlock = new SqlCommand(@"
                        INSERT INTO [dbo].[SundaySchoolJuboBlock]
                            (JuboId, BlockType, BlockTitle, BlockContent,
                             SortOrder, IsVisible, LastModifiedBy, LastModifiedAt)
                        VALUES (@JuboId, @BlockType, @BlockTitle, '',
                                @SortOrder, @IsVisible, @ModifiedBy, GETDATE())",
                        conn, tx);
                    cmdBlock.Parameters.AddWithValue("@JuboId",     juboId);
                    cmdBlock.Parameters.AddWithValue("@BlockType",  blockType);
                    cmdBlock.Parameters.AddWithValue("@BlockTitle", JuboBlockTypes.GetDefaultTitle(blockType));
                    cmdBlock.Parameters.AddWithValue("@SortOrder",  JuboBlockTypes.GetDefaultSortOrder(blockType));
                    cmdBlock.Parameters.AddWithValue("@IsVisible",  isVisible);
                    cmdBlock.Parameters.AddWithValue("@ModifiedBy", userName);
                    await cmdBlock.ExecuteNonQueryAsync();
                }

                tx.Commit();
                return juboId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateJuboMetaAsync(int id, string title, string? season, string templateType, string userName)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE [dbo].[SundaySchoolJubo]
                SET Title          = @Title,
                    Season         = @Season,
                    TemplateType   = @TemplateType,
                    LastModifiedBy = @ModifiedBy,
                    LastModifiedAt = GETDATE()
                WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id",           id);
            cmd.Parameters.AddWithValue("@Title",        title);
            cmd.Parameters.AddWithValue("@Season",       (object?)season ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TemplateType", templateType);
            cmd.Parameters.AddWithValue("@ModifiedBy",   userName);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteJuboAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("DELETE FROM [dbo].[SundaySchoolJubo] WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> TogglePublishAsync(int id, string userName)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE [dbo].[SundaySchoolJubo]
                SET IsPublished    = CASE WHEN IsPublished = 1 THEN 0 ELSE 1 END,
                    LastModifiedBy = @ModifiedBy,
                    LastModifiedAt = GETDATE()
                WHERE Id = @Id;
                SELECT IsPublished FROM [dbo].[SundaySchoolJubo] WHERE Id = @Id;", conn);
            cmd.Parameters.AddWithValue("@Id",         id);
            cmd.Parameters.AddWithValue("@ModifiedBy", userName);

            return await cmd.ExecuteScalarAsync() != null;
        }

        public async Task<bool> SaveBlocksAsync(int juboId, List<BlockEditViewModel> blocks, string userName)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var block in blocks)
                    await UpsertBlock(block, userName, conn, tx);

                using var cmdJubo = new SqlCommand(@"
                    UPDATE [dbo].[SundaySchoolJubo]
                    SET LastModifiedBy = @ModifiedBy, LastModifiedAt = GETDATE()
                    WHERE Id = @Id", conn, tx);
                cmdJubo.Parameters.AddWithValue("@Id",         juboId);
                cmdJubo.Parameters.AddWithValue("@ModifiedBy", userName);
                await cmdJubo.ExecuteNonQueryAsync();

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> SaveSingleBlockAsync(BlockEditViewModel block, string userName)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            try
            {
                await UpsertBlock(block, userName, conn, tx);

                using var cmdJubo = new SqlCommand(@"
                    UPDATE [dbo].[SundaySchoolJubo]
                    SET LastModifiedBy = @ModifiedBy, LastModifiedAt = GETDATE()
                    WHERE Id = @Id", conn, tx);
                cmdJubo.Parameters.AddWithValue("@Id",         block.JuboId);
                cmdJubo.Parameters.AddWithValue("@ModifiedBy", userName);
                await cmdJubo.ExecuteNonQueryAsync();

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private static async Task UpsertBlock(
            BlockEditViewModel block, string userName,
            SqlConnection conn, SqlTransaction tx)
        {
            if (block.Id == 0)
            {
                using var cmd = new SqlCommand(@"
                    INSERT INTO [dbo].[SundaySchoolJuboBlock]
                        (JuboId, BlockType, BlockTitle, BlockContent,
                         SortOrder, IsVisible, LastModifiedBy, LastModifiedAt)
                    VALUES (@JuboId, @BlockType, @BlockTitle, @BlockContent,
                            @SortOrder, @IsVisible, @ModifiedBy, GETDATE())",
                    conn, tx);
                cmd.Parameters.AddWithValue("@JuboId",       block.JuboId);
                cmd.Parameters.AddWithValue("@BlockType",    block.BlockType);
                cmd.Parameters.AddWithValue("@BlockTitle",   (object?)block.BlockTitle   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BlockContent", (object?)block.BlockContent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SortOrder",    block.SortOrder);
                cmd.Parameters.AddWithValue("@IsVisible",    block.IsVisible);
                cmd.Parameters.AddWithValue("@ModifiedBy",   userName);
                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                using var cmd = new SqlCommand(@"
                    UPDATE [dbo].[SundaySchoolJuboBlock]
                    SET BlockTitle     = @BlockTitle,
                        BlockContent   = @BlockContent,
                        SortOrder      = @SortOrder,
                        IsVisible      = @IsVisible,
                        LastModifiedBy = @ModifiedBy,
                        LastModifiedAt = GETDATE()
                    WHERE Id = @Id AND JuboId = @JuboId",
                    conn, tx);
                cmd.Parameters.AddWithValue("@Id",           block.Id);
                cmd.Parameters.AddWithValue("@JuboId",       block.JuboId);
                cmd.Parameters.AddWithValue("@BlockTitle",   (object?)block.BlockTitle   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BlockContent", (object?)block.BlockContent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SortOrder",    block.SortOrder);
                cmd.Parameters.AddWithValue("@IsVisible",    block.IsVisible);
                cmd.Parameters.AddWithValue("@ModifiedBy",   userName);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> CreateSnapshotAsync(int juboId, string userName)
        {
            var preview = await GetJuboForPreviewAsync(juboId);
            if (preview == null) return 0;

            var json = JsonConvert.SerializeObject(preview, Formatting.Indented);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO [dbo].[SundaySchoolJuboSnapshot]
                    (JuboId, SnapshotJson, CreatedAt, CreatedBy)
                VALUES (@JuboId, @SnapshotJson, GETDATE(), @CreatedBy);
                SELECT SCOPE_IDENTITY();", conn);
            cmd.Parameters.AddWithValue("@JuboId",       juboId);
            cmd.Parameters.AddWithValue("@SnapshotJson", json);
            cmd.Parameters.AddWithValue("@CreatedBy",    userName);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<SundaySchoolJuboSnapshot?> GetLatestSnapshotAsync(int juboId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT TOP 1 Id, JuboId, SnapshotJson, CreatedAt, CreatedBy
                FROM [dbo].[SundaySchoolJuboSnapshot]
                WHERE JuboId = @JuboId
                ORDER BY CreatedAt DESC", conn);
            cmd.Parameters.AddWithValue("@JuboId", juboId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new SundaySchoolJuboSnapshot
                {
                    Id           = (int)reader["Id"],
                    JuboId       = (int)reader["JuboId"],
                    SnapshotJson = ReadNullableString(reader, "SnapshotJson"),
                    CreatedAt    = (DateTime)reader["CreatedAt"],
                    CreatedBy    = ReadNullableString(reader, "CreatedBy")
                };
            }
            return null;
        }

        public async Task<AttendanceSummaryViewModel> GetAttendanceSummaryAsync(DateTime date)
        {
            var result = new AttendanceSummaryViewModel { JuboDate = date };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // 올해 누적 주일 수 (출석 기록이 있는 날짜 기준)
            using (var cmdW = new SqlCommand(@"
                SELECT COUNT(DISTINCT AttendDate)
                FROM [dbo].[AttendanceCheck_2026]
                WHERE YEAR(AttendDate) = YEAR(@Date) AND AttendDate <= @Date", conn))
            {
                cmdW.Parameters.AddWithValue("@Date", date.Date);
                var wv = await cmdW.ExecuteScalarAsync();
                result.WeeksCount = (wv == null || wv is DBNull) ? 1 : Math.Max(1, Convert.ToInt32(wv));
            }

            // 학생별 출석 현황 (ClassInfo_2026 + PersonMaster_2026 + AttendanceCheck_2026)
            const string sql = @"
SELECT
    ci.ClassYear,
    ci.ClassBan,
    COALESCE(NULLIF(LTRIM(RTRIM(pm.fullName)), N''), N'친구') AS StudentName,
    CASE WHEN acToday.PersonId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsPresent,
    ISNULL((
        SELECT COUNT(*)
        FROM [dbo].[AttendanceCheck_2026] acY
        WHERE acY.PersonId = ci.PersonId
          AND YEAR(acY.AttendDate) = YEAR(@Date)
          AND acY.AttendDate <= @Date
          AND CAST(ISNULL(acY.IsPresent, 0) AS BIT) = 1
    ), 0) AS YearPresentCount,
    CASE WHEN nc.Id IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsNewComer,
    ISNULL(ncv.VisitCount, 0) AS NewComerVisitCount
FROM [dbo].[ClassInfo_2026] ci
INNER JOIN [dbo].[PersonMaster_2026] pm ON pm.PersonId = ci.PersonId
LEFT JOIN [dbo].[AttendanceCheck_2026] acToday
    ON acToday.PersonId = ci.PersonId
   AND acToday.AttendDate = @Date
   AND CAST(ISNULL(acToday.IsPresent, 0) AS BIT) = 1
LEFT JOIN [dbo].[AttendanceNewComer] nc
    ON nc.PersonId = ci.PersonId AND nc.IsDeleted = 0
LEFT JOIN (
    SELECT NewComerId, COUNT(*) AS VisitCount
    FROM [dbo].[AttendanceNewComerVisit]
    GROUP BY NewComerId
) ncv ON ncv.NewComerId = nc.Id
WHERE ci.PersonId IS NOT NULL AND (ci.ts IS NULL OR ci.ts <> 'd')
ORDER BY ci.ClassYear, ci.ClassBan, pm.fullName";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Date", date.Date);
            cmd.CommandTimeout = 30;

            var classMap = new Dictionary<string, ClassAttendanceViewModel>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var classYear = Convert.ToInt32(reader["ClassYear"]);
                var classBan  = Convert.ToInt32(reader["ClassBan"]);
                var key       = $"{classYear}_{classBan}";

                if (!classMap.TryGetValue(key, out var cls))
                {
                    cls = new ClassAttendanceViewModel
                    {
                        ClassYear = classYear,
                        ClassBan  = classBan,
                        ClassName = $"{classYear}학년 {classBan}반"
                    };
                    classMap[key] = cls;
                    result.Classes.Add(cls);
                }

                var isPresent  = reader["IsPresent"]  is not DBNull && (bool)reader["IsPresent"];
                var isNewComer = reader["IsNewComer"] is not DBNull && (bool)reader["IsNewComer"];

                cls.Students.Add(new StudentJuboViewModel
                {
                    StudentName        = reader["StudentName"]?.ToString() ?? "친구",
                    IsPresent          = isPresent,
                    YearPresentCount   = reader["YearPresentCount"] is DBNull ? 0 : Convert.ToInt32(reader["YearPresentCount"]),
                    IsNewComer         = isNewComer,
                    NewComerVisitCount = reader["NewComerVisitCount"] is DBNull ? 0 : Convert.ToInt32(reader["NewComerVisitCount"])
                });

                if (isPresent) cls.PresentCount++;
                cls.TotalCount++;
            }

            // 반별 새친구 수 집계
            foreach (var cls in result.Classes)
                cls.NewComerCount = cls.Students.Count(s => s.IsNewComer);

            return result;
        }

        public async Task<bool> SaveAttendanceClassAsync(ClassAttendanceSaveItem item, DateTime date, string userName)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            if (item.Id == 0)
            {
                using var cmd = new SqlCommand(@"
                    INSERT INTO [dbo].[AttendanceCheck_2026]
                        (JuboDate, ClassName, TotalCount, PresentCount, NewComerCount,
                         Note, RecordedBy, RecordedAt, IsLocked)
                    VALUES (@Date, @ClassName, @TotalCount, @PresentCount, @NewComerCount,
                            @Note, @RecordedBy, GETDATE(), 0)", conn);
                cmd.Parameters.AddWithValue("@Date",          date.Date);
                cmd.Parameters.AddWithValue("@ClassName",     item.ClassName);
                cmd.Parameters.AddWithValue("@TotalCount",    item.TotalCount);
                cmd.Parameters.AddWithValue("@PresentCount",  item.PresentCount);
                cmd.Parameters.AddWithValue("@NewComerCount", item.NewComerCount);
                cmd.Parameters.AddWithValue("@Note",          (object?)item.Note ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RecordedBy",    userName);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            else
            {
                // Check locked
                using (var checkCmd = new SqlCommand(
                    "SELECT IsLocked FROM [dbo].[AttendanceCheck_2026] WHERE Id = @Id", conn))
                {
                    checkCmd.Parameters.AddWithValue("@Id", item.Id);
                    var v = await checkCmd.ExecuteScalarAsync();
                    if (v != null && v is not DBNull && (bool)v) return false;
                }

                using var cmd = new SqlCommand(@"
                    UPDATE [dbo].[AttendanceCheck_2026]
                    SET ClassName     = @ClassName,
                        TotalCount    = @TotalCount,
                        PresentCount  = @PresentCount,
                        NewComerCount = @NewComerCount,
                        Note          = @Note,
                        RecordedBy    = @RecordedBy,
                        RecordedAt    = GETDATE()
                    WHERE Id = @Id AND IsLocked = 0", conn);
                cmd.Parameters.AddWithValue("@Id",            item.Id);
                cmd.Parameters.AddWithValue("@ClassName",     item.ClassName);
                cmd.Parameters.AddWithValue("@TotalCount",    item.TotalCount);
                cmd.Parameters.AddWithValue("@PresentCount",  item.PresentCount);
                cmd.Parameters.AddWithValue("@NewComerCount", item.NewComerCount);
                cmd.Parameters.AddWithValue("@Note",          (object?)item.Note ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RecordedBy",    userName);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> DeleteAttendanceClassAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(
                "DELETE FROM [dbo].[AttendanceCheck_2026] WHERE Id = @Id AND IsLocked = 0", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<AttendanceNewComerModel>> GetNewComersAsync(DateTime date)
        {
            var result = new List<AttendanceNewComerModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id, AttendanceDate, Name, Age, Grade,
                       ParentName, PhoneNumber, Address, Note, RecordedBy, RecordedAt
                FROM [dbo].[view_AttendanceNewComer]
                WHERE AttendanceDate = @Date
                ORDER BY RecordedAt", conn);
            cmd.Parameters.AddWithValue("@Date", date.Date);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new AttendanceNewComerModel
                {
                    Id             = (int)reader["Id"],
                    AttendanceDate = (DateTime)reader["AttendanceDate"],
                    Name           = (string)reader["Name"],
                    Age            = ReadNullable<int>(reader, "Age"),
                    Grade          = ReadNullableString(reader, "Grade"),
                    ParentName     = ReadNullableString(reader, "ParentName"),
                    PhoneNumber    = ReadNullableString(reader, "PhoneNumber"),
                    Address        = ReadNullableString(reader, "Address"),
                    Note           = ReadNullableString(reader, "Note"),
                    RecordedBy     = ReadNullableString(reader, "RecordedBy"),
                    RecordedAt     = (DateTime)reader["RecordedAt"]
                });
            }

            return result;
        }

        public async Task<int> SaveNewComerAsync(SaveNewComerViewModel model, string userName)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO [dbo].[AttendanceNewComer]
                    (AttendanceDate, Name, Age, Grade, ParentName,
                     PhoneNumber, Address, Note, RecordedBy, RecordedAt)
                VALUES (@Date, @Name, @Age, @Grade, @ParentName,
                        @PhoneNumber, @Address, @Note, @RecordedBy, GETDATE());
                SELECT SCOPE_IDENTITY();", conn);
            cmd.Parameters.AddWithValue("@Date",        model.AttendanceDate.Date);
            cmd.Parameters.AddWithValue("@Name",        model.Name);
            cmd.Parameters.AddWithValue("@Age",         (object?)model.Age         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Grade",       (object?)model.Grade       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ParentName",  (object?)model.ParentName  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)model.PhoneNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address",     (object?)model.Address     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Note",        (object?)model.Note        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RecordedBy",  userName);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> DeleteNewComerAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(
                "DELETE FROM [dbo].[AttendanceNewComer] WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}

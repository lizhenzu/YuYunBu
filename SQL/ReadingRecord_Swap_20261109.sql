/* ==========================================================================
   ReadingRecord_Swap_20261109.sql   (step 3B-2, run after ReadingRecord_20261109.sql)
   From now on the reading records live only in the unified tables.
   The old names stay as "pass-through" views so every existing screen and query keeps working:

     dbo.UserBibleProgress (UserId, VerseId, ReadDate) = verses of the OPEN round of DM2_ReadingVerse
     dbo.UserBookMark      (UserId, VerseId, InsertDT) = DM2_ReadingBookmark
   INSERT / UPDATE / DELETE on these views are turned into writes on the unified tables (INSTEAD OF triggers).
   Old tables are kept as UserBibleProgress_bak_20261109b / UserBookMark_bak_20261109b.

   Run order: ReadingRecord_20261109.sql -> this script -> deploy the new code (round completion) .
   Rollback : ReadingRecord_Swap_20261109_rollback.sql
   Plain ASCII.
   ========================================================================== */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.usp_DM2_SyncReadingFromOld') IS NULL
BEGIN
    RAISERROR(N'Run SQL/ReadingRecord_20261109.sql first. Nothing was changed.', 16, 1);
    SET NOEXEC ON;
END
GO

/* 1) last copy from the old tables, then keep them under a backup name */
EXEC dbo.usp_DM2_SyncReadingFromOld;
GO
IF OBJECTPROPERTY(OBJECT_ID(N'dbo.UserBibleProgress'), 'IsUserTable') = 1 AND OBJECT_ID(N'dbo.UserBibleProgress_bak_20261109b') IS NULL
    EXEC sp_rename N'dbo.UserBibleProgress', N'UserBibleProgress_bak_20261109b';
GO
IF OBJECTPROPERTY(OBJECT_ID(N'dbo.UserBookMark'), 'IsUserTable') = 1 AND OBJECT_ID(N'dbo.UserBookMark_bak_20261109b') IS NULL
    EXEC sp_rename N'dbo.UserBookMark', N'UserBookMark_bak_20261109b';
GO

/* 2) pass-through view: verses read in the open round */
IF OBJECT_ID(N'dbo.UserBibleProgress') IS NULL
    EXEC (N'CREATE VIEW dbo.UserBibleProgress AS SELECT CAST(NULL AS NVARCHAR(900)) AS UserId, 0 AS VerseId, GETDATE() AS ReadDate WHERE 1 = 0');
GO
ALTER VIEW dbo.UserBibleProgress
AS
SELECT CAST(u.Id AS NVARCHAR(900)) AS UserId,
       k.VerseId,
       CAST(v.ReadAt AS DATETIME) AS ReadDate
FROM dbo.DM2_ReadingVerse v
JOIN dbo.DM2_ReadingRound r ON r.UserName = v.UserName AND r.RoundNo = v.RoundNo AND r.CompletedAt IS NULL AND r.ClosedAt IS NULL
JOIN [Identity].[User] u ON u.UserName = v.UserName
JOIN dbo.DM2_VerseKey k ON k.BookId = v.BookId AND k.Chapter = v.Chapter AND k.Verse = v.Verse;
GO

CREATE TRIGGER dbo.TR_UserBibleProgress_Insert ON dbo.UserBibleProgress
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    -- open a round for anyone who has none
    INSERT INTO dbo.DM2_ReadingRound (UserName, RoundNo, StartedAt)
    SELECT u.UserName, dbo.ufn_DM2_NextRoundNo(u.UserName), MIN(CAST(ISNULL(i.ReadDate, GETDATE()) AS DATETIME2(3)))
    FROM inserted i
    JOIN [Identity].[User] u ON u.Id = i.UserId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.DM2_ReadingRound r WHERE r.UserName = u.UserName AND r.CompletedAt IS NULL AND r.ClosedAt IS NULL)
    GROUP BY u.UserName;

    INSERT INTO dbo.DM2_ReadingVerse (UserName, RoundNo, BookId, Chapter, Verse, ReadAt)
    SELECT x.UserName, x.RoundNo, x.BookId, x.Chapter, x.Verse, MIN(x.ReadAt)
    FROM (
        SELECT u.UserName, r.RoundNo, k.BookId, k.Chapter, k.Verse, CAST(ISNULL(i.ReadDate, GETDATE()) AS DATETIME2(3)) AS ReadAt
        FROM inserted i
        JOIN [Identity].[User] u ON u.Id = i.UserId
        JOIN dbo.DM2_VerseKey k ON k.VerseId = i.VerseId
        JOIN dbo.DM2_ReadingRound r ON r.UserName = u.UserName AND r.CompletedAt IS NULL AND r.ClosedAt IS NULL
    ) x
    WHERE NOT EXISTS (SELECT 1 FROM dbo.DM2_ReadingVerse v WHERE v.UserName = x.UserName AND v.RoundNo = x.RoundNo AND v.BookId = x.BookId AND v.Chapter = x.Chapter AND v.Verse = x.Verse)
    GROUP BY x.UserName, x.RoundNo, x.BookId, x.Chapter, x.Verse;
END
GO

CREATE TRIGGER dbo.TR_UserBibleProgress_Update ON dbo.UserBibleProgress
INSTEAD OF UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE v SET v.ReadAt = CAST(i.ReadDate AS DATETIME2(3))
    FROM dbo.DM2_ReadingVerse v
    JOIN dbo.DM2_ReadingRound r ON r.UserName = v.UserName AND r.RoundNo = v.RoundNo AND r.CompletedAt IS NULL AND r.ClosedAt IS NULL
    JOIN [Identity].[User] u ON u.UserName = v.UserName
    JOIN dbo.DM2_VerseKey k ON k.BookId = v.BookId AND k.Chapter = v.Chapter AND k.Verse = v.Verse
    JOIN inserted i ON i.UserId = u.Id AND i.VerseId = k.VerseId;
END
GO

CREATE TRIGGER dbo.TR_UserBibleProgress_Delete ON dbo.UserBibleProgress
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DELETE v
    FROM dbo.DM2_ReadingVerse v
    JOIN dbo.DM2_ReadingRound r ON r.UserName = v.UserName AND r.RoundNo = v.RoundNo AND r.CompletedAt IS NULL AND r.ClosedAt IS NULL
    JOIN [Identity].[User] u ON u.UserName = v.UserName
    JOIN dbo.DM2_VerseKey k ON k.BookId = v.BookId AND k.Chapter = v.Chapter AND k.Verse = v.Verse
    JOIN deleted d ON d.UserId = u.Id AND d.VerseId = k.VerseId;
END
GO

/* 3) pass-through view: bookmarks */
IF OBJECT_ID(N'dbo.UserBookMark') IS NULL
    EXEC (N'CREATE VIEW dbo.UserBookMark AS SELECT CAST(NULL AS NVARCHAR(900)) AS UserId, 0 AS VerseId, GETDATE() AS InsertDT WHERE 1 = 0');
GO
ALTER VIEW dbo.UserBookMark
AS
SELECT CAST(u.Id AS NVARCHAR(900)) AS UserId,
       k.VerseId,
       CAST(m.CreatedAt AS DATETIME) AS InsertDT
FROM dbo.DM2_ReadingBookmark m
JOIN [Identity].[User] u ON u.UserName = m.UserName
JOIN dbo.DM2_VerseKey k ON k.BookId = m.BookId AND k.Chapter = m.Chapter AND k.Verse = m.Verse;
GO

CREATE TRIGGER dbo.TR_UserBookMark_Insert ON dbo.UserBookMark
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.DM2_ReadingBookmark (UserName, BookId, Chapter, Verse, CreatedAt)
    SELECT u.UserName, k.BookId, k.Chapter, k.Verse, MIN(CAST(ISNULL(i.InsertDT, GETDATE()) AS DATETIME2(3)))
    FROM inserted i
    JOIN [Identity].[User] u ON u.Id = i.UserId
    JOIN dbo.DM2_VerseKey k ON k.VerseId = i.VerseId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.DM2_ReadingBookmark m WHERE m.UserName = u.UserName AND m.BookId = k.BookId AND m.Chapter = k.Chapter AND m.Verse = k.Verse)
    GROUP BY u.UserName, k.BookId, k.Chapter, k.Verse;
END
GO

CREATE TRIGGER dbo.TR_UserBookMark_Delete ON dbo.UserBookMark
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DELETE m
    FROM dbo.DM2_ReadingBookmark m
    JOIN [Identity].[User] u ON u.UserName = m.UserName
    JOIN dbo.DM2_VerseKey k ON k.BookId = m.BookId AND k.Chapter = m.Chapter AND k.Verse = m.Verse
    JOIN deleted d ON d.UserId = u.Id AND d.VerseId = k.VerseId;
END
GO

/* 4) refresh views that read the old names */
DECLARE @v NVARCHAR(300);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR
    SELECT QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(o.name)
    FROM sys.views o JOIN sys.sql_modules m ON m.object_id = o.object_id
    WHERE o.is_ms_shipped = 0 AND o.name NOT IN (N'UserBibleProgress', N'UserBookMark')
      AND (m.definition LIKE N'%UserBibleProgress%' OR m.definition LIKE N'%UserBookMark%');
OPEN c; FETCH NEXT FROM c INTO @v;
WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY EXEC sp_refreshview @v; END TRY BEGIN CATCH PRINT 'skip ' + @v + ' : ' + ERROR_MESSAGE(); END CATCH
    FETCH NEXT FROM c INTO @v;
END
CLOSE c; DEALLOCATE c;
GO

SET NOEXEC OFF;
GO
SELECT (SELECT COUNT(*) FROM dbo.UserBibleProgress) AS ViewProgressRows,
       (SELECT COUNT(*) FROM dbo.UserBibleProgress_bak_20261109b) AS OldProgressRows,
       (SELECT COUNT(*) FROM dbo.UserBookMark) AS ViewBookmarkRows,
       (SELECT COUNT(*) FROM dbo.UserBookMark_bak_20261109b) AS OldBookmarkRows;
GO

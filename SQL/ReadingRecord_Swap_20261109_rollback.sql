/* ==========================================================================
   ReadingRecord_Swap_20261109_rollback.sql
   Puts the old tables back under their old names (undo of ReadingRecord_Swap_20261109.sql).
   What was read / bookmarked after the swap is copied back into the old tables first
   (open-round verses -> UserBibleProgress, bookmarks -> UserBookMark), so nothing is lost.
   The unified tables are kept (not dropped).
   Plain ASCII.
   ========================================================================== */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.UserBibleProgress_bak_20261109b') IS NULL OR OBJECTPROPERTY(OBJECT_ID(N'dbo.UserBibleProgress'), 'IsView') <> 1
BEGIN
    RAISERROR(N'Nothing to roll back (the swap was not done).', 16, 1);
    SET NOEXEC ON;
END
GO

/* 1) copy back: old progress table = open-round verses */
DELETE b FROM dbo.UserBibleProgress_bak_20261109b b
WHERE NOT EXISTS (SELECT 1 FROM dbo.UserBibleProgress v WHERE v.UserId = b.UserId AND v.VerseId = b.VerseId);
INSERT INTO dbo.UserBibleProgress_bak_20261109b (UserId, VerseId, ReadDate)
SELECT v.UserId, v.VerseId, v.ReadDate FROM dbo.UserBibleProgress v
WHERE NOT EXISTS (SELECT 1 FROM dbo.UserBibleProgress_bak_20261109b b WHERE b.UserId = v.UserId AND b.VerseId = v.VerseId);
GO
DELETE b FROM dbo.UserBookMark_bak_20261109b b
WHERE NOT EXISTS (SELECT 1 FROM dbo.UserBookMark v WHERE v.UserId = b.UserId AND v.VerseId = b.VerseId);
INSERT INTO dbo.UserBookMark_bak_20261109b (UserId, VerseId, InsertDT)
SELECT v.UserId, v.VerseId, v.InsertDT FROM dbo.UserBookMark v
WHERE NOT EXISTS (SELECT 1 FROM dbo.UserBookMark_bak_20261109b b WHERE b.UserId = v.UserId AND b.VerseId = v.VerseId);
GO

/* 2) drop the pass-through views (their triggers go with them) and rename the tables back */
DROP VIEW dbo.UserBibleProgress;
DROP VIEW dbo.UserBookMark;
GO
EXEC sp_rename N'dbo.UserBibleProgress_bak_20261109b', N'UserBibleProgress';
EXEC sp_rename N'dbo.UserBookMark_bak_20261109b', N'UserBookMark';
GO

/* 3) refresh views that read the old names */
DECLARE @v NVARCHAR(300);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR
    SELECT QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(o.name)
    FROM sys.views o JOIN sys.sql_modules m ON m.object_id = o.object_id
    WHERE o.is_ms_shipped = 0 AND (m.definition LIKE N'%UserBibleProgress%' OR m.definition LIKE N'%UserBookMark%');
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

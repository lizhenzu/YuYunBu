/* ==========================================================================
   ReadingRecord_20261109.sql   (step 3B-1 of docs/mission-reading unified table design)
   Unified Bible-reading record tables + copy of the old rows.

     dbo.DM2_ReadingRound    : rounds (1st reading, 2nd reading ...) - completed rounds + the one open round
     dbo.DM2_ReadingVerse    : verses read, per round, as book / chapter / verse
     dbo.DM2_ReadingBookmark : bookmarks, as book / chapter / verse
     dbo.DM2_VerseKey        : helper only (not sent to ChallengeBible) - old verse number (eBible.Id) <-> book / chapter / verse

   Old tables are NOT changed here:
     UserBibleProgress (UserId + VerseId)  -> DM2_ReadingVerse of the open round
     UserBookMark      (UserId + VerseId)  -> DM2_ReadingBookmark
     BibleCompletionLog                     -> completed rounds in DM2_ReadingRound
   The open round number = (highest of: completed round numbers, completion count in UserBibleProgressCompleted) + 1.
   Users that are not in [Identity].[User] are skipped.

   Safe to run again: dbo.usp_DM2_SyncReadingFromOld makes the open round / bookmarks equal to the old tables
   (only while the old tables are still real tables - after ReadingRecord_Swap_20261109.sql it does nothing).
   Plain ASCII.
   ========================================================================== */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

/* ---- helper: verse number <-> book / chapter / verse (book number = canonical order 1..66 from eBible) ---- */
IF OBJECT_ID(N'dbo.DM2_VerseKey') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_VerseKey (
        VerseId  INT      NOT NULL CONSTRAINT PK_DM2_VerseKey PRIMARY KEY,
        BookId   TINYINT  NOT NULL,
        Chapter  SMALLINT NOT NULL,
        Verse    SMALLINT NOT NULL
    );
    CREATE UNIQUE INDEX UX_DM2_VerseKey_BCV ON dbo.DM2_VerseKey (BookId, Chapter, Verse);
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.DM2_VerseKey)
BEGIN
    ;WITH B AS (SELECT BibleText, MIN(Id) AS FirstId FROM dbo.eBible GROUP BY BibleText),
    R AS (SELECT BibleText, DENSE_RANK() OVER (ORDER BY FirstId) AS BookId FROM B)
    INSERT INTO dbo.DM2_VerseKey (VerseId, BookId, Chapter, Verse)
    SELECT e.Id, R.BookId, e.Jang, e.zul
    FROM dbo.eBible e JOIN R ON R.BibleText = e.BibleText;
END
GO

IF OBJECT_ID(N'dbo.DM2_ReadingRound') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_ReadingRound (
        UserName    NVARCHAR(256) NOT NULL,
        RoundNo     INT           NOT NULL CONSTRAINT CK_DM2_ReadingRound_No CHECK (RoundNo >= 1),
        StartedAt   DATETIME2(3)  NOT NULL,
        CompletedAt DATETIME2(3)  NULL,      -- all verses read (finished)
        ClosedAt    DATETIME2(3)  NULL,      -- stopped without finishing
        CONSTRAINT PK_DM2_ReadingRound PRIMARY KEY (UserName, RoundNo),
        CONSTRAINT CK_DM2_ReadingRound_End CHECK (CompletedAt IS NULL OR ClosedAt IS NULL)
    );
    CREATE UNIQUE INDEX UX_DM2_ReadingRound_Open ON dbo.DM2_ReadingRound (UserName) WHERE CompletedAt IS NULL AND ClosedAt IS NULL;
END
GO

IF OBJECT_ID(N'dbo.DM2_ReadingVerse') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_ReadingVerse (
        UserName NVARCHAR(256) NOT NULL,
        RoundNo  INT           NOT NULL,
        BookId   TINYINT       NOT NULL,
        Chapter  SMALLINT      NOT NULL,
        Verse    SMALLINT      NOT NULL,
        ReadAt   DATETIME2(3)  NOT NULL,
        CONSTRAINT PK_DM2_ReadingVerse PRIMARY KEY (UserName, RoundNo, BookId, Chapter, Verse),
        CONSTRAINT FK_DM2_ReadingVerse_Round FOREIGN KEY (UserName, RoundNo) REFERENCES dbo.DM2_ReadingRound (UserName, RoundNo)
    );
    CREATE INDEX IX_DM2_ReadingVerse_ReadAt ON dbo.DM2_ReadingVerse (ReadAt) INCLUDE (UserName, RoundNo);
END
GO

IF OBJECT_ID(N'dbo.DM2_ReadingBookmark') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_ReadingBookmark (
        UserName  NVARCHAR(256) NOT NULL,
        BookId    TINYINT       NOT NULL,
        Chapter   SMALLINT      NOT NULL,
        Verse     SMALLINT      NOT NULL,
        CreatedAt DATETIME2(3)  NOT NULL,
        CONSTRAINT PK_DM2_ReadingBookmark PRIMARY KEY (UserName, BookId, Chapter, Verse)
    );
END
GO

/* ---- next round number for a user (used by the copy, the swap triggers and round completion) ---- */
IF OBJECT_ID(N'dbo.ufn_DM2_NextRoundNo') IS NOT NULL DROP FUNCTION dbo.ufn_DM2_NextRoundNo;
GO
CREATE FUNCTION dbo.ufn_DM2_NextRoundNo (@UserName NVARCHAR(256))
RETURNS INT
AS
BEGIN
    DECLARE @r INT = ISNULL((SELECT MAX(RoundNo) FROM dbo.DM2_ReadingRound WHERE UserName = @UserName), 0);
    DECLARE @c INT = 0;
    IF OBJECT_ID(N'dbo.UserBibleProgressCompleted') IS NOT NULL
        SET @c = ISNULL((SELECT MAX(readBibleAllCount) FROM dbo.UserBibleProgressCompleted WHERE UserName = @UserName), 0);
    RETURN (CASE WHEN @r > @c THEN @r ELSE @c END) + 1;
END
GO

/* ---- the open round of a user; makes one when there is none ---- */
IF OBJECT_ID(N'dbo.usp_DM2_EnsureOpenRound') IS NOT NULL DROP PROCEDURE dbo.usp_DM2_EnsureOpenRound;
GO
CREATE PROCEDURE dbo.usp_DM2_EnsureOpenRound
    @UserName NVARCHAR(256),
    @StartedAt DATETIME2(3),
    @RoundNo INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @RoundNo = RoundNo FROM dbo.DM2_ReadingRound WHERE UserName = @UserName AND CompletedAt IS NULL AND ClosedAt IS NULL;
    IF @RoundNo IS NULL
    BEGIN
        SET @RoundNo = dbo.ufn_DM2_NextRoundNo(@UserName);
        INSERT INTO dbo.DM2_ReadingRound (UserName, RoundNo, StartedAt) VALUES (@UserName, @RoundNo, @StartedAt);
    END
END
GO

/* ---- finish the open round ("start next reading" button) and open the next one ---- */
IF OBJECT_ID(N'dbo.usp_DM2_CompleteReadingRound') IS NOT NULL DROP PROCEDURE dbo.usp_DM2_CompleteReadingRound;
GO
CREATE PROCEDURE dbo.usp_DM2_CompleteReadingRound
    @UserName NVARCHAR(256),
    @CompletedAt DATETIME2(3),
    @Now DATETIME2(3)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @open INT = (SELECT RoundNo FROM dbo.DM2_ReadingRound WHERE UserName = @UserName AND CompletedAt IS NULL AND ClosedAt IS NULL);
    IF @open IS NOT NULL
        UPDATE dbo.DM2_ReadingRound SET CompletedAt = @CompletedAt WHERE UserName = @UserName AND RoundNo = @open;
    ELSE
    BEGIN
        SET @open = dbo.ufn_DM2_NextRoundNo(@UserName);
        INSERT INTO dbo.DM2_ReadingRound (UserName, RoundNo, StartedAt, CompletedAt) VALUES (@UserName, @open, @CompletedAt, @CompletedAt);
    END
    INSERT INTO dbo.DM2_ReadingRound (UserName, RoundNo, StartedAt) VALUES (@UserName, @open + 1, @Now);
END
GO

/* ---- copy / re-sync from the old tables (does nothing once the old names are views) ---- */
IF OBJECT_ID(N'dbo.usp_DM2_SyncReadingFromOld') IS NOT NULL DROP PROCEDURE dbo.usp_DM2_SyncReadingFromOld;
GO
CREATE PROCEDURE dbo.usp_DM2_SyncReadingFromOld
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECTPROPERTY(OBJECT_ID(N'dbo.UserBibleProgress'), 'IsUserTable') <> 1 RETURN;

    -- an open round that the old code has finished since the last sync ("start next reading" wrote a log row
    -- with that round number) becomes a completed round; it keeps the verses copied at the last sync
    UPDATE r SET r.CompletedAt = x.CompletedAt
    FROM dbo.DM2_ReadingRound r
    JOIN (SELECT u.UserName, l.RoundNo, MIN(CAST(l.CompletedAt AS DATETIME2(3))) AS CompletedAt
          FROM dbo.BibleCompletionLog l JOIN [Identity].[User] u ON u.Id = l.UserId
          GROUP BY u.UserName, l.RoundNo) x ON x.UserName = r.UserName AND x.RoundNo = r.RoundNo
    WHERE r.CompletedAt IS NULL AND r.ClosedAt IS NULL;

    -- completed rounds from the completion log
    INSERT INTO dbo.DM2_ReadingRound (UserName, RoundNo, StartedAt, CompletedAt)
    SELECT u.UserName, l.RoundNo, MIN(CAST(l.CompletedAt AS DATETIME2(3))), MIN(CAST(l.CompletedAt AS DATETIME2(3)))
    FROM dbo.BibleCompletionLog l
    JOIN [Identity].[User] u ON u.Id = l.UserId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.DM2_ReadingRound r WHERE r.UserName = u.UserName AND r.RoundNo = l.RoundNo)
    GROUP BY u.UserName, l.RoundNo;

    -- an open round for everyone who has progress rows
    INSERT INTO dbo.DM2_ReadingRound (UserName, RoundNo, StartedAt)
    SELECT u.UserName, dbo.ufn_DM2_NextRoundNo(u.UserName), MIN(CAST(p.ReadDate AS DATETIME2(3)))
    FROM dbo.UserBibleProgress p
    JOIN [Identity].[User] u ON u.Id = p.UserId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.DM2_ReadingRound r WHERE r.UserName = u.UserName AND r.CompletedAt IS NULL AND r.ClosedAt IS NULL)
    GROUP BY u.UserName;

    -- open-round verses = old progress rows exactly (remove what was un-marked, add what is new)
    ;WITH O AS (
        SELECT u.UserName, r.RoundNo, k.BookId, k.Chapter, k.Verse, CAST(p.ReadDate AS DATETIME2(3)) AS ReadAt
        FROM dbo.UserBibleProgress p
        JOIN [Identity].[User] u ON u.Id = p.UserId
        JOIN dbo.DM2_VerseKey k ON k.VerseId = p.VerseId
        JOIN dbo.DM2_ReadingRound r ON r.UserName = u.UserName AND r.CompletedAt IS NULL AND r.ClosedAt IS NULL
    )
    DELETE v
    FROM dbo.DM2_ReadingVerse v
    JOIN dbo.DM2_ReadingRound r ON r.UserName = v.UserName AND r.RoundNo = v.RoundNo AND r.CompletedAt IS NULL AND r.ClosedAt IS NULL
    WHERE NOT EXISTS (SELECT 1 FROM O WHERE O.UserName = v.UserName AND O.RoundNo = v.RoundNo AND O.BookId = v.BookId AND O.Chapter = v.Chapter AND O.Verse = v.Verse);

    ;WITH O AS (
        SELECT u.UserName, r.RoundNo, k.BookId, k.Chapter, k.Verse, CAST(p.ReadDate AS DATETIME2(3)) AS ReadAt
        FROM dbo.UserBibleProgress p
        JOIN [Identity].[User] u ON u.Id = p.UserId
        JOIN dbo.DM2_VerseKey k ON k.VerseId = p.VerseId
        JOIN dbo.DM2_ReadingRound r ON r.UserName = u.UserName AND r.CompletedAt IS NULL AND r.ClosedAt IS NULL
    )
    INSERT INTO dbo.DM2_ReadingVerse (UserName, RoundNo, BookId, Chapter, Verse, ReadAt)
    SELECT O.UserName, O.RoundNo, O.BookId, O.Chapter, O.Verse, O.ReadAt
    FROM O
    WHERE NOT EXISTS (SELECT 1 FROM dbo.DM2_ReadingVerse v WHERE v.UserName = O.UserName AND v.RoundNo = O.RoundNo AND v.BookId = O.BookId AND v.Chapter = O.Chapter AND v.Verse = O.Verse);

    -- bookmarks = old bookmarks exactly
    ;WITH O AS (
        SELECT u.UserName, k.BookId, k.Chapter, k.Verse, CAST(b.InsertDT AS DATETIME2(3)) AS CreatedAt
        FROM dbo.UserBookMark b
        JOIN [Identity].[User] u ON u.Id = b.UserId
        JOIN dbo.DM2_VerseKey k ON k.VerseId = b.VerseId
    )
    DELETE m FROM dbo.DM2_ReadingBookmark m
    WHERE NOT EXISTS (SELECT 1 FROM O WHERE O.UserName = m.UserName AND O.BookId = m.BookId AND O.Chapter = m.Chapter AND O.Verse = m.Verse);

    ;WITH O AS (
        SELECT u.UserName, k.BookId, k.Chapter, k.Verse, CAST(b.InsertDT AS DATETIME2(3)) AS CreatedAt
        FROM dbo.UserBookMark b
        JOIN [Identity].[User] u ON u.Id = b.UserId
        JOIN dbo.DM2_VerseKey k ON k.VerseId = b.VerseId
    )
    INSERT INTO dbo.DM2_ReadingBookmark (UserName, BookId, Chapter, Verse, CreatedAt)
    SELECT O.UserName, O.BookId, O.Chapter, O.Verse, O.CreatedAt
    FROM O
    WHERE NOT EXISTS (SELECT 1 FROM dbo.DM2_ReadingBookmark m WHERE m.UserName = O.UserName AND m.BookId = O.BookId AND m.Chapter = O.Chapter AND m.Verse = O.Verse);
END
GO

EXEC dbo.usp_DM2_SyncReadingFromOld;
GO

/* ---- report ---- */
SELECT (SELECT COUNT(*) FROM dbo.DM2_VerseKey) AS VerseKeys,
       (SELECT COUNT(*) FROM dbo.DM2_ReadingRound WHERE CompletedAt IS NOT NULL) AS CompletedRounds,
       (SELECT COUNT(*) FROM dbo.DM2_ReadingRound WHERE CompletedAt IS NULL AND ClosedAt IS NULL) AS OpenRounds,
       (SELECT COUNT(*) FROM dbo.DM2_ReadingVerse) AS Verses,
       (SELECT COUNT(*) FROM dbo.DM2_ReadingBookmark) AS Bookmarks;
GO

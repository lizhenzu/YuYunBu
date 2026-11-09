/* ==========================================================================
   ReadingPosition_20261109.sql   (resume reading - "e-eo ilgi")
   Two more reading tables, shared with ChallengeBible (same names / columns, see docs/challenge-bible/07 section 3-1):

     dbo.DM2_ReadingChapterProgress : the verse where the person stopped viewing a chapter (per round)
     dbo.DM2_ReadingChapterRead     : a chapter finished in a round (YunYunBu does not write it:
                                      "chapter done" is computed from the verses read; the table exists so both sites stay identical)

   Needs ReadingRecord_20261109.sql first (DM2_ReadingRound). Safe to run again. Plain ASCII.
   ========================================================================== */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.DM2_ReadingRound') IS NULL
BEGIN
    RAISERROR(N'Run SQL/ReadingRecord_20261109.sql first. Nothing was changed.', 16, 1);
    SET NOEXEC ON;
END
GO

IF OBJECT_ID(N'dbo.DM2_ReadingChapterProgress') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_ReadingChapterProgress (
        UserName        NVARCHAR(256) NOT NULL,
        RoundNo         INT           NOT NULL,
        BookId          TINYINT       NOT NULL,
        Chapter         SMALLINT      NOT NULL,
        LastViewedVerse SMALLINT      NOT NULL CONSTRAINT CK_DM2_ReadingChapterProgress_Verse CHECK (LastViewedVerse >= 1),
        UpdatedAt       DATETIME2(3)  NOT NULL,
        CONSTRAINT PK_DM2_ReadingChapterProgress PRIMARY KEY (UserName, RoundNo, BookId, Chapter),
        CONSTRAINT FK_DM2_ReadingChapterProgress_Round FOREIGN KEY (UserName, RoundNo) REFERENCES dbo.DM2_ReadingRound (UserName, RoundNo)
    );
    CREATE INDEX IX_DM2_ReadingChapterProgress_Recent ON dbo.DM2_ReadingChapterProgress (UserName, UpdatedAt DESC);
END
GO

IF OBJECT_ID(N'dbo.DM2_ReadingChapterRead') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_ReadingChapterRead (
        UserName NVARCHAR(256) NOT NULL,
        RoundNo  INT           NOT NULL,
        BookId   TINYINT       NOT NULL,
        Chapter  SMALLINT      NOT NULL CONSTRAINT CK_DM2_ReadingChapterRead_Chapter CHECK (Chapter >= 1),
        ReadAt   DATETIME2(3)  NOT NULL,
        CONSTRAINT PK_DM2_ReadingChapterRead PRIMARY KEY (UserName, RoundNo, BookId, Chapter),
        CONSTRAINT FK_DM2_ReadingChapterRead_Round FOREIGN KEY (UserName, RoundNo) REFERENCES dbo.DM2_ReadingRound (UserName, RoundNo)
    );
    CREATE INDEX IX_DM2_ReadingChapterRead_Day ON dbo.DM2_ReadingChapterRead (UserName, ReadAt);
END
GO

/* save (or move) the viewing position of one chapter in the person's open round */
IF OBJECT_ID(N'dbo.usp_DM2_SaveReadingPosition') IS NOT NULL DROP PROCEDURE dbo.usp_DM2_SaveReadingPosition;
GO
CREATE PROCEDURE dbo.usp_DM2_SaveReadingPosition
    @UserId   NVARCHAR(450),
    @BookName NVARCHAR(100),
    @Chapter  SMALLINT,
    @Verse    SMALLINT,
    @Now      DATETIME2(3)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @un NVARCHAR(256) = (SELECT UserName FROM [Identity].[User] WHERE Id = @UserId);
    DECLARE @book TINYINT = (SELECT TOP 1 k.BookId FROM dbo.DM2_VerseKey k JOIN dbo.eBible e ON e.Id = k.VerseId WHERE e.BibleText = @BookName);
    IF @un IS NULL OR @book IS NULL RETURN;
    IF NOT EXISTS (SELECT 1 FROM dbo.DM2_VerseKey WHERE BookId = @book AND Chapter = @Chapter AND Verse = @Verse) RETURN;
    DECLARE @round INT;
    EXEC dbo.usp_DM2_EnsureOpenRound @UserName = @un, @StartedAt = @Now, @RoundNo = @round OUTPUT;
    UPDATE dbo.DM2_ReadingChapterProgress SET LastViewedVerse = @Verse, UpdatedAt = @Now
    WHERE UserName = @un AND RoundNo = @round AND BookId = @book AND Chapter = @Chapter;
    IF @@ROWCOUNT = 0
        INSERT INTO dbo.DM2_ReadingChapterProgress (UserName, RoundNo, BookId, Chapter, LastViewedVerse, UpdatedAt)
        VALUES (@un, @round, @book, @Chapter, @Verse, @Now);
END
GO

SET NOEXEC OFF;
GO
SELECT (SELECT COUNT(*) FROM dbo.DM2_ReadingChapterProgress) AS Progress, (SELECT COUNT(*) FROM dbo.DM2_ReadingChapterRead) AS ChapterRead;
GO

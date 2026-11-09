/* ==========================================================================
   MissionActivity_20261109.sql   (step 3A of docs/mission-reading unified table design)
   Creates the unified mission record tables and copies the old click-history rows into them.

     dbo.DM2_MissionActivity : who did which mission on which day (one row per mission item, points included)
     dbo.DM2_QuizAttempt     : every quiz try (question number is kept only here)

   Old tables (kept, never changed here):
     ClickHistoryForTodayWordToRead  -> Word.Me / Word.Parent      (1 point each, max 2 per day)
     ClickHistoryForPrayPhrase       -> Prayer.Me / Prayer.Others  (1 point each, max 2 per day; the ToFriend column has no point)
     ClickHistoryForQuestion         -> DM2_QuizAttempt + Quiz.Correct (1 point per day, even if answered right twice)
     ClickHistoryForWeeklyWordToRead -> Recite                     (1 point per week)

   Rules
     - Users that do not exist in [Identity].[User] are skipped (their rows stay only in the old tables).
     - Safe to run again and again: it only adds rows that are missing (run it once before deploy and once right after).
     - Plain ASCII on purpose (safe in any editor).
   ========================================================================== */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.DM2_MissionActivity') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_MissionActivity (
        ActivityId      BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DM2_MissionActivity PRIMARY KEY,
        UserName        NVARCHAR(256) NOT NULL,
        MissionDate     DATE          NOT NULL,
        MissionItem     VARCHAR(20)   NOT NULL,
        Points          INT           NOT NULL CONSTRAINT CK_DM2_MissionActivity_Points CHECK (Points >= 0),
        ReciteWeekStart DATE          NULL,
        PerformedAt     DATETIME2(3)  NOT NULL CONSTRAINT DF_DM2_MissionActivity_At DEFAULT (SYSDATETIME()),
        CONSTRAINT CK_DM2_MissionActivity_Item CHECK (MissionItem IN ('Word.Me','Word.Parent','Prayer.Me','Prayer.Others','Quiz.Correct','Recite')),
        CONSTRAINT CK_DM2_MissionActivity_Week CHECK ((MissionItem = 'Recite' AND ReciteWeekStart IS NOT NULL) OR (MissionItem <> 'Recite' AND ReciteWeekStart IS NULL))
    );
    CREATE UNIQUE INDEX UX_DM2_MissionActivity_Daily  ON dbo.DM2_MissionActivity (UserName, MissionDate, MissionItem) WHERE MissionItem <> 'Recite';
    CREATE UNIQUE INDEX UX_DM2_MissionActivity_Recite ON dbo.DM2_MissionActivity (UserName, ReciteWeekStart)          WHERE MissionItem = 'Recite';
    CREATE INDEX IX_DM2_MissionActivity_Date ON dbo.DM2_MissionActivity (MissionDate) INCLUDE (UserName, MissionItem, Points);
END
GO

IF OBJECT_ID(N'dbo.DM2_QuizAttempt') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_QuizAttempt (
        UserName          NVARCHAR(256) NOT NULL,
        MissionDate       DATE          NOT NULL,
        QuestionId        INT           NOT NULL,
        AttemptNo         TINYINT       NOT NULL CONSTRAINT CK_DM2_QuizAttempt_No CHECK (AttemptNo >= 1),
        AnswerText        NVARCHAR(400) NULL,
        CorrectAnswerText NVARCHAR(400) NULL,
        IsCorrect         BIT           NOT NULL,
        AttemptedAt       DATETIME2(3)  NOT NULL CONSTRAINT DF_DM2_QuizAttempt_At DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_DM2_QuizAttempt PRIMARY KEY (UserName, MissionDate, QuestionId, AttemptNo)
    );
END
GO

/* ---- copy: Word.Me / Word.Parent ---- */
INSERT INTO dbo.DM2_MissionActivity (UserName, MissionDate, MissionItem, Points, PerformedAt)
SELECT u.UserName, CAST(h.ClickDate AS DATE), v.Item, 1, MIN(ISNULL(h.InsertDT, h.ClickDate))
FROM dbo.ClickHistoryForTodayWordToRead h
JOIN [Identity].[User] u ON LOWER(u.UserName) = LOWER(h.UserName)
CROSS APPLY (VALUES ('Word.Me', h.ToMe), ('Word.Parent', h.ToParent)) v(Item, Flag)
WHERE h.UserName IS NOT NULL AND LOWER(v.Flag) = 'y'
  AND NOT EXISTS (SELECT 1 FROM dbo.DM2_MissionActivity a WHERE a.UserName = u.UserName AND a.MissionDate = CAST(h.ClickDate AS DATE) AND a.MissionItem = v.Item)
GROUP BY u.UserName, CAST(h.ClickDate AS DATE), v.Item;
GO

/* ---- copy: Prayer.Me / Prayer.Others (ToFriend has no point and is not copied) ---- */
INSERT INTO dbo.DM2_MissionActivity (UserName, MissionDate, MissionItem, Points, PerformedAt)
SELECT u.UserName, CAST(h.ClickDate AS DATE), v.Item, 1, MIN(ISNULL(h.InsertDT, h.ClickDate))
FROM dbo.ClickHistoryForPrayPhrase h
JOIN [Identity].[User] u ON LOWER(u.UserName) = LOWER(h.UserName)
CROSS APPLY (VALUES ('Prayer.Me', h.ToMe), ('Prayer.Others', h.ToParent)) v(Item, Flag)
WHERE h.UserName IS NOT NULL AND LOWER(v.Flag) = 'y'
  AND NOT EXISTS (SELECT 1 FROM dbo.DM2_MissionActivity a WHERE a.UserName = u.UserName AND a.MissionDate = CAST(h.ClickDate AS DATE) AND a.MissionItem = v.Item)
GROUP BY u.UserName, CAST(h.ClickDate AS DATE), v.Item;
GO

/* ---- copy: every quiz try (AttemptNo = order of the rows that day, so no old row is lost) ---- */
;WITH q AS (
    SELECT u.UserName, CAST(h.ClickDate AS DATE) AS MissionDate, h.QuestionId, h.AnswerQ, h.CorrectAnswer,
           ISNULL(h.InsertDT, h.ClickDate) AS At,
           ROW_NUMBER() OVER (PARTITION BY u.UserName, CAST(h.ClickDate AS DATE), h.QuestionId ORDER BY h.Id) AS AttemptNo
    FROM dbo.ClickHistoryForQuestion h
    JOIN [Identity].[User] u ON LOWER(u.UserName) = LOWER(h.UserName)
    WHERE h.UserName IS NOT NULL
)
INSERT INTO dbo.DM2_QuizAttempt (UserName, MissionDate, QuestionId, AttemptNo, AnswerText, CorrectAnswerText, IsCorrect, AttemptedAt)
SELECT q.UserName, q.MissionDate, q.QuestionId, CASE WHEN q.AttemptNo > 255 THEN 255 ELSE q.AttemptNo END, q.AnswerQ, q.CorrectAnswer,
       CASE WHEN q.AnswerQ = q.CorrectAnswer THEN 1 ELSE 0 END, q.At
FROM q
WHERE q.AttemptNo <= 255
  AND NOT EXISTS (SELECT 1 FROM dbo.DM2_QuizAttempt t WHERE t.UserName = q.UserName AND t.MissionDate = q.MissionDate AND t.QuestionId = q.QuestionId AND t.AttemptNo = q.AttemptNo);
GO

/* ---- copy: Quiz.Correct = 1 point per day when any try that day was right ---- */
INSERT INTO dbo.DM2_MissionActivity (UserName, MissionDate, MissionItem, Points, PerformedAt)
SELECT t.UserName, t.MissionDate, 'Quiz.Correct', 1, MIN(t.AttemptedAt)
FROM dbo.DM2_QuizAttempt t
WHERE t.IsCorrect = 1
  AND NOT EXISTS (SELECT 1 FROM dbo.DM2_MissionActivity a WHERE a.UserName = t.UserName AND a.MissionDate = t.MissionDate AND a.MissionItem = 'Quiz.Correct')
GROUP BY t.UserName, t.MissionDate;
GO

/* ---- copy: Recite = 1 point per week (week start = DateFrom of the weekly word; if that word is gone, the Monday of the click day) ---- */
;WITH r AS (
    SELECT u.UserName, CAST(h.ClickDate AS DATE) AS MissionDate,
           COALESCE(CAST(w.DateFrom AS DATE), DATEADD(DAY, -(DATEDIFF(DAY, '2024-01-01', CAST(h.ClickDate AS DATE)) % 7), CAST(h.ClickDate AS DATE))) AS WeekStart,
           ISNULL(h.InsertDT, h.ClickDate) AS At
    FROM dbo.ClickHistoryForWeeklyWordToRead h
    JOIN [Identity].[User] u ON LOWER(u.UserName) = LOWER(h.UserName)
    LEFT JOIN dbo.WeeklyWordToRead w ON w.Id = h.WeeklyWordToReadId
    WHERE h.UserName IS NOT NULL
),
r1 AS (
    SELECT UserName, WeekStart, MIN(MissionDate) AS MissionDate, MIN(At) AS At FROM r GROUP BY UserName, WeekStart
)
INSERT INTO dbo.DM2_MissionActivity (UserName, MissionDate, MissionItem, Points, ReciteWeekStart, PerformedAt)
SELECT r1.UserName, r1.MissionDate, 'Recite', 1, r1.WeekStart, r1.At
FROM r1
WHERE NOT EXISTS (SELECT 1 FROM dbo.DM2_MissionActivity a WHERE a.UserName = r1.UserName AND a.MissionItem = 'Recite' AND a.ReciteWeekStart = r1.WeekStart);
GO

/* ---- report ---- */
SELECT MissionItem, COUNT(*) AS Rows_, SUM(Points) AS Points_ FROM dbo.DM2_MissionActivity GROUP BY MissionItem ORDER BY MissionItem;
SELECT COUNT(*) AS QuizAttemptRows FROM dbo.DM2_QuizAttempt;
SELECT N'skipped users (not in Identity.User)' AS Info, COUNT(DISTINCT x.UserName) AS Users
FROM (SELECT UserName FROM dbo.ClickHistoryForTodayWordToRead UNION SELECT UserName FROM dbo.ClickHistoryForPrayPhrase
      UNION SELECT UserName FROM dbo.ClickHistoryForQuestion UNION SELECT UserName FROM dbo.ClickHistoryForWeeklyWordToRead) x
WHERE x.UserName IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Identity].[User] u WHERE LOWER(u.UserName) = LOWER(x.UserName));
GO

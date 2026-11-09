/* ==========================================================================
   MissionActivity_Readers_20261109.sql   (step 3A-2)
   Points functions and views now read the unified tables (DM2_MissionActivity / DM2_QuizAttempt)
   instead of the four old ClickHistoryFor* tables. Names, parameters and output columns are unchanged,
   so every caller keeps working.

   Run order on production (cutover):
     1) SQL/MissionActivity_20261109.sql          (tables + copy)
     2) this script                                (readers)
     3) deploy the new code (it writes the new tables)
     4) SQL/MissionActivity_20261109.sql again     (picks up clicks that landed in the old tables in between)
   Rollback: SQL/MissionActivity_Readers_20261109_old_definitions.sql puts the old readers back.
   Plain ASCII on purpose.
   ========================================================================== */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

/* ---- view: one row per earned point (same shape as before) ---- */
ALTER VIEW dbo.view_click_data_all_person
AS
SELECT CAST(MissionDate AS DATETIME) AS clickDate, UserName AS userName,
       CASE MissionItem
            WHEN 'Prayer.Me'      THEN 'PrayToMe'
            WHEN 'Prayer.Others'  THEN 'PrayToparent'
            WHEN 'Quiz.Correct'   THEN 'Question'
            WHEN 'Word.Me'        THEN 'TodayToMe'
            WHEN 'Word.Parent'    THEN 'TodayToParent'
            WHEN 'Recite'         THEN 'Weekly'
       END AS TKind
FROM dbo.DM2_MissionActivity;
GO

ALTER VIEW dbo.vw_DM2_ActiveUsers
AS
    SELECT UserName FROM dbo.DM2_MissionActivity WHERE UserName IS NOT NULL
    UNION
    SELECT UserName FROM dbo.DM2_QuizAttempt WHERE UserName IS NOT NULL;
GO

/* ---- all-time totals per user ---- */
ALTER FUNCTION dbo.ufn_UserAreaPoints
(
    @UserName NVARCHAR(256) = NULL
)
RETURNS TABLE
AS
RETURN
WITH C AS (
    SELECT UserName,
           SUM(CASE WHEN MissionItem = 'Prayer.Me'     THEN 1 ELSE 0 END) AS PrayPhraseToMe,
           SUM(CASE WHEN MissionItem = 'Prayer.Others' THEN 1 ELSE 0 END) AS PrayPhraseToParent,
           SUM(CASE WHEN MissionItem = 'Quiz.Correct'  THEN 1 ELSE 0 END) AS Question,
           SUM(CASE WHEN MissionItem = 'Word.Me'       THEN 1 ELSE 0 END) AS TodayWordToReadToMe,
           SUM(CASE WHEN MissionItem = 'Word.Parent'   THEN 1 ELSE 0 END) AS TodayWordToReadToParent,
           SUM(CASE WHEN MissionItem = 'Recite'        THEN 1 ELSE 0 END) AS WeeklyWordToRead
    FROM dbo.DM2_MissionActivity
    WHERE @UserName IS NULL OR LOWER(UserName) = LOWER(@UserName)
    GROUP BY UserName
)
SELECT UserName, PrayPhraseToMe, PrayPhraseToParent,
       0 AS PrayPhraseToFriend,
       Question, TodayWordToReadToMe, TodayWordToReadToParent, WeeklyWordToRead,
       (PrayPhraseToMe + PrayPhraseToParent + Question + TodayWordToReadToMe + TodayWordToReadToParent + WeeklyWordToRead) AS Total
FROM C;
GO

/* ---- totals for one day (or all days when @DateStr is NULL) ---- */
ALTER FUNCTION dbo.ufn_UserAreaPointsByDate
(
    @UserName NVARCHAR(256) = NULL,
    @DateStr  NVARCHAR(30)  = NULL
)
RETURNS TABLE
AS
RETURN
WITH P AS (SELECT TRY_CONVERT(date, @DateStr) AS D),
C AS (
    SELECT a.UserName,
           SUM(CASE WHEN a.MissionItem = 'Prayer.Me'     THEN 1 ELSE 0 END) AS PrayPhraseToMe,
           SUM(CASE WHEN a.MissionItem = 'Prayer.Others' THEN 1 ELSE 0 END) AS PrayPhraseToParent,
           SUM(CASE WHEN a.MissionItem = 'Quiz.Correct'  THEN 1 ELSE 0 END) AS Question,
           SUM(CASE WHEN a.MissionItem = 'Word.Me'       THEN 1 ELSE 0 END) AS TodayWordToReadToMe,
           SUM(CASE WHEN a.MissionItem = 'Word.Parent'   THEN 1 ELSE 0 END) AS TodayWordToReadToParent,
           SUM(CASE WHEN a.MissionItem = 'Recite'        THEN 1 ELSE 0 END) AS WeeklyWordToRead
    FROM dbo.DM2_MissionActivity a
    CROSS JOIN P
    WHERE (@UserName IS NULL OR LOWER(a.UserName) = LOWER(@UserName))
      AND ( P.D IS NULL
            OR (a.MissionItem <> 'Recite' AND a.MissionDate = P.D)
            OR (a.MissionItem = 'Recite'  AND P.D BETWEEN a.ReciteWeekStart AND DATEADD(DAY, 6, a.ReciteWeekStart)) )
    GROUP BY a.UserName
)
SELECT UserName, PrayPhraseToMe, PrayPhraseToParent,
       0 AS PrayPhraseToFriend,
       Question, TodayWordToReadToMe, TodayWordToReadToParent, WeeklyWordToRead,
       (PrayPhraseToMe + PrayPhraseToParent + Question + TodayWordToReadToMe + TodayWordToReadToParent + WeeklyWordToRead) AS Total
FROM C;
GO

/* ---- same, filtered by year (the year of the mission date) ---- */
ALTER FUNCTION dbo.ufn_UserAreaPointsByDate_ByYear
(
    @UserName NVARCHAR(256) = NULL,
    @DateStr  NVARCHAR(30)  = NULL,
    @CYear    INT           = NULL
)
RETURNS TABLE
AS
RETURN
WITH P AS (SELECT TRY_CONVERT(date, @DateStr) AS D),
C AS (
    SELECT a.UserName,
           SUM(CASE WHEN a.MissionItem = 'Prayer.Me'     THEN 1 ELSE 0 END) AS PrayPhraseToMe,
           SUM(CASE WHEN a.MissionItem = 'Prayer.Others' THEN 1 ELSE 0 END) AS PrayPhraseToParent,
           SUM(CASE WHEN a.MissionItem = 'Quiz.Correct'  THEN 1 ELSE 0 END) AS Question,
           SUM(CASE WHEN a.MissionItem = 'Word.Me'       THEN 1 ELSE 0 END) AS TodayWordToReadToMe,
           SUM(CASE WHEN a.MissionItem = 'Word.Parent'   THEN 1 ELSE 0 END) AS TodayWordToReadToParent,
           SUM(CASE WHEN a.MissionItem = 'Recite'        THEN 1 ELSE 0 END) AS WeeklyWordToRead
    FROM dbo.DM2_MissionActivity a
    CROSS JOIN P
    WHERE (@UserName IS NULL OR LOWER(a.UserName) = LOWER(@UserName))
      AND (@CYear IS NULL OR YEAR(a.MissionDate) = @CYear)
      AND ( P.D IS NULL
            OR (a.MissionItem <> 'Recite' AND a.MissionDate = P.D)
            OR (a.MissionItem = 'Recite'  AND P.D BETWEEN a.ReciteWeekStart AND DATEADD(DAY, 6, a.ReciteWeekStart)) )
    GROUP BY a.UserName
)
SELECT UserName, PrayPhraseToMe, PrayPhraseToParent,
       0 AS PrayPhraseToFriend,
       Question, TodayWordToReadToMe, TodayWordToReadToParent, WeeklyWordToRead,
       (PrayPhraseToMe + PrayPhraseToParent + Question + TodayWordToReadToMe + TodayWordToReadToParent + WeeklyWordToRead) AS Total
FROM C;
GO

/* ---- one user, one year ---- */
ALTER FUNCTION dbo.ufn_DM2_GetUserYearPoint
(
    @userName NVARCHAR(256),
    @cYear INT
)
RETURNS TABLE
AS
RETURN
(
    WITH A AS (
        SELECT SUM(CASE WHEN MissionItem IN ('Word.Me','Word.Parent')     THEN 1 ELSE 0 END) AS WordPoint,
               SUM(CASE WHEN MissionItem IN ('Prayer.Me','Prayer.Others') THEN 1 ELSE 0 END) AS PrayPoint,
               SUM(CASE WHEN MissionItem = 'Quiz.Correct'                 THEN 1 ELSE 0 END) AS QuizPoint,
               SUM(CASE WHEN MissionItem = 'Recite'                       THEN 1 ELSE 0 END) AS RecitePoint
        FROM dbo.DM2_MissionActivity
        WHERE UserName = @userName AND YEAR(MissionDate) = @cYear
    )
    SELECT
        @userName AS UserName,
        @cYear AS cYear,
        ISNULL(WordPoint, 0)   AS WordPoint,
        ISNULL(PrayPoint, 0)   AS PrayPoint,
        ISNULL(QuizPoint, 0)   AS QuizPoint,
        ISNULL(RecitePoint, 0) AS RecitePoint,
        ISNULL(WordPoint, 0) + ISNULL(PrayPoint, 0) + ISNULL(QuizPoint, 0) + ISNULL(RecitePoint, 0) AS YearTotalPoint
    FROM A
);
GO

/* ---- day-by-day history of one user (2026-01-01 .. today) ---- */
ALTER FUNCTION dbo.ufn_GetUserClickHistory
(
    @userName NVARCHAR(100)
)
RETURNS @Result TABLE
(
    [DateBy] DATE,

    [word_ToMe] NVARCHAR(1),
    [word_ToParent] NVARCHAR(1),
    [word_Cnt] int,

    [quiz_result] NVARCHAR(1),
    [quiz_Cnt] int,

    [pray_ToMe] NVARCHAR(1),
    [pray_ToParent] NVARCHAR(1),
    [pray_Cnt] int,

    [recite_yn]  NVARCHAR(1),
    [recite_Cnt] int,

    [chcekYN] NVARCHAR(10)
)
AS
BEGIN
    WITH DateList AS (
        SELECT DATEADD(DAY, v.number, '2026-01-01') AS Date
        FROM master.dbo.spt_values v
        WHERE v.type = 'P'
          AND v.number BETWEEN 0 AND DATEDIFF(DAY, '2026-01-01', GETDATE()+1)
    ),
    t1 AS (
        SELECT MissionDate AS clickDate,
               MAX(CASE WHEN MissionItem = 'Word.Me' THEN 1 ELSE 0 END) AS HasMe,
               MAX(CASE WHEN MissionItem = 'Word.Parent' THEN 1 ELSE 0 END) AS HasParent
        FROM dbo.DM2_MissionActivity
        WHERE UserName = @userName AND MissionItem IN ('Word.Me','Word.Parent')
        GROUP BY MissionDate
    ),
    t2 AS (
        SELECT MissionDate AS clickDate,
               MAX(CASE WHEN IsCorrect = 1 THEN 't' ELSE 'f' END) AS QuizResult,
               COUNT(*) AS Cnt
        FROM dbo.DM2_QuizAttempt
        WHERE UserName = @userName
        GROUP BY MissionDate
    ),
    t3 AS (
        SELECT MissionDate AS clickDate,
               MAX(CASE WHEN MissionItem = 'Prayer.Me' THEN 1 ELSE 0 END) AS HasMe,
               MAX(CASE WHEN MissionItem = 'Prayer.Others' THEN 1 ELSE 0 END) AS HasOthers
        FROM dbo.DM2_MissionActivity
        WHERE UserName = @userName AND MissionItem IN ('Prayer.Me','Prayer.Others')
        GROUP BY MissionDate
    ),
    t4 AS (
        SELECT MissionDate AS clickDate, COUNT(*) AS Cnt
        FROM dbo.DM2_MissionActivity
        WHERE UserName = @userName AND MissionItem = 'Recite'
        GROUP BY MissionDate
    ),
    t5 AS (  -- a day that has a correct quiz point but no try row would still be a correct day
        SELECT MissionDate AS clickDate
        FROM dbo.DM2_MissionActivity
        WHERE UserName = @userName AND MissionItem = 'Quiz.Correct'
    )
    INSERT INTO @Result
    SELECT
        D.Date AS DateBy,

        CASE WHEN t1.clickDate IS NULL THEN '' WHEN t1.HasMe = 1 THEN 'y' ELSE 'n' END AS [word_ToMe],
        CASE WHEN t1.clickDate IS NULL THEN '' WHEN t1.HasParent = 1 THEN 'y' ELSE 'n' END AS [word_ToParent],
        CASE WHEN t1.clickDate IS NULL THEN 0 ELSE 1 END AS [word_Cnt],

        CASE WHEN t5.clickDate IS NOT NULL THEN 't' ELSE t2.QuizResult END AS [quiz_result],
        t2.Cnt AS [quiz_Cnt],

        CASE WHEN t3.clickDate IS NULL THEN '' WHEN t3.HasMe = 1 THEN 'y' ELSE 'n' END AS [pray_ToMe],
        CASE WHEN t3.clickDate IS NULL THEN '' WHEN t3.HasOthers = 1 THEN 'y' ELSE 'n' END AS [pray_ToParent],
        CASE WHEN t3.clickDate IS NULL THEN 0 ELSE 1 END AS [pray_Cnt],

        CASE WHEN t4.clickDate IS NULL THEN '' ELSE 'y' END AS [Recite_yn],
        ISNULL(t4.Cnt, 0) AS [Recite_Cnt],

        CASE WHEN ISNULL(t2.Cnt, 0) > 1 OR ISNULL(t4.Cnt, 0) > 1 THEN 'check' ELSE '' END AS chcekYN
    FROM DateList D
    LEFT JOIN t1 ON D.Date = t1.clickDate
    LEFT JOIN t2 ON D.Date = t2.clickDate
    LEFT JOIN t3 ON D.Date = t3.clickDate
    LEFT JOIN t4 ON D.Date = t4.clickDate
    LEFT JOIN t5 ON D.Date = t5.clickDate;

    RETURN;
END
GO

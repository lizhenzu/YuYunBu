/* Old definitions of the readers replaced by MissionActivity_Readers_20261109.sql - run this to roll them back (plain ASCII text of the old objects; Korean comments inside may need a UTF-8 aware editor) */
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;
GO
ALTER view view_click_data_all_person

as 
select clickDate ,userName ,'PrayToMe'      TKind from [dbo].[ClickHistoryForPrayPhrase]       where ToMe     = 'y'   
  						  union all  
  						  select clickDate ,userName ,'PrayToparent'  TKind from [dbo].[ClickHistoryForPrayPhrase]       where Toparent = 'y'   
  						  union all  
  						  select clickDate ,userName ,'PrayToFriend'  TKind from [dbo].[ClickHistoryForPrayPhrase]       where ToFriend = 'y'   
  						  union all  
  						  select clickDate ,userName ,'Question'      TKind from [dbo].[ClickHistoryForQuestion]         where AnswerQ = CorrectAnswer                    
  						  union all  
  						  select clickDate ,userName ,'TodayToMe'     TKind from [dbo].[ClickHistoryForTodayWordToRead]  where ToMe     = 'y'   
  						  union all  
  						  select clickDate ,userName ,'TodayToParent' TKind from [dbo].[ClickHistoryForTodayWordToRead]  where ToParent = 'y'   
  						  union all  
  						  select clickDate ,userName ,'Weekly'        TKind from [dbo].[ClickHistoryForWeeklyWordToRead]
GO
ALTER VIEW dbo.vw_DM2_ActiveUsers
AS
    SELECT DISTINCT UserName FROM dbo.ClickHistoryForTodayWordToRead WHERE UserName IS NOT NULL
    UNION
    SELECT DISTINCT UserName FROM dbo.ClickHistoryForPrayPhrase WHERE UserName IS NOT NULL
    UNION
    SELECT DISTINCT UserName FROM dbo.ClickHistoryForQuestion WHERE UserName IS NOT NULL
    UNION
    SELECT DISTINCT UserName FROM dbo.ClickHistoryForWeeklyWordToRead WHERE UserName IS NOT NULL;
GO
ALTER FUNCTION dbo.ufn_UserAreaPoints
(
    @UserName NVARCHAR(256) = NULL  -- 필터할 사용자명, NULL이면 전체
)
RETURNS TABLE
AS
RETURN
WITH Base AS (
    /* PrayPhrase - ToMe */
    SELECT CA.UserName, 'PrayPhraseToMe' AS Category, COUNT(*) AS Cnt
    FROM [dbo].[ClickHistoryForPrayPhrase] CA
    JOIN [dbo].[PrayPhrase] PP ON CA.[PrayPhraseId] = PP.Id
    WHERE CA.UserName IS NOT NULL
      AND LOWER(CA.[ToMe]) = 'y'
    GROUP BY CA.UserName

    UNION ALL
    /* PrayPhrase - ToParent */
    SELECT CA.UserName, 'PrayPhraseToParent' AS Category, COUNT(*) AS Cnt
    FROM [dbo].[ClickHistoryForPrayPhrase] CA
    JOIN [dbo].[PrayPhrase] PP ON CA.[PrayPhraseId] = PP.Id
    WHERE CA.UserName IS NOT NULL
      AND LOWER(CA.[ToParent]) = 'y'
    GROUP BY CA.UserName

    UNION ALL
    /* PrayPhrase - ToFriend */
    SELECT CA.UserName, 'PrayPhraseToFriend' AS Category, COUNT(*) AS Cnt
    FROM [dbo].[ClickHistoryForPrayPhrase] CA
    JOIN [dbo].[PrayPhrase] PP ON CA.[PrayPhraseId] = PP.Id
    WHERE CA.UserName IS NOT NULL
      AND LOWER(CA.[ToFriend]) = 'y'
    GROUP BY CA.UserName

    UNION ALL
    /* Question - 정답만 카운트 */
    SELECT CA.UserName, 'Question' AS Category, COUNT(*) AS Cnt
    FROM [dbo].[ClickHistoryForQuestion] CA
    JOIN [dbo].[QuestionData] QD ON CA.[QuestionId] = QD.Id
    WHERE CA.UserName IS NOT NULL
      AND CA.[AnswerQ] = CA.[CorrectAnswer]
    GROUP BY CA.UserName

    UNION ALL
    /* TodayWordToRead - ToMe */
    SELECT CA.UserName, 'TodayWordToReadToMe' AS Category, COUNT(*) AS Cnt
    FROM [dbo].[ClickHistoryForTodayWordToRead] CA
    JOIN [dbo].[TodayWordToRead] TW ON CA.[TodayWordToReadId] = TW.Id
    WHERE CA.UserName IS NOT NULL
      AND LOWER(CA.[ToMe]) = 'y'
    GROUP BY CA.UserName

    UNION ALL
    /* TodayWordToRead - ToParent */
    SELECT CA.UserName, 'TodayWordToReadToParent' AS Category, COUNT(*) AS Cnt
    FROM [dbo].[ClickHistoryForTodayWordToRead] CA
    JOIN [dbo].[TodayWordToRead] TW ON CA.[TodayWordToReadId] = TW.Id
    WHERE CA.UserName IS NOT NULL
      AND LOWER(CA.[ToParent]) = 'y'
    GROUP BY CA.UserName

    UNION ALL
    /* WeeklyWordToRead */
    SELECT CA.UserName, 'WeeklyWordToRead' AS Category, COUNT(*) AS Cnt
    FROM [dbo].[ClickHistoryForWeeklyWordToRead] CA
    JOIN [dbo].[WeeklyWordToRead] WW ON CA.[WeeklyWordToReadId] = WW.Id
    WHERE CA.UserName IS NOT NULL
    GROUP BY CA.UserName
),
Pivoted AS (
    SELECT 
        UserName,
        ISNULL([PrayPhraseToMe]         , 0) AS PrayPhraseToMe,
        ISNULL([PrayPhraseToParent]     , 0) AS PrayPhraseToParent,
        ISNULL([PrayPhraseToFriend]     , 0) AS PrayPhraseToFriend,
        ISNULL([Question]               , 0) AS Question,
        ISNULL([TodayWordToReadToMe]    , 0) AS TodayWordToReadToMe,
        ISNULL([TodayWordToReadToParent], 0) AS TodayWordToReadToParent,
        ISNULL([WeeklyWordToRead]       , 0) AS WeeklyWordToRead
    FROM Base
    PIVOT (
        MAX(Cnt) FOR Category IN (
            [PrayPhraseToMe],
            [PrayPhraseToParent],
            [PrayPhraseToFriend],
            [Question],
            [TodayWordToReadToMe],
            [TodayWordToReadToParent],
            [WeeklyWordToRead]
        )
    ) p
)
SELECT
    UserName,
    PrayPhraseToMe,
    PrayPhraseToParent,
    PrayPhraseToFriend,
    Question,
    TodayWordToReadToMe,
    TodayWordToReadToParent,
    WeeklyWordToRead,
    (PrayPhraseToMe
   + PrayPhraseToParent
   + PrayPhraseToFriend
   + Question
   + TodayWordToReadToMe
   + TodayWordToReadToParent
   + WeeklyWordToRead) AS Total
FROM Pivoted
WHERE @UserName IS NULL 
   OR LOWER(UserName) = LOWER(@UserName);  -- NULL이면 전체, 값이 있으면 해당 사용자만
GO
ALTER FUNCTION [dbo].[ufn_UserAreaPointsByDate]
(
    @UserName NVARCHAR(256) = NULL,   -- 특정 사용자만 (NULL이면 전체)
    @DateStr  NVARCHAR(30)  = NULL    -- 'YYYY-MM-DD'; NULL이면 전 범위
)
RETURNS TABLE
AS
RETURN
WITH Params AS (
    SELECT TRY_CONVERT(date, @DateStr) AS TargetDate
),
-- 각 카테고리를 "사용자별 집계 1줄"로 만들기
PrayToMe AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForPrayPhrase CA
    JOIN dbo.PrayPhrase PP ON PP.Id = CA.PrayPhraseId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND LOWER(CA.ToMe) = 'y'
      AND (p.TargetDate IS NULL OR CONVERT(date, PP.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),
PrayToParent AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForPrayPhrase CA
    JOIN dbo.PrayPhrase PP ON PP.Id = CA.PrayPhraseId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND LOWER(CA.ToParent) = 'y'
      AND (p.TargetDate IS NULL OR CONVERT(date, PP.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),
PrayToFriend AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForPrayPhrase CA
    JOIN dbo.PrayPhrase PP ON PP.Id = CA.PrayPhraseId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND LOWER(CA.ToFriend) = 'y'
      AND (p.TargetDate IS NULL OR CONVERT(date, PP.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),
QuestionOK AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForQuestion CA
    JOIN dbo.QuestionData QD ON QD.Id = CA.QuestionId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND CA.AnswerQ = CA.CorrectAnswer
      AND (p.TargetDate IS NULL OR CONVERT(date, QD.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),
TodayToMe AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForTodayWordToRead CA
    JOIN dbo.TodayWordToRead TW ON TW.Id = CA.TodayWordToReadId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND LOWER(CA.ToMe) = 'y'
      AND (p.TargetDate IS NULL OR CONVERT(date, TW.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),
TodayToParent AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForTodayWordToRead CA
    JOIN dbo.TodayWordToRead TW ON TW.Id = CA.TodayWordToReadId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND LOWER(CA.ToParent) = 'y'
      AND (p.TargetDate IS NULL OR CONVERT(date, TW.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),
-- Weekly: @DateStr 있으면 "그 날 1/0", 없으면 "서로 다른 날짜 수"
--WeeklyDaily AS (  -- only when TargetDate is NOT NULL
--    SELECT u.UserName, 1 AS Cnt
--    FROM (
--        SELECT DISTINCT CA.UserName
--        FROM dbo.ClickHistoryForWeeklyWordToRead CA
--        CROSS JOIN Params p
--        WHERE p.TargetDate IS NOT NULL
--          AND CONVERT(date, CA.ClickDate) = p.TargetDate
--    ) u
--),

WeeklyDaily AS (  -- only when TargetDate is NOT NULL
    SELECT
        CA.UserName,
        COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForWeeklyWordToRead CA
    JOIN dbo.WeeklyWordToRead WW
        ON WW.Id = CA.WeeklyWordToReadId
    CROSS JOIN Params p
    WHERE p.TargetDate IS NOT NULL
      AND CA.UserName IS NOT NULL
      -- 예시와 동일하게 TargetDate에 +16시간 보정 후 날짜 비교
      AND CONVERT(date, p.TargetDate)
          BETWEEN CONVERT(date, WW.DateFrom) AND CONVERT(date, WW.DateTo)
    GROUP BY CA.UserName
),
WeeklyAll AS (
    --SELECT CA.UserName, COUNT(*) AS Cnt
    --FROM dbo.ClickHistoryForWeeklyWordToRead CA
    --CROSS JOIN Params p
    --WHERE p.TargetDate IS NULL
    --GROUP BY CA.UserName
    SELECT
        CA.UserName,
        COUNT(DISTINCT CA.WeeklyWordToReadId) AS Cnt
    FROM dbo.ClickHistoryForWeeklyWordToRead CA
    CROSS JOIN Params p
    WHERE p.TargetDate IS NULL
      AND CA.UserName IS NOT NULL
    GROUP BY CA.UserName

),
-- 모든 사용자 목록 (어느 카테고리에든 등장한 사용자)
Users AS (
    SELECT UserName FROM PrayToMe
    UNION
    SELECT UserName FROM PrayToParent
    UNION
    SELECT UserName FROM PrayToFriend
    UNION
    SELECT UserName FROM QuestionOK
    UNION
    SELECT UserName FROM TodayToMe
    UNION
    SELECT UserName FROM TodayToParent
    UNION
    SELECT UserName FROM WeeklyDaily
    UNION
    SELECT UserName FROM WeeklyAll
)
SELECT
    u.UserName,
    ISNULL(pm.Cnt, 0)  AS PrayPhraseToMe,
    ISNULL(pp.Cnt, 0)  AS PrayPhraseToParent,
    ISNULL(pf.Cnt, 0)  AS PrayPhraseToFriend,
    ISNULL(qo.Cnt, 0)  AS Question,
    ISNULL(tm.Cnt, 0)  AS TodayWordToReadToMe,
    ISNULL(tp.Cnt, 0)  AS TodayWordToReadToParent,
    ISNULL( CASE WHEN (SELECT TargetDate FROM Params) IS NULL 
                 THEN wa.Cnt ELSE wd.Cnt END , 0) AS WeeklyWordToRead,
    -- 최종 합계
    ISNULL(pm.Cnt,0) + ISNULL(pp.Cnt,0) + ISNULL(pf.Cnt,0) +
    ISNULL(qo.Cnt,0) + ISNULL(tm.Cnt,0) + ISNULL(tp.Cnt,0) +
    ISNULL( CASE WHEN (SELECT TargetDate FROM Params) IS NULL 
                 THEN wa.Cnt ELSE wd.Cnt END , 0) AS Total
FROM Users u
LEFT JOIN PrayToMe       pm ON pm.UserName = u.UserName
LEFT JOIN PrayToParent   pp ON pp.UserName = u.UserName
LEFT JOIN PrayToFriend   pf ON pf.UserName = u.UserName
LEFT JOIN QuestionOK     qo ON qo.UserName = u.UserName
LEFT JOIN TodayToMe      tm ON tm.UserName = u.UserName
LEFT JOIN TodayToParent  tp ON tp.UserName = u.UserName
LEFT JOIN WeeklyDaily    wd ON wd.UserName = u.UserName
LEFT JOIN WeeklyAll      wa ON wa.UserName = u.UserName
WHERE (@UserName IS NULL OR LOWER(u.UserName) = LOWER(@UserName));
GO
ALTER FUNCTION [dbo].[ufn_UserAreaPointsByDate_ByYear]
(
    @UserName NVARCHAR(256) = NULL,   -- 특정 사용자만 (NULL이면 전체)
    @DateStr  NVARCHAR(30)  = NULL,   -- 'YYYY-MM-DD'; NULL이면 전 범위
    @CYear    INT           = NULL    -- ✅ 추가: 년도 필터 (NULL이면 전체)
)
RETURNS TABLE
AS
RETURN
WITH Params AS (
    SELECT TRY_CONVERT(date, @DateStr) AS TargetDate
),

/* ---------------------------
   PrayPhrase
--------------------------- */
PrayToMe AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForPrayPhrase CA
    JOIN dbo.PrayPhrase PP ON PP.Id = CA.PrayPhraseId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND (@CYear IS NULL OR CA.cyear = @CYear)            -- ✅ 추가
      AND LOWER(CA.ToMe) = 'y'
      AND (p.TargetDate IS NULL OR CONVERT(date, PP.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),
PrayToParent AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForPrayPhrase CA
    JOIN dbo.PrayPhrase PP ON PP.Id = CA.PrayPhraseId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND (@CYear IS NULL OR CA.cyear = @CYear)            -- ✅ 추가
      AND LOWER(CA.ToParent) = 'y'
      AND (p.TargetDate IS NULL OR CONVERT(date, PP.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),
PrayToFriend AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForPrayPhrase CA
    JOIN dbo.PrayPhrase PP ON PP.Id = CA.PrayPhraseId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND (@CYear IS NULL OR CA.cyear = @CYear)            -- ✅ 추가
      AND LOWER(CA.ToFriend) = 'y'
      AND (p.TargetDate IS NULL OR CONVERT(date, PP.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),

/* ---------------------------
   Question
--------------------------- */
QuestionOK AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForQuestion CA
    JOIN dbo.QuestionData QD ON QD.Id = CA.QuestionId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND (@CYear IS NULL OR CA.cyear = @CYear)            -- ✅ 추가
      AND CA.AnswerQ = CA.CorrectAnswer
      AND (p.TargetDate IS NULL OR CONVERT(date, QD.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),

/* ---------------------------
   TodayWordToRead
--------------------------- */
TodayToMe AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForTodayWordToRead CA
    JOIN dbo.TodayWordToRead TW ON TW.Id = CA.TodayWordToReadId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND (@CYear IS NULL OR CA.cyear = @CYear)            -- ✅ 추가
      AND LOWER(CA.ToMe) = 'y'
      AND (p.TargetDate IS NULL OR CONVERT(date, TW.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),
TodayToParent AS (
    SELECT CA.UserName, COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForTodayWordToRead CA
    JOIN dbo.TodayWordToRead TW ON TW.Id = CA.TodayWordToReadId
    CROSS JOIN Params p
    WHERE CA.UserName IS NOT NULL
      AND (@CYear IS NULL OR CA.cyear = @CYear)            -- ✅ 추가
      AND LOWER(CA.ToParent) = 'y'
      AND (p.TargetDate IS NULL OR CONVERT(date, TW.ExposeDate) = p.TargetDate)
    GROUP BY CA.UserName
),

/* ---------------------------
   WeeklyWordToRead
--------------------------- */
WeeklyDaily AS (
    SELECT
        CA.UserName,
        COUNT(*) AS Cnt
    FROM dbo.ClickHistoryForWeeklyWordToRead CA
    JOIN dbo.WeeklyWordToRead WW
        ON WW.Id = CA.WeeklyWordToReadId
    CROSS JOIN Params p
    WHERE p.TargetDate IS NOT NULL
      AND CA.UserName IS NOT NULL
      AND (@CYear IS NULL OR CA.cyear = @CYear)            -- ✅ 추가
      AND CONVERT(date, p.TargetDate)
          BETWEEN CONVERT(date, WW.DateFrom) AND CONVERT(date, WW.DateTo)
    GROUP BY CA.UserName
),
WeeklyAll AS (
    SELECT
        CA.UserName,
        COUNT(DISTINCT CA.WeeklyWordToReadId) AS Cnt
    FROM dbo.ClickHistoryForWeeklyWordToRead CA
    CROSS JOIN Params p
    WHERE p.TargetDate IS NULL
      AND CA.UserName IS NOT NULL
      AND (@CYear IS NULL OR CA.cyear = @CYear)            -- ✅ 추가
    GROUP BY CA.UserName
),

/* ---------------------------
   Users: 4개 집계에서 등장한 사용자만
--------------------------- */
Users AS (
    SELECT UserName FROM PrayToMe
    UNION
    SELECT UserName FROM PrayToParent
    UNION
    SELECT UserName FROM PrayToFriend
    UNION
    SELECT UserName FROM QuestionOK
    UNION
    SELECT UserName FROM TodayToMe
    UNION
    SELECT UserName FROM TodayToParent
    UNION
    SELECT UserName FROM WeeklyDaily
    UNION
    SELECT UserName FROM WeeklyAll
)

SELECT
    u.UserName,
    ISNULL(pm.Cnt, 0)  AS PrayPhraseToMe,
    ISNULL(pp.Cnt, 0)  AS PrayPhraseToParent,
    ISNULL(pf.Cnt, 0)  AS PrayPhraseToFriend,
    ISNULL(qo.Cnt, 0)  AS Question,
    ISNULL(tm.Cnt, 0)  AS TodayWordToReadToMe,
    ISNULL(tp.Cnt, 0)  AS TodayWordToReadToParent,
    ISNULL(
        CASE WHEN (SELECT TargetDate FROM Params) IS NULL THEN wa.Cnt ELSE wd.Cnt END
    , 0) AS WeeklyWordToRead,

    ISNULL(pm.Cnt,0) + ISNULL(pp.Cnt,0) + ISNULL(pf.Cnt,0) +
    ISNULL(qo.Cnt,0) + ISNULL(tm.Cnt,0) + ISNULL(tp.Cnt,0) +
    ISNULL(
        CASE WHEN (SELECT TargetDate FROM Params) IS NULL THEN wa.Cnt ELSE wd.Cnt END
    , 0) AS Total
FROM Users u
LEFT JOIN PrayToMe       pm ON pm.UserName = u.UserName
LEFT JOIN PrayToParent   pp ON pp.UserName = u.UserName
LEFT JOIN PrayToFriend   pf ON pf.UserName = u.UserName
LEFT JOIN QuestionOK     qo ON qo.UserName = u.UserName
LEFT JOIN TodayToMe      tm ON tm.UserName = u.UserName
LEFT JOIN TodayToParent  tp ON tp.UserName = u.UserName
LEFT JOIN WeeklyDaily    wd ON wd.UserName = u.UserName
LEFT JOIN WeeklyAll      wa ON wa.UserName = u.UserName
WHERE (@UserName IS NULL OR LOWER(u.UserName) = LOWER(@UserName));
GO
ALTER FUNCTION dbo.ufn_DM2_GetUserYearPoint
(
    @userName NVARCHAR(256),
    @cYear INT
)
RETURNS TABLE
AS
RETURN
(
    WITH WordPoint AS
    (
        SELECT
            UserName,
            SUM(
                (CASE WHEN UPPER(ToMe)='Y' THEN 1 ELSE 0 END) +
                (CASE WHEN UPPER(ToParent)='Y' THEN 1 ELSE 0 END)
            ) AS Point
        FROM dbo.ClickHistoryForTodayWordToRead
        WHERE UserName = @userName
          AND cYear = @cYear
        GROUP BY UserName
    ),
    PrayPoint AS
    (
        SELECT
            UserName,
            SUM(
                (CASE WHEN UPPER(ToMe)='Y' THEN 1 ELSE 0 END) +
                (CASE WHEN UPPER(ToParent)='Y' OR UPPER(ToFriend)='Y' THEN 1 ELSE 0 END)
            ) AS Point
        FROM dbo.ClickHistoryForPrayPhrase
        WHERE UserName = @userName
          AND cYear = @cYear
        GROUP BY UserName
    ),
    QuizPoint AS
    (
        SELECT
            UserName,
            COUNT(*) AS Point
        FROM dbo.ClickHistoryForQuestion
        WHERE UserName = @userName
          AND cYear = @cYear
          AND AnswerQ = CorrectAnswer
        GROUP BY UserName
    ),
    RecitePoint AS
    (
        SELECT
            UserName,
            COUNT(DISTINCT WeeklyWordToReadId) AS Point
        FROM dbo.ClickHistoryForWeeklyWordToRead
        WHERE UserName = @userName
          AND cYear = @cYear
        GROUP BY UserName
    )
    SELECT
        @userName AS UserName,
        @cYear AS cYear,
        ISNULL(W.Point,0) AS WordPoint,
        ISNULL(P.Point,0) AS PrayPoint,
        ISNULL(Q.Point,0) AS QuizPoint,
        ISNULL(R.Point,0) AS RecitePoint,
        ISNULL(W.Point,0)
      + ISNULL(P.Point,0)
      + ISNULL(Q.Point,0)
      + ISNULL(R.Point,0) AS YearTotalPoint
    FROM (SELECT 1 AS X) T
    LEFT JOIN WordPoint W ON 1=1
    LEFT JOIN PrayPoint P ON 1=1
    LEFT JOIN QuizPoint Q ON 1=1
    LEFT JOIN RecitePoint R ON 1=1
);
GO
ALTER FUNCTION [dbo].[ufn_GetUserClickHistory]
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
        SELECT clickDate, 
               MAX(CASE ToMe     WHEN 'y' THEN ToMe     WHEN 'n' THEN ToMe     END) AS ToMe,
               MAX(CASE ToParent WHEN 'y' THEN ToParent WHEN 'n' THEN ToParent END) AS ToParent,
               COUNT(*) AS Cnt
        FROM [dbo].[ClickHistoryForTodayWordToRead]
        WHERE UserName = @userName
        GROUP BY clickDate
    ),

    t2 AS (
        SELECT clickDate, 
               MAX(CASE WHEN CorrectAnswer = AnswerQ THEN 't' ELSE 'f' END) AS QuizResult,
               COUNT(*) AS Cnt
        FROM [dbo].[ClickHistoryForQuestion]
        WHERE UserName = @userName
        GROUP BY clickDate
    ),

    t3 AS (
        SELECT clickDate,
               MAX(CASE ToMe     WHEN 'y' THEN ToMe     WHEN 'n' THEN ToMe     END) AS PrayToMe,
               MAX(CASE ToParent WHEN 'y' THEN ToParent WHEN 'n' THEN ToParent END) AS PrayToParent,
               COUNT(*) AS Cnt
        FROM [dbo].[ClickHistoryForPrayPhrase]
        WHERE UserName = @userName
        GROUP BY clickDate
    ),

    t4 AS (
        SELECT clickDate, 'y' AS WeeklyWord, COUNT(*) AS Cnt
        FROM [dbo].[ClickHistoryForWeeklyWordToRead]
        WHERE UserName = @userName
        GROUP BY clickDate
    )

    INSERT INTO @Result
    SELECT 
        D.Date            As DateBy ,

        isnull(t1.ToMe    ,'') AS [word_ToMe]    ,
        isnull(t1.ToParent,'') AS [word_ToParent],
        isnull(t1.Cnt,0)       AS [word_Cnt]     ,

        t2.QuizResult  AS [quiz_result]  ,
        t2.Cnt         AS [quiz_Cnt]     ,

        isnull(t3.PrayToMe,'')      AS [pray_ToMe],
        isnull(t3.PrayToParent,'')  AS [pray_ToParent],
        isnull(t3.Cnt,0)            AS [pray_Cnt]     ,

        CASE WHEN t4.WeeklyWord IS NULL THEN '' ELSE 'y' END AS [Recite_yn],
        isnull(t4.Cnt, 0)       AS [Recite_Cnt]     ,

        CASE 
            WHEN ISNULL(t1.Cnt,0) > 1 OR ISNULL(t2.Cnt,0) > 1 
              OR ISNULL(t3.Cnt,0) > 1 OR ISNULL(t4.Cnt,0) > 1 
            THEN 'check'
            ELSE ''
        END AS chcekYN
    FROM DateList D
    LEFT JOIN t1 ON D.Date = t1.clickDate
    LEFT JOIN t2 ON D.Date = t2.clickDate
    LEFT JOIN t3 ON D.Date = t3.clickDate
    LEFT JOIN t4 ON D.Date = t4.clickDate

    RETURN;
END
GO

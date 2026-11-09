/* ==========================================================================
   TeamAward_20261109.sql
   Team award record table.
     - dbo.DM2_TeamAward : "team X got the award of step T" (one row per team + step)
   The family-team lock (leave / kick / role change are blocked once a team has an award row)
   and the team-name reuse check read ONLY this table (they no longer parse the gift order memo text).
   There is no on/off switch: a team without an award row is simply not locked.

   Safe to run more than once (creates only what is missing, backfill adds only missing rows).
   Run it BEFORE deploying the new code. Old code keeps working (it does not use these tables).
   ========================================================================== */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.DM2_TeamAward') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_TeamAward (
        AwardId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DM2_TeamAward PRIMARY KEY,
        Kind       NVARCHAR(10)  NOT NULL CONSTRAINT CK_DM2_TeamAward_Kind CHECK (Kind IN (N'FAMILY', N'FRIEND')),
        AwardKey   NVARCHAR(200) NOT NULL,
        Threshold  INT           NOT NULL,
        AwardedAt  DATETIME      NOT NULL CONSTRAINT DF_DM2_TeamAward_At DEFAULT (GETDATE()),
        CONSTRAINT UQ_DM2_TeamAward UNIQUE (Kind, AwardKey, Threshold)
    );
    CREATE INDEX IX_DM2_TeamAward_Key ON dbo.DM2_TeamAward (AwardKey);
END
GO

/* Backfill from the existing gift orders (old memo format: family prefix + key [+ ":" + step], friend prefix + key + ":" + step).
   The two Korean prefixes are built from NCHAR codes so this file stays plain ASCII (safe in any editor). */
DECLARE @fam NVARCHAR(10) = NCHAR(44032) + NCHAR(51313) + NCHAR(54633) + NCHAR(49328) + N':';   -- family-sum:
DECLARE @fri NVARCHAR(10) = NCHAR(52828) + NCHAR(44396) + NCHAR(54633) + NCHAR(49328) + N':';   -- friend-sum:
;WITH src AS (
    SELECT CASE WHEN o.Memo LIKE @fam + N'%' THEN N'FAMILY' ELSE N'FRIEND' END AS Kind,
           CASE WHEN o.Memo LIKE @fam + N'%' THEN SUBSTRING(o.Memo, LEN(@fam) + 1, 400)
                ELSE SUBSTRING(o.Memo, LEN(@fri) + 1, 400) END AS Rest,
           o.CreatedAt
    FROM dbo.StudentPointGiftOrder o
    WHERE o.Memo LIKE @fam + N'%' OR o.Memo LIKE @fri + N'%'
),
p AS (
    SELECT Kind, Rest, CreatedAt,
           CASE WHEN CHARINDEX(N':', Rest) > 0
                 AND LEN(Rest) - LEN(REVERSE(LEFT(REVERSE(Rest), CHARINDEX(N':', REVERSE(Rest)) - 1))) > 0
                 AND REVERSE(LEFT(REVERSE(Rest), CHARINDEX(N':', REVERSE(Rest)) - 1)) NOT LIKE N'%[^0-9]%'
                 AND LEN(REVERSE(LEFT(REVERSE(Rest), CHARINDEX(N':', REVERSE(Rest)) - 1))) > 0
                THEN 1 ELSE 0 END AS HasStep
    FROM src
),
q AS (
    SELECT Kind, CreatedAt,
           CASE WHEN HasStep = 1 THEN LEFT(Rest, LEN(Rest) - CHARINDEX(N':', REVERSE(Rest))) ELSE Rest END AS AwardKey,
           CASE WHEN HasStep = 1 THEN CAST(REVERSE(LEFT(REVERSE(Rest), CHARINDEX(N':', REVERSE(Rest)) - 1)) AS INT) ELSE 1000 END AS Threshold
    FROM p
)
INSERT INTO dbo.DM2_TeamAward (Kind, AwardKey, Threshold, AwardedAt)
SELECT q.Kind, LTRIM(RTRIM(q.AwardKey)), q.Threshold, MIN(q.CreatedAt)
FROM q
WHERE LTRIM(RTRIM(q.AwardKey)) <> N''
  AND NOT EXISTS (SELECT 1 FROM dbo.DM2_TeamAward a WHERE a.Kind = q.Kind AND a.AwardKey = LTRIM(RTRIM(q.AwardKey)) AND a.Threshold = q.Threshold)
GROUP BY q.Kind, LTRIM(RTRIM(q.AwardKey)), q.Threshold;
GO

SELECT Kind, AwardKey, Threshold, AwardedAt FROM dbo.DM2_TeamAward ORDER BY AwardedAt;
GO

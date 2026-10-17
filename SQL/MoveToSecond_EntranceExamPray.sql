-- ============================================================
--  수시·수능 준비생을 위한 기도문 — 「학교와 학과를 정하며」를 내일 자리로
--
--  기준일(2026-09-06)이 순환 1번이므로 내일은 2번입니다.
--  이 기도문을 2번 자리로 옮기고, 뒤의 기도문들은 한 칸씩 밀립니다.
--
--  · SortOrder 만 다시 매깁니다. 순환 차례(RotationNo)는 SortOrder 로 정해집니다.
--  · 순환에 드는 기도문은 1,2,3… 으로, 그 밖(그날에 읽는 것·숨긴 것)은 101 부터.
--    → 목록 왼쪽 번호가 빈 자리 없이 이어집니다.
--  · Title / PrayText / CreatedAt / UpdatedAt 은 건드리지 않습니다.
--    (검증완료 순서가 흐트러지지 않도록)
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EntranceExamPray_bak_v5' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.EntranceExamPray_bak_v5 FROM dbo.EntranceExamPray;
    PRINT '[EntranceExamPray_bak_v5] 백업 생성';
END
ELSE
    PRINT '[EntranceExamPray_bak_v5] 백업이 이미 있습니다 — 건너뜀';

DECLARE @targetId INT = (
    SELECT TOP 1 PrayId
      FROM dbo.EntranceExamPray
     WHERE Title = N'학교와 학과를 정하며'
       AND ISNULL(UseYN,'Y') = 'Y' AND ISNULL(RotateYN,'Y') = 'Y'
     ORDER BY SortOrder, PrayId);

IF @targetId IS NULL
BEGIN
    RAISERROR(N'「학교와 학과를 정하며」를 순환 목록에서 찾지 못했습니다.', 16, 1);
    RETURN;
END

PRINT N'대상 PrayId = ' + CAST(@targetId AS NVARCHAR(10));

-- ── 순환에 드는 기도문 : 1,2,3… (대상은 2번 자리로) ──
;WITH rot AS (
    SELECT PrayId,
           ROW_NUMBER() OVER (ORDER BY SortOrder ASC, PrayId ASC) AS rn
      FROM dbo.EntranceExamPray
     WHERE ISNULL(UseYN,'Y') = 'Y' AND ISNULL(RotateYN,'Y') = 'Y'
),
placed AS (
    -- 1.5 를 주면 1번과 2번 사이로 끼어듭니다
    SELECT PrayId,
           CASE WHEN PrayId = @targetId THEN CAST(1.5 AS DECIMAL(10,2))
                ELSE CAST(rn AS DECIMAL(10,2)) END AS ord
      FROM rot
),
renum AS (
    SELECT PrayId, ROW_NUMBER() OVER (ORDER BY ord ASC) AS newSort FROM placed
)
UPDATE p
   SET p.SortOrder = r.newSort
  FROM dbo.EntranceExamPray p
  JOIN renum r ON r.PrayId = p.PrayId;

PRINT N'순환 기도문 번호 다시 매김 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';

-- ── 순환 밖(그날에 읽는 것 · 숨긴 것) : 101 부터 ──
;WITH rest AS (
    SELECT PrayId,
           100 + ROW_NUMBER() OVER (ORDER BY SortOrder ASC, PrayId ASC) AS newSort
      FROM dbo.EntranceExamPray
     WHERE NOT (ISNULL(UseYN,'Y') = 'Y' AND ISNULL(RotateYN,'Y') = 'Y')
)
UPDATE p
   SET p.SortOrder = r.newSort
  FROM dbo.EntranceExamPray p
  JOIN rest r ON r.PrayId = p.PrayId;

PRINT N'순환 밖 기도문 번호 다시 매김 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';
GO


-- ────────────────────────────────────────────────────────────
--  확인 — 오늘과 내일 어느 기도문이 나오는지 실제로 계산해 봅니다
--  (앱의 ManageEntranceExamPray.BaseDate 와 같은 식)
-- ────────────────────────────────────────────────────────────
DECLARE @base DATE = '2026-09-06';   -- BaseDate
DECLARE @cnt  INT  = (SELECT COUNT(*) FROM dbo.EntranceExamPray
                       WHERE ISNULL(UseYN,'Y')='Y' AND ISNULL(RotateYN,'Y')='Y');

SELECT N'오늘'   AS 날, CAST(DATEADD(HOUR,16,GETDATE()) AS DATE) AS 날짜 INTO #d
UNION ALL SELECT N'내일', DATEADD(DAY,1, CAST(DATEADD(HOUR,16,GETDATE()) AS DATE))
UNION ALL SELECT N'모레', DATEADD(DAY,2, CAST(DATEADD(HOUR,16,GETDATE()) AS DATE));

SELECT d.날, d.날짜,
       ((DATEDIFF(DAY, @base, d.날짜) % @cnt) + @cnt) % @cnt + 1 AS 차례,
       t.Title
  FROM #d d
  CROSS APPLY (
      SELECT Title FROM (
          SELECT Title, ROW_NUMBER() OVER (ORDER BY SortOrder ASC, PrayId ASC) AS rn
            FROM dbo.EntranceExamPray
           WHERE ISNULL(UseYN,'Y')='Y' AND ISNULL(RotateYN,'Y')='Y'
      ) x WHERE x.rn = ((DATEDIFF(DAY, @base, d.날짜) % @cnt) + @cnt) % @cnt + 1
  ) t
 ORDER BY d.날짜;

DROP TABLE #d;

-- 앞쪽 다섯 편
SELECT TOP 5 SortOrder, Title
  FROM dbo.EntranceExamPray
 WHERE ISNULL(UseYN,'Y')='Y' AND ISNULL(RotateYN,'Y')='Y'
 ORDER BY SortOrder;

-- 번호가 1부터 빈 자리 없이 이어지는지
SELECT MIN(SortOrder) AS 최소, MAX(SortOrder) AS 최대, COUNT(*) AS 건수
  FROM dbo.EntranceExamPray
 WHERE ISNULL(UseYN,'Y')='Y' AND ISNULL(RotateYN,'Y')='Y';
GO

-- ============================================================
--  치유와 회복을 위한 기도문 — 본문 안에 제목이 그대로 다시 나오는 것 제거
--
--  화면 상단(제목)과 본문에 같은 문장이 두 번 나오는 행이 있어 본문 쪽을 지웁니다.
--  · 대상 : PrayText 안에 Title 이 그대로 들어 있는 행 (전수 검사 결과 1건)
--  · 방법 : "제목." + 뒤따르는 공백 을 지웁니다. 문단 구조는 건드리지 않습니다.
--  · UPDATE 만 씁니다. PrayId 는 바뀌지 않습니다.
-- ============================================================
SET NOCOUNT ON;

-- 되돌릴 수 있게 백업 (이미 있으면 건너뜀)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HealingAndRestorationPray_bak_v4' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.HealingAndRestorationPray_bak_v4 FROM dbo.HealingAndRestorationPray;
    PRINT '[HealingAndRestorationPray_bak_v4] 백업 생성';
END
ELSE
    PRINT '[HealingAndRestorationPray_bak_v4] 백업이 이미 있습니다 — 건너뜀';

SELECT PrayId, Title, LEN(PrayText) AS len_before
  INTO #before
  FROM dbo.HealingAndRestorationPray
 WHERE PrayText LIKE N'%' + Title + N'%';

-- "제목. " → 삭제  (마침표와 뒤 공백까지 함께 지워 문장이 자연스럽게 이어지게)
UPDATE dbo.HealingAndRestorationPray
   SET PrayText  = REPLACE(PrayText, Title + N'. ', N''),
       UpdatedAt = DATEADD(HOUR, 16, GETDATE())
 WHERE PrayText LIKE N'%' + Title + N'. %';

PRINT N'본문 속 제목 제거 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';

SELECT b.PrayId, b.Title, b.len_before, LEN(p.PrayText) AS len_after,
       LEFT(REPLACE(REPLACE(p.PrayText, CHAR(13), ''), CHAR(10), N' / '), 90) AS head_now
  FROM #before b
  JOIN dbo.HealingAndRestorationPray p ON p.PrayId = b.PrayId
 ORDER BY b.PrayId;

DROP TABLE #before;

-- 남은 게 있는지 확인 (0 이어야 정상)
SELECT COUNT(*) AS still_duplicated
  FROM dbo.HealingAndRestorationPray
 WHERE PrayText LIKE N'%' + Title + N'%';
GO

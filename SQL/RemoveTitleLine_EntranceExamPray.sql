-- ============================================================
--  수시·수능 준비생을 위한 기도문 — 본문 맨 앞의 "기도 제목: …" 줄 제거
--
--  화면 상단(제목)과 본문 첫 줄에 같은 제목이 두 번 나와서 지웁니다.
--  · 대상 : PrayText 가 '**기도 제목:' 으로 시작하는 행만
--  · 방법 : 첫 빈 줄(CRLF CRLF)까지를 잘라 냅니다. 제목 줄 하나만 사라지고
--           그 아래 기도문은 그대로 남습니다.
--  · UPDATE 만 씁니다. PrayId 는 바뀌지 않습니다.
-- ============================================================
SET NOCOUNT ON;

-- 되돌릴 수 있게 백업 (이미 있으면 건너뜀)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EntranceExamPray_bak_v1' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.EntranceExamPray_bak_v1 FROM dbo.EntranceExamPray;
    PRINT '[EntranceExamPray_bak_v1] 백업 생성';
END
ELSE
    PRINT '[EntranceExamPray_bak_v1] 백업이 이미 있습니다 — 건너뜀';

DECLARE @sep NVARCHAR(4) = CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10);   -- 빈 줄

SELECT PrayId, Title, LEN(PrayText) AS len_before
  INTO #before
  FROM dbo.EntranceExamPray
 WHERE PrayText LIKE N'**기도 제목:%'
   AND CHARINDEX(@sep, PrayText) > 0;

UPDATE dbo.EntranceExamPray
   SET PrayText  = LTRIM(SUBSTRING(PrayText,
                        CHARINDEX(@sep, PrayText) + LEN(@sep),
                        LEN(PrayText))),
       UpdatedAt = DATEADD(HOUR, 16, GETDATE())
 WHERE PrayText LIKE N'**기도 제목:%'
   AND CHARINDEX(@sep, PrayText) > 0;

PRINT '제목 줄 제거 완료 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + '건';

SELECT b.PrayId, b.Title, b.len_before, LEN(p.PrayText) AS len_after,
       LEFT(p.PrayText, 40) AS head_now
  FROM #before b
  JOIN dbo.EntranceExamPray p ON p.PrayId = b.PrayId
 ORDER BY b.PrayId;

DROP TABLE #before;

-- 남은 게 있는지 확인 (0 이어야 정상)
SELECT SUM(CASE WHEN PrayText LIKE N'%기도 제목:%' THEN 1 ELSE 0 END) AS still_has_label
  FROM dbo.EntranceExamPray;
GO

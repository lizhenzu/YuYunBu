-- ============================================================
--  YunYunBu2025 — HealingAndRestorationPray : PrayId = 0 인 행만 복구
--
--  [증상] 그 기도문이 보이는 날에는 "이 기도문 펼쳐 보기" 링크가
--         PrayId=0 으로 만들어져 상세 화면이 열리지 않습니다.
--
--  [원인] IDENTITY 는 정상인데, 어떤 이유로 한 행이 PrayId 0 으로 들어갔습니다.
--         (IDENTITY 컬럼은 UPDATE 로 값을 바꿀 수 없습니다)
--
--  [조치] 해당 행을 그대로 복사해 새 PrayId 로 넣고 원본(0)을 지웁니다.
--         · 내용 · 조회수 · 순서 · 작성일 모두 그대로 보존
--         · 나머지 행의 PrayId 는 건드리지 않습니다
--
--  * PrayId=0 인 행이 없으면 아무것도 하지 않습니다 (여러 번 실행해도 안전).
--  실행 전: USE [YourDatabaseName] 확인
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='HealingAndRestorationPray' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
    PRINT '[HealingAndRestorationPray] 테이블이 없습니다.';
    RETURN;
END

DECLARE @zero INT = (SELECT COUNT(*) FROM dbo.HealingAndRestorationPray WHERE PrayId = 0);

IF @zero = 0
BEGIN
    PRINT '[HealingAndRestorationPray] PrayId=0 인 행이 없습니다 — 조치 불필요';
    RETURN;
END

PRINT CONCAT('[HealingAndRestorationPray] PrayId=0 인 행 ', @zero, '건을 복구합니다…');

-- 복구 전 모습 기록
SELECT PrayId AS [복구전_PrayId], SortOrder, Title, ClickCount
FROM   dbo.HealingAndRestorationPray WHERE PrayId = 0;

BEGIN TRAN;

    DECLARE @moved TABLE (NewPrayId INT, SortOrder INT, Title NVARCHAR(200));

    -- 1) 같은 내용을 새 PrayId 로 다시 넣기 (조회수·작성일 그대로)
    INSERT INTO dbo.HealingAndRestorationPray
        (SortOrder, Title, PrayText, BibleRef, BibleText, ClickCount, UseYN, CreatedAt, UpdatedAt)
    OUTPUT inserted.PrayId, inserted.SortOrder, inserted.Title INTO @moved
    SELECT SortOrder, Title, PrayText, BibleRef, BibleText,
           ISNULL(ClickCount,0), ISNULL(UseYN,'Y'),
           ISNULL(CreatedAt, GETDATE()), UpdatedAt
    FROM   dbo.HealingAndRestorationPray
    WHERE  PrayId = 0;

    DECLARE @ins INT = @@ROWCOUNT;

    IF @ins <> @zero
    BEGIN
        RAISERROR('복사된 건수(%d)가 대상(%d)과 다릅니다. 롤백합니다.', 16, 1, @ins, @zero);
        ROLLBACK TRAN;
        RETURN;
    END

    -- 2) 원본(PrayId=0) 삭제
    DELETE FROM dbo.HealingAndRestorationPray WHERE PrayId = 0;

COMMIT TRAN;

PRINT '[HealingAndRestorationPray] 복구 완료';
SELECT NewPrayId AS [복구후_PrayId], SortOrder, Title FROM @moved;
GO

-- ── 결과 확인 ────────────────────────────────────────────────
SELECT COUNT(*)                AS [총건수],
       SUM(CASE WHEN PrayId=0 THEN 1 ELSE 0 END) AS [PrayId0],
       COUNT(DISTINCT PrayId)  AS [PrayId고유],
       MIN(PrayId)             AS [최소],
       MAX(PrayId)             AS [최대]
FROM   dbo.HealingAndRestorationPray;

SELECT TOP 5 PrayId, SortOrder, Title, ClickCount
FROM   dbo.HealingAndRestorationPray
ORDER  BY SortOrder, PrayId;
GO

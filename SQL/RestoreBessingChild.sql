-- ============================================================
--  자녀 축복 기도문 되돌리기 (BessingChild)
--
--  ⚠️ 테이블 이름을 바꾸는 방식(rename)은 쓰지 마세요.
--     · dbo.BessingChild 가 이미 있어 같은 이름으로 rename 이 안 됩니다
--     · 백업본에는 기본값(DEFAULT) 제약 5개가 없습니다
--     · 백업 이후 늘어난 조회수(visited)가 사라집니다
--
--  아래처럼 "글 내용만" 되돌리면 표 구조와 조회수는 그대로 유지됩니다.
--  실행 전: USE [db_a64b16_youyunbu] 확인
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('dbo.BessingChild_bak_v1') IS NULL
BEGIN
    PRINT '백업 테이블(dbo.BessingChild_bak_v1)이 없습니다.';
    RETURN;
END

BEGIN TRAN;

UPDATE p
   SET p.PrayTitle = b.PrayTitle,
       p.PrayText  = b.PrayText,
       p.HolyWord  = b.HolyWord,
       p.updateDT  = DATEADD(HOUR, 16, GETDATE())
  FROM dbo.BessingChild             AS p
  JOIN dbo.BessingChild_bak_v1      AS b ON p.Id = b.Id;

DECLARE @n INT = @@ROWCOUNT;

COMMIT TRAN;

PRINT CONCAT('[BessingChild] ', @n, '건 원문으로 되돌렸습니다. (조회수·기본값은 그대로)');

-- 확인
SELECT COUNT(*)                                            AS 총건수,
       SUM(CASE WHEN p.PrayText = b.PrayText THEN 1 ELSE 0 END) AS 원문과동일,
       SUM(p.visited)                                      AS 조회수합
FROM   dbo.BessingChild p
JOIN   dbo.BessingChild_bak_v1 b ON p.Id = b.Id;

-- ============================================================
--  BessingChild 개행 복구 : LF 단독 -> CRLF
--  (뷰가 \r\n 기준으로 줄바꿈을 렌더링함)
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

UPDATE dbo.BessingChild
SET PrayText = REPLACE(REPLACE(REPLACE(PrayText, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10))
WHERE PrayText LIKE '%' + CHAR(10) + '%';

COMMIT TRAN;
PRINT '[BessingChild] 개행 CRLF 복구 완료';
GO

-- BessingHusband 개행 정규화 : LF 단독 -> CRLF
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

UPDATE dbo.BessingHusband
SET PrayText = REPLACE(REPLACE(REPLACE(PrayText, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10))
WHERE PrayText LIKE '%' + CHAR(10) + '%';

UPDATE dbo.BessingHusband
SET PrayHolyWord = REPLACE(REPLACE(REPLACE(PrayHolyWord, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10))
WHERE PrayHolyWord LIKE '%' + CHAR(10) + '%';

COMMIT TRAN;
PRINT '[BessingHusband] 개행 CRLF 정규화 완료';
GO

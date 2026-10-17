-- BessingWife 개행 정규화 : LF 단독 -> CRLF (PrayText, PrayHolyWord)
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

UPDATE dbo.BessingWife
SET PrayText = REPLACE(REPLACE(REPLACE(PrayText, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10))
WHERE PrayText LIKE '%' + CHAR(10) + '%';

UPDATE dbo.BessingWife
SET PrayHolyWord = REPLACE(REPLACE(REPLACE(PrayHolyWord, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10))
WHERE PrayHolyWord LIKE '%' + CHAR(10) + '%';

COMMIT TRAN;
PRINT '[BessingWife] 개행 CRLF 정규화 완료';
GO

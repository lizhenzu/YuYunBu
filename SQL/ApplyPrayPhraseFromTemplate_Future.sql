-- ============================================================
--  템플릿의 새 기도문을 PrayPhrase 로 반영 (오늘 이후 날짜만)
--
--  ※ sp_InsertPrayPhrase 는 DELETE + INSERT 이므로 쓰지 않는다.
--    Id 가 바뀌면 안 되므로 UPDATE ... FROM JOIN 으로만 처리한다.
--    ExposeDate / Id / InsertDT 는 건드리지 않는다.
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

-- 1) 개행을 CRLF 로 정규화 (템플릿)
UPDATE dbo.PrayPhraseTemplate
SET PrayContent1 = REPLACE(REPLACE(REPLACE(PrayContent1, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10))
WHERE PrayContent1 LIKE '%' + CHAR(10) + '%';

UPDATE dbo.PrayPhraseTemplate
SET PrayContent2 = REPLACE(REPLACE(REPLACE(PrayContent2, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10))
WHERE PrayContent2 LIKE '%' + CHAR(10) + '%';

-- 2) 오늘(KST) 이후 PrayPhrase 를 템플릿 내용으로 갱신
DECLARE @todayKst DATE = CAST(DATEADD(HOUR,16,GETDATE()) AS DATE);

UPDATE p
SET p.PrayTitle    = t.PrayTitle,
    p.PrayContent1 = t.PrayContent1,
    p.PrayContent2 = t.PrayContent2,
    p.updateDT     = DATEADD(HOUR,16,GETDATE())
FROM dbo.PrayPhrase p
JOIN dbo.PrayPhraseTemplate t
  ON t.DayNo = DAY(p.ExposeDate)
WHERE CAST(p.ExposeDate AS DATE) >= @todayKst;

PRINT '[PrayPhrase] 갱신 건수 (오늘 이후) : ' + CAST(@@ROWCOUNT AS VARCHAR(10));

COMMIT TRAN;
GO

-- ============================================================
--  남편을 위한 아내의 기도(BessingHusband) — 교정
--
--  40편을 정독하여 찾은 비문 · 어색한 구문 · 제목 불일치 · 반복 표현을 고칩니다.
--
--  ⚠️ PrayTitle 과 PrayText 만 수정합니다.
--     PrayHolyWord · UpdateDT · InsertDT · visited · PageNo 는
--     한 곳도 건드리지 않습니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BessingHusband_bak_v2' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.BessingHusband_bak_v2 FROM dbo.BessingHusband;
    PRINT '[BessingHusband_bak_v2] 백업 생성';
END
ELSE
    PRINT '[BessingHusband_bak_v2] 백업이 이미 있습니다 — 건너뜀';


-- ────────────────────────────────────────────────────────────
-- 1) 37번 — 주님께 "강요하지 마시고"라고 구하는 문장입니다.
--    뜻은 "억지로 잊으려 애쓰지 않게 해 달라" 는 것입니다.
--    (아내 기도문 14번에도 같은 문장이 있어 함께 고쳤습니다)
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingHusband
   SET PrayText = REPLACE(PrayText,
        N'잊으라 강요하지 마시고, 다만 그 기억이',
        N'억지로 잊으려 애쓰지 않게 하시고, 다만 그 기억이')
 WHERE Id = 37;
PRINT N'1) 37번 문맥 교정';


-- ────────────────────────────────────────────────────────────
-- 2) 28번 — "듣게 하지 마시고"는 주님께 명령하는 어투가 됩니다
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingHusband
   SET PrayText = REPLACE(PrayText,
        N'다투는 소리를 아이들이 듣게 하지 마시고, 화해하는 모습까지',
        N'다투는 소리를 아이들이 듣지 않게 하시고, 화해하는 모습까지')
 WHERE Id = 28;
PRINT N'2) 28번 어투 교정';


-- ────────────────────────────────────────────────────────────
-- 3) 11번 제목 — 40편 가운데 유일하게 「…주님께」로 끝나 결이 다릅니다.
--    나머지는 모두 「…하소서 / …주소서 / …위한 기도」 입니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingHusband
   SET PrayTitle = N'남편의 외로움을 채워 주소서'
 WHERE Id = 11 AND PrayTitle = N'남편의 외로움을 아시는 주님께';
PRINT N'3) 11번 제목 교정 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';


-- ────────────────────────────────────────────────────────────
-- 4) 13번 — "칭찬에 오르내리다"는 말이 되지 않습니다.
--    (오르내리는 것은 칭찬이 아니라 마음입니다)
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingHusband
   SET PrayText = REPLACE(PrayText,
        N'사람의 칭찬에 오르내리지 않게 하시고',
        N'사람의 칭찬과 비난에 마음이 오르내리지 않게 하시고')
 WHERE Id = 13;
PRINT N'4) 13번 비문 교정';


-- ────────────────────────────────────────────────────────────
-- 5) 39번 제목이 본문과 어긋납니다.
--    제목은 「끝까지 함께 걷는 부부」인데 본문은 남편의 노년으로 시작해
--    부부의 노년으로 이어집니다. 둘을 함께 담는 제목으로 바꿉니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingHusband
   SET PrayTitle = N'함께 늙어 가는 길을 지켜 주소서'
 WHERE Id = 39 AND PrayTitle = N'끝까지 함께 걷는 부부가 되게 하소서';
PRINT N'5) 39번 제목 교정 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';


-- ────────────────────────────────────────────────────────────
-- 6) 34번 — 1번과 "쉬는 것이 게으름이 아니라"가 그대로 겹칩니다
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingHusband
   SET PrayText = REPLACE(PrayText,
        N'쉬는 것이 게으름이 아니라 다음을 위한 준비임을 알게 하옵소서.',
        N'쉼이 다음 걸음을 위한 준비임을 알게 하옵소서.')
 WHERE Id = 34;
PRINT N'6) 34번 반복 표현 정리';


-- ────────────────────────────────────────────────────────────
-- 7) 30번 — "다들 그렇게 한다는 말"이 20·36번에도 있어 세 편에서 겹칩니다.
--    20번(분별)과 36번(정직)에서는 핵심이라 그대로 두고 30번만 바꿉니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingHusband
   SET PrayText = REPLACE(PrayText,
        N'다들 그렇게 한다는 말에 흔들리지 않게 하시고, 남편의 성실함을 통해',
        N'관행이라는 말로 양심을 덮지 않게 하시고, 남편의 성실함을 통해')
 WHERE Id = 30;
PRINT N'7) 30번 반복 표현 정리';
GO


-- ────────────────────────────────────────────────────────────
-- 8) 본문 앞 군더더기 공백·줄바꿈 제거 + 줄바꿈 CRLF 통일
-- ────────────────────────────────────────────────────────────
WHILE EXISTS (SELECT 1 FROM dbo.BessingHusband
               WHERE LEFT(PrayText,1) IN (N' ', CHAR(9), CHAR(13), CHAR(10)))
BEGIN
    UPDATE dbo.BessingHusband
       SET PrayText = SUBSTRING(PrayText, 2, LEN(PrayText))
     WHERE LEFT(PrayText,1) IN (N' ', CHAR(9), CHAR(13), CHAR(10));
END

UPDATE dbo.BessingHusband
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10));

UPDATE dbo.BessingHusband SET PrayTitle = LTRIM(RTRIM(PrayTitle))
 WHERE PrayTitle <> LTRIM(RTRIM(PrayTitle));
PRINT N'8) 앞 공백 제거 · 줄바꿈 통일';
GO


-- ────────────────────────────────────────────────────────────
--  확인
-- ────────────────────────────────────────────────────────────
SELECT p.Id, p.PrayTitle
  FROM dbo.BessingHusband p JOIN dbo.BessingHusband_bak_v2 b ON b.Id = p.Id
 WHERE ISNULL(p.PrayText,N'') <> ISNULL(b.PrayText,N'') OR ISNULL(p.PrayTitle,N'') <> ISNULL(b.PrayTitle,N'')
 ORDER BY p.Id;

-- 다른 필드가 그대로인지 (0 이어야 정상)
SELECT COUNT(*) AS 다른필드가_바뀐_행
  FROM dbo.BessingHusband p JOIN dbo.BessingHusband_bak_v2 b ON b.Id = p.Id
 WHERE ISNULL(p.PrayHolyWord,N'') <> ISNULL(b.PrayHolyWord,N'')
    OR p.UpdateDT <> b.UpdateDT OR p.InsertDT <> b.InsertDT
    OR p.visited  <> b.visited  OR ISNULL(p.PageNo,-1) <> ISNULL(b.PageNo,-1);

-- 남은 문제 (모두 0 이어야 정상)
SELECT
    SUM(CASE WHEN PrayText  LIKE N'%강요하지 마시고%'   THEN 1 ELSE 0 END) AS 강요하지마시고,
    SUM(CASE WHEN PrayText  LIKE N'%듣게 하지 마시고%'  THEN 1 ELSE 0 END) AS 명령어투,
    SUM(CASE WHEN PrayTitle LIKE N'%주님께'             THEN 1 ELSE 0 END) AS 제목결불일치,
    SUM(CASE WHEN PrayText  LIKE N'%칭찬에 오르내리%'   THEN 1 ELSE 0 END) AS 비문,
    SUM(CASE WHEN PrayText  LIKE N'%쉬는 것이 게으름이 아니라%' THEN 1 ELSE 0 END) AS 쉼표현반복,
    SUM(CASE WHEN PrayText  LIKE N'%다들 그렇게 한다는 말%'     THEN 1 ELSE 0 END) AS 다들그렇게,
    SUM(CASE WHEN LEFT(PrayText,1) IN (N' ',CHAR(9),CHAR(13),CHAR(10)) THEN 1 ELSE 0 END) AS 앞공백
  FROM dbo.BessingHusband;

-- 제목·첫 줄이 겹치는지 (0 이어야 정상)
SELECT PrayTitle, COUNT(*) AS cnt FROM dbo.BessingHusband
 GROUP BY PrayTitle HAVING COUNT(*) > 1;

SELECT LEFT(PrayText, CHARINDEX(CHAR(13), PrayText + CHAR(13)) - 1) AS 첫줄, COUNT(*) AS cnt
  FROM dbo.BessingHusband
 GROUP BY LEFT(PrayText, CHARINDEX(CHAR(13), PrayText + CHAR(13)) - 1) HAVING COUNT(*) > 1;

-- PageNo 겹침 (참고용 — 손대지 않습니다)
SELECT PageNo, COUNT(*) AS cnt FROM dbo.BessingHusband GROUP BY PageNo HAVING COUNT(*) > 1;
GO

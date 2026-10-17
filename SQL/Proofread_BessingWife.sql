-- ============================================================
--  아내를 위한 남편의 기도(BessingWife) — 교정
--
--  60편을 정독하여 찾은 비문 · 어색한 구문 · 문맥 어긋남을 고칩니다.
--
--  ⚠️ PrayTitle 과 PrayText 만 수정합니다.
--     PrayHolyWord · UpdateDT · InsertDT · visited · PageNo 는
--     한 곳도 건드리지 않습니다.
--  ⚠️ REPLACE 로 해당 부분만 바꾸므로 이미 고쳐진 행은 그대로 지나갑니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BessingWife_bak_v2' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.BessingWife_bak_v2 FROM dbo.BessingWife;
    PRINT '[BessingWife_bak_v2] 백업 생성';
END
ELSE
    PRINT '[BessingWife_bak_v2] 백업이 이미 있습니다 — 건너뜀';


-- ────────────────────────────────────────────────────────────
-- 1) 13번 제목이 비문입니다
--    「…자유롭게 하시는 기도」는 주님께서 드리는 기도처럼 읽힙니다.
--    나머지 59편처럼 「…하소서」 로 맞춥니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingWife
   SET PrayTitle = N'비교하는 마음에서 자유롭게 하소서'
 WHERE Id = 13 AND PrayTitle = N'비교하는 마음에서 자유롭게 하시는 기도';
PRINT N'1) 13번 제목 교정 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';


-- ────────────────────────────────────────────────────────────
-- 2) 14번 — 주님께 "강요하지 마시고" 라고 구하는 문장이 되어 있습니다.
--    뜻은 "억지로 잊으려 애쓰지 않게 해 달라" 는 것입니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingWife
   SET PrayText = REPLACE(PrayText,
        N'잊으라 강요하지 마시고, 다만 그 기억이',
        N'억지로 잊으려 애쓰지 않게 하시고, 다만 그 기억이')
 WHERE Id = 14;
PRINT N'2) 14번 문맥 교정';


-- ────────────────────────────────────────────────────────────
-- 3) 31번 — 부정이 겹쳐 뜻이 흐립니다
--    "듣게 하지 않게 하옵소서" → "듣지 않게 하옵소서"
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingWife
   SET PrayText = REPLACE(PrayText,
        N'다투는 소리를 아이들이 듣게 하지 않게 하옵소서.',
        N'다투는 소리를 아이들이 듣지 않게 하옵소서.')
 WHERE Id = 31;
PRINT N'3) 31번 겹친 부정 정리';


-- ────────────────────────────────────────────────────────────
-- 4) 39번 — "진짜"는 구어체라 기도문 문체에 튑니다
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingWife
   SET PrayText = REPLACE(PrayText,
        N'주님이 그 아이의 진짜 아버지이심을',
        N'주님이 그 아이의 참 아버지이심을')
 WHERE Id = 39;
PRINT N'4) 39번 어휘 교정';


-- ────────────────────────────────────────────────────────────
-- 5) 55번 — 띄어쓰기 오류. '젊은이'는 한 낱말입니다
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingWife
   SET PrayText = REPLACE(PrayText, N'젊은 이들에게', N'젊은이들에게')
 WHERE PrayText LIKE N'%젊은 이들에게%';
PRINT N'5) 띄어쓰기 교정 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';


-- ────────────────────────────────────────────────────────────
-- 6) 60번 — 남편이 드리는 기도인데 자녀를 "아내의 자녀들"이라 부릅니다.
--    자기 자녀이기도 하므로 "저희 자녀들"이 맞습니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingWife
   SET PrayText = REPLACE(PrayText,
        N'아내의 자녀들이 일어나 아내를 감사하게 하옵소서.',
        N'저희 자녀들이 일어나 아내를 감사하게 하옵소서.')
 WHERE Id = 60;
PRINT N'6) 60번 인칭 교정';
GO


-- ────────────────────────────────────────────────────────────
-- 7) 본문 앞 군더더기 공백·줄바꿈 제거 + 줄바꿈 CRLF 통일
-- ────────────────────────────────────────────────────────────
WHILE EXISTS (SELECT 1 FROM dbo.BessingWife
               WHERE LEFT(PrayText,1) IN (N' ', CHAR(9), CHAR(13), CHAR(10)))
BEGIN
    UPDATE dbo.BessingWife
       SET PrayText = SUBSTRING(PrayText, 2, LEN(PrayText))
     WHERE LEFT(PrayText,1) IN (N' ', CHAR(9), CHAR(13), CHAR(10));
END

UPDATE dbo.BessingWife
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10));

UPDATE dbo.BessingWife SET PrayTitle = LTRIM(RTRIM(PrayTitle))
 WHERE PrayTitle <> LTRIM(RTRIM(PrayTitle));
PRINT N'7) 앞 공백 제거 · 줄바꿈 통일';
GO


-- ────────────────────────────────────────────────────────────
--  확인
-- ────────────────────────────────────────────────────────────
SELECT p.Id, p.PrayTitle
  FROM dbo.BessingWife p JOIN dbo.BessingWife_bak_v2 b ON b.Id = p.Id
 WHERE ISNULL(p.PrayText,N'') <> ISNULL(b.PrayText,N'') OR ISNULL(p.PrayTitle,N'') <> ISNULL(b.PrayTitle,N'')
 ORDER BY p.Id;

-- 다른 필드가 그대로인지 (0 이어야 정상)
SELECT COUNT(*) AS 다른필드가_바뀐_행
  FROM dbo.BessingWife p JOIN dbo.BessingWife_bak_v2 b ON b.Id = p.Id
 WHERE ISNULL(p.PrayHolyWord,N'') <> ISNULL(b.PrayHolyWord,N'')
    OR p.UpdateDT <> b.UpdateDT OR p.InsertDT <> b.InsertDT
    OR p.visited  <> b.visited  OR ISNULL(p.PageNo,-1) <> ISNULL(b.PageNo,-1);

-- 남은 문제 (모두 0 이어야 정상)
SELECT
    SUM(CASE WHEN PrayTitle LIKE N'%하시는 기도%'        THEN 1 ELSE 0 END) AS 제목비문,
    SUM(CASE WHEN PrayText  LIKE N'%강요하지 마시고%'     THEN 1 ELSE 0 END) AS 강요하지마시고,
    SUM(CASE WHEN PrayText  LIKE N'%듣게 하지 않게%'      THEN 1 ELSE 0 END) AS 겹친부정,
    SUM(CASE WHEN PrayText  LIKE N'%진짜 아버지%'         THEN 1 ELSE 0 END) AS 구어체,
    SUM(CASE WHEN PrayText  LIKE N'%젊은 이들%'           THEN 1 ELSE 0 END) AS 띄어쓰기,
    SUM(CASE WHEN PrayText  LIKE N'%아내의 자녀들%'       THEN 1 ELSE 0 END) AS 인칭,
    SUM(CASE WHEN LEFT(PrayText,1) IN (N' ',CHAR(9),CHAR(13),CHAR(10)) THEN 1 ELSE 0 END) AS 앞공백
  FROM dbo.BessingWife;

-- 제목이 겹치는지 (0 이어야 정상)
SELECT PrayTitle, COUNT(*) AS cnt FROM dbo.BessingWife
 GROUP BY PrayTitle HAVING COUNT(*) > 1;

-- 첫 줄(부르는 말)이 겹치는지 — 같은 호칭이 여러 편에 쓰였는지 확인
SELECT LEFT(PrayText, CHARINDEX(CHAR(13), PrayText + CHAR(13)) - 1) AS 첫줄,
       COUNT(*) AS cnt
  FROM dbo.BessingWife
 GROUP BY LEFT(PrayText, CHARINDEX(CHAR(13), PrayText + CHAR(13)) - 1)
HAVING COUNT(*) > 1;

-- PageNo 가 겹치는지 (참고용 — 이 스크립트는 손대지 않습니다)
SELECT PageNo, COUNT(*) AS cnt, MIN(Id) AS id1, MAX(Id) AS id2
  FROM dbo.BessingWife GROUP BY PageNo HAVING COUNT(*) > 1;
GO

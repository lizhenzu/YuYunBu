-- ============================================================
--  치유와 회복을 위한 기도문 — 교정
--
--  31편을 정독하여 찾은 오타 · 비문 · 문맥 어긋남 · 중복 문장을 고칩니다.
--
--  ⚠️ Title 과 PrayText 만 수정합니다.
--     CreatedAt · UpdatedAt · BibleRef · BibleText · UseYN 등 다른 필드는
--     한 곳도 건드리지 않습니다.
--  ⚠️ REPLACE 로 해당 부분만 바꾸므로 이미 고쳐진 행은 그대로 지나갑니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HealingAndRestorationPray_bak_v5' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.HealingAndRestorationPray_bak_v5 FROM dbo.HealingAndRestorationPray;
    PRINT '[HealingAndRestorationPray_bak_v5] 백업 생성';
END
ELSE
    PRINT '[HealingAndRestorationPray_bak_v5] 백업이 이미 있습니다 — 건너뜀';


-- ────────────────────────────────────────────────────────────
-- 1) 2번 「고치실 수 있는 분은 주님뿐입니다」 — 인용 뒤 조사가 빠진 비문
--    …하실 수 있나이다" 고백하였을 때  →  …하실 수 있나이다" 하고 고백하였을 때
-- ────────────────────────────────────────────────────────────
UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'하실 수 있나이다" 고백하였을 때',
        N'하실 수 있나이다" 하고 고백하였을 때')
 WHERE SortOrder = 2;

PRINT N'1) 2번 인용 조사 보완';


-- ────────────────────────────────────────────────────────────
-- 2) 10번 「온전한 회복과 감사를 구합니다」 — "지침"은 指針으로 오해하기 쉽습니다
-- ────────────────────────────────────────────────────────────
UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'두려움과 지침까지 씻어 주옵소서',
        N'두려움과 지친 마음까지 씻어 주옵소서')
 WHERE SortOrder = 10;

PRINT N'2) 10번 "지침" 표현 교정';


-- ────────────────────────────────────────────────────────────
-- 3) 16번 「낙심하지 않게 붙들어 주소서」 — 완곡 표현이 겹쳐 뜻이 흐립니다
--    "보이지 않는 듯하여" → "좀처럼 보이지 아니하여"
-- ────────────────────────────────────────────────────────────
UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'나아지는 기색이 보이지 않는 듯하여',
        N'나아지는 기색이 좀처럼 보이지 아니하여')
 WHERE SortOrder = 16;

PRINT N'3) 16번 겹친 완곡 표현 정리';


-- ────────────────────────────────────────────────────────────
-- 4) 20번 「약한 자리에서 주님을 구합니다」 — 비문
--    "돌아눕다"는 목적어를 받지 않습니다. "몸을 돌아눕는" → "돌아눕는"
-- ────────────────────────────────────────────────────────────
UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'약을 넘기는 것도, 몸을 돌아눕는 것도',
        N'약을 넘기는 것도, 돌아눕는 것도')
 WHERE SortOrder = 20;

PRINT N'4) 20번 비문 교정';


-- ────────────────────────────────────────────────────────────
-- 5) 한 기도문 안에서 "저"와 "저희"가 섞인 곳 (25, 28, 29번)
--    이 표의 기도는 곁에서 함께 드리는 기도이므로 "저희"로 맞춥니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'곁에서 지켜보는 일이 저를 더욱 지치게 합니다',
        N'곁에서 지켜보는 일이 저희를 더욱 지치게 합니다')
 WHERE SortOrder = 25;

UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'**○○(이)**를 두고 제가 붙잡고 있던',
        N'**○○(이)**를 두고 저희가 붙잡고 있던')
 WHERE SortOrder = 28;

UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'오늘 저도 그렇게 구합니다. 저의 믿음 없는 것을 도와주옵소서.',
        N'오늘 저희도 그렇게 구합니다. 저희의 믿음 없는 것을 도와주옵소서.')
 WHERE SortOrder = 29;

PRINT N'5) 25·28·29번 인칭 통일';


-- ────────────────────────────────────────────────────────────
-- 6) 30번 「끝까지 함께 걸어 주시기를 구합니다」
--    "구간"은 길·교통에서 쓰는 말이라 기도문 문체에 튑니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'지치는 구간마다 업어 주시고',
        N'지치는 자리마다 업어 주시고')
 WHERE SortOrder = 30;

PRINT N'6) 30번 어휘 교정';


-- ────────────────────────────────────────────────────────────
-- 7) 32번 「낫기 전에 먼저 감사드립니다」 — 빌립보서 4:6 인용이 끊겨 뜻이 흐립니다
-- ────────────────────────────────────────────────────────────
UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'모든 일에 기도와 간구로, 감사함으로 아뢰라 하셨사오니',
        N'모든 일에 기도와 간구로 구할 것을 감사함으로 아뢰라 하셨사오니')
 WHERE SortOrder = 32;

PRINT N'7) 32번 성경 인용 정리';


-- ────────────────────────────────────────────────────────────
-- 8) 34번 — 제목이 문장으로 맺어지지 않고, 본문 두 문단이 다른 편과 겹칩니다
--     · 제목 「소망의 하나님께」 → 다른 편과 같은 형태로
--     · "두려움이 커질수록…" 문단 : 15번과 겹침
--     · "회복의 날을 정하여 두신 줄 믿고 기다립니다" : 2번과 똑같은 문장
--    표시된 구절(시편 121:1-2)에 맞추어 새로 씁니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.HealingAndRestorationPray
   SET Title = N'소망을 두고 기다리며 구합니다'
 WHERE SortOrder = 34 AND Title = N'소망의 하나님께';

UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'기다리는 동안 **○○(이)**의 마음을 지켜 주옵소서. 두려움이 커질수록 주님께서 더욱 크신 분이심을 기억하게 하시고, 세상이 줄 수 없는 평안으로 그 마음을 채워 주옵소서.',
        N'기다리는 동안 **○○(이)**의 마음을 지켜 주옵소서. 밤이 길어도 아침은 오는 줄 알게 하시고, 오늘 하루를 견딜 소망 한 조각을 그 손에 쥐어 주옵소서.')
 WHERE SortOrder = 34;

UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(PrayText,
        N'회복의 날을 정하여 두신 줄 믿고 기다립니다. 그날에 가장 먼저 주님께 감사하게 하옵소서.',
        N'내가 산을 향하여 눈을 들리라 나의 도움이 어디서 올까 하였습니다. 저희의 도움이 천지를 지으신 주님께로부터 오는 줄 믿고, 오늘도 눈을 들어 주님을 바라봅니다.')
 WHERE SortOrder = 34;

PRINT N'8) 34번 제목·중복 문단 교정';
GO


-- ────────────────────────────────────────────────────────────
--  본문 앞 군더더기 공백·줄바꿈 제거
-- ────────────────────────────────────────────────────────────
WHILE EXISTS (SELECT 1 FROM dbo.HealingAndRestorationPray
               WHERE LEFT(PrayText,1) IN (N' ', CHAR(9), CHAR(13), CHAR(10)))
BEGIN
    UPDATE dbo.HealingAndRestorationPray
       SET PrayText = SUBSTRING(PrayText, 2, LEN(PrayText))
     WHERE LEFT(PrayText,1) IN (N' ', CHAR(9), CHAR(13), CHAR(10));
END
GO

-- 줄바꿈을 CRLF 로 통일 (화면이 \r\n 로 문단을 나눕니다)
UPDATE dbo.HealingAndRestorationPray
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10));
GO


-- ────────────────────────────────────────────────────────────
--  확인
-- ────────────────────────────────────────────────────────────
SELECT p.SortOrder, p.Title
  FROM dbo.HealingAndRestorationPray p
  JOIN dbo.HealingAndRestorationPray_bak_v5 b ON b.PrayId = p.PrayId
 WHERE p.PrayText <> b.PrayText OR p.Title <> b.Title
 ORDER BY p.SortOrder;

-- 다른 필드가 그대로인지 대조 (0 이어야 정상)
SELECT COUNT(*) AS 다른필드가_바뀐_행
  FROM dbo.HealingAndRestorationPray p
  JOIN dbo.HealingAndRestorationPray_bak_v5 b ON b.PrayId = p.PrayId
 WHERE ISNULL(p.CreatedAt,'1900-01-01')  <> ISNULL(b.CreatedAt,'1900-01-01')
    OR ISNULL(p.UpdatedAt,'1900-01-01')  <> ISNULL(b.UpdatedAt,'1900-01-01')
    OR ISNULL(p.BibleRef,N'')            <> ISNULL(b.BibleRef,N'')
    OR ISNULL(p.BibleText,N'')           <> ISNULL(b.BibleText,N'')
    OR ISNULL(p.UseYN,'Y')               <> ISNULL(b.UseYN,'Y')
    OR p.SortOrder                       <> b.SortOrder
    OR p.ClickCount                      <> b.ClickCount;

-- 남은 문제 확인 (모두 0 이어야 정상)
SELECT
    SUM(CASE WHEN PrayText LIKE N'%지침%'                THEN 1 ELSE 0 END) AS 지침,
    SUM(CASE WHEN PrayText LIKE N'%몸을 돌아눕는%'        THEN 1 ELSE 0 END) AS 비문,
    SUM(CASE WHEN PrayText LIKE N'%구간마다%'            THEN 1 ELSE 0 END) AS 구간,
    SUM(CASE WHEN PrayText LIKE N'%있나이다" 고백%'       THEN 1 ELSE 0 END) AS 조사누락,
    SUM(CASE WHEN LEFT(PrayText,1) IN (N' ',CHAR(9),CHAR(13),CHAR(10)) THEN 1 ELSE 0 END) AS 앞공백,
    SUM(CASE WHEN PrayText LIKE N'%회복의 날을 정하여 두신 줄 믿고 기다립니다%' THEN 1 ELSE 0 END) AS 중복문장
  FROM dbo.HealingAndRestorationPray;

SELECT Title, COUNT(*) AS cnt FROM dbo.HealingAndRestorationPray
 GROUP BY Title HAVING COUNT(*) > 1;
GO

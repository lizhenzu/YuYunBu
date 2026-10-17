-- ============================================================
--  한 달치 자녀 기도문(PrayPhraseTemplate) — 교정
--  그리고 그 결과를 PrayPhrase 에 반영
--
--  31편을 정독하여 찾은 것
--   1) 어색한 구문        : 11일 자녀용, 24일 부모·자녀용
--   2) 제목 결 불일치     : 31일 (다른 30편은 두세 글자 낱말)
--   3) 중복되는 기도문    : 7일(정직성) ↔ 26일(진실성)
--                           12일(분별력) ↔ 16일(거룩) ↔ 20일(용기)
--
--  ⚠️ PrayTitle · PrayContent1(부모용) · PrayContent2(자녀용) 만 수정합니다.
--     BibleTextToPray · InsertDT · updateDT 등 다른 필드는 건드리지 않습니다.
--  ⚠️ DELETE / INSERT 를 쓰지 않습니다. 전부 UPDATE 이므로 Id 는 바뀌지 않습니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PrayPhraseTemplate_bak_v3' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.PrayPhraseTemplate_bak_v3 FROM dbo.PrayPhraseTemplate;
    PRINT '[PrayPhraseTemplate_bak_v3] 백업 생성';
END
ELSE
    PRINT '[PrayPhraseTemplate_bak_v3] 백업이 이미 있습니다 — 건너뜀';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PrayPhrase_bak_v3' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.PrayPhrase_bak_v3 FROM dbo.PrayPhrase;
    PRINT '[PrayPhrase_bak_v3] 백업 생성';
END
ELSE
    PRINT '[PrayPhrase_bak_v3] 백업이 이미 있습니다 — 건너뜀';


-- ────────────────────────────────────────────────────────────
-- 1) 11일 (순종) 자녀용 — "대답부터 먼저 하게"는 뜻이 잡히지 않습니다
-- ────────────────────────────────────────────────────────────
UPDATE dbo.PrayPhraseTemplate
   SET PrayContent2 = REPLACE(PrayContent2,
        N'대답부터 먼저 하게 해 주세요.',
        N'미루지 않고 먼저 대답하게 해 주세요.')
 WHERE DayNo = 11;
PRINT N'1) 11일 자녀용 구문 교정';


-- ────────────────────────────────────────────────────────────
-- 2) 24일 (자신감) — "마음을 펴다"는 쓰이지 않는 표현입니다
-- ────────────────────────────────────────────────────────────
UPDATE dbo.PrayPhraseTemplate
   SET PrayContent1 = REPLACE(PrayContent1,
        N'못하는 것 앞에서도 마음을 펴게 하옵소서.',
        N'못하는 것 앞에서도 마음이 움츠러들지 않게 하옵소서.'),
       PrayContent2 = REPLACE(PrayContent2,
        N'못하는 것 앞에서도 마음을 펴게 해 주세요.',
        N'못하는 것 앞에서도 마음이 움츠러들지 않게 해 주세요.')
 WHERE DayNo = 24;
PRINT N'2) 24일 표현 교정';


-- ────────────────────────────────────────────────────────────
-- 3) 31일 제목 — 나머지 30편은 모두 두세 글자 낱말입니다
--    (존중 · 인내 · 순결 · 용서 …)
-- ────────────────────────────────────────────────────────────
UPDATE dbo.PrayPhraseTemplate
   SET PrayTitle = N'배움 (Teachability)'
 WHERE DayNo = 31 AND PrayTitle = N'배우고자 하는 마음 (Teachability)';
PRINT N'3) 31일 제목 교정 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';


-- ────────────────────────────────────────────────────────────
-- 4) 26일 (진실성) — 7일(정직성)과 간구가 거의 같았습니다.
--    두 편 모두 "혼날까 봐 망설일 때 사실대로 말하게" 였습니다.
--    26일은 "말을 옮기고 전하는 정직"으로 초점을 좁힙니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.PrayPhraseTemplate
   SET PrayContent1 = N'하나님,

**○○(이)**의 입에서 나오는 말이 사실 그대로이게 해 주세요.
들은 말을 옮길 때 보태거나 빼지 않게 하시고, 재미를 위하여 이야기를 부풀리지 않게 하옵소서.
그 말에 무게가 있어 어디서나 믿을 만한 사람으로 자라게 하소서.

예수님의 이름으로 간절히 기도합니다. 아멘!',
       PrayContent2 = N'하나님,

제 입에서 나오는 말이 사실 그대로이게 해 주세요.
들은 이야기를 전할 때 보태지 않게 하시고, 재미있게 하려고 부풀리지 않게 해 주세요.
제 말을 믿을 수 있는 사람이 되게 해 주세요.

예수님의 이름으로 기도합니다. 아멘!'
 WHERE DayNo = 26;
PRINT N'4) 26일 중복 해소';


-- ────────────────────────────────────────────────────────────
-- 5) 16일 (거룩) — 12일(분별력) · 20일(용기)과 겹쳤습니다.
--    세 편 모두 "다들 그렇게 해도 옳지 않으면 멈추게" 였습니다.
--    16일은 거룩의 본뜻인 "구별됨 — 몸과 마음이 하나님의 것"으로 옮깁니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.PrayPhraseTemplate
   SET PrayContent1 = N'하나님,

**○○(이)**의 몸과 마음이 하나님의 것임을 알게 해 주세요.
주님께서 머무시는 자리인 줄 알아 스스로를 소중히 여기게 하시고, 무엇을 보고 듣고 담을지 가려서 택하게 하옵소서.
날마다 새로워져 **○○(이)**의 말과 행동에 예수님의 모습이 자연스럽게 드러나게 하소서.

예수님의 이름으로 간절히 기도합니다. 아멘!',
       PrayContent2 = N'하나님,

제 몸과 마음이 하나님의 것임을 알게 해 주세요.
하나님이 제 안에 계신 줄 알고 저를 소중히 여기게 하시고, 무엇을 보고 들을지 가려서 고르게 해 주세요.
날마다 예수님을 닮아 가게 해 주세요.

예수님의 이름으로 기도합니다. 아멘!'
 WHERE DayNo = 16;
PRINT N'5) 16일 중복 해소';
GO


-- ────────────────────────────────────────────────────────────
-- 6) 줄바꿈을 CRLF 로 통일 (화면이 \r\n 로 문단을 나눕니다)
-- ────────────────────────────────────────────────────────────
UPDATE dbo.PrayPhraseTemplate
   SET PrayContent1 = REPLACE(REPLACE(REPLACE(PrayContent1, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10)),
       PrayContent2 = REPLACE(REPLACE(REPLACE(PrayContent2, CHAR(13)+CHAR(10), CHAR(10)), CHAR(13), CHAR(10)), CHAR(10), CHAR(13)+CHAR(10))
 WHERE DayNo IN (11, 16, 24, 26, 31);
PRINT N'6) 줄바꿈 통일';
GO


-- ────────────────────────────────────────────────────────────
-- 7) PrayPhrase 에 반영
--    ※ sp_InsertPrayPhrase 는 DELETE + INSERT 라 쓰지 않습니다.
--      Id 가 바뀌면 안 되므로 UPDATE ... FROM JOIN 으로만 옮깁니다.
--    ※ 고친 다섯 날(11·16·24·26·31)만, 오늘 이후 날짜에만 반영합니다.
--      지나간 날은 이미 드린 기도라 그대로 둡니다.
--    ※ updateDT 는 건드리지 않습니다.
-- ────────────────────────────────────────────────────────────
DECLARE @todayKst DATE = CAST(DATEADD(HOUR, 16, GETDATE()) AS DATE);

UPDATE p
   SET p.PrayTitle    = t.PrayTitle,
       p.PrayContent1 = t.PrayContent1,
       p.PrayContent2 = t.PrayContent2
  FROM dbo.PrayPhrase p
  JOIN dbo.PrayPhraseTemplate t ON t.DayNo = DAY(p.ExposeDate)
 WHERE t.DayNo IN (11, 16, 24, 26, 31)
   AND CAST(p.ExposeDate AS DATE) >= @todayKst;

PRINT N'7) PrayPhrase 반영 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건 (오늘 이후)';
GO


-- ────────────────────────────────────────────────────────────
--  확인
-- ────────────────────────────────────────────────────────────
SELECT DayNo, PrayTitle, LEN(PrayContent1) AS 부모길이, LEN(PrayContent2) AS 자녀길이
  FROM dbo.PrayPhraseTemplate WHERE DayNo IN (11,16,24,26,31) ORDER BY DayNo;

-- 다른 필드가 그대로인지 (0 이어야 정상)
SELECT COUNT(*) AS 템플릿_다른필드가_바뀐_행
  FROM dbo.PrayPhraseTemplate t JOIN dbo.PrayPhraseTemplate_bak_v3 b ON b.Id = t.Id
 WHERE ISNULL(t.BibleTextToPray,N'') <> ISNULL(b.BibleTextToPray,N'')
    OR ISNULL(t.PrayContent3,N'')    <> ISNULL(b.PrayContent3,N'')
    OR t.InsertDT <> b.InsertDT OR t.updateDT <> b.updateDT OR t.DayNo <> b.DayNo;

SELECT COUNT(*) AS 실제_다른필드가_바뀐_행
  FROM dbo.PrayPhrase p JOIN dbo.PrayPhrase_bak_v3 b ON b.Id = p.Id
 WHERE p.ExposeDate <> b.ExposeDate
    OR ISNULL(p.BibleTextToPray,N'') <> ISNULL(b.BibleTextToPray,N'')
    OR p.InsertDT <> b.InsertDT OR p.updateDT <> b.updateDT;

SELECT COUNT(*) AS 실제_Id가_사라진_행
  FROM dbo.PrayPhrase_bak_v3 b
 WHERE NOT EXISTS (SELECT 1 FROM dbo.PrayPhrase p WHERE p.Id = b.Id);

-- 남은 문제 (모두 0 이어야 정상)
SELECT
    SUM(CASE WHEN PrayContent2 LIKE N'%대답부터 먼저%' THEN 1 ELSE 0 END) AS 어색구문,
    SUM(CASE WHEN PrayContent1 LIKE N'%마음을 펴게%' OR PrayContent2 LIKE N'%마음을 펴게%' THEN 1 ELSE 0 END) AS 없는표현,
    SUM(CASE WHEN PrayTitle    LIKE N'%배우고자 하는 마음%' THEN 1 ELSE 0 END) AS 제목결불일치,
    SUM(CASE WHEN PrayContent1 LIKE N'%혼날까 봐%' THEN 1 ELSE 0 END) AS 정직중복,
    SUM(CASE WHEN PrayContent1 LIKE N'%다들 그렇게 한다는 말%' OR PrayContent1 LIKE N'%남들이 다 하는 일%' THEN 1 ELSE 0 END) AS 분별중복
  FROM dbo.PrayPhraseTemplate;
GO

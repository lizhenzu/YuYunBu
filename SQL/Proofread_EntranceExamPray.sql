-- ============================================================
--  수시·수능 준비생을 위한 기도문 — 교정
--
--  40편을 정독하여 찾은 오타 · 이상한 구문 · 문맥 어긋남을 고칩니다.
--
--  ⚠️ CreatedAt / UpdatedAt 은 건드리지 않습니다.
--     (검증 흐름의 "가장 오래된 기도문" 순서가 흐트러지지 않도록)
--  ⚠️ Title 과 PrayText 만 수정합니다.
--  ⚠️ REPLACE 로 해당 부분만 바꾸므로 이미 고쳐진 행은 그대로 지나갑니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EntranceExamPray_bak_v3' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.EntranceExamPray_bak_v3 FROM dbo.EntranceExamPray;
    PRINT '[EntranceExamPray_bak_v3] 백업 생성';
END
ELSE
    PRINT '[EntranceExamPray_bak_v3] 백업이 이미 있습니다 — 건너뜀';


-- ────────────────────────────────────────────────────────────
-- 1) 본문 맨 앞에 붙은 군더더기 공백 (6번, 17번)
--    화면에서 첫 줄이 한 칸 밀려 보입니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET PrayText = LTRIM(PrayText)
 WHERE LEFT(PrayText, 1) IN (N' ', CHAR(9), CHAR(13), CHAR(10));

PRINT N'1) 앞 공백 제거 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';


-- ────────────────────────────────────────────────────────────
-- 2) 21번 「수능일을 앞두고」 — 제목과 본문이 어긋남
--    제목은 "수능일을 앞두고" 인데 본문은 "아직 백일이 있음" 이라 하여
--    D-100 에만 맞는 기도가 되어 있습니다. 언제 읽어도 맞도록 고칩니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(PrayText,
        N'아직 백일이 있음을 감사하게 하시고',
        N'아직 남은 날이 있음을 감사하게 하시고')
 WHERE SortOrder = 21;

-- 2-1) 21번의 맺는 말씀이 2번 기도문과 같은 구절(시편 90:12)이라 겹칩니다.
--      표시된 구절(전도서 3:1)로 바꾸어 중복을 없앱니다.
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(PrayText,
        N'우리에게 우리 날 계수함을 가르치사 지혜로운 마음을 얻게 하소서 하였습니다. 그 말씀대로 이 날들을 살게 하옵소서.',
        N'범사에 기한이 있고 천하 만사가 다 때가 있다 하였습니다. 그 말씀을 붙들고 이 날들을 살게 하옵소서.')
 WHERE SortOrder = 21;

PRINT N'2) 21번 백일·중복 구절 교정';


-- ────────────────────────────────────────────────────────────
-- 3) 22번 — 성경 인용 표기 오류
--    시편 4:8 개역개정은 "오직 여호와이시니이다" 입니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(PrayText,
        N'오직 여호와시니이다',
        N'오직 여호와이시니이다')
 WHERE SortOrder = 22;

PRINT N'3) 22번 시편 4:8 표기 교정';


-- ────────────────────────────────────────────────────────────
-- 4) 40번 — 잠언 3:24 인용에서 "네가 누운즉" 이 빠져 있습니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(PrayText,
        N'네가 누울 때에 두려워하지 아니하겠고 네 잠이 달리로다',
        N'네가 누울 때에 두려워하지 아니하겠고 네가 누운즉 네 잠이 달리로다')
 WHERE SortOrder = 40;

PRINT N'4) 40번 잠언 3:24 인용 보완';


-- ────────────────────────────────────────────────────────────
-- 5) 39번 — "지침" 이 두 번 나오는데, 읽는 이가 指針(가이드)으로
--    오해하기 쉽습니다. 뜻이 분명한 말로 바꿉니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(REPLACE(PrayText,
        N'짜증으로 새어 나오는 것이 사실은 지침이고 두려움인 줄 아시오니',
        N'짜증으로 새어 나오는 것이 사실은 지쳐 있음이고 두려움인 줄 아시오니'),
        N'서로의 지침을 헤아리며 이 시기를 함께 지나가게 하옵소서.',
        N'서로의 지친 마음을 헤아리며 이 시기를 함께 지나가게 하옵소서.')
 WHERE SortOrder = 39;

PRINT N'5) 39번 "지침" 표현 교정';


-- ────────────────────────────────────────────────────────────
-- 6) 25번 「시험 도중 막힐 때」 — 첫 문단에 주어가 없어
--    기도하는 부모 자신의 이야기처럼 읽힙니다. 대상을 밝혀 줍니다.
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(PrayText,
        N'갑자기 머리가 하얘지는 순간이 있습니다. 분명 알던 것인데 떠오르지 아니하고, 마음만 급해지기 쉽습니다.',
        N'시험을 치르는 중에 **○○(이)**의 머리가 갑자기 하얘지는 순간이 있을 것입니다. 분명 알던 것인데 떠오르지 아니하고, 마음만 급해지기 쉬운 자리입니다.')
 WHERE SortOrder = 25;

PRINT N'6) 25번 주어 보완';
GO


-- ────────────────────────────────────────────────────────────
--  줄바꿈을 CRLF 로 통일 (화면이 \r\n 로 문단을 나눕니다)
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10));
GO


-- ────────────────────────────────────────────────────────────
--  확인
-- ────────────────────────────────────────────────────────────
SELECT SortOrder, Title, LEN(PrayText) AS len
  FROM dbo.EntranceExamPray
 WHERE SortOrder IN (6, 17, 21, 22, 25, 39, 40)
 ORDER BY SortOrder;

SELECT
    SUM(CASE WHEN LEFT(PrayText,1) IN (N' ', CHAR(9), CHAR(13), CHAR(10)) THEN 1 ELSE 0 END) AS 앞공백,
    SUM(CASE WHEN PrayText LIKE N'%백일%'          THEN 1 ELSE 0 END) AS 백일,
    SUM(CASE WHEN PrayText LIKE N'%여호와시니이다%' THEN 1 ELSE 0 END) AS 표기오류,
    SUM(CASE WHEN PrayText LIKE N'%지침%'          THEN 1 ELSE 0 END) AS 지침,
    SUM(CASE WHEN PrayText LIKE N'%우리 날 계수함%' THEN 1 ELSE 0 END) AS 시90_12사용
  FROM dbo.EntranceExamPray;
GO

-- ============================================================
--  자녀를 위한 축복 기도문(BessingChild) — 남은 항목 정리
--
--  ※ 이 표는 Id 가 곧 "날짜" 입니다.
--     ControlBessingChild.fetchBessingChild : and Id = day(오늘)
--     → Id 1 은 매달 1일, Id 14 는 14일에 나옵니다.
--     → Id 32 는 32일이 없으므로 영원히 화면에 나오지 않습니다.
--
--  1) Id 14 ↔ Id 32 기도문 맞바꿈
--     · Id 14 「76. 만남의 복을 주소서(I)」는 Id 23 (II)와 제목·본문·성경말씀이
--       모두 겹칩니다. 같은 달 14일과 23일에 사실상 같은 기도가 나옵니다.
--     · Id 32 「203. 온유한 자녀로 자라게 하소서」는 도달할 수 없어 사장돼 있습니다.
--     → 둘을 맞바꾸면 중복이 사라지고 잠자던 기도문이 살아납니다.
--       (지우는 것이 아니라 자리를 바꾸는 것이라 되돌리기 쉽습니다)
--
--  2) Id 3 · 4 의 "해 주세요"체를 나머지 30편과 같은 "하옵소서"체로
--
--  3) 제목 형식이 다른 3편 정리 (16 · 19 · 30) — 앞 번호는 원본 그대로 둡니다
--
--  ⚠️ InsertDT · updateDT · FromDate · ToDate · visited 는 건드리지 않습니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BessingChild_bak_v4' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.BessingChild_bak_v4 FROM dbo.BessingChild;
    PRINT '[BessingChild_bak_v4] 백업 생성';
END
ELSE
    PRINT '[BessingChild_bak_v4] 백업이 이미 있습니다 — 건너뜀';


-- ────────────────────────────────────────────────────────────
-- 1) Id 14 ↔ Id 32 맞바꿈 (제목 · 본문 · 성경말씀 · 영문 을 함께 옮깁니다.
--    성경말씀을 두고 가면 기도문과 구절이 어긋나기 때문입니다)
-- ────────────────────────────────────────────────────────────
DECLARE @t14 NVARCHAR(200), @x14 NVARCHAR(MAX), @h14 NVARCHAR(MAX), @e14 NVARCHAR(MAX);
DECLARE @t32 NVARCHAR(200), @x32 NVARCHAR(MAX), @h32 NVARCHAR(MAX), @e32 NVARCHAR(MAX);

SELECT @t14 = PrayTitle, @x14 = PrayText, @h14 = HolyWord, @e14 = PrayText_Eng
  FROM dbo.BessingChild WHERE Id = 14;
SELECT @t32 = PrayTitle, @x32 = PrayText, @h32 = HolyWord, @e32 = PrayText_Eng
  FROM dbo.BessingChild WHERE Id = 32;

IF @t14 IS NOT NULL AND @t32 IS NOT NULL AND @t14 <> @t32
BEGIN
    UPDATE dbo.BessingChild
       SET PrayTitle = @t32, PrayText = @x32, HolyWord = @h32, PrayText_Eng = @e32
     WHERE Id = 14;

    UPDATE dbo.BessingChild
       SET PrayTitle = @t14, PrayText = @x14, HolyWord = @h14, PrayText_Eng = @e14
     WHERE Id = 32;

    PRINT N'1) Id 14 ↔ 32 맞바꿈 완료';
END
ELSE
    PRINT N'1) 이미 맞바꾸었거나 대상이 없습니다 — 건너뜀';


-- ────────────────────────────────────────────────────────────
-- 2) Id 3 · 4 문체 통일 — "해 주세요" → "하옵소서"
--    구체적인 것부터 바꾸고 마지막에 일반형을 적용합니다 (순서가 중요합니다)
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText, N'자라게 해 주셔서 감사해요.', N'자라게 하여 주시니 감사드립니다.')
 WHERE Id IN (3, 4);

UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText, N'저희 부모도 스스로를', N'저희 부모(교사)도 스스로를')
 WHERE Id IN (3, 4);

UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText, N'잃지 않게 도와주세요.', N'잃지 않게 도와주옵소서.')
 WHERE Id IN (3, 4);

UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText, N'곁에 함께 계셔 주세요.', N'곁에 함께하여 주옵소서.')
 WHERE Id IN (3, 4);

UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText, N'자라나도록 오늘도 함께해 주세요.', N'자라나도록 오늘도 함께하여 주옵소서.')
 WHERE Id IN (3, 4);

UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText, N'해 주시고,', N'하시고,')
 WHERE Id IN (3, 4);

-- 일반형 : "…게 해 주세요." → "…게 하옵소서."
UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText, N'해 주세요.', N'하옵소서.')
 WHERE Id IN (3, 4);

UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText, N'예수님의 이름으로 기도드려요.', N'예수님의 이름으로 기도드립니다.')
 WHERE Id IN (3, 4);

PRINT N'2) Id 3 · 4 문체 통일 완료';


-- ────────────────────────────────────────────────────────────
-- 3) 제목 형식 정리 (앞 번호는 원본 그대로)
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingChild
   SET PrayTitle = N'129. 삶에 열정이 넘치게 하소서'
 WHERE Id = 16 AND PrayTitle = N'129. 우리 자녀의 삶에 열정이 넘치게 하소서';

UPDATE dbo.BessingChild
   SET PrayTitle = N'120. 좋은 스승과 좋은 만남을 주소서'
 WHERE Id = 19 AND PrayTitle = N'120. 좋은 만남을 위한 기도';

UPDATE dbo.BessingChild
   SET PrayTitle = N'58. 선한 마음과 깊은 삶을 살게 하소서'
 WHERE Id = 30 AND PrayTitle = N'58. 우리 아이의 선한 마음과 깊은 삶을 위한 기도';

PRINT N'3) 제목 형식 정리 완료';
GO


-- 줄바꿈 CRLF 통일
UPDATE dbo.BessingChild
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10));
GO


-- ────────────────────────────────────────────────────────────
--  확인
-- ────────────────────────────────────────────────────────────
SELECT Id, PrayTitle FROM dbo.BessingChild WHERE Id IN (3,4,14,16,19,23,30,32) ORDER BY Id;

-- 제목 앞 번호가 겹치는지 (0 이어야 정상)
;WITH t AS (
  SELECT Id, LEFT(PrayTitle, CHARINDEX(N'.', PrayTitle) - 1) AS 번호
    FROM dbo.BessingChild WHERE CHARINDEX(N'.', PrayTitle) > 1
)
SELECT 번호, COUNT(*) AS 건수 FROM t GROUP BY 번호 HAVING COUNT(*) > 1;

-- Id 3 · 4 에 "해 주세요 / 기도드려요" 가 남았는지 (0 이어야 정상)
SELECT Id, PrayTitle FROM dbo.BessingChild
 WHERE Id IN (3,4) AND (PrayText LIKE N'%해 주세요%' OR PrayText LIKE N'%기도드려요%');

-- 날짜·조회수 필드가 그대로인지 대조 (0 이어야 정상)
SELECT COUNT(*) AS 날짜조회수가_바뀐_행
  FROM dbo.BessingChild p JOIN dbo.BessingChild_bak_v4 b ON b.Id = p.Id
 WHERE ISNULL(p.FromDate,N'') <> ISNULL(b.FromDate,N'')
    OR ISNULL(p.ToDate,N'')   <> ISNULL(b.ToDate,N'')
    OR p.visited  <> b.visited
    OR p.InsertDT <> b.InsertDT
    OR p.updateDT <> b.updateDT;
GO

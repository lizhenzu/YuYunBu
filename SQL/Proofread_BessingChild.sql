-- ============================================================
--  자녀를 위한 축복 기도문(BessingChild) — 교정
--
--  32편을 정독하여 찾은 것을 고칩니다.
--   1) 제목 표기 흐트러짐 (번호 뒤 공백 없음 · 끝 마침표 · 이중 공백 · 띄어쓰기)
--   2) 인용부호가 사라져 뜻이 끊긴 문장
--   3) 띄어쓰기 오류
--   4) 한 기도문 안에서 "해 주세요"체와 "하옵소서"체가 섞인 곳
--   5) 부모를 가리키는 말이 단수·복수로 흔들리는 곳
--
--  ⚠️ PrayTitle 과 PrayText 만 수정합니다.
--     InsertDT · updateDT · FromDate · ToDate · HolyWord · PrayText_Eng ·
--     visited 는 한 곳도 건드리지 않습니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BessingChild_bak_v3' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.BessingChild_bak_v3 FROM dbo.BessingChild;
    PRINT '[BessingChild_bak_v3] 백업 생성';
END
ELSE
    PRINT '[BessingChild_bak_v3] 백업이 이미 있습니다 — 건너뜀';


-- ────────────────────────────────────────────────────────────
-- 1) 제목 표기 정리
-- ────────────────────────────────────────────────────────────

-- 1-1) 앞뒤 공백 · 이중 공백 (예: "78.뛰어난 기초학습  능력을  갖게 하소서")
UPDATE dbo.BessingChild
   SET PrayTitle = LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(PrayTitle, N'  ', N' '), N'  ', N' '), N'  ', N' ')))
 WHERE PrayTitle LIKE N'%  %' OR PrayTitle <> LTRIM(RTRIM(PrayTitle));
PRINT N'1-1) 제목 공백 정리 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';

-- 1-2) 끝에 붙은 마침표 (예: "79. 정직한 영을 내려 주소서.")
UPDATE dbo.BessingChild
   SET PrayTitle = LEFT(PrayTitle, LEN(PrayTitle) - 1)
 WHERE RIGHT(RTRIM(PrayTitle), 1) = N'.';
PRINT N'1-2) 제목 끝 마침표 제거 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';

-- 1-3) 번호 뒤에 공백 넣기 (예: "95.깨끗한" → "95. 깨끗한")
UPDATE dbo.BessingChild
   SET PrayTitle = STUFF(PrayTitle, CHARINDEX(N'.', PrayTitle) + 1, 0, N' ')
 WHERE CHARINDEX(N'.', PrayTitle) > 1
   AND SUBSTRING(PrayTitle, CHARINDEX(N'.', PrayTitle) + 1, 1) <> N' '
   AND LEFT(PrayTitle, CHARINDEX(N'.', PrayTitle) - 1) NOT LIKE N'%[^0-9]%';
PRINT N'1-3) 번호 뒤 공백 보완 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';

-- 1-4) "예배 드리게" → "예배드리게" ('예배드리다'는 한 낱말입니다)
UPDATE dbo.BessingChild
   SET PrayTitle = REPLACE(PrayTitle, N'예배 드리게', N'예배드리게')
 WHERE PrayTitle LIKE N'%예배 드리게%';
PRINT N'1-4) 제목 띄어쓰기 교정 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';


-- ────────────────────────────────────────────────────────────
-- 2) 인용부호가 사라져 뜻이 끊긴 문장
-- ────────────────────────────────────────────────────────────

-- 3번 「깨끗한 자녀가 되게 하소서」
UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText,
        N'이건 옳지 않아 라고 말할 수 있는 용기를 주시고',
        N'이건 옳지 않다고 말할 수 있는 용기를 주시고')
 WHERE Id = 3;

-- 13번 「긍정적인 삶을 살게 하소서」
UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText,
        N'예수님께서 하나님 앞에서 언제나 예 라고 응답하셨듯이',
        N'예수님께서 하나님 앞에서 언제나 순종으로 응답하셨듯이')
 WHERE Id = 13;

-- 29번 「성실하신 주님을 닮게 하소서」
UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText,
        N'내 삶의 기준은 오직 성실하신 주님을 닮는 것이라는 믿음의 원칙이',
        N'삶의 기준은 오직 성실하신 주님을 닮는 것이라는 믿음의 원칙이')
 WHERE Id = 29;

PRINT N'2) 인용부호 없이 끊긴 문장 3곳 교정';


-- ────────────────────────────────────────────────────────────
-- 3) 띄어쓰기 오류 — "못자국" → "못 자국"
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText, N'못자국 난', N'못 자국 난')
 WHERE PrayText LIKE N'%못자국 난%';
PRINT N'3) 띄어쓰기 교정 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';


-- ────────────────────────────────────────────────────────────
-- 4) 한 기도문 안에서 문체가 섞인 곳 (27 · 28 · 30번)
--    이 표는 "하옵소서"체가 기본입니다. 중간에 끼어든 "해 주세요"를 맞춥니다.
-- ────────────────────────────────────────────────────────────

-- 27번 「어려움을 극복할 용기와 소망을 주소서」
UPDATE dbo.BessingChild
   SET PrayText =
        REPLACE(REPLACE(REPLACE(REPLACE(PrayText,
        N'더 높이 주님을 바라볼 수 있게 도와주세요.',
        N'더 높이 주님을 바라보게 하여 주옵소서.'),
        N'희망과 믿음과 건강한 생각들이 깊이 자라나게 해 주세요.',
        N'희망과 믿음과 건강한 생각들이 깊이 자라나게 하여 주옵소서.'),
        N'주님께서 강한 팔로 붙드심을 느끼게 해 주세요.',
        N'주님께서 강한 팔로 붙드심을 느끼게 하여 주옵소서.'),
        N'그 어둠 속에서도 등불이 되어 인도해 주세요.',
        N'그 어둠 속에서도 등불이 되어 인도하여 주옵소서.')
 WHERE Id = 27;

-- 27번 — 부모를 가리키는 말이 단수로 흔들림
UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText,
        N'저는 부모(교사)이지만 이 아이의 인생 전체를 책임질 수는 없습니다.',
        N'저희는 부모(교사)이지만 이 아이의 인생 전체를 책임질 수는 없습니다.')
 WHERE Id = 27;

-- 28번 「영적 분별력을 주소서」
UPDATE dbo.BessingChild
   SET PrayText =
        REPLACE(REPLACE(REPLACE(PrayText,
        N'바른 길을 걸어가게 인도해 주세요.',
        N'바른 길을 걸어가게 인도하여 주옵소서.'),
        N'악한 유혹에 흔들리지 않도록 붙들어 주세요.',
        N'악한 유혹에 흔들리지 않도록 붙들어 주옵소서.'),
        N'바른 선택을 할 수 있는 지혜를 허락해 주세요.',
        N'바른 선택을 할 수 있는 지혜를 허락하여 주옵소서.')
 WHERE Id = 28;

UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText,
        N'부모(교사)인 제가 아무리 품고 지켜 주고 싶어도',
        N'부모(교사)인 저희가 아무리 품고 지켜 주고 싶어도')
 WHERE Id = 28;

-- 30번 「선한 마음과 깊은 삶을 위한 기도」
UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText,
        N'언제나 평안한 미소를 잃지 않는 아이가 되게 해 주세요.',
        N'언제나 평안한 미소를 잃지 않는 아이가 되게 하옵소서.')
 WHERE Id = 30;

PRINT N'4) 27·28·30번 문체·인칭 통일';


-- ────────────────────────────────────────────────────────────
-- 5) 22번 — 다른 편은 모두 "부모(교사)" 인데 여기만 "부모"
-- ────────────────────────────────────────────────────────────
UPDATE dbo.BessingChild
   SET PrayText = REPLACE(PrayText,
        N'저희 부모가 먼저 믿음의 본이 되어',
        N'저희 부모(교사)가 먼저 믿음의 본이 되어')
 WHERE Id = 22;
PRINT N'5) 22번 표기 통일 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';


-- ────────────────────────────────────────────────────────────
-- 6) 본문 앞 군더더기 공백·줄바꿈 제거 + 줄바꿈 CRLF 통일
--    (화면이 \r\n 로 문단을 나눕니다)
-- ────────────────────────────────────────────────────────────
WHILE EXISTS (SELECT 1 FROM dbo.BessingChild
               WHERE LEFT(PrayText,1) IN (N' ', CHAR(9), CHAR(13), CHAR(10)))
BEGIN
    UPDATE dbo.BessingChild
       SET PrayText = SUBSTRING(PrayText, 2, LEN(PrayText))
     WHERE LEFT(PrayText,1) IN (N' ', CHAR(9), CHAR(13), CHAR(10));
END

UPDATE dbo.BessingChild
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10));
PRINT N'6) 앞 공백 제거 · 줄바꿈 통일';
GO


-- ────────────────────────────────────────────────────────────
--  확인
-- ────────────────────────────────────────────────────────────
SELECT p.Id, p.PrayTitle
  FROM dbo.BessingChild p
  JOIN dbo.BessingChild_bak_v3 b ON b.Id = p.Id
 WHERE ISNULL(p.PrayText,N'') <> ISNULL(b.PrayText,N'') OR ISNULL(p.PrayTitle,N'') <> ISNULL(b.PrayTitle,N'')
 ORDER BY p.Id;

-- 다른 필드가 그대로인지 대조 (0 이어야 정상)
SELECT COUNT(*) AS 다른필드가_바뀐_행
  FROM dbo.BessingChild p
  JOIN dbo.BessingChild_bak_v3 b ON b.Id = p.Id
 WHERE ISNULL(p.FromDate,N'')     <> ISNULL(b.FromDate,N'')
    OR ISNULL(p.ToDate,N'')       <> ISNULL(b.ToDate,N'')
    OR ISNULL(p.HolyWord,N'')     <> ISNULL(b.HolyWord,N'')
    OR ISNULL(p.PrayText_Eng,N'') <> ISNULL(b.PrayText_Eng,N'')
    OR p.visited                  <> b.visited
    OR p.InsertDT                 <> b.InsertDT
    OR p.updateDT                 <> b.updateDT;

-- 남은 문제 (모두 0 이어야 정상)
SELECT
    SUM(CASE WHEN PrayTitle LIKE N'%  %'          THEN 1 ELSE 0 END) AS 제목_이중공백,
    SUM(CASE WHEN RIGHT(RTRIM(PrayTitle),1)=N'.'  THEN 1 ELSE 0 END) AS 제목_끝마침표,
    SUM(CASE WHEN PrayTitle LIKE N'[0-9].[^ ]%' OR PrayTitle LIKE N'[0-9][0-9].[^ ]%' OR PrayTitle LIKE N'[0-9][0-9][0-9].[^ ]%'
             THEN 1 ELSE 0 END)                                      AS 제목_번호뒤공백없음,
    SUM(CASE WHEN PrayText LIKE N'%못자국%'        THEN 1 ELSE 0 END) AS 못자국,
    SUM(CASE WHEN PrayText LIKE N'%옳지 않아 라고%' THEN 1 ELSE 0 END) AS 끊긴인용,
    SUM(CASE WHEN LEFT(PrayText,1) IN (N' ',CHAR(9),CHAR(13),CHAR(10)) THEN 1 ELSE 0 END) AS 앞공백
  FROM dbo.BessingChild;

-- 27·28·30 번에 "해 주세요"가 남았는지 (0 이어야 정상)
SELECT Id, PrayTitle FROM dbo.BessingChild
 WHERE Id IN (27,28,30) AND PrayText LIKE N'%해 주세요%';
GO

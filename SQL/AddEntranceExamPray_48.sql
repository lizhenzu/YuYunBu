-- ============================================================
--  수시·수능 준비생을 위한 기도문 — 매일 순환 1편 추가
--
--   48 남은 날이 얼마 없다고 느낄 때   (시간에 쫓기는 조급함)
--
--  ※ RotateYN = 'Y' — 매일 순환에 들어갑니다.
--  ※ 문체·분량은 기존 편들과 맞췄습니다 (하여 주옵소서 체 · 다섯 문단 · 440~560자).
--  ※ 화자는 "부모가 자녀를 위하여" 입니다. 1인칭('저')을 쓰지 않습니다.
--  ※ 성경 구절(시편 31:15)은 이미 쓰인 것과 겹치지 않습니다.
--  ※ 이미 들어 있으면 다시 넣지 않습니다.
--
--  ⚠️ SortOrder 는 고정 숫자로 넣지 않습니다.
--     MoveToSecond_EntranceExamPray.sql 이 순환 기도문을 1,2,3… 으로,
--     순환 밖(그날에 읽는 것·숨긴 것)을 101 부터로 다시 매겨 두었으므로,
--     순환 목록의 마지막 번호 + 1 을 계산해서 맨 뒤에 붙입니다.
-- ============================================================
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.EntranceExamPray WHERE Title = N'남은 날이 얼마 없다고 느낄 때')
BEGIN
    PRINT N'「남은 날이 얼마 없다고 느낄 때」가 이미 있습니다 — 삽입 건너뜀';
END
ELSE
BEGIN
    -- 순환에 드는 기도문 중 가장 뒤 번호의 다음 자리
    DECLARE @nextSort INT = (
        SELECT ISNULL(MAX(SortOrder), 0) + 1
          FROM dbo.EntranceExamPray
         WHERE ISNULL(UseYN,'Y') = 'Y' AND ISNULL(RotateYN,'Y') = 'Y'
    );

    INSERT INTO dbo.EntranceExamPray (SortOrder, Title, PrayText, BibleRef, BibleText, UseYN, RotateYN, CreatedAt)
    VALUES
    (@nextSort, N'남은 날이 얼마 없다고 느낄 때',
N'때를 정하시는 하나님,

달력을 보는 **○○(이)**의 마음이 조급합니다. 남은 날은 자꾸 줄어드는데 해야 할 것은 그대로인 것 같아, 앉기도 전에 마음이 먼저 지칩니다.

시간을 세는 눈보다 오늘을 사는 눈을 주옵소서. 아직 오지 아니한 날들을 미리 살아 내느라 힘을 다 쓰지 않게 하시고, 지금 앞에 놓인 한 시간을 성실히 붙들게 하여 주옵소서.

늦었다는 생각에 주저앉지 않게 하여 주옵소서. 지나간 날을 아쉬워하는 마음이 남은 날까지 삼키지 않게 하시고, 오늘부터 다시 시작할 수 있음을 알게 하여 주옵소서.

조급함이 몸을 상하게 하지 않게 하여 주옵소서. 쫓기듯 서두르다 끼니와 잠을 잃지 않게 하시고, 남은 길을 끝까지 걸을 수 있도록 오늘의 걸음을 지켜 주옵소서.

나의 앞날이 주의 손에 있다 하였습니다. 남은 날의 길이를 아시는 분이 주님이신 줄 믿고, **○○(이)**가 그 날들을 두려움이 아니라 맡김으로 맞이하게 하옵소서.

예수님의 이름으로 기도드립니다. 아멘.',
     N'시편 31:15', N'나의 앞날이 주의 손에 있사오니 내 원수들과 나를 핍박하는 자들의 손에서 나를 건져 주소서',
     'Y', 'Y', DATEADD(HOUR, 16, GETDATE()));

    PRINT N'기도문 1편 추가 완료 — SortOrder ' + CAST(@nextSort AS NVARCHAR(10));
END
GO

-- 줄바꿈을 CRLF 로 통일 (화면이 \r\n 로 문단을 나눕니다)
UPDATE dbo.EntranceExamPray
   SET PrayText = REPLACE(REPLACE(REPLACE(PrayText,
                    CHAR(13) + CHAR(10), CHAR(10)),
                    CHAR(13),            CHAR(10)),
                    CHAR(10),            CHAR(13) + CHAR(10))
 WHERE Title = N'남은 날이 얼마 없다고 느낄 때';
GO

-- 결과 확인
SELECT PrayId, SortOrder, Title, LEN(PrayText) AS len,
       (LEN(PrayText) - LEN(REPLACE(PrayText, CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10), N''))) / 4 + 1 AS paras,
       RotateYN, BibleRef
  FROM dbo.EntranceExamPray
 WHERE Title = N'남은 날이 얼마 없다고 느낄 때';

SELECT COUNT(*) AS rows_total,
       SUM(CASE WHEN ISNULL(UseYN,'Y') = 'Y' THEN 1 ELSE 0 END) AS active_count,
       SUM(CASE WHEN ISNULL(UseYN,'Y') = 'Y' AND ISNULL(RotateYN,'Y') = 'Y' THEN 1 ELSE 0 END) AS rotation_count,
       SUM(CASE WHEN PrayText LIKE N'%저를%' OR PrayText LIKE N'%저만%' OR PrayText LIKE N'%제가%'
                 OR PrayText LIKE N'%저는%' OR PrayText LIKE N'%저의%' OR PrayText LIKE N'%저에게%'
                THEN 1 ELSE 0 END) AS first_person_left,
       SUM(CASE WHEN PrayText LIKE N'%해 주세요%'  THEN 1 ELSE 0 END) AS casual_left,
       SUM(CASE WHEN PrayText LIKE N'%기도 제목:%' THEN 1 ELSE 0 END) AS label_left
  FROM dbo.EntranceExamPray;

-- 성경 구절이 겹치는지 확인 (없어야 정상)
SELECT BibleRef, COUNT(*) AS cnt FROM dbo.EntranceExamPray
 GROUP BY BibleRef HAVING COUNT(*) > 1;

-- 순환 번호가 1부터 빈 자리 없이 이어지는지 (맨 뒤에 붙었는지)
SELECT MIN(SortOrder) AS 최소, MAX(SortOrder) AS 최대, COUNT(*) AS 건수
  FROM dbo.EntranceExamPray
 WHERE ISNULL(UseYN,'Y')='Y' AND ISNULL(RotateYN,'Y')='Y';
GO

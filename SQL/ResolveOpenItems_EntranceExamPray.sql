-- ============================================================
--  수시·수능 준비생을 위한 기도문 — 남아 있던 세 가지 정리
--
--  1) 본문 인용과 화면 표시 구절이 다른 6곳
--     → 표시 구절(BibleRef/BibleText)을 본문에 맞춥니다.
--       본문의 인용은 맺는 문장에 녹아 있어 손대면 글이 상합니다.
--       화면 옆에 붙는 구절을 바꾸는 편이 안전하고 결과도 낫습니다.
--
--  2) 제목이 겹치는 13번 · 34번 「학교와 학과를 정하며」
--     → 34번을 숨깁니다. 13번이 더 온전하고(7문단), 34번은 13번을 줄인 판입니다.
--       삭제 대신 숨김이라 언제든 되살릴 수 있습니다.
--
--  3) 30번 제목 「부모님을 위한 기도」
--     → 내용은 부모가 자기 마음을 두고 드리는 기도인데 제목은
--       자녀가 부모를 위해 드리는 기도처럼 읽힙니다. 제목을 내용에 맞춥니다.
--
--  ⚠️ CreatedAt / UpdatedAt 은 건드리지 않습니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EntranceExamPray_bak_v4' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    SELECT * INTO dbo.EntranceExamPray_bak_v4 FROM dbo.EntranceExamPray;
    PRINT '[EntranceExamPray_bak_v4] 백업 생성';
END
ELSE
    PRINT '[EntranceExamPray_bak_v4] 백업이 이미 있습니다 — 건너뜀';


-- ────────────────────────────────────────────────────────────
-- 1) 표시 구절을 본문 인용에 맞춤 (5, 7, 8, 10, 13)
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET BibleRef  = N'잠언 24:16',
       BibleText = N'대저 의인은 일곱 번 넘어질지라도 다시 일어나려니와 악인은 재앙으로 말미암아 엎드러지느니라'
 WHERE SortOrder = 5;

UPDATE dbo.EntranceExamPray
   SET BibleRef  = N'마태복음 11:28',
       BibleText = N'수고하고 무거운 짐 진 자들아 다 내게로 오라 내가 너희를 쉬게 하리라'
 WHERE SortOrder = 7;

UPDATE dbo.EntranceExamPray
   SET BibleRef  = N'이사야 40:31',
       BibleText = N'오직 여호와를 앙망하는 자는 새 힘을 얻으리니 독수리가 날개치며 올라감 같을 것이요 달음박질하여도 곤비하지 아니하겠고 걸어가도 피곤하지 아니하리로다'
 WHERE SortOrder = 8;

UPDATE dbo.EntranceExamPray
   SET BibleRef  = N'갈라디아서 5:22-23',
       BibleText = N'오직 성령의 열매는 사랑과 희락과 화평과 오래 참음과 자비와 양선과 충성과 온유와 절제니 이같은 것을 금지할 법이 없느니라'
 WHERE SortOrder = 10;

UPDATE dbo.EntranceExamPray
   SET BibleRef  = N'잠언 16:9',
       BibleText = N'사람이 마음으로 자기의 길을 계획할지라도 그의 걸음을 인도하시는 이는 여호와시니라'
 WHERE SortOrder = 13;

PRINT N'1) 표시 구절 5건 정렬';


-- ────────────────────────────────────────────────────────────
-- 2) 제목이 겹치는 34번을 숨김 (삭제 아님 — 언제든 되살릴 수 있습니다)
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET UseYN = 'N'
 WHERE SortOrder = 34;

PRINT N'2) 34번 숨김 처리 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';


-- ────────────────────────────────────────────────────────────
-- 3) 30번 제목을 내용에 맞춤
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET Title = N'부모의 마음을 지켜 주소서'
 WHERE SortOrder = 30 AND Title = N'부모님을 위한 기도';

PRINT N'3) 30번 제목 교정 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';
GO


-- ────────────────────────────────────────────────────────────
--  확인
-- ────────────────────────────────────────────────────────────
SELECT SortOrder, Title, UseYN, RotateYN, BibleRef
  FROM dbo.EntranceExamPray
 WHERE SortOrder IN (5, 7, 8, 10, 13, 30, 34)
 ORDER BY SortOrder;

-- 성경 구절이 겹치는지 (사용중인 것끼리, 없어야 정상)
SELECT BibleRef, COUNT(*) AS cnt
  FROM dbo.EntranceExamPray
 WHERE ISNULL(UseYN,'Y') = 'Y'
 GROUP BY BibleRef HAVING COUNT(*) > 1;

-- 제목이 겹치는지 (사용중인 것끼리, 없어야 정상)
SELECT Title, COUNT(*) AS cnt
  FROM dbo.EntranceExamPray
 WHERE ISNULL(UseYN,'Y') = 'Y'
 GROUP BY Title HAVING COUNT(*) > 1;

SELECT COUNT(*) AS 전체,
       SUM(CASE WHEN ISNULL(UseYN,'Y')='Y' THEN 1 ELSE 0 END) AS 사용중,
       SUM(CASE WHEN ISNULL(UseYN,'Y')='Y' AND ISNULL(RotateYN,'Y')='Y' THEN 1 ELSE 0 END) AS 매일순환
  FROM dbo.EntranceExamPray;

-- 날짜가 그대로인지 대조 (0 이어야 정상)
SELECT COUNT(*) AS 날짜가_바뀐_행
  FROM dbo.EntranceExamPray p
  JOIN dbo.EntranceExamPray_bak_v4 b ON b.PrayId = p.PrayId
 WHERE ISNULL(p.CreatedAt,'1900-01-01') <> ISNULL(b.CreatedAt,'1900-01-01')
    OR ISNULL(p.UpdatedAt,'1900-01-01') <> ISNULL(b.UpdatedAt,'1900-01-01');
GO

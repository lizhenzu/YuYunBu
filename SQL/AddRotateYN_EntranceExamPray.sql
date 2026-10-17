-- ============================================================
--  수시·수능 준비생을 위한 기도문 — 순환 제외 표시 추가
--
--  매일 한 편씩 돌아가는 기도문 가운데, 특정한 날이나 순간에만 어울리는 것이
--  있습니다. 예를 들어 "수능 날 아침에" 가 3월 어느 화요일에 오늘의 기도로
--  나오면 어색합니다.
--
--  · RotateYN = 'Y' : 매일 순환에 포함 (기본값)
--  · RotateYN = 'N' : 순환에서 빠짐. 목록에는 그대로 남아 직접 골라 읽을 수 있음.
--
--  ※ UseYN 과는 다릅니다. UseYN='N' 은 목록에서도 사라지지만
--     RotateYN='N' 은 목록에 남습니다.
-- ============================================================
SET NOCOUNT ON;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
     WHERE object_id = OBJECT_ID('dbo.EntranceExamPray') AND name = 'RotateYN'
)
BEGIN
    ALTER TABLE dbo.EntranceExamPray
        ADD [RotateYN] CHAR(1) NOT NULL
            CONSTRAINT [DF_EntranceExamPray_RotateYN] DEFAULT ('Y');
    PRINT '[EntranceExamPray.RotateYN] 컬럼 추가 완료';
END
ELSE
    PRINT '[EntranceExamPray.RotateYN] 컬럼이 이미 있습니다 — 건너뜀';
GO

-- 순환 조회용 인덱스도 RotateYN 을 포함하도록 다시 만듭니다
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EntranceExamPray_UseYN_SortOrder'
                                       AND object_id = OBJECT_ID('dbo.EntranceExamPray'))
    DROP INDEX [IX_EntranceExamPray_UseYN_SortOrder] ON [dbo].[EntranceExamPray];
GO

CREATE NONCLUSTERED INDEX [IX_EntranceExamPray_Rotate]
    ON [dbo].[EntranceExamPray] ([UseYN] ASC, [RotateYN] ASC, [SortOrder] ASC, [PrayId] ASC);
GO

-- ────────────────────────────────────────────────────────────
--  특정한 날·순간에만 어울리는 기도문을 순환에서 제외
--  (목록에서는 그대로 보이고 직접 골라 읽을 수 있습니다)
-- ────────────────────────────────────────────────────────────
UPDATE dbo.EntranceExamPray
   SET RotateYN = 'N'
 WHERE Title IN (
        N'면접을 앞두고',          -- 면접 당일·전날
        N'발표를 기다리며',        -- 합격자 발표 기간
        N'수능 전날 밤에',         -- 그날 밤
        N'수능 날 아침에',         -- 그날 아침
        N'시험장에 들어서며',      -- 자리에 앉는 순간
        N'시험 도중 막힐 때',      -- 시험 중
        N'시험을 마치고'           -- 시험을 마친 날
       );

PRINT N'순환 제외 표시 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';
GO

-- 결과 확인
SELECT RotateYN,
       COUNT(*) AS cnt
  FROM dbo.EntranceExamPray
 WHERE ISNULL(UseYN,'Y') = 'Y'
 GROUP BY RotateYN;

SELECT SortOrder, Title, RotateYN
  FROM dbo.EntranceExamPray
 WHERE ISNULL(UseYN,'Y') = 'Y'
 ORDER BY RotateYN, SortOrder;
GO

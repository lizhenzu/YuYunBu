-- ============================================================
--  dbo.HealingAndRestorationPray 에 좋아요 수(LikeCount) 추가
--  기존 행은 0 으로 채운다.
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.HealingAndRestorationPray')
      AND name = 'LikeCount')
BEGIN
    ALTER TABLE [dbo].[HealingAndRestorationPray]
        ADD [LikeCount] INT NOT NULL
            CONSTRAINT [DF_HealingAndRestorationPray_LikeCount] DEFAULT (0);

    PRINT '[LikeCount] 컬럼 추가 완료';
END
ELSE
BEGIN
    PRINT '[LikeCount] 이미 있음 — 건너뜀';
END

COMMIT TRAN;
GO

-- 확인
SELECT COUNT(*) AS 전체건수, SUM(LikeCount) AS 좋아요합계
FROM [dbo].[HealingAndRestorationPray];
GO

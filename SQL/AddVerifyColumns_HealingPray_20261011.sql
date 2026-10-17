-- ============================================================
--  치유와 회복 기도문 검증 칸 추가 (2026-10-11)
--  HealingAndRestorationPray 에 IsVerified · VerifiedDT · VerifiedBy
--  · 부부 기도(AddVerifyColumns_BessingCouple_20261011.sql) 와 같은 구조
--  · 이미 있으면 건너뜀 — 여러 번 실행해도 안전합니다.
--  · 기존 기도문은 모두 "검증 전"(IsVerified = 0) 으로 시작합니다.
--  · 실행 후 1분 안에 화면에 검증 버튼 · 표시가 나타납니다 (서버 재시작 불필요).
-- ============================================================
SET NOCOUNT ON;

IF COL_LENGTH('dbo.HealingAndRestorationPray', 'IsVerified') IS NULL
BEGIN
    ALTER TABLE dbo.HealingAndRestorationPray ADD
        IsVerified BIT           NOT NULL CONSTRAINT DF_HealingAndRestorationPray_IsVerified DEFAULT (0),
        VerifiedDT DATETIME      NULL,
        VerifiedBy NVARCHAR(100) NULL;
    PRINT '[HealingAndRestorationPray] 검증 칸 추가';
END
ELSE
    PRINT '[HealingAndRestorationPray] 검증 칸이 이미 있습니다 - 건너뜀';
GO

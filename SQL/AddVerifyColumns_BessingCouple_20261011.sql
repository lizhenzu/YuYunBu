-- ============================================================
--  부부 기도 검증 칸 추가 (2026-10-11)
--  BessingHusband · BessingWife 에 IsVerified · VerifiedDT · VerifiedBy
--  · 이미 있으면 건너뜀 — 여러 번 실행해도 안전합니다.
--  · 기존 기도문은 모두 "검증 전"(IsVerified = 0) 으로 시작합니다.
--  · 실행 후 1분 안에 화면에 검증 버튼 · 표시가 나타납니다 (서버 재시작 불필요).
-- ============================================================
SET NOCOUNT ON;

IF COL_LENGTH('dbo.BessingHusband', 'IsVerified') IS NULL
BEGIN
    ALTER TABLE dbo.BessingHusband ADD
        IsVerified BIT           NOT NULL CONSTRAINT DF_BessingHusband_IsVerified DEFAULT (0),
        VerifiedDT DATETIME      NULL,
        VerifiedBy NVARCHAR(100) NULL;
    PRINT '[BessingHusband] 검증 칸 추가';
END
ELSE
    PRINT '[BessingHusband] 검증 칸이 이미 있습니다 - 건너뜀';

IF COL_LENGTH('dbo.BessingWife', 'IsVerified') IS NULL
BEGIN
    ALTER TABLE dbo.BessingWife ADD
        IsVerified BIT           NOT NULL CONSTRAINT DF_BessingWife_IsVerified DEFAULT (0),
        VerifiedDT DATETIME      NULL,
        VerifiedBy NVARCHAR(100) NULL;
    PRINT '[BessingWife] 검증 칸 추가';
END
ELSE
    PRINT '[BessingWife] 검증 칸이 이미 있습니다 - 건너뜀';
GO
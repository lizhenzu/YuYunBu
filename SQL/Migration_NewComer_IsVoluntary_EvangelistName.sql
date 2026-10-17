-- =====================================================
-- Migration: AttendanceNewComer 에 자진출석/전도자 컬럼 추가
-- 실행 전 반드시 DB 백업 후 적용하세요.
-- =====================================================

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.AttendanceNewComer')
      AND name = 'IsVoluntary'
)
BEGIN
    ALTER TABLE dbo.AttendanceNewComer
    ADD IsVoluntary BIT NOT NULL DEFAULT 1;

    PRINT 'IsVoluntary 컬럼이 추가되었습니다.';
END
ELSE
BEGIN
    PRINT 'IsVoluntary 컬럼이 이미 존재합니다.';
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.AttendanceNewComer')
      AND name = 'EvangelistName'
)
BEGIN
    ALTER TABLE dbo.AttendanceNewComer
    ADD EvangelistName NVARCHAR(100) NULL;

    PRINT 'EvangelistName 컬럼이 추가되었습니다.';
END
ELSE
BEGIN
    PRINT 'EvangelistName 컬럼이 이미 존재합니다.';
END

GO

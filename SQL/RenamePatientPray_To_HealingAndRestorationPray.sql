-- ============================================================
--  dbo.PatientPray  →  dbo.HealingAndRestorationPray 로 이름 변경
--
--  "환자"라는 표현 대신 "치유와 회복"으로 통일하기 위한 개명.
--  데이터는 그대로 두고 이름만 바꾼다 (sp_rename).
--  참조하는 뷰/프로시저가 없음을 확인하고 진행.
--
--  ※ 백업 테이블(PatientPray_bak_v2 / _bak_v3 / _backup_20260827 / _1)은
--    개명 이전 시점의 자료임이 드러나도록 옛 이름 그대로 둔다.
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

IF OBJECT_ID('dbo.PatientPray') IS NOT NULL
   AND OBJECT_ID('dbo.HealingAndRestorationPray') IS NULL
BEGIN
    -- 1) 테이블
    EXEC sp_rename 'dbo.PatientPray', 'HealingAndRestorationPray';

    -- 2) 기본키 (제약 이름은 스키마 없이 'dbo.이름' 형태로 넘긴다)
    EXEC sp_rename 'dbo.PK_PatientPray', 'PK_HealingAndRestorationPray', 'OBJECT';

    -- 3) 기본값 제약
    EXEC sp_rename 'dbo.DF_PatientPray_UseYN',      'DF_HealingAndRestorationPray_UseYN',      'OBJECT';
    EXEC sp_rename 'dbo.DF_PatientPray_CreatedAt',  'DF_HealingAndRestorationPray_CreatedAt',  'OBJECT';
    EXEC sp_rename 'dbo.DF_PatientPray_SortOrder',  'DF_HealingAndRestorationPray_SortOrder',  'OBJECT';
    EXEC sp_rename 'dbo.DF_PatientPray_ClickCount', 'DF_HealingAndRestorationPray_ClickCount', 'OBJECT';

    -- 4) 인덱스 (인덱스는 '테이블.인덱스' 형태 + 'INDEX')
    EXEC sp_rename 'dbo.HealingAndRestorationPray.IX_PatientPray_UseYN_SortOrder',
                   'IX_HealingAndRestorationPray_UseYN_SortOrder', 'INDEX';

    PRINT '[개명] dbo.PatientPray -> dbo.HealingAndRestorationPray 완료';
END
ELSE
BEGIN
    PRINT '[개명] 건너뜀 (이미 바뀌었거나 원본이 없음)';
END

COMMIT TRAN;
GO

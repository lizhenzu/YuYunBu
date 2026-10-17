-- ============================================================
--  DM2HubClickLog  (도전! 성경말씀 허브 — 버전 선택 클릭 기록)
--  /DailyMission2/Welcome 의 "지금 쓰던 버전"(old) · "새 버전"(new) 버튼을 누를 때마다 한 줄
--  Basic · SuperAdmin · Admin · anonymous 계정의 클릭은 저장하지 않는다 (BibleHubClickLog 와 같은 기준)
-- ============================================================
IF NOT EXISTS (
    SELECT 1
    FROM   sys.tables
    WHERE  name = 'DM2HubClickLog'
      AND  schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE [dbo].[DM2HubClickLog] (
        [Id]        INT            IDENTITY(1,1) NOT NULL,
        [Version]   VARCHAR(10)    NOT NULL,          -- 'old' | 'new'
        [UserId]    NVARCHAR(450)  NULL,
        [UserName]  NVARCHAR(256)  NULL,
        [ClickedAt] DATETIME2(7)   NOT NULL,          -- 한국 시간 (DateHelper.GetKoreaNow)

        CONSTRAINT [PK_DM2HubClickLog] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_DM2HubClickLog_Version] CHECK ([Version] IN ('old', 'new'))
    );

    -- 버전별 · 기간별 집계용
    CREATE NONCLUSTERED INDEX [IX_DM2HubClickLog_Version_ClickedAt]
        ON [dbo].[DM2HubClickLog] ([Version] ASC, [ClickedAt] DESC)
        INCLUDE ([UserId]);

    PRINT '[DM2HubClickLog] 테이블 생성 완료';
END
ELSE
BEGIN
    PRINT '[DM2HubClickLog] 테이블이 이미 존재합니다 — 건너뜀';
END
GO

-- ============================================================
--  BibleCompletionLog  (성경 완독 기록 — 몇 번째 완독을 언제 마쳤는지)
--  홈의 "성경 완독을 축하해요!" 세로 타임라인이 이 표의 날짜를 읽는다.
--  · 한 사람이 1독 · 2독 … 마칠 때마다 한 줄
--  · 실제 기록은 완독 처리(/Bible2/ResetProgress) 때 자동으로 쌓인다.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE name = 'BibleCompletionLog' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE [dbo].[BibleCompletionLog] (
        [Id]          INT            IDENTITY(1,1) NOT NULL,
        [UserId]      NVARCHAR(450)  NOT NULL,
        [UserName]    NVARCHAR(256)  NULL,
        [RoundNo]     INT            NOT NULL,      -- 1 = 1독, 2 = 2독 …
        [CompletedAt] DATE           NOT NULL,      -- 완독한 날 (한국 날짜)
        [CreatedAt]   DATETIME2(7)   NOT NULL CONSTRAINT [DF_BibleCompletionLog_CreatedAt] DEFAULT (SYSDATETIME()),
        [IsSeed]      BIT            NOT NULL CONSTRAINT [DF_BibleCompletionLog_IsSeed]    DEFAULT (0),  -- 1 = 가짜(샘플) 데이터

        CONSTRAINT [PK_BibleCompletionLog] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_BibleCompletionLog_User_Round] UNIQUE ([UserId], [RoundNo])
    );

    CREATE NONCLUSTERED INDEX [IX_BibleCompletionLog_User_Round]
        ON [dbo].[BibleCompletionLog] ([UserId] ASC, [RoundNo] ASC)
        INCLUDE ([CompletedAt]);

    PRINT '[BibleCompletionLog] 테이블 생성 완료';
END
ELSE
BEGIN
    PRINT '[BibleCompletionLog] 테이블이 이미 존재합니다 — 건너뜀';
END
GO

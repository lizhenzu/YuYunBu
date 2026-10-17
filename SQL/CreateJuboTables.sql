-- ============================================================
--  YunYunBu2025 — 테이블 생성 스크립트
--  실행 전: USE [YourDatabaseName] 확인
-- ============================================================

-- ────────────────────────────────────────────────────────────
--  BibleSearchLog  (성경 검색어 로그)
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1
    FROM   sys.tables
    WHERE  name = 'BibleSearchLog'
      AND  schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE [dbo].[BibleSearchLog] (
        [Id]         INT            IDENTITY(1,1) NOT NULL,
        [Query]      NVARCHAR(200)  NOT NULL,
        [SearchedAt] DATETIME2(7)   NOT NULL,
        [UserId]     NVARCHAR(450)  NULL,

        CONSTRAINT [PK_BibleSearchLog] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    -- 검색어 순위 집계 쿼리 성능용 인덱스
    CREATE NONCLUSTERED INDEX [IX_BibleSearchLog_Query_SearchedAt]
        ON [dbo].[BibleSearchLog] ([Query] ASC, [SearchedAt] DESC);

    PRINT '[BibleSearchLog] 테이블 생성 완료';
END
ELSE
BEGIN
    PRINT '[BibleSearchLog] 테이블이 이미 존재합니다 — 건너뜀';
END
GO

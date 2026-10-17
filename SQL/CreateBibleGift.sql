-- ============================================================
--  성경통독 완독 선물 — 표 3개
--   1) BibleGiftMilestoneConfig : 몇 독에 얼마짜리 선물인지 기준
--   2) BibleGiftOrder           : 누가 몇 독으로 무엇을 골랐고 구매했는지
--   3) BibleGiftOrderLog        : 고름/바꿈/취소/구매/되돌림 이력 (지우지 않음)
--  · 완독 자격의 근거 = 기존 dbo.BibleCompletionLog
--  · 선물 품목        = 기존 dbo.AwardItem
-- ============================================================
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;   -- 조건부(filtered) 인덱스에 필요

-- ── 1) 기준표 ───────────────────────────────────────────────
IF OBJECT_ID('dbo.BibleGiftMilestoneConfig', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BibleGiftMilestoneConfig
    (
        Id           INT IDENTITY(1,1) NOT NULL,
        cYear        INT            NOT NULL,
        RoundNo      INT            NOT NULL,           -- 몇 독
        Title        NVARCHAR(100)  NOT NULL DEFAULT(N''),
        SubTitle     NVARCHAR(200)  NULL,
        MaxUnitPrice INT            NOT NULL DEFAULT(0),-- 선물 한도 금액(0 = 제한 없음)
        IsActive     BIT            NOT NULL DEFAULT(1),
        SortOrder    INT            NOT NULL DEFAULT(0),
        CreatedAt    DATETIME       NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT PK_BibleGiftMilestoneConfig PRIMARY KEY (Id),
        CONSTRAINT UQ_BibleGiftMilestoneConfig UNIQUE (cYear, RoundNo)
    );
    PRINT '[BibleGiftMilestoneConfig] 만듦';
END
ELSE PRINT '[BibleGiftMilestoneConfig] 이미 있음';

-- ── 2) 선택·구매 본표 ──────────────────────────────────────
IF OBJECT_ID('dbo.BibleGiftOrder', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BibleGiftOrder
    (
        OrderId           INT IDENTITY(1,1) NOT NULL,
        UserId            NVARCHAR(450)  NOT NULL,
        UserName          NVARCHAR(256)  NULL,
        RoundNo           INT            NOT NULL,
        ConfigId          INT            NULL,
        ProductID         INT            NOT NULL,
        UnitPriceSnapshot INT            NOT NULL DEFAULT(0),
        Status            NVARCHAR(20)   NOT NULL DEFAULT(N'DRAFT'),  -- DRAFT / PURCHASED / CANCELED
        IsCurrentDraft    BIT            NOT NULL DEFAULT(1),
        SelectedAt        DATETIME       NULL,
        PurchasedAt       DATETIME       NULL,
        AdminId           NVARCHAR(100)  NULL,
        Memo              NVARCHAR(200)  NULL,
        CreatedAt         DATETIME       NOT NULL DEFAULT(GETDATE()),
        UpdatedAt         DATETIME       NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT PK_BibleGiftOrder PRIMARY KEY (OrderId)
    );

    PRINT '[BibleGiftOrder] 만듦';
END
ELSE PRINT '[BibleGiftOrder] 이미 있음';

-- (사람, 회차)당 살아 있는 선택은 하나만
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_BibleGiftOrder_Current' AND object_id = OBJECT_ID('dbo.BibleGiftOrder'))
    CREATE UNIQUE INDEX UX_BibleGiftOrder_Current
        ON dbo.BibleGiftOrder (UserId, RoundNo)
        WHERE IsCurrentDraft = 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BibleGiftOrder_Status' AND object_id = OBJECT_ID('dbo.BibleGiftOrder'))
    CREATE INDEX IX_BibleGiftOrder_Status ON dbo.BibleGiftOrder (Status, RoundNo);

-- ── 3) 이력표 ──────────────────────────────────────────────
IF OBJECT_ID('dbo.BibleGiftOrderLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BibleGiftOrderLog
    (
        LogId     BIGINT IDENTITY(1,1) NOT NULL,
        OrderId   INT            NULL,
        UserId    NVARCHAR(450)  NOT NULL,
        UserName  NVARCHAR(256)  NULL,
        RoundNo   INT            NOT NULL,
        Action    NVARCHAR(20)   NOT NULL,   -- SELECT / CHANGE / CANCEL / PURCHASE / UNDO
        ProductID INT            NULL,
        UnitPrice INT            NULL,
        ActorId   NVARCHAR(256)  NULL,
        ActorRole NVARCHAR(20)   NULL,       -- USER / ADMIN
        CreatedAt DATETIME       NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT PK_BibleGiftOrderLog PRIMARY KEY (LogId)
    );
    PRINT '[BibleGiftOrderLog] 만듦';
END
ELSE PRINT '[BibleGiftOrderLog] 이미 있음';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BibleGiftOrderLog_User' AND object_id = OBJECT_ID('dbo.BibleGiftOrderLog'))
    CREATE INDEX IX_BibleGiftOrderLog_User ON dbo.BibleGiftOrderLog (UserId, RoundNo, LogId);

-- ── 기본 기준 넣기 (1~10독, 한도 15,000원) ────────────────
DECLARE @y INT = YEAR(GETDATE());
;WITH n AS (SELECT * FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10)) AS t(RoundNo))
INSERT INTO dbo.BibleGiftMilestoneConfig (cYear, RoundNo, Title, SubTitle, MaxUnitPrice, IsActive, SortOrder)
SELECT @y,
       n.RoundNo,
       CONCAT(N'성경 ', n.RoundNo, N'독을 마쳤어요, 축하해요!'),
       N'선물을 골라 주세요',
       15000, 1, n.RoundNo
FROM   n
WHERE  NOT EXISTS (SELECT 1 FROM dbo.BibleGiftMilestoneConfig c
                   WHERE c.cYear = @y AND c.RoundNo = n.RoundNo);

PRINT CONCAT('[BibleGiftMilestoneConfig] 기준 ', @@ROWCOUNT, '줄 넣음');

SELECT * FROM dbo.BibleGiftMilestoneConfig ORDER BY cYear, RoundNo;
GO

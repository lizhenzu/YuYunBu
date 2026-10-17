-- ══════════════════════════════════════════════════════════════
--  추석맞이 우리 가족 사진 콘테스트 — 표 4개
--  (앱이 처음 켜질 때 FamilyPhotoService.EnsureTablesAsync 가 같은 내용을 자동으로 실행한다.
--   손으로 만들고 싶으면 이 스크립트를 그대로 돌리면 된다.  여러 번 돌려도 안전.)
-- ══════════════════════════════════════════════════════════════

IF OBJECT_ID(N'dbo.FamilyPhotoEntry', N'U') IS NULL
CREATE TABLE dbo.FamilyPhotoEntry (
    EntryId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FamilyPhotoEntry PRIMARY KEY,
    FamilyName    NVARCHAR(40)  NOT NULL,
    FamilyNameKey NVARCHAR(40)  NOT NULL,
    PinHash       NVARCHAR(128) NOT NULL,
    PinSalt       NVARCHAR(32)  NOT NULL,
    IsHidden      BIT           NOT NULL CONSTRAINT DF_FamilyPhotoEntry_IsHidden DEFAULT(0),
    HiddenBy      NVARCHAR(100) NULL,
    HiddenAt      DATETIME2     NULL,
    CreatedIp     NVARCHAR(64)  NULL,
    CreatedAt     DATETIME2     NOT NULL,
    UpdatedAt     DATETIME2     NOT NULL
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_FamilyPhotoEntry_NameKey')
    CREATE UNIQUE INDEX UX_FamilyPhotoEntry_NameKey ON dbo.FamilyPhotoEntry (FamilyNameKey);

IF OBJECT_ID(N'dbo.FamilyPhotoItem', N'U') IS NULL
CREATE TABLE dbo.FamilyPhotoItem (
    PhotoId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FamilyPhotoItem PRIMARY KEY,
    EntryId    INT           NOT NULL,
    Slot       INT           NOT NULL,
    Caption    NVARCHAR(80)  NULL,
    FilePath   NVARCHAR(300) NOT NULL,
    ThumbPath  NVARCHAR(300) NOT NULL,
    Width      INT           NOT NULL CONSTRAINT DF_FamilyPhotoItem_W DEFAULT(0),
    Height     INT           NOT NULL CONSTRAINT DF_FamilyPhotoItem_H DEFAULT(0),
    IsHidden   BIT           NOT NULL CONSTRAINT DF_FamilyPhotoItem_IsHidden DEFAULT(0),
    HiddenBy   NVARCHAR(100) NULL,
    HiddenAt   DATETIME2     NULL,
    CreatedAt  DATETIME2     NOT NULL,
    UpdatedAt  DATETIME2     NOT NULL
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_FamilyPhotoItem_EntrySlot')
    CREATE UNIQUE INDEX UX_FamilyPhotoItem_EntrySlot ON dbo.FamilyPhotoItem (EntryId, Slot);

IF OBJECT_ID(N'dbo.FamilyPhotoLike', N'U') IS NULL
CREATE TABLE dbo.FamilyPhotoLike (
    LikeId     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FamilyPhotoLike PRIMARY KEY,
    PhotoId    INT          NOT NULL,
    DeviceKey  NVARCHAR(64) NOT NULL,
    Ip         NVARCHAR(64) NULL,
    CreatedAt  DATETIME2    NOT NULL
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_FamilyPhotoLike_PhotoDevice')
    CREATE UNIQUE INDEX UX_FamilyPhotoLike_PhotoDevice ON dbo.FamilyPhotoLike (PhotoId, DeviceKey);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_FamilyPhotoLike_PhotoIp')
    CREATE INDEX IX_FamilyPhotoLike_PhotoIp ON dbo.FamilyPhotoLike (PhotoId, Ip);

IF OBJECT_ID(N'dbo.FamilyPhotoContest', N'U') IS NULL
CREATE TABLE dbo.FamilyPhotoContest (
    ContestId    INT           NOT NULL CONSTRAINT PK_FamilyPhotoContest PRIMARY KEY,
    IsPublished  BIT           NOT NULL CONSTRAINT DF_FamilyPhotoContest_Pub DEFAULT(0),
    PublishedAt  DATETIME2     NULL,
    PublishedBy  NVARCHAR(100) NULL
);
IF NOT EXISTS (SELECT 1 FROM dbo.FamilyPhotoContest WHERE ContestId = 1)
    INSERT INTO dbo.FamilyPhotoContest (ContestId, IsPublished) VALUES (1, 0);

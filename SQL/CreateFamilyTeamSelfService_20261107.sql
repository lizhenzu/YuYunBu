-- ============================================================
-- Family team self service (2026-11-07)
--   * dbo.DM2_FamilyRequest : pending join requests (Kind='R') and invites (Kind='I')
--   * dbo.DM2_FamilyNotice  : one-time "you are now a team member" notice
--   Works on SQL Server 2014. Safe to run more than once.
--   Existing tables (DM2_FamilyTeam / DM2_FamilyMember) are NOT changed.
-- ============================================================

IF OBJECT_ID(N'dbo.DM2_FamilyRequest', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_FamilyRequest
    (
        Id               INT IDENTITY(1,1) NOT NULL,
        FamilyTeamId     INT           NOT NULL,
        Kind             CHAR(1)       NOT NULL,              -- 'R' = join request (target = applicant), 'I' = invite (target = invited person)
        TargetUserName   NVARCHAR(200) NOT NULL,
        FromUserName     NVARCHAR(200) NOT NULL,              -- who sent it
        RelationKey      VARCHAR(10)   NOT NULL,              -- dad / mom / child / grandpa / grandma / etc
        CreatedAt        DATETIME      NOT NULL CONSTRAINT DF_DM2_FamilyRequest_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT PK_DM2_FamilyRequest PRIMARY KEY (Id),
        CONSTRAINT CK_DM2_FamilyRequest_Kind CHECK (Kind IN ('R', 'I')),
        CONSTRAINT FK_DM2_FamilyRequest_Team FOREIGN KEY (FamilyTeamId)
            REFERENCES dbo.DM2_FamilyTeam (FamilyTeamId) ON DELETE CASCADE,
        CONSTRAINT UX_DM2_FamilyRequest_Team_Kind_Target UNIQUE (FamilyTeamId, Kind, TargetUserName)
    );
    CREATE INDEX IX_DM2_FamilyRequest_Target ON dbo.DM2_FamilyRequest (TargetUserName, Kind);
    CREATE INDEX IX_DM2_FamilyRequest_From   ON dbo.DM2_FamilyRequest (FromUserName, Kind);
END
GO

IF OBJECT_ID(N'dbo.DM2_FamilyNotice', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DM2_FamilyNotice
    (
        Id           INT IDENTITY(1,1) NOT NULL,
        UserName     NVARCHAR(200) NOT NULL,
        FamilyTeamId INT           NOT NULL,
        TeamName     NVARCHAR(100) NOT NULL,
        Kind         VARCHAR(10)   NOT NULL CONSTRAINT DF_DM2_FamilyNotice_Kind DEFAULT ('JOINED'),
        CreatedAt    DATETIME      NOT NULL CONSTRAINT DF_DM2_FamilyNotice_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT PK_DM2_FamilyNotice PRIMARY KEY (Id)
    );
    CREATE INDEX IX_DM2_FamilyNotice_User ON dbo.DM2_FamilyNotice (UserName);
END
GO

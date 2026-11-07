-- ============================================================
-- Family team award key / creator (2026-11-08)
--   * AwardKey        : fixed internal key used by award records (gift order Memo).
--                       Existing teams keep their current name as the key, so existing award records keep matching.
--                       It never changes when the team is renamed.
--   * CreatorUserName : who made the team. Existing teams = earliest registered active member.
--   Works on SQL Server 2014. Safe to run more than once.
--   RUN THIS BEFORE PUBLISHING THE NEW CODE.
-- ============================================================
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF COL_LENGTH('dbo.DM2_FamilyTeam', 'AwardKey') IS NULL
    ALTER TABLE dbo.DM2_FamilyTeam ADD AwardKey NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.DM2_FamilyTeam', 'CreatorUserName') IS NULL
    ALTER TABLE dbo.DM2_FamilyTeam ADD CreatorUserName NVARCHAR(200) NULL;
GO

UPDATE dbo.DM2_FamilyTeam SET AwardKey = TeamName WHERE AwardKey IS NULL;

UPDATE t
SET CreatorUserName = (SELECT TOP 1 m.UserName FROM dbo.DM2_FamilyMember m
                       WHERE m.FamilyTeamId = t.FamilyTeamId AND m.IsActive = 1
                       ORDER BY m.Id)
FROM dbo.DM2_FamilyTeam t
WHERE t.CreatorUserName IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_DM2_FamilyTeam_AwardKey' AND object_id = OBJECT_ID('dbo.DM2_FamilyTeam'))
   AND NOT EXISTS (SELECT AwardKey FROM dbo.DM2_FamilyTeam WHERE AwardKey IS NOT NULL GROUP BY AwardKey HAVING COUNT(*) > 1)
    CREATE UNIQUE INDEX UX_DM2_FamilyTeam_AwardKey ON dbo.DM2_FamilyTeam (AwardKey) WHERE AwardKey IS NOT NULL;
GO

-- Check: teams without a creator (no active member), and duplicate keys (index was skipped if any)
SELECT FamilyTeamId, TeamName FROM dbo.DM2_FamilyTeam WHERE CreatorUserName IS NULL;
SELECT AwardKey, COUNT(*) AS Cnt FROM dbo.DM2_FamilyTeam GROUP BY AwardKey HAVING COUNT(*) > 1;
GO

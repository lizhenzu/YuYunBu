-- ============================================================
-- Friend team (2026-11-08)
--   * TeamType: 'FAMILY' (existing teams) or 'FRIEND' (at most 2 students)
--   Gift records for friend teams are stored in StudentPointGiftOrder with Memo = <prefix>:{AwardKey}:{threshold},
--   one row per person, so no gift table changes are needed.
--   Works on SQL Server 2014. Safe to run more than once.
--   RUN THIS BEFORE PUBLISHING THE NEW CODE.
-- ============================================================
SET QUOTED_IDENTIFIER ON;   -- the filtered index on this table requires it for ALTER TABLE
SET ANSI_NULLS ON;
GO

IF COL_LENGTH('dbo.DM2_FamilyTeam', 'TeamType') IS NULL
    ALTER TABLE dbo.DM2_FamilyTeam
        ADD TeamType VARCHAR(10) NOT NULL CONSTRAINT DF_DM2_FamilyTeam_TeamType DEFAULT ('FAMILY');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_DM2_FamilyTeam_TeamType')
    ALTER TABLE dbo.DM2_FamilyTeam
        ADD CONSTRAINT CK_DM2_FamilyTeam_TeamType CHECK (TeamType IN ('FAMILY', 'FRIEND'));
GO

-- Check: every existing team is a family team
SELECT TeamType, COUNT(*) AS Cnt FROM dbo.DM2_FamilyTeam GROUP BY TeamType;
GO

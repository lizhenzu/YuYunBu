/* ==========================================================================
   DropTeamSetting_20261109.sql
   The award-lock on/off switch was removed (the lock is always on for a team that has an award row).
   This drops the unused settings table that TeamAward_20261109.sql created earlier.
   Safe to run more than once. Old deployed code treats a missing table as "lock on", so it can run before or after deploy.
   ========================================================================== */
SET NOCOUNT ON;
GO
IF OBJECT_ID(N'dbo.DM2_TeamSetting') IS NOT NULL
    DROP TABLE dbo.DM2_TeamSetting;
GO
SELECT CASE WHEN OBJECT_ID(N'dbo.DM2_TeamSetting') IS NULL THEN N'dropped' ELSE N'still there' END AS DM2_TeamSetting;
GO

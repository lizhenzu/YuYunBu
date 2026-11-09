/* ==========================================================================
   RemoveDept_20261109.sql  -- remove department (DeptCode) from the DB
   Run AFTER:
     1) SQL/DeptToEtc_20261109.sql   (all rows must be D1001)
     2) the new site version is published (code no longer reads DeptCode)
   What it does
     - backups  : *_bak_20261109 tables (dropped tables, DeptTeacherMap_2026, CodeData D001)
     - drops    : unused old objects that still carry DeptCode
                  view_UserCompositeInfo_2025, view_UserCompositeInfo_2026_N, view_UserCompositeInfo_2026old,
                  GetThisWeekRewarding____, GetThisWeekRewarding2__, GetPointByDate (already broken),
                  GetThisWeekRewardingP (replaced by GetThisWeekRewarding())
     - views    : view_All_Person_2026, view_UserCompositeInfo_2026, view_Ranking_By_Person,
                  view_Ranking_By_Person_ext, view_all_point_info  -> no DeptCode / DeptNM
     - function : dbo.GetThisWeekRewarding()  (no parameter)
     - columns  : DeptRoleUserAssoc.DeptCode, DeptTeacherMap_2026.DeptCode
     - tables   : DeptInfo, DeptGroupInfo, DeptGroupInfo2, ChurchDepartments (+ 2 FKs that point to it)
     - codes    : CodeData MajorCD = 'D001'
   Old definitions are kept in SQL/RemoveDept_20261109_old_definitions.sql
   ========================================================================== */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- 0) guard: step 1 must be done
IF OBJECT_ID(N'dbo.DeptRoleUserAssoc') IS NOT NULL
   AND COL_LENGTH(N'dbo.DeptRoleUserAssoc', N'DeptCode') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.DeptRoleUserAssoc WHERE ISNULL(DeptCode, N'D1001') <> N'D1001')
BEGIN
    RAISERROR(N'Run SQL/DeptToEtc_20261109.sql first. Nothing was changed.', 16, 1);
    SET NOEXEC ON;
END
GO

-- 1) backups
IF OBJECT_ID(N'dbo.DeptInfo') IS NOT NULL AND OBJECT_ID(N'dbo.DeptInfo_bak_20261109') IS NULL
    SELECT * INTO dbo.DeptInfo_bak_20261109 FROM dbo.DeptInfo;
IF OBJECT_ID(N'dbo.DeptGroupInfo') IS NOT NULL AND OBJECT_ID(N'dbo.DeptGroupInfo_bak_20261109') IS NULL
    SELECT * INTO dbo.DeptGroupInfo_bak_20261109 FROM dbo.DeptGroupInfo;
IF OBJECT_ID(N'dbo.DeptGroupInfo2') IS NOT NULL AND OBJECT_ID(N'dbo.DeptGroupInfo2_bak_20261109') IS NULL
    SELECT * INTO dbo.DeptGroupInfo2_bak_20261109 FROM dbo.DeptGroupInfo2;
IF OBJECT_ID(N'dbo.ChurchDepartments') IS NOT NULL AND OBJECT_ID(N'dbo.ChurchDepartments_bak_20261109') IS NULL
    SELECT * INTO dbo.ChurchDepartments_bak_20261109 FROM dbo.ChurchDepartments;
IF OBJECT_ID(N'dbo.DeptTeacherMap_2026_bak_20261109') IS NULL
    SELECT * INTO dbo.DeptTeacherMap_2026_bak_20261109 FROM dbo.DeptTeacherMap_2026;
IF OBJECT_ID(N'dbo.CodeData_D001_bak_20261109') IS NULL
    SELECT * INTO dbo.CodeData_D001_bak_20261109 FROM dbo.CodeData WHERE MajorCD = N'D001';
IF OBJECT_ID(N'dbo.DeptRoleUserAssoc_bak_20261109') IS NULL
    SELECT * INTO dbo.DeptRoleUserAssoc_bak_20261109 FROM dbo.DeptRoleUserAssoc;
GO

-- 2) old objects nobody uses (they still read DeptCode)
IF OBJECT_ID(N'dbo.GetThisWeekRewarding2__') IS NOT NULL DROP FUNCTION dbo.GetThisWeekRewarding2__;
IF OBJECT_ID(N'dbo.GetThisWeekRewarding____') IS NOT NULL DROP FUNCTION dbo.GetThisWeekRewarding____;
IF OBJECT_ID(N'dbo.GetThisWeekRewardingP') IS NOT NULL DROP FUNCTION dbo.GetThisWeekRewardingP;
IF OBJECT_ID(N'dbo.GetPointByDate') IS NOT NULL DROP FUNCTION dbo.GetPointByDate;
IF OBJECT_ID(N'dbo.view_UserCompositeInfo_2025') IS NOT NULL DROP VIEW dbo.view_UserCompositeInfo_2025;
IF OBJECT_ID(N'dbo.view_UserCompositeInfo_2026_N') IS NOT NULL DROP VIEW dbo.view_UserCompositeInfo_2026_N;
IF OBJECT_ID(N'dbo.view_UserCompositeInfo_2026old') IS NOT NULL DROP VIEW dbo.view_UserCompositeInfo_2026old;
GO

-- 3) views without DeptCode / DeptNM
ALTER VIEW [dbo].[view_All_Person_2026] AS
SELECT 'T1001' RoleCode, b.userid, a.personid, fullName FROM [dbo].[ClassInfo_2026] A JOIN [dbo].[view_PersonMaster_2026] b ON a.personid = b.personid
UNION
SELECT 'T1002' RoleCode, b.userid, b.personid, fullName FROM [dbo].[ClassTeacherMap_2026] A JOIN [dbo].[view_PersonMaster_2026] b ON a.Teacherpersonid = b.personid
UNION
SELECT 'T1002' RoleCode, b.userid, b.personid, fullName FROM [dbo].[ClassTeacherMap_2026] A JOIN [dbo].[view_PersonMaster_2026] b ON a.SubTeacherPersonId = b.personid
UNION
SELECT RoleCode, b.userid, a.personid, fullName FROM [dbo].[DeptTeacherMap_2026] A JOIN [dbo].[view_PersonMaster_2026] b ON a.personid = b.personid
GO

ALTER VIEW [dbo].[view_UserCompositeInfo_2026] AS
SELECT
    IDA.UserId                         AS UserId,
    LastName,
    FirstName,
    IDA.FullName,
    UserName,
    ISNULL(bMonth,'')                  AS bMonth,
    ISNULL(bDay,'')                    AS bDay,
    ISNULL([birthDay_PublishYN],'')    AS [birthDay_PublishYN],
    email,
    ISNULL(PhoneNumber,'')             AS PhoneNumber,
    ISNULL(PhoneNumber2,'')            AS PhoneNumber2,
    ISNULL(HomeAaddress,'')            AS HomeAaddress,
    ISNULL(school,'')                  AS school,
    ISNULL(sex,'')                     AS sex,
    ISNULL([readBibleAllCount],0)      AS [readBibleAllCount],
    ClassYear                          AS ClassYear,
    ClassBan                           AS ClassBan,
    ISNULL(IDA.PersonId, 0)            AS PersonId,
    ISNULL(RoleCode, '')               AS RoleCode,
    (SELECT MinorNM FROM [dbo].[CodeData] WHERE MajorCD = 'T001' AND MinorCD = RoleCode) AS RoleNM,
    ''                                 AS ProfilePicture
FROM            [dbo].[view_PersonMaster_2026] IDA
LEFT OUTER JOIN [dbo].[view_All_Person_2026]  DRUA ON IDA.PersonId = DRUA.PersonId
LEFT OUTER JOIN [dbo].[ClassInfo_2026]        CI   ON IDA.PersonId = CI.PersonId
GO

ALTER VIEW [dbo].[view_Ranking_By_Person] AS
SELECT DC, userName, [LastName], [FirstName], FullName, RoleCode, RoleNM, ClassYear, ClassBan, minclickDate, maxclickDate, UserId
FROM (
    SELECT data.userName, TotalDC DC, [LastName], [FirstName], FullName,
           ISNULL(RoleCode,'') RoleCode, ISNULL(RoleNM,'') RoleNM,
           ISNULL(ClassYear,0) ClassYear, ISNULL(ClassBan,0) ClassBan,
           FirstClickDate minclickDate, LastClickDate maxclickDate, UserId
    FROM view_Total_Summary_Point data
    JOIN [dbo].[view_UserCompositeInfo_2026] US ON data.userName = US.userName
) data
GO

ALTER VIEW [dbo].[view_Ranking_By_Person_ext] AS
SELECT UserId, MA.userName, MA.FullName, (DC - ISNULL(AD.AwardLevel, 0)) DC, DC DC_Total,
       (ISNULL(Total, 0) + ISNULL(SDC,0)) Total, ISNULL(SDC,0) SDC, MA.ClassYear, MA.ClassBan, RoleCode, RoleNM,
       ISNULL(AD.AwardLevel, 0) AwardLevel, ISNULL(AwardLevel1000Value, 0) AwardLevel1000Value,
       [minclickDate], [maxclickDate], MA.[LastName], MA.[FirstName]
FROM [dbo].view_Ranking_By_Person MA
LEFT OUTER JOIN [dbo].[awardData] AD ON MA.userName = AD.userName
LEFT OUTER JOIN [dbo].[awardDataUnit1000] AD1000 ON MA.UserId = AD1000.Award1000UserId
LEFT OUTER JOIN dbo.ufn_UserAreaPointsByDate(NULL, NULL) UAP ON MA.userName = UAP.userName
LEFT OUTER JOIN (
    SELECT username AS userName, SUM(point) AS SDC, MIN(clickDate) AS A, MAX(clickDate) AS B
    FROM ClickHistoryForPoint
    GROUP BY username
) UAP1 ON MA.userName = UAP1.userName
WHERE MA.userName NOT LIKE '%test%'
GO

ALTER VIEW [dbo].[view_all_point_info] AS
SELECT *
FROM (
    SELECT MA.UserId, MA.userName, ((DC_Total + SDC) - ISNULL(AD.AwardLevel, 0)) DC, (DC_Total + SDC) DC_Total, SDC,
           ISNULL(AD.AwardLevel, 0) AwardLevel, MA.[LastName], MA.[FirstName], MA.FullName, MA.ClassYear, MA.ClassBan,
           RoleCode, RoleNM, [minclickDate], [maxclickDate], ISNULL(AP.ProductID, 0) ProductID, ISNULL(AP.PickID, 0) PickID
    FROM [dbo].[view_Ranking_By_Person_ext] MA
    LEFT OUTER JOIN [dbo].[awardData]     AD ON MA.userName = AD.userName
    LEFT OUTER JOIN [dbo].[AwardItemPick] AP ON MA.UserId   = AP.UserId
    WHERE 1 = 1
) data
GO

-- 4) this-week reward list without a department parameter (replaces GetThisWeekRewardingP)
IF OBJECT_ID(N'dbo.GetThisWeekRewarding') IS NOT NULL DROP FUNCTION dbo.GetThisWeekRewarding;
GO
CREATE FUNCTION [dbo].[GetThisWeekRewarding]()
RETURNS TABLE
AS
RETURN
(
    SELECT ROW_NUMBER() OVER (ORDER BY DC DESC) AS sequenceNo, *, '' AS ProfilePicturePath
    FROM (
        SELECT DC, DC_Total, (DC_Total + SDC) Total, SDC,
               MA.userName, MA.LastName, MA.FirstName, MA.FullName, MA.ClassYear, MA.ClassBan,
               MA.RoleNM, MA.RoleCode, minclickDate, maxclickDate, AwardLevel, MA.UserId,
               0 AS AwardLevel1000Value
        FROM [dbo].view_Ranking_By_Person_ext MA
    ) data
    WHERE (AwardLevel = 0    AND (DC_Total + SDC) >= 100)
       OR (AwardLevel = 100  AND (DC_Total + SDC) >= 400)
       OR (AwardLevel = 400  AND (DC_Total + SDC) >= 700)
       OR (AwardLevel = 700  AND (DC_Total + SDC) >= 1000)
       OR (AwardLevel = 1000 AND (DC_Total + SDC) >= 1500)
);
GO

-- 5) department columns
IF COL_LENGTH(N'dbo.DeptRoleUserAssoc', N'DeptCode') IS NOT NULL
    ALTER TABLE dbo.DeptRoleUserAssoc DROP COLUMN DeptCode;
IF COL_LENGTH(N'dbo.DeptTeacherMap_2026', N'DeptCode') IS NOT NULL
    ALTER TABLE dbo.DeptTeacherMap_2026 DROP COLUMN DeptCode;
GO

-- 6) department tables (ChurchDepartments: drop the two FKs that point to it first)
IF OBJECT_ID(N'dbo.FK_Bulletins_ChurchDepartments_DepartmentId', N'F') IS NOT NULL
    ALTER TABLE dbo.Bulletins DROP CONSTRAINT FK_Bulletins_ChurchDepartments_DepartmentId;
IF OBJECT_ID(N'dbo.FK_BirthdayRecords_ChurchDepartments_DepartmentId', N'F') IS NOT NULL
    ALTER TABLE dbo.BirthdayRecords DROP CONSTRAINT FK_BirthdayRecords_ChurchDepartments_DepartmentId;
IF OBJECT_ID(N'dbo.ChurchDepartments') IS NOT NULL DROP TABLE dbo.ChurchDepartments;
IF OBJECT_ID(N'dbo.DeptInfo') IS NOT NULL DROP TABLE dbo.DeptInfo;
IF OBJECT_ID(N'dbo.DeptGroupInfo') IS NOT NULL DROP TABLE dbo.DeptGroupInfo;
IF OBJECT_ID(N'dbo.DeptGroupInfo2') IS NOT NULL DROP TABLE dbo.DeptGroupInfo2;
GO

-- 7) department codes
DELETE FROM dbo.CodeData WHERE MajorCD = N'D001';
GO

-- 8) refresh objects that read the changed views
EXEC sp_refreshview N'dbo.view_AwardPayouts_WithTotal';
EXEC sp_refreshsqlmodule N'dbo.GetVisitInfo';
GO

-- check: every line should be 0
SELECT 'dept columns left' AS k, COUNT(*) AS n FROM sys.columns c JOIN sys.objects o ON o.object_id = c.object_id
 WHERE c.name LIKE '%Dept%' AND o.is_ms_shipped = 0 AND o.name NOT LIKE '%[_]bak[_]20261109'
UNION ALL
SELECT 'D1001 in modules', COUNT(*) FROM sys.sql_modules WHERE definition LIKE '%D1001%'
UNION ALL
SELECT 'dept tables left', COUNT(*) FROM sys.tables WHERE name IN ('DeptInfo','DeptGroupInfo','DeptGroupInfo2','ChurchDepartments')
UNION ALL
SELECT 'D001 codes left', COUNT(*) FROM dbo.CodeData WHERE MajorCD = N'D001';
GO
SET NOEXEC OFF;
GO

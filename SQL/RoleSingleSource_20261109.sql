/* ==========================================================================
   RoleSingleSource_20261109.sql
   Role is read from ONE place only: dbo.DeptRoleUserAssoc (one row per user).
   Class assignment / teacher assignment no longer decide the role.

   What this script does
     1) backup  : DeptRoleUserAssoc_bak_20261109r
     2) copy    : every user found in view_All_Person_2026 gets that role in DeptRoleUserAssoc
                  (same result the screens showed until now: assignment role wins)
     3) view    : view_UserCompositeInfo_2026.RoleCode / RoleNM now come from DeptRoleUserAssoc

   Run order : run this script BEFORE deploying the new code.
               (old code keeps working after this script: both sources agree)
   Rollback  : restore DeptRoleUserAssoc from the _bak table and ALTER the view back
               (old view definition is kept in SQL/RoleSingleSource_20261109_old_definitions.sql)
   ========================================================================== */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- 1) backup
IF OBJECT_ID(N'dbo.DeptRoleUserAssoc_bak_20261109r') IS NULL
    SELECT * INTO dbo.DeptRoleUserAssoc_bak_20261109r FROM dbo.DeptRoleUserAssoc;
GO

-- 2) copy the assignment role into DeptRoleUserAssoc (one role per user: lowest RoleCode, like the old reader)
;WITH r AS (
    SELECT ap.userid AS UserId,
           MIN(LTRIM(RTRIM(ap.RoleCode))) AS RoleCode
    FROM dbo.view_All_Person_2026 ap
    WHERE ap.userid IS NOT NULL AND ISNULL(ap.RoleCode, N'') <> N''
    GROUP BY ap.userid
)
UPDATE d SET d.RoleCode = r.RoleCode, d.updateDT = GETDATE()
FROM dbo.DeptRoleUserAssoc d
JOIN r ON r.UserId = d.UserId
WHERE ISNULL(d.RoleCode, N'') <> r.RoleCode;
GO

;WITH r AS (
    SELECT ap.userid AS UserId,
           MIN(LTRIM(RTRIM(ap.RoleCode))) AS RoleCode
    FROM dbo.view_All_Person_2026 ap
    WHERE ap.userid IS NOT NULL AND ISNULL(ap.RoleCode, N'') <> N''
    GROUP BY ap.userid
)
INSERT INTO dbo.DeptRoleUserAssoc (UserId, RoleCode, InsertDT, updateDT)
SELECT r.UserId, r.RoleCode, GETDATE(), GETDATE()
FROM r
WHERE NOT EXISTS (SELECT 1 FROM dbo.DeptRoleUserAssoc d WHERE d.UserId = r.UserId);
GO

-- 3) view: role from DeptRoleUserAssoc
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
    ISNULL(DR.RoleCode, '')            AS RoleCode,
    (SELECT MinorNM FROM [dbo].[CodeData] WHERE MajorCD = 'T001' AND MinorCD = DR.RoleCode) AS RoleNM,
    ''                                 AS ProfilePicture
FROM            [dbo].[view_PersonMaster_2026] IDA
LEFT OUTER JOIN [dbo].[DeptRoleUserAssoc]     DR   ON IDA.UserId   = DR.UserId
LEFT OUTER JOIN [dbo].[ClassInfo_2026]        CI   ON IDA.PersonId = CI.PersonId
GO

-- 4) refresh dependent views
DECLARE @v NVARCHAR(300);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR
    SELECT QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name) FROM sys.views WHERE is_ms_shipped = 0;
OPEN c; FETCH NEXT FROM c INTO @v;
WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY EXEC sp_refreshview @v; END TRY BEGIN CATCH PRINT 'skip ' + @v + ' : ' + ERROR_MESSAGE(); END CATCH
    FETCH NEXT FROM c INTO @v;
END
CLOSE c; DEALLOCATE c;
GO

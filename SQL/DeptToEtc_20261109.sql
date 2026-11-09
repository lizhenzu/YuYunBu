/* ==========================================================================
   DeptToEtc_20261109.sql
   Members outside D1001 (D1002 / D1099) -> role forced to Etc (T1099), dept D1001.
   - Backup first: DeptRoleUserAssoc_bak_20261109, ClassInfo_2026_bak_20261109
   - Rows whose login account no longer exists are deleted (4 rows)
   - Class assignment of those members is removed (Etc has no class) (1 row)
   - Remaining members: DeptCode = D1001, RoleCode = T1099 (29 rows)
   Run once. Safe to re-run: second run finds nothing to change.
   ========================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.DeptRoleUserAssoc_bak_20261109') IS NULL
    SELECT * INTO dbo.DeptRoleUserAssoc_bak_20261109 FROM dbo.DeptRoleUserAssoc;

IF OBJECT_ID(N'dbo.ClassInfo_2026_bak_20261109') IS NULL
    SELECT * INTO dbo.ClassInfo_2026_bak_20261109 FROM dbo.ClassInfo_2026;

BEGIN TRAN;

    -- 1) rows without a login account
    DELETE d
    FROM dbo.DeptRoleUserAssoc d
    WHERE d.DeptCode <> N'D1001'
      AND NOT EXISTS (SELECT 1 FROM [Identity].[User] u WHERE u.Id = d.UserId);
    PRINT 'deleted orphan rows: ' + CAST(@@ROWCOUNT AS varchar(10));

    -- 2) class assignment of the members who become Etc
    DELETE ci
    FROM dbo.ClassInfo_2026 ci
    JOIN dbo.view_PersonMaster_2026 pm ON pm.PersonId = ci.PersonID
    JOIN dbo.DeptRoleUserAssoc d ON d.UserId = pm.UserId
    WHERE d.DeptCode <> N'D1001';
    PRINT 'deleted class rows: ' + CAST(@@ROWCOUNT AS varchar(10));

    -- 3) force role Etc (T1099), dept D1001
    UPDATE dbo.DeptRoleUserAssoc
    SET DeptCode = N'D1001', RoleCode = N'T1099'
    WHERE DeptCode <> N'D1001';
    PRINT 'changed to Etc: ' + CAST(@@ROWCOUNT AS varchar(10));

COMMIT;

-- check: should be one row, D1001 only
SELECT DeptCode, COUNT(*) AS n FROM dbo.DeptRoleUserAssoc GROUP BY DeptCode;

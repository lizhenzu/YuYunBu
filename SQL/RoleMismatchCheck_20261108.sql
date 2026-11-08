-- 역할 기준 점검 (2026-11-08, docs/역할기준_설계서.md 기준 4) — 조회만 한다. 아무것도 고치지 않는다.
-- 역할: T1001 학생 · T1002/T1003 교사 · 그 밖의 코드 임원 · 코드 없음 학부모
-- 가족팀 관계(DM2_FamilyMember.RoleCode): 0 학생 · 1 엄마 · 2 아빠 · 3 할머니 · 4 할아버지
SET NOCOUNT ON;

;WITH mem AS (
    SELECT ft.FamilyTeamId, ft.TeamName, ISNULL(ft.TeamType, N'FAMILY') AS TeamType, ft.CreatorUserName,
           m.UserName, m.RoleCode AS Rel,
           ISNULL(pm.FullName, m.UserName) AS FullName,
           -- Common/UserRole.RoleCodeSql 과 같은 기준: 배정 표(view_All_Person_2026) 우선, 없으면 DeptRoleUserAssoc
           COALESCE((SELECT TOP 1 LTRIM(RTRIM(ap.RoleCode)) FROM dbo.view_All_Person_2026 ap WHERE ap.userid = u.Id AND ISNULL(ap.RoleCode, N'') <> N'' ORDER BY ap.RoleCode),
                    (SELECT TOP 1 LTRIM(RTRIM(dr.RoleCode)) FROM dbo.DeptRoleUserAssoc dr WHERE dr.UserId = u.Id AND ISNULL(dr.RoleCode, N'') <> N''
                     ORDER BY CASE WHEN dr.DeptCode = N'D1001' THEN 0 ELSE 1 END), N'') AS RoleCode
    FROM dbo.DM2_FamilyMember m
    JOIN dbo.DM2_FamilyTeam ft ON ft.FamilyTeamId = m.FamilyTeamId AND ft.IsActive = 1
    LEFT JOIN [Identity].[User] u ON u.UserName = m.UserName
    LEFT JOIN dbo.view_PersonMaster_2026 pm ON pm.UserId = u.Id
    WHERE m.IsActive = 1
), k AS (
    SELECT *, CASE WHEN RoleCode = N'' THEN N'학부모' WHEN RoleCode = N'T1001' THEN N'학생'
                   WHEN RoleCode IN (N'T1002', N'T1003') THEN N'교사' ELSE N'임원' END AS Kind,
              CASE Rel WHEN 0 THEN N'학생' WHEN 1 THEN N'엄마' WHEN 2 THEN N'아빠' WHEN 3 THEN N'할머니' WHEN 4 THEN N'할아버지' ELSE N'기타' END AS RelLabel
    FROM mem
)
SELECT TeamName, CASE TeamType WHEN N'FRIEND' THEN N'친구팀' ELSE N'가족팀' END AS TeamKind,
       FullName, UserName, Kind AS 역할, RelLabel AS 관계,
       CASE WHEN UserName = CreatorUserName THEN N'방장' ELSE N'' END AS 방장,
       CASE
         WHEN TeamType = N'FRIEND' AND Kind <> N'학생' THEN N'친구팀에 학생 아닌 사람'
         WHEN TeamType <> N'FRIEND' AND Kind IN (N'교사', N'임원') THEN N'가족팀에 교사·임원'
         WHEN TeamType <> N'FRIEND' AND Kind = N'학생' AND Rel <> 0 THEN N'학생인데 관계가 학생 아님'
         WHEN TeamType <> N'FRIEND' AND Kind = N'학부모' AND Rel = 0 THEN N'학부모인데 관계가 학생'
       END AS 문제
FROM k
WHERE (TeamType = N'FRIEND' AND Kind <> N'학생')
   OR (TeamType <> N'FRIEND' AND Kind IN (N'교사', N'임원'))
   OR (TeamType <> N'FRIEND' AND Kind = N'학생' AND Rel <> 0)
   OR (TeamType <> N'FRIEND' AND Kind = N'학부모' AND Rel = 0)
ORDER BY 문제, TeamName, FullName;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BulletinBoard.Common
{
    // ──────────────────────────────────────────────────────────────
    //  인별 소속 (2026-11-08) — 역할은 UserRole.RoleCodeSql 한 곳 기준
    //   학생: 몇 학년 몇 반 · 교사: 담당 반(담임 · 부담임) · 임원: 직분 이름 · 학부모 · 기타
    //   가입 승인 대기 중이면 신청한 역할 + "(승인 대기)"
    //   쓰는 곳: /UserRoles/Index, /Bible2/Participants
    // ──────────────────────────────────────────────────────────────

    public class UserAffiliationInfo
    {
        public UserRoleKind Kind { get; set; } = UserRoleKind.Parent;
        public string RoleNM { get; set; } = "";              // 임원 코드 이름 (CodeData T001)
        public int ClassYear { get; set; }                    // 학생 반
        public int ClassBan { get; set; }
        public List<string> TeachClasses { get; set; } = new(); // "1학년 2반 담임"
        public bool Pending { get; set; }
        public string Label { get; set; } = "학부모";          // 화면에 보이는 한 줄

        /// <summary>CSS 짧은 이름 — st 학생 · te 교사 · of 임원 · et 기타 · pa 학부모</summary>
        public string Css => Kind switch
        {
            UserRoleKind.Student => "st",
            UserRoleKind.Teacher => "te",
            UserRoleKind.Officer => "of",
            UserRoleKind.Etc => "et",
            _ => "pa",
        };
    }

    public static class UserAffiliation
    {
        /// <summary>모든 계정의 소속 ([Identity].[User].Id → 소속). 조회가 실패하면 빈 사전</summary>
        public static Dictionary<string, UserAffiliationInfo> LoadAll()
        {
            var map = new Dictionary<string, UserAffiliationInfo>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string sql = $@"
SELECT u.[Id] AS uid, x.rc,
       ISNULL((SELECT TOP 1 cd.[MinorNM] FROM [dbo].[CodeData] cd WHERE cd.[MajorCD] = 'T001' AND cd.[MinorCD] = x.rc), N'') AS rnm,
       ISNULL(s.[ClassYear], 0) AS cy, ISNULL(s.[ClassBan], 0) AS cb,
       CASE WHEN x.rc = N'' THEN ISNULL((SELECT TOP 1 sa.[RoleKind] FROM [dbo].[SignupApplication] sa
                                         WHERE sa.[UserName] = u.[UserName] AND sa.[Status] = N'PENDING' AND sa.[ReqType] = N'JOIN'
                                         ORDER BY sa.[Id] DESC), N'')
            ELSE N'' END AS pk
FROM [Identity].[User] u
CROSS APPLY (SELECT {UserRole.RoleCodeSql("u.[Id]")} AS rc) x
OUTER APPLY (SELECT TOP 1 c.[ClassYear], c.[ClassBan] FROM [dbo].[view_UserCompositeInfo_2026] c
             WHERE c.[UserName] = u.[UserName] AND ISNULL(c.[ClassYear], 0) > 0 ORDER BY c.[ClassYear], c.[ClassBan]) s;

SELECT DISTINCT ap.[userid] AS uid, m.[ClassYear] AS cy, m.[ClassBan] AS cb,
       CASE WHEN m.[TeacherPersonId] = ap.[personid] THEN 0 ELSE 1 END AS sub
FROM [dbo].[ClassTeacherMap_2026] m
JOIN [dbo].[view_All_Person_2026] ap ON ap.[personid] IN (m.[TeacherPersonId], m.[SubTeacherPersonId])
WHERE ISNULL(m.[IsActive], 0) = 1 AND ISNULL(ap.[personid], 0) <> 0 AND ap.[userid] IS NOT NULL
ORDER BY cy, cb;";
                var ds = DataAccessModule.commonQuery(sql);
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        string uid = Convert.ToString(r["uid"]) ?? "";
                        if (uid.Length == 0) continue;
                        string rc = Convert.ToString(r["rc"]) ?? "";
                        string pk = (Convert.ToString(r["pk"]) ?? "").Trim();
                        var a = new UserAffiliationInfo { Kind = UserRole.KindOf(rc) };
                        if (rc.Trim().Length == 0 && pk.Length > 0)
                        {
                            a.Pending = true;
                            a.Kind = pk switch { "S" => UserRoleKind.Student, "T" => UserRoleKind.Teacher, "E" => UserRoleKind.Etc, _ => UserRoleKind.Parent };
                        }
                        a.RoleNM = Convert.ToString(r["rnm"]) ?? "";
                        a.ClassYear = Convert.ToInt32(r["cy"]);
                        a.ClassBan = Convert.ToInt32(r["cb"]);
                        map[uid] = a;
                    }
                }
                if (ds != null && ds.Tables.Count > 1)
                {
                    foreach (DataRow r in ds.Tables[1].Rows)
                    {
                        if (!map.TryGetValue(Convert.ToString(r["uid"]) ?? "", out var a)) continue;
                        string t = $"{Convert.ToInt32(r["cy"])}학년 {Convert.ToInt32(r["cb"])}반 {(Convert.ToInt32(r["sub"]) == 1 ? "부담임" : "담임")}";
                        if (!a.TeachClasses.Contains(t)) a.TeachClasses.Add(t);
                    }
                }
            }
            catch (Exception ex)
            {
                BulletinBoard.Helpers.DbLogger.LogDebugAsync("소속 조회 실패: " + ex.Message);
            }

            foreach (var a in map.Values) a.Label = MakeLabel(a);
            return map;
        }

        private static string MakeLabel(UserAffiliationInfo a)
        {
            string teach = a.TeachClasses.Count > 0 ? string.Join(", ", a.TeachClasses) : "";
            string lbl = a.Kind switch
            {
                UserRoleKind.Student => a.ClassYear > 0 ? $"학생 · {a.ClassYear}학년 {a.ClassBan}반" : "학생 · 반 없음",
                UserRoleKind.Teacher => teach.Length > 0 ? $"교사 · {teach}" : "교사",
                UserRoleKind.Officer => (string.IsNullOrWhiteSpace(a.RoleNM) ? "임원" : $"임원 · {a.RoleNM}") + (teach.Length > 0 ? $" · {teach}" : ""),
                _ => UserRole.LabelOf(a.Kind),
            };
            return a.Pending ? lbl + " (승인 대기)" : lbl;
        }
    }
}

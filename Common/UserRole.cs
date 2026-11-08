using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace BulletinBoard.Common
{
    // ──────────────────────────────────────────────────────────────
    //  역할 기준 (2026-11-08) — 설계서 docs/역할기준_설계서.md
    //   · 역할 코드는 RoleCodeSql 한 곳에서 읽는다: 배정 표(view_All_Person_2026) 우선, 없으면 DeptRoleUserAssoc
    //       T1001 = 학생 · T1002/T1003 = 교사 · 그 밖의 코드 = 임원 · 코드 없음 = 학부모
    //   · 팀 규칙도 여기 한 곳에서 정한다
    //       가족팀: 학생(관계 "학생"만) · 학부모(엄마 · 아빠 · 할머니 · 할아버지) — 교사 · 임원은 못 들어간다
    //       친구팀: 학생만
    //   역할을 따지는 새 코드는 반드시 이 클래스를 쓴다.
    // ──────────────────────────────────────────────────────────────

    public enum UserRoleKind
    {
        Parent = 0,    // 학부모 (역할 코드 없음)
        Student = 1,   // 학생 T1001
        Teacher = 2,   // 교사 T1002 · 교사(S) T1003
        Officer = 3,   // 임원 등 그 밖의 코드
    }

    public static class UserRole
    {
        public const string StudentCode = "T1001";
        public const string TeacherCode = "T1002";
        public const string TeacherSCode = "T1003";

        /// <summary>가족팀 관계 번호 (DM2_FamilyMember.RoleCode) — 0 학생 · 1 엄마 · 2 아빠 · 3 할머니 · 4 할아버지</summary>
        public const int RelStudent = 0;

        public static UserRoleKind KindOf(string? roleCode)
        {
            var rc = (roleCode ?? "").Trim();
            if (rc.Length == 0) return UserRoleKind.Parent;
            if (rc == StudentCode) return UserRoleKind.Student;
            if (rc == TeacherCode || rc == TeacherSCode) return UserRoleKind.Teacher;
            return UserRoleKind.Officer;
        }

        public static string LabelOf(UserRoleKind k) => k switch
        {
            UserRoleKind.Student => "학생",
            UserRoleKind.Teacher => "교사",
            UserRoleKind.Officer => "임원",
            _ => "학부모",
        };

        /// <summary>
        /// SQL 안에서 한 사람의 역할 코드를 읽는 식 (userIdExpr = [Identity].[User].Id 를 가리키는 식).
        /// 2026-11-08 사용자 결정 — 배정 표 우선, 없으면 승인된 역할:
        ///   1) view_All_Person_2026 (반 배정 = T1001 · 담임/부담임 = T1002 · 부서 교사 표 = 그 코드) — 대부분의 화면이 보는 것과 같다
        ///   2) DeptRoleUserAssoc (가입 승인 · 역할 변경이 쓰는 표 — 반 없는 학생, 담임 아닌 교사)
        ///   3) 둘 다 없으면 빈 문자열 = 학부모
        /// </summary>
        public static string RoleCodeSql(string userIdExpr) =>
            "COALESCE(" +
            $"(SELECT TOP 1 LTRIM(RTRIM(ap.[RoleCode])) FROM [dbo].[view_All_Person_2026] ap WHERE ap.[userid] = {userIdExpr} AND ISNULL(ap.[RoleCode], N'') <> N'' ORDER BY ap.[RoleCode]), " +
            $"(SELECT TOP 1 LTRIM(RTRIM(dr.[RoleCode])) FROM [dbo].[DeptRoleUserAssoc] dr WHERE dr.[UserId] = {userIdExpr} AND ISNULL(dr.[RoleCode], N'') <> N'' " +
            $"ORDER BY CASE WHEN dr.[DeptCode] = N'{ConstatsCommon.FixedDeptCode}' THEN 0 ELSE 1 END), N'')";

        /// <summary>
        /// 로그인 아이디로 역할을 읽는다 (트랜잭션 안에서도 쓸 수 있다).
        /// 역할 코드가 없고 가입 승인을 기다리는 중이면(SignupApplication JOIN · PENDING) 신청한 역할로 본다 —
        /// 가입한 학생이 승인 전에 가족팀에 들어갈 때 부모 관계를 강요받지 않게.
        /// </summary>
        public static async Task<UserRoleKind> GetKindAsync(SqlConnection c, SqlTransaction? tx, string userName)
        {
            string sql = $@"
DECLARE @rc NVARCHAR(20) = (SELECT TOP 1 {RoleCodeSql("u.[Id]")} FROM [Identity].[User] u WHERE u.[UserName] = @u);
DECLARE @pk NVARCHAR(1) = N'';
IF ISNULL(@rc, N'') = N'' AND OBJECT_ID(N'dbo.SignupApplication') IS NOT NULL
    EXEC sp_executesql N'SELECT TOP 1 @o = [RoleKind] FROM [dbo].[SignupApplication] WHERE [UserName] = @n AND [Status] = N''PENDING'' AND [ReqType] = N''JOIN'' ORDER BY [Id] DESC',
         N'@n NVARCHAR(256), @o NVARCHAR(1) OUTPUT', @n = @u, @o = @pk OUTPUT;
SELECT ISNULL(@rc, N''), ISNULL(@pk, N'');";
            using var cmd = new SqlCommand(sql, c, tx);
            cmd.Parameters.AddWithValue("@u", userName ?? "");
            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return UserRoleKind.Parent;
            string rc = Convert.ToString(rd[0]) ?? "";
            string pending = (Convert.ToString(rd[1]) ?? "").Trim();
            if (rc.Trim().Length == 0)
            {
                if (pending == "S") return UserRoleKind.Student;
                if (pending == "T") return UserRoleKind.Teacher;
            }
            return KindOf(rc);
        }

        // ── 팀 규칙 ─────────────────────────────────────────────

        public static bool CanJoinFamily(UserRoleKind k) => k == UserRoleKind.Student || k == UserRoleKind.Parent;

        public static bool CanJoinFriend(UserRoleKind k) => k == UserRoleKind.Student;

        /// <summary>이 역할이 가족팀에서 이 관계(0 학생 · 1~4 부모 쪽)를 가질 수 있나</summary>
        public static bool RelationAllowed(UserRoleKind k, int relRole) => k switch
        {
            UserRoleKind.Student => relRole == RelStudent,
            UserRoleKind.Parent => relRole >= 1 && relRole <= 4,
            _ => false,
        };

        /// <summary>
        /// 가족팀 관계를 이 사람에게 줄 수 있는지 — 안 되면 화면에 띄울 문구, 되면 null.
        /// self = 본인이 하는 일인가 (문구만 다르다)
        /// </summary>
        public static string? FamilyRelationError(UserRoleKind k, int relRole, bool self)
        {
            if (!CanJoinFamily(k))
                return self ? "교사 · 임원은 가족팀에 들어갈 수 없어요." : $"그분은 {LabelOf(k)}이라 가족팀에 들어올 수 없어요.";
            if (RelationAllowed(k, relRole)) return null;
            if (k == UserRoleKind.Student)
                return self ? "학생은 관계를 '학생'으로만 정할 수 있어요." : "그분은 학생이라 관계를 '학생'으로만 정할 수 있어요.";
            return self ? "학부모는 엄마 · 아빠 · 할머니 · 할아버지 중에서 골라 주세요." : "그분은 학부모라 엄마 · 아빠 · 할머니 · 할아버지 중에서 골라 주세요.";
        }
    }
}

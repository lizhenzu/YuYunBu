using BulletinBoard.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BulletinBoard.Services
{
    // ──────────────────────────────────────────────────────────────
    //  가족팀 직접 구성 (카톡 방식) — 2026-11-07
    //   · 한 사람은 한 팀에만 (DM2_FamilyMember 의 UserName 은 표 전체에서 유일)
    //   · 누구나 팀을 만들고(이름은 "○○○네 가족팀" 자동), 누구나 초대한다
    //   · 가입 신청은 팀원 한 명이 수락하면 바로 팀원, 초대는 초대받은 본인이 수락
    //   · 2026-11-08 재시상 방지: 시상은 팀 키(AwardKey, 처음 이름으로 고정)로 관리하고 팀 이름은 팀원이 바꿀 수 있다
    //   · 처음 만든 사람(CreatorUserName)이 방장이다. 보호자일 필요 없다. 시상 기록이 있는 팀은 누구도 탈퇴할 수 없고 내보낼 수도 없다. 방장은 탈퇴할 수 없고(혼자이고 시상 기록이 없으면 탈퇴 가능, 팀 삭제), 다른 팀원을 내보낼 수 있는 사람도 방장뿐이다. 다른 팀원은 언제든 탈퇴
    //   · 팀 삭제는 관리자만. 예외: 혼자이고 시상 기록이 없는 팀은 다른 팀에 신청할 때 지운다
    //   · 처리 안 된 신청·초대는 30일 뒤 지운다
    //  표: dbo.DM2_FamilyRequest · dbo.DM2_FamilyNotice (SQL/CreateFamilyTeamSelfService_20261107.sql)
    // ──────────────────────────────────────────────────────────────

    public sealed class FamilyTeamResult
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = "";
        public int Id { get; set; }
        public static FamilyTeamResult Fail(string m) => new FamilyTeamResult { Ok = false, Message = m };
        public static FamilyTeamResult Success(string m, int id = 0) => new FamilyTeamResult { Ok = true, Message = m, Id = id };
    }

    public sealed class FamilyTeamBrief
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<string> MemberNames { get; set; } = new();
        public int RequestId { get; set; }          // 내가 이미 신청했으면 그 번호 (취소용), 아니면 0
        public bool IsFull { get; set; }            // 친구팀 정원(2명)이 찼다
    }

    public sealed class FamilyMemberInfo
    {
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string RelLabel { get; set; } = "";
        public bool IsMe { get; set; }
        public int RoleCode { get; set; }
        public string RelKey { get; set; } = "";      // 지금 관계의 키 (child · mom · dad · grandma · grandpa)
    }

    public sealed class FamilyRequestInfo
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = "";
        public string TargetUserName { get; set; } = "";
        public string TargetFullName { get; set; } = "";
        public string FromUserName { get; set; } = "";
        public string FromFullName { get; set; } = "";
        public string RelLabel { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public List<string> MemberNames { get; set; } = new();
        public string TeamType { get; set; } = "FAMILY";
    }

    public sealed class FamilyTeamDetail
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = "";
        public string CreatorUserName { get; set; } = "";
        public bool IsCreator { get; set; }        // 로그인한 내가 만든 사람인가
        public bool HasAwardRecord { get; set; }   // 이 팀에 시상 기록이 있는가
        public string TeamType { get; set; } = "FAMILY";   // FAMILY | FRIEND
        public bool IsFriend => TeamType == FamilyTeamService.TypeFriend;
        public List<FamilyMemberInfo> Members { get; set; } = new();
        public List<FamilyRequestInfo> Requests { get; set; } = new();   // 들어오고 싶다는 신청
        public List<FamilyRequestInfo> Invites { get; set; } = new();    // 우리 팀이 보낸 초대
    }

    public sealed class FamilyUserHit
    {
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string State { get; set; } = "ok";   // ok(팀 없음) | inTeam(다른 팀) | mine(우리 팀) | invited(우리 팀이 초대함)
        public int TeamId { get; set; }             // 그 사람이 속한 팀 (없으면 0)
        public string TeamName { get; set; } = "";
        public List<string> MemberNames { get; set; } = new();   // 그 팀의 구성원 이름
        public bool Requested { get; set; }         // 내가 그 팀에 이미 가입 신청을 보냈다
        public string TeamType { get; set; } = "";  // 그 사람이 속한 팀의 종류 (FAMILY | FRIEND)
        public bool IsFull { get; set; }            // 그 팀이 친구팀이고 정원이 찼다
        public bool IsStudent { get; set; }         // 학생인가 (친구팀은 학생끼리만)
    }

    public class FamilyTeamService
    {
        private readonly string _conn;

        public const int ExpireDays = 30;
        public const int MaxPendingPerUser = 20;

        // 팀 종류. 가족팀과 친구팀은 서로 배타적이다 (한 사람 한 팀)
        public const string TypeFamily = "FAMILY";
        public const string TypeFriend = "FRIEND";
        public const int FriendCapacity = 2;
        private const string FriendRelationKey = "child";   // 친구팀은 관계를 고르지 않는다 (학생으로 저장)

        public FamilyTeamService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection not found.");
        }

        // 테스트용 (연결 문자열을 바로 받는다)
        public FamilyTeamService(string connectionString) { _conn = connectionString; }

        // ── 관계 ──────────────────────────────────────────────────
        //   관계는 다섯 가지뿐이다 (그 밖의 가족 · 삼촌 · 이모는 없다). key → (RoleCode, 글자). RoleCode 는 기존 가계도와 같다: 0=학생 1=엄마 2=아빠 3=할머니 4=할아버지
        //   (key "child" 는 학생. 예전 이름 그대로 두어 이미 저장된 요청과 맞는다)
        public static readonly (string Key, int Role, string Label)[] Relations =
        {
            ("child",   0, "학생"),
            ("mom",     1, "엄마"),
            ("dad",     2, "아빠"),
            ("grandma", 3, "할머니"),
            ("grandpa", 4, "할아버지"),
        };

        public static string KeyOfRole(int role) => role switch { 0 => "child", 1 => "mom", 2 => "dad", 3 => "grandma", 4 => "grandpa", _ => "" };

        public static bool TryRelation(string? key, out int role, out string label)
        {
            foreach (var r in Relations)
                if (string.Equals(r.Key, key, StringComparison.Ordinal)) { role = r.Role; label = r.Label; return true; }
            role = 0; label = ""; return false;
        }

        public static string LabelOf(string key) => TryRelation(key, out _, out var l) ? l : "가족";

        private SqlConnection Conn() => new SqlConnection(_conn);

        private static async Task<SqlTransaction> BeginAsync(SqlConnection c)
        {
            await c.OpenAsync();
            return c.BeginTransaction(IsolationLevel.Serializable);
        }

        private static SqlCommand Cmd(SqlConnection c, SqlTransaction? t, string sql, params (string Name, object? Value)[] ps)
        {
            var cmd = new SqlCommand(sql, c, t) { CommandTimeout = 30 };
            foreach (var (n, v) in ps)
                cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
            return cmd;
        }

        private static async Task<T?> ScalarAsync<T>(SqlConnection c, SqlTransaction? t, string sql, params (string, object?)[] ps)
        {
            var o = await Cmd(c, t, sql, ps).ExecuteScalarAsync();
            if (o == null || o == DBNull.Value) return default;
            return (T)Convert.ChangeType(o, typeof(T));
        }

        // ── 30일 지난 신청·초대 지우기 ──────────────────────────
        public async Task PurgeExpiredAsync()
        {
            try
            {
                using var c = Conn();
                await c.OpenAsync();
                await Cmd(c, null, "DELETE FROM dbo.DM2_FamilyRequest WHERE CreatedAt < @lim;",
                    ("@lim", DateTime.Now.AddDays(-ExpireDays))).ExecuteNonQueryAsync();
                await Cmd(c, null, "DELETE FROM dbo.DM2_FamilyNotice WHERE CreatedAt < @lim;",
                    ("@lim", DateTime.Now.AddDays(-ExpireDays))).ExecuteNonQueryAsync();
            }
            catch { /* 정리는 부가 기능 */ }
        }

        // ── 내 팀 ────────────────────────────────────────────────
        /// <summary>내가 들어 있는 팀 번호 (없으면 0)</summary>
        public async Task<int> GetMyTeamIdAsync(string userName)
        {
            using var c = Conn();
            await c.OpenAsync();
            return await MyTeamIdAsync(c, null, userName);
        }

        private static async Task<int> MyTeamIdAsync(SqlConnection c, SqlTransaction? t, string userName)
        {
            return await ScalarAsync<int>(c, t,
                @"SELECT TOP 1 m.FamilyTeamId FROM dbo.DM2_FamilyMember m
                  JOIN dbo.DM2_FamilyTeam ft ON ft.FamilyTeamId = m.FamilyTeamId AND ft.IsActive = 1
                  WHERE m.UserName = @u AND m.IsActive = 1;", ("@u", userName));
        }

        public async Task<FamilyTeamDetail?> GetTeamDetailAsync(string userName)
        {
            await PurgeExpiredAsync();
            using var c = Conn();
            await c.OpenAsync();

            int teamId = await MyTeamIdAsync(c, null, userName);
            if (teamId == 0) return null;

            var d = new FamilyTeamDetail { TeamId = teamId };
            d.TeamName = await ScalarAsync<string>(c, null, "SELECT TeamName FROM dbo.DM2_FamilyTeam WHERE FamilyTeamId=@t;", ("@t", teamId)) ?? "";
            d.CreatorUserName = await CreatorOfAsync(c, null, teamId);
            d.IsCreator = string.Equals(d.CreatorUserName, userName, StringComparison.OrdinalIgnoreCase);
            d.HasAwardRecord = await HasAwardRecordAsync(c, null, teamId);
            d.TeamType = await TeamTypeOfAsync(c, null, teamId);

            const string sqlMembers = @"
SELECT m.UserName, ISNULL(pm.FullName, m.UserName), m.RoleCode, ISNULL(m.RelationLabel, N'')
FROM dbo.DM2_FamilyMember m
LEFT JOIN [Identity].[User] u ON u.UserName = m.UserName
LEFT JOIN dbo.view_PersonMaster_2026 pm ON pm.UserId = u.Id
WHERE m.FamilyTeamId = @t AND m.IsActive = 1
ORDER BY m.RoleCode, m.RelationCode, m.SortNo;";
            using (var rd = await Cmd(c, null, sqlMembers, ("@t", teamId)).ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                {
                    var un = rd.GetString(0);
                    int role = Convert.ToInt32(rd[2]);
                    d.Members.Add(new FamilyMemberInfo
                    {
                        UserName = un,
                        FullName = rd.GetString(1),
                        RelLabel = DefaultLabel(role),   // 화면에는 다섯 관계 이름만 쓴다 (예전에 손으로 적은 글자는 보이지 않는다)
                        RoleCode = role,
                        RelKey = KeyOfRole(role),
                        IsMe = string.Equals(un, userName, StringComparison.OrdinalIgnoreCase),
                    });
                }
            }

            const string sqlReq = @"
SELECT r.Id, r.FamilyTeamId, r.Kind, r.TargetUserName, ISNULL(tp.FullName, r.TargetUserName),
       r.FromUserName, ISNULL(fp.FullName, r.FromUserName), r.RelationKey, r.CreatedAt
FROM dbo.DM2_FamilyRequest r
LEFT JOIN [Identity].[User] tu ON tu.UserName = r.TargetUserName
LEFT JOIN dbo.view_PersonMaster_2026 tp ON tp.UserId = tu.Id
LEFT JOIN [Identity].[User] fu ON fu.UserName = r.FromUserName
LEFT JOIN dbo.view_PersonMaster_2026 fp ON fp.UserId = fu.Id
WHERE r.FamilyTeamId = @t
ORDER BY r.CreatedAt;";
            using (var rd = await Cmd(c, null, sqlReq, ("@t", teamId)).ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                {
                    var info = new FamilyRequestInfo
                    {
                        Id = rd.GetInt32(0),
                        TeamId = rd.GetInt32(1),
                        TeamName = d.TeamName,
                        TargetUserName = rd.GetString(3),
                        TargetFullName = rd.GetString(4),
                        FromUserName = rd.GetString(5),
                        FromFullName = rd.GetString(6),
                        RelLabel = LabelOf(rd.GetString(7)),
                        CreatedAt = rd.GetDateTime(8),
                    };
                    if (rd.GetString(2) == "R") d.Requests.Add(info); else d.Invites.Add(info);
                }
            }
            return d;
        }

        private static string DefaultLabel(int role) => role switch
        {
            0 => "학생", 1 => "엄마", 2 => "아빠", 3 => "할머니", 4 => "할아버지", _ => "가족"
        };

        /// <summary>
        /// 대기 중인 요청 개수 — 화면에 "수락을 기다려요" 문구를 띄우는 데 쓴다.
        ///  In  = 우리 팀으로 들어오고 싶다는 신청 (내가 팀원일 때)
        ///  Out = 내가 낸 가입 신청 (아직 팀이 없을 때)
        ///  표가 없거나 오류가 나면 (0, 0)
        /// </summary>
        public async Task<(int In, int Out)> GetPendingCountsAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return (0, 0);
            try
            {
                await PurgeExpiredAsync();
                using var c = Conn();
                await c.OpenAsync();
                int teamId = await MyTeamIdAsync(c, null, userName);
                if (teamId != 0)
                {
                    int n = await ScalarAsync<int>(c, null,
                        "SELECT COUNT(*) FROM dbo.DM2_FamilyRequest WHERE FamilyTeamId=@t AND Kind='R';", ("@t", teamId));
                    return (n, 0);
                }
                int mine = await ScalarAsync<int>(c, null,
                    "SELECT COUNT(*) FROM dbo.DM2_FamilyRequest WHERE Kind='R' AND TargetUserName=@u;", ("@u", userName));
                return (0, mine);
            }
            catch { return (0, 0); }
        }

        // ── 팀 찾기 ──────────────────────────────────────────────
        public async Task<List<FamilyTeamBrief>> ListOpenTeamsAsync(string userName, string teamType = TypeFamily)
        {
            await PurgeExpiredAsync();
            using var c = Conn();
            await c.OpenAsync();

            var map = new Dictionary<int, FamilyTeamBrief>();
            var order = new List<FamilyTeamBrief>();
            const string sql = @"
SELECT ft.FamilyTeamId, ft.TeamName, ISNULL(pm.FullName, m.UserName)
FROM dbo.DM2_FamilyTeam ft
JOIN dbo.DM2_FamilyMember m ON m.FamilyTeamId = ft.FamilyTeamId AND m.IsActive = 1
LEFT JOIN [Identity].[User] u ON u.UserName = m.UserName
LEFT JOIN dbo.view_PersonMaster_2026 pm ON pm.UserId = u.Id
WHERE ft.IsActive = 1 AND ft.TeamType = @type
ORDER BY ft.FamilyTeamId DESC, m.RoleCode, m.RelationCode;";
            using (var rd = await Cmd(c, null, sql, ("@type", teamType)).ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                {
                    int id = rd.GetInt32(0);
                    if (!map.TryGetValue(id, out var b))
                    {
                        b = new FamilyTeamBrief { Id = id, Name = rd.GetString(1) };
                        map[id] = b; order.Add(b);
                    }
                    b.MemberNames.Add(rd.GetString(2));
                }
            }

            using (var rd = await Cmd(c, null,
                "SELECT Id, FamilyTeamId FROM dbo.DM2_FamilyRequest WHERE Kind='R' AND TargetUserName=@u;", ("@u", userName)).ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                    if (map.TryGetValue(rd.GetInt32(1), out var b)) b.RequestId = rd.GetInt32(0);
            }

            // 친구팀은 자리가 있는 팀만 보여 준다 (내가 신청해 둔 팀은 그대로 둔다)
            if (teamType == TypeFriend)
            {
                foreach (var b in order) b.IsFull = b.MemberNames.Count >= FriendCapacity;
                order = order.Where(b => !b.IsFull || b.RequestId != 0).ToList();
            }
            return order;
        }

        /// <summary>
        /// 이름(실명)으로 사람 찾기 — 그 사람이 어느 팀에 있는지(팀 이름 · 구성원 명단)도 함께 돌려준다.
        /// 팀이 없는 사람도 찾을 수 있다 (가족이 만든 팀을 찾아 신청하려고).
        /// </summary>
        public async Task<List<FamilyUserHit>> SearchUsersAsync(string userName, string query)
        {
            var list = new List<FamilyUserHit>();
            query = (query ?? "").Trim();
            if (query.Length < 2) return list;

            using var c = Conn();
            await c.OpenAsync();
            int myTeam = await MyTeamIdAsync(c, null, userName);

            string like = "%" + query.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            const string sql = @"
SELECT TOP 20 u.UserName, pm.FullName, ISNULL(tm.FamilyTeamId, 0), ISNULL(tm.TeamName, N''),
  CASE WHEN EXISTS (SELECT 1 FROM dbo.DM2_FamilyRequest r
                    WHERE r.Kind = 'I' AND r.FamilyTeamId = @team AND r.TargetUserName = u.UserName) THEN 1 ELSE 0 END,
  ISNULL(tm.TeamType, ''),
  CASE WHEN EXISTS (SELECT 1 FROM dbo.view_UserCompositeInfo_2026 ui WHERE ui.UserName = u.UserName AND ui.RoleCode = @student) THEN 1 ELSE 0 END
FROM [Identity].[User] u
JOIN dbo.view_PersonMaster_2026 pm ON pm.UserId = u.Id
OUTER APPLY (SELECT TOP 1 m.FamilyTeamId, ft.TeamName, ft.TeamType
             FROM dbo.DM2_FamilyMember m
             JOIN dbo.DM2_FamilyTeam ft ON ft.FamilyTeamId = m.FamilyTeamId AND ft.IsActive = 1
             WHERE m.UserName = u.UserName AND m.IsActive = 1) tm
WHERE u.UserName <> @me AND pm.FullName LIKE @q
ORDER BY pm.FullName, u.UserName;";
            using (var rd = await Cmd(c, null, sql, ("@me", userName), ("@team", myTeam), ("@q", like), ("@student", StudentRoleCode)).ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                {
                    int tid = rd.GetInt32(2);
                    list.Add(new FamilyUserHit
                    {
                        UserName = rd.GetString(0),
                        FullName = rd.GetString(1),
                        TeamId = tid,
                        TeamName = rd.GetString(3),
                        TeamType = rd.GetString(5),
                        IsStudent = rd.GetInt32(6) == 1,
                        State = tid == 0 ? (rd.GetInt32(4) == 1 ? "invited" : "ok")
                              : (tid == myTeam ? "mine" : "inTeam"),
                    });
                }
            }

            // 팀별 구성원 명단 (같은 팀이 여러 번 나와도 한 번만 읽는다)
            var names = new Dictionary<int, List<string>>();
            foreach (int tid in list.Where(h => h.TeamId != 0).Select(h => h.TeamId).Distinct())
            {
                var one = new List<string>();
                using var rd = await Cmd(c, null,
                    @"SELECT ISNULL(pm.FullName, m.UserName) FROM dbo.DM2_FamilyMember m
                      LEFT JOIN [Identity].[User] u ON u.UserName = m.UserName
                      LEFT JOIN dbo.view_PersonMaster_2026 pm ON pm.UserId = u.Id
                      WHERE m.FamilyTeamId = @t AND m.IsActive = 1 ORDER BY m.RoleCode, m.RelationCode;", ("@t", tid)).ExecuteReaderAsync();
                while (await rd.ReadAsync()) one.Add(rd.GetString(0));
                names[tid] = one;
            }

            // 내가 이미 신청한 팀 표시
            var requested = new HashSet<int>();
            using (var rd = await Cmd(c, null,
                "SELECT FamilyTeamId FROM dbo.DM2_FamilyRequest WHERE Kind='R' AND TargetUserName=@u;", ("@u", userName)).ExecuteReaderAsync())
            {
                while (await rd.ReadAsync()) requested.Add(rd.GetInt32(0));
            }

            foreach (var h in list)
            {
                if (h.TeamId == 0) continue;
                h.MemberNames = names[h.TeamId];
                h.IsFull = h.TeamType == TypeFriend && h.MemberNames.Count >= FriendCapacity;
                h.Requested = requested.Contains(h.TeamId);
            }
            return list;
        }

        // ── 로그인할 때 보여 줄 것 ───────────────────────────────
        /// <summary>나에게 온 초대 (팀 구성원 이름 포함)</summary>
        public async Task<List<FamilyRequestInfo>> GetIncomingInvitesAsync(string userName)
        {
            var list = new List<FamilyRequestInfo>();
            if (string.IsNullOrWhiteSpace(userName)) return list;
            await PurgeExpiredAsync();
            using var c = Conn();
            await c.OpenAsync();

            const string sql = @"
SELECT r.Id, r.FamilyTeamId, ft.TeamName, r.FromUserName, ISNULL(fp.FullName, r.FromUserName), r.RelationKey, r.CreatedAt, ft.TeamType
FROM dbo.DM2_FamilyRequest r
JOIN dbo.DM2_FamilyTeam ft ON ft.FamilyTeamId = r.FamilyTeamId AND ft.IsActive = 1
LEFT JOIN [Identity].[User] fu ON fu.UserName = r.FromUserName
LEFT JOIN dbo.view_PersonMaster_2026 fp ON fp.UserId = fu.Id
WHERE r.Kind = 'I' AND r.TargetUserName = @u
ORDER BY r.CreatedAt;";
            using (var rd = await Cmd(c, null, sql, ("@u", userName)).ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                    list.Add(new FamilyRequestInfo
                    {
                        Id = rd.GetInt32(0), TeamId = rd.GetInt32(1), TeamName = rd.GetString(2),
                        FromUserName = rd.GetString(3), FromFullName = rd.GetString(4),
                        RelLabel = LabelOf(rd.GetString(5)), CreatedAt = rd.GetDateTime(6), TeamType = rd.GetString(7),
                    });
            }
            if (list.Count == 0) return list;
            if (await MyTeamIdAsync(c, null, userName) != 0) return new List<FamilyRequestInfo>();   // 이미 팀이 있으면 보여 주지 않는다

            foreach (var inv in list)
            {
                using var rd = await Cmd(c, null,
                    @"SELECT ISNULL(pm.FullName, m.UserName) FROM dbo.DM2_FamilyMember m
                      LEFT JOIN [Identity].[User] u ON u.UserName = m.UserName
                      LEFT JOIN dbo.view_PersonMaster_2026 pm ON pm.UserId = u.Id
                      WHERE m.FamilyTeamId = @t AND m.IsActive = 1 ORDER BY m.RoleCode, m.RelationCode;", ("@t", inv.TeamId)).ExecuteReaderAsync();
                while (await rd.ReadAsync()) inv.MemberNames.Add(rd.GetString(0));
            }
            return list;
        }

        /// <summary>"팀원이 되었어요" 알림 — 한 번 읽으면 지운다. 없으면 null</summary>
        public async Task<string?> TakeJoinedNoticeAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return null;
            using var c = Conn();
            await c.OpenAsync();
            var name = await ScalarAsync<string>(c, null,
                @"DECLARE @t TABLE (TeamName NVARCHAR(100));
                  DELETE FROM dbo.DM2_FamilyNotice OUTPUT deleted.TeamName INTO @t WHERE UserName = @u AND Kind = 'JOINED';
                  SELECT TOP 1 TeamName FROM @t;", ("@u", userName));
            return name;
        }

        // ── 팀 만들기 ────────────────────────────────────────────
        public async Task<FamilyTeamResult> CreateTeamAsync(string userName, string? relationKey, string teamType = TypeFamily)
        {
            bool friend = teamType == TypeFriend;
            if (friend) relationKey = FriendRelationKey;
            if (!TryRelation(relationKey, out _, out _)) return FamilyTeamResult.Fail("관계를 골라 주세요.");

            using var c = Conn();
            using var tx = await BeginAsync(c);
            try
            {
                if (await UserExistsAsync(c, tx, userName) == false) return Rollback(tx, "로그인 정보를 찾을 수 없어요.");
                if (await HasActiveMembershipAsync(c, tx, userName)) return Rollback(tx, "이미 팀에 들어 있어요. 한 사람은 한 팀에만 들어갈 수 있어요.");
                if (friend && !await IsStudentAsync(c, tx, userName)) return Rollback(tx, "친구팀은 학생만 만들 수 있어요.");

                string full = await ScalarAsync<string>(c, tx,
                    @"SELECT TOP 1 ISNULL(pm.FullName, u.UserName) FROM [Identity].[User] u
                      LEFT JOIN dbo.view_PersonMaster_2026 pm ON pm.UserId = u.Id WHERE u.UserName = @u;", ("@u", userName)) ?? userName;
                full = full.Trim();

                string baseName = full + (friend ? "네 친구팀" : "네 가족팀");
                string name = baseName;
                for (int n = 2; await NameTakenAsync(c, tx, name); n++)
                    name = baseName + " " + n;

                int teamId = await ScalarAsync<int>(c, tx,
                    @"INSERT INTO dbo.DM2_FamilyTeam (TeamName, AwardKey, CreatorUserName, Memo, TeamType) VALUES (@n, @n, @u, N'직접 만든 팀', @type);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);", ("@n", name), ("@u", userName), ("@type", friend ? TypeFriend : TypeFamily));

                await InsertMemberAsync(c, tx, teamId, userName, relationKey!);
                tx.Commit();
                return FamilyTeamResult.Success($"\"{name}\"을 만들었어요.", teamId);
            }
            catch (Exception ex) { TryRollback(tx); return FamilyTeamResult.Fail("팀을 만들지 못했어요. 잠시 후 다시 시도해 주세요. (" + ex.Message + ")"); }
        }

        // ── 가입 신청 ────────────────────────────────────────────
        public async Task<FamilyTeamResult> ApplyAsync(string userName, int teamId, string? relationKey)
        {
            using var c = Conn();
            using var tx = await BeginAsync(c);
            try
            {
                // 친구팀은 관계를 고르지 않고, 학생만, 자리가 있을 때만 신청할 수 있다
                string targetType = await TeamTypeOfAsync(c, tx, teamId);
                if (targetType == TypeFriend)
                {
                    relationKey = FriendRelationKey;
                    if (!await IsStudentAsync(c, tx, userName)) return Rollback(tx, "친구팀은 학생만 들어갈 수 있어요.");
                    if (await ActiveCountAsync(c, tx, teamId) >= FriendCapacity) return Rollback(tx, "이 친구팀은 정원이 찼어요.");
                }
                if (!TryRelation(relationKey, out _, out _)) return Rollback(tx, "관계를 골라 주세요.");

                // 내 팀에 나 혼자이고 시상 기록이 없으면 다른 팀에 신청할 수 있다 (신청이 들어가는 순간 내 팀은 지운다).
                // 시상 기록이 있는 팀은 지우면 재시상으로 이어지므로 관리자만 정리한다.
                int myTeam = await MyTeamIdAsync(c, tx, userName);
                if (myTeam == teamId && myTeam != 0) return Rollback(tx, "이미 이 팀의 구성원이에요.");
                if (myTeam != 0)
                {
                    if (await ScalarAsync<int>(c, tx,
                            "SELECT COUNT(*) FROM dbo.DM2_FamilyMember WITH (UPDLOCK, HOLDLOCK) WHERE FamilyTeamId=@t AND IsActive=1;", ("@t", myTeam)) > 1)
                        return Rollback(tx, "이미 팀에 들어 있어요. 한 사람은 한 팀에만 들어갈 수 있어요.");
                    if (await HasAwardRecordAsync(c, tx, myTeam))
                        return Rollback(tx, "시상 기록이 있는 팀은 없앨 수 없어요. 관리자에게 문의해 주세요.");
                }

                if (await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyTeam WHERE FamilyTeamId=@t AND IsActive=1;", ("@t", teamId)) == 0)
                    return Rollback(tx, "그 팀은 이제 없어요.");
                if (await PendingCountAsync(c, tx, userName) >= MaxPendingPerUser) return Rollback(tx, "보낸 신청과 초대가 너무 많아요. 일부를 취소한 뒤 다시 보내 주세요.");

                // 그 팀이 먼저 초대해 둔 상태면 바로 수락한 것으로 본다
                var inv = await ScalarAsync<int>(c, tx,
                    "SELECT TOP 1 Id FROM dbo.DM2_FamilyRequest WHERE FamilyTeamId=@t AND Kind='I' AND TargetUserName=@u;", ("@t", teamId), ("@u", userName));
                if (inv != 0)
                {
                    if (myTeam == 0)
                    {
                        tx.Rollback();
                        return await AcceptInviteAsync(userName, inv);
                    }

                    // 혼자인 내 팀을 지우고 초대한 팀으로 바로 들어간다 (한 트랜잭션)
                    var invited = await ReadRequestAsync(c, tx, inv);
                    await RemoveMemberAsync(c, tx, myTeam, userName, hardDeleteIfEmpty: true);
                    await InsertMemberAsync(c, tx, teamId, userName, invited!.RelationKey);
                    await ClearPendingOfAsync(c, tx, userName);
                    await ClearPendingIfFullAsync(c, tx, teamId);
                    tx.Commit();
                    return FamilyTeamResult.Success("초대받은 팀에 들어왔어요. 내가 만든 팀은 삭제했어요.", teamId);
                }

                if (await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyRequest WHERE FamilyTeamId=@t AND Kind='R' AND TargetUserName=@u;", ("@t", teamId), ("@u", userName)) > 0)
                    return Rollback(tx, "이미 신청했어요. 수락을 기다려 주세요.");

                // 한 번에 한 팀에만 신청할 수 있다 — 다른 팀에 신청 중이면 그 신청을 취소해야 한다
                if (await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyRequest WHERE Kind='R' AND TargetUserName=@u;", ("@u", userName)) > 0)
                    return Rollback(tx, "다른 팀에 가입 신청 중이에요. 그 신청을 취소한 뒤 다시 신청해 주세요.");

                // 모든 확인을 통과했다 → 혼자인 내 팀은 지금 지운다
                if (myTeam != 0) await RemoveMemberAsync(c, tx, myTeam, userName, hardDeleteIfEmpty: true);

                await Cmd(c, tx, "INSERT INTO dbo.DM2_FamilyRequest (FamilyTeamId, Kind, TargetUserName, FromUserName, RelationKey, CreatedAt) VALUES (@t,'R',@u,@u,@k,@now);",
                    ("@t", teamId), ("@u", userName), ("@k", relationKey!), ("@now", DateHelper.GetKoreaNow())).ExecuteNonQueryAsync();
                tx.Commit();
                return FamilyTeamResult.Success(myTeam != 0
                    ? "가입 신청을 보냈어요. 내가 만든 팀은 삭제했어요. 팀원이 수락하면 팀원이 돼요."
                    : "가입 신청을 보냈어요. 팀원이 수락하면 팀원이 돼요.");
            }
            catch (Exception ex) { TryRollback(tx); return FamilyTeamResult.Fail("신청하지 못했어요. 잠시 후 다시 시도해 주세요. (" + ex.Message + ")"); }
        }

        // ── 초대 ─────────────────────────────────────────────────
        public async Task<FamilyTeamResult> InviteAsync(string userName, string targetUserName, string? relationKey)
        {
            if (string.Equals(userName, targetUserName, StringComparison.OrdinalIgnoreCase)) return FamilyTeamResult.Fail("본인은 초대할 수 없어요.");

            using var c = Conn();
            using var tx = await BeginAsync(c);
            try
            {
                int teamId = await MyTeamIdAsync(c, tx, userName);
                if (teamId == 0) return Rollback(tx, "먼저 팀에 들어가 있어야 초대할 수 있어요.");
                if (await UserExistsAsync(c, tx, targetUserName) == false) return Rollback(tx, "초대할 사람을 찾을 수 없어요.");
                if (await HasActiveMembershipAsync(c, tx, targetUserName)) return Rollback(tx, "그분은 이미 다른 팀에 있어요.");

                if (await TeamTypeOfAsync(c, tx, teamId) == TypeFriend)
                {
                    relationKey = FriendRelationKey;
                    if (await ActiveCountAsync(c, tx, teamId) >= FriendCapacity) return Rollback(tx, "친구팀은 2명까지예요. 정원이 찼어요.");
                    if (!await IsStudentAsync(c, tx, targetUserName)) return Rollback(tx, "친구팀에는 학생만 초대할 수 있어요.");
                }
                if (!TryRelation(relationKey, out _, out _)) return Rollback(tx, "관계를 골라 주세요.");
                if (await PendingCountAsync(c, tx, userName) >= MaxPendingPerUser) return Rollback(tx, "보낸 신청과 초대가 너무 많아요. 일부를 취소한 뒤 다시 보내 주세요.");

                // 그분이 우리 팀에 이미 신청해 둔 상태면 초대할 필요 없이 수락하면 된다
                if (await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyRequest WHERE FamilyTeamId=@t AND Kind='R' AND TargetUserName=@u;", ("@t", teamId), ("@u", targetUserName)) > 0)
                    return Rollback(tx, "그분이 이미 가입 신청을 보냈어요. 받은 신청에서 수락해 주세요.");
                if (await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyRequest WHERE FamilyTeamId=@t AND Kind='I' AND TargetUserName=@u;", ("@t", teamId), ("@u", targetUserName)) > 0)
                    return Rollback(tx, "이미 초대했어요.");

                await Cmd(c, tx, "INSERT INTO dbo.DM2_FamilyRequest (FamilyTeamId, Kind, TargetUserName, FromUserName, RelationKey, CreatedAt) VALUES (@t,'I',@u,@f,@k,@now);",
                    ("@t", teamId), ("@u", targetUserName), ("@f", userName), ("@k", relationKey!), ("@now", DateHelper.GetKoreaNow())).ExecuteNonQueryAsync();
                tx.Commit();
                return FamilyTeamResult.Success("초대를 보냈어요. 상대가 로그인해서 승인하면 팀원이 돼요.");
            }
            catch (Exception ex) { TryRollback(tx); return FamilyTeamResult.Fail("초대하지 못했어요. 잠시 후 다시 시도해 주세요. (" + ex.Message + ")"); }
        }

        // ── 수락 · 거절 · 취소 ───────────────────────────────────
        /// <summary>팀원이 가입 신청을 수락한다 (한 명이면 충분)</summary>
        public async Task<FamilyTeamResult> AcceptRequestAsync(string userName, int requestId)
        {
            using var c = Conn();
            using var tx = await BeginAsync(c);
            try
            {
                var r = await ReadRequestAsync(c, tx, requestId);
                if (r == null || r.Kind != "R") return Rollback(tx, "이미 처리됐거나 사라진 신청이에요.");
                if (!TryRelation(r.RelationKey, out _, out _))
                {
                    // 없어진 관계(예전 "삼촌·이모" 등)로 들어온 신청 — 지우고 다시 신청하게 한다
                    await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyRequest WHERE Id=@i;", ("@i", requestId)).ExecuteNonQueryAsync();
                    tx.Commit();
                    return FamilyTeamResult.Fail("이 신청은 없어진 관계로 들어온 신청이라 지웠어요. 상대에게 다시 신청해 달라고 해 주세요.");
                }

                int myTeam = await MyTeamIdAsync(c, tx, userName);
                if (myTeam == 0 || myTeam != r.TeamId) return Rollback(tx, "이 팀의 구성원만 수락할 수 있어요.");

                if (await HasActiveMembershipAsync(c, tx, r.Target))
                {
                    await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyRequest WHERE Id=@i;", ("@i", requestId)).ExecuteNonQueryAsync();
                    tx.Commit();
                    return FamilyTeamResult.Fail("그분은 그사이 다른 팀에 들어갔어요.");
                }
                if (await TeamTypeOfAsync(c, tx, r.TeamId) == TypeFriend && await ActiveCountAsync(c, tx, r.TeamId) >= FriendCapacity)
                {
                    await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyRequest WHERE Id=@i;", ("@i", requestId)).ExecuteNonQueryAsync();
                    tx.Commit();
                    return FamilyTeamResult.Fail("친구팀은 2명까지예요. 정원이 찼어요.");
                }

                await InsertMemberAsync(c, tx, r.TeamId, r.Target, r.RelationKey);
                string teamName = await ScalarAsync<string>(c, tx, "SELECT TeamName FROM dbo.DM2_FamilyTeam WHERE FamilyTeamId=@t;", ("@t", r.TeamId)) ?? "";
                await ClearPendingOfAsync(c, tx, r.Target);
                await ClearPendingIfFullAsync(c, tx, r.TeamId);
                await Cmd(c, tx, "INSERT INTO dbo.DM2_FamilyNotice (UserName, FamilyTeamId, TeamName, Kind, CreatedAt) VALUES (@u,@t,@n,'JOINED',@now);",
                    ("@u", r.Target), ("@t", r.TeamId), ("@n", teamName), ("@now", DateHelper.GetKoreaNow())).ExecuteNonQueryAsync();
                tx.Commit();
                return FamilyTeamResult.Success("수락했어요. 새 팀원이 들어왔어요.");
            }
            catch (Exception ex) { TryRollback(tx); return FamilyTeamResult.Fail("수락하지 못했어요. 잠시 후 다시 시도해 주세요. (" + ex.Message + ")"); }
        }

        /// <summary>팀원이 가입 신청을 거절한다 (신청은 지운다)</summary>
        public async Task<FamilyTeamResult> DeclineRequestAsync(string userName, int requestId)
        {
            using var c = Conn();
            await c.OpenAsync();
            int myTeam = await MyTeamIdAsync(c, null, userName);
            int n = myTeam == 0 ? 0 : await Cmd(c, null, "DELETE FROM dbo.DM2_FamilyRequest WHERE Id=@i AND Kind='R' AND FamilyTeamId=@t;",
                ("@i", requestId), ("@t", myTeam)).ExecuteNonQueryAsync();
            return n > 0 ? FamilyTeamResult.Success("거절했어요.") : FamilyTeamResult.Fail("이미 처리됐거나 사라진 신청이에요.");
        }

        /// <summary>초대받은 본인이 수락한다</summary>
        public async Task<FamilyTeamResult> AcceptInviteAsync(string userName, int requestId)
        {
            using var c = Conn();
            using var tx = await BeginAsync(c);
            try
            {
                var r = await ReadRequestAsync(c, tx, requestId);
                if (r == null || r.Kind != "I" || !string.Equals(r.Target, userName, StringComparison.OrdinalIgnoreCase))
                    return Rollback(tx, "이미 처리됐거나 사라진 초대예요.");
                if (!TryRelation(r.RelationKey, out _, out _))
                {
                    await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyRequest WHERE Id=@i;", ("@i", requestId)).ExecuteNonQueryAsync();
                    tx.Commit();
                    return FamilyTeamResult.Fail("이 초대는 없어진 관계로 보낸 초대라 지웠어요. 다시 초대해 달라고 해 주세요.");
                }
                if (await HasActiveMembershipAsync(c, tx, userName))
                    return Rollback(tx, "이미 팀에 들어 있어요. 한 사람은 한 팀에만 들어갈 수 있어요.");
                if (await TeamTypeOfAsync(c, tx, r.TeamId) == TypeFriend && await ActiveCountAsync(c, tx, r.TeamId) >= FriendCapacity)
                {
                    await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyRequest WHERE Id=@i;", ("@i", requestId)).ExecuteNonQueryAsync();
                    tx.Commit();
                    return FamilyTeamResult.Fail("이 친구팀은 정원이 찼어요.");
                }
                if (await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyTeam WHERE FamilyTeamId=@t AND IsActive=1;", ("@t", r.TeamId)) == 0)
                    return Rollback(tx, "그 팀은 이제 없어요.");
                if (await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyMember WHERE FamilyTeamId=@t AND IsActive=1;", ("@t", r.TeamId)) == 0)
                    return Rollback(tx, "그 팀은 이제 없어요.");

                await InsertMemberAsync(c, tx, r.TeamId, userName, r.RelationKey);
                await ClearPendingOfAsync(c, tx, userName);
                await ClearPendingIfFullAsync(c, tx, r.TeamId);
                tx.Commit();
                return FamilyTeamResult.Success("팀에 들어왔어요.", r.TeamId);
            }
            catch (Exception ex) { TryRollback(tx); return FamilyTeamResult.Fail("수락하지 못했어요. 잠시 후 다시 시도해 주세요. (" + ex.Message + ")"); }
        }

        /// <summary>초대받은 본인이 거절한다</summary>
        public async Task<FamilyTeamResult> DeclineInviteAsync(string userName, int requestId)
        {
            using var c = Conn();
            await c.OpenAsync();
            int n = await Cmd(c, null, "DELETE FROM dbo.DM2_FamilyRequest WHERE Id=@i AND Kind='I' AND TargetUserName=@u;",
                ("@i", requestId), ("@u", userName)).ExecuteNonQueryAsync();
            return n > 0 ? FamilyTeamResult.Success("거절했어요.") : FamilyTeamResult.Fail("이미 처리됐거나 사라진 초대예요.");
        }

        /// <summary>보낸 사람이 자기가 보낸 신청 · 초대를 취소한다</summary>
        public async Task<FamilyTeamResult> CancelAsync(string userName, int requestId)
        {
            using var c = Conn();
            await c.OpenAsync();
            int n = await Cmd(c, null, "DELETE FROM dbo.DM2_FamilyRequest WHERE Id=@i AND FromUserName=@u;",
                ("@i", requestId), ("@u", userName)).ExecuteNonQueryAsync();
            return n > 0 ? FamilyTeamResult.Success("취소했어요.") : FamilyTeamResult.Fail("이미 처리됐거나 사라진 요청이에요.");
        }

        // ── 탈퇴 · 내보내기 ──────────────────────────────────────
        public async Task<FamilyTeamResult> LeaveAsync(string userName)
        {
            using var c = Conn();
            using var tx = await BeginAsync(c);
            try
            {
                int teamId = await MyTeamIdAsync(c, tx, userName);
                if (teamId == 0) return Rollback(tx, "가족팀에 들어 있지 않아요.");
                // 시상 기록이 있는 팀은 누구도 나갈 수 없다 (팀을 옮겨 다니며 재시상 받는 것을 막는다)
                if (await HasAwardRecordAsync(c, tx, teamId))
                    return Rollback(tx, "시상 기록이 있는 팀은 탈퇴할 수 없어요. 관리자에게 문의해 주세요.");

                // 방장은 나갈 수 없다. 예외: 혼자이면(시상 기록이 없으므로) 나갈 수 있고 팀은 지워진다
                if (string.Equals(await CreatorOfAsync(c, tx, teamId), userName, StringComparison.OrdinalIgnoreCase))
                {
                    int others = await ScalarAsync<int>(c, tx,
                        "SELECT COUNT(*) FROM dbo.DM2_FamilyMember WITH (UPDLOCK, HOLDLOCK) WHERE FamilyTeamId=@t AND IsActive=1 AND UserName<>@u;", ("@t", teamId), ("@u", userName));
                    if (others > 0) return Rollback(tx, "방장은 팀에서 나갈 수 없어요.");

                    await RemoveMemberAsync(c, tx, teamId, userName, hardDeleteIfEmpty: true);
                    tx.Commit();
                    return FamilyTeamResult.Success("팀에서 나왔어요. 팀을 삭제했어요.");
                }

                await RemoveMemberAsync(c, tx, teamId, userName, hardDeleteIfEmpty: false);
                tx.Commit();
                return FamilyTeamResult.Success("팀에서 나왔어요.");
            }
            catch (Exception ex) { TryRollback(tx); return FamilyTeamResult.Fail("탈퇴하지 못했어요. 잠시 후 다시 시도해 주세요. (" + ex.Message + ")"); }
        }

        /// <summary>방장만 다른 구성원을 내보낼 수 있다</summary>
        public async Task<FamilyTeamResult> KickAsync(string userName, string targetUserName)
        {
            if (string.Equals(userName, targetUserName, StringComparison.OrdinalIgnoreCase)) return await LeaveAsync(userName);

            using var c = Conn();
            using var tx = await BeginAsync(c);
            try
            {
                int teamId = await MyTeamIdAsync(c, tx, userName);
                if (teamId == 0) return Rollback(tx, "가족팀에 들어 있지 않아요.");
                int targetTeam = await MyTeamIdAsync(c, tx, targetUserName);
                if (targetTeam != teamId) return Rollback(tx, "같은 팀의 구성원만 내보낼 수 있어요.");
                if (!string.Equals(await CreatorOfAsync(c, tx, teamId), userName, StringComparison.OrdinalIgnoreCase))
                    return Rollback(tx, "방장만 내보낼 수 있어요.");
                // 시상 기록이 있는 팀은 구성원을 옮길 수 없다 (내보낸 사람이 다른 팀에서 재시상 받는 것을 막는다)
                if (await HasAwardRecordAsync(c, tx, teamId))
                    return Rollback(tx, "시상 기록이 있는 팀은 구성원을 내보낼 수 없어요. 관리자에게 문의해 주세요.");
                await RemoveMemberAsync(c, tx, teamId, targetUserName, hardDeleteIfEmpty: false);
                tx.Commit();
                return FamilyTeamResult.Success("팀에서 내보냈어요.");
            }
            catch (Exception ex) { TryRollback(tx); return FamilyTeamResult.Fail("내보내지 못했어요. 잠시 후 다시 시도해 주세요. (" + ex.Message + ")"); }
        }

        // ── 관계 바꾸기 ──────────────────────────────────────────
        /// <summary>
        /// 구성원의 관계를 바꾼다. 본인은 자기 것, 방장은 모든 구성원의 것을 바꿀 수 있다.
        /// 시상 기록이 있는 팀은 선물 받는 사람(학생 본인)이 달라질 수 있어서 아무도 바꿀 수 없다.
        /// </summary>
        public async Task<FamilyTeamResult> ChangeRelationAsync(string userName, string targetUserName, string relationKey)
        {
            if (!TryRelation(relationKey, out int role, out string label)) return FamilyTeamResult.Fail("관계를 골라 주세요.");

            using var c = Conn();
            using var tx = await BeginAsync(c);
            try
            {
                int teamId = await MyTeamIdAsync(c, tx, userName);
                if (teamId == 0) return Rollback(tx, "가족팀에 들어 있지 않아요.");
                if (await TeamTypeOfAsync(c, tx, teamId) == TypeFriend) return Rollback(tx, "친구팀은 관계를 정하지 않아요.");
                if (await MyTeamIdAsync(c, tx, targetUserName) != teamId) return Rollback(tx, "같은 팀의 구성원만 바꿀 수 있어요.");

                bool self = string.Equals(userName, targetUserName, StringComparison.OrdinalIgnoreCase);
                bool owner = string.Equals(await CreatorOfAsync(c, tx, teamId), userName, StringComparison.OrdinalIgnoreCase);
                if (!self && !owner) return Rollback(tx, "다른 구성원의 관계는 방장만 바꿀 수 있어요.");
                if (await HasAwardRecordAsync(c, tx, teamId))
                    return Rollback(tx, "시상 기록이 있는 팀은 관계를 바꿀 수 없어요. 관리자에게 문의해 주세요.");

                int curRole = 0, curRel = 9;
                using (var rd = await Cmd(c, tx, "SELECT RoleCode, RelationCode FROM dbo.DM2_FamilyMember WITH (UPDLOCK, HOLDLOCK) WHERE FamilyTeamId=@t AND UserName=@u AND IsActive=1;", ("@t", teamId), ("@u", targetUserName)).ExecuteReaderAsync())
                {
                    if (!await rd.ReadAsync()) return Rollback(tx, "구성원을 찾을 수 없어요.");
                    curRole = Convert.ToInt32(rd[0]); curRel = Convert.ToInt32(rd[1]);
                }
                if (curRole == role) return Rollback(tx, "지금 관계와 같아요.");

                // 팀마다 '본인(학생, 관계 0)' 은 한 명만 — 이미 있으면 일반 학생(9)으로 둔다
                int rel = 9;
                if (role == 0)
                {
                    bool selfTaken = await ScalarAsync<int>(c, tx,
                        "SELECT COUNT(*) FROM dbo.DM2_FamilyMember WITH (UPDLOCK, HOLDLOCK) WHERE FamilyTeamId=@t AND IsActive=1 AND RoleCode=0 AND RelationCode=0 AND UserName<>@u;", ("@t", teamId), ("@u", targetUserName)) > 0;
                    rel = selfTaken ? 9 : 0;
                }

                await Cmd(c, tx, "UPDATE dbo.DM2_FamilyMember SET RoleCode=@role, RelationCode=@rel, RelationLabel=@lbl, UpdateDT=GETDATE() WHERE FamilyTeamId=@t AND UserName=@u;",
                    ("@role", (byte)role), ("@rel", (byte)rel), ("@lbl", label), ("@t", teamId), ("@u", targetUserName)).ExecuteNonQueryAsync();
                tx.Commit();
                return FamilyTeamResult.Success(self ? $"내 관계를 {label}(으)로 바꿨어요." : $"관계를 {label}(으)로 바꿨어요.");
            }
            catch (Exception ex) { TryRollback(tx); return FamilyTeamResult.Fail("관계를 바꾸지 못했어요. 잠시 후 다시 시도해 주세요. (" + ex.Message + ")"); }
        }

        // ── 팀 이름 바꾸기 ───────────────────────────────────────
        public const int MaxTeamNameLength = 30;

        /// <summary>팀원이면 누구나 바꿀 수 있다. 시상을 찾는 팀 키는 그대로다</summary>
        public async Task<FamilyTeamResult> RenameTeamAsync(string userName, string? newName)
        {
            string name = (newName ?? "").Trim();
            if (name.Length == 0) return FamilyTeamResult.Fail("팀 이름을 적어 주세요.");
            if (name.Length > MaxTeamNameLength) return FamilyTeamResult.Fail($"팀 이름은 {MaxTeamNameLength}자까지 쓸 수 있어요.");

            using var c = Conn();
            using var tx = await BeginAsync(c);
            try
            {
                int teamId = await MyTeamIdAsync(c, tx, userName);
                if (teamId == 0) return Rollback(tx, "가족팀에 들어 있지 않아요.");
                string cur = await ScalarAsync<string>(c, tx, "SELECT TeamName FROM dbo.DM2_FamilyTeam WHERE FamilyTeamId=@t;", ("@t", teamId)) ?? "";
                if (string.Equals(cur, name, StringComparison.Ordinal)) return Rollback(tx, "지금 쓰는 이름과 같아요.");
                string key = await ScalarAsync<string>(c, tx, "SELECT ISNULL(AwardKey, TeamName) FROM dbo.DM2_FamilyTeam WHERE FamilyTeamId=@t;", ("@t", teamId)) ?? "";

                // 우리 팀의 처음 이름(팀 키)으로 되돌리는 것은 괜찮다
                if (!string.Equals(key, name, StringComparison.OrdinalIgnoreCase) && await NameTakenAsync(c, tx, name))
                    return Rollback(tx, "이미 쓰이는 이름이에요. 다른 이름을 적어 주세요.");

                await Cmd(c, tx, "UPDATE dbo.DM2_FamilyTeam SET TeamName=@n, UpdateDT=GETDATE() WHERE FamilyTeamId=@t;", ("@n", name), ("@t", teamId)).ExecuteNonQueryAsync();
                tx.Commit();
                return FamilyTeamResult.Success("팀 이름을 바꿨어요.");
            }
            catch (Exception ex) { TryRollback(tx); return FamilyTeamResult.Fail("이름을 바꾸지 못했어요. 잠시 후 다시 시도해 주세요. (" + ex.Message + ")"); }
        }

        // ── 안쪽 도우미 ──────────────────────────────────────────
        private sealed class ReqRow { public int TeamId; public string Kind = ""; public string Target = ""; public string RelationKey = ""; }

        private static async Task<ReqRow?> ReadRequestAsync(SqlConnection c, SqlTransaction tx, int id)
        {
            using var rd = await Cmd(c, tx, "SELECT FamilyTeamId, Kind, TargetUserName, RelationKey FROM dbo.DM2_FamilyRequest WITH (UPDLOCK, HOLDLOCK) WHERE Id=@i;", ("@i", id)).ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return null;
            return new ReqRow { TeamId = rd.GetInt32(0), Kind = rd.GetString(1), Target = rd.GetString(2), RelationKey = rd.GetString(3) };
        }

        private static FamilyTeamResult Rollback(SqlTransaction tx, string message)
        {
            TryRollback(tx);
            return FamilyTeamResult.Fail(message);
        }

        private static void TryRollback(SqlTransaction tx) { try { tx.Rollback(); } catch { } }

        private static async Task<bool?> UserExistsAsync(SqlConnection c, SqlTransaction tx, string userName)
            => await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM [Identity].[User] WHERE UserName=@u;", ("@u", userName)) > 0;

        private static async Task<bool> HasActiveMembershipAsync(SqlConnection c, SqlTransaction tx, string userName)
            => await ScalarAsync<int>(c, tx,
                @"SELECT COUNT(*) FROM dbo.DM2_FamilyMember m
                  JOIN dbo.DM2_FamilyTeam ft ON ft.FamilyTeamId = m.FamilyTeamId AND ft.IsActive = 1
                  WHERE m.UserName=@u AND m.IsActive=1;", ("@u", userName)) > 0;

        private static async Task<int> PendingCountAsync(SqlConnection c, SqlTransaction tx, string userName)
            => await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyRequest WHERE FromUserName=@u;", ("@u", userName));

        /// <summary>팀 이름이 이미 쓰였나 — 숨겨진 팀과, 선물 기록("가족합산:팀이름")에 남은 이름도 쓰인 것으로 본다</summary>
        private static async Task<bool> NameTakenAsync(SqlConnection c, SqlTransaction tx, string name)
        {
            const string sql = @"
DECLARE @r INT = 0;
IF EXISTS (SELECT 1 FROM dbo.DM2_FamilyTeam WITH (UPDLOCK, HOLDLOCK) WHERE TeamName = @n OR AwardKey = @n) SET @r = 1;
IF @r = 0 AND OBJECT_ID(N'dbo.StudentPointGiftOrder') IS NOT NULL
    EXEC sp_executesql N'IF EXISTS (SELECT 1 FROM dbo.StudentPointGiftOrder WHERE Memo = @p OR LEFT(Memo, LEN(@p) + 1) = @p + N'':'' OR LEFT(Memo, LEN(@q) + 1) = @q + N'':'') SET @o = 1',
         N'@p NVARCHAR(300), @q NVARCHAR(300), @o INT OUTPUT', @p = @pref, @q = @prefF, @o = @r OUTPUT;
SELECT @r;";
            return await ScalarAsync<int>(c, tx, sql, ("@n", name), ("@pref", "가족합산:" + name), ("@prefF", "친구합산:" + name)) == 1;
        }

        /// <summary>새 구성원을 넣는다. 예전에 숨겨진 줄(IsActive=0)이 이 사람의 표 전체 유일 칸(UserName)을 막으니 먼저 지운다</summary>
        private static async Task InsertMemberAsync(SqlConnection c, SqlTransaction tx, int teamId, string userName, string relationKey)
        {
            TryRelation(relationKey, out int role, out string label);

            await Cmd(c, tx,
                @"DELETE m FROM dbo.DM2_FamilyMember m
                  LEFT JOIN dbo.DM2_FamilyTeam ft ON ft.FamilyTeamId = m.FamilyTeamId
                  WHERE m.UserName = @u AND (m.IsActive = 0 OR ft.FamilyTeamId IS NULL OR ft.IsActive = 0);", ("@u", userName)).ExecuteNonQueryAsync();
            if (await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyMember WHERE UserName=@u;", ("@u", userName)) > 0)
                throw new InvalidOperationException("이미 가족팀에 들어 있어요.");

            // 팀마다 '본인(학생, 관계 0)' 은 한 명만 둘 수 있다 — 첫 자녀는 0, 그다음부터는 9
            int rel = 9;
            if (role == 0)
            {
                bool selfTaken = await ScalarAsync<int>(c, tx,
                    "SELECT COUNT(*) FROM dbo.DM2_FamilyMember WITH (UPDLOCK, HOLDLOCK) WHERE FamilyTeamId=@t AND IsActive=1 AND RoleCode=0 AND RelationCode=0;", ("@t", teamId)) > 0;
                rel = selfTaken ? 9 : 0;
            }
            int sort = await ScalarAsync<int>(c, tx, "SELECT ISNULL(MAX(SortNo),0)+1 FROM dbo.DM2_FamilyMember WHERE FamilyTeamId=@t;", ("@t", teamId));

            await Cmd(c, tx,
                @"INSERT INTO dbo.DM2_FamilyMember (FamilyTeamId, UserName, RoleCode, SortNo, IsActive, RelationCode, RelationLabel)
                  VALUES (@t, @u, @role, @sort, 1, @rel, @lbl);",
                ("@t", teamId), ("@u", userName), ("@role", (byte)role), ("@sort", sort), ("@rel", (byte)rel), ("@lbl", label)).ExecuteNonQueryAsync();
        }

        /// <summary>이 사람이 보내거나 받은 다른 신청 · 초대를 모두 지운다 (한 팀에만 들어가니까)</summary>
        private static async Task ClearPendingOfAsync(SqlConnection c, SqlTransaction tx, string userName)
            => await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyRequest WHERE TargetUserName=@u OR FromUserName=@u;", ("@u", userName)).ExecuteNonQueryAsync();

        /// <summary>
        /// 구성원을 뺀다. 아무도 남지 않으면 팀을 정리한다.
        ///  hardDeleteIfEmpty = true  : 표에서 지운다 (혼자 + 시상 기록 없는 팀이 다른 팀에 신청할 때만)
        ///  hardDeleteIfEmpty = false : 숨기기만 한다 (팀 키와 이름을 남겨 재시상을 막는다)
        /// </summary>
        private static async Task RemoveMemberAsync(SqlConnection c, SqlTransaction tx, int teamId, string userName, bool hardDeleteIfEmpty)
        {
            await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyMember WHERE FamilyTeamId=@t AND UserName=@u;", ("@t", teamId), ("@u", userName)).ExecuteNonQueryAsync();
            // 이 사람이 보낸 신청 · 초대는 그만 둔다
            await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyRequest WHERE FromUserName=@u;", ("@u", userName)).ExecuteNonQueryAsync();

            int left = await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyMember WITH (UPDLOCK, HOLDLOCK) WHERE FamilyTeamId=@t AND IsActive=1;", ("@t", teamId));
            if (left > 0) return;

            await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyRequest WHERE FamilyTeamId=@t;", ("@t", teamId)).ExecuteNonQueryAsync();
            if (hardDeleteIfEmpty)
            {
                await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyMember WHERE FamilyTeamId=@t;", ("@t", teamId)).ExecuteNonQueryAsync();
                await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyTeam WHERE FamilyTeamId=@t;", ("@t", teamId)).ExecuteNonQueryAsync();
            }
            else
            {
                await Cmd(c, tx, "UPDATE dbo.DM2_FamilyTeam SET IsActive=0, UpdateDT=GETDATE() WHERE FamilyTeamId=@t;", ("@t", teamId)).ExecuteNonQueryAsync();
            }
        }

        private const string StudentRoleCode = "T1001";   // RoleCodeType.T1001 — StudentPointGiftService.IsStudentAsync 와 같은 기준

        /// <summary>학생인가 — 선물 쪽과 같은 기준</summary>
        public async Task<bool> IsStudentAsync(string userName)
        {
            using var c = Conn();
            await c.OpenAsync();
            return await IsStudentAsync(c, null, userName);
        }

        private static async Task<bool> IsStudentAsync(SqlConnection c, SqlTransaction? tx, string userName)
            => string.Equals(userName, "test@gmail.com", StringComparison.OrdinalIgnoreCase)
               || await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.view_UserCompositeInfo_2026 WHERE UserName=@u AND RoleCode=@r;", ("@u", userName), ("@r", StudentRoleCode)) > 0;

        private static async Task<string> TeamTypeOfAsync(SqlConnection c, SqlTransaction? tx, int teamId)
            => await ScalarAsync<string>(c, tx, "SELECT TeamType FROM dbo.DM2_FamilyTeam WHERE FamilyTeamId=@t;", ("@t", teamId)) ?? TypeFamily;

        private static async Task<int> ActiveCountAsync(SqlConnection c, SqlTransaction tx, int teamId)
            => await ScalarAsync<int>(c, tx, "SELECT COUNT(*) FROM dbo.DM2_FamilyMember WITH (UPDLOCK, HOLDLOCK) WHERE FamilyTeamId=@t AND IsActive=1;", ("@t", teamId));

        /// <summary>친구팀이 2명이 되면 대기 중인 신청 · 초대를 지운다</summary>
        private static async Task ClearPendingIfFullAsync(SqlConnection c, SqlTransaction tx, int teamId)
        {
            if (await TeamTypeOfAsync(c, tx, teamId) != TypeFriend) return;
            if (await ActiveCountAsync(c, tx, teamId) < FriendCapacity) return;
            await Cmd(c, tx, "DELETE FROM dbo.DM2_FamilyRequest WHERE FamilyTeamId=@t;", ("@t", teamId)).ExecuteNonQueryAsync();
        }

        private static async Task<string> CreatorOfAsync(SqlConnection c, SqlTransaction? tx, int teamId)
            => await ScalarAsync<string>(c, tx, "SELECT ISNULL(CreatorUserName, N'') FROM dbo.DM2_FamilyTeam WHERE FamilyTeamId=@t;", ("@t", teamId)) ?? "";

        /// <summary>이 팀에 시상 기록이 있는가 — 선물 기록(메모의 팀 키) 또는 박수 숨김 기록</summary>
        private static async Task<bool> HasAwardRecordAsync(SqlConnection c, SqlTransaction? tx, int teamId)
        {
            string key = await ScalarAsync<string>(c, tx, "SELECT ISNULL(AwardKey, TeamName) FROM dbo.DM2_FamilyTeam WHERE FamilyTeamId=@t;", ("@t", teamId)) ?? "";
            const string sql = @"
DECLARE @r INT = 0;
IF EXISTS (SELECT 1 FROM dbo.DM2_FamilyMilestoneHidden WHERE FamilyTeamId = @t) SET @r = 1;
IF @r = 0 AND OBJECT_ID(N'dbo.StudentPointGiftOrder') IS NOT NULL
    EXEC sp_executesql N'IF EXISTS (SELECT 1 FROM dbo.StudentPointGiftOrder WHERE Memo = @p OR LEFT(Memo, LEN(@p) + 1) = @p + N'':'' OR LEFT(Memo, LEN(@q) + 1) = @q + N'':'') SET @o = 1',
         N'@p NVARCHAR(300), @q NVARCHAR(300), @o INT OUTPUT', @p = @pref, @q = @prefF, @o = @r OUTPUT;
SELECT @r;";
            return await ScalarAsync<int>(c, tx, sql, ("@t", teamId), ("@pref", "가족합산:" + key), ("@prefF", "친구합산:" + key)) == 1;
        }
    }
}

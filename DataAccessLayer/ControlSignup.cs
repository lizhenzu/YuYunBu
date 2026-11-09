using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BulletinBoard.Common;
using Microsoft.Data.SqlClient;

namespace BulletinBoard.DataAccessLayer
{
    /// <summary>가입 후 마법사로 낸 가입 신청 한 건</summary>
    public class SignupApplication
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string RoleKind { get; set; } = "";     // S 학생 · T 교사 · P 학부모 · E 기타(T1099)
        public int PersonId { get; set; }               // 같은 이름 기록 중 "이 사람이 저예요" 로 고른 것 (0 = 새로 등록 / 해당 없음)
        public int ClassYear { get; set; }              // 0 = 잘 모르겠어요
        public int ClassBan { get; set; }
        public int BMonth { get; set; }
        public int BDay { get; set; }
        public string Sex { get; set; } = "";           // M · F
        public string School { get; set; } = "";
        public string Phone { get; set; } = "";         // 학생: 부모님 연락처 · 교사/학부모: 본인 연락처
        public string Kids { get; set; } = "";          // 학부모: "김하늘 2학년 3반, …" (참고용)
        public string ReqType { get; set; } = "JOIN";   // JOIN 가입 직후 · CHANGE 역할 변경
        public string PrevRole { get; set; } = "";      // 신청 당시 역할(표시용) — "학생 2학년 3반", "교사", "역할 없음"
        public string Status { get; set; } = "";        // PENDING · APPROVED · REJECTED
        public string RejectReason { get; set; } = "";
        public DateTime InsertDT { get; set; }

        // 승인 화면용 — 연결하려는 기존 기록
        public string TargetFullName { get; set; } = "";
        public string TargetUserName { get; set; } = "";
        public int TargetClassYear { get; set; }
        public int TargetClassBan { get; set; }

        public string RoleLabel => RoleKind == "S" ? "학생" : RoleKind == "T" ? "교사" : RoleKind == "P" ? "학부모" : RoleKind == "E" ? "기타" : "";
        public bool IsChange => ReqType == "CHANGE";
    }

    /// <summary>지금 역할 (view_UserCompositeInfo_2026 기준)</summary>
    public class CurrentRole
    {
        public string RoleCode { get; set; } = "";
        public string RoleNM { get; set; } = "";
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
        public string Kind => RoleCode == "T1001" ? "S" : RoleCode == "T1002" || RoleCode == "T1003" ? "T" : RoleCode == UserRole.EtcCode ? "E" : RoleCode.Length > 0 ? "X" : "";
        public string Label =>
            RoleCode == "T1001" ? "학생" + (ClassYear > 0 && ClassBan > 0 ? $" {ClassYear}학년 {ClassBan}반" : "")
            : RoleCode.Length > 0 ? (string.IsNullOrWhiteSpace(RoleNM) ? RoleCode : RoleNM)
            : "역할 없음(학부모)";
    }

    /// <summary>
    /// 가입 마법사 신청 저장 · 관리자 승인/반려.
    /// 승인하면 사람 명부(PersonMaster_2026) · 반(ClassInfo_2026) · 역할(DeptRoleUserAssoc)을 한 트랜잭션으로 반영한다.
    /// 학부모는 역할 코드 없이 사람 명부(이름 · 연락처)만 만든다. 자녀 정보는 참고용으로 신청서에만 남긴다.
    /// </summary>
    public class ControlSignup
    {
        private static async Task EnsureTableAsync(SqlConnection conn)
        {
            const string sql = @"
                IF OBJECT_ID(N'[dbo].[SignupApplication]', N'U') IS NULL
                CREATE TABLE [dbo].[SignupApplication] (
                    [Id]           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [UserId]       NVARCHAR(450) NOT NULL,
                    [UserName]     NVARCHAR(256) NOT NULL,
                    [FullName]     NVARCHAR(100) NOT NULL,
                    [RoleKind]     CHAR(1) NOT NULL,
                    [PersonId]     INT NOT NULL,
                    [ClassYear]    INT NOT NULL,
                    [ClassBan]     INT NOT NULL,
                    [BMonth]       INT NOT NULL,
                    [BDay]         INT NOT NULL,
                    [Sex]          NVARCHAR(1) NULL,
                    [School]       NVARCHAR(200) NULL,
                    [Phone]        NVARCHAR(50) NULL,
                    [Kids]         NVARCHAR(1000) NULL,
                    [ReqType]      NVARCHAR(10) NOT NULL,
                    [PrevRole]     NVARCHAR(100) NULL,
                    [Status]       NVARCHAR(10) NOT NULL,
                    [RejectReason] NVARCHAR(300) NULL,
                    [InsertDT]     DATETIME NOT NULL,
                    [DecidedDT]    DATETIME NULL,
                    [DecidedBy]    NVARCHAR(256) NULL
                );";
            using (var cmd = new SqlCommand(sql, conn)) { await cmd.ExecuteNonQueryAsync(); }
        }

        private static SqlConnection Conn() => new SqlConnection(DataAccessModule.connectionString_Server);

        // ── 신청 ──────────────────────────────────────────────────────
        /// <summary>신청 저장 — 대기 중인 신청이 있으면 새로 만들지 않는다. 새로 저장했으면 true</summary>
        public async Task<bool> CreateAsync(SignupApplication a)
        {
            using (var conn = Conn())
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                const string sql = @"
                    IF EXISTS (SELECT 1 FROM [dbo].[SignupApplication] WHERE [UserId] = @UserId AND [Status] = N'PENDING')
                        SELECT 0;
                    ELSE
                    BEGIN
                        INSERT INTO [dbo].[SignupApplication]
                            ([UserId],[UserName],[FullName],[RoleKind],[PersonId],[ClassYear],[ClassBan],[BMonth],[BDay],[Sex],[School],[Phone],[Kids],[ReqType],[PrevRole],[Status],[InsertDT])
                        VALUES
                            (@UserId,@UserName,@FullName,@RoleKind,@PersonId,@ClassYear,@ClassBan,@BMonth,@BDay,@Sex,@School,@Phone,@Kids,@ReqType,@PrevRole,N'PENDING',@Now);
                        SELECT 1;
                    END";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", a.UserId);
                    cmd.Parameters.AddWithValue("@UserName", a.UserName);
                    cmd.Parameters.AddWithValue("@FullName", a.FullName);
                    cmd.Parameters.AddWithValue("@RoleKind", a.RoleKind);
                    cmd.Parameters.AddWithValue("@PersonId", a.PersonId);
                    cmd.Parameters.AddWithValue("@ClassYear", a.ClassYear);
                    cmd.Parameters.AddWithValue("@ClassBan", a.ClassBan);
                    cmd.Parameters.AddWithValue("@BMonth", a.BMonth);
                    cmd.Parameters.AddWithValue("@BDay", a.BDay);
                    cmd.Parameters.AddWithValue("@Sex", Db(a.Sex));
                    cmd.Parameters.AddWithValue("@School", Db(a.School));
                    cmd.Parameters.AddWithValue("@Phone", Db(a.Phone));
                    cmd.Parameters.AddWithValue("@Kids", Db(a.Kids));
                    cmd.Parameters.AddWithValue("@ReqType", a.ReqType == "CHANGE" ? "CHANGE" : "JOIN");
                    cmd.Parameters.AddWithValue("@PrevRole", Db(a.PrevRole));
                    cmd.Parameters.AddWithValue("@Now", SharedComm.GetKoreaNow());
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 1;
                }
            }
        }

        private static object Db(string? s) => string.IsNullOrWhiteSpace(s) ? DBNull.Value : s.Trim();

        /// <summary>이 계정의 가장 최근 신청 (없으면 null)</summary>
        public async Task<SignupApplication?> GetLatestOfAsync(string userId)
        {
            using (var conn = Conn())
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                var list = await QueryAsync(conn, "WHERE a.[UserId] = @k", "TOP 1", "a.[Id] DESC", ("@k", userId));
                return list.Count > 0 ? list[0] : null;
            }
        }

        /// <summary>대기 중인 신청. reqType: "JOIN" 가입 직후 · "CHANGE" 역할 변경 · null 이면 둘 다</summary>
        public async Task<List<SignupApplication>> GetPendingAsync(string? reqType = null)
        {
            using (var conn = Conn())
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                if (reqType == "JOIN" || reqType == "CHANGE")
                    return await QueryAsync(conn, "WHERE a.[Status] = N'PENDING' AND a.[ReqType] = @t", "", "a.[Id]", ("@t", reqType));
                return await QueryAsync(conn, "WHERE a.[Status] = N'PENDING'", "", "a.[Id]");
            }
        }

        /// <summary>대기 중인 신청 개수 (가입 · 역할 변경 따로) — 메뉴 배지용</summary>
        public async Task<(int Join, int Change)> GetPendingCountsAsync()
        {
            using (var conn = Conn())
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                using (var cmd = new SqlCommand(@"SELECT
                        ISNULL(SUM(CASE WHEN [ReqType] = N'JOIN' THEN 1 ELSE 0 END), 0),
                        ISNULL(SUM(CASE WHEN [ReqType] = N'CHANGE' THEN 1 ELSE 0 END), 0)
                    FROM [dbo].[SignupApplication] WHERE [Status] = N'PENDING'", conn))
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    await rd.ReadAsync();
                    return (Convert.ToInt32(rd[0]), Convert.ToInt32(rd[1]));
                }
            }
        }

        /// <summary>승인을 기다리는 가입자 아이디 모음 (미배정 회원 화면 표시용)</summary>
        public async Task<HashSet<string>> GetPendingUserNamesAsync()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var a in await GetPendingAsync()) set.Add(a.UserName);
            return set;
        }

        private static async Task<List<SignupApplication>> QueryAsync(SqlConnection conn, string where, string top, string order, params (string, object)[] ps)
        {
            var list = new List<SignupApplication>();
            string sql = $@"
                SELECT {top} a.[Id],a.[UserId],a.[UserName],a.[FullName],a.[RoleKind],a.[PersonId],a.[ClassYear],a.[ClassBan],a.[BMonth],a.[BDay],
                       ISNULL(a.[Sex],N''),ISNULL(a.[School],N''),ISNULL(a.[Phone],N''),ISNULL(a.[Kids],N''),a.[Status],ISNULL(a.[RejectReason],N''),a.[InsertDT],
                       ISNULL(pm.[fullName],N''),ISNULL(pm.[UserName],N''),ISNULL(ci.[ClassYear],0),ISNULL(ci.[ClassBan],0),
                       a.[ReqType],ISNULL(a.[PrevRole],N'')
                FROM [dbo].[SignupApplication] a
                LEFT JOIN [dbo].[PersonMaster_2026] pm ON pm.[PersonId] = a.[PersonId] AND a.[PersonId] > 0
                LEFT JOIN [dbo].[ClassInfo_2026] ci ON ci.[PersonID] = pm.[PersonId]
                {where}
                ORDER BY {order}";
            using (var cmd = new SqlCommand(sql, conn))
            {
                foreach (var p in ps) cmd.Parameters.AddWithValue(p.Item1, p.Item2);
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        list.Add(new SignupApplication
                        {
                            Id = rd.GetInt32(0),
                            UserId = rd.GetString(1),
                            UserName = rd.GetString(2),
                            FullName = rd.GetString(3),
                            RoleKind = Convert.ToString(rd[4])?.Trim() ?? "",
                            PersonId = rd.GetInt32(5),
                            ClassYear = rd.GetInt32(6),
                            ClassBan = rd.GetInt32(7),
                            BMonth = rd.GetInt32(8),
                            BDay = rd.GetInt32(9),
                            Sex = rd.GetString(10),
                            School = rd.GetString(11),
                            Phone = rd.GetString(12),
                            Kids = rd.GetString(13),
                            Status = rd.GetString(14),
                            RejectReason = rd.GetString(15),
                            InsertDT = rd.GetDateTime(16),
                            TargetFullName = rd.GetString(17),
                            TargetUserName = rd.GetString(18),
                            TargetClassYear = Convert.ToInt32(rd[19]),
                            TargetClassBan = Convert.ToInt32(rd[20]),
                            ReqType = rd.GetString(21),
                            PrevRole = rd.GetString(22)
                        });
                    }
                }
            }
            return list;
        }

        // ── 취소 (신청한 본인) ───────────────────────────────────────────
        /// <summary>대기 중인 내 신청을 취소한다. 본인 것만, 대기 중일 때만. 취소됐으면 true</summary>
        public async Task<bool> CancelAsync(int id, string userId)
        {
            using (var conn = Conn())
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                using (var cmd = new SqlCommand(@"UPDATE [dbo].[SignupApplication]
                        SET [Status] = N'CANCELED', [DecidedDT] = @Now, [DecidedBy] = @By
                        WHERE [Id] = @Id AND [UserId] = @UserId AND [Status] = N'PENDING'", conn))
                {
                    cmd.Parameters.AddWithValue("@Now", SharedComm.GetKoreaNow());
                    cmd.Parameters.AddWithValue("@By", userId);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        // ── 반려 ──────────────────────────────────────────────────────
        public async Task<SignupApplication?> RejectAsync(int id, string reason, string decidedBy)
        {
            using (var conn = Conn())
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                var found = await QueryAsync(conn, "WHERE a.[Id] = @k AND a.[Status] = N'PENDING'", "", "a.[Id]", ("@k", id));
                if (found.Count == 0) return null;
                using (var cmd = new SqlCommand(@"UPDATE [dbo].[SignupApplication]
                        SET [Status] = N'REJECTED', [RejectReason] = @R, [DecidedDT] = @Now, [DecidedBy] = @By
                        WHERE [Id] = @Id AND [Status] = N'PENDING'", conn))
                {
                    cmd.Parameters.AddWithValue("@R", Db(reason));
                    cmd.Parameters.AddWithValue("@Now", SharedComm.GetKoreaNow());
                    cmd.Parameters.AddWithValue("@By", decidedBy);
                    cmd.Parameters.AddWithValue("@Id", id);
                    if (await cmd.ExecuteNonQueryAsync() == 0) return null;
                }
                return found[0];
            }
        }

        // ── 승인 ──────────────────────────────────────────────────────
        /// <summary>
        /// 승인. linkExisting: 신청자가 고른 같은 이름 기록에 연결할지(false 면 새로 등록).
        /// classYear/classBan: 관리자가 고친 학년·반(학생). teacherRoleCode: T1002(교사) · T1003(교사(S)).
        /// 반환: (성공, 안내 문구, 신청서)
        /// </summary>
        public async Task<(bool Ok, string Message, SignupApplication? App)> ApproveAsync(int id, string decidedBy, bool linkExisting, int classYear, int classBan, string? teacherRoleCode)
        {
            using (var conn = Conn())
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                var found = await QueryAsync(conn, "WHERE a.[Id] = @k AND a.[Status] = N'PENDING'", "", "a.[Id]", ("@k", id));
                if (found.Count == 0) return (false, "이미 처리된 신청이에요.", null);
                var a = found[0];
                var now = SharedComm.GetKoreaNow();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) 사람 명부 — 이미 내 기록이 있으면 그것, 없으면 고른 기록에 연결하거나 새로 만든다
                        int pid = await ScalarIntAsync(conn, tx, "SELECT TOP 1 [PersonId] FROM [dbo].[PersonMaster_2026] WHERE [UserName] = @u ORDER BY [PersonId]", ("@u", a.UserName));
                        if (pid == 0 && linkExisting && a.PersonId > 0)
                        {
                            int n = await ExecAsync(conn, tx, "UPDATE [dbo].[PersonMaster_2026] SET [UserName] = @u WHERE [PersonId] = @p AND ISNULL([UserName], N'') = N''", ("@u", a.UserName), ("@p", a.PersonId));
                            if (n == 0)
                            {
                                tx.Rollback();
                                return (false, "고른 기록이 이미 다른 계정과 연결됐어요. \"새로 등록\"으로 승인해 주세요.", a);
                            }
                            pid = a.PersonId;
                        }
                        if (pid == 0)
                        {
                            pid = await ScalarIntAsync(conn, tx, @"
                                INSERT INTO [dbo].[PersonMaster_2026] ([fullName],[UserName],[bMonth],[bDay],[insertDT])
                                VALUES (@n, @u, 0, 0, @now);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);", ("@n", a.FullName), ("@u", a.UserName), ("@now", now));
                        }

                        // 입력한 값만 덮어쓴다 (빈 값은 기존 값 유지)
                        await ExecAsync(conn, tx, @"
                            UPDATE [dbo].[PersonMaster_2026] SET
                                [bMonth]      = CASE WHEN @bm > 0 THEN @bm ELSE [bMonth] END,
                                [bDay]        = CASE WHEN @bd > 0 THEN @bd ELSE [bDay] END,
                                [sex]         = COALESCE(@sex, [sex]),
                                [school]      = COALESCE(@school, [school]),
                                [PhoneNumber] = COALESCE(@phone, [PhoneNumber]),
                                [updateDT]    = @now
                            WHERE [PersonId] = @p",
                            ("@bm", a.BMonth), ("@bd", a.BDay), ("@sex", Db(a.Sex)), ("@school", Db(a.School)), ("@phone", Db(a.Phone)), ("@now", now), ("@p", pid));

                        // 2) 반 · 역할 — 역할 변경이면 이전 역할의 흔적도 정리한다
                        string prevRole = await ScalarStringAsync(conn, tx, "SELECT TOP 1 [RoleCode] FROM [dbo].[DeptRoleUserAssoc] WHERE [UserId] = @i", ("@i", a.UserId));
                        const string clearClass = "DELETE FROM [dbo].[ClassInfo_2026] WHERE [PersonID] = @p;";

                        string? role = null;
                        if (a.RoleKind == "S")
                        {
                            role = "T1001";
                            if (classYear > 0 && classBan > 0)
                            {
                                await ExecAsync(conn, tx, clearClass + @"
                                    INSERT INTO [dbo].[ClassInfo_2026] ([ClassYear],[ClassBan],[PersonID]) VALUES (@y,@b,@p);",
                                    ("@p", pid), ("@y", classYear), ("@b", classBan));
                            }
                            else if (prevRole != "T1001")
                            {
                                // 교사 · 학부모였다가 학생이 됐는데 반을 모르면, 예전 반(담임 반 등)을 학생 반으로 남기지 않는다
                                await ExecAsync(conn, tx, clearClass, ("@p", pid));
                            }
                        }
                        else if (a.RoleKind == "T")
                        {
                            role = teacherRoleCode == "T1003" ? "T1003" : "T1002";
                            // 학생이었다가 교사가 되면 학생 반 배정을 지운다 (교사였으면 담임 반은 그대로)
                            if (prevRole == "T1001") await ExecAsync(conn, tx, clearClass, ("@p", pid));
                        }
                        else if (a.RoleKind == "E")
                        {
                            // 기타(T1099) — 학생 · 교사 · 학부모가 아닌 사람. 학생 반 배정은 지운다
                            role = UserRole.EtcCode;
                            await ExecAsync(conn, tx, clearClass, ("@p", pid));
                        }
                        else
                        {
                            // 학부모 — 역할 코드가 없다. 예전 역할 · 반은 지운다
                            await ExecAsync(conn, tx, clearClass + " DELETE FROM [dbo].[DeptRoleUserAssoc] WHERE [UserId] = @i;", ("@p", pid), ("@i", a.UserId));
                        }

                        if (role != null)
                        {
                            await ExecAsync(conn, tx, @"
                                DELETE FROM [dbo].[DeptRoleUserAssoc] WHERE [UserId] = @i;
                                INSERT INTO [dbo].[DeptRoleUserAssoc] ([UserId],[RoleCode]) VALUES (@i,@r);",
                                ("@i", a.UserId), ("@r", role));
                        }

                        // 2-1) 역할 기준 (2026-11-08) — 새 역할에 맞지 않는 팀에서 빠지고, 맞지 않는 신청 · 초대는 지운다
                        var (leftTeam, newKind) = await BulletinBoard.Services.FamilyTeamService.FitTeamToRoleAsync(conn, tx, a.UserName);
                        if (leftTeam != null)
                            await NotifyLeftTeamAsync(conn, tx, a.UserName, decidedBy, leftTeam, newKind, now);

                        // 3) 신청서 닫기
                        await ExecAsync(conn, tx, @"UPDATE [dbo].[SignupApplication]
                            SET [Status] = N'APPROVED', [ClassYear] = @y, [ClassBan] = @b, [DecidedDT] = @now, [DecidedBy] = @by
                            WHERE [Id] = @id",
                            ("@y", a.RoleKind == "S" ? classYear : a.ClassYear), ("@b", a.RoleKind == "S" ? classBan : a.ClassBan),
                            ("@now", now), ("@by", decidedBy), ("@id", id));

                        tx.Commit();
                        return (true, leftTeam != null ? $"승인했어요. 새 역할에 맞지 않아 \"{leftTeam}\"에서 뺐어요." : "승인했어요.", a);
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 관리자가 다른 사람의 역할을 바로 바꾼다(승인 없음).
        /// roleCode: T001 코드(T1001 학생 · T1002 교사 · …) · 빈 값 = 학부모(역할 없음).
        /// 학생이면 classYear/classBan(0 = 미정). 정리 규칙은 역할 변경 승인과 같다.
        /// </summary>
        /// 반환: 새 역할에 맞지 않아 빠진 팀 이름 (없으면 null)
        public async Task<string?> ChangeRoleDirectAsync(string userId, string userName, string fullName, string roleCode, int classYear, int classBan, string changedBy = "")
        {
            string? leftTeam = null;
            using (var conn = Conn())
            {
                await conn.OpenAsync();
                var now = SharedComm.GetKoreaNow();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 사람 명부가 없으면 만든다 (반을 넣으려면 PersonId 가 필요)
                        int pid = await ScalarIntAsync(conn, tx, "SELECT TOP 1 [PersonId] FROM [dbo].[PersonMaster_2026] WHERE [UserName] = @u ORDER BY [PersonId]", ("@u", userName));
                        if (pid == 0)
                        {
                            pid = await ScalarIntAsync(conn, tx, @"
                                INSERT INTO [dbo].[PersonMaster_2026] ([fullName],[UserName],[bMonth],[bDay],[insertDT])
                                VALUES (@n, @u, 0, 0, @now);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);", ("@n", fullName), ("@u", userName), ("@now", now));
                        }

                        string prevRole = await ScalarStringAsync(conn, tx, "SELECT TOP 1 [RoleCode] FROM [dbo].[DeptRoleUserAssoc] WHERE [UserId] = @i", ("@i", userId));
                        const string clearClass = "DELETE FROM [dbo].[ClassInfo_2026] WHERE [PersonID] = @p;";
                        roleCode = (roleCode ?? "").Trim();

                        if (roleCode == "T1001")
                        {
                            if (classYear > 0 && classBan > 0)
                                await ExecAsync(conn, tx, clearClass + " INSERT INTO [dbo].[ClassInfo_2026] ([ClassYear],[ClassBan],[PersonID]) VALUES (@y,@b,@p);",
                                    ("@p", pid), ("@y", classYear), ("@b", classBan));
                            else if (prevRole != "T1001")
                                await ExecAsync(conn, tx, clearClass, ("@p", pid));
                        }
                        else if (roleCode.Length > 0)
                        {
                            // 학생이었다가 교사 · 임원이 되면 학생 반 배정을 지운다
                            if (prevRole == "T1001") await ExecAsync(conn, tx, clearClass, ("@p", pid));
                        }
                        else
                        {
                            // 학부모(역할 없음)
                            await ExecAsync(conn, tx, clearClass, ("@p", pid));
                        }

                        await ExecAsync(conn, tx, "DELETE FROM [dbo].[DeptRoleUserAssoc] WHERE [UserId] = @i;", ("@i", userId));
                        if (roleCode.Length > 0)
                            await ExecAsync(conn, tx, "INSERT INTO [dbo].[DeptRoleUserAssoc] ([UserId],[RoleCode]) VALUES (@i,@r);",
                                ("@i", userId), ("@r", roleCode));

                        // 역할 기준 (2026-11-08) — 새 역할에 맞지 않는 팀에서 빠지고, 맞지 않는 신청 · 초대는 지운다
                        var fit = await BulletinBoard.Services.FamilyTeamService.FitTeamToRoleAsync(conn, tx, userName);
                        leftTeam = fit.LeftTeam;
                        if (leftTeam != null)
                            await NotifyLeftTeamAsync(conn, tx, userName, changedBy, leftTeam, fit.Kind, now);

                        tx.Commit();
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }
            return leftTeam;
        }

        /// <summary>역할이 바뀌어 팀에서 빠졌다고 본인에게 쪽지 (같은 트랜잭션)</summary>
        private static async Task NotifyLeftTeamAsync(SqlConnection conn, SqlTransaction tx, string toUser, string fromUser, string teamName, UserRoleKind newKind, DateTime now)
        {
            string label = UserRole.LabelOf(newKind);
            await ExecAsync(conn, tx, @"
                IF OBJECT_ID(N'dbo.PopMessage') IS NOT NULL
                    INSERT INTO [dbo].[PopMessage] ([MessageText],[ToWhom],[FromWhom],[updateDT],[InsertDT]) VALUES (@t,@to,@f,@now,@now);",
                ("@t", $"역할이 {label}(으)로 바뀌어 \"{teamName}\"에서 빠졌어요. {(label == "학생" || label == "학부모" ? "새 역할에 맞는 팀에 다시 들어가 주세요." : "교사 · 임원은 팀에 들어가지 않아요.")}"),
                ("@to", toUser), ("@f", string.IsNullOrWhiteSpace(fromUser) ? "Basic" : fromUser), ("@now", now));
        }

        /// <summary>지금 역할 — 역할 변경 화면 · 신청서의 "이전 역할" 표시용</summary>
        public async Task<CurrentRole> GetCurrentRoleAsync(string userName)
        {
            var r = new CurrentRole();
            using (var conn = Conn())
            {
                await conn.OpenAsync();
                // 역할 기준 (2026-11-08) — UserRole.RoleCodeSql: 배정 표 우선, 없으면 승인된 역할(DeptRoleUserAssoc)
                using (var cmd = new SqlCommand(@"SELECT TOP 1 x.rc,
                                                         ISNULL((SELECT TOP 1 cd.[MinorNM] FROM [dbo].[CodeData] cd WHERE cd.[MajorCD] = 'T001' AND cd.[MinorCD] = x.rc), N''),
                                                         ISNULL(ci.[ClassYear],0), ISNULL(ci.[ClassBan],0)
                                                  FROM [Identity].[User] u
                                                  CROSS APPLY (SELECT " + UserRole.RoleCodeSql("u.[Id]") + @" AS rc) x
                                                  LEFT JOIN [dbo].[view_PersonMaster_2026] pm ON pm.[UserId] = u.[Id]
                                                  LEFT JOIN [dbo].[ClassInfo_2026] ci ON ci.[PersonID] = pm.[PersonId]
                                                  WHERE u.[UserName] = @u", conn))
                {
                    cmd.Parameters.AddWithValue("@u", userName);
                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        if (await rd.ReadAsync())
                        {
                            r.RoleCode = Convert.ToString(rd[0])?.Trim() ?? "";
                            r.RoleNM = Convert.ToString(rd[1])?.Trim() ?? "";
                            r.ClassYear = Convert.ToInt32(rd[2]);
                            r.ClassBan = Convert.ToInt32(rd[3]);
                        }
                    }
                }
            }
            return r;
        }

        private static async Task<string> ScalarStringAsync(SqlConnection c, SqlTransaction tx, string sql, params (string, object)[] ps)
        {
            using (var cmd = new SqlCommand(sql, c, tx))
            {
                foreach (var p in ps) cmd.Parameters.AddWithValue(p.Item1, p.Item2);
                var r = await cmd.ExecuteScalarAsync();
                return r == null || r == DBNull.Value ? "" : (Convert.ToString(r) ?? "").Trim();
            }
        }

        private static async Task<int> ExecAsync(SqlConnection c, SqlTransaction tx, string sql, params (string, object)[] ps)
        {
            using (var cmd = new SqlCommand(sql, c, tx))
            {
                foreach (var p in ps) cmd.Parameters.AddWithValue(p.Item1, p.Item2);
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task<int> ScalarIntAsync(SqlConnection c, SqlTransaction tx, string sql, params (string, object)[] ps)
        {
            using (var cmd = new SqlCommand(sql, c, tx))
            {
                foreach (var p in ps) cmd.Parameters.AddWithValue(p.Item1, p.Item2);
                var r = await cmd.ExecuteScalarAsync();
                return r == null || r == DBNull.Value ? 0 : Convert.ToInt32(r);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulletinBoard.Common;
using Microsoft.Data.SqlClient;

namespace BulletinBoard.DataAccessLayer
{
    /// <summary>탈퇴 요청 화면에 보여 줄 "삭제될 정보" 한 묶음</summary>
    public class WithdrawGroup
    {
        public string Title { get; set; } = "";
        public string Desc { get; set; } = "";
        public int Count { get; set; }
    }

    public class WithdrawRequestRow
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Reason { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime InsertDT { get; set; }
    }

    /// <summary>
    /// 회원 탈퇴 — 요청 저장 · 삭제될 정보 미리보기 · 승인 때 실제 삭제.
    /// 삭제 범위: 설계서(docs/탈퇴요청_설계서.md) 6-1 의 "F. 소통·사진"을 뺀 모든 정보.
    /// 사람을 가리키는 키가 세 가지(UserName · UserId · PersonId)라 세 키로 모두 찾아 지운다.
    /// 표가 늘거나 없는 환경이어도 되도록, 표·열이 실제로 있을 때만 건드린다.
    /// </summary>
    public class ControlWithdraw
    {
        // 묶음 번호 → 화면 이름 · 설명
        private const int G_Basic = 0, G_Class = 1, G_Mission = 2, G_Bible = 3, G_Gift = 4, G_Family = 5, G_Access = 6, G_Account = 7;

        private static readonly (string Title, string Desc)[] Groups =
        {
            ("내 기본 정보", "이름 · 전화 · 생일 · 프로필 사진"),
            ("반 · 출석", "반 배정 · 출석 기록 · 묵상 점수 · 교사 배정"),
            ("미션 기록", "말씀 · 기도 · 퀴즈 · 공과 · 영어 퀴즈 기록"),
            ("성경 읽기", "읽은 절 · 북마크 · 완독 기록 · 365 기도 기록"),
            ("선물 · 도토리", "선물 신청 · 찜 · 선물 알림 상태"),
            ("가족팀", "구성원 정보 · 보낸 신청 · 받은 초대 (방장이면 다음 사람이 이어받아요)"),
            ("접속 · 설정", "접속 기록 · 검색 기록 · 화면 설정"),
            ("로그인 계정", "아이디 · 이메일 · 비밀번호 · 역할"),
        };

        // 키 종류: U = UserName · I = UserId · P = PersonId(사람 명부에서 UserName 으로 찾음)
        private sealed class Item
        {
            public int Group;
            public string Table = "";
            public string Where = "";
            public string[] Columns = Array.Empty<string>();
        }

        private static Item One(int g, string table, string col, char key)
        {
            string where = key switch
            {
                'U' => $"[{col}] = @u",
                'I' => $"[{col}] = @i",
                _ => $"[{col}] IN (SELECT [PersonId] FROM [dbo].[PersonMaster_2026] WHERE [UserName] = @u)"
            };
            return new Item { Group = g, Table = table, Where = where, Columns = new[] { col } };
        }

        private static Item Raw(int g, string table, string where, params string[] cols)
            => new Item { Group = g, Table = table, Where = where, Columns = cols };

        // 지우는 순서대로. PersonMaster_2026 은 PersonId 로 찾는 표들이 먼저 지워진 뒤 마지막에 지운다.
        private static readonly Item[] Items =
        {
            // 가족팀 (구성원 · 신청 · 알림) — 방장 이어받기는 DeleteDataAsync 에서 먼저 처리
            One(G_Family, "DM2_FamilyMember", "UserName", 'U'),
            Raw(G_Family, "DM2_FamilyRequest", "[FromUserName] = @u OR [TargetUserName] = @u", "FromUserName", "TargetUserName"),
            One(G_Family, "DM2_FamilyNotice", "UserName", 'U'),

            // 반 · 출석 (PersonId)
            One(G_Class, "ClassInfo_2026", "PersonID", 'P'),
            One(G_Class, "AttendanceCheck_2026", "PersonId", 'P'),
            One(G_Class, "Point_DevotionMonthly", "PersonId", 'P'),
            One(G_Class, "BirthdayGiftPickMeMo", "PersonID", 'P'),
            One(G_Class, "BirthdayGiftSkip", "PersonID", 'P'),
            One(G_Class, "DeptTeacherMap_2026", "PersonId", 'P'),
            One(G_Class, "TeacherCandidate_2026", "PersonId", 'P'),
            One(G_Class, "ClassTeacherMap_2026", "TeacherPersonId", 'P'),
            Raw(G_Class, "AttendanceNewComerVisit", "[NewComerId] IN (SELECT [Id] FROM [dbo].[AttendanceNewComer] WHERE [PersonId] IN (SELECT [PersonId] FROM [dbo].[PersonMaster_2026] WHERE [UserName] = @u))", "NewComerId"),
            One(G_Class, "AttendanceNewComer", "PersonId", 'P'),

            // 미션 기록 (UserName)
            One(G_Mission, "ClickHistoryForTodayWordToRead", "UserName", 'U'),
            One(G_Mission, "ClickHistoryForTodayWordToRead_2025", "UserName", 'U'),
            One(G_Mission, "ClickHistoryForPrayPhrase", "UserName", 'U'),
            One(G_Mission, "ClickHistoryForPrayPhrase_2025", "UserName", 'U'),
            One(G_Mission, "ClickHistoryForPrayPhrase_20260831", "UserName", 'U'),
            One(G_Mission, "ClickHistoryForQuestion", "UserName", 'U'),
            One(G_Mission, "ClickHistoryForQuestion_2025", "UserName", 'U'),
            One(G_Mission, "ClickHistoryForWeeklyWordToRead", "UserName", 'U'),
            One(G_Mission, "ClickHistoryForWeeklyWordToRead_2025", "UserName", 'U'),
            One(G_Mission, "ClickHistoryForEnglishBibleQuiz", "UserName", 'U'),
            One(G_Mission, "ClickHistoryForPoint", "UserName", 'U'),

            // 성경 읽기
            One(G_Bible, "UserBibleProgress", "UserId", 'I'),
            One(G_Bible, "UserBookMark", "UserId", 'I'),
            One(G_Bible, "BibleCompletionLog", "UserId", 'I'),
            One(G_Bible, "UserBibleProgressCompleted", "UserName", 'U'),
            One(G_Bible, "Prayer365PrayLog", "UserName", 'U'),
            One(G_Bible, "eBibleYouTuBuHit", "UserId", 'I'),

            // 선물 · 도토리
            One(G_Gift, "StudentPointGiftOrder", "UserName", 'U'),
            One(G_Gift, "StudentPointGiftPopupState", "UserName", 'U'),
            One(G_Gift, "GiftPopupState", "UserName", 'U'),
            One(G_Gift, "GiftPopupActionLog", "UserName", 'U'),
            One(G_Gift, "AwardUserClaim", "UserName", 'U'),
            One(G_Gift, "AwardUserDelivery", "UserName", 'U'),
            One(G_Gift, "AwardUserSnooze", "UserName", 'U'),
            One(G_Gift, "UserAwardOrder", "UserId", 'I'),
            One(G_Gift, "BibleGiftOrder", "UserId", 'I'),
            One(G_Gift, "BibleGiftOrderLog", "UserId", 'I'),
            One(G_Gift, "BirthdayGiftPickSms", "UserId", 'I'),
            One(G_Gift, "AwardItemPick", "UserId", 'I'),
            One(G_Gift, "PresentItemPick", "UserId", 'I'),
            One(G_Gift, "MyPhotoPick", "UserName", 'U'),

            // 접속 · 설정
            One(G_Access, "loginHistory", "UserName", 'U'),
            One(G_Access, "loginHistoryN", "UserName", 'U'),
            One(G_Access, "UserUiPreference", "UserName", 'U'),
            One(G_Access, "DM2HubClickLog", "UserName", 'U'),
            One(G_Access, "BibleHubClickLog", "UserId", 'I'),
            One(G_Access, "BibleSearchLog", "UserId", 'I'),
            One(G_Access, "UserSettings", "Id", 'I'),
            One(G_Access, "UserSettings_cookieName", "Id", 'I'),

            // 계정에 딸린 연결 (Identity.User 자체는 호출한 쪽이 UserManager 로 지운다)
            One(G_Account, "DeptRoleUserAssoc", "UserId", 'I'),
            One(G_Account, "DeptInfo", "UserId", 'I'),
            One(G_Account, "PersonLinkRequest", "UserName", 'U'),
            One(G_Account, "SignupApplication", "UserId", 'I'),   // 가입 마법사 신청(연락처 · 학교 · 자녀 정보)

            // 사람 명부는 마지막에
            One(G_Basic, "PersonMaster_2026", "UserName", 'U'),
        };

        // ── 요청 표 (첫 호출 때 스스로 만든다) ─────────────────────────
        private static async Task EnsureTableAsync(SqlConnection conn)
        {
            const string sql = @"
                IF OBJECT_ID(N'[dbo].[PersonWithdrawRequest]', N'U') IS NULL
                CREATE TABLE [dbo].[PersonWithdrawRequest] (
                    [Id]        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [UserId]    NVARCHAR(450) NOT NULL,
                    [UserName]  NVARCHAR(256) NOT NULL,
                    [FullName]  NVARCHAR(100) NOT NULL,
                    [Reason]    NVARCHAR(500) NULL,
                    [Status]    NVARCHAR(10) NOT NULL,
                    [InsertDT]  DATETIME NOT NULL,
                    [DecidedDT] DATETIME NULL,
                    [DecidedBy] NVARCHAR(256) NULL
                );";
            using (var cmd = new SqlCommand(sql, conn)) { await cmd.ExecuteNonQueryAsync(); }
        }

        private static async Task<HashSet<string>> LoadColumnsAsync(SqlConnection conn, SqlTransaction? tx)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            const string sql = @"
                SELECT t.[name] + N'.' + c.[name]
                FROM sys.tables t JOIN sys.columns c ON c.[object_id] = t.[object_id]
                WHERE t.[schema_id] = SCHEMA_ID(N'dbo')";
            using (var cmd = new SqlCommand(sql, conn, tx))
            using (var rd = await cmd.ExecuteReaderAsync())
            {
                while (await rd.ReadAsync()) set.Add(Convert.ToString(rd[0]) ?? "");
            }
            return set;
        }

        private static bool Exists(HashSet<string> cols, Item it)
            => it.Columns.All(c => cols.Contains(it.Table + "." + c)) && cols.Contains(it.Table + "." + it.Columns[0]);

        private static void AddParams(SqlCommand cmd, string userId, string userName)
        {
            cmd.Parameters.AddWithValue("@u", userName);
            cmd.Parameters.AddWithValue("@i", userId);
        }

        // ── 삭제될 정보 미리보기 ──────────────────────────────────────
        public async Task<List<WithdrawGroup>> PreviewAsync(string userId, string userName)
        {
            var counts = new int[Groups.Length];
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                var cols = await LoadColumnsAsync(conn, null);
                foreach (var it in Items)
                {
                    if (!Exists(cols, it)) continue;
                    using (var cmd = new SqlCommand($"SELECT COUNT(1) FROM [dbo].[{it.Table}] WHERE {it.Where}", conn))
                    {
                        AddParams(cmd, userId, userName);
                        counts[it.Group] += Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            counts[G_Account] += 1;   // Identity.User
            return Groups.Select((g, i) => new WithdrawGroup { Title = g.Title, Desc = g.Desc, Count = counts[i] }).ToList();
        }

        // ── 요청 ──────────────────────────────────────────────────────
        /// <summary>탈퇴 요청 저장 — 계정당 대기 중인 요청은 한 건만. 새로 저장했으면 true</summary>
        public async Task<bool> CreateRequestAsync(string userId, string userName, string fullName, string? reason)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                const string sql = @"
                    IF EXISTS (SELECT 1 FROM [dbo].[PersonWithdrawRequest] WHERE [UserId] = @UserId AND [Status] = N'PENDING')
                        SELECT 0;
                    ELSE
                    BEGIN
                        INSERT INTO [dbo].[PersonWithdrawRequest] ([UserId],[UserName],[FullName],[Reason],[Status],[InsertDT])
                        VALUES (@UserId,@UserName,@FullName,@Reason,N'PENDING',@Now);
                        SELECT 1;
                    END";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@FullName", fullName.Trim());
                    var r = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
                    if (r != null && r.Length > 500) r = r.Substring(0, 500);
                    cmd.Parameters.AddWithValue("@Reason", (object?)r ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Now", SharedComm.GetKoreaNow());
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 1;
                }
            }
        }

        public async Task<WithdrawRequestRow?> GetPendingOfAsync(string userId)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                var list = await QueryAsync(conn, "WHERE [UserId] = @UserId AND [Status] = N'PENDING'", ("@UserId", userId));
                return list.FirstOrDefault();
            }
        }

        public async Task<List<WithdrawRequestRow>> GetPendingListAsync()
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                return await QueryAsync(conn, "WHERE [Status] = N'PENDING'");
            }
        }

        public async Task<WithdrawRequestRow?> GetByIdAsync(int id)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                var list = await QueryAsync(conn, "WHERE [Id] = @Id", ("@Id", id));
                return list.FirstOrDefault();
            }
        }

        private static async Task<List<WithdrawRequestRow>> QueryAsync(SqlConnection conn, string where, params (string, object)[] ps)
        {
            var list = new List<WithdrawRequestRow>();
            using (var cmd = new SqlCommand($"SELECT [Id],[UserId],[UserName],[FullName],ISNULL([Reason],N''),[Status],[InsertDT] FROM [dbo].[PersonWithdrawRequest] {where} ORDER BY [Id]", conn))
            {
                foreach (var p in ps) cmd.Parameters.AddWithValue(p.Item1, p.Item2);
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        list.Add(new WithdrawRequestRow
                        {
                            Id = Convert.ToInt32(rd[0]),
                            UserId = Convert.ToString(rd[1]) ?? "",
                            UserName = Convert.ToString(rd[2]) ?? "",
                            FullName = Convert.ToString(rd[3]) ?? "",
                            Reason = Convert.ToString(rd[4]) ?? "",
                            Status = Convert.ToString(rd[5]) ?? "",
                            InsertDT = Convert.ToDateTime(rd[6])
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>대기 중인 요청의 상태를 바꾼다(취소 · 반려 · 승인). 바뀌었으면 true. 승인 · 취소 때는 사유를 비운다.</summary>
        public async Task<bool> SetStatusAsync(int id, string status, string? decidedBy, string? onlyUserId = null)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureTableAsync(conn);
                string sql = @"UPDATE [dbo].[PersonWithdrawRequest]
                               SET [Status] = @Status, [DecidedDT] = @Now, [DecidedBy] = @By" +
                             (status == "APPROVED" ? ", [Reason] = NULL" : "") + @"
                               WHERE [Id] = @Id AND [Status] = N'PENDING'" +
                             (onlyUserId != null ? " AND [UserId] = @OnlyUserId" : "");
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Now", SharedComm.GetKoreaNow());
                    cmd.Parameters.AddWithValue("@By", (object?)decidedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", id);
                    if (onlyUserId != null) cmd.Parameters.AddWithValue("@OnlyUserId", onlyUserId);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        // ── 쪽지 알림 ────────────────────────────────────────────────
        public async Task SendMessageAsync(string toUserName, string fromUserName, string text)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                const string sql = @"
                    INSERT INTO [dbo].[PopMessage] ([MessageText],[ToWhom],[FromWhom],[updateDT],[InsertDT])
                    VALUES (@Text,@To,@From,@Now,@Now)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Text", text);
                    cmd.Parameters.AddWithValue("@To", toUserName);
                    cmd.Parameters.AddWithValue("@From", fromUserName);
                    cmd.Parameters.AddWithValue("@Now", SharedComm.GetKoreaNow());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // ── 실제 삭제 (SuperAdmin 승인 때) ────────────────────────────
        /// <summary>
        /// 이 사람의 모든 정보를 한 트랜잭션으로 지운다. [Identity].[User] 와 프로필 사진 파일은 호출한 쪽이 지운다.
        /// 가족팀: 방장이면 남은 구성원 중 먼저 들어온 사람이 이어받고, 아무도 안 남으면 팀을 숨긴다(재시상 방지를 위해 팀 키는 남김).
        /// </summary>
        public async Task DeleteDataAsync(string userId, string userName)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        var cols = await LoadColumnsAsync(conn, tx);

                        // 1) 가족팀 — 방장 이어받기 · 빈 팀 숨기기 (구성원 행을 지우기 전에)
                        if (cols.Contains("DM2_FamilyMember.UserName") && cols.Contains("DM2_FamilyTeam.CreatorUserName"))
                        {
                            const string teamSql = @"
                                DECLARE @teams TABLE ([Id] INT);
                                INSERT INTO @teams SELECT [FamilyTeamId] FROM [dbo].[DM2_FamilyMember] WHERE [UserName] = @u;

                                UPDATE t SET [CreatorUserName] =
                                    (SELECT TOP 1 m.[UserName] FROM [dbo].[DM2_FamilyMember] m
                                     WHERE m.[FamilyTeamId] = t.[FamilyTeamId] AND m.[IsActive] = 1 AND m.[UserName] <> @u
                                     ORDER BY m.[SortNo], m.[Id])
                                FROM [dbo].[DM2_FamilyTeam] t
                                WHERE t.[CreatorUserName] = @u
                                  AND EXISTS (SELECT 1 FROM [dbo].[DM2_FamilyMember] m
                                              WHERE m.[FamilyTeamId] = t.[FamilyTeamId] AND m.[IsActive] = 1 AND m.[UserName] <> @u);

                                DELETE FROM [dbo].[DM2_FamilyMember] WHERE [UserName] = @u;

                                UPDATE [dbo].[DM2_FamilyTeam] SET [IsActive] = 0, [CreatorUserName] = N'', [UpdateDT] = @now
                                WHERE ([FamilyTeamId] IN (SELECT [Id] FROM @teams) OR [CreatorUserName] = @u)
                                  AND NOT EXISTS (SELECT 1 FROM [dbo].[DM2_FamilyMember] m
                                                  WHERE m.[FamilyTeamId] = [dbo].[DM2_FamilyTeam].[FamilyTeamId] AND m.[IsActive] = 1);

                                IF OBJECT_ID(N'[dbo].[DM2_FamilyRequest]', N'U') IS NOT NULL
                                    DELETE FROM [dbo].[DM2_FamilyRequest]
                                    WHERE [FamilyTeamId] IN (SELECT [FamilyTeamId] FROM [dbo].[DM2_FamilyTeam] WHERE [IsActive] = 0 AND [FamilyTeamId] IN (SELECT [Id] FROM @teams));";
                            using (var cmd = new SqlCommand(teamSql, conn, tx))
                            {
                                cmd.CommandTimeout = 120;
                                AddParams(cmd, userId, userName);
                                cmd.Parameters.AddWithValue("@now", SharedComm.GetKoreaNow());
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        // 2) 반 담임 보조 교사 칸은 지우지 않고 비운다 (반 자체는 남는다)
                        if (cols.Contains("ClassTeacherMap_2026.SubTeacherPersonId"))
                        {
                            using (var cmd = new SqlCommand(@"UPDATE [dbo].[ClassTeacherMap_2026] SET [SubTeacherPersonId] = 0
                                WHERE [SubTeacherPersonId] IN (SELECT [PersonId] FROM [dbo].[PersonMaster_2026] WHERE [UserName] = @u)", conn, tx))
                            {
                                AddParams(cmd, userId, userName);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        // 3) 나머지 — 표·열이 있을 때만
                        foreach (var it in Items)
                        {
                            if (!Exists(cols, it)) continue;
                            using (var cmd = new SqlCommand($"DELETE FROM [dbo].[{it.Table}] WHERE {it.Where}", conn, tx))
                            {
                                cmd.CommandTimeout = 120;
                                AddParams(cmd, userId, userName);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}

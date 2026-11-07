using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BulletinBoard.Common;
using Microsoft.Data.SqlClient;

namespace BulletinBoard.DataAccessLayer
{
    /// <summary>회원 가입 직후 PersonMaster_2026(사람 명부)과 로그인 계정(UserName)을 잇는 데이터 접근</summary>
    public class PersonCandidate
    {
        public int PersonId { get; set; }
        public string FullName { get; set; } = "";
        public int BMonth { get; set; }
        public int BDay { get; set; }
        public string UserName { get; set; } = "";   // 비어 있으면 아직 연결된 계정이 없다
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
    }

    /// <summary>SuperAdmin 연결 요청 확인 화면의 한 줄</summary>
    public class PersonLinkRequestRow
    {
        public int RequestId { get; set; }
        public string UserName { get; set; } = "";     // 요청한 가입자 아이디
        public string FullName { get; set; } = "";     // 가입자 이름
        public DateTime InsertDT { get; set; }
        public int PersonId { get; set; }               // 연결하려는 기존 기록
        public string TargetFullName { get; set; } = "";
        public int BMonth { get; set; }
        public int BDay { get; set; }
        public string TargetUserName { get; set; } = ""; // 비어 있지 않으면 그 사이 다른 계정과 연결됨
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
    }

    public class ControlPersonMatch
    {
        // 연결 요청 표 — 다른 표와 같은 방식으로 첫 호출 때 스스로 만든다
        private static async Task EnsureRequestTableAsync(SqlConnection conn)
        {
            const string sql = @"
                IF OBJECT_ID(N'[dbo].[PersonLinkRequest]', N'U') IS NULL
                CREATE TABLE [dbo].[PersonLinkRequest] (
                    [Id]        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [UserName]  NVARCHAR(256) NOT NULL,
                    [FullName]  NVARCHAR(100) NOT NULL,
                    [PersonId]  INT NOT NULL,
                    [Status]    NVARCHAR(10) NOT NULL,
                    [InsertDT]  DATETIME NOT NULL,
                    [DecidedDT] DATETIME NULL
                );";
            using (var cmd = new SqlCommand(sql, conn)) { await cmd.ExecuteNonQueryAsync(); }
        }

        /// <summary>"이 사람이 저예요" — 연결 요청만 저장한다(승인 전에는 연결되지 않음)</summary>
        public async Task CreateRequestAsync(string userName, string fullName, int personId)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureRequestTableAsync(conn);
                const string sql = @"
                    IF NOT EXISTS (SELECT 1 FROM [dbo].[PersonLinkRequest] WHERE [UserName] = @UserName AND [Status] = N'PENDING')
                    INSERT INTO [dbo].[PersonLinkRequest] ([UserName], [FullName], [PersonId], [Status], [InsertDT])
                    VALUES (@UserName, @FullName, @PersonId, N'PENDING', @InsertDT);";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@FullName", fullName.Trim());
                    cmd.Parameters.AddWithValue("@PersonId", personId);
                    cmd.Parameters.AddWithValue("@InsertDT", SharedComm.GetKoreaNow());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>이 아이디의 대기 중인 연결 요청이 있는가</summary>
        public async Task<bool> HasPendingAsync(string userName)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureRequestTableAsync(conn);
                using (var cmd = new SqlCommand("SELECT TOP 1 1 FROM [dbo].[PersonLinkRequest] WHERE [UserName] = @UserName AND [Status] = N'PENDING'", conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    return await cmd.ExecuteScalarAsync() != null;
                }
            }
        }

        /// <summary>대기 중인 요청을 가진 가입자 아이디 모음 (미배정 회원 화면 표시용)</summary>
        public async Task<HashSet<string>> GetPendingUserNamesAsync()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureRequestTableAsync(conn);
                using (var cmd = new SqlCommand("SELECT [UserName] FROM [dbo].[PersonLinkRequest] WHERE [Status] = N'PENDING'", conn))
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    while (await rd.ReadAsync()) set.Add(Convert.ToString(rd[0]) ?? "");
                }
            }
            return set;
        }

        /// <summary>UserId 로 본 대기 요청 여부 (미배정 회원 저장 막기용)</summary>
        public async Task<bool> HasPendingByUserIdAsync(string userId)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureRequestTableAsync(conn);
                const string sql = @"
                    SELECT TOP 1 1 FROM [dbo].[PersonLinkRequest] r
                    JOIN [Identity].[User] u ON u.[UserName] = r.[UserName]
                    WHERE u.[Id] = @UserId AND r.[Status] = N'PENDING'";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    return await cmd.ExecuteScalarAsync() != null;
                }
            }
        }

        /// <summary>
        /// 사람 한 명의 데이터 삭제 — PersonMaster_2026 · ClassInfo_2026 · DeptRoleUserAssoc · PersonLinkRequest (한 트랜잭션).
        /// [Identity].[User] 는 호출한 쪽이 UserManager 로 지운다.
        /// 출석 기록이 있는 사람은 지우지 않는다. 반환: (성공 여부, 안내 문구)
        /// </summary>
        public async Task<(bool Ok, string Message)> DeletePersonDataAsync(string userId, string userName)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureRequestTableAsync(conn);
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        var personIds = new List<int>();
                        using (var cmd = new SqlCommand("SELECT [PersonId] FROM [dbo].[PersonMaster_2026] WHERE [UserName] = @UserName", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserName", userName);
                            using (var rd = await cmd.ExecuteReaderAsync())
                            {
                                while (await rd.ReadAsync()) personIds.Add(Convert.ToInt32(rd[0]));
                            }
                        }

                        foreach (int pid in personIds)
                        {
                            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM [dbo].[AttendanceCheck_2026] WHERE [PersonId] = @PersonId", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@PersonId", pid);
                                if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0)
                                {
                                    tx.Rollback();
                                    return (false, "출석 기록이 있는 사람이라 삭제할 수 없어요.");
                                }
                            }
                        }

                        foreach (int pid in personIds)
                        {
                            using (var cmd = new SqlCommand("DELETE FROM [dbo].[ClassInfo_2026] WHERE [PersonID] = @PersonId; DELETE FROM [dbo].[PersonMaster_2026] WHERE [PersonId] = @PersonId;", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@PersonId", pid);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        using (var cmd = new SqlCommand("DELETE FROM [dbo].[DeptRoleUserAssoc] WHERE [UserId] = @UserId; DELETE FROM [dbo].[PersonLinkRequest] WHERE [UserName] = @UserName;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@UserName", userName);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        tx.Commit();
                        return (true, "");
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>SuperAdmin 확인 대기 목록</summary>
        public async Task<List<PersonLinkRequestRow>> GetPendingRequestsAsync()
        {
            var list = new List<PersonLinkRequestRow>();
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureRequestTableAsync(conn);
                const string sql = @"
                    SELECT r.Id, r.UserName, r.FullName, r.InsertDT,
                           pm.PersonId, ISNULL(pm.fullName,'') AS TargetFullName,
                           ISNULL(pm.bMonth,0) AS bMonth, ISNULL(pm.bDay,0) AS bDay,
                           ISNULL(pm.UserName,'') AS TargetUserName,
                           ISNULL(ci.ClassYear,0) AS ClassYear, ISNULL(ci.ClassBan,0) AS ClassBan
                    FROM [dbo].[PersonLinkRequest] r
                    LEFT JOIN [dbo].[PersonMaster_2026] pm ON pm.PersonId = r.PersonId
                    LEFT JOIN [dbo].[ClassInfo_2026] ci ON ci.PersonID = r.PersonId
                    WHERE r.[Status] = N'PENDING'
                    ORDER BY r.Id";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        list.Add(new PersonLinkRequestRow
                        {
                            RequestId = Convert.ToInt32(rd["Id"]),
                            UserName = Convert.ToString(rd["UserName"]) ?? "",
                            FullName = Convert.ToString(rd["FullName"]) ?? "",
                            InsertDT = Convert.ToDateTime(rd["InsertDT"]),
                            PersonId = rd["PersonId"] == DBNull.Value ? 0 : Convert.ToInt32(rd["PersonId"]),
                            TargetFullName = Convert.ToString(rd["TargetFullName"]) ?? "",
                            BMonth = Convert.ToInt32(rd["bMonth"]),
                            BDay = Convert.ToInt32(rd["bDay"]),
                            TargetUserName = Convert.ToString(rd["TargetUserName"]) ?? "",
                            ClassYear = Convert.ToInt32(rd["ClassYear"]),
                            ClassBan = Convert.ToInt32(rd["ClassBan"])
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 요청 처리. approve=true: 기존 기록에 계정 연결 · approve=false: "다른 사람"으로 보고 새 기록을 만들어 계정 연결.
        /// 반환: (성공 여부, 안내 문구)
        /// </summary>
        public async Task<(bool Ok, string Message)> DecideAsync(int requestId, bool approve)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                await EnsureRequestTableAsync(conn);
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        string userName, fullName; int personId;
                        using (var cmd = new SqlCommand("SELECT [UserName],[FullName],[PersonId] FROM [dbo].[PersonLinkRequest] WHERE [Id] = @Id AND [Status] = N'PENDING'", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", requestId);
                            using (var rd = await cmd.ExecuteReaderAsync())
                            {
                                if (!await rd.ReadAsync()) { return (false, "이미 처리된 요청이에요."); }
                                userName = Convert.ToString(rd[0]) ?? "";
                                fullName = Convert.ToString(rd[1]) ?? "";
                                personId = Convert.ToInt32(rd[2]);
                            }
                        }

                        // 그 사이 이 가입자에게 이미 사람 기록이 생겼으면 중복으로 만들지 않는다
                        bool alreadyHas;
                        using (var cmd = new SqlCommand("SELECT TOP 1 1 FROM [dbo].[PersonMaster_2026] WHERE [UserName] = @UserName", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@UserName", userName);
                            alreadyHas = await cmd.ExecuteScalarAsync() != null;
                        }

                        string status;
                        if (alreadyHas)
                        {
                            status = "REJECTED";
                        }
                        else if (approve)
                        {
                            using (var cmd = new SqlCommand("UPDATE [dbo].[PersonMaster_2026] SET [UserName] = @UserName, [updateDT] = @Now WHERE [PersonId] = @PersonId AND ISNULL([UserName],'') = ''", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@UserName", userName);
                                cmd.Parameters.AddWithValue("@Now", SharedComm.GetKoreaNow());
                                cmd.Parameters.AddWithValue("@PersonId", personId);
                                if (await cmd.ExecuteNonQueryAsync() == 0)
                                {
                                    tx.Rollback();
                                    return (false, "그 기록은 이미 다른 계정과 연결돼 있어요. \"다른 사람이에요\"로 처리해 주세요.");
                                }
                            }
                            status = "APPROVED";
                        }
                        else
                        {
                            using (var cmd = new SqlCommand("INSERT INTO [dbo].[PersonMaster_2026] ([fullName],[UserName],[bMonth],[bDay],[insertDT]) VALUES (@FullName,@UserName,0,0,@Now)", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@FullName", fullName.Trim());
                                cmd.Parameters.AddWithValue("@UserName", userName);
                                cmd.Parameters.AddWithValue("@Now", SharedComm.GetKoreaNow());
                                await cmd.ExecuteNonQueryAsync();
                            }
                            status = "REJECTED";
                        }

                        using (var cmd = new SqlCommand("UPDATE [dbo].[PersonLinkRequest] SET [Status] = @Status, [DecidedDT] = @Now WHERE [Id] = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Status", status);
                            cmd.Parameters.AddWithValue("@Now", SharedComm.GetKoreaNow());
                            cmd.Parameters.AddWithValue("@Id", requestId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        tx.Commit();
                        return (true, approve && !alreadyHas ? "연결했어요." : "새 기록으로 등록했어요.");
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>이 로그인 아이디가 이미 연결된 사람 기록이 있는가</summary>
        public async Task<bool> HasPersonForUserAsync(string userName)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT TOP 1 1 FROM [dbo].[PersonMaster_2026] WHERE [UserName] = @UserName", conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    return await cmd.ExecuteScalarAsync() != null;
                }
            }
        }

        /// <summary>같은 한글 이름의 사람 기록 목록 (반 정보 포함)</summary>
        public async Task<List<PersonCandidate>> FindByFullNameAsync(string fullName)
        {
            var list = new List<PersonCandidate>();
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                const string sql = @"
                    SELECT pm.PersonId, pm.fullName, ISNULL(pm.bMonth,0) AS bMonth, ISNULL(pm.bDay,0) AS bDay,
                           ISNULL(pm.UserName,'') AS UserName,
                           ISNULL(ci.ClassYear,0) AS ClassYear, ISNULL(ci.ClassBan,0) AS ClassBan
                    FROM [dbo].[PersonMaster_2026] pm
                    LEFT JOIN [dbo].[ClassInfo_2026] ci ON ci.PersonID = pm.PersonId
                    WHERE LTRIM(RTRIM(pm.fullName)) = @FullName
                    ORDER BY pm.PersonId";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullName.Trim());
                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        while (await rd.ReadAsync())
                        {
                            list.Add(new PersonCandidate
                            {
                                PersonId = Convert.ToInt32(rd["PersonId"]),
                                FullName = Convert.ToString(rd["fullName"]) ?? "",
                                BMonth = Convert.ToInt32(rd["bMonth"]),
                                BDay = Convert.ToInt32(rd["bDay"]),
                                UserName = Convert.ToString(rd["UserName"]) ?? "",
                                ClassYear = Convert.ToInt32(rd["ClassYear"]),
                                ClassBan = Convert.ToInt32(rd["ClassBan"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>새 사람 기록을 만들고 UserName 을 넣는다</summary>
        public async Task<int> CreateAsync(string fullName, string userName)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                const string sql = @"
                    INSERT INTO [dbo].[PersonMaster_2026] ([fullName], [UserName], [bMonth], [bDay], [insertDT])
                    VALUES (@FullName, @UserName, 0, 0, @InsertDT);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullName.Trim());
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@InsertDT", SharedComm.GetKoreaNow());
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        /// <summary>기존 사람 기록에 내 계정을 잇는다 — 아직 계정이 없는 기록에만 된다(이미 연결된 기록은 건드리지 않음)</summary>
        public async Task<bool> LinkAsync(int personId, string userName)
        {
            using (var conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();
                const string sql = @"
                    UPDATE [dbo].[PersonMaster_2026]
                    SET [UserName] = @UserName, [updateDT] = @UpdateDT
                    WHERE [PersonId] = @PersonId AND ISNULL([UserName],'') = ''";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@UpdateDT", SharedComm.GetKoreaNow());
                    cmd.Parameters.AddWithValue("@PersonId", personId);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }
    }
}

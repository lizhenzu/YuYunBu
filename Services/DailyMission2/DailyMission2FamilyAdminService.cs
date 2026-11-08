using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using BulletinBoard.Models.DailyMission2;

namespace BulletinBoard.Services
{
    public class DailyMission2FamilyAdminService
    {
        private readonly string _conn;
        public DailyMission2FamilyAdminService(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection not found.");
        }

        private SqlConnection Conn() => new SqlConnection(_conn);

        public async Task<List<FamilyTeamRow>> GetTeamsAsync()
        {
            const string sql = @"
SELECT t.FamilyTeamId, t.TeamName, t.IsActive, t.Memo, t.InsertDT, t.UpdateDT,
       (SELECT COUNT(*) FROM dbo.DM2_FamilyMember m WITH (NOLOCK)
         WHERE m.FamilyTeamId = t.FamilyTeamId AND m.IsActive = 1) AS MemberCount
FROM dbo.DM2_FamilyTeam t WITH (NOLOCK)
ORDER BY t.IsActive DESC, t.FamilyTeamId DESC;";

            var list = new List<FamilyTeamRow>();
            using var conn = Conn();
            using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new FamilyTeamRow
                {
                    FamilyTeamId = r.GetInt32(0),
                    TeamName = r.GetString(1),
                    IsActive = r.GetBoolean(2),
                    Memo = r.IsDBNull(3) ? null : r.GetString(3),
                    InsertDT = r.GetDateTime(4),
                    UpdateDT = r.GetDateTime(5),
                    MemberCount = r.GetInt32(6),
                });
            }
            return list;
        }

        public async Task<FamilyTeamRow?> GetTeamAsync(int teamId)
        {
            const string sql = @"
SELECT TOP 1 FamilyTeamId, TeamName, IsActive, Memo, InsertDT, UpdateDT, CreatorUserName
FROM dbo.DM2_FamilyTeam WITH (NOLOCK)
WHERE FamilyTeamId=@id;";

            using var conn = Conn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", teamId);
            await conn.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await r.ReadAsync()) return null;

            return new FamilyTeamRow
            {
                FamilyTeamId = r.GetInt32(0),
                TeamName = r.GetString(1),
                IsActive = r.GetBoolean(2),
                Memo = r.IsDBNull(3) ? null : r.GetString(3),
                InsertDT = r.GetDateTime(4),
                UpdateDT = r.GetDateTime(5),
                CreatorUserName = r.IsDBNull(6) ? null : r.GetString(6),
            };
        }

        public async Task<List<FamilyMemberRow2>> GetMembersAsync(int teamId)
        {
            const string sql = @"
SELECT m.FamilyTeamId, m.UserName, m.RoleCode, m.SortNo, m.IsActive, m.RelationCode,
       pm.FullName,                                   -- 실명 (참여자 화면과 같은 출처)
       CASE WHEN u.Id IS NULL THEN 0 ELSE 1 END AS UserExists
FROM dbo.DM2_FamilyMember m WITH (NOLOCK)
LEFT JOIN [Identity].[User]           u  WITH (NOLOCK) ON u.UserName = m.UserName
LEFT JOIN dbo.view_PersonMaster_2026  pm WITH (NOLOCK) ON pm.UserId  = u.Id
WHERE m.FamilyTeamId=@id
ORDER BY m.IsActive DESC, m.SortNo ASC, m.RoleCode ASC, m.UserName ASC;";

            var list = new List<FamilyMemberRow2>();
            using var conn = Conn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", teamId);
            await conn.OpenAsync();

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var uname = r.GetString(1).Trim();
                list.Add(new FamilyMemberRow2
                {
                    FamilyTeamId = r.GetInt32(0),
                    UserName = uname,
                    RoleCode = Convert.ToInt32(r.GetByte(2)),
                    SortNo = r.GetInt32(3),
                    IsActive = r.GetBoolean(4),
                    RelationCode = Convert.ToInt32(r.GetByte(5)), // ✅ 추가
                    FullName = r.IsDBNull(6) || string.IsNullOrWhiteSpace(r.GetString(6)) ? uname : r.GetString(6).Trim(),
                    UserExists = r.GetInt32(7) == 1,
                });
            }
            return list;
        }

        /// <summary>이 팀의 대기 중인 가입 신청(R) · 초대(I) — 표가 아직 없으면 빈 목록</summary>
        public async Task<List<FamilyRequestRow2>> GetRequestsAsync(int teamId)
        {
            const string sql = @"
SELECT r.Id, r.Kind, r.TargetUserName, ISNULL(tp.FullName, r.TargetUserName),
       r.FromUserName, ISNULL(fp.FullName, r.FromUserName), r.RelationKey, r.CreatedAt
FROM dbo.DM2_FamilyRequest r WITH (NOLOCK)
LEFT JOIN [Identity].[User]          tu WITH (NOLOCK) ON tu.UserName = r.TargetUserName
LEFT JOIN dbo.view_PersonMaster_2026 tp WITH (NOLOCK) ON tp.UserId   = tu.Id
LEFT JOIN [Identity].[User]          fu WITH (NOLOCK) ON fu.UserName = r.FromUserName
LEFT JOIN dbo.view_PersonMaster_2026 fp WITH (NOLOCK) ON fp.UserId   = fu.Id
WHERE r.FamilyTeamId = @id
ORDER BY r.Kind DESC, r.CreatedAt;";

            var list = new List<FamilyRequestRow2>();
            try
            {
                using var conn = Conn();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", teamId);
                await conn.OpenAsync();
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    list.Add(new FamilyRequestRow2
                    {
                        Id = r.GetInt32(0),
                        Kind = r.GetString(1).Trim(),
                        TargetUserName = r.GetString(2).Trim(),
                        TargetFullName = r.GetString(3).Trim(),
                        FromUserName = r.GetString(4).Trim(),
                        FromFullName = r.GetString(5).Trim(),
                        RelationKey = r.GetString(6).Trim(),
                        CreatedAt = r.GetDateTime(7),
                    });
                }
            }
            catch (SqlException) { /* 신청 표(DM2_FamilyRequest)를 아직 만들지 않았다 */ }
            return list;
        }

        public async Task<int> CreateTeamAsync(string teamName, string? memo = null)
        {
            const string sql = @"
INSERT INTO dbo.DM2_FamilyTeam (TeamName, AwardKey, Memo)
VALUES (@name, @name, @memo);
SELECT SCOPE_IDENTITY();";

            using var conn = Conn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", teamName);
            cmd.Parameters.AddWithValue("@memo", (object?)memo ?? DBNull.Value);
            await conn.OpenAsync();

            var idObj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(idObj);
        }

        /// <summary>관리자 강제 삭제 — 구성원 · 대기 요청과 함께 팀을 지운다. 선물 기록은 남는다(그 이름도 다시 쓰이지 않는다)</summary>
        public async Task DeleteTeamAsync(int teamId)
        {
            const string sql = @"
DELETE FROM dbo.DM2_FamilyRequest WHERE FamilyTeamId=@id;
DELETE FROM dbo.DM2_FamilyMember  WHERE FamilyTeamId=@id;
DELETE FROM dbo.DM2_FamilyTeam    WHERE FamilyTeamId=@id;";

            using var conn = Conn();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@id", teamId);
            await cmd.ExecuteNonQueryAsync();
            tx.Commit();
        }

        /// <summary>만든 사람 바꾸기 (관리자) — 그 팀의 활성 구성원만 지정할 수 있다</summary>
        public async Task<bool> SetCreatorAsync(int teamId, string userName)
        {
            const string sql = @"
UPDATE t SET CreatorUserName=@u, UpdateDT=GETDATE()
FROM dbo.DM2_FamilyTeam t
WHERE t.FamilyTeamId=@id
  AND EXISTS (SELECT 1 FROM dbo.DM2_FamilyMember m WHERE m.FamilyTeamId=t.FamilyTeamId AND m.UserName=@u AND m.IsActive=1);";

            using var conn = Conn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", teamId);
            cmd.Parameters.AddWithValue("@u", userName.Trim());
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task UpdateTeamAsync(int teamId, string teamName, bool isActive, string? memo = null)
        {
            const string sql = @"
UPDATE dbo.DM2_FamilyTeam
SET TeamName=@name, IsActive=@active, Memo=@memo, UpdateDT=GETDATE()
WHERE FamilyTeamId=@id;";

            using var conn = Conn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", teamId);
            cmd.Parameters.AddWithValue("@name", teamName);
            cmd.Parameters.AddWithValue("@active", isActive);
            cmd.Parameters.AddWithValue("@memo", (object?)memo ?? DBNull.Value);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddMemberAsync(int teamId, string userName, int roleCode, int sortNo, int relationCode)
        {
            // ✅ 학생이 아니면 관계는 의미 없으니 9로 강제
            if (roleCode != 0) relationCode = 9;

            const string sql = @"
INSERT INTO dbo.DM2_FamilyMember (FamilyTeamId, UserName, RoleCode, SortNo, IsActive, RelationCode)
VALUES (@tid, @uname, @role, @sort, 1, @rel);";

            using var conn = Conn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tid", teamId);
            cmd.Parameters.AddWithValue("@uname", userName.Trim());
            cmd.Parameters.AddWithValue("@role", roleCode);
            cmd.Parameters.AddWithValue("@sort", sortNo);
            cmd.Parameters.AddWithValue("@rel", relationCode);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateMemberAsync(int teamId, string userName, int roleCode, int sortNo, bool isActive, int relationCode)
        {
            // ✅ 학생이 아니면 관계는 의미 없으니 9로 강제
            if (roleCode != 0) relationCode = 9;

            const string sql = @"
UPDATE dbo.DM2_FamilyMember
SET RoleCode=@role, SortNo=@sort, IsActive=@active, RelationCode=@rel, UpdateDT=GETDATE()
WHERE FamilyTeamId=@tid AND UserName=@uname;";

            using var conn = Conn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tid", teamId);
            cmd.Parameters.AddWithValue("@uname", userName.Trim());
            cmd.Parameters.AddWithValue("@role", roleCode);
            cmd.Parameters.AddWithValue("@sort", sortNo);
            cmd.Parameters.AddWithValue("@active", isActive);
            cmd.Parameters.AddWithValue("@rel", relationCode);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteMemberAsync(int teamId, string userName)
        {
            const string sql = @"
UPDATE dbo.DM2_FamilyMember
SET IsActive=0, UpdateDT=GETDATE()
WHERE FamilyTeamId=@tid AND UserName=@uname;";

            using var conn = Conn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tid", teamId);
            cmd.Parameters.AddWithValue("@uname", userName.Trim());
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

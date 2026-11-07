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

    public class ControlPersonMatch
    {
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

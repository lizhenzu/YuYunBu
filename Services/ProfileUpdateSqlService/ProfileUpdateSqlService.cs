using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Profile;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.Services
{
    public class ProfileUpdateSqlService : IProfileUpdateSqlService
    {
        private readonly string _connStr;
        private readonly string _webRootPath;

        public ProfileUpdateSqlService(IConfiguration cfg, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _connStr = cfg.GetConnectionString("DefaultConnection");
            _webRootPath = env.WebRootPath; // wwwroot 경로
        }

        static string TrimS(string s) => (s ?? "").Trim();
        static string JoinFullName(string last, string first) => $"{TrimS(last)}{TrimS(first)}";

        string BuildProfileUrl(string userId)
        {
            // 파일 존재 확인은 서버 실제 경로로 체크
            var rel = $"/images/profile/{userId}.png";
            var abs = Path.Combine(_webRootPath, "images", "profile", $"{userId}.png");
            if (File.Exists(abs)) return rel;
            return "/images/icon/people/people_01.webp";
        }

        public async Task<ProfileEditVM> GetEditAsync(string userId)
        {
            using var con = new SqlConnection(_connStr);
            await con.OpenAsync();

            // 1) User 가져오기
            string sqlUser = @"
SELECT TOP 1
    Id,
UserName,
Email,
    ISNULL(LastName,'') AS LastName,
    ISNULL(FirstName,'') AS FirstName,
    ISNULL(PhoneNumber,'') AS PhoneNumber
FROM [Identity].[User]
WHERE Id = @Id;
";
            string lastName = "", firstName = "", userPhone = "";
            string UserName = "", Email = "";

            using (var cmd = new SqlCommand(sqlUser, con))
            {
                cmd.Parameters.Add("@Id", SqlDbType.NVarChar, 450).Value = userId;

                using var rd = await cmd.ExecuteReaderAsync();
                if (!await rd.ReadAsync()) return null;

                lastName = rd["LastName"]?.ToString() ?? "";
                firstName = rd["FirstName"]?.ToString() ?? "";
                userPhone = rd["PhoneNumber"]?.ToString() ?? "";
                UserName = rd["UserName"]?.ToString() ?? "";
                Email = rd["Email"]?.ToString() ?? "";
            }

            var oldFullName = JoinFullName(lastName, firstName);

            // 2) PersonMaster는 "현재 이름(fullName)"으로 조회 (있으면 동기화 대상)
            string sqlPerson = @"
SELECT TOP 1
    ISNULL(PhoneNumber,'') AS PhoneNumber,
    ISNULL(PhoneNumber2,'') AS PhoneNumber2,
    ISNULL(bMonth,0) AS bMonth,
    ISNULL(bDay,0) AS bDay,
    ISNULL(birthDay_PublishYN,'') AS birthDay_PublishYN
FROM dbo.PersonMaster_2026
WHERE fullName = @FullName;
";

            string p1 = userPhone, p2 = "";
            int bm = 0, bd = 0;
            string pub = "";

            bool match = false;
            using (var cmd = new SqlCommand(sqlPerson, con))
            {
                cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = oldFullName;

                using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    match = true;
                    p1 = rd["PhoneNumber"]?.ToString() ?? "";
                    p2 = rd["PhoneNumber2"]?.ToString() ?? "";
                    bm = Convert.ToInt32(rd["bMonth"]);
                    bd = Convert.ToInt32(rd["bDay"]);
                    pub = rd["birthDay_PublishYN"]?.ToString() ?? "";
                }
            }

            return new ProfileEditVM
            {
                Id = userId,
                LastName = lastName,
                UserName = UserName,
                Email = Email,
                FirstName = firstName,
                PhoneNumber = p1,
                PhoneNumber2 = p2,
                bMonth = bm,
                bDay = bd,
                birthDay_PublishYN = pub,
                ProfileImageUrl = BuildProfileUrl(userId),
                PersonUpdateAllowed = match,
                NoticeMessage = match
                    ? "✅ 현재 이름이 PersonMaster_2026과 일치하여, 저장 시 PersonMaster도 함께 갱신됩니다."
                    : "⚠️ 현재 이름이 PersonMaster_2026의 fullName과 일치하지 않아, 저장해도 PersonMaster_2026는 변경되지 않습니다."
            };
        }

     
        public async Task<(bool ok, string message, bool personUpdated)> SaveAsync(ProfileEditVM vm, string loginUserId)
        {
            if (vm == null) return (false, "잘못된 요청입니다.", false);

            vm.Id = loginUserId;

            var newLast = TrimS(vm.LastName);
            var newFirst = TrimS(vm.FirstName);
            var newFullName = JoinFullName(newLast, newFirst);

            var newPhone1 = TrimS(vm.PhoneNumber);
            var newPhone2 = TrimS(vm.PhoneNumber2);

            int? newBMonth = (vm.bMonth <= 0) ? (int?)null : vm.bMonth;
            int? newBDay = (vm.bDay <= 0) ? (int?)null : vm.bDay;

            var pub = TrimS(vm.birthDay_PublishYN);
            if (pub != "y" && pub != "h") pub = "";

            using var con = new SqlConnection(_connStr);
            await con.OpenAsync();
            using var tx = con.BeginTransaction();

            try
            {
                // 0) 저장 직전 User 정보 확보 (oldFullName, 기존 Phone)
                string sqlGetOld = @"
SELECT TOP 1
    ISNULL(LastName,'') AS LastName,
    ISNULL(FirstName,'') AS FirstName,
    ISNULL(PhoneNumber,'') AS PhoneNumber
FROM [Identity].[User]
WHERE Id = @Id;
";
                string oldLast = "", oldFirst = "", oldUserPhone = "";
                using (var cmd = new SqlCommand(sqlGetOld, con, tx))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.NVarChar, 450).Value = vm.Id;
                    using var rd = await cmd.ExecuteReaderAsync();
                    if (!await rd.ReadAsync())
                    {
                        tx.Rollback();
                        return (false, "사용자를 찾을 수 없습니다.", false);
                    }

                    oldLast = rd["LastName"]?.ToString() ?? "";
                    oldFirst = rd["FirstName"]?.ToString() ?? "";
                    oldUserPhone = rd["PhoneNumber"]?.ToString() ?? "";
                }

                var oldFullName = JoinFullName(oldLast, oldFirst);
                var lookupPhone = string.IsNullOrWhiteSpace(newPhone1) ? TrimS(oldUserPhone) : newPhone1;

                // 1) Identity.User 업데이트
                string sqlUpdateUser = @"
UPDATE [Identity].[User]
SET
    LastName = @NewLastName,
    FirstName = @NewFirstName,
    PhoneNumber = @NewPhone1
WHERE Id = @Id;
";
                using (var cmd = new SqlCommand(sqlUpdateUser, con, tx))
                {
                    cmd.Parameters.Add("@NewLastName", SqlDbType.NVarChar).Value = (object)newLast ?? DBNull.Value;
                    cmd.Parameters.Add("@NewFirstName", SqlDbType.NVarChar).Value = (object)newFirst ?? DBNull.Value;
                    cmd.Parameters.Add("@NewPhone1", SqlDbType.NVarChar).Value = (object)newPhone1 ?? DBNull.Value;
                    cmd.Parameters.Add("@Id", SqlDbType.NVarChar, 450).Value = vm.Id;

                    var affected = await cmd.ExecuteNonQueryAsync();
                    if (affected <= 0)
                    {
                        tx.Rollback();
                        return (false, "User 갱신 실패", false);
                    }
                }

                // 2) PersonMaster 대상 찾기 (fullName 우선, 없으면 PhoneNumber로 찾기)
                //    - PersonId를 확보해서 "한 행"만 정확히 UPDATE 하도록 함 (중복 레코드 방지)
                int personId = 0;

                // 2-1) oldFullName으로 찾기
                string sqlFindByName = @"
SELECT TOP 2 PersonId
FROM dbo.PersonMaster_2026
WHERE fullName = @OldFullName
ORDER BY PersonId ASC;
";
                using (var cmd = new SqlCommand(sqlFindByName, con, tx))
                {
                    cmd.Parameters.Add("@OldFullName", SqlDbType.NVarChar, 100).Value = oldFullName;
                    using var rd = await cmd.ExecuteReaderAsync();

                    int cnt = 0;
                    int firstId = 0;
                    while (await rd.ReadAsync())
                    {
                        cnt++;
                        if (cnt == 1) firstId = Convert.ToInt32(rd["PersonId"]);
                        if (cnt >= 2) break;
                    }

                    if (cnt == 1) personId = firstId;
                    else if (cnt >= 2)
                    {
                        // 동명이인/중복 fullName이면 안전상 UPDATE 금지
                        tx.Commit();
                        return (true, "✅ 저장 완료! (User만 갱신) — PersonMaster_2026에 같은 fullName이 여러 건이라 업데이트를 막았습니다.", false);
                    }
                }

                // 2-2) 없으면 PhoneNumber로 찾기 (핸드폰이 있으면 강한 식별자)
                if (personId == 0 && !string.IsNullOrWhiteSpace(lookupPhone))
                {
                    string sqlFindByPhone = @"
SELECT TOP 2 PersonId
FROM dbo.PersonMaster_2026
WHERE PhoneNumber = @Phone
ORDER BY PersonId ASC;
";
                    using (var cmd = new SqlCommand(sqlFindByPhone, con, tx))
                    {
                        cmd.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = lookupPhone;
                        using var rd = await cmd.ExecuteReaderAsync();

                        int cnt = 0;
                        int firstId = 0;
                        while (await rd.ReadAsync())
                        {
                            cnt++;
                            if (cnt == 1) firstId = Convert.ToInt32(rd["PersonId"]);
                            if (cnt >= 2) break;
                        }

                        if (cnt == 1) personId = firstId;
                        else if (cnt >= 2)
                        {
                            tx.Commit();
                            return (true, "✅ 저장 완료! (User만 갱신) — PersonMaster_2026에 같은 PhoneNumber가 여러 건이라 업데이트를 막았습니다.", false);
                        }
                    }
                }

                // 3) 그래도 없으면 INSERT (요구사항: PersonMaster에 데이터 없으면 추가)
                if (personId == 0)
                {
                    // (방어) fullName이 이미 있으면 INSERT하지 않음 (중복 생성 방지)
                    string sqlExistsNewName = @"
SELECT COUNT(1)
FROM dbo.PersonMaster_2026
WHERE fullName = @NewFullName;
";
                    using (var cmd = new SqlCommand(sqlExistsNewName, con, tx))
                    {
                        cmd.Parameters.Add("@NewFullName", SqlDbType.NVarChar, 100).Value = newFullName;
                        var c = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        if (c > 0)
                        {
                            tx.Commit();
                            return (true, "✅ 저장 완료! (User만 갱신) — PersonMaster_2026에 동일한 이름이 이미 있어 신규 생성을 막았습니다.", false);
                        }
                    }

                    // INSERT 후 생성된 PersonId 반환
                    string sqlInsert = @"
INSERT INTO dbo.PersonMaster_2026
(
    fullName,
    PhoneNumber,
    PhoneNumber2,
    bMonth,
    bDay,
    birthDay_PublishYN,
    insertDT,
    updateDT
)
OUTPUT INSERTED.PersonId
VALUES
(
    @FullName,
    @Phone1,
    @Phone2,
    @BMonth,
    @BDay,
    @PublishYN,
    GETDATE(),
    GETDATE()
);
";
                    using (var cmd = new SqlCommand(sqlInsert, con, tx))
                    {
                        cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = newFullName;
                        cmd.Parameters.Add("@Phone1", SqlDbType.NVarChar).Value = (object)newPhone1 ?? DBNull.Value;
                        cmd.Parameters.Add("@Phone2", SqlDbType.NVarChar, 20).Value = (object)newPhone2 ?? DBNull.Value;

                        var pBM = cmd.Parameters.Add("@BMonth", SqlDbType.Int);
                        pBM.Value = (object)newBMonth ?? DBNull.Value;

                        var pBD = cmd.Parameters.Add("@BDay", SqlDbType.Int);
                        pBD.Value = (object)newBDay ?? DBNull.Value;

                        cmd.Parameters.Add("@PublishYN", SqlDbType.NVarChar, 1).Value = (object)pub ?? "";

                        personId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
                else
                {
                    // 4) personId가 있으면 UPDATE (한 행만 정확히)
                    string sqlUpdatePersonById = @"
UPDATE dbo.PersonMaster_2026
SET
    fullName = @NewFullName,
    PhoneNumber = @NewPhone1,
    PhoneNumber2 = @NewPhone2,
    bMonth = @NewBMonth,
    bDay = @NewBDay,
    birthDay_PublishYN = @NewPublishYN,
    updateDT = GETDATE()
WHERE PersonId = @PersonId;
";
                    using (var cmd = new SqlCommand(sqlUpdatePersonById, con, tx))
                    {
                        cmd.Parameters.Add("@NewFullName", SqlDbType.NVarChar, 100).Value = newFullName;
                        cmd.Parameters.Add("@NewPhone1", SqlDbType.NVarChar).Value = (object)newPhone1 ?? DBNull.Value;
                        cmd.Parameters.Add("@NewPhone2", SqlDbType.NVarChar, 20).Value = (object)newPhone2 ?? DBNull.Value;

                        var pBM = cmd.Parameters.Add("@NewBMonth", SqlDbType.Int);
                        pBM.Value = (object)newBMonth ?? DBNull.Value;

                        var pBD = cmd.Parameters.Add("@NewBDay", SqlDbType.Int);
                        pBD.Value = (object)newBDay ?? DBNull.Value;

                        cmd.Parameters.Add("@NewPublishYN", SqlDbType.NVarChar, 1).Value = (object)pub ?? "";
                        cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                tx.Commit();
                return (true, "✅ 저장 완료! (User + PersonMaster_2026) 갱신/생성 처리되었습니다.", true);
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                return (false, $"저장 중 오류: {ex.Message}", false);
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BulletinBoard.Models.SuperAdmin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.DataAccessLayer.SuperAdmin
{
    public class AnomalyRepository
    {
        private readonly string _connStr;

        public AnomalyRepository(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<IdentityDupRow>> GetIdentityDuplicateNamesAsync()
        {
            var sql = @"
;WITH d AS (
    SELECT
        FullNameKey = ISNULL(LastName,'') + ISNULL(FirstName,''),
        Cnt = COUNT(*)
    FROM [Identity].[User] WITH (NOLOCK)
    GROUP BY ISNULL(LastName,''), ISNULL(FirstName,'')
    HAVING COUNT(*) >= 2
)
SELECT
    d.FullNameKey,
    d.Cnt,
    u.Id        AS UserId,
    u.UserName,
    u.Email,
    u.PhoneNumber,
    u.InsertDT,
    u.LastName,
    u.FirstName
FROM d
JOIN [Identity].[User] u
  ON (ISNULL(u.LastName,'') + ISNULL(u.FirstName,'')) = d.FullNameKey
ORDER BY d.Cnt DESC, d.FullNameKey, u.UserName;
";

            var list = new List<IdentityDupRow>();

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new IdentityDupRow
                {
                    FullNameKey = r["FullNameKey"]?.ToString() ?? "",
                    Cnt = Convert.ToInt32(r["Cnt"]),
                    UserId = r["UserId"]?.ToString() ?? "",
                    UserName = r["UserName"] as string,
                    Email = r["Email"] as string,
                    PhoneNumber = r["PhoneNumber"] as string,
                    InsertDT = r["InsertDT"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(r["InsertDT"]),
                    LastName = r["LastName"] as string,
                    FirstName = r["FirstName"] as string
                });
            }

            return list;
        }

        public async Task<List<PersonMasterDupRow>> GetPersonMasterDuplicateFullNamesAsync()
        {
            // ⚠️ 아래 SELECT 컬럼은 view에 실제 존재하는 컬럼만 남기세요.
            var sql = @"
;WITH d AS (
    SELECT fullName, Cnt = COUNT(*)
    FROM dbo.view_PersonMaster_2026 WITH (NOLOCK)
    GROUP BY fullName
    HAVING COUNT(*) >= 2
)
SELECT
    d.fullName,
    d.Cnt,
    v.PersonId,
    v.UserId,
    isnull(v.PhoneNumber,'') PhoneNumber,
    isnull(v.PhoneNumber2,'') PhoneNumber2,
    isnull(v.bYear ,0) bYear,
    isnull(v.bMonth,0) bMonth,
    isnull(v.bDay  ,0) bDay,
    isnull(v.sex         ,'') sex,
    isnull(v.HomeAaddress,'') HomeAaddress,
    isnull(v.school       ,'') school,
    isnull(v.readBibleAllCount,0) readBibleAllCount,
    isnull(v.plusMins          ,'') plusMins,
    isnull(v.birthDay_PublishYN,'') birthDay_PublishYN,
    isnull(v.UserName  ,'') UserName,
    isnull(v.LastName  ,'') LastName,
    isnull(v.FirstName ,'') FirstName,
    isnull( v.email    ,'') email,
    v.insertDT,
    v.updateDT
FROM d
JOIN dbo.view_PersonMaster_2026 v
  ON v.fullName = d.fullName
ORDER BY d.Cnt DESC, d.fullName, v.PersonId;
";


            var list = new List<PersonMasterDupRow>();

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new PersonMasterDupRow
                {
                    FullName = r["fullName"]?.ToString() ?? "",
                    Cnt = Convert.ToInt32(r["Cnt"]),

                    PersonId = r["PersonId"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["PersonId"]),
                    UserId = r["UserId"] as string,

                    PhoneNumber = r["PhoneNumber"] as string,
                    PhoneNumber2 = r["PhoneNumber2"] as string,

                    BYear = r["bYear"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["bYear"]),
                    BMonth = r["bMonth"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["bMonth"]),
                    BDay = r["bDay"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["bDay"]),

                    Sex = r["sex"] as string,
                    HomeAddress = r["HomeAaddress"] as string,
                    School = r["school"] as string,

                    ReadBibleAllCount = r["readBibleAllCount"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["readBibleAllCount"]),
                    PlusMins = r["plusMins"] as string,

                    BirthDayPublishYN = r["birthDay_PublishYN"] as string,

                    UserName = r["UserName"] as string,
                    LastName = r["LastName"] as string,
                    FirstName = r["FirstName"] as string,
                    Email = r["email"] as string,

                    InsertDT = r["insertDT"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(r["insertDT"]),
                    UpdateDT = r["updateDT"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(r["updateDT"]),
                });
            }


            return list;
        }
    }
}

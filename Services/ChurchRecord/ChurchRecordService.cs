using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BulletinBoard.Services
{
    public class ChurchRecordService : IChurchRecordService
    {
        private readonly IConfiguration _config;

        public ChurchRecordService(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection OpenConn()
            => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public ChurchRecordHomeVM GetHome(string? searchName = null)
        {
            var vm = new ChurchRecordHomeVM { SearchName = searchName };

            using var conn = OpenConn();
            conn.Open();

            var sql = @"
SELECT
    cm.ClassYear,
    cm.ClassBan,
    ISNULL(cm.ClassName, CONCAT(cm.ClassYear, N'학년 ', cm.ClassBan, N'반')) AS ClassDisplayName,
    tmap.TeacherPersonId,
    t.fullName AS TeacherName,
    t.PhoneNumber AS TeacherPhone,
    COUNT(ci.PersonID) AS StudentCount
FROM dbo.ClassMaster cm
LEFT JOIN dbo.ClassTeacherMap_2026 tmap
    ON tmap.ClassYear = cm.ClassYear
   AND tmap.ClassBan  = cm.ClassBan
   AND tmap.IsActive  = 1
LEFT JOIN dbo.view_PersonMaster_2026 t
    ON t.PersonId = tmap.TeacherPersonId
LEFT JOIN dbo.ClassInfo_2026 ci
    ON ci.ClassYear = cm.ClassYear
   AND ci.ClassBan  = cm.ClassBan
WHERE cm.IsActive = 1
GROUP BY
    cm.ClassYear, cm.ClassBan, cm.ClassName,
    tmap.TeacherPersonId, t.fullName, t.PhoneNumber
ORDER BY cm.ClassYear, cm.ClassBan;
";

            using (var cmd = new SqlCommand(sql, conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    vm.Classes.Add(new ClassCardVM
                    {
                        ClassYear = Convert.ToInt32(rd["ClassYear"]),
                        ClassBan = Convert.ToInt32(rd["ClassBan"]),
                        ClassDisplayName = Convert.ToString(rd["ClassDisplayName"]) ?? "",
                        StudentCount = Convert.ToInt32(rd["StudentCount"]),
                        TeacherPersonId = rd["TeacherPersonId"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["TeacherPersonId"]),
                        TeacherName = rd["TeacherName"] == DBNull.Value ? null : Convert.ToString(rd["TeacherName"]),
                        TeacherPhone = rd["TeacherPhone"] == DBNull.Value ? null : Convert.ToString(rd["TeacherPhone"]),
                    });
                }
            }

            // ✅ 이름 검색 — 전체 반에서 아이 찾기
            var key = (searchName ?? "").Trim();
            if (key.Length > 0)
            {
                static string EscapeLike(string input)
                    => input.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_").Replace("[", @"\[");

                using var cmd = new SqlCommand(@"
SELECT TOP 50 ci.PersonID, ISNULL(p.fullName, '') AS fullName, ci.ClassYear, ci.ClassBan
FROM dbo.ClassInfo_2026 ci
JOIN dbo.view_PersonMaster_2026 p ON p.PersonId = ci.PersonID
JOIN dbo.ClassMaster cm ON cm.ClassYear = ci.ClassYear AND cm.ClassBan = ci.ClassBan AND cm.IsActive = 1
WHERE p.fullName LIKE @Like ESCAPE '\'
ORDER BY CASE WHEN p.fullName LIKE @Prefix ESCAPE '\' THEN 0 ELSE 1 END, p.fullName;", conn);
                cmd.Parameters.AddWithValue("@Like", "%" + EscapeLike(key) + "%");
                cmd.Parameters.AddWithValue("@Prefix", EscapeLike(key) + "%");
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    vm.Hits.Add(new StudentHitVM
                    {
                        PersonId = Convert.ToInt32(rd["PersonID"]),
                        FullName = Convert.ToString(rd["fullName"]) ?? "",
                        ClassYear = Convert.ToInt32(rd["ClassYear"]),
                        ClassBan = Convert.ToInt32(rd["ClassBan"])
                    });
                }
            }

            return vm;
        }

        public ClassRosterVM GetClassRoster(int year, int ban, string? searchName = null)
        {
            var vm = new ClassRosterVM
            {
                ClassYear = year,
                ClassBan = ban,
                SearchName = searchName,
                ClassDisplayName = $"{year}학년 {ban}반"
            };

            using var conn = OpenConn();
            conn.Open();

            // 1) 반 이름
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 ISNULL(ClassName, CONCAT(ClassYear, N'학년 ', ClassBan, N'반')) AS ClassDisplayName
FROM dbo.ClassMaster
WHERE ClassYear=@y AND ClassBan=@b;", conn))
            {
                cmd.Parameters.AddWithValue("@y", year);
                cmd.Parameters.AddWithValue("@b", ban);
                var obj = cmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value)
                    vm.ClassDisplayName = Convert.ToString(obj) ?? vm.ClassDisplayName;
            }

            // 2) 담임 정보
            using (var cmd = new SqlCommand(@"
SELECT TOP 1
    tmap.TeacherPersonId,
    t.fullName AS TeacherName,
    t.PhoneNumber AS TeacherPhone,
    tmap.Memo
FROM dbo.ClassTeacherMap_2026 tmap
LEFT JOIN dbo.view_PersonMaster_2026 t
    ON t.PersonId = tmap.TeacherPersonId
WHERE tmap.ClassYear=@y AND tmap.ClassBan=@b AND tmap.IsActive=1;", conn))
            {
                cmd.Parameters.AddWithValue("@y", year);
                cmd.Parameters.AddWithValue("@b", ban);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    vm.TeacherPersonId = rd["TeacherPersonId"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["TeacherPersonId"]);
                    vm.TeacherName = rd["TeacherName"] == DBNull.Value ? null : Convert.ToString(rd["TeacherName"]);
                    vm.TeacherPhone = rd["TeacherPhone"] == DBNull.Value ? null : Convert.ToString(rd["TeacherPhone"]);
                    vm.TeacherMemo = rd["Memo"] == DBNull.Value ? null : Convert.ToString(rd["Memo"]);
                }
            }

            // 3) 학생 목록
            var students = new List<StudentRowVM>();

            using (var cmd = new SqlCommand(@"
SELECT
    ci.PersonID,
    p.fullName,
    p.sex,
    p.bMonth,
    p.bDay,
    p.birthDay_PublishYN,
    p.PhoneNumber,
    p.PhoneNumber2,
    p.school,
    p.HomeAaddress
FROM dbo.ClassInfo_2026 ci
LEFT JOIN dbo.view_PersonMaster_2026 p
    ON p.PersonId = ci.PersonID
WHERE ci.ClassYear=@y AND ci.ClassBan=@b
ORDER BY p.fullName;", conn))
            {
                cmd.Parameters.AddWithValue("@y", year);
                cmd.Parameters.AddWithValue("@b", ban);

                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    students.Add(new StudentRowVM
                    {
                        PersonId = Convert.ToInt32(rd["PersonID"]),
                        FullName = rd["fullName"] == DBNull.Value ? null : Convert.ToString(rd["fullName"]),
                        Sex = rd["sex"] == DBNull.Value ? null : Convert.ToString(rd["sex"]),
                        BMonth = rd["bMonth"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["bMonth"]),
                        BDay = rd["bDay"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["bDay"]),
                        BirthDayPublishYN = rd["birthDay_PublishYN"] == DBNull.Value ? null : Convert.ToString(rd["birthDay_PublishYN"]),
                        PhoneNumber = rd["PhoneNumber"] == DBNull.Value ? null : Convert.ToString(rd["PhoneNumber"]),
                        PhoneNumber2 = rd["PhoneNumber2"] == DBNull.Value ? null : Convert.ToString(rd["PhoneNumber2"]),
                        School = rd["school"] == DBNull.Value ? null : Convert.ToString(rd["school"]),
                        HomeAddress = rd["HomeAaddress"] == DBNull.Value ? null : Convert.ToString(rd["HomeAaddress"]),
                    });
                }
            }

            // 4) 서버측 검색 필터
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var key = searchName.Trim();
                students = students
                    .Where(s => (s.FullName ?? "").Contains(key, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            vm.Students = students;
            return vm;
        }

        // ============================
        // ✅ 인명 수정(모달)용 추가 메서드
        // ============================

        public PersonEditVM? GetPersonForEdit(int personId)
        {
            using var conn = OpenConn();
            conn.Open();

            var sql = @"
SELECT TOP 1
    PersonId,
    bYear, bMonth, bDay,
    fullName,
    sex,
    PhoneNumber,
    PhoneNumber2,
    HomeAaddress,
    school
FROM dbo.PersonMaster_2026
WHERE PersonId = @PersonId;
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PersonId", personId);

            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            return new PersonEditVM
            {
                PersonId = Convert.ToInt32(rd["PersonId"]),
                BYear = rd["bYear"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["bYear"]),
                BMonth = rd["bMonth"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["bMonth"]),
                BDay = rd["bDay"] == DBNull.Value ? null : (int?)Convert.ToInt32(rd["bDay"]),
                FullName = rd["fullName"] == DBNull.Value ? null : Convert.ToString(rd["fullName"]),
                Sex = rd["sex"] == DBNull.Value ? null : Convert.ToString(rd["sex"]),
                PhoneNumber = rd["PhoneNumber"] == DBNull.Value ? null : Convert.ToString(rd["PhoneNumber"]),
                PhoneNumber2 = rd["PhoneNumber2"] == DBNull.Value ? null : Convert.ToString(rd["PhoneNumber2"]),
                HomeAaddress = rd["HomeAaddress"] == DBNull.Value ? null : Convert.ToString(rd["HomeAaddress"]),
                School = rd["school"] == DBNull.Value ? null : Convert.ToString(rd["school"]),
            };
        }

        public bool UpdatePerson(PersonEditVM vm, out string message)
        {
            message = "";

            //var fullName = (vm.FullName ?? "").Trim();
            //if (string.IsNullOrWhiteSpace(fullName))
            //{
            //    message = "이름은 필수입니다.";
            //    return false;
            //}

            var sex = (vm.Sex ?? "").Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(sex) && sex != "M" && sex != "F")
            {
                message = "성별은 M 또는 F만 가능합니다.";
                return false;
            }

            // 간단 범위 체크(원하시면 더 강화 가능)
            if (vm.BMonth.HasValue && (vm.BMonth.Value < 1 || vm.BMonth.Value > 12))
            {
                message = "월은 1~12만 가능합니다.";
                return false;
            }
            if (vm.BDay.HasValue && (vm.BDay.Value < 1 || vm.BDay.Value > 31))
            {
                message = "일은 1~31만 가능합니다.";
                return false;
            }

            using var conn = OpenConn();
            conn.Open();

            var sql = @"
UPDATE dbo.PersonMaster_2026
SET
    bYear = @bYear,
    bMonth = @bMonth,
    bDay = @bDay,
 
    sex = @sex,
    PhoneNumber = @PhoneNumber,
    PhoneNumber2 = @PhoneNumber2,
    HomeAaddress = @HomeAaddress,
    school = @school,
    updateDT = GETDATE()
WHERE PersonId = @PersonId;
";

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@PersonId", vm.PersonId);
            cmd.Parameters.AddWithValue("@bYear", (object?)vm.BYear ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bMonth", (object?)vm.BMonth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bDay", (object?)vm.BDay ?? DBNull.Value);

         
            cmd.Parameters.AddWithValue("@sex", string.IsNullOrWhiteSpace(sex) ? (object)DBNull.Value : sex);

            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)vm.PhoneNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PhoneNumber2", (object?)vm.PhoneNumber2 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HomeAaddress", (object?)vm.HomeAaddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@school", (object?)vm.School ?? DBNull.Value);

            var rows = cmd.ExecuteNonQuery();
            if (rows < 1)
            {
                message = "저장할 데이터가 없거나 PersonId가 올바르지 않습니다.";
                return false;
            }

            message = "저장되었습니다.";
            return true;
        }
    }
}

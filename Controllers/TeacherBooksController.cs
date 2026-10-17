using BulletinBoard.Helpers;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BulletinBoard.Controllers
{
    [LogActionFilter]
    public class TeacherBooksController : Controller
    {
        private readonly string _conn;

        public TeacherBooksController(IConfiguration configuration)
        {
            _conn = configuration.GetConnectionString("DefaultConnection");
        }

        // GET /viewTeacherBooks/{id}  (id = 과 번호 or "성탄주일" 등)
        public async Task<IActionResult> Index(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToAction(nameof(List));

            var vm = await FetchLesson(id);
            if (vm == null)
                return NotFound();

            // 이전/다음 탐색 (숫자 과만)
            if (vm.LessonNumber.HasValue)
            {
                var prev = await FetchLessonByNumber(vm.LessonNumber.Value - 1);
                var next = await FetchLessonByNumber(vm.LessonNumber.Value + 1);
                vm.PrevLessonKey = prev?.LessonKey;
                vm.PrevLessonNumber = prev?.LessonNumber;
                vm.NextLessonKey = next?.LessonKey;
                vm.NextLessonNumber = next?.LessonNumber;
            }

            return View(vm);
        }

        // GET /viewTeacherBooks/list
        public async Task<IActionResult> List()
        {
            var list = new System.Collections.Generic.List<TeacherBookVM>();
            const string sql = @"
                SELECT Id, LessonKey, LessonNumber, LessonLabel, LessonTitle,
                       UnitNumber, UnitTitle, Page1ImagePath
                FROM [dbo].[TeacherBook]
                WHERE AcademicYear=2026 AND Semester=2 AND PublishYn='y'
                ORDER BY ISNULL(LessonNumber, 9999), LessonKey";

            await using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            await using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new TeacherBookVM
                {
                    Id           = rdr.GetInt32(0),
                    LessonKey    = rdr.IsDBNull(1) ? "" : rdr.GetString(1),
                    LessonNumber = rdr.IsDBNull(2) ? (int?)null : rdr.GetInt32(2),
                    LessonLabel  = rdr.IsDBNull(3) ? "" : rdr.GetString(3),
                    LessonTitle  = rdr.IsDBNull(4) ? "" : rdr.GetString(4),
                    UnitNumber   = rdr.IsDBNull(5) ? (int?)null : rdr.GetInt32(5),
                    UnitTitle    = rdr.IsDBNull(6) ? "" : rdr.GetString(6),
                    Page1ImagePath = rdr.IsDBNull(7) ? "" : rdr.GetString(7),
                });
            }
            return View(list);
        }

        private async Task<TeacherBookVM> FetchLesson(string key)
        {
            const string sql = @"
                SELECT Id,LessonKey,LessonNumber,LessonLabel,LessonTitle,UnitNumber,UnitTitle,
                       BibleTextRef,MemoryVerseRef,MemoryVerseText,LearningObjective,ApplicationText,
                       TeacherStudyTitle,TeacherStudyContent,
                       UnitIntroContent,LearnContent,WordGlassesText,LiveContent,
                       TeacherTip,ClosingContent,PrayerText,
                       Page1ImagePath,Page2ImagePath,Page3ImagePath,Page4ImagePath,
                       AcademicYear,Semester
                FROM [dbo].[TeacherBook]
                WHERE LessonKey=@Key AND AcademicYear=2026 AND Semester=2";

            await using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Key", SqlDbType.NVarChar, 50).Value = key;
            await using var rdr = await cmd.ExecuteReaderAsync();
            if (!await rdr.ReadAsync()) return null;
            return MapRow(rdr);
        }

        private async Task<TeacherBookVM> FetchLessonByNumber(int num)
        {
            const string sql = @"
                SELECT Id,LessonKey,LessonNumber,LessonLabel,LessonTitle,UnitNumber,UnitTitle,
                       BibleTextRef,MemoryVerseRef,MemoryVerseText,LearningObjective,ApplicationText,
                       TeacherStudyTitle,TeacherStudyContent,
                       UnitIntroContent,LearnContent,WordGlassesText,LiveContent,
                       TeacherTip,ClosingContent,PrayerText,
                       Page1ImagePath,Page2ImagePath,Page3ImagePath,Page4ImagePath,
                       AcademicYear,Semester
                FROM [dbo].[TeacherBook]
                WHERE LessonNumber=@Num AND AcademicYear=2026 AND Semester=2";

            await using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Num", SqlDbType.Int).Value = num;
            await using var rdr = await cmd.ExecuteReaderAsync();
            if (!await rdr.ReadAsync()) return null;
            return MapRow(rdr);
        }

        private static TeacherBookVM MapRow(SqlDataReader r) => new()
        {
            Id                  = r.GetInt32(0),
            LessonKey           = r.IsDBNull(1)  ? "" : r.GetString(1),
            LessonNumber        = r.IsDBNull(2)  ? null : r.GetInt32(2),
            LessonLabel         = r.IsDBNull(3)  ? "" : r.GetString(3),
            LessonTitle         = r.IsDBNull(4)  ? "" : r.GetString(4),
            UnitNumber          = r.IsDBNull(5)  ? null : r.GetInt32(5),
            UnitTitle           = r.IsDBNull(6)  ? "" : r.GetString(6),
            BibleTextRef        = r.IsDBNull(7)  ? "" : r.GetString(7),
            MemoryVerseRef      = r.IsDBNull(8)  ? "" : r.GetString(8),
            MemoryVerseText     = r.IsDBNull(9)  ? "" : r.GetString(9),
            LearningObjective   = r.IsDBNull(10) ? "" : r.GetString(10),
            ApplicationText     = r.IsDBNull(11) ? "" : r.GetString(11),
            TeacherStudyTitle   = r.IsDBNull(12) ? "" : r.GetString(12),
            TeacherStudyContent = r.IsDBNull(13) ? "" : r.GetString(13),
            UnitIntroContent    = r.IsDBNull(14) ? "" : r.GetString(14),
            LearnContent        = r.IsDBNull(15) ? "" : r.GetString(15),
            WordGlassesText     = r.IsDBNull(16) ? "" : r.GetString(16),
            LiveContent         = r.IsDBNull(17) ? "" : r.GetString(17),
            TeacherTip          = r.IsDBNull(18) ? "" : r.GetString(18),
            ClosingContent      = r.IsDBNull(19) ? "" : r.GetString(19),
            PrayerText          = r.IsDBNull(20) ? "" : r.GetString(20),
            Page1ImagePath      = r.IsDBNull(21) ? "" : r.GetString(21),
            Page2ImagePath      = r.IsDBNull(22) ? "" : r.GetString(22),
            Page3ImagePath      = r.IsDBNull(23) ? "" : r.GetString(23),
            Page4ImagePath      = r.IsDBNull(24) ? "" : r.GetString(24),
            AcademicYear        = r.GetInt32(25),
            Semester            = r.GetInt32(26),
        };
    }
}

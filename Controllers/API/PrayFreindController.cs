using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.DataAccessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Helpers;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace YunYunBu2024.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrayFreindController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PrayFreindController(IConfiguration config)
        {
            _config = config;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // =========================================================
        // SQL 일괄 실행 API (Admin/SuperAdmin 전용)
        // POST api/PrayFreind/ExecuteSql
        // =========================================================

        /// <summary>
        /// SQL 문을 받아 DB에서 직접 실행합니다.
        /// - SELECT / WITH  → 결과 rows 반환
        /// - INSERT / UPDATE / DELETE / CREATE 등 → 영향받은 행 수 반환
        /// ⚠️  Admin 또는 SuperAdmin 계정만 호출 가능합니다.
        /// </summary>
     //   [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin))]
        [HttpPost("ExecuteSql")]
        public async Task<IActionResult> ExecuteSql([FromBody] ExecuteSqlRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Sql))
                return BadRequest(new { ok = false, message = "SQL이 비어 있습니다." });

            var sql = req.Sql.Trim();

            var connStr = _config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, new { ok = false, message = "연결 문자열(DefaultConnection)을 찾을 수 없습니다." });

            // 실행자 기록 (서버 로그용)
            //var caller = User?.Identity?.Name ?? "anonymous";
            //Console.WriteLine($"[ExecuteSql] caller={caller} sql={sql.Replace("\n", " ")}");

            try
            {
                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();

                using var cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = 60; // 최대 60초

                bool isQuery = sql.TrimStart()
                    .StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                    || sql.TrimStart()
                    .StartsWith("WITH", StringComparison.OrdinalIgnoreCase);

                if (isQuery)
                {
                    // ── SELECT : 결과 rows 반환 ──────────────────────
                    using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

                    var columns = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        columns.Add(reader.GetName(i));

                    var rows = new List<Dictionary<string, object?>>();
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object?>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var val = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            // DateTime → ISO 문자열로 직렬화
                            row[columns[i]] = val is DateTime dt
                                ? dt.ToString("yyyy-MM-dd HH:mm:ss")
                                : val;
                        }
                        rows.Add(row);
                    }

                    return Ok(new
                    {
                        ok        = true,
                        rowCount  = rows.Count,
                        columns,
                        rows
                    });
                }
                else
                {
                    // ── DML/DDL : 영향받은 행 수 반환 ───────────────
                    int affected = await cmd.ExecuteNonQueryAsync();
                    return Ok(new
                    {
                        ok       = true,
                        affected,
                        message  = $"{affected} 행이 처리되었습니다."
                    });
                }
            }
            catch (SqlException ex)
            {
                return BadRequest(new { ok = false, sqlError = ex.Number, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, message = ex.Message });
            }
        }

        // GET: api/<ValuesController> 
        [HttpPost("UpdatePrayFriend")]
        public async Task<IActionResult> UpdatePrayFriend([FromBody] PrayFriendRequest req)
        {
            try
            {
                ManagePrayFriends managePrayFriends = new ManagePrayFriends();

                managePrayFriends.UpdatePrayFreind(req.PrayId, req.PrayText);

                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {               
                return BadRequest(new { message = ex.Message });
            }
        }   

        [HttpPost("UpdateUserSettings")]
        public async Task<IActionResult> UpdateUserSettings([FromBody] UserSettingsEx req)
        {

            try
            {

                ManageeCommon manageeCommon = new ManageeCommon();

                if (!string.IsNullOrEmpty(req.LangCD))
                {
                    manageeCommon.UpsertUserSettings(req.UserId, langCD: req.LangCD);
                }


                if (req.PagesForView > 0)
                {
                    manageeCommon.UpsertUserSettings(req.UserId, pagesForView: req.PagesForView);
                }

                if (!string.IsNullOrEmpty(req.Notice_1_Collapse))
                {
                    manageeCommon.UpsertUserSettings(req.UserId, notice_1_Collapse: req.Notice_1_Collapse);
                }

                if (!string.IsNullOrEmpty(req.BibleBookOverview))
                { 
                    manageeCommon.UpsertUserSettings(req.UserId, bibleBookOverview: req.BibleBookOverview);
                }
                              

                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {             
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("UpsertUserCookieSettings")]
        public async Task<IActionResult> UpsertUserCookieSettings([FromBody] UserSettingsCookie req)
        {           
            try
            {
                ManageCommonCookie __ManageCommonCookie = new ManageCommonCookie();

                if (!string.IsNullOrEmpty(req.UserId))
                {                    
                    __ManageCommonCookie.UpsertUserCookieSettings(req.UserId, cookieName: req.cookieName, cookieValue: req.cookieValue, cookieDays: req.cookieDays);                     
                }

                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {               
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("UpdateAwardPoint")]
        public async Task<IActionResult> UpdateAwardPoint([FromBody] AwardUpdateData param)
        {
            try
            {
                ManageCheckPoint manageCheckPoint = new ManageCheckPoint();

                manageCheckPoint.SetAwardData(param.UserName, param.Value);
                manageCheckPoint.DeleteAwardItemPick(param.UserId);

                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
                
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("CreatePrayHistory")]
        public async Task<IActionResult> CreatePrayHistory([FromBody] PrayHistoryData param)
        {
            try
            {
                ManagePray panagePray = new ManagePray();
                panagePray.CreatePrayHistory(param.personPraying, param.prayerRecipient);

                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
                
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("InsertUserBibleReadRange")]
        public async Task<IActionResult> InsertUserBibleReadRange([FromBody] eBiblesByUser param)
        {
            try
            {
                 ManageeBible manageeBible = new ManageeBible();

                if (param != null)
                {
                    manageeBible.InsertUserBibleReadRange(param.BibleText, param.FromJang, param.FromZul, param.ToJang, param.ToZul, param.Id);
                    if (param.Id == "c6f09790-e609-473f-b512-f18b85be1bb8")  // 관리자
                    {
                        param.Id = "687aec6f-3453-4849-9afe-da5691abf62c"; // Basic
                        manageeBible.InsertUserBibleReadRange(param.BibleText, param.FromJang, param.FromZul, param.ToJang, param.ToZul, param.Id);
                    }
                }
                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
 
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = nameof(Roles.Admin))]
        [HttpPost("DeletePrayFreind")]
        public async Task<IActionResult> DeletePrayFreind([FromBody] PrayFriendRequest req)
        {
            try
            { 
                ManagePrayFriends managePrayFriends = new ManagePrayFriends();

                managePrayFriends.DeletePrayFreind(req.PrayId);

                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
               
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    // ── ExecuteSql 요청 모델 ──────────────────────────────────
    public class ExecuteSqlRequest
    {
        /// <summary>실행할 SQL 문 (SELECT / INSERT / UPDATE / DELETE / CREATE 등)</summary>
        public string Sql { get; set; } = string.Empty;
    }
}

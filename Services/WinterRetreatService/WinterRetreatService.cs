using BulletinBoard.Common;
using BulletinBoard.Helpers;
using BulletinBoard.Services.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BulletinBoard.Services
{
    public class VBSParticipationSurveyService : IVBSParticipationSurveyService
    {
        private readonly string _conn;

        public VBSParticipationSurveyService(IConfiguration configuration)
        {
            _conn = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        private SqlConnection Conn() => new SqlConnection(_conn);

        private static readonly DateTime[] Days = new[]
        {
            ConstatsCommon.VBSParticipationStartDate,
            ConstatsCommon.VBSParticipationStartDate.AddDays(1),
            ConstatsCommon.VBSParticipationStartDate.AddDays(2)
        };
        // ✅ 참여 여부(= 참석 정보 입력이 한 번이라도 있는지) 판단
        // 현재 로직은 SurveyFinal OR Attendance 하나라도 있으면 참여로 간주
        public async Task<bool> HasParticipatedAsync(string UserName)
        {
            var sql = @"
SELECT CASE WHEN EXISTS(
    SELECT 1 FROM dbo.VBSParticipationSurveySurveyFinal WHERE UserName=@UserName
    UNION ALL
    SELECT 1 FROM dbo.VBSParticipationSurveyAttendance WHERE UserName=@UserName
) THEN 1 ELSE 0 END;";

            await using var con = Conn();
            await con.OpenAsync();

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 100) { Value = UserName });

            var r = await cmd.ExecuteScalarAsync();
            return (r != null && Convert.ToInt32(r) == 1);
        }

        /// <summary>
        /// ✅ 홈 가이드 팝업 노출 여부 (UserName 기준)
        /// - 1) 로그인 유저가 참여(= 참석 입력) 했으면 팝업은 의미 없음 → false
        /// - 2) 참여 안 했을 때만 dbo.VBSParticipationSurveyGuideUserState.NextShowAt으로 판단
        /// </summary>
        public async Task<bool> ShouldShowGuideAsync(string UserName, string? UserId)
        {
            // 로그인 사용자 기준 팝업이므로 UserName이 없으면 띄우지 않음(안전)
            if (string.IsNullOrWhiteSpace(UserName)) return false;

            // 1) ✅ 참여(참석 입력) 했으면 팝업은 절대 안 뜸
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                if (await HasParticipatedAsync(UserName)) return false;
            }

            // 2) 참여 안 한 사람만 유예 상태 체크
            var sql = @"
SELECT NextShowAt
FROM dbo.VBSParticipationSurveyGuideUserState WITH (NOLOCK)
WHERE UserName=@UserName;";

            await using var con = Conn();
            await con.OpenAsync();

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 100) { Value = UserName });

            var obj = await cmd.ExecuteScalarAsync();

            // row가 없거나 NextShowAt이 NULL이면 => 팝업 노출
            if (obj == null || obj == DBNull.Value) return true;

            var next = Convert.ToDateTime(obj);
            return DateHelper.GetKoreaNow() >= next;
        }

        /// <summary>
        /// ✅ 홈 가이드 팝업 유예 (UserName 기준)
        /// days=0 => "다음에 할래요" (저장 안 함)
        /// days=3 => "3일후 할래요"
        /// days=7 => "7일후 할래요"
        /// </summary>
        public async Task DeferGuideAsync(string userName, int days)
        {
            if (string.IsNullOrWhiteSpace(userName)) return;

            // "다음에 할래요"는 DB에 유예를 저장하지 않음(다음 접속 시 다시 판단)
            if (days <= 0) return;

            // 음수 방지 + 허용값 정리(원하면 3/7만 허용해도 됨)
            if (days != 3 && days != 7) days = 3;

            var sql = @"
MERGE dbo.VBSParticipationSurveyGuideUserState AS T
USING (SELECT @UserName AS UserName) AS S
ON (T.UserName = S.UserName)
WHEN MATCHED THEN
    UPDATE SET NextShowAt = DATEADD(DAY, @Days, GETDATE()),
               UpdatedAt = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (UserName, NextShowAt, UpdatedAt)
    VALUES (@UserName, DATEADD(DAY, @Days, GETDATE()), GETDATE());";

            await using var con = Conn();
            await con.OpenAsync();

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 256) { Value = userName });
            cmd.Parameters.Add(new SqlParameter("@Days", SqlDbType.Int) { Value = days });

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpsertSlotAsync(
            string UserName,
            string fullName,
            string roleCode,
            DateTime date,
            string slot,
            bool? isAttend,
            string requestIp,
            string? updatedByUserName,
            string? statusNote = null
        )
        {
            var sql = @"
MERGE dbo.VBSParticipationSurveyAttendance AS T
USING (SELECT @UserName AS UserName, @D AS RetreatDate, @Slot AS Slot) AS S
ON (T.UserName=S.UserName AND T.RetreatDate=S.RetreatDate AND T.Slot=S.Slot)
WHEN MATCHED THEN
    UPDATE SET IsAttend=@IsAttend, StatusNote=@StatusNote, UpdatedAt=GETDATE(), FullName=@FullName, RoleCode=@RoleCode, RequestIp=@Ip, UpdatedByUserName=@UpdatedByUserName
WHEN NOT MATCHED THEN
    INSERT (UserName, FullName, RoleCode, RetreatDate, Slot, IsAttend, StatusNote, UpdatedAt, RequestIp, UpdatedByUserName)
    VALUES (@UserName, @FullName, @RoleCode, @D, @Slot, @IsAttend, @StatusNote, GETDATE(), @Ip, @UpdatedByUserName);";

            await using var con = Conn();
            await con.OpenAsync();

            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 100) { Value = UserName });
            cmd.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = (object?)fullName ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.NVarChar, 20) { Value = (object?)roleCode ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@D", SqlDbType.Date) { Value = date.Date });
            cmd.Parameters.Add(new SqlParameter("@Slot", SqlDbType.Char, 2) { Value = slot });
            cmd.Parameters.Add(new SqlParameter("@IsAttend", SqlDbType.Bit) { Value = (object?)isAttend ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@StatusNote", SqlDbType.NVarChar, 50) { Value = (object?)statusNote ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Ip", SqlDbType.NVarChar, 64) { Value = (object?)requestIp ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@UpdatedByUserName", SqlDbType.NVarChar, 100) { Value = (object?)updatedByUserName ?? DBNull.Value });

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SetAllStatusNoteAsync(string UserName, string fullName, string roleCode, string statusNote, string requestIp)
        {
            foreach (var d in Days)
            {
                await UpsertSlotAsync(UserName, fullName, roleCode, d, "AM", null, requestIp, UserName, statusNote);
                await UpsertSlotAsync(UserName, fullName, roleCode, d, "PM", null, requestIp, UserName, statusNote);
            }
        }

        public async Task SetAllAsync(string UserName, string fullName, string roleCode, bool isAttend, string requestIp)
        {
            foreach (var d in Days)
            {
                await UpsertSlotAsync(UserName, fullName, roleCode, d, "AM", isAttend, requestIp, UserName);
                await UpsertSlotAsync(UserName, fullName, roleCode, d, "PM", isAttend, requestIp, UserName);
            }

            var mode = isAttend ? "ALL_ATTEND" : "ALL_ABSENT";

            var sql = @"
MERGE dbo.VBSParticipationSurveySurveyFinal AS T
USING (SELECT @UserName AS UserName) AS S
ON T.UserName=S.UserName
WHEN MATCHED THEN UPDATE SET FinalMode=@Mode, FinalAt=GETDATE(), RequestIp=@Ip
WHEN NOT MATCHED THEN INSERT(UserName, FinalMode, FinalAt, RequestIp) VALUES(@UserName, @Mode, GETDATE(), @Ip);";

            await using var con = Conn();
            await con.OpenAsync();

            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 100) { Value = UserName });
            cmd.Parameters.Add(new SqlParameter("@Mode", SqlDbType.NVarChar, 20) { Value = mode });
            cmd.Parameters.Add(new SqlParameter("@Ip", SqlDbType.NVarChar, 64) { Value = (object?)requestIp ?? DBNull.Value });

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Dictionary<(DateTime Date, string Slot), (bool? IsAttend, string? StatusNote)>> GetMySlotsAsync(string UserName)
        {
            var map = new Dictionary<(DateTime, string), (bool? IsAttend, string? StatusNote)>();

            var sql = @"
;WITH ranked AS (
    SELECT RetreatDate, Slot, IsAttend, StatusNote,
           ROW_NUMBER() OVER(
             PARTITION BY RetreatDate, Slot
             ORDER BY UpdatedAt DESC
           ) AS rn
    FROM dbo.VBSParticipationSurveyAttendance WITH (NOLOCK)
    WHERE UserName=@UserName
      AND RetreatDate IN (@D1,@D2,@D3)
)
SELECT RetreatDate, Slot, IsAttend, StatusNote
FROM ranked
WHERE rn = 1;";

            await using var con = Conn();
            await con.OpenAsync();

            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 100) { Value = UserName });
            cmd.Parameters.Add(new SqlParameter("@D1", SqlDbType.Date) { Value = Days[0].Date });
            cmd.Parameters.Add(new SqlParameter("@D2", SqlDbType.Date) { Value = Days[1].Date });
            cmd.Parameters.Add(new SqlParameter("@D3", SqlDbType.Date) { Value = Days[2].Date });

            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var d = r.GetDateTime(0).Date;
                var slot = r.GetString(1);
                bool? isAttend = r.IsDBNull(2) ? (bool?)null : r.GetBoolean(2);
                string? statusNote = r.IsDBNull(3) ? null : r.GetString(3);

                map[(d, slot)] = (isAttend, statusNote);
            }

            return map;
        }


        public async Task<VBSParticipationSurveyAdminVM> GetAdminVMAsync(IEnumerable<Models.UserInfo> users , string DeptCode)
        {
            var vm = new VBSParticipationSurveyAdminVM { Days = Days };

            // -------------------------------------------------
            // 0) ✅ RoleCode 정렬순서 로드 (MajorCD='T001')
            //     - MinorCD = RoleCode (T1001, T1002 ...)
            // -------------------------------------------------
            try
            {
                const string roleOrderSql = @"
SELECT MinorCD, displayOrder
FROM dbo.CodeData WITH (NOLOCK)
WHERE MajorCD = 'T001';";

                await using var conRole = Conn();
                await conRole.OpenAsync();

                await using var cmdRole = new SqlCommand(roleOrderSql, conRole);
                await using var rrRole = await cmdRole.ExecuteReaderAsync();

                while (await rrRole.ReadAsync())
                {
                    var minorCd = rrRole.IsDBNull(0) ? "" : (rrRole.GetString(0) ?? "");
                    minorCd = minorCd.Trim();

                    int order = rrRole.IsDBNull(1) ? 999999 : Convert.ToInt32(rrRole.GetValue(1));

                    if (!string.IsNullOrWhiteSpace(minorCd))
                        vm.RoleDisplayOrder[minorCd] = order;
                }
            }
            catch
            {
                // ✅ 정렬 데이터가 없어도 화면 자체는 살아야 하므로 조용히 무시
            } 

            var userList = users?.ToList() ?? new List<Models.UserInfo>();

            // ✅ 대상 UserName 목록
            var userNames = userList
                .Select(u => (u.UserName ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (userNames.Count == 0) return vm;

            // -------------------------------------------------
            // 1) ✅ VBSParticipationSurveyAttendance 로드 (FullName 포함)
            //    - 이후 view_UserCompositeInfo_2026의 FullName join용
            // -------------------------------------------------
            var attendSql = $@"
SELECT UserName, FullName, RetreatDate, Slot, IsAttend, StatusNote
FROM dbo.VBSParticipationSurveyAttendance WITH (NOLOCK)
WHERE RetreatDate IN (@D1,@D2,@D3)
  AND UserName IN ({string.Join(",", userNames.Select((x, i) => $"@U{i}"))});";

            var dict = new Dictionary<(string UserName, DateTime Date, string Slot), bool?>();
            var noteDict = new Dictionary<(string UserName, DateTime Date, string Slot), string?>();
            var attendFullNameByUserName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var attendFullNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await using (var con = Conn())
            {
                await con.OpenAsync();
                await using var cmd = new SqlCommand(attendSql, con);

                cmd.Parameters.Add(new SqlParameter("@D1", SqlDbType.Date) { Value = Days[0].Date });
                cmd.Parameters.Add(new SqlParameter("@D2", SqlDbType.Date) { Value = Days[1].Date });
                cmd.Parameters.Add(new SqlParameter("@D3", SqlDbType.Date) { Value = Days[2].Date });

                for (int i = 0; i < userNames.Count; i++)
                    cmd.Parameters.Add(new SqlParameter($"@U{i}", SqlDbType.NVarChar, 100) { Value = userNames[i] });

                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var un = r.IsDBNull(0) ? "" : (r.GetString(0) ?? "");
                    un = un.Trim();

                    var fn = r.IsDBNull(1) ? "" : (r.GetString(1) ?? "");
                    fn = fn.Trim();

                    var d = r.GetDateTime(2).Date;
                    var slot = r.GetString(3);
                    bool? v = r.IsDBNull(4) ? (bool?)null : r.GetBoolean(4);
                    string? sn = r.IsDBNull(5) ? null : r.GetString(5);

                    if (!string.IsNullOrWhiteSpace(un))
                    {
                        dict[(un, d, slot)] = v;
                        noteDict[(un, d, slot)] = sn;

                        // ✅ UserName → FullName 매핑(가장 최신/유효값 유지)
                        if (!string.IsNullOrWhiteSpace(fn))
                        {
                            attendFullNameByUserName[un] = fn;
                            attendFullNames.Add(fn);
                        }
                    }
                }
            }

            // -------------------------------------------------
            // 2) ✅ 사용자 추가 정보 로드(+ PersonId/RoleCode)
            //    - view_UserCompositeInfo_2026 에서
            //      (UserName IN ...) OR (FullName IN ...)
            //    - view에서 UserName이 NULL이면 FullName으로 매칭
            // -------------------------------------------------
            var userFullNames = userList
                .Select(u => (u.FullName ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // ✅ FullName 후보 = users.FullName + attendance.FullName
            var fullNames = userFullNames
                .Concat(attendFullNames)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // ✅ infoMap을 2개로: UserName키 / FullName키
            //    (PersonId/RoleCode 추가)
            var infoByUserName = new Dictionary<string, (int? ClassYear, int? ClassBan, string RoleNM, string RoleCode, int? PersonId)>(StringComparer.OrdinalIgnoreCase);
            var infoByFullName = new Dictionary<string, (int? ClassYear, int? ClassBan, string RoleNM, string RoleCode, int? PersonId)>(StringComparer.OrdinalIgnoreCase);

            await using (var conInfo = Conn())
            {
                await conInfo.OpenAsync();

                var hasFullNames = fullNames.Count > 0;

                var infoSql = $@"
SELECT UserName, FullName, PersonId, RoleCode, ClassYear, ClassBan, RoleNM
FROM dbo.view_UserCompositeInfo_2026 WITH (NOLOCK)
WHERE DeptCode = '{DeptCode}' and UserName IN ({string.Join(",", userNames.Select((x, i) => $"@IU{i}"))})
{(hasFullNames ? $"   OR FullName IN ({string.Join(",", fullNames.Select((x, i) => $"@IF{i}"))})" : "")};";

                await using var cmdInfo = new SqlCommand(infoSql, conInfo);

                for (int i = 0; i < userNames.Count; i++)
                    cmdInfo.Parameters.Add(new SqlParameter($"@IU{i}", SqlDbType.NVarChar, 100) { Value = userNames[i] });

                if (hasFullNames)
                {
                    for (int i = 0; i < fullNames.Count; i++)
                        cmdInfo.Parameters.Add(new SqlParameter($"@IF{i}", SqlDbType.NVarChar, 200) { Value = fullNames[i] });
                }

                await using var rr = await cmdInfo.ExecuteReaderAsync();
                while (await rr.ReadAsync())
                {
                    var un = rr.IsDBNull(0) ? "" : (rr.GetString(0) ?? "");
                    un = un.Trim();

                    var fn = rr.IsDBNull(1) ? "" : (rr.GetString(1) ?? "");
                    fn = fn.Trim();

                    int? personId = rr.IsDBNull(2) ? (int?)null : Convert.ToInt32(rr.GetValue(2));

                    var roleCodeFromView = rr.IsDBNull(3) ? "" : (rr.GetString(3) ?? "");
                    roleCodeFromView = (roleCodeFromView ?? "").Trim();

                    int? cy = rr.IsDBNull(4) ? (int?)null : Convert.ToInt32(rr.GetValue(4));
                    int? cb = rr.IsDBNull(5) ? (int?)null : Convert.ToInt32(rr.GetValue(5));

                    var rnm = rr.IsDBNull(6) ? "" : (rr.GetString(6) ?? "");
                    rnm = (rnm ?? "").Trim();

                    var val = (cy, cb, rnm, roleCodeFromView, personId);

                    if (!string.IsNullOrWhiteSpace(un))
                        infoByUserName[un] = val;

                    if (!string.IsNullOrWhiteSpace(fn))
                        infoByFullName[fn] = val;
                }
            }

            // -------------------------------------------------
            // 2-1) ✅ 반 매핑 로드: (TeacherPersonId/ SubTeacherPersonId) → (ClassYear, ClassBan)
            //     - 여기서는 RoleCode로 필터링하지 말고 PersonId 있는 대상 전부를 대상으로 매핑을 만들어둔다.
            //     - 이후 표시 단계에서 "교사"인 사람에게만 "{N}학년 {M}반 교사" 적용.
            // -------------------------------------------------
            var teacherClassByPersonId = new Dictionary<int, (int? ClassYear, int? ClassBan)>();

            // ✅ PersonId가 있는 대상은 전부 후보로 모은다 (RoleCode 필터링 금지)
            var personIds = infoByUserName.Values
                .Concat(infoByFullName.Values)
                .Where(x => x.PersonId.HasValue)
                .Select(x => x.PersonId!.Value)
                .Distinct()
                .ToList();

            if (personIds.Count > 0)
            {
                await using var conT = Conn();
                await conT.OpenAsync();

                // ✅ 담임 + 보조 모두 포함
                //   - SubTeacherPersonId는 >0 인 경우만 의미 있음
                //   - IN 파라미터 배열을 1번만 만들어서 재사용
                var inParams = string.Join(",", personIds.Select((x, i) => $"@P{i}"));

                var tSql = $@"
SELECT TeacherPersonId, SubTeacherPersonId, ClassYear, ClassBan
FROM dbo.ClassTeacherMap_2026 WITH (NOLOCK)
WHERE TeacherPersonId IN ({inParams})
   OR (SubTeacherPersonId > 0 AND SubTeacherPersonId IN ({inParams}));";

                await using var cmdT = new SqlCommand(tSql, conT);
                for (int i = 0; i < personIds.Count; i++)
                    cmdT.Parameters.Add(new SqlParameter($"@P{i}", SqlDbType.Int) { Value = personIds[i] });

                await using var tr = await cmdT.ExecuteReaderAsync();
                while (await tr.ReadAsync())
                {
                    int? tp = tr.IsDBNull(0) ? (int?)null : Convert.ToInt32(tr.GetValue(0));
                    int? sp = tr.IsDBNull(1) ? (int?)null : Convert.ToInt32(tr.GetValue(1));
                    int? cy = tr.IsDBNull(2) ? (int?)null : Convert.ToInt32(tr.GetValue(2));
                    int? cb = tr.IsDBNull(3) ? (int?)null : Convert.ToInt32(tr.GetValue(3));

                    if (tp.HasValue)
                        teacherClassByPersonId[tp.Value] = (cy, cb);

                    if (sp.HasValue && sp.Value > 0)
                        teacherClassByPersonId[sp.Value] = (cy, cb);
                }
            }

            // -------------------------------------------------
            // 3) ✅ VM Rows 조립: UserName 우선 → (Attendance FullName) → (User.FullName)
            //    + 교사(T1002) RoleNM 표시: "{ClassYear}학년 {ClassBan}반 교사"
            // -------------------------------------------------
            foreach (var u in userList)
            {
                var un = (u.UserName ?? "").Trim();
                var userFn = (u.FullName ?? "").Trim();

                // Attendance에 저장된 FullName (view UserName NULL 케이스 대응용)
                attendFullNameByUserName.TryGetValue(un, out var attendFn);
                attendFn = (attendFn ?? "").Trim();

                (int? ClassYear, int? ClassBan, string RoleNM, string RoleCode, int? PersonId) info;

                // 1) UserName으로 매칭
                if (!string.IsNullOrWhiteSpace(un) && infoByUserName.TryGetValue(un, out info))
                {
                    // ok
                }
                // 2) Attendance의 FullName으로 매칭
                else if (!string.IsNullOrWhiteSpace(attendFn) && infoByFullName.TryGetValue(attendFn, out info))
                {
                    // ok
                }
                // 3) User.FullName으로 매칭
                else if (!string.IsNullOrWhiteSpace(userFn) && infoByFullName.TryGetValue(userFn, out info))
                {
                    // ok
                }
                else
                {
                    info = (null, null, "", "", null);
                }

                // ✅ 표시용 RoleNM 계산
                string displayRoleNm = info.RoleNM ?? "";

                // ✅ 교사 여부 판단 보강: info.RoleCode 뿐 아니라 u.RoleCode도 함께 본다
                var isTeacher =
                    string.Equals((info.RoleCode ?? "").Trim(), "T1002", StringComparison.OrdinalIgnoreCase)
                    || string.Equals((u.RoleCode ?? "").Trim(), "T1002", StringComparison.OrdinalIgnoreCase);

                // ✅ 교사인 경우: PersonId → 반 매핑 우선 적용
                if (isTeacher && info.PersonId.HasValue)
                {
                    if (teacherClassByPersonId.TryGetValue(info.PersonId.Value, out var tc))
                    {
                        var tcy = tc.ClassYear ?? info.ClassYear;
                        var tcb = tc.ClassBan ?? info.ClassBan;

                        if (tcy.HasValue && tcb.HasValue)
                            displayRoleNm = $"{tcy.Value}학년 {tcb.Value}반 교사";
                        else if (tcy.HasValue)
                            displayRoleNm = $"{tcy.Value}학년 교사";
                        else
                            displayRoleNm = "교사";
                    }
                    else
                    {
                        // 매핑이 없으면 view의 값으로라도 표시
                        if (info.ClassYear.HasValue && info.ClassBan.HasValue)
                            displayRoleNm = $"{info.ClassYear.Value}학년 {info.ClassBan.Value}반 교사";
                        else
                            displayRoleNm = "교사";
                    }
                }

                var row = new VBSParticipationSurveyAdminRow
                {
                    UserName = un,
                    FullName = string.IsNullOrWhiteSpace(userFn) ? un : userFn,  // 표시용 이름
                    RoleCode = (u.RoleCode ?? "").Trim(),
                    ClassYear = info.ClassYear,
                    ClassBan = info.ClassBan,
                    RoleNM = displayRoleNm
                };

                foreach (var d in Days)
                {
                    row.Slots[(d, "AM")] = dict.TryGetValue((row.UserName, d, "AM"), out var am) ? am : (bool?)null;
                    row.Slots[(d, "PM")] = dict.TryGetValue((row.UserName, d, "PM"), out var pm) ? pm : (bool?)null;
                    row.StatusNotes[(d, "AM")] = noteDict.TryGetValue((row.UserName, d, "AM"), out var snAm) ? snAm : null;
                    row.StatusNotes[(d, "PM")] = noteDict.TryGetValue((row.UserName, d, "PM"), out var snPm) ? snPm : null;
                }

                vm.Rows.Add(row);
            }

            // -------------------------------------------------
            // 4) ✅ 요약
            // -------------------------------------------------
            foreach (var d in Days)
            {
                int am = vm.Rows.Count(r => r.Slots[(d, "AM")] == true);
                int pm = vm.Rows.Count(r => r.Slots[(d, "PM")] == true);

                vm.Summaries.Add(new VBSParticipationSurveyDaySummary
                {
                    Date = d,
                    AmAttend = am,
                    PmAttend = pm
                });
            }

            return vm;
        }


    }
}

using BulletinBoard.DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

public class UserUiPreferenceService : IUserUiPreferenceService
{
    private readonly IConfiguration _cfg;
    private readonly string _ipSalt;
    private readonly UiMenuPolicyRepository _menuRepo;

    public UserUiPreferenceService(IConfiguration cfg, UiMenuPolicyRepository menuRepo)
    {
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _menuRepo = menuRepo ?? throw new ArgumentNullException(nameof(menuRepo));
        _ipSalt = _cfg["UiPref:IpSalt"] ?? "CHANGE_ME_SALT";
    }

    public Dictionary<string, string> GetMenuPolicyMap()
        => _menuRepo.GetAllActivePolicies();

    public bool IsMenuAllowed(Dictionary<string, string> map, char userType, string menuUrl)
    {
        if (string.IsNullOrWhiteSpace(menuUrl)) return true;
        if (map == null) return true;

        menuUrl = NormalizeMenuUrl(menuUrl);

        if (!map.TryGetValue(menuUrl, out var allowed) || string.IsNullOrWhiteSpace(allowed))
            return true;

        allowed = allowed.Trim().ToUpperInvariant();
        var t = char.ToUpperInvariant(userType);
        return allowed.IndexOf(t) >= 0;
    }

    private static string NormalizeMenuUrl(string url)
    {
        url = (url ?? "").Trim();
        if (url.Length == 0) return url;

        var hashPos = url.IndexOf('#');
        if (hashPos >= 0) url = url.Substring(0, hashPos);

        var qPos = url.IndexOf('?');
        if (qPos >= 0) url = url.Substring(0, qPos);

        url = url.Trim();

        if (url.Length > 1 && url.EndsWith("/"))
            url = url.TrimEnd('/');

        return url;
    }

    private SqlConnection OpenConn()
        => new SqlConnection(_cfg.GetConnectionString("DefaultConnection"));

    // =========================
    // IPKey (비로그인 식별자)
    // =========================
    private string GetClientIp(HttpContext http)
    {
        if (http == null) return "";

        var cf = http.Request.Headers["CF-Connecting-IP"].ToString();
        if (!string.IsNullOrWhiteSpace(cf)) return cf.Trim();

        var tci = http.Request.Headers["True-Client-IP"].ToString();
        if (!string.IsNullOrWhiteSpace(tci)) return tci.Trim();

        var xff = http.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(xff))
        {
            foreach (var part in xff.Split(','))
            {
                var ip = (part ?? "").Trim();
                if (ip.Length == 0) continue;
                if (ip.Equals("unknown", StringComparison.OrdinalIgnoreCase)) continue;
                return ip;
            }
        }

        var xri = http.Request.Headers["X-Real-IP"].ToString();
        if (!string.IsNullOrWhiteSpace(xri))
            return xri.Trim();

        return http.Connection.RemoteIpAddress?.ToString() ?? "";
    }

    private string GetIpKey(HttpContext http)
    {
        var ip = GetClientIp(http);
        if (string.IsNullOrWhiteSpace(ip)) return "";

        var raw = ip + "|" + _ipSalt;
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes); // 64 hex
    }

    private static char NormalizeType(char t)
    {
        t = char.ToUpperInvariant(t);
        return (t == 'A' || t == 'T' || t == 'P' || t == 'C') ? t : 'C';
    }

    private static string NormalizeUserName(string? userName)
    {
        userName = (userName ?? "").Trim();
        return userName.Length == 0 ? "" : userName;
    }

    // =========================
    // 조회: 로그인(UserName) 우선 → IPKey
    // =========================
    public char? GetEffectiveUserType(HttpContext http, string? userName)
    {
        userName = NormalizeUserName(userName);
        var ipKey = GetIpKey(http);

        using var conn = OpenConn();
        conn.Open();

        // ✅ 로그인 상태라면: 같은 브라우저의 IPKey 행을 UserName으로 "연결/정리"
        if (!string.IsNullOrWhiteSpace(userName))
            LinkOrCleanupOnLogin(conn, userName, ipKey);

        // 1) 로그인 우선
        if (!string.IsNullOrWhiteSpace(userName))
        {
            var t = GetByUserName(conn, userName);
            if (t != null)
            {
                TouchByUserName(conn, userName);
                return t;
            }
        }

        // 2) 비로그인: IPKey
        if (!string.IsNullOrWhiteSpace(ipKey))
        {
            var ti = GetByIpKey(conn, ipKey);
            if (ti != null)
            {
                TouchByIpKey(conn, ipKey);
                return ti;
            }
        }

        return null;
    }

    // =========================
    // 저장: 로그인(UserName)면 UserName, 아니면 IPKey
    // =========================
    public void SaveUserTypeForCurrent(HttpContext http, string? userName, char userType)
    {
        userName = NormalizeUserName(userName);
        userType = NormalizeType(userType);
        var ipKey = GetIpKey(http);

        using var conn = OpenConn();
        conn.Open();

        // ✅ 로그인: 가능하면 "현재 브라우저(IPKey)" 행을 승격해서 같은 row에 저장
        if (!string.IsNullOrWhiteSpace(userName))
        {
            if (!string.IsNullOrWhiteSpace(ipKey))
            {
                if (PromoteIpRowToUserName(conn, userName, ipKey, userType))
                {
                    // 승격 성공 후, 혹시 남아있는 IPKey 찌꺼기(빈 UserName) 정리
                    CleanupEmptyUserNameByIpKey(conn, ipKey);
                    return;
                }

                // 이미 UserName row가 있거나, 둘 다 존재하는 상황이면 정리
                LinkOrCleanupOnLogin(conn, userName, ipKey);
            }

            // 기본: UserName으로 Upsert
            UpsertByUserName(conn, userName, userType);
            return;
        }

        // ✅ 비로그인: IPKey로 Upsert
        if (!string.IsNullOrWhiteSpace(ipKey))
        {
            UpsertByIpKey(conn, ipKey, userType);
            return;
        }
    }

    // =========================
    // 승격/연결: 로그인 시점에 IPKey 행을 UserName 행으로 "합치기"
    // =========================
    public void PromoteIfNeeded(HttpContext http, string userName)
    {
        userName = NormalizeUserName(userName);
        if (string.IsNullOrWhiteSpace(userName)) return;

        var ipKey = GetIpKey(http);

        using var conn = OpenConn();
        conn.Open();

        LinkOrCleanupOnLogin(conn, userName, ipKey);
    }

    /// <summary>
    /// 로그인하면, 같은 브라우저(IPKey)에서 만들어진 비로그인 행을
    /// - 가능하면 같은 row에 UserName을 채워 "한 행으로 합치고"
    /// - 이미 UserName 행이 따로 있으면 IPKey 쪽 찌꺼기를 제거
    /// </summary>
    private void LinkOrCleanupOnLogin(SqlConnection conn, string userName, string ipKey)
    {
        if (string.IsNullOrWhiteSpace(userName)) return;
        if (string.IsNullOrWhiteSpace(ipKey)) return;

        var hasUserRow = ExistsByUserName(conn, userName);
        var hasIpRow = ExistsByIpKey(conn, ipKey);

        if (!hasIpRow) return;

        if (!hasUserRow)
        {
            // ✅ UserName 행이 없고 IPKey 행이 있으면:
            // IPKey 행에 UserName을 채워 "한 행으로 승격"
            const string sql = @"
UPDATE dbo.UserUiPreference
SET UserName = @UserName,
    LastSeenDT = SYSDATETIME(),
    UpdateDT = SYSDATETIME()
WHERE IPKey = @IPKey
  AND (UserName IS NULL OR LTRIM(RTRIM(UserName))='');";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = userName;
                cmd.Parameters.Add("@IPKey", SqlDbType.NVarChar, 64).Value = ipKey;
                cmd.ExecuteNonQuery();
            }

            // ✅ 혹시 같은 IPKey로 중복 row가 있으면(비정상) 빈 UserName은 정리
            CleanupEmptyUserNameByIpKey(conn, ipKey);
            return;
        }

        // ✅ 둘 다 존재하면: IPKey로만 존재하는 찌꺼기(빈 UserName) 행 삭제
        CleanupEmptyUserNameByIpKey(conn, ipKey);
    }

    private void CleanupEmptyUserNameByIpKey(SqlConnection conn, string ipKey)
    {
        const string del = @"
DELETE FROM dbo.UserUiPreference
WHERE IPKey=@IPKey
  AND (UserName IS NULL OR LTRIM(RTRIM(UserName))='');";

        using var dcmd = new SqlCommand(del, conn);
        dcmd.Parameters.Add("@IPKey", SqlDbType.NVarChar, 64).Value = ipKey;
        dcmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 로그인 상태에서 저장할 때, IPKey row를 UserName으로 승격하면서 UserType까지 반영.
    /// 성공하면 true.
    /// </summary>
    private bool PromoteIpRowToUserName(SqlConnection conn, string userName, string ipKey, char userType)
    {
        // 이미 UserName row가 있으면 승격하지 않음(유니크 충돌 위험)
        if (ExistsByUserName(conn, userName))
            return false;

        const string sql = @"
UPDATE dbo.UserUiPreference
SET UserName=@UserName,
    UserType=@UserType,
    LastSeenDT=SYSDATETIME(),
    UpdateDT=SYSDATETIME()
WHERE IPKey=@IPKey
  AND (UserName IS NULL OR LTRIM(RTRIM(UserName))='');";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = userName;
        cmd.Parameters.Add("@UserType", SqlDbType.Char, 1).Value = userType;
        cmd.Parameters.Add("@IPKey", SqlDbType.NVarChar, 64).Value = ipKey;

        var rows = cmd.ExecuteNonQuery();
        return rows > 0;
    }

    // =========================
    // Clear: 현재 브라우저 기준으로도 정리
    // =========================
    public void ClearUserTypeForCurrent(HttpContext http, string? userName)
    {
        userName = NormalizeUserName(userName);
        var ipKey = GetIpKey(http);

        using var conn = OpenConn();
        conn.Open();

        if (!string.IsNullOrWhiteSpace(userName))
        {
            const string sql = @"
DELETE FROM dbo.UserUiPreference
WHERE UserName=@UserName
   OR (IPKey=@IPKey AND (UserName IS NULL OR LTRIM(RTRIM(UserName))='' OR UserName=@UserName));";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = userName;
            cmd.Parameters.Add("@IPKey", SqlDbType.NVarChar, 64).Value =
                string.IsNullOrWhiteSpace(ipKey) ? (object)DBNull.Value : ipKey;
            cmd.ExecuteNonQuery();
            return;
        }

        if (!string.IsNullOrWhiteSpace(ipKey))
        {
            const string sql = "DELETE FROM dbo.UserUiPreference WHERE IPKey=@IPKey";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@IPKey", SqlDbType.NVarChar, 64).Value = ipKey;
            cmd.ExecuteNonQuery();
        }
    }

    // =========================
    // DB helper: Exists
    // =========================
    private bool ExistsByUserName(SqlConnection conn, string userName)
    {
        const string sql = "SELECT 1 WHERE EXISTS(SELECT 1 FROM dbo.UserUiPreference WHERE UserName=@UserName)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = userName;
        return cmd.ExecuteScalar() != null;
    }

    private bool ExistsByIpKey(SqlConnection conn, string ipKey)
    {
        const string sql = "SELECT 1 WHERE EXISTS(SELECT 1 FROM dbo.UserUiPreference WHERE IPKey=@IPKey)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@IPKey", SqlDbType.NVarChar, 64).Value = ipKey;
        return cmd.ExecuteScalar() != null;
    }

    // =========================
    // DB helper: Get
    // =========================
    private char? GetByUserName(SqlConnection conn, string userName)
    {
        const string sql = "SELECT TOP 1 UserType FROM dbo.UserUiPreference WHERE UserName=@UserName";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = userName;

        var o = cmd.ExecuteScalar();
        var s = o?.ToString()?.Trim();
        return string.IsNullOrEmpty(s) ? null : s[0];
    }

    private char? GetByIpKey(SqlConnection conn, string ipKey)
    {
        const string sql = "SELECT TOP 1 UserType FROM dbo.UserUiPreference WHERE IPKey=@IPKey";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@IPKey", SqlDbType.NVarChar, 64).Value = ipKey;

        var o = cmd.ExecuteScalar();
        var s = o?.ToString()?.Trim();
        return string.IsNullOrEmpty(s) ? null : s[0];
    }

    // =========================
    // DB helper: Touch
    // =========================
    private void TouchByUserName(SqlConnection conn, string userName)
    {
        const string sql = @"
UPDATE dbo.UserUiPreference
SET LastSeenDT=SYSDATETIME(), UpdateDT=SYSDATETIME()
WHERE UserName=@UserName";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = userName;
        cmd.ExecuteNonQuery();
    }

    private void TouchByIpKey(SqlConnection conn, string ipKey)
    {
        const string sql = @"
UPDATE dbo.UserUiPreference
SET LastSeenDT=SYSDATETIME(), UpdateDT=SYSDATETIME()
WHERE IPKey=@IPKey";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@IPKey", SqlDbType.NVarChar, 64).Value = ipKey;
        cmd.ExecuteNonQuery();
    }

    // =========================
    // DB helper: Upsert (MERGE)
    // =========================
    private void UpsertByUserName(SqlConnection conn, string userName, char userType)
    {
        const string sql = @"
MERGE dbo.UserUiPreference WITH (HOLDLOCK) AS t
USING (SELECT @UserName AS UserName) AS s
ON (t.UserName = s.UserName)
WHEN MATCHED THEN
  UPDATE SET
    UserType=@UserType,
    LastSeenDT=SYSDATETIME(),
    UpdateDT=SYSDATETIME()
WHEN NOT MATCHED THEN
  INSERT (UserName, UserType, LastSeenDT, UpdateDT)
  VALUES (@UserName, @UserType, SYSDATETIME(), SYSDATETIME());";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = userName;
        cmd.Parameters.Add("@UserType", SqlDbType.Char, 1).Value = userType;
        cmd.ExecuteNonQuery();
    }

    private void UpsertByIpKey(SqlConnection conn, string ipKey, char userType)
    {
        const string sql = @"
MERGE dbo.UserUiPreference WITH (HOLDLOCK) AS t
USING (SELECT @IPKey AS IPKey) AS s
ON (t.IPKey = s.IPKey)
WHEN MATCHED THEN
  UPDATE SET
    UserType=@UserType,
    LastSeenDT=SYSDATETIME(),
    UpdateDT=SYSDATETIME()
WHEN NOT MATCHED THEN
  INSERT (IPKey, UserType, LastSeenDT, UpdateDT)
  VALUES (@IPKey, @UserType, SYSDATETIME(), SYSDATETIME());";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@IPKey", SqlDbType.NVarChar, 64).Value = ipKey;
        cmd.Parameters.Add("@UserType", SqlDbType.Char, 1).Value = userType;
        cmd.ExecuteNonQuery();
    }
}

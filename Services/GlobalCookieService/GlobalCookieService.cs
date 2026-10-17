using System.Data;
using System.Data.SqlClient;

public class GlobalCookieService : IGlobalCookieService
{
    private readonly string _connectionString;

    public GlobalCookieService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    /// <summary>
    /// 클라이언트(한국 사용자) 기준 시간 = 한국 표준시(KST)
    /// </summary>
    private static DateTime GetKoreaNow()
    {
        // SmarterASP.NET → Windows 서버 기준
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    }

    private string GetClientKey(HttpContext httpContext)
    {
        // ✅ 로그인한 사람은 계정 기준으로 구분한다 (같은 와이파이를 써도, PC 가 달라도 그 사람의 숨김 설정은 한 벌)
        //    로그인 전(또는 계정 이름이 없을 때)만 예전처럼 접속 주소(IP)로 구분한다.
        var name = httpContext.User?.Identity?.IsAuthenticated == true
            ? httpContext.User.Identity.Name
            : null;
        if (!string.IsNullOrWhiteSpace(name))
            return "u:" + name.Trim();

        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
        return ip;
    }

    /// <summary>
    /// 배너를 보여줄지 여부 판단 (KST 기준)
    /// </summary>
    public async Task<bool> ShouldShowAsync(string cookieKey, HttpContext httpContext)
    {
        var clientKey = GetClientKey(httpContext);

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = conn.CreateCommand())
        {
            await conn.OpenAsync();

            var sql = @"
SELECT HideUntil
FROM dbo.GlobalClientCookie WITH (NOLOCK)
WHERE CookieKey = @CookieKey
  AND ClientKey = @ClientKey;
";
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@CookieKey", cookieKey);
            cmd.Parameters.AddWithValue("@ClientKey", clientKey);

            var result = await cmd.ExecuteScalarAsync();

            // 기록이 없으면 무조건 보여줌
            if (result == null || result == DBNull.Value)
                return true;

            var hideUntil = (DateTime)result;

            // ✅ KST 기준 현재 시간
            var now = GetKoreaNow();

            // hideUntil이 지났으면 다시 보여줌
            return now > hideUntil;
        }
    }

    /// <summary>
    /// 숨김 설정 (KST 기준으로 계산)
    /// </summary>
    public async Task SetHideAsync(string cookieKey, string durationCode, HttpContext httpContext)
    {
        var clientKey = GetClientKey(httpContext);

        // ✅ 기준 시간: 한국 표준시
        var now = GetKoreaNow();
        DateTime? hideUntil = null;

        switch (durationCode)
        {
            case "today":
                // 오늘 끝날 때까지 → 다음날 00:00 (KST)
                hideUntil = now.Date.AddDays(1);
                break;
            case "3day":
                hideUntil = now.AddDays(3);
                break;

            case "7day":
            case "1week":
                hideUntil = now.AddDays(7);
                break;

            case "3week":
                hideUntil = now.AddDays(21);
                break;
            case "99year":
                hideUntil = now.AddYears(99);
                break;

            default:
                hideUntil = null; // 잘못된 코드면 숨김 해제
                break;
        }

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = conn.CreateCommand())
        {
            await conn.OpenAsync();

            // ✅ SYSUTCDATETIME 제거 → 파라미터 사용
            var sql = @"
IF EXISTS (
    SELECT 1
    FROM dbo.GlobalClientCookie WITH (UPDLOCK, HOLDLOCK)
    WHERE CookieKey = @CookieKey
      AND ClientKey = @ClientKey
)
BEGIN
    UPDATE dbo.GlobalClientCookie
    SET HideUntil = @HideUntil,
        UpdatedAt = @Now
    WHERE CookieKey = @CookieKey
      AND ClientKey = @ClientKey;
END
ELSE
BEGIN
    INSERT INTO dbo.GlobalClientCookie
        (CookieKey, ClientKey, HideUntil, CreatedAt)
    VALUES
        (@CookieKey, @ClientKey, @HideUntil, @Now);
END
";

            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.AddWithValue("@CookieKey", cookieKey);
            cmd.Parameters.AddWithValue("@ClientKey", clientKey);
            cmd.Parameters.AddWithValue("@HideUntil", (object?)hideUntil ?? DBNull.Value);

            // ✅ KST 기준 현재 시간 저장
            cmd.Parameters.AddWithValue("@Now", now);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}

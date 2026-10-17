using BulletinBoard.DataAccessLayer;
using BulletinBoard.Helpers;
using BulletinBoard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

public class UserSettingsService : IUserSettingsService
{
    private readonly string _connectionString;

    public UserSettingsService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<string> GetLangAsync(string userId)
    {
        const string sql = @"
SELECT LangCD
FROM dbo.UserSettings
WHERE Id = @Id";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", userId);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();

        return result?.ToString() ?? "ko"; // ✅ 기본값
    }

    public async Task SaveLangAsync(string userId, string lang)
    {
        const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.UserSettings WHERE Id = @Id)
BEGIN
    UPDATE dbo.UserSettings
       SET LangCD = @Lang,
           updateDT = GETDATE()
     WHERE Id = @Id
END
ELSE
BEGIN
    INSERT INTO dbo.UserSettings (Id, LangCD)
    VALUES (@Id, @Lang)
END";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@Id", userId);
        cmd.Parameters.AddWithValue("@Lang", lang);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }


    public async Task<bool> IsCookieSuppressedAsync(string userId, string cookieName)
    {
        if (string.IsNullOrWhiteSpace(userId)) return false;
        if (string.IsNullOrWhiteSpace(cookieName)) return false;

        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
SELECT TOP 1 cookieDays, updateDT
FROM dbo.UserSettings_cookieName
WHERE Id = @Id AND cookieName = @cookieName
ORDER BY updateDT DESC;
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar, 450) { Value = userId });
            cmd.Parameters.Add(new SqlParameter("@cookieName", SqlDbType.NVarChar, 200) { Value = cookieName });

            using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return false;

            int days = (r["cookieDays"] == DBNull.Value) ? 0 : Convert.ToInt32(r["cookieDays"]);
            DateTime dt = (r["updateDT"] == DBNull.Value) ? DateTime.MinValue : Convert.ToDateTime(r["updateDT"]);
            if (days <= 0 || dt == DateTime.MinValue) return false;

            // updateDT 기준으로 cookieDays 동안 suppress
            var until = dt.AddDays(days);
            var now = DateHelper.GetKoreaNow(); // 이미 프로젝트에 사용중
            return now < until;
        }
        catch
        {
            return false;
        }
    }

    public async Task UpsertCookieSuppressionAsync(string userId, string cookieName, string cookieValue, int cookieDays)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;
        if (string.IsNullOrWhiteSpace(cookieName)) return;
        if (cookieDays <= 0) cookieDays = 1;

        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
IF EXISTS (SELECT 1 FROM dbo.UserSettings_cookieName WHERE Id=@Id AND cookieName=@cookieName)
BEGIN
    UPDATE dbo.UserSettings_cookieName
    SET cookieValue=@cookieValue,
        cookieDays=@cookieDays,
        updateDT=GETDATE()
    WHERE Id=@Id AND cookieName=@cookieName;
END
ELSE
BEGIN
    INSERT INTO dbo.UserSettings_cookieName (Id, cookieName, cookieValue, cookieDays, InsertDT, updateDT)
    VALUES (@Id, @cookieName, @cookieValue, @cookieDays, GETDATE(), GETDATE());
END
";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar, 450) { Value = userId });
            cmd.Parameters.Add(new SqlParameter("@cookieName", SqlDbType.NVarChar, 200) { Value = cookieName });
            cmd.Parameters.Add(new SqlParameter("@cookieValue", SqlDbType.NVarChar, 200) { Value = cookieValue ?? "" });
            cmd.Parameters.Add(new SqlParameter("@cookieDays", SqlDbType.Int) { Value = cookieDays });

            await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // suppress 저장 실패해도 화면 흐름은 깨지지 않게
        }
    }


}

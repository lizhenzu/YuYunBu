using BulletinBoard.Areas.Hanmadi.Data;
using BulletinBoard.Areas.Hanmadi.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace BulletinBoard.Areas.Hanmadi.Pages;

public class JoinModel : PageModel
{
    private readonly HanmadiDbContext _db;
    public JoinModel(HanmadiDbContext db) => _db = db;

    [BindProperty, Required(ErrorMessage = "참여 코드를 입력하세요."), StringLength(3, MinimumLength = 3)]
    public string Code { get; set; } = "";

    [BindProperty, StringLength(20)]
    public string? Nickname { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var code = Code.ToUpper();
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Code == code);
        if (session == null)
        {
            ErrorMessage = "코드를 다시 확인해주세요.";
            return Page();
        }
        if (session.Status == 2)
        {
            ErrorMessage = "이미 종료된 세션입니다.";
            return Page();
        }

        var ip = GetClientIp();

        // 같은 세션+IP면 기존 토큰 재사용
        var existing = await _db.Participants
            .FirstOrDefaultAsync(p => p.SessionId == session.Id && p.IpAddress == ip);

        string token;
        if (existing != null)
        {
            // 기존 참여자 — 토큰 쿠키 재설정 후 안내 페이지로
            Response.Cookies.Append($"hm_token_{code}", existing.Token, new CookieOptions
            {
                Expires  = DateTimeOffset.UtcNow.AddHours(4),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
            });
            return RedirectToPage("/Join", new { rejoined = code });
        }
        else
        {
            token = GenerateToken();
            _db.Participants.Add(new HmParticipant
            {
                SessionId  = session.Id,
                Token      = token,
                IpAddress  = ip,
                Nickname   = string.IsNullOrWhiteSpace(Nickname) ? null : Nickname.Trim(),
                JoinedAt   = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
        }

        Response.Cookies.Append($"hm_token_{code}", token, new CookieOptions
        {
            Expires  = DateTimeOffset.UtcNow.AddHours(4),
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
        });

        return RedirectToPage("/Play", new { area = "Hanmadi", code });
    }

    private string GetClientIp()
    {
        var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string GenerateToken()
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLower();
    }
}

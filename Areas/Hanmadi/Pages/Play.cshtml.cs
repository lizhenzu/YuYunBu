using BulletinBoard.Areas.Hanmadi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Areas.Hanmadi.Pages;

public class PlayModel : PageModel
{
    private readonly HanmadiDbContext _db;
    public PlayModel(HanmadiDbContext db) => _db = db;

    public string Code { get; set; } = "";
    public bool HasActiveQuestion { get; set; }

    public async Task<IActionResult> OnGetAsync(string code)
    {
        Code = code.ToUpper();
        var session = await _db.Sessions
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(s => s.Code == Code);

        if (session == null) return RedirectToPage("/Join");
        if (session.Status == 2)
            return RedirectToPage("/Join");

        // 토큰 쿠키 확인 — 없거나 유효하지 않으면 참여 페이지로
        var cookieToken = Request.Cookies[$"hm_token_{Code}"];
        if (string.IsNullOrEmpty(cookieToken))
            return RedirectToPage("/Join", new { blocked = true });

        var participant = await _db.Participants
            .FirstOrDefaultAsync(p => p.SessionId == session.Id && p.Token == cookieToken);
        if (participant == null)
            return RedirectToPage("/Join", new { blocked = true });

        HasActiveQuestion = session.ActiveQuestionId.HasValue
            && session.Questions.Any(q => q.Id == session.ActiveQuestionId.Value && q.Status == 1);

        return Page();
    }
}

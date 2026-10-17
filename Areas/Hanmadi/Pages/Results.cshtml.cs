using BulletinBoard.Areas.Hanmadi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Areas.Hanmadi.Pages;

public class ResultsModel : PageModel
{
    private readonly HanmadiDbContext _db;
    public ResultsModel(HanmadiDbContext db) => _db = db;

    public string Code { get; set; } = "";
    public string SessionTitle { get; set; } = "";
    public int SessionId { get; set; }

    public async Task<IActionResult> OnGetAsync(string code)
    {
        Code = code.ToUpper();
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Code == Code);
        if (session == null) return RedirectToPage("/Join");

        // 참여자 토큰 확인
        var cookieToken = Request.Cookies[$"hm_token_{Code}"];
        if (string.IsNullOrEmpty(cookieToken)) return RedirectToPage("/Join", new { blocked = true });

        var participant = await _db.Participants
            .FirstOrDefaultAsync(p => p.SessionId == session.Id && p.Token == cookieToken);
        if (participant == null) return RedirectToPage("/Join", new { blocked = true });

        SessionTitle = session.Title;
        SessionId    = session.Id;
        return Page();
    }
}

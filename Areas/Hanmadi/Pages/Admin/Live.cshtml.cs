using BulletinBoard.Areas.Hanmadi.Data;
using BulletinBoard.Areas.Hanmadi.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Areas.Hanmadi.Pages.Admin;

[Authorize]
public class LiveModel : PageModel
{
    private readonly HanmadiDbContext _db;
    public LiveModel(HanmadiDbContext db) => _db = db;

    public HmSession? Session { get; set; }
    public int ParticipantCount { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Session = await _db.Sessions
            .Include(s => s.Questions.OrderBy(q => q.Order))
            .FirstOrDefaultAsync(s => s.Id == id);

        if (Session == null) return NotFound();

        ParticipantCount = await _db.Participants.CountAsync(p => p.SessionId == id);
        return Page();
    }
}

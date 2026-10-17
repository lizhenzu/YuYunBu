using BulletinBoard.Areas.Hanmadi.Data;
using BulletinBoard.Areas.Hanmadi.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Areas.Hanmadi.Pages.Admin;

[Authorize]
public class ParticipantsModel : PageModel
{
    private readonly HanmadiDbContext _db;
    public ParticipantsModel(HanmadiDbContext db) => _db = db;

    public HmSession? Session { get; set; }
    public List<HmParticipant> Participants { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == id);
        if (Session == null) return NotFound();

        Participants = await _db.Participants
            .Where(p => p.SessionId == id)
            .OrderBy(p => p.Nickname == null ? 1 : 0)
            .ThenBy(p => p.JoinedAt)
            .ToListAsync();

        return Page();
    }
}

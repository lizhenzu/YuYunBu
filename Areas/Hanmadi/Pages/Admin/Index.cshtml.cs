using BulletinBoard.Areas.Hanmadi.Data;
using BulletinBoard.Areas.Hanmadi.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Areas.Hanmadi.Pages.Admin;

[Authorize]
public class IndexModel : PageModel
{
    private readonly HanmadiDbContext _db;
    public IndexModel(HanmadiDbContext db) => _db = db;

    public List<HmSession> Sessions { get; set; } = new();

    public async Task OnGetAsync()
    {
        Sessions = await _db.Sessions
            .Include(s => s.Questions)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteSessionAsync(int sessionId)
    {
        // FK 순서: AnswerTokens → Answers → Participants → QuestionOptions → Questions → Session
        var tokenIds = await _db.AnswerTokens
            .Where(t => _db.Questions.Any(q => q.SessionId == sessionId && q.Id == t.QuestionId))
            .ToListAsync();
        _db.AnswerTokens.RemoveRange(tokenIds);

        var answers = await _db.Answers
            .Where(a => _db.Questions.Any(q => q.SessionId == sessionId && q.Id == a.QuestionId))
            .ToListAsync();
        _db.Answers.RemoveRange(answers);

        var participants = await _db.Participants.Where(p => p.SessionId == sessionId).ToListAsync();
        _db.Participants.RemoveRange(participants);

        var options = await _db.QuestionOptions
            .Where(o => _db.Questions.Any(q => q.SessionId == sessionId && q.Id == o.QuestionId))
            .ToListAsync();
        _db.QuestionOptions.RemoveRange(options);

        var questions = await _db.Questions.Where(q => q.SessionId == sessionId).ToListAsync();
        _db.Questions.RemoveRange(questions);

        var session = await _db.Sessions.FindAsync(sessionId);
        if (session != null) _db.Sessions.Remove(session);

        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}

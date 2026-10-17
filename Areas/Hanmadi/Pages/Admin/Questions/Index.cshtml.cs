using BulletinBoard.Areas.Hanmadi.Data;
using BulletinBoard.Areas.Hanmadi.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Areas.Hanmadi.Pages.Admin.Questions;

[Authorize]
public class IndexModel : PageModel
{
    private readonly HanmadiDbContext _db;
    public IndexModel(HanmadiDbContext db) => _db = db;

    public HmSession? Session { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Session = await _db.Sessions
            .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (Session == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAddQuestionAsync(int sessionId, int type, string title, List<string>? options)
    {
        if (string.IsNullOrWhiteSpace(title))
            return RedirectToPage(new { id = sessionId });

        var maxOrder = _db.Questions
            .Where(q => q.SessionId == sessionId)
            .Select(q => (int?)q.Order)
            .Max() ?? 0;

        var question = new HmQuestion
        {
            SessionId = sessionId,
            Type      = type,
            Title     = title.Trim(),
            Order     = maxOrder + 1,
            Status    = 0,
        };
        _db.Questions.Add(question);
        await _db.SaveChangesAsync();

        // 객관식 선택지 저장
        if (type == 2 && options != null)
        {
            int order = 1;
            foreach (var opt in options.Where(o => !string.IsNullOrWhiteSpace(o)))
            {
                _db.QuestionOptions.Add(new HmQuestionOption
                {
                    QuestionId = question.Id,
                    Text       = opt.Trim(),
                    Order      = order++,
                });
            }
            await _db.SaveChangesAsync();
        }

        return RedirectToPage(new { id = sessionId });
    }

    public async Task<IActionResult> OnPostEditQuestionAsync(int questionId, string title, List<string>? options)
    {
        var q = await _db.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId);
        if (q == null) return NotFound();

        q.Title = title.Trim();

        if (q.Type == 2 && options != null)
        {
            _db.QuestionOptions.RemoveRange(q.Options);
            int order = 1;
            foreach (var opt in options.Where(o => !string.IsNullOrWhiteSpace(o)))
            {
                _db.QuestionOptions.Add(new HmQuestionOption
                {
                    QuestionId = questionId,
                    Text       = opt.Trim(),
                    Order      = order++,
                });
            }
        }

        await _db.SaveChangesAsync();
        return RedirectToPage(new { id = q.SessionId });
    }

    public async Task<IActionResult> OnPostDeleteQuestionAsync(int questionId)
    {
        var q = await _db.Questions.FindAsync(questionId);
        if (q != null)
        {
            // FK 순서: AnswerTokens → Answers → Question
            var tokens = _db.AnswerTokens.Where(t => t.QuestionId == questionId);
            _db.AnswerTokens.RemoveRange(tokens);

            var answers = _db.Answers.Where(a => a.QuestionId == questionId);
            _db.Answers.RemoveRange(answers);

            _db.Questions.Remove(q);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id = q?.SessionId ?? 0 });
    }
}

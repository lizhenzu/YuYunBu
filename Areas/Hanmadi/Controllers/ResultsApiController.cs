using BulletinBoard.Areas.Hanmadi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Areas.Hanmadi.Controllers;

[Area("Hanmadi")]
[Route("hanmadi/api/results")]
[ApiController]
public class ResultsApiController : ControllerBase
{
    private readonly HanmadiDbContext _db;
    public ResultsApiController(HanmadiDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int questionId)
    {
        var q = await _db.Questions.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == questionId);
        if (q == null) return NotFound();

        var totalCount = await _db.Answers.CountAsync(a => a.QuestionId == questionId);

        if (q.Type == 1)
        {
            var bubbles = await _db.AnswerTokens
                .Where(t => t.QuestionId == questionId)
                .GroupBy(t => t.NormalizedText)
                .Select(g => new { text = g.First().DisplayText, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(60)
                .ToListAsync();
            return Ok(new { bubbles, totalCount });
        }

        if (q.Type == 2)
        {
            var answers = await _db.Answers
                .Where(a => a.QuestionId == questionId && a.OptionIds != null)
                .Select(a => a.OptionIds!)
                .ToListAsync();

            var counts = q.Options.ToDictionary(o => o.Id, o => 0);
            foreach (var json in answers)
            {
                try
                {
                    var ids = System.Text.Json.JsonSerializer.Deserialize<List<int>>(json);
                    if (ids != null) foreach (var id in ids) if (counts.ContainsKey(id)) counts[id]++;
                }
                catch { }
            }

            var bars = q.Options.OrderBy(o => o.Order).Select(o => new { text = o.Text, count = counts[o.Id] }).ToList();
            return Ok(new { bars, totalCount });
        }

        if (q.Type == 3)
        {
            var scale = await _db.Answers
                .Where(a => a.QuestionId == questionId && a.ScaleValue.HasValue)
                .GroupBy(a => a.ScaleValue!.Value)
                .Select(g => new { val = g.Key, count = g.Count() })
                .ToListAsync();
            var scaleDict = scale.ToDictionary(x => x.val, x => x.count);
            return Ok(new { scale = scaleDict, totalCount });
        }

        // Type == 4: 자유서술
        var cards = await _db.Answers
            .Where(a => a.QuestionId == questionId && !a.IsRemoved && a.RawText != null)
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => a.RawText!)
            .Take(100)
            .ToListAsync();
        return Ok(new { cards, totalCount });
    }
}

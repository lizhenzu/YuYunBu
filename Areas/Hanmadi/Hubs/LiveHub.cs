using BulletinBoard.Areas.Hanmadi.Data;
using BulletinBoard.Areas.Hanmadi.Data.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Areas.Hanmadi.Hubs;

public class LiveHub : Hub
{
    private readonly HanmadiDbContext _db;
    public LiveHub(HanmadiDbContext db) => _db = db;

    // 참여자: 세션 참여 (코드로 그룹 조인)
    public async Task JoinSession(string code, string token)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Code == code);
        if (session == null) { await Clients.Caller.SendAsync("Error", "세션을 찾을 수 없습니다."); return; }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupKey(session.Id));
        await Groups.AddToGroupAsync(Context.ConnectionId, AdminKey(session.Id));

        // 현재 열린 문항이 있으면 즉시 전송
        if (session.ActiveQuestionId.HasValue)
        {
            var q = await _db.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == session.ActiveQuestionId.Value);
            if (q != null)
                await Clients.Caller.SendAsync("QuestionOpened", QuestionDto(q));
        }

        // 참여자 수 브로드캐스트
        var count = await _db.Participants.CountAsync(p => p.SessionId == session.Id);
        await Clients.Group(AdminKey(session.Id)).SendAsync("ParticipantCount", count);
    }

    // 관리자: 문항 오픈
    public async Task OpenQuestion(int questionId)
    {
        var q = await _db.Questions.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == questionId);
        if (q == null) return;

        // 이전 문항 마감
        var prev = await _db.Questions.FirstOrDefaultAsync(x => x.SessionId == q.SessionId && x.Status == 1);
        if (prev != null) { prev.Status = 2; prev.ClosedAt = DateTime.UtcNow; }

        q.Status = 1;
        q.OpenedAt = DateTime.UtcNow;

        var session = await _db.Sessions.FindAsync(q.SessionId);
        if (session != null) session.ActiveQuestionId = questionId;

        await _db.SaveChangesAsync();
        await Clients.Group(GroupKey(q.SessionId)).SendAsync("QuestionOpened", QuestionDto(q));
    }

    // 관리자: 문항 마감
    public async Task CloseQuestion(int questionId)
    {
        var q = await _db.Questions.FindAsync(questionId);
        if (q == null) return;

        q.Status = 2;
        q.ClosedAt = DateTime.UtcNow;

        var session = await _db.Sessions.FindAsync(q.SessionId);
        if (session != null && session.ActiveQuestionId == questionId)
            session.ActiveQuestionId = null;

        await _db.SaveChangesAsync();
        await Clients.Group(GroupKey(q.SessionId)).SendAsync("QuestionClosed", questionId);
    }

    // 참여자: 답변 제출
    public async Task SubmitAnswer(int questionId, string token, string rawText, int? scaleValue, string? optionIds)
    {
        var q = await _db.Questions.FindAsync(questionId);
        if (q == null || q.Status != 1) { await Clients.Caller.SendAsync("Error", "답변을 제출할 수 없습니다."); return; }

        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.Token == token && p.SessionId == q.SessionId);
        if (participant == null) { await Clients.Caller.SendAsync("Error", "참여자 정보가 없습니다."); return; }

        // 중복 제출 방지
        var existing = await _db.Answers.FirstOrDefaultAsync(a => a.QuestionId == questionId && a.ParticipantId == participant.Id);
        if (existing != null) { await Clients.Caller.SendAsync("Error", "이미 제출했습니다."); return; }

        var answer = new HmAnswer
        {
            QuestionId    = questionId,
            ParticipantId = participant.Id,
            RawText       = rawText?.Trim(),
            ScaleValue    = scaleValue,
            OptionIds     = optionIds,
            SubmittedAt   = DateTime.UtcNow,
        };
        _db.Answers.Add(answer);
        await _db.SaveChangesAsync();

        // 단답형: 입력값 전체를 버블 하나로 저장
        if (q.Type == 1 && !string.IsNullOrWhiteSpace(rawText))
        {
            var display = rawText.Trim();
            _db.AnswerTokens.Add(new HmAnswerToken
            {
                AnswerId       = answer.Id,
                QuestionId     = questionId,
                NormalizedText = display.ToLower(),
                DisplayText    = display,
            });
            await _db.SaveChangesAsync();

            // 집계 후 브로드캐스트
            var bubbles = await _db.AnswerTokens
                .Where(t => t.QuestionId == questionId)
                .GroupBy(t => t.NormalizedText)
                .Select(g => new { text = g.First().DisplayText, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(50)
                .ToListAsync();
            await Clients.Group(AdminKey(q.SessionId)).SendAsync("BubbleUpdate", bubbles);
        }

        // 제출 수 브로드캐스트
        var submittedCount = await _db.Answers.CountAsync(a => a.QuestionId == questionId);
        await Clients.Group(AdminKey(q.SessionId)).SendAsync("SubmitCount", submittedCount);

        // 객관식/척도/자유서술: 관리자에게 재조회 신호
        if (q.Type != 1)
            await Clients.Group(AdminKey(q.SessionId)).SendAsync("ResultUpdate", new { questionId });

        await Clients.Caller.SendAsync("AnswerAccepted");
    }

    // 관리자: 세션 종료
    public async Task EndSession(int sessionId)
    {
        var session = await _db.Sessions.FindAsync(sessionId);
        if (session == null) return;

        // 열린 문항 마감
        var openQ = await _db.Questions.FirstOrDefaultAsync(q => q.SessionId == sessionId && q.Status == 1);
        if (openQ != null) { openQ.Status = 2; openQ.ClosedAt = DateTime.UtcNow; }

        session.Status          = 2;
        session.ActiveQuestionId = null;
        session.ClosedAt        = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await Clients.Group(GroupKey(sessionId)).SendAsync("SessionEnded");
    }

    private static string GroupKey(int sessionId) => $"session_{sessionId}";
    private static string AdminKey(int sessionId) => $"admin_{sessionId}";

    private static object QuestionDto(HmQuestion q) => new
    {
        id      = q.Id,
        type    = q.Type,
        title   = q.Title,
        options = q.Options.OrderBy(o => o.Order).Select(o => new { o.Id, o.Text }),
    };

}

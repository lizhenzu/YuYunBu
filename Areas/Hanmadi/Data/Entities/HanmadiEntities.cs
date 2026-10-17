using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulletinBoard.Areas.Hanmadi.Data.Entities;

// ── 세션 ──────────────────────────────────────────────
public class HmSession
{
    public int Id { get; set; }

    [Required, StringLength(3)]
    public string Code { get; set; } = "";          // 3자리 참여코드

    [Required, StringLength(100)]
    public string Title { get; set; } = "";

    public string? OwnerUserId { get; set; }        // AspNetUsers.Id

    public int Status { get; set; }                 // 0=대기 1=진행중 2=종료
    public int? ActiveQuestionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public ICollection<HmQuestion> Questions { get; set; } = new List<HmQuestion>();
    public ICollection<HmParticipant> Participants { get; set; } = new List<HmParticipant>();
}

// ── 문항 ──────────────────────────────────────────────
public class HmQuestion
{
    public int Id { get; set; }
    public int SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public HmSession Session { get; set; } = null!;

    public int Order { get; set; }

    // 1=단답형(버블) 2=객관식(막대) 3=척도(게이지) 4=자유서술(카드)
    public int Type { get; set; }

    [Required, StringLength(2000)]
    public string Title { get; set; } = "";

    public string? ConfigJson { get; set; }         // 유형별 설정 JSON

    public int Status { get; set; }                 // 0=대기 1=열림 2=마감
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public ICollection<HmQuestionOption> Options { get; set; } = new List<HmQuestionOption>();
    public ICollection<HmAnswer> Answers { get; set; } = new List<HmAnswer>();
}

// ── 객관식 선택지 ──────────────────────────────────────
public class HmQuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public HmQuestion Question { get; set; } = null!;

    public int Order { get; set; }

    [Required, StringLength(200)]
    public string Text { get; set; } = "";

    public bool IsCorrect { get; set; }
    public string? ImageUrl { get; set; }
}

// ── 참여자 ────────────────────────────────────────────
public class HmParticipant
{
    public int Id { get; set; }
    public int SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public HmSession Session { get; set; } = null!;

    [Required, StringLength(64)]
    public string Token { get; set; } = "";         // 익명 참여 토큰

    [StringLength(30)]
    public string? Nickname { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeenAt { get; set; }
    public bool IsBlocked { get; set; }
}

// ── 답변 ──────────────────────────────────────────────
public class HmAnswer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public HmQuestion Question { get; set; } = null!;

    public int ParticipantId { get; set; }

    [ForeignKey(nameof(ParticipantId))]
    public HmParticipant Participant { get; set; } = null!;

    public string? RawText { get; set; }            // 단답형·자유서술
    public string? OptionIds { get; set; }          // 객관식 (JSON 배열)
    public int? ScaleValue { get; set; }            // 척도
    public bool IsRemoved { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HmAnswerToken> Tokens { get; set; } = new List<HmAnswerToken>();
}

// ── 정규화 토큰 (버블 집계용) ─────────────────────────
public class HmAnswerToken
{
    public int Id { get; set; }
    public int AnswerId { get; set; }

    [ForeignKey(nameof(AnswerId))]
    public HmAnswer Answer { get; set; } = null!;

    public int QuestionId { get; set; }

    [Required, StringLength(100)]
    public string NormalizedText { get; set; } = "";

    [Required, StringLength(100)]
    public string DisplayText { get; set; } = "";   // 최초 또는 최다 빈도 원문
}

// ── 동의어 사전 ───────────────────────────────────────
public class HmWordAlias
{
    public int Id { get; set; }
    public int? SessionId { get; set; }             // NULL = 전역

    [Required, StringLength(100)]
    public string FromText { get; set; } = "";

    [Required, StringLength(100)]
    public string ToText { get; set; } = "";
}

// ── 금칙어 ────────────────────────────────────────────
public class HmBannedWord
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Word { get; set; } = "";

    public int Severity { get; set; }               // 1=마스킹 2=차단
}

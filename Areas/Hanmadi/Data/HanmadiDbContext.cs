using BulletinBoard.Areas.Hanmadi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Areas.Hanmadi.Data;

public class HanmadiDbContext : DbContext
{
    public HanmadiDbContext(DbContextOptions<HanmadiDbContext> options) : base(options) { }

    public DbSet<HmSession>        Sessions        => Set<HmSession>();
    public DbSet<HmQuestion>       Questions       => Set<HmQuestion>();
    public DbSet<HmQuestionOption> QuestionOptions => Set<HmQuestionOption>();
    public DbSet<HmParticipant>    Participants    => Set<HmParticipant>();
    public DbSet<HmAnswer>         Answers         => Set<HmAnswer>();
    public DbSet<HmAnswerToken>    AnswerTokens    => Set<HmAnswerToken>();
    public DbSet<HmWordAlias>      WordAliases     => Set<HmWordAlias>();
    public DbSet<HmBannedWord>     BannedWords     => Set<HmBannedWord>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // 세션 코드 유니크
        b.Entity<HmSession>()
            .HasIndex(s => s.Code).IsUnique();

        // 참여자 토큰 유니크
        b.Entity<HmParticipant>()
            .HasIndex(p => p.Token).IsUnique();

        // 답변: 문항당 참여자 1개 (UNIQUE)
        b.Entity<HmAnswer>()
            .HasIndex(a => new { a.QuestionId, a.ParticipantId }).IsUnique();

        // 정규화 토큰 인덱스
        b.Entity<HmAnswerToken>()
            .HasIndex(t => new { t.QuestionId, t.NormalizedText });

        // 순환 cascade 방지: Answers FK를 NoAction으로
        b.Entity<HmAnswer>()
            .HasOne(a => a.Question).WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);

        b.Entity<HmAnswer>()
            .HasOne(a => a.Participant).WithMany()
            .HasForeignKey(a => a.ParticipantId)
            .OnDelete(DeleteBehavior.NoAction);

        b.Entity<HmAnswerToken>()
            .HasOne(t => t.Answer).WithMany(a => a.Tokens)
            .HasForeignKey(t => t.AnswerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

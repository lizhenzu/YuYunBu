using BibleReadingThrough.Models;
using Microsoft.EntityFrameworkCore;

namespace BibleReadingThrough.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<EBible>           EBibles              { get; set; }
    public DbSet<EBibleEn>        EBibleEns            { get; set; }  // 영문 성경 (eBibleEN)
    public DbSet<EBibleHD>        EBibleHDs            { get; set; }  // 현대인성경 (eBibleHD)
    public DbSet<UserBibleProgress> UserBibleProgresses { get; set; }
    public DbSet<UserBookMark>    UserBookMarks         { get; set; }
    public DbSet<EBibleYouTuBu>   EBibleYouTuBus        { get; set; }
    public DbSet<EBibleSummary>   EBibleSummaries       { get; set; }
    public DbSet<BibleSearchLog>  BibleSearchLogs       { get; set; }
    public DbSet<BibleHubClickLog> BibleHubClickLogs    { get; set; }  // 성경통독 허브 버전 선택 클릭
    public DbSet<BulletinBoard.Models.DailyMission2.DM2HubClickLog> DM2HubClickLogs { get; set; }  // 도전! 성경말씀 허브 버전 선택 클릭
    public DbSet<BibleCompletionLog> BibleCompletionLogs { get; set; }  // 성경 완독 기록 (몇 독을 언제)

    // ── 성경통독 완독 선물 ──
    public DbSet<BibleReadingThrough.Models.BibleGift.BibleGiftMilestoneConfig> BibleGiftConfigs   { get; set; }
    public DbSet<BibleReadingThrough.Models.BibleGift.BibleGiftOrder>           BibleGiftOrders    { get; set; }
    public DbSet<BibleReadingThrough.Models.BibleGift.BibleGiftOrderLog>        BibleGiftOrderLogs { get; set; }
    public DbSet<BulletinBoard.Models.AwardItem>                                AwardItems         { get; set; }  // 선물 품목 (기존 표)
    public DbSet<BibleReadingThrough.Models.BibleGift.UserCompositeInfo>        UserComposites     { get; set; }  // 이름·역할 (뷰)

    // 추석맞이 우리 가족 사진 콘테스트
    public DbSet<BulletinBoard.Models.FamilyPhoto.FamilyPhotoEntry>        FamilyPhotoEntries { get; set; }
    public DbSet<BulletinBoard.Models.FamilyPhoto.FamilyPhotoItem>         FamilyPhotoItems   { get; set; }
    public DbSet<BulletinBoard.Models.FamilyPhoto.FamilyPhotoLike>         FamilyPhotoLikes   { get; set; }
    public DbSet<BulletinBoard.Models.FamilyPhoto.FamilyPhotoContestState> FamilyPhotoStates  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EBible>(entity =>
        {
            entity.ToTable("eBible");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UpdateDT).HasColumnName("updateDT");
            entity.Property(e => e.Zul).HasColumnName("zul");
        });

        modelBuilder.Entity<EBibleEn>(entity =>
        {
            entity.ToTable("eBibleEN");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UpdateDT).HasColumnName("updateDT");
            entity.Property(e => e.Zul).HasColumnName("zul");
        });

        modelBuilder.Entity<EBibleHD>(entity =>
        {
            entity.ToTable("eBibleHD");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UpdateDT).HasColumnName("updateDT");
            entity.Property(e => e.Zul).HasColumnName("zul");
        });

        modelBuilder.Entity<UserBibleProgress>(entity =>
        {
            entity.ToTable("UserBibleProgress");
            entity.HasKey(e => new { e.UserId, e.VerseId });
            entity.Property(e => e.UserId).HasMaxLength(450);
        });

        modelBuilder.Entity<UserBookMark>(entity =>
        {
            entity.ToTable("UserBookMark");
            entity.HasKey(e => new { e.UserId, e.VerseId });
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.InsertDT).HasColumnName("InsertDT");
        });

        modelBuilder.Entity<EBibleYouTuBu>(entity =>
        {
            entity.ToTable("eBibleYouTuBu");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UpdateDT).HasColumnName("updateDT");
        });

        modelBuilder.Entity<EBibleSummary>(entity =>
        {
            entity.ToTable("eBibleSummary");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UpdateDT).HasColumnName("updateDT");
        });

        modelBuilder.Entity<BibleSearchLog>(entity =>
        {
            entity.ToTable("BibleSearchLog");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Query).HasMaxLength(200).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(450);
        });

        modelBuilder.Entity<BibleHubClickLog>(entity =>
        {
            entity.ToTable("BibleHubClickLog");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Version).HasMaxLength(10).IsUnicode(false).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<BibleCompletionLog>(entity =>
        {
            entity.ToTable("BibleCompletionLog");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.CompletedAt).HasColumnType("date");
        });

        modelBuilder.Entity<BulletinBoard.Models.DailyMission2.DM2HubClickLog>(entity =>
        {
            entity.ToTable("DM2HubClickLog");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Version).HasMaxLength(10).IsUnicode(false).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        // ── 성경통독 완독 선물 ──
        modelBuilder.Entity<BibleReadingThrough.Models.BibleGift.BibleGiftMilestoneConfig>(entity =>
        {
            entity.ToTable("BibleGiftMilestoneConfig");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CYear).HasColumnName("cYear");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.SubTitle).HasMaxLength(200);
        });

        modelBuilder.Entity<BibleReadingThrough.Models.BibleGift.BibleGiftOrder>(entity =>
        {
            entity.ToTable("BibleGiftOrder");
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.AdminId).HasMaxLength(100);
            entity.Property(e => e.Memo).HasMaxLength(200);
        });

        modelBuilder.Entity<BibleReadingThrough.Models.BibleGift.BibleGiftOrderLog>(entity =>
        {
            entity.ToTable("BibleGiftOrderLog");
            entity.HasKey(e => e.LogId);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.Action).HasMaxLength(20);
            entity.Property(e => e.ActorId).HasMaxLength(256);
            entity.Property(e => e.ActorRole).HasMaxLength(20);
        });

        modelBuilder.Entity<BulletinBoard.Models.AwardItem>(entity =>
        {
            entity.ToTable("AwardItem");
            entity.HasKey(e => e.ProductID);
            entity.Ignore(e => e.PointRequirement);   // 표에 없는 호환용 속성
        });

        modelBuilder.Entity<BibleReadingThrough.Models.BibleGift.UserCompositeInfo>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("view_UserCompositeInfo_2026");
        });

        // ── 추석맞이 우리 가족 사진 콘테스트 ──
        modelBuilder.Entity<BulletinBoard.Models.FamilyPhoto.FamilyPhotoEntry>(entity =>
        {
            entity.ToTable("FamilyPhotoEntry");
            entity.HasKey(e => e.EntryId);
            entity.Property(e => e.FamilyName).HasMaxLength(40).IsRequired();
            entity.Property(e => e.FamilyNameKey).HasMaxLength(40).IsRequired();
            entity.Property(e => e.PinHash).HasMaxLength(128).IsRequired();
            entity.Property(e => e.PinSalt).HasMaxLength(32).IsRequired();
            entity.Property(e => e.HiddenBy).HasMaxLength(100);
            entity.Property(e => e.CreatedIp).HasMaxLength(64);
            entity.HasIndex(e => e.FamilyNameKey).IsUnique();
        });

        modelBuilder.Entity<BulletinBoard.Models.FamilyPhoto.FamilyPhotoItem>(entity =>
        {
            entity.ToTable("FamilyPhotoItem");
            entity.HasKey(e => e.PhotoId);
            entity.Property(e => e.Caption).HasMaxLength(80);
            entity.Property(e => e.FilePath).HasMaxLength(300).IsRequired();
            entity.Property(e => e.ThumbPath).HasMaxLength(300).IsRequired();
            entity.Property(e => e.HiddenBy).HasMaxLength(100);
            entity.HasIndex(e => new { e.EntryId, e.Slot }).IsUnique();
        });

        modelBuilder.Entity<BulletinBoard.Models.FamilyPhoto.FamilyPhotoLike>(entity =>
        {
            entity.ToTable("FamilyPhotoLike");
            entity.HasKey(e => e.LikeId);
            entity.Property(e => e.DeviceKey).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Ip).HasMaxLength(64);
            entity.HasIndex(e => new { e.PhotoId, e.DeviceKey }).IsUnique();
        });

        modelBuilder.Entity<BulletinBoard.Models.FamilyPhoto.FamilyPhotoContestState>(entity =>
        {
            entity.ToTable("FamilyPhotoContest");
            entity.HasKey(e => e.ContestId);
            entity.Property(e => e.ContestId).ValueGeneratedNever();
            entity.Property(e => e.PublishedBy).HasMaxLength(100);
        });
    }
}

using System.Security.Cryptography;
using System.Text;
using BibleReadingThrough.Data;
using BulletinBoard.Helpers;
using BulletinBoard.Models.FamilyPhoto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Cfg = BulletinBoard.Models.FamilyPhoto.FamilyPhotoContestConfig;

namespace BulletinBoard.Services.FamilyPhoto;

public class FamilyPhotoService : IFamilyPhotoService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    private static bool _tablesChecked;
    private static readonly SemaphoreSlim _tableLock = new(1, 1);

    public FamilyPhotoService(AppDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    // ══════════════════════════════════════════════════════════
    //  표 만들기 (Sql/FamilyPhotoContest.sql 과 같은 내용)
    // ══════════════════════════════════════════════════════════
    public async Task EnsureTablesAsync()
    {
        if (_tablesChecked) return;
        await _tableLock.WaitAsync();
        try
        {
            if (_tablesChecked) return;
            await _db.Database.ExecuteSqlRawAsync(Ddl);
            _tablesChecked = true;
        }
        finally { _tableLock.Release(); }
    }

    // Sql/FamilyPhotoContest.sql 과 같은 내용 (배포 파일에 의존하지 않도록 여기 둔다)
    private const string Ddl = @"
IF OBJECT_ID(N'dbo.FamilyPhotoEntry', N'U') IS NULL
CREATE TABLE dbo.FamilyPhotoEntry (
    EntryId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FamilyPhotoEntry PRIMARY KEY,
    FamilyName    NVARCHAR(40)  NOT NULL,
    FamilyNameKey NVARCHAR(40)  NOT NULL,
    PinHash       NVARCHAR(128) NOT NULL,
    PinSalt       NVARCHAR(32)  NOT NULL,
    IsHidden      BIT           NOT NULL CONSTRAINT DF_FamilyPhotoEntry_IsHidden DEFAULT(0),
    HiddenBy      NVARCHAR(100) NULL,
    HiddenAt      DATETIME2     NULL,
    CreatedIp     NVARCHAR(64)  NULL,
    CreatedAt     DATETIME2     NOT NULL,
    UpdatedAt     DATETIME2     NOT NULL
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_FamilyPhotoEntry_NameKey')
    CREATE UNIQUE INDEX UX_FamilyPhotoEntry_NameKey ON dbo.FamilyPhotoEntry (FamilyNameKey);

IF OBJECT_ID(N'dbo.FamilyPhotoItem', N'U') IS NULL
CREATE TABLE dbo.FamilyPhotoItem (
    PhotoId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FamilyPhotoItem PRIMARY KEY,
    EntryId    INT           NOT NULL,
    Slot       INT           NOT NULL,
    Caption    NVARCHAR(80)  NULL,
    FilePath   NVARCHAR(300) NOT NULL,
    ThumbPath  NVARCHAR(300) NOT NULL,
    Width      INT           NOT NULL CONSTRAINT DF_FamilyPhotoItem_W DEFAULT(0),
    Height     INT           NOT NULL CONSTRAINT DF_FamilyPhotoItem_H DEFAULT(0),
    IsHidden   BIT           NOT NULL CONSTRAINT DF_FamilyPhotoItem_IsHidden DEFAULT(0),
    HiddenBy   NVARCHAR(100) NULL,
    HiddenAt   DATETIME2     NULL,
    CreatedAt  DATETIME2     NOT NULL,
    UpdatedAt  DATETIME2     NOT NULL
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_FamilyPhotoItem_EntrySlot')
    CREATE UNIQUE INDEX UX_FamilyPhotoItem_EntrySlot ON dbo.FamilyPhotoItem (EntryId, Slot);

IF OBJECT_ID(N'dbo.FamilyPhotoLike', N'U') IS NULL
CREATE TABLE dbo.FamilyPhotoLike (
    LikeId     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FamilyPhotoLike PRIMARY KEY,
    PhotoId    INT          NOT NULL,
    DeviceKey  NVARCHAR(64) NOT NULL,
    Ip         NVARCHAR(64) NULL,
    CreatedAt  DATETIME2    NOT NULL
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_FamilyPhotoLike_PhotoDevice')
    CREATE UNIQUE INDEX UX_FamilyPhotoLike_PhotoDevice ON dbo.FamilyPhotoLike (PhotoId, DeviceKey);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_FamilyPhotoLike_PhotoIp')
    CREATE INDEX IX_FamilyPhotoLike_PhotoIp ON dbo.FamilyPhotoLike (PhotoId, Ip);

IF OBJECT_ID(N'dbo.FamilyPhotoContest', N'U') IS NULL
CREATE TABLE dbo.FamilyPhotoContest (
    ContestId    INT           NOT NULL CONSTRAINT PK_FamilyPhotoContest PRIMARY KEY,
    IsPublished  BIT           NOT NULL CONSTRAINT DF_FamilyPhotoContest_Pub DEFAULT(0),
    PublishedAt  DATETIME2     NULL,
    PublishedBy  NVARCHAR(100) NULL
);
IF NOT EXISTS (SELECT 1 FROM dbo.FamilyPhotoContest WHERE ContestId = 1)
    INSERT INTO dbo.FamilyPhotoContest (ContestId, IsPublished) VALUES (1, 0);
";

    // ══════════════════════════════════════════════════════════
    //  도우미
    // ══════════════════════════════════════════════════════════
    private static string NameKey(string name)
        => new string((name ?? "").Trim().Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLowerInvariant();

    private static string Hash(string pin, string salt)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salt + ":" + pin));
        return Convert.ToHexString(bytes);
    }

    private static string NewSalt() => Convert.ToHexString(RandomNumberGenerator.GetBytes(8));

    private static bool IsPin(string? pin) => pin != null && pin.Length == 4 && pin.All(char.IsDigit);

    private static string Root(IWebHostEnvironment env) => Path.Combine(env.WebRootPath, "uploads", "familyphoto");

    private async Task<FamilyPhotoContestState> StateAsync()
    {
        var s = await _db.FamilyPhotoStates.FindAsync(1);
        if (s == null)
        {
            s = new FamilyPhotoContestState { ContestId = 1 };
            _db.FamilyPhotoStates.Add(s);
            await _db.SaveChangesAsync();
        }
        return s;
    }

    private static FamilyPhotoPhotoVM ToVM(FamilyPhotoItem p, HashSet<int> mine, Dictionary<int, int>? counts) => new()
    {
        PhotoId   = p.PhotoId,
        Slot      = p.Slot,
        Caption   = p.Caption,
        FilePath  = p.FilePath,
        ThumbPath = p.ThumbPath,
        IsHidden  = p.IsHidden,
        Liked     = mine.Contains(p.PhotoId),
        LikeCount = counts != null && counts.TryGetValue(p.PhotoId, out var c) ? c : 0,
    };

    /// <summary>보이는 가족 + 보이는 사진 (좋아요 수는 withCounts 일 때만)</summary>
    private async Task<List<FamilyPhotoEntryVM>> LoadEntriesAsync(bool includeHidden, string? deviceKey, bool withCounts)
    {
        var entries = await _db.FamilyPhotoEntries.AsNoTracking()
                               .Where(e => includeHidden || !e.IsHidden)
                               .OrderBy(e => e.CreatedAt).ToListAsync();
        var ids = entries.Select(e => e.EntryId).ToList();
        var photos = await _db.FamilyPhotoItems.AsNoTracking()
                              .Where(p => ids.Contains(p.EntryId) && (includeHidden || !p.IsHidden))
                              .OrderBy(p => p.Slot).ToListAsync();
        var photoIds = photos.Select(p => p.PhotoId).ToList();

        var mine = new HashSet<int>();
        if (!string.IsNullOrWhiteSpace(deviceKey))
            mine = (await _db.FamilyPhotoLikes.AsNoTracking()
                             .Where(l => l.DeviceKey == deviceKey && photoIds.Contains(l.PhotoId))
                             .Select(l => l.PhotoId).ToListAsync()).ToHashSet();

        Dictionary<int, int>? counts = null;
        if (withCounts)
            counts = await _db.FamilyPhotoLikes.AsNoTracking()
                              .Where(l => photoIds.Contains(l.PhotoId))
                              .GroupBy(l => l.PhotoId)
                              .Select(g => new { g.Key, N = g.Count() })
                              .ToDictionaryAsync(x => x.Key, x => x.N);

        var byEntry = photos.GroupBy(p => p.EntryId).ToDictionary(g => g.Key, g => g.ToList());
        return entries.Select(e => new FamilyPhotoEntryVM
        {
            EntryId    = e.EntryId,
            FamilyName = e.FamilyName,
            IsHidden   = e.IsHidden,
            CreatedAt  = e.CreatedAt,
            Photos     = byEntry.TryGetValue(e.EntryId, out var ps) ? ps.Select(p => ToVM(p, mine, counts)).ToList() : new(),
        }).ToList();
    }

    // ══════════════════════════════════════════════════════════
    //  보기
    // ══════════════════════════════════════════════════════════
    public async Task<FamilyPhotoGalleryVM> GetGalleryAsync(string deviceKey, bool hasMyEntry, bool isStaff = false)
    {
        var now   = DateHelper.GetKoreaNow();
        var state = await StateAsync();
        var list  = await LoadEntriesAsync(includeHidden: false, deviceKey, withCounts: false);
        list = list.Where(e => e.Photos.Count > 0).ToList();

        // 들어올 때마다 무작위
        var rnd = new Random();
        list = list.OrderBy(_ => rnd.Next()).ToList();

        return new FamilyPhotoGalleryVM
        {
            Phase       = Cfg.PhaseAt(now),
            IsPublished = Cfg.IsAnnouncedAt(now, state.IsPublished),
            CanLike     = Cfg.CanLikeAt(now) || isStaff,   // 관리자는 투표 시작 전에도 누를 수 있다
            CanUpload   = Cfg.CanUploadAt(now),
            EntryCount  = list.Count,
            PhotoCount  = list.Sum(e => e.Photos.Count),
            HasMyEntry  = hasMyEntry,
            Entries     = list,
        };
    }

    // ── 홈 배너 캐시 (2026-09-27) — 홈을 여는 사람마다 DB 를 다시 읽지 않게 1분 동안 둔다.
    //    사진 올리기 · 지우기 · 숨기기 · 발표를 하면 InvalidateBanner() 로 바로 비운다.
    private static FamilyPhotoBannerVM? _bannerCache;
    private static DateTime _bannerAt = DateTime.MinValue;
    private static readonly TimeSpan BannerTtl = TimeSpan.FromMinutes(1);
    public static void InvalidateBanner() => _bannerAt = DateTime.MinValue;

    public async Task<FamilyPhotoBannerVM> GetBannerAsync()
    {
        var now = DateHelper.GetKoreaNow();
        var cached = _bannerCache;
        if (cached != null && now - _bannerAt < BannerTtl && cached.Phase == Cfg.PhaseAt(now))
            return cached;

        var state = await StateAsync();
        // 발표 = 관리자가 발표했거나 발표일(10/4)이 됨 — 그때부터 좋아요 수(♥)를 보여 준다
        var vm = new FamilyPhotoBannerVM { Phase = Cfg.PhaseAt(now), IsPublished = Cfg.IsAnnouncedAt(now, state.IsPublished) };

        // 보이는 가족의 보이는 사진 — 썸네일에 필요한 칸만 읽는다
        var entries = await _db.FamilyPhotoEntries.AsNoTracking().Where(e => !e.IsHidden)
                               .Select(e => new { e.EntryId, e.FamilyName }).ToListAsync();
        var entryIds = entries.Select(e => e.EntryId).ToList();
        var photos   = await _db.FamilyPhotoItems.AsNoTracking().Where(p => entryIds.Contains(p.EntryId) && !p.IsHidden)
                                .Select(p => new { p.EntryId, p.PhotoId, p.Slot, p.ThumbPath, p.UpdatedAt }).ToListAsync();
        vm.EntryCount = photos.Select(p => p.EntryId).Distinct().Count();
        vm.PhotoCount = photos.Count;

        var names = entries.ToDictionary(e => e.EntryId, e => e.FamilyName);
        if (vm.IsPublished)
        {
            var r = await GetResultAsync(preview: false);
            vm.LikeTotal = r.LikeTotal;
            vm.Winners   = r.Ranking.Take(Cfg.WinnerCount).Select(e => e.FamilyName).ToList();
            // 발표 뒤 — 좋아요 많은 가족 순, 가족마다 가장 많이 받은 사진
            vm.Thumbs = r.Ranking.Where(e => e.Photos.Count > 0).Take(5).Select(e =>
            {
                var best = e.Photos.OrderByDescending(p => p.LikeCount).ThenBy(p => p.Slot).First();
                return new FamilyPhotoBannerThumb { FamilyName = e.FamilyName, ThumbPath = best.ThumbPath, LikeCount = best.LikeCount };
            }).ToList();
        }
        else
        {
            // 최근에 사진을 올린 가족 순, 가족마다 1번 자리(가장 앞) 사진
            vm.Thumbs = photos.GroupBy(p => p.EntryId)
                              .OrderByDescending(g => g.Max(p => p.UpdatedAt))
                              .Take(5)
                              .Select(g =>
                              {
                                  var first = g.OrderBy(p => p.Slot).First();
                                  return new FamilyPhotoBannerThumb { FamilyName = names.TryGetValue(g.Key, out var n) ? n : "", ThumbPath = first.ThumbPath };
                              }).ToList();
        }

        _bannerCache = vm;
        _bannerAt    = now;
        return vm;
    }

    public async Task<bool> IsPublishedAsync() => (await StateAsync()).IsPublished;

    public async Task<FamilyPhotoResultVM> GetResultAsync(bool preview)
    {
        var state = await StateAsync();
        // 좋아요 수는 발표 전(투표하는 동안)에는 보여 주지 않는다 — 관리자 미리 보기(preview)만 예외
        bool announced = Cfg.IsAnnouncedAt(DateHelper.GetKoreaNow(), state.IsPublished);
        var vm = new FamilyPhotoResultVM { IsPublished = announced, IsPreview = preview };
        if (!announced && !preview) return vm;

        var list = await LoadEntriesAsync(includeHidden: false, deviceKey: null, withCounts: true);
        list = list.Where(e => e.Photos.Count > 0)
                   .OrderByDescending(e => e.LikeTotal).ThenBy(e => e.CreatedAt).ToList();
        vm.Ranking    = list;
        vm.EntryCount = list.Count;
        vm.PhotoCount = list.Sum(e => e.Photos.Count);
        vm.LikeTotal  = list.Sum(e => e.LikeTotal);
        return vm;
    }

    // ══════════════════════════════════════════════════════════
    //  참가
    // ══════════════════════════════════════════════════════════
    public async Task<(bool ok, string msg, int entryId)> EnterAsync(string familyName, string pin, string? ip)
    {
        familyName = (familyName ?? "").Trim();
        if (familyName.Length == 0) return (false, "가족명을 적어 주세요.", 0);
        if (familyName.Length > Cfg.FamilyNameMax) return (false, $"가족명은 {Cfg.FamilyNameMax}자까지예요.", 0);
        if (!IsPin(pin)) return (false, "비밀번호는 숫자 4자리예요.", 0);

        var key = NameKey(familyName);
        var found = await _db.FamilyPhotoEntries.FirstOrDefaultAsync(e => e.FamilyNameKey == key);
        if (found != null)
        {
            if (Hash(pin, found.PinSalt) != found.PinHash)
                return (false, "이미 있는 가족명이에요. 우리 가족이 맞으면 비밀번호를 확인해 주세요.", 0);
            if (found.IsHidden)
                return (false, "이 가족은 관리자가 보류해 두었어요. 선생님께 문의해 주세요.", 0);
            return (true, "", found.EntryId);
        }

        var now = DateHelper.GetKoreaNow();
        // 올리기 마감은 없다 (2026-09-27) — 여는 날 전에만 막는다
        if (!Cfg.CanUploadAt(now))
            return (false, $"사진 올리기는 {Cfg.UploadStart:M월 d일}부터예요.", 0);

        var salt = NewSalt();
        var e = new FamilyPhotoEntry
        {
            FamilyName    = familyName,
            FamilyNameKey = key,
            PinSalt       = salt,
            PinHash       = Hash(pin, salt),
            CreatedIp     = ip,
            CreatedAt     = now,
            UpdatedAt     = now,
        };
        _db.FamilyPhotoEntries.Add(e);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException) { return (false, "같은 가족명이 방금 만들어졌어요. 다시 시도해 주세요.", 0); }
        return (true, "", e.EntryId);
    }

    public async Task<FamilyPhotoManageVM?> GetManageAsync(int entryId, string deviceKey)
    {
        var e = await _db.FamilyPhotoEntries.AsNoTracking().FirstOrDefaultAsync(x => x.EntryId == entryId);
        if (e == null) return null;
        var photos = await _db.FamilyPhotoItems.AsNoTracking().Where(p => p.EntryId == entryId).OrderBy(p => p.Slot).ToListAsync();
        var mine = new HashSet<int>();
        return new FamilyPhotoManageVM
        {
            CanUpload = Cfg.CanUploadAt(DateHelper.GetKoreaNow()),
            Entry = new FamilyPhotoEntryVM
            {
                EntryId = e.EntryId, FamilyName = e.FamilyName, IsHidden = e.IsHidden, CreatedAt = e.CreatedAt,
                Photos = photos.Select(p => ToVM(p, mine, null)).ToList(),
            },
        };
    }

    public async Task<(bool ok, string msg)> UploadAsync(int entryId, int slot, IFormFile file, string? caption)
    {
        var now = DateHelper.GetKoreaNow();
        if (!Cfg.CanUploadAt(now)) return (false, "사진 올리기 기간이 아니에요.");
        if (slot < 1 || slot > Cfg.MaxPhotos) return (false, "잘못된 자리예요.");
        if (file == null || file.Length == 0) return (false, "사진을 골라 주세요.");
        if (file.Length > Cfg.MaxFileBytes) return (false, $"사진은 한 장에 {Cfg.MaxFileBytes / 1024 / 1024}MB까지예요.");
        caption = Trunc(caption, Cfg.CaptionMax);

        var entry = await _db.FamilyPhotoEntries.FirstOrDefaultAsync(e => e.EntryId == entryId);
        if (entry == null) return (false, "가족을 찾을 수 없어요.");

        // 긴 변 1600 jpg + 480 썸네일.  EXIF 회전을 먼저 바로잡는다.
        string filePath, thumbPath; int w, h;
        try
        {
            await using var src = file.OpenReadStream();
            using var image = await Image.LoadAsync(src);
            image.Mutate(x => x.AutoOrient());

            var dir = Path.Combine(Root(_env), entryId.ToString());
            Directory.CreateDirectory(dir);
            var stamp = now.Ticks.ToString();

            using (var big = image.Clone(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(Cfg.LongEdge, Cfg.LongEdge) })))
            {
                w = big.Width; h = big.Height;
                var name = $"{slot}_{stamp}.jpg";
                await big.SaveAsJpegAsync(Path.Combine(dir, name), new JpegEncoder { Quality = 85 });
                filePath = $"/uploads/familyphoto/{entryId}/{name}";
            }
            using (var th = image.Clone(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(Cfg.ThumbEdge, Cfg.ThumbEdge) })))
            {
                var name = $"{slot}_{stamp}_t.jpg";
                await th.SaveAsJpegAsync(Path.Combine(dir, name), new JpegEncoder { Quality = 80 });
                thumbPath = $"/uploads/familyphoto/{entryId}/{name}";
            }
        }
        catch (UnknownImageFormatException)
        {
            return (false, "읽을 수 없는 사진 형식이에요. jpg · png 로 올려 주세요. (아이폰 HEIC 는 사진 앱에서 '호환성' 으로 저장하거나 카톡으로 한 번 보낸 사진을 쓰면 돼요)");
        }
        catch (Exception ex)
        {
            return (false, "사진을 저장하지 못했어요. (" + ex.Message + ")");
        }

        var item = await _db.FamilyPhotoItems.FirstOrDefaultAsync(p => p.EntryId == entryId && p.Slot == slot);
        if (item == null)
        {
            item = new FamilyPhotoItem { EntryId = entryId, Slot = slot, CreatedAt = now };
            _db.FamilyPhotoItems.Add(item);
        }
        else
        {
            DeleteFiles(item);
            // 사진을 바꾸면 그 사진의 좋아요는 지운다 (다른 사진이 된 것이므로)
            var likes = await _db.FamilyPhotoLikes.Where(l => l.PhotoId == item.PhotoId).ToListAsync();
            _db.FamilyPhotoLikes.RemoveRange(likes);
        }
        item.Caption   = caption;
        item.FilePath  = filePath;
        item.ThumbPath = thumbPath;
        item.Width     = w;
        item.Height    = h;
        item.IsHidden  = false;
        item.UpdatedAt = now;
        entry.UpdatedAt = now;
        await _db.SaveChangesAsync(); InvalidateBanner();   // 홈 배너 썸네일을 바로 새로
        return (true, "사진을 올렸어요.");
    }

    public async Task<(bool ok, string msg)> SetCaptionAsync(int entryId, int slot, string? caption)
    {
        if (!Cfg.CanUploadAt(DateHelper.GetKoreaNow())) return (false, "사진 올리기 기간이 아니에요.");
        var item = await _db.FamilyPhotoItems.FirstOrDefaultAsync(p => p.EntryId == entryId && p.Slot == slot);
        if (item == null) return (false, "사진이 없어요.");
        item.Caption   = Trunc(caption, Cfg.CaptionMax);
        item.UpdatedAt = DateHelper.GetKoreaNow();
        await _db.SaveChangesAsync();
        return (true, "설명을 고쳤어요.");
    }

    public async Task<(bool ok, string msg)> RemovePhotoAsync(int entryId, int slot)
    {
        if (!Cfg.CanUploadAt(DateHelper.GetKoreaNow())) return (false, "사진 올리기 기간이 아니에요.");
        var item = await _db.FamilyPhotoItems.FirstOrDefaultAsync(p => p.EntryId == entryId && p.Slot == slot);
        if (item == null) return (false, "사진이 없어요.");
        DeleteFiles(item);
        var likes = await _db.FamilyPhotoLikes.Where(l => l.PhotoId == item.PhotoId).ToListAsync();
        _db.FamilyPhotoLikes.RemoveRange(likes);
        _db.FamilyPhotoItems.Remove(item);
        await _db.SaveChangesAsync(); InvalidateBanner();   // 홈 배너 썸네일을 바로 새로
        return (true, "사진을 지웠어요.");
    }

    public async Task<(bool ok, string msg)> DeleteEntryAsync(int entryId)
    {
        if (!Cfg.CanUploadAt(DateHelper.GetKoreaNow())) return (false, "사진 올리기 기간이 아니에요.");
        var e = await _db.FamilyPhotoEntries.FirstOrDefaultAsync(x => x.EntryId == entryId);
        if (e == null) return (false, "가족을 찾을 수 없어요.");
        var items = await _db.FamilyPhotoItems.Where(p => p.EntryId == entryId).ToListAsync();
        var ids = items.Select(i => i.PhotoId).ToList();
        var likes = await _db.FamilyPhotoLikes.Where(l => ids.Contains(l.PhotoId)).ToListAsync();
        foreach (var i in items) DeleteFiles(i);
        _db.FamilyPhotoLikes.RemoveRange(likes);
        _db.FamilyPhotoItems.RemoveRange(items);
        _db.FamilyPhotoEntries.Remove(e);
        await _db.SaveChangesAsync(); InvalidateBanner();   // 홈 배너 썸네일을 바로 새로
        try { var dir = Path.Combine(Root(_env), entryId.ToString()); if (Directory.Exists(dir)) Directory.Delete(dir, true); } catch { }
        return (true, "가족 참가를 취소했어요.");
    }

    private void DeleteFiles(FamilyPhotoItem item)
    {
        foreach (var p in new[] { item.FilePath, item.ThumbPath })
        {
            try
            {
                if (string.IsNullOrWhiteSpace(p)) continue;
                var phys = Path.Combine(_env.WebRootPath, p.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(phys)) File.Delete(phys);
            }
            catch { }
        }
    }

    private static string? Trunc(string? s, int max)
    {
        s = (s ?? "").Trim();
        if (s.Length == 0) return null;
        return s.Length > max ? s[..max] : s;
    }

    // ══════════════════════════════════════════════════════════
    //  좋아요
    // ══════════════════════════════════════════════════════════
    /// <summary>좋아요 켜기 · 끄기 (다시 누르면 취소).  마감 없음 · 관리자는 투표 시작 전에도 누를 수 있다</summary>
    public async Task<(bool ok, string msg, bool liked)> ToggleLikeAsync(int photoId, string deviceKey, string? ip, bool isStaff = false)
    {
        var now = DateHelper.GetKoreaNow();
        if (!Cfg.CanLikeAt(now) && !isStaff)
            return (false, $"좋아요 투표는 {Cfg.LikeStart:M월 d일}부터예요.", false);
        if (string.IsNullOrWhiteSpace(deviceKey)) return (false, "잠시 후 다시 눌러 주세요.", false);

        var photo = await _db.FamilyPhotoItems.AsNoTracking().FirstOrDefaultAsync(p => p.PhotoId == photoId && !p.IsHidden);
        if (photo == null) return (false, "사진이 없어요.", false);
        var entryHidden = await _db.FamilyPhotoEntries.AsNoTracking().AnyAsync(e => e.EntryId == photo.EntryId && e.IsHidden);
        if (entryHidden) return (false, "사진이 없어요.", false);

        var mine = await _db.FamilyPhotoLikes.FirstOrDefaultAsync(l => l.PhotoId == photoId && l.DeviceKey == deviceKey);
        if (mine != null)
        {
            _db.FamilyPhotoLikes.Remove(mine);
            await _db.SaveChangesAsync();
            return (true, "좋아요를 취소했어요.", false);
        }

        if (!string.IsNullOrWhiteSpace(ip))
        {
            int fromIp = await _db.FamilyPhotoLikes.CountAsync(l => l.PhotoId == photoId && l.Ip == ip);
            if (fromIp >= Cfg.IpLimitPerPhoto)
                return (false, "같은 곳에서 너무 많이 눌렀어요. 다른 네트워크에서 눌러 주세요.", false);
        }

        _db.FamilyPhotoLikes.Add(new FamilyPhotoLike { PhotoId = photoId, DeviceKey = deviceKey, Ip = ip, CreatedAt = now });
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException) { return (true, "이미 눌렀어요.", true); }
        return (true, "좋아요!", true);
    }

    // ══════════════════════════════════════════════════════════
    //  AI 리뷰 보드 스티커 (2026-10-06)
    //  표 dbo.FamilyPhotoSticker 는 SQL/FamilyPhotoSticker_20261006.sql 로 만든다 (여기 Ddl 에는 넣지 않았다).
    //  표가 없으면 Available = false → 화면은 스티커 줄 없이 그린다.
    // ══════════════════════════════════════════════════════════
    private const string StickerTableSql = "SELECT CASE WHEN OBJECT_ID(N'dbo.FamilyPhotoSticker', N'U') IS NULL THEN 0 ELSE 1 END";

    private static System.Data.Common.DbParameter Param(System.Data.Common.DbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
        return p;
    }

    private async Task<bool> StickerTableExistsAsync(System.Data.Common.DbConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = StickerTableSql;
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 1;
    }

    private static async Task<int[]> StickerCountsAsync(System.Data.Common.DbConnection conn, int photoId)
    {
        var counts = new int[FamilyPhotoStickerVM.Emojis.Length];
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Kind, COUNT(*) FROM dbo.FamilyPhotoSticker WHERE PhotoId = @p GROUP BY Kind";
        Param(cmd, "@p", photoId);
        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            int k = Convert.ToInt32(rd.GetValue(0));
            if (k >= 0 && k < counts.Length) counts[k] = Convert.ToInt32(rd.GetValue(1));
        }
        return counts;
    }

    public async Task<FamilyPhotoStickerVM> GetStickersAsync(string? deviceKey)
    {
        var vm = new FamilyPhotoStickerVM();
        try
        {
            await _db.Database.OpenConnectionAsync();
            try
            {
                var conn = _db.Database.GetDbConnection();
                if (!await StickerTableExistsAsync(conn)) return vm;
                vm.Available = true;
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT PhotoId, Kind, COUNT(*), SUM(CASE WHEN DeviceKey = @d THEN 1 ELSE 0 END)
                                    FROM dbo.FamilyPhotoSticker GROUP BY PhotoId, Kind";
                Param(cmd, "@d", deviceKey ?? "");
                using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    int pid = Convert.ToInt32(rd.GetValue(0)), k = Convert.ToInt32(rd.GetValue(1));
                    if (k < 0 || k >= FamilyPhotoStickerVM.Emojis.Length) continue;
                    if (!vm.Counts.TryGetValue(pid, out var arr)) vm.Counts[pid] = arr = new int[FamilyPhotoStickerVM.Emojis.Length];
                    arr[k] = Convert.ToInt32(rd.GetValue(2));
                    if (Convert.ToInt32(rd.GetValue(3)) > 0) vm.Mine[pid] = k;
                }
            }
            finally { await _db.Database.CloseConnectionAsync(); }
        }
        catch { vm = new FamilyPhotoStickerVM(); }   // DB 문제면 스티커 없이
        return vm;
    }

    /// <summary>스티커 붙이기 — 처음이면 붙이고, 같은 걸 다시 누르면 떼고, 다른 걸 누르면 바꾼다.  mine = -1 이면 뗀 상태</summary>
    public async Task<(bool ok, string msg, int mine, int[] counts)> SetStickerAsync(int photoId, int kind, string deviceKey, string? ip)
    {
        var empty = new int[FamilyPhotoStickerVM.Emojis.Length];
        if (kind < 0 || kind >= FamilyPhotoStickerVM.Emojis.Length) return (false, "스티커를 다시 골라 주세요.", -1, empty);
        if (string.IsNullOrWhiteSpace(deviceKey)) return (false, "잠시 후 다시 눌러 주세요.", -1, empty);

        var photo = await _db.FamilyPhotoItems.AsNoTracking().FirstOrDefaultAsync(p => p.PhotoId == photoId && !p.IsHidden);
        if (photo == null || await _db.FamilyPhotoEntries.AsNoTracking().AnyAsync(e => e.EntryId == photo.EntryId && e.IsHidden))
            return (false, "사진이 없어요.", -1, empty);

        await _db.Database.OpenConnectionAsync();
        try
        {
            var conn = _db.Database.GetDbConnection();
            if (!await StickerTableExistsAsync(conn)) return (false, "스티커는 곧 열려요.", -1, empty);

            if (!string.IsNullOrWhiteSpace(ip))
            {
                using var c0 = conn.CreateCommand();
                c0.CommandText = "SELECT COUNT(*) FROM dbo.FamilyPhotoSticker WHERE PhotoId = @p AND Ip = @ip AND DeviceKey <> @d";
                Param(c0, "@p", photoId); Param(c0, "@ip", ip); Param(c0, "@d", deviceKey);
                if (Convert.ToInt32(await c0.ExecuteScalarAsync()) >= Cfg.IpLimitPerPhoto)
                    return (false, "같은 곳에서 너무 많이 눌렀어요.", -1, await StickerCountsAsync(conn, photoId));
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
DECLARE @cur TINYINT = (SELECT Kind FROM dbo.FamilyPhotoSticker WHERE PhotoId = @p AND DeviceKey = @d);
IF @cur IS NULL
BEGIN
    INSERT INTO dbo.FamilyPhotoSticker (PhotoId, DeviceKey, Kind, Ip, CreatedAt) VALUES (@p, @d, @k, @ip, @now);
    SELECT CAST(@k AS INT);
END
ELSE IF @cur = @k
BEGIN
    DELETE FROM dbo.FamilyPhotoSticker WHERE PhotoId = @p AND DeviceKey = @d;
    SELECT CAST(-1 AS INT);
END
ELSE
BEGIN
    UPDATE dbo.FamilyPhotoSticker SET Kind = @k, Ip = @ip, CreatedAt = @now WHERE PhotoId = @p AND DeviceKey = @d;
    SELECT CAST(@k AS INT);
END";
            Param(cmd, "@p", photoId); Param(cmd, "@d", deviceKey); Param(cmd, "@k", (byte)kind);
            Param(cmd, "@ip", Trunc(ip, 64)); Param(cmd, "@now", DateHelper.GetKoreaNow());
            int mine;
            try { mine = Convert.ToInt32(await cmd.ExecuteScalarAsync()); }
            catch (System.Data.Common.DbException) { mine = kind; }   // 두 번 빠르게 눌러 같은 줄이 겹친 경우
            return (true, mine < 0 ? "스티커를 뗐어요." : "스티커를 붙였어요!", mine, await StickerCountsAsync(conn, photoId));
        }
        finally { await _db.Database.CloseConnectionAsync(); }
    }

    // ══════════════════════════════════════════════════════════
    //  관리자
    // ══════════════════════════════════════════════════════════
    public async Task<FamilyPhotoAdminVM> GetAdminAsync()
    {
        var state = await StateAsync();
        var list  = await LoadEntriesAsync(includeHidden: true, deviceKey: null, withCounts: true);
        list = list.OrderByDescending(e => e.LikeTotal).ThenBy(e => e.CreatedAt).ToList();

        // 한 IP 가 한 사진에 몰아 누른 경우 (10표 이상)
        var sus = await _db.FamilyPhotoLikes.AsNoTracking()
                           .Where(l => l.Ip != null)
                           .GroupBy(l => new { l.PhotoId, l.Ip })
                           .Select(g => new { g.Key.PhotoId, N = g.Count() })
                           .Where(x => x.N >= 10)
                           .ToListAsync();

        return new FamilyPhotoAdminVM
        {
            Phase       = Cfg.PhaseNow(),
            State       = state,
            EntryCount  = list.Count(e => !e.IsHidden),
            PhotoCount  = list.Where(e => !e.IsHidden).Sum(e => e.Photos.Count(p => !p.IsHidden)),
            LikeTotal   = list.Where(e => !e.IsHidden).Sum(e => e.Photos.Where(p => !p.IsHidden).Sum(p => p.LikeCount)),
            HiddenCount = list.Count(e => e.IsHidden) + list.Sum(e => e.Photos.Count(p => p.IsHidden)),
            Entries     = list,
            SuspiciousIpMax = sus.GroupBy(x => x.PhotoId).ToDictionary(g => g.Key, g => g.Max(x => x.N)),
        };
    }

    public async Task<(bool ok, string msg)> SetEntryHiddenAsync(int entryId, bool hidden, string adminName)
    {
        var e = await _db.FamilyPhotoEntries.FirstOrDefaultAsync(x => x.EntryId == entryId);
        if (e == null) return (false, "가족을 찾을 수 없어요.");
        e.IsHidden = hidden;
        e.HiddenBy = hidden ? adminName : null;
        e.HiddenAt = hidden ? DateHelper.GetKoreaNow() : null;
        e.UpdatedAt = DateHelper.GetKoreaNow();
        await _db.SaveChangesAsync(); InvalidateBanner();   // 홈 배너 썸네일을 바로 새로
        return (true, hidden ? $"'{e.FamilyName}' 를 숨겼어요." : $"'{e.FamilyName}' 를 다시 보여요.");
    }

    public async Task<(bool ok, string msg)> SetPhotoHiddenAsync(int photoId, bool hidden, string adminName)
    {
        var p = await _db.FamilyPhotoItems.FirstOrDefaultAsync(x => x.PhotoId == photoId);
        if (p == null) return (false, "사진을 찾을 수 없어요.");
        p.IsHidden = hidden;
        p.HiddenBy = hidden ? adminName : null;
        p.HiddenAt = hidden ? DateHelper.GetKoreaNow() : null;
        p.UpdatedAt = DateHelper.GetKoreaNow();
        await _db.SaveChangesAsync(); InvalidateBanner();   // 홈 배너 썸네일을 바로 새로
        return (true, hidden ? "사진을 숨겼어요." : "사진을 다시 보여요.");
    }

    public async Task<(bool ok, string msg, string newPin)> ResetPinAsync(int entryId, string adminName)
    {
        var e = await _db.FamilyPhotoEntries.FirstOrDefaultAsync(x => x.EntryId == entryId);
        if (e == null) return (false, "가족을 찾을 수 없어요.", "");
        var pin  = RandomNumberGenerator.GetInt32(0, 10000).ToString("0000");
        e.PinSalt = NewSalt();
        e.PinHash = Hash(pin, e.PinSalt);
        e.UpdatedAt = DateHelper.GetKoreaNow();
        await _db.SaveChangesAsync();
        return (true, $"'{e.FamilyName}' 새 비밀번호: {pin}  (지금만 보여요 · 가족에게 알려 주세요)", pin);
    }

    public async Task<(bool ok, string msg)> SetPublishedAsync(bool published, string adminName)
    {
        var s = await StateAsync();
        s.IsPublished = published;
        s.PublishedAt = published ? DateHelper.GetKoreaNow() : null;
        s.PublishedBy = published ? adminName : null;
        await _db.SaveChangesAsync(); InvalidateBanner();   // 홈 배너 썸네일을 바로 새로
        return (true, published ? "결과를 발표했어요. 이제 누구나 결과 화면을 볼 수 있어요." : "발표를 되돌렸어요.");
    }

    public async Task<(bool ok, string msg)> ResetAllAsync(string adminName)
    {
        int nE = await _db.FamilyPhotoEntries.CountAsync();
        int nP = await _db.FamilyPhotoItems.CountAsync();
        int nL = await _db.FamilyPhotoLikes.CountAsync();

        await _db.Database.ExecuteSqlRawAsync("DELETE FROM dbo.FamilyPhotoLike; DELETE FROM dbo.FamilyPhotoItem; DELETE FROM dbo.FamilyPhotoEntry;");
        var s = await StateAsync();
        s.IsPublished = false; s.PublishedAt = null; s.PublishedBy = null;
        await _db.SaveChangesAsync(); InvalidateBanner();   // 홈 배너 썸네일을 바로 새로

        try { var root = Root(_env); if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        return (true, $"전부 지웠어요 — 가족 {nE} · 사진 {nP} · 좋아요 {nL}. ({adminName})");
    }
}

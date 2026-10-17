using BibleReadingThrough.Data;
using BibleReadingThrough.Models;
using BibleReadingThrough.Models.BibleGift;
using BulletinBoard.BusinessLayer;
using BulletinBoard.Common;
using BulletinBoard.Helpers;
using BBM = BulletinBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BibleReadingThrough.Controllers;

[Authorize]
public class BibleController : Controller
{
    private static readonly HashSet<string> NtBooks = new()
    {
        "마태복음","마가복음","누가복음","요한복음","사도행전","로마서",
        "고린도전서","고린도후서","갈라디아서","에베소서","빌립보서","골로새서",
        "데살로니가전서","데살로니가후서","디모데전서","디모데후서","디도서",
        "빌레몬서","히브리서","야고보서","베드로전서","베드로후서",
        "요한일서","요한이서","요한삼서",
        "요한1서","요한2서","요한3서",
        "유다서","요한계시록"
    };

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    public BibleController(AppDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    // /Bible/Welcome — 성경통독 허브 페이지
    [HttpGet]
    public IActionResult Welcome()
    {
        ViewData["Title"] = "성경통독 허브";
        ViewData["Page"]  = "welcome";

        try
        {
            var mgr   = new ManageReadBible();
            var users = mgr.GetUserTotalProgressRatesByUser(_env, BilbeProcessOption.TotalProgressRate);
            ViewBag.CommunityUsers = users;
        }
        catch
        {
            ViewBag.CommunityUsers = new List<BBM.BibleLastReadRateByUserSimple>();
        }

        return View();
    }

    // /Bible/Participants — 성경통독 참여자 현황 페이지
    [HttpGet]
    public IActionResult Participants()
    {
        ViewData["Title"] = "성경통독 참여자";
        ViewData["Page"]  = "participants";

        try
        {
            var mgr   = new ManageReadBible();
            var users = mgr.GetUserTotalProgressRatesByUser(_env, BilbeProcessOption.TotalProgressRate)
                           .OrderByDescending(x => x.TotalProgressRate)
                           .ToList();

            // 사용자별 마지막 읽은 책/장 조회
            var lastReadDict = new Dictionary<string, BBM.BibleLastReadInfo>();
            foreach (var u in users)
            {
                var lr = mgr.GetLastReadVerse(u.UserId, 0, 0)
                            .OrderByDescending(x => x.ReadDate)
                            .FirstOrDefault();
                if (lr != null) lastReadDict[u.UserId] = lr;
            }

            ViewBag.CommunityUsers = users;
            ViewBag.LastReadBooks  = lastReadDict;

            // 완독자 명단 (UserBibleProgressCompleted 기반)
            try
            {
                var completedUsers = mgr.GetUserTotalProgressRatesByUserCompleted(_env)
                                        .OrderBy(x => x.LastName)
                                        .ThenBy(x => x.FirstName)
                                        .ToList();
                ViewBag.CompletedUsers = completedUsers;
            }
            catch
            {
                ViewBag.CompletedUsers = new List<BBM.BibleLastReadRateByUserCompleted>();
            }
        }
        catch
        {
            ViewBag.CommunityUsers = new List<BBM.BibleLastReadRateByUserSimple>();
            ViewBag.LastReadBooks  = new Dictionary<string, BBM.BibleLastReadInfo>();
            ViewBag.CompletedUsers = new List<BBM.BibleLastReadRateByUserCompleted>();
        }

        return View();
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();

        var chapterMeta = await _db.EBibles
            .GroupBy(e => new { e.BibleText, e.Jang })
            .Select(g => new
            {
                BibleText = g.Key.BibleText ?? string.Empty,
                Jang = g.Key.Jang,
                MinId = g.Min(e => e.Id),
                TotalVerses = g.Count()
            })
            .OrderBy(x => x.MinId)
            .ToListAsync();

        var userChapterProgress = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId)
            .Join(_db.EBibles,
                p => p.VerseId,
                e => e.Id,
                (p, e) => new
                {
                    BibleText = e.BibleText ?? string.Empty,
                    e.Jang,
                    p.VerseId,
                    ReadDate = p.ReadDate.Date
                })
            .ToListAsync();

        var readDates = userChapterProgress
            .Select(x => x.ReadDate)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var chapterReadSet = userChapterProgress
            .GroupBy(x => new { x.BibleText, x.Jang })
            .Where(g => chapterMeta.Any(c => c.BibleText == g.Key.BibleText && c.Jang == g.Key.Jang && c.TotalVerses == g.Select(x => x.VerseId).Distinct().Count()))
            .Select(g => $"{g.Key.BibleText}_{g.Key.Jang}")
            .ToHashSet();

        var bookGroups = chapterMeta
            .GroupBy(c => c.BibleText)
            .Select(g => new
            {
                Name = g.Key,
                Testament = NtBooks.Contains(g.Key) ? "신약" : "구약",
                TotalChapters = g.Count(),
                ReadChapters = g.Count(c => chapterReadSet.Contains($"{c.BibleText}_{c.Jang}"))
            })
            .ToList();

        int streak = 0;
        var today = DateHelper.GetKoreaNow().Date; // 한국 시간 기준 오늘
        if (readDates.Contains(today) || readDates.Contains(today.AddDays(-1)))
        {
            var d = readDates.Contains(today) ? today : today.AddDays(-1);
            while (readDates.Contains(d))
            {
                streak++;
                d = d.AddDays(-1);
            }
        }

        var dateCnt = userChapterProgress
            .GroupBy(p => p.ReadDate)
            .ToDictionary(g => g.Key, g => g.Count());

        var heatmap = Enumerable.Range(0, 56)
            .Select(i => today.AddDays(-55 + i))
            .Select(d => new HeatmapDay { Date = d, ChaptersRead = dateCnt.GetValueOrDefault(d, 0) })
            .ToList();

        // 중요 성경구절 목록 (책이름, 장, 절)
        var keyVerses = new (string Book, int Jang, int Zul)[]
        {
            ("요한복음",  3, 16), ("로마서",     8, 28), ("빌립보서",   4, 13),
            ("시편",     23,  1), ("이사야",    40, 31), ("예레미야",  29, 11),
            ("요한복음", 14,  6), ("마태복음",  11, 28), ("로마서",     8, 38),
            ("빌립보서",  4,  7), ("여호수아",   1,  9), ("잠언",       3,  5),
            ("시편",    119,105), ("로마서",     6, 23), ("갈라디아서", 2, 20),
            ("에베소서",  2,  8), ("요한복음",  15,  5), ("마태복음",   6, 33),
            ("고린도전서",13,  4), ("고린도후서", 5, 17), ("시편",      46,  1),
            ("이사야",   41, 10), ("로마서",     3, 23), ("야고보서",   1,  2),
            ("마태복음", 28, 19), ("사도행전",   1,  8), ("요한일서",   4,  8),
            ("히브리서", 11,  1), ("데살로니가전서",5,16), ("골로새서",  3, 23),
            ("로마서",   12,  1), ("마태복음",  22, 37), ("요한복음",   1,  1),
            ("시편",      1,  1), ("잠언",      16,  3), ("신명기",      6,  5),
            ("미가",      6,  8), ("로마서",     5,  8), ("에베소서",   6, 10),
            ("요한복음", 10, 10), ("누가복음",   1, 37), ("베드로전서", 5,  7),
            ("히브리서", 12,  1), ("창세기",     1,  1), ("시편",      91,  1),
            ("마태복음",  5,  3), ("로마서",    10,  9), ("빌립보서",   4,  4),
            ("시편",     34,  8), ("요한복음",  11, 25),
        };

        var keyIdx    = (today.DayOfYear + today.Year) % keyVerses.Length;
        var (kvBook, kvJang, kvZul) = keyVerses[keyIdx];
        var todayVerse = await _db.EBibles
            .FirstOrDefaultAsync(e => e.BibleText == kvBook && e.Jang == kvJang && e.Zul == kvZul)
            // 해당 절이 DB에 없으면 fallback
            ?? await _db.EBibles.OrderBy(e => e.Id)
                .Skip((today.DayOfYear * 7 + today.Year) % Math.Max(await _db.EBibles.CountAsync(), 1))
                .FirstOrDefaultAsync();

        // 마지막으로 읽은 구절 (같은 일시이면 장·절이 가장 큰 것 우선)
        var lastRead = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId)
            .Join(_db.EBibles,
                p => p.VerseId,
                e => e.Id,
                (p, e) => new LastReadVerseItem
                {
                    BookName  = e.BibleText ?? string.Empty,
                    Jang      = e.Jang,
                    Zul       = e.Zul,
                    VerseText = e.BibleData ?? string.Empty,
                    ReadDate  = p.ReadDate
                })
            .OrderByDescending(x => x.ReadDate)
            .ThenByDescending(x => x.Jang)
            .ThenByDescending(x => x.Zul)
            .FirstOrDefaultAsync();

        var vm = new DashboardViewModel
        {
            TotalChapters = bookGroups.Sum(b => b.TotalChapters),
            ReadChapters = bookGroups.Sum(b => b.ReadChapters),
            OtTotal = bookGroups.Where(b => b.Testament == "구약").Sum(b => b.TotalChapters),
            OtRead = bookGroups.Where(b => b.Testament == "구약").Sum(b => b.ReadChapters),
            NtTotal = bookGroups.Where(b => b.Testament == "신약").Sum(b => b.TotalChapters),
            NtRead = bookGroups.Where(b => b.Testament == "신약").Sum(b => b.ReadChapters),
            StreakDays = streak,
            TotalReadDays = readDates.Count,
            Heatmap = heatmap,
            TodayVerseText = todayVerse?.BibleData ?? string.Empty,
            TodayVerseRef  = todayVerse != null ? $"{todayVerse.BibleText} {todayVerse.Jang}:{todayVerse.Zul}" : string.Empty,
            LastReadVerse  = lastRead,
            BookProgress = bookGroups.Select(b => new BookProgressItem
            {
                Name = b.Name,
                Testament = b.Testament,
                TotalChapters = b.TotalChapters,
                ReadChapters = b.ReadChapters
            }).ToList(),
            NextToRead = chapterMeta
                .GroupBy(c => c.BibleText)
                .Select(g => g.FirstOrDefault(c => !chapterReadSet.Contains($"{c.BibleText}_{c.Jang}")))
                .Where(c => c != null)
                .Take(5)
                .Select(c => new NextReadItem
                {
                    BookName = c!.BibleText,
                    ChapterNumber = c.Jang,
                    Testament = NtBooks.Contains(c.BibleText) ? "신약" : "구약"
                }).ToList()
        };

        return View(vm);
    }

    public async Task<IActionResult> List(string testament = "구약")
    {
        var userId = GetCurrentUserId();

        var chapterMeta = await _db.EBibles
            .GroupBy(e => new { e.BibleText, e.Jang })
            .Select(g => new
            {
                BibleText = g.Key.BibleText ?? string.Empty,
                Jang = g.Key.Jang,
                MinId = g.Min(e => e.Id),
                TotalVerses = g.Count()
            })
            .OrderBy(x => x.MinId)
            .ToListAsync();

        var chapterReadKeys = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId)
            .Join(_db.EBibles,
                p => p.VerseId,
                e => e.Id,
                (p, e) => new { BibleText = e.BibleText ?? string.Empty, e.Jang, p.VerseId })
            .ToListAsync();

        var chapterReadSet = chapterReadKeys
            .GroupBy(x => new { x.BibleText, x.Jang })
            .Where(g => chapterMeta.Any(c => c.BibleText == g.Key.BibleText && c.Jang == g.Key.Jang && c.TotalVerses == g.Select(x => x.VerseId).Distinct().Count()))
            .Select(g => $"{g.Key.BibleText}_{g.Key.Jang}")
            .ToHashSet();

        var books = chapterMeta
            .Where(c => NtBooks.Contains(c.BibleText) ? testament == "신약" : testament == "구약")
            .GroupBy(c => c.BibleText)
            .Select(g => new BookReadingViewModel
            {
                Name = g.Key,
                Testament = testament,
                Chapters = g.OrderBy(c => c.Jang)
                    .Select(c => new ChapterItem
                    {
                        Jang = c.Jang,
                        IsRead = chapterReadSet.Contains($"{c.BibleText}_{c.Jang}")
                    })
                    .ToList()
            })
            .ToList();

        ViewBag.Testament = testament;
        return View(books);
    }

    public async Task<IActionResult> Read(string bookName, int jang)
    {
        var userId = GetCurrentUserId();

        var verses = await _db.EBibles
            .Where(e => e.BibleText == bookName && e.Jang == jang)
            .OrderBy(e => e.Zul)
            .ToListAsync();

        if (!verses.Any())
        {
            return NotFound();
        }

        var readVerseIds = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId)
            .Join(_db.EBibles.Where(e => e.BibleText == bookName && e.Jang == jang),
                p => p.VerseId,
                e => e.Id,
                (p, e) => e.Id)
            .ToListAsync();

        var readSet = readVerseIds.ToHashSet();

        var bookmarkVerseIds = await _db.UserBookMarks
            .Where(b => b.UserId == userId)
            .Join(_db.EBibles.Where(e => e.BibleText == bookName && e.Jang == jang),
                b => b.VerseId,
                e => e.Id,
                (b, e) => e.Id)
            .ToListAsync();

        var bookmarkSet = bookmarkVerseIds.ToHashSet();

        var allChapters = await _db.EBibles
            .Where(e => e.BibleText == bookName)
            .Select(e => e.Jang)
            .Distinct()
            .OrderBy(j => j)
            .ToListAsync();

        // 책 전체 장별 절 수
        var chapterVerseCounts = (await _db.EBibles
            .Where(e => e.BibleText == bookName)
            .GroupBy(e => e.Jang)
            .Select(g => new { Jang = g.Key, Total = g.Count() })
            .ToListAsync())
            .ToDictionary(g => g.Jang, g => g.Total);

        // 이 사용자가 읽은 절 수 (책 전체)
        var readCountByChapter = (await _db.UserBibleProgresses
            .Where(p => p.UserId == userId)
            .Join(_db.EBibles.Where(e => e.BibleText == bookName),
                p => p.VerseId, e => e.Id, (p, e) => e.Jang)
            .GroupBy(j => j)
            .Select(g => new { Jang = g.Key, ReadCount = g.Count() })
            .ToListAsync())
            .ToDictionary(g => g.Jang, g => g.ReadCount);

        // 전체 절을 모두 읽은 장만 ReadChapters에 포함
        var readChapters = chapterVerseCounts
            .Where(kv => readCountByChapter.GetValueOrDefault(kv.Key, 0) >= kv.Value)
            .Select(kv => kv.Key)
            .ToHashSet();

        var allBooks = await _db.EBibles
            .GroupBy(e => e.BibleText)
            .Select(g => new { BibleText = g.Key, MinId = g.Min(e => e.Id) })
            .OrderBy(x => x.MinId)
            .Select(x => x.BibleText!)
            .ToListAsync();

        int ci = allChapters.IndexOf(jang);
        int bi = allBooks.IndexOf(bookName);
        string? pb = null, nb = null;
        int pj = 0, nj = 0;

        if (ci > 0)
        {
            pb = bookName;
            pj = allChapters[ci - 1];
        }
        else if (bi > 0)
        {
            pb = allBooks[bi - 1];
            pj = await _db.EBibles.Where(e => e.BibleText == pb).MaxAsync(e => e.Jang);
        }

        if (ci < allChapters.Count - 1)
        {
            nb = bookName;
            nj = allChapters[ci + 1];
        }
        else if (bi < allBooks.Count - 1)
        {
            nb = allBooks[bi + 1];
            nj = await _db.EBibles.Where(e => e.BibleText == nb).MinAsync(e => e.Jang);
        }

        // 오늘 하루 읽은 전체 절수 (모든 책·장 합산)
        var todayKst = DateHelper.GetKoreaNow().Date;
        var todayTotalReadCount = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId && p.ReadDate >= todayKst && p.ReadDate < todayKst.AddDays(1))
            .CountAsync();

        // YouTube 듣기 URL 조회
        var youtube = await _db.EBibleYouTuBus
            .Where(y => y.BibleText == bookName && y.Jang == jang)
            .FirstOrDefaultAsync();

        // 현재 장에 해당하는 요약 조회 (FromJang <= jang <= ToJang)
        var summaries = await _db.EBibleSummaries
            .Where(s => s.BibleText == bookName && s.FromJang <= jang && s.ToJang >= jang)
            .OrderBy(s => s.FromJang)
            .ThenBy(s => s.FromZul)
            .ToListAsync();

        // 성경 개요 데이터 — 간략본(BasicInfo) · 상세본(OverviewDetail)
        BBM.BibleBasicInfos2? basicInfo = null;
        BBM.BibleBookOverviewDetail? overviewDetail = null;
        try
        {
            var mngBulk    = new BulletinBoard.BusinessLayer.ManageeBulkInputBible();
            var basicList  = mngBulk.getBibleBasicListByBibleText2(0, bookName);
            var detailList = mngBulk.getBibleBasicListByBibleText3(0, bookName);
            basicInfo      = basicList.FirstOrDefault();
            overviewDetail = detailList.FirstOrDefault();
        }
        catch { /* 개요 데이터는 선택사항 — 없어도 읽기 기능은 정상 동작 */ }

        // 영문 성경 (eBibleEN) — 한국어 Id와 1:1 pair
        var engMap = await _db.EBibleEns
            .Where(e => e.BibleText == bookName && e.Jang == jang)
            .ToDictionaryAsync(e => e.Id, e => e.BibleData ?? string.Empty);

        // 현대인성경 (eBibleHD) — 한국어 Id와 1:1 pair
        var modernMap = await _db.EBibleHDs
            .Where(e => e.BibleText == bookName && e.Jang == jang)
            .ToDictionaryAsync(e => e.Id, e => e.BibleData ?? string.Empty);

        return View(new BibleReadViewModel
        {
            BookName = bookName,
            Jang = jang,
            Testament = NtBooks.Contains(bookName) ? "신약" : "구약",
            Verses = verses.Select(v => new VerseItem
            {
                VerseId         = v.Id,
                Zul             = v.Zul,
                BibleData       = v.BibleData ?? string.Empty,
                BibleDataModern = modernMap.TryGetValue(v.Id, out var mod) ? mod : null,
                BibleDataEn     = engMap.TryGetValue(v.Id, out var eng) ? eng : null,
                IsRead          = readSet.Contains(v.Id),
                IsBookmarked    = bookmarkSet.Contains(v.Id)
            }).ToList(),
            AllChapters = allChapters,
            ReadChapters = readChapters,
            IsChapterRead = verses.All(v => readSet.Contains(v.Id)),
            PrevBook = pb,
            PrevJang = pj,
            NextBook = nb,
            NextJang = nj,
            YoutubeUrl          = youtube?.URL,
            YoutubeTimeMM       = youtube?.TimeMM ?? 0,
            YoutubeTimeSS       = youtube?.TimeSS ?? 0,
            Summaries           = summaries,
            TodayTotalReadCount = todayTotalReadCount,
            BasicInfo           = basicInfo,
            OverviewDetail      = overviewDetail,
        });
    }

    private const int SearchPageSize = 20;

    /// <summary>인기 검색어 순위 공통 조회 (상위 10개)</summary>
    private async Task<List<SearchRankItem>> GetSearchRankingsAsync()
    {
        try
        {
            var raw = await _db.BibleSearchLogs
                .GroupBy(l => l.Query)
                .Select(g => new SearchRankItem
                {
                    Query          = g.Key,
                    Count          = g.Count(),
                    LastSearchedAt = g.Max(l => l.SearchedAt)
                })
                .OrderByDescending(r => r.Count)
                .ThenByDescending(r => r.LastSearchedAt)
                .Take(10)
                .ToListAsync();

            for (int i = 0; i < raw.Count; i++) raw[i].Rank = i + 1;
            return raw;
        }
        catch { return new List<SearchRankItem>(); }
    }

    [AllowAnonymous]
    public async Task<IActionResult> Search(string? q)
    {
        // ── 검색어 로그 저장 ──
        if (!string.IsNullOrWhiteSpace(q))
        {
            try
            {
                _db.BibleSearchLogs.Add(new BibleSearchLog
                {
                    Query      = q.Trim(),
                    SearchedAt = DateHelper.GetKoreaNow(),
                    UserId     = User.FindFirstValue(ClaimTypes.NameIdentifier)
                });
                await _db.SaveChangesAsync();
            }
            catch { /* 로그 저장 실패 시 검색은 계속 진행 */ }
        }

        // ── 인기 검색어 순위 ──
        var rankings = await GetSearchRankingsAsync();

        if (string.IsNullOrWhiteSpace(q))
            return View(new SearchViewModel { Rankings = rankings });

        var query = _db.EBibles
            .Where(e => e.BibleData != null && e.BibleData.Contains(q))
            .OrderBy(e => e.Id);

        // 전체 건수 + 첫 페이지만 동시 조회
        var total   = await query.CountAsync();
        var results = await query.Take(SearchPageSize + 1).ToListAsync();
        var hasMore = results.Count > SearchPageSize;
        if (hasMore) results = results.Take(SearchPageSize).ToList();

        return View(new SearchViewModel
        {
            Query      = q,
            Results    = results,
            TotalCount = total,
            HasMore    = hasMore,
            Rankings   = rankings
        });
    }

    // 무한 스크롤 추가 로드 API
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchMore(string q, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(q)) return Json(new { results = Array.Empty<object>(), hasMore = false });

        var batch = await _db.EBibles
            .Where(e => e.BibleData != null && e.BibleData.Contains(q))
            .OrderBy(e => e.Id)
            .Skip(offset)
            .Take(SearchPageSize + 1)
            .Select(e => new { e.Id, bibleText = e.BibleText, jang = e.Jang, zul = e.Zul, bibleData = e.BibleData })
            .ToListAsync();

        var hasMore = batch.Count > SearchPageSize;
        if (hasMore) batch = batch.Take(SearchPageSize).ToList();

        return Json(new { results = batch, hasMore });
    }

    public async Task<IActionResult> Bookmarks()
    {
        var userId = GetCurrentUserId();

        var bms = await _db.UserBookMarks
            .Where(b => b.UserId == userId)
            .Join(_db.EBibles,
                b => b.VerseId,
                e => e.Id,
                (b, e) => new UserBookMarkListItem
                {
                    UserId = b.UserId,
                    VerseId = b.VerseId,
                    BibleText = e.BibleText ?? string.Empty,
                    Jang = e.Jang,
                    Zul = e.Zul,
                    VerseText = e.BibleData ?? string.Empty,
                    InsertDT = b.InsertDT
                })
            .OrderByDescending(b => b.InsertDT)
            .ThenBy(b => b.BibleText)
            .ThenBy(b => b.Jang)
            .ThenBy(b => b.Zul)
            .ToListAsync();

        return View(bms);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleChapter(string bookName, int jang)
    {
        var userId = GetCurrentUserId();

        // verseIds.Contains → CTE(WITH) 오류 방지: 서버사이드 JOIN COUNT
        var totalVerses = await _db.EBibles.CountAsync(e => e.BibleText == bookName && e.Jang == jang);
        if (totalVerses == 0) return NotFound();

        var existingCount = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId)
            .Join(_db.EBibles.Where(e => e.BibleText == bookName && e.Jang == jang),
                p => p.VerseId, e => e.Id, (p, e) => p.VerseId)
            .CountAsync();

        bool makeRead = existingCount < totalVerses;
        if (makeRead)
        {
            var now = DateHelper.GetKoreaNow();
            await _db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO UserBibleProgress (UserId, VerseId, ReadDate)
                  SELECT {0}, eb.Id, {2}
                  FROM eBible eb
                  WHERE eb.BibleText = {1} AND eb.Jang = {3}
                    AND NOT EXISTS (
                        SELECT 1 FROM UserBibleProgress ubp
                        WHERE ubp.UserId = {0} AND ubp.VerseId = eb.Id
                    )",
                userId, bookName, now, jang);
        }
        else
        {
            await _db.Database.ExecuteSqlRawAsync(
                @"DELETE FROM UserBibleProgress
                  WHERE UserId = {0}
                    AND VerseId IN (SELECT Id FROM eBible WHERE BibleText = {1} AND Jang = {2})",
                userId, bookName, jang);
        }

        var todayKstC = DateHelper.GetKoreaNow().Date;
        int todayTotalC = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId && p.ReadDate >= todayKstC && p.ReadDate < todayKstC.AddDays(1))
            .CountAsync();

        return Json(new { isRead = makeRead, readVerses = makeRead ? totalVerses : 0, totalVerses, todayTotal = todayTotalC });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleBook(string bookName)
    {
        var userId = GetCurrentUserId();

        // verseIds.Contains → CTE(WITH) 오류 방지: 서버사이드 JOIN COUNT
        var totalVerses = await _db.EBibles.CountAsync(e => e.BibleText == bookName);
        if (totalVerses == 0) return NotFound();

        var existingCount = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId)
            .Join(_db.EBibles.Where(e => e.BibleText == bookName),
                p => p.VerseId, e => e.Id, (p, e) => p.VerseId)
            .CountAsync();

        bool newState = existingCount < totalVerses;

        if (newState)
        {
            var now = DateHelper.GetKoreaNow();
            await _db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO UserBibleProgress (UserId, VerseId, ReadDate)
                  SELECT {0}, eb.Id, {2}
                  FROM eBible eb
                  WHERE eb.BibleText = {1}
                    AND NOT EXISTS (
                        SELECT 1 FROM UserBibleProgress ubp
                        WHERE ubp.UserId = {0} AND ubp.VerseId = eb.Id
                    )",
                userId, bookName, now);
        }
        else
        {
            await _db.Database.ExecuteSqlRawAsync(
                @"DELETE FROM UserBibleProgress
                  WHERE UserId = {0}
                    AND VerseId IN (SELECT Id FROM eBible WHERE BibleText = {1})",
                userId, bookName);
        }

        var totalChapters = await _db.EBibles
            .Where(e => e.BibleText == bookName)
            .Select(e => e.Jang)
            .Distinct()
            .CountAsync();

        return Json(new
        {
            allRead = newState,
            readCount = newState ? totalChapters : 0,
            totalCount = totalChapters
        });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleVerse(int verseId)
    {
        var userId = GetCurrentUserId();

        var verse = await _db.EBibles.FirstOrDefaultAsync(e => e.Id == verseId);
        if (verse == null) return NotFound();

        var progress = await _db.UserBibleProgresses.FindAsync(userId, verseId);
        bool isRead;

        if (progress == null)
        {
            _db.UserBibleProgresses.Add(new UserBibleProgress
            {
                UserId = userId,
                VerseId = verseId,
                ReadDate = DateHelper.GetKoreaNow()
            });
            isRead = true;
        }
        else
        {
            _db.UserBibleProgresses.Remove(progress);
            isRead = false;
        }

        await _db.SaveChangesAsync();

        int totalV = await _db.EBibles.CountAsync(e => e.BibleText == verse.BibleText && e.Jang == verse.Jang);
        int readV = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId)
            .Join(_db.EBibles.Where(e => e.BibleText == verse.BibleText && e.Jang == verse.Jang),
                p => p.VerseId,
                e => e.Id,
                (p, e) => e.Id)
            .Distinct()
            .CountAsync();

        // 오늘 하루 읽은 전체 절수
        var todayKst2 = DateHelper.GetKoreaNow().Date;
        int todayTotal = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId && p.ReadDate >= todayKst2 && p.ReadDate < todayKst2.AddDays(1))
            .CountAsync();

        return Json(new
        {
            isRead,
            readVerses  = readV,
            totalVerses = totalV,
            allRead     = readV >= totalV,
            todayTotal
        });
    }

    [HttpPost]
    public async Task<IActionResult> MarkChapterRead(string bookName, int jang)
    {
        var userId = GetCurrentUserId();

        // verseIds.Contains → CTE(WITH) 오류 방지: 서버사이드 COUNT + raw SQL INSERT
        var totalVerses = await _db.EBibles.CountAsync(e => e.BibleText == bookName && e.Jang == jang);
        if (totalVerses == 0) return NotFound();

        var now = DateHelper.GetKoreaNow();
        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO UserBibleProgress (UserId, VerseId, ReadDate)
              SELECT {0}, eb.Id, {2}
              FROM eBible eb
              WHERE eb.BibleText = {1} AND eb.Jang = {3}
                AND NOT EXISTS (
                    SELECT 1 FROM UserBibleProgress ubp
                    WHERE ubp.UserId = {0} AND ubp.VerseId = eb.Id
                )",
            userId, bookName, now, jang);

        return Json(new { success = true, readVerses = totalVerses, totalVerses, allRead = true });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleBookmark(int verseId)
    {
        var userId = GetCurrentUserId();

        var verse = await _db.EBibles.FirstOrDefaultAsync(e => e.Id == verseId);
        if (verse == null) return NotFound();

        var bm = await _db.UserBookMarks.FindAsync(userId, verseId);
        bool isBookmarked;

        if (bm == null)
        {
            _db.UserBookMarks.Add(new UserBookMark
            {
                UserId = userId,
                VerseId = verseId,
                InsertDT = DateHelper.GetKoreaNow()
            });
            isBookmarked = true;
        }
        else
        {
            _db.UserBookMarks.Remove(bm);
            isBookmarked = false;
        }

        await _db.SaveChangesAsync();

        return Json(new
        {
            isBookmarked,
            verseId,
            bookName = verse.BibleText,
            jang = verse.Jang,
            zul = verse.Zul
        });
    }

    // ── /Bible/ResetProgress GET — 완독 기록 삭제 확인 페이지 ──
    [HttpGet]
    public async Task<IActionResult> ResetProgress(string userId)
    {
        var myId = GetCurrentUserId();

        // 본인만 접근 가능
        if (string.IsNullOrWhiteSpace(userId) || myId != userId)
            return Forbid();

        // 이름 조회
        var identityUser = await _db.Database
            .SqlQueryRaw<IdentityUserRow>(
                $"SELECT Id, UserName, LastName = ISNULL(LastName,''), FirstName = ISNULL(FirstName,'') FROM [Identity].[User] WHERE Id = {{0}}", userId)
            .FirstOrDefaultAsync();

        string userName  = identityUser?.UserName ?? "";
        string fullName  = (identityUser?.LastName ?? "") + (identityUser?.FirstName ?? "");

        // 현재 완독 횟수
        int readCount = 0;
        var cntResult = await _db.Database
            .SqlQueryRaw<IntRow>(
                "SELECT ISNULL(readBibleAllCount,0) AS Value FROM [dbo].[UserBibleProgressCompleted] WHERE UserName = {0}", userName)
            .FirstOrDefaultAsync();
        readCount = cntResult?.Value ?? 0;

        // 읽은 구절 수
        int readVerses  = await _db.UserBibleProgresses.CountAsync(p => p.UserId == userId);
        int totalVerses = await _db.EBibles.CountAsync();

        ViewBag.UserId      = userId;
        ViewBag.UserName    = userName;
        ViewBag.FullName    = fullName;
        ViewBag.ReadCount   = readCount;
        ViewBag.ReadVerses  = readVerses;
        ViewBag.TotalVerses = totalVerses;

        return View();
    }

    // ── /Bible/ResetProgress POST — 실제 처리 ──
    //  ⚠ 홈 화면 '숨기기' 버튼이 실제로 오는 곳이 여기다 (/Bible2/ResetProgress 가 아니다).
    //     예전에는 여기서 BibleCompletionLog 를 남기지 않아, 회차별 완독 날짜·선물 자격이
    //     영구히 사라졌다. 완독 날짜(MAX ReadDate)는 반드시 ① 삭제 전에 읽어 둬야 한다 —
    //     지운 뒤에는 그 값을 구할 방법이 없다.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetProgress(string userId, [FromForm] string dummy = "")
    {
        var myId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId) || myId != userId)
            return Forbid();

        // UserName 조회
        var identityUser = await _db.Database
            .SqlQueryRaw<IdentityUserRow>(
                $"SELECT Id, UserName, LastName = ISNULL(LastName,''), FirstName = ISNULL(FirstName,'') FROM [Identity].[User] WHERE Id = {{0}}", userId)
            .FirstOrDefaultAsync();

        string userName = identityUser?.UserName ?? "";

        // ⓪ 실제 완독일 — 지우기 전에 반드시 먼저 읽어 둔다 (MAX ReadDate)
        var lastReadDate = await _db.UserBibleProgresses
            .Where(p => p.UserId == userId)
            .Select(p => (DateTime?)p.ReadDate)
            .MaxAsync();

        // ① UserBibleProgress 전체 삭제
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM [dbo].[UserBibleProgress] WHERE UserId = {0}", userId);

        // ② UserBibleProgressCompleted UPSERT
        int existing = await _db.Database
            .SqlQueryRaw<IntRow>(
                "SELECT COUNT(*) AS Value FROM [dbo].[UserBibleProgressCompleted] WHERE UserName = {0}", userName)
            .Select(r => r.Value)
            .FirstOrDefaultAsync();

        if (existing > 0)
        {
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE [dbo].[UserBibleProgressCompleted] SET readBibleAllCount = readBibleAllCount + 1 WHERE UserName = {0}",
                userName);
        }
        else
        {
            await _db.Database.ExecuteSqlRawAsync(
                "INSERT INTO [dbo].[UserBibleProgressCompleted] (UserName, readBibleAllCount) VALUES ({0}, 1)",
                userName);
        }

        // ③ 완독 날짜 기록 — 홈의 세로 타임라인·선물 자격이 이 표를 읽는다 (표가 없으면 조용히 넘어간다)
        try
        {
            int nextRound = await _db.BibleCompletionLogs
                .Where(l => l.UserId == userId)
                .Select(l => (int?)l.RoundNo)
                .MaxAsync() ?? 0;

            // 기존 완독 횟수(readBibleAllCount, 방금 ②에서 이미 +1 됨)와도 맞춰 둔다
            // — BibleCompletionLog 에 과거 기록이 비어 있어도(예전 버그로 못 남긴 회차)
            //   이 숫자가 진짜 몇 번째 완독인지 알려 주는 최종 기준이 된다.
            int doneCount = await _db.Database
                .SqlQueryRaw<IntRow>(
                    "SELECT ISNULL(MAX(readBibleAllCount),0) AS Value FROM [dbo].[UserBibleProgressCompleted] WHERE UserName = {0}", userName)
                .Select(r => r.Value)
                .FirstOrDefaultAsync();

            int round = Math.Max(nextRound + 1, doneCount);

            _db.BibleCompletionLogs.Add(new BibleCompletionLog
            {
                UserId      = userId,
                UserName    = userName,
                RoundNo     = round,
                CompletedAt = (lastReadDate ?? DateHelper.GetKoreaNow()).Date,
                CreatedAt   = DateHelper.GetKoreaNow(),
                IsSeed      = false
            });
            await _db.SaveChangesAsync();
        }
        catch { /* 날짜 기록 실패해도 완독 처리는 그대로 */ }

        return RedirectToAction(nameof(Participants));
    }

    // 내부용 raw-SQL 프로젝션 타입
    private class IdentityUserRow
    {
        public string Id        { get; set; } = "";
        public string UserName  { get; set; } = "";
        public string LastName  { get; set; } = "";
        public string FirstName { get; set; } = "";
    }
    private class IntRow { public int Value { get; set; } }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("로그인 사용자 정보를 찾을 수 없습니다.");
        }
        return userId;
    }
}

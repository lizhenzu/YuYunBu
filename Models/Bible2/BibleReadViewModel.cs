namespace BibleReadingThrough.Models;

public class BibleReadViewModel
{
    public string BookName { get; set; } = string.Empty;
    public int Jang { get; set; }
    public string Testament { get; set; } = string.Empty;
    public List<VerseItem> Verses { get; set; } = new();
    public List<int> AllChapters { get; set; } = new();
    /// <summary>이 책에서 모든 절을 읽은 장 번호 집합</summary>
    public HashSet<int> ReadChapters { get; set; } = new();
    public bool IsChapterRead { get; set; }
    public int ReadVerseCount => Verses.Count(v => v.IsRead);
    public bool AllVersesRead => Verses.Count > 0 && Verses.All(v => v.IsRead);

    public string? PrevBook { get; set; }
    public int PrevJang { get; set; }
    public string? NextBook { get; set; }
    public int NextJang { get; set; }

    // YouTube 듣기
    public string? YoutubeUrl    { get; set; }
    public int     YoutubeTimeMM { get; set; }
    public int     YoutubeTimeSS { get; set; }

    // 오늘 하루 읽은 전체 절수 (모든 책·장 합산)
    public int TodayTotalReadCount { get; set; }

    // 장 요약
    public List<EBibleSummary> Summaries { get; set; } = new();

    // 성경 개요 — 간략본 (저자·기록시기·개요·목적)
    public BulletinBoard.Models.BibleBasicInfos2? BasicInfo { get; set; }
    // 성경 개요 — 상세본 (저자설명·연대·장소·대상·역사배경·핵심어·내용요약)
    public BulletinBoard.Models.BibleBookOverviewDetail? OverviewDetail { get; set; }
}

public class VerseItem
{
    public int VerseId { get; set; }
    public int Zul { get; set; }
    public string BibleData { get; set; } = string.Empty;     // 개역개정 본문 (eBible)
    public string? BibleDataModern { get; set; }              // 현대인성경 본문 (eBibleHD, ID pair)
    public string? BibleDataEn { get; set; }                  // 영문 본문 NIV (eBibleEN, ID pair)
    public bool IsRead { get; set; }
    public bool IsBookmarked { get; set; }
}

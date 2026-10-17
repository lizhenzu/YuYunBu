namespace BibleReadingThrough.Models;

public class DashboardViewModel
{
    public int TotalChapters { get; set; }
    public int ReadChapters { get; set; }
    public double ProgressPercent => TotalChapters == 0 ? 0 : Math.Round((double)ReadChapters / TotalChapters * 100, 1);

    public int OtTotal { get; set; }
    public int OtRead { get; set; }
    public int NtTotal { get; set; }
    public int NtRead { get; set; }

    public int StreakDays { get; set; }
    public int TotalReadDays { get; set; }

    public List<HeatmapDay> Heatmap { get; set; } = new();

    public string TodayVerseText { get; set; } = string.Empty;
    public string TodayVerseRef  { get; set; } = string.Empty;

    public List<BookProgressItem> BookProgress { get; set; } = new();
    public List<NextReadItem> NextToRead { get; set; } = new();

    public LastReadVerseItem? LastReadVerse { get; set; }
}

public class LastReadVerseItem
{
    public string BookName    { get; set; } = string.Empty;
    public int    Jang        { get; set; }
    public int    Zul         { get; set; }
    public string VerseText   { get; set; } = string.Empty;
    public DateTime ReadDate  { get; set; }
}

public class BookProgressItem
{
    public string Name { get; set; } = string.Empty;
    public string Testament { get; set; } = string.Empty;
    public int TotalChapters { get; set; }
    public int ReadChapters { get; set; }
    public bool IsComplete => ReadChapters == TotalChapters;
    public double Pct => TotalChapters == 0 ? 0 : Math.Round((double)ReadChapters / TotalChapters * 100, 0);
}

public class NextReadItem
{
    public string BookName { get; set; } = string.Empty;
    public int ChapterNumber { get; set; }
    public string Testament { get; set; } = string.Empty;
}

public class HeatmapDay
{
    public DateTime Date { get; set; }
    public int ChaptersRead { get; set; }
    public bool IsToday => Date.Date == DateTime.Today;
    public string Level => ChaptersRead == 0 ? "" : ChaptersRead <= 2 ? "read-sm" : ChaptersRead <= 5 ? "read-md" : "read-lg";
}

public class BookReadingViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Testament { get; set; } = string.Empty;
    public List<ChapterItem> Chapters { get; set; } = new();
}

public class ChapterItem
{
    public int Jang { get; set; }
    public bool IsRead { get; set; }
}

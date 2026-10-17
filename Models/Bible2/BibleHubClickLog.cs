namespace BibleReadingThrough.Models;

/// <summary>성경통독 허브 버전 선택 클릭 — BibleHubClickLog 테이블 매핑</summary>
public class BibleHubClickLog
{
    public int      Id        { get; set; }
    public string   Version   { get; set; } = string.Empty;   // "old" | "new"
    public string?  UserId    { get; set; }
    public string?  UserName  { get; set; }
    public DateTime ClickedAt { get; set; }
}

/// <summary>허브 버전 선택 집계 한 기간분 (전체 · 오늘 · 7일)</summary>
public class HubClickPeriod
{
    public int OldClicks { get; set; }
    public int NewClicks { get; set; }
    public int OldUsers  { get; set; }
    public int NewUsers  { get; set; }

    public int Total  => OldClicks + NewClicks;
    public int OldPct => Total == 0 ? 0 : (int)Math.Round(OldClicks * 100.0 / Total);
    public int NewPct => Total == 0 ? 0 : 100 - OldPct;
}

/// <summary>허브 관리자용 집계 — Basic · SuperAdmin 에게만 보인다</summary>
public class HubClickStats
{
    public HubClickPeriod All   { get; set; } = new();
    public HubClickPeriod Today { get; set; } = new();
    public HubClickPeriod Week  { get; set; } = new();
}

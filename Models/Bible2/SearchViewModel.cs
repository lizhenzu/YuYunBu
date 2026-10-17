namespace BibleReadingThrough.Models;

public class SearchViewModel
{
    public string       Query      { get; set; } = string.Empty;
    public List<EBible> Results    { get; set; } = new();
    public int          TotalCount { get; set; }
    public bool         HasMore    { get; set; }

    /// <summary>인기 검색어 순위 (상위 10개)</summary>
    public List<SearchRankItem> Rankings { get; set; } = new();
}

public class SearchRankItem
{
    public int      Rank           { get; set; }
    public string   Query          { get; set; } = string.Empty;
    public int      Count          { get; set; }
    public DateTime LastSearchedAt { get; set; }
}

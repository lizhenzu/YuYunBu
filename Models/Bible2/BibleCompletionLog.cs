namespace BibleReadingThrough.Models;

/// <summary>성경 완독 기록 — BibleCompletionLog 테이블 매핑 (몇 번째 완독을 언제 마쳤는지)</summary>
public class BibleCompletionLog
{
    public int      Id          { get; set; }
    public string   UserId      { get; set; } = string.Empty;
    public string?  UserName    { get; set; }
    public int      RoundNo     { get; set; }        // 1 = 1독, 2 = 2독 …
    public DateTime CompletedAt { get; set; }        // 완독한 날
    public DateTime CreatedAt   { get; set; }
    public bool     IsSeed      { get; set; }        // true = 가짜(샘플) 데이터
}

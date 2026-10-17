namespace BibleReadingThrough.Models;

// 절 단위 읽기 기록 (필요할 때만 생성)
public class VerseProgress
{
    public int Id { get; set; }
    public string BibleText { get; set; } = string.Empty;
    public int Jang { get; set; }
    public int Zul { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
}

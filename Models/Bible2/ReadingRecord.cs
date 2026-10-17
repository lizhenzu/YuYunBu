namespace BibleReadingThrough.Models;

// 읽기 진도 추적 테이블 (앱에서 생성)
public class ReadingProgress
{
    public int Id { get; set; }
    public string BibleText { get; set; } = string.Empty;  // 책 이름
    public int Jang { get; set; }                          // 장
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
}

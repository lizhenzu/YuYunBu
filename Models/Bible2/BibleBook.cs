namespace BibleReadingThrough.Models;

// DB 테이블이 아닌 ViewModel용 클래스 (eBible에서 조회해서 채움)
public class BibleBook
{
    public string Name { get; set; } = string.Empty;
    public string Testament { get; set; } = string.Empty;
    public int Chapters { get; set; }
    public int MinId { get; set; }  // 순서 정렬용
}

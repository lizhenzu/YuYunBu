namespace BibleReadingThrough.Models;

// MSSQL eBible 테이블과 1:1 매핑 (읽기 전용)
public class EBible
{
    public int Id { get; set; }
    public string? BibleText { get; set; }   // 책 이름 (예: 창세기)
    public int Jang { get; set; }            // 장
    public int Zul { get; set; }             // 절
    public string? BibleData { get; set; }   // 성경 본문
    public DateTime InsertDT { get; set; }
    public DateTime UpdateDT { get; set; }
}

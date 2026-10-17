namespace BibleReadingThrough.Models;

// MSSQL eBibleEN 테이블 — eBible 테이블과 동일 구조, ID pair로 연결
public class EBibleEn
{
    public int Id { get; set; }
    public string? BibleText { get; set; }   // 책 이름 (영문)
    public int Jang { get; set; }            // 장
    public int Zul { get; set; }             // 절
    public string? BibleData { get; set; }   // 영문 성경 본문
    public DateTime InsertDT { get; set; }
    public DateTime UpdateDT { get; set; }
}

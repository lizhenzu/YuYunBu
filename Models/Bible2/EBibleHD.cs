namespace BibleReadingThrough.Models;

// MSSQL eBibleHD 테이블 — 현대인성경, eBible 테이블과 동일 구조, ID pair로 연결
public class EBibleHD
{
    public int Id { get; set; }
    public string? BibleText { get; set; }   // 책 이름
    public int Jang { get; set; }            // 장
    public int Zul { get; set; }             // 절
    public string? BibleData { get; set; }   // 현대인성경 본문
    public DateTime InsertDT { get; set; }
    public DateTime UpdateDT { get; set; }
}

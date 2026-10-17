namespace BibleReadingThrough.Models;

/// <summary>성경 검색어 로그 — BibleSearchLog 테이블 매핑</summary>
public class BibleSearchLog
{
    public int      Id         { get; set; }
    public string   Query      { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; }
    public string?  UserId     { get; set; }   // 비로그인 시 null
}

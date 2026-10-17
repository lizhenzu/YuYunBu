namespace BibleReadingThrough.Models;

public class Bookmark
{
    public int Id { get; set; }
    public string BibleText { get; set; } = string.Empty;
    public int Jang { get; set; }
    public int Zul { get; set; }
    public string? VerseText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

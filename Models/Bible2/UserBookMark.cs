namespace BibleReadingThrough.Models;

public class UserBookMark
{
    public string UserId { get; set; } = string.Empty;
    public int VerseId { get; set; }
    public DateTime InsertDT { get; set; } = DateTime.Now;
}

public class UserBookMarkListItem
{
    public string UserId { get; set; } = string.Empty;
    public int VerseId { get; set; }
    public string BibleText { get; set; } = string.Empty;
    public int Jang { get; set; }
    public int Zul { get; set; }
    public string VerseText { get; set; } = string.Empty;
    public DateTime InsertDT { get; set; }
}

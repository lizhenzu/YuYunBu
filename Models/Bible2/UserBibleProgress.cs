namespace BibleReadingThrough.Models;

public class UserBibleProgress
{
    public string UserId { get; set; } = string.Empty;
    public int VerseId { get; set; }
    public DateTime ReadDate { get; set; } = DateTime.Now;
}

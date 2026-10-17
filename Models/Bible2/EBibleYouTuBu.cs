namespace BibleReadingThrough.Models;

public class EBibleYouTuBu
{
    public int      Id        { get; set; }
    public string   BibleText { get; set; } = string.Empty;
    public int      Jang      { get; set; }
    public string   URL       { get; set; } = string.Empty;
    public int      TimeMM    { get; set; }
    public int      TimeSS    { get; set; }
    public DateTime InsertDT  { get; set; }
    public DateTime UpdateDT  { get; set; }
}

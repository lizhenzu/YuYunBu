namespace BibleReadingThrough.Models;

public class EBibleSummary
{
    public int     Id           { get; set; }
    public string? BibleText    { get; set; }
    public string? BibleKeyWord { get; set; }
    public string? BibleSummary { get; set; }
    public int     FromJang     { get; set; }
    public int     FromZul      { get; set; }
    public int     ToJang       { get; set; }
    public int     ToZul        { get; set; }
    public DateTime InsertDT    { get; set; }
    public DateTime UpdateDT    { get; set; }
}

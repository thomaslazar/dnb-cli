namespace DnbCli.Models;

public sealed class DnbRecord
{
    public string DnbId { get; set; } = "";
    public List<string> Isbns { get; set; } = new();
    public Title Title { get; set; } = new();
    public List<SeriesEntry> Series { get; set; } = new();
    public Languages Languages { get; set; } = new();
    public List<Contributor> Contributors { get; set; } = new();
    public Publication Publication { get; set; } = new();
    public string? Edition { get; set; }
    public string? Extent { get; set; }
    public string? Description { get; set; }
    public List<string> Genres { get; set; } = new();
    public List<string> Subjects { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public string MarcSource { get; set; } = "";
}

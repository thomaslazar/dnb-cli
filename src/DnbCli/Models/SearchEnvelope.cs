namespace DnbCli.Models;

public sealed class SearchEnvelope
{
    public string Query { get; set; } = "";
    public int TotalResults { get; set; }
    public int ReturnedResults { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public List<DnbRecord> Results { get; set; } = new();
}

namespace DnbCli.Models;

public sealed class Title
{
    public string Main { get; set; } = "";
    public string? Subtitle { get; set; }
    public string? PartNumber { get; set; }
    public string? PartName { get; set; }
    public string? Uniform { get; set; }
    public string? StatementOfResponsibility { get; set; }
}

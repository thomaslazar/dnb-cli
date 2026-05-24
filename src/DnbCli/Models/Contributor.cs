namespace DnbCli.Models;

public sealed class Contributor
{
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string? RoleLabel { get; set; }
    public string? GndId { get; set; }
}

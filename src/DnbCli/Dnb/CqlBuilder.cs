using System.Text;
using System.Text.RegularExpressions;

namespace DnbCli.Dnb;

public static class CqlBuilder
{
    public static string ForIsbn(string isbn)
    {
        var normalized = NormalizeIsbn(isbn);
        return $"isbn={normalized}";
    }

    public static string ForDnbId(string dnbId)
    {
        return $"IDN={dnbId}";
    }

    public static string ForSearch(
        string? title = null,
        string? author = null,
        string? year = null,
        string? series = null,
        string? any = null)
    {
        var clauses = new List<string>();
        if (!string.IsNullOrWhiteSpace(title)) clauses.Add($"TIT={Format(title!)}");
        if (!string.IsNullOrWhiteSpace(author)) clauses.Add($"PER={Format(author!)}");
        if (!string.IsNullOrWhiteSpace(year)) clauses.Add($"JHR={Format(year!)}");
        if (!string.IsNullOrWhiteSpace(series)) clauses.Add($"WOE={Format(series!)}");
        if (!string.IsNullOrWhiteSpace(any)) clauses.Add($"WOE={Format(any!)}");
        if (clauses.Count == 0)
            throw new ArgumentException("At least one of --title/--author/--year/--series/--any must be supplied.");
        return string.Join(" and ", clauses);
    }

    public static string NormalizeIsbn(string raw)
    {
        var sb = new StringBuilder(raw.Length);
        foreach (var c in raw)
        {
            if (char.IsDigit(c)) sb.Append(c);
            else if (c == 'X' || c == 'x') sb.Append(char.ToUpperInvariant(c));
        }
        return sb.ToString();
    }

    public static bool IsValidIsbnShape(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return false;
        var normalized = NormalizeIsbn(raw);
        if (normalized.Length is < 1 or > 13) return false;
        // Tail char must be digit or X; leading chars digits
        for (var i = 0; i < normalized.Length - 1; i++)
            if (!char.IsDigit(normalized[i])) return false;
        var last = normalized[^1];
        return char.IsDigit(last) || last == 'X';
    }

    private static string Format(string value)
    {
        if (value.Contains(' ')) return $"\"{value}\"";
        return value;
    }
}

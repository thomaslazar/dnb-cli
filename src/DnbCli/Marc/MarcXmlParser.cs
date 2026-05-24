using System.Xml.Linq;
using DnbCli.Dnb;
using DnbCli.Models;

namespace DnbCli.Marc;

public static class MarcXmlParser
{
    public static SearchEnvelope ParseSearchResponse(string xml, string query)
    {
        var doc = XDocument.Parse(xml);
        ThrowIfDiagnostic(doc);
        // Real implementation comes in Task 8+.
        throw new NotImplementedException("ParseSearchResponse body — implemented in later tasks");
    }

    public static DnbRecord ParseRecord(XElement recordEl, string dnbId)
    {
        var record = new DnbRecord
        {
            DnbId = dnbId,
            Isbns = ParseIsbns(recordEl),
            Title = ParseTitle(recordEl),
            Languages = ParseLanguages(recordEl),
            Publication = ParsePublication(recordEl),
            Edition = NullIfEmpty(First(recordEl, "250", "a")),
            Extent = NullIfEmpty(First(recordEl, "300", "a")),
            Description = ParseDescription(recordEl),
            Contributors = ParseContributors(recordEl),
            // Series, Keywords, Genres, Subjects, MarcSource — added in later tasks
            MarcSource = BuildMarcSourceUrl(dnbId)
        };
        return record;
    }

    private static List<string> ParseIsbns(XElement recordEl)
    {
        var raw = DataFields(recordEl, "020")
            .SelectMany(df => Subs(df, "a"))
            .Concat(DataFields(recordEl, "020").SelectMany(df => Subs(df, "z")))
            .Select(NormalizeIsbn)
            .Where(s => s.Length > 0)
            .ToList();
        // Fall back to $9 if no $a/$z
        if (raw.Count == 0)
        {
            raw = DataFields(recordEl, "020")
                .SelectMany(df => Subs(df, "9"))
                .Select(NormalizeIsbn)
                .Where(s => s.Length > 0)
                .ToList();
        }
        return raw.Distinct().ToList();
    }

    private static Title ParseTitle(XElement recordEl)
    {
        var d245 = DataFields(recordEl, "245").FirstOrDefault();
        var d240 = DataFields(recordEl, "240").FirstOrDefault();
        return new Title
        {
            Main = TrimIsbd(SubFirst(d245, "a")),
            Subtitle = NullIfEmpty(TrimIsbd(SubFirst(d245, "b"))),
            PartNumber = JoinIfAny(Subs(d245, "n")),
            PartName = JoinIfAny(Subs(d245, "p")),
            Uniform = NullIfEmpty(SubFirst(d240, "a")),
            StatementOfResponsibility = NullIfEmpty(TrimIsbd(SubFirst(d245, "c")))
        };
    }

    private static Languages ParseLanguages(XElement recordEl)
    {
        var d041 = DataFields(recordEl, "041").FirstOrDefault();
        return new Languages
        {
            Publication = d041 == null ? new List<string>() : Subs(d041, "a").ToList(),
            Original = d041 == null ? new List<string>() : Subs(d041, "h").ToList()
        };
    }

    private static Publication ParsePublication(XElement recordEl)
    {
        // Prefer ind2=1 (publication); fall back to ind2=4 (copyright) for date only.
        var d264Pub = DataFields(recordEl, "264").FirstOrDefault(f => f.Attribute("ind2")?.Value == "1");
        var d264Cop = DataFields(recordEl, "264").FirstOrDefault(f => f.Attribute("ind2")?.Value == "4");
        return new Publication
        {
            Place = NullIfEmpty(SubFirst(d264Pub, "a")),
            Publisher = NullIfEmpty(SubFirst(d264Pub, "b")),
            Date = NullIfEmpty(SubFirst(d264Pub, "c")) ?? NullIfEmpty(SubFirst(d264Cop, "c"))
        };
    }

    private static string? ParseDescription(XElement recordEl)
    {
        var d520 = DataFields(recordEl, "520").FirstOrDefault();
        if (d520 == null) return null;
        var a = SubFirst(d520, "a");
        var b = SubFirst(d520, "b");
        var joined = string.Join(" ", new[] { a, b }.Where(s => !string.IsNullOrEmpty(s)));
        return string.IsNullOrEmpty(joined) ? null : joined;
    }

    private static List<Contributor> ParseContributors(XElement r)
    {
        var list = new List<Contributor>();
        foreach (var tag in new[] { "100", "700" })
        {
            foreach (var df in DataFields(r, tag))
            {
                var name = SubFirst(df, "a");
                if (string.IsNullOrEmpty(name)) continue;
                list.Add(new Contributor
                {
                    Name = name.Normalize(),
                    Role = Subs(df, "4").FirstOrDefault() ?? "",
                    RoleLabel = NullIfEmpty(SubFirst(df, "e")),
                    GndId = ExtractGndId(string.Join(" ", Subs(df, "0")))
                });
            }
        }
        return list;
    }

    internal static string? ExtractGndId(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;
        foreach (var token in raw.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.StartsWith("(DE-588)", StringComparison.Ordinal))
                return token["(DE-588)".Length..];
            if (token.StartsWith("https://d-nb.info/gnd/", StringComparison.Ordinal))
                return token["https://d-nb.info/gnd/".Length..];
            if (token.StartsWith("(DE-101)", StringComparison.Ordinal))
                return token["(DE-101)".Length..];
        }
        return null;
    }

    internal static string BuildMarcSourceUrl(string dnbId)
    {
        var encoded = Uri.EscapeDataString($"IDN={dnbId}");
        return $"https://services.dnb.de/sru/dnb?version=1.1&operation=searchRetrieve&query={encoded}&recordSchema=MARC21-xml";
    }

    // ------------------- low-level XML helpers --------------------

    private static IEnumerable<XElement> DataFields(XElement r, string tag)
        => r.Elements(MarcXmlConstants.Marc + "datafield").Where(e => e.Attribute("tag")?.Value == tag);

    private static IEnumerable<string> Subs(XElement? df, string code)
    {
        if (df == null) yield break;
        foreach (var sub in df.Elements(MarcXmlConstants.Marc + "subfield"))
        {
            if (sub.Attribute("code")?.Value == code)
            {
                var v = sub.Value;
                if (!string.IsNullOrEmpty(v)) yield return v;
            }
        }
    }

    private static string SubFirst(XElement? df, string code) => Subs(df, code).FirstOrDefault() ?? "";

    private static string First(XElement r, string tag, string code)
        => Subs(DataFields(r, tag).FirstOrDefault(), code).FirstOrDefault() ?? "";

    private static string? NullIfEmpty(string s) => string.IsNullOrEmpty(s) ? null : s;

    private static string? JoinIfAny(IEnumerable<string> values)
    {
        var arr = values.ToArray();
        return arr.Length == 0 ? null : string.Join(", ", arr);
    }

    private static string NormalizeIsbn(string raw)
    {
        var sb = new System.Text.StringBuilder(raw.Length);
        foreach (var c in raw)
        {
            if (char.IsDigit(c)) sb.Append(c);
            else if (c == 'X' || c == 'x') sb.Append(char.ToUpperInvariant(c));
        }
        return sb.ToString();
    }

    private static string TrimIsbd(string s)
    {
        // Strip trailing ISBD field-separator marks: " /", " :", " =", " ;", " ,"
        if (string.IsNullOrEmpty(s)) return s;
        var trimmed = s.TrimEnd();
        foreach (var marker in new[] { " /", " :", " =", " ;", " ," })
        {
            while (trimmed.EndsWith(marker, StringComparison.Ordinal))
                trimmed = trimmed[..^marker.Length].TrimEnd();
        }
        return trimmed;
    }

    internal static void ThrowIfDiagnostic(XDocument doc)
    {
        // SRW/diagnostic and DNB-specific diagnostic both surface inside <diagnostics>.
        var diagnostic = doc.Descendants(MarcXmlConstants.DiagSrw + "diagnostic").FirstOrDefault()
                      ?? doc.Descendants(MarcXmlConstants.DiagDnb + "diagnostic").FirstOrDefault();
        if (diagnostic == null) return;
        var message = (string?)diagnostic.Element(MarcXmlConstants.DiagSrw + "message")
                   ?? (string?)diagnostic.Element(MarcXmlConstants.DiagDnb + "message")
                   ?? "DNB returned a diagnostic response";
        var details = (string?)diagnostic.Element(MarcXmlConstants.DiagSrw + "details")
                   ?? (string?)diagnostic.Element(MarcXmlConstants.DiagDnb + "details");
        var full = details is null ? message : $"{message} ({details})";
        throw new DnbUpstreamException($"DNB returned diagnostic: {full}");
    }
}

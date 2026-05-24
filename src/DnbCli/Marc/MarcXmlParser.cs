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
        // Implemented in later tasks
        throw new NotImplementedException();
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

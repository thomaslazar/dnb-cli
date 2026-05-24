using System.Xml.Linq;

namespace DnbCli.Marc;

internal static class MarcXmlConstants
{
    public static readonly XNamespace Srw = "http://www.loc.gov/zing/srw/";
    public static readonly XNamespace Marc = "http://www.loc.gov/MARC21/slim";
    public static readonly XNamespace DiagSrw = "http://www.loc.gov/zing/srw/diagnostic/";
    public static readonly XNamespace DiagDnb = "http://services.dnb.de/sru/dnb/diag";
}

// tests/DnbCli.Tests/Marc/MarcXmlParserDiagnosticTests.cs
using DnbCli.Dnb;
using DnbCli.Marc;

namespace DnbCli.Tests.Marc;

public class MarcXmlParserDiagnosticTests
{
    [Fact]
    public void ParseSearchResponse_throws_DnbUpstreamException_when_diagnostic_present()
    {
        var xml = File.ReadAllText("fixtures/diagnostic.xml");
        var ex = Assert.Throws<DnbUpstreamException>(
            () => MarcXmlParser.ParseSearchResponse(xml, query: "BAD"));
        Assert.Contains("unsupported index", ex.Message);
    }
}

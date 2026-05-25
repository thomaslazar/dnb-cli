// tests/DnbCli.Tests/Marc/MarcXmlParserKeywordsTests.cs
using System.Xml.Linq;
using DnbCli.Marc;

namespace DnbCli.Tests.Marc;

public class MarcXmlParserKeywordsTests
{
    [Fact]
    public void Butcher_blendwerk_15_keywords_kept_raw_including_prefixed_entries()
    {
        var xml = File.ReadAllText("fixtures/butcher-blendwerk-15.xml");
        var r = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1314588753");
        Assert.Contains("krimi", r.Keywords);
        Assert.Contains("fantasy", r.Keywords);
        Assert.Contains("(Produktform)Downloadable audio file", r.Keywords);
        Assert.Contains("(BISAC Subject Heading)FIC009060", r.Keywords);
    }

    [Fact]
    public void Butcher_blendwerk_15_subjects_is_empty()
    {
        var xml = File.ReadAllText("fixtures/butcher-blendwerk-15.xml");
        var r = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1314588753");
        Assert.Empty(r.Subjects);
    }

    [Fact]
    public void Butcher_blendwerk_15_marcSource_url_is_constructed_correctly()
    {
        var xml = File.ReadAllText("fixtures/butcher-blendwerk-15.xml");
        var r = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1314588753");
        Assert.Equal(
            "https://services.dnb.de/sru/dnb?version=1.1&operation=searchRetrieve&query=IDN%3D1314588753&recordSchema=MARC21-xml",
            r.MarcSource);
    }
}

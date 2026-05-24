// tests/DnbCli.Tests/Marc/MarcXmlParserKeywordsTests.cs
using System.Xml.Linq;
using DnbCli.Marc;

namespace DnbCli.Tests.Marc;

public class MarcXmlParserKeywordsTests
{
    [Fact]
    public void YoRHa_vol4_ebook_keywords_kept_raw_including_prefixed_entries()
    {
        var xml = File.ReadAllText("fixtures/yorha-vol4-ebook.xml");
        var r = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1356869467");
        Assert.Contains("Krieg", r.Keywords);
        Assert.Contains("Yoko Taro", r.Keywords);
        Assert.Contains("(Produktform)Electronic book text", r.Keywords);
        Assert.Contains("(Zielgruppe)ab 16 Jahre", r.Keywords);
    }

    [Fact]
    public void YoRHa_vol4_ebook_subjects_is_empty_for_manga()
    {
        var xml = File.ReadAllText("fixtures/yorha-vol4-ebook.xml");
        var r = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1356869467");
        Assert.Empty(r.Subjects);
    }

    [Fact]
    public void YoRHa_vol4_ebook_marcSource_url_is_constructed_correctly()
    {
        var xml = File.ReadAllText("fixtures/yorha-vol4-ebook.xml");
        var r = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1356869467");
        Assert.Equal(
            "https://services.dnb.de/sru/dnb?version=1.1&operation=searchRetrieve&query=IDN%3D1356869467&recordSchema=MARC21-xml",
            r.MarcSource);
    }
}

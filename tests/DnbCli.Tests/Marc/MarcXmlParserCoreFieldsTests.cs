// tests/DnbCli.Tests/Marc/MarcXmlParserCoreFieldsTests.cs
using System.Xml.Linq;
using DnbCli.Marc;
using DnbCli.Models;

namespace DnbCli.Tests.Marc;

public class MarcXmlParserCoreFieldsTests
{
    private static DnbRecord ParseFixture(string filename, string dnbId)
    {
        var xml = File.ReadAllText($"fixtures/{filename}");
        var el = XElement.Parse(xml);
        return MarcXmlParser.ParseRecord(el, dnbId);
    }

    [Fact]
    public void YoRHa_vol4_ebook_dnbId_and_isbns()
    {
        var r = ParseFixture("yorha-vol4-ebook.xml", "1356869467");
        Assert.Equal("1356869467", r.DnbId);
        Assert.Contains("9783753931104", r.Isbns);
    }

    [Fact]
    public void YoRHa_vol4_ebook_title_block()
    {
        var r = ParseFixture("yorha-vol4-ebook.xml", "1356869467");
        Assert.Equal("YoRHa - Abstieg 11941 04", r.Title.Main);
        Assert.Equal("Eine NieR:Automata Story", r.Title.Subtitle);
        Assert.Equal("YoRHa Shinjuwan Koka Sakusen Kiroku 04", r.Title.Uniform);
        Assert.Equal("Taro Yoko, Megumu Soramichi", r.Title.StatementOfResponsibility);
    }

    [Fact]
    public void YoRHa_vol4_ebook_languages()
    {
        var r = ParseFixture("yorha-vol4-ebook.xml", "1356869467");
        Assert.Equal(new List<string> { "ger" }, r.Languages.Publication);
        Assert.Equal(new List<string> { "jpn" }, r.Languages.Original);
    }

    [Fact]
    public void YoRHa_vol4_ebook_publication_and_extent_and_edition()
    {
        var r = ParseFixture("yorha-vol4-ebook.xml", "1356869467");
        Assert.Equal("Hamburg", r.Publication.Place);
        Assert.Equal("Altraverse", r.Publication.Publisher);
        Assert.Equal("2025", r.Publication.Date);
        Assert.Equal("Online-Ressource, 228 Seiten", r.Extent);
        // YoRHa Bd 4 e-book has no 250 — older print editions do
        Assert.Null(r.Edition);
    }

    [Fact]
    public void YoRHa_vol4_ebook_no_description()
    {
        var r = ParseFixture("yorha-vol4-ebook.xml", "1356869467");
        Assert.Null(r.Description);
    }

    [Fact]
    public void YoRHa_vol4_ebook_strips_ISBD_trailing_punctuation_from_title_main()
    {
        var r = ParseFixture("yorha-vol4-ebook.xml", "1356869467");
        // Common DNB tail: "...Abstieg 11941 04 /" → trim trailing ` /`
        Assert.DoesNotMatch("[ /:=;,]\\s*$", r.Title.Main);
    }
}

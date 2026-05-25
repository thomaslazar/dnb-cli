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
    public void Butcher_blendwerk_15_dnbId_and_isbns()
    {
        var r = ParseFixture("butcher-blendwerk-15.xml", "1314588753");
        Assert.Equal("1314588753", r.DnbId);
        Assert.Contains("9783837165890", r.Isbns);
    }

    [Fact]
    public void Butcher_blendwerk_15_title_block()
    {
        var r = ParseFixture("butcher-blendwerk-15.xml", "1314588753");
        Assert.Equal("Die dunklen Fälle des Harry Dresden - Blendwerk", r.Title.Main);
        Assert.Equal("Roman", r.Title.Subtitle);
        Assert.Equal("Skin Game (The Dresden Files 15) (Penguin RoC, New York 2014)", r.Title.Uniform);
        Assert.Equal("Jim Butcher", r.Title.StatementOfResponsibility);
    }

    [Fact]
    public void Butcher_blendwerk_15_languages()
    {
        var r = ParseFixture("butcher-blendwerk-15.xml", "1314588753");
        Assert.Equal(new List<string> { "ger" }, r.Languages.Publication);
        Assert.Equal(new List<string> { "eng" }, r.Languages.Original);
    }

    [Fact]
    public void Butcher_blendwerk_15_publication_and_extent_and_edition()
    {
        var r = ParseFixture("butcher-blendwerk-15.xml", "1314588753");
        Assert.Equal("München", r.Publication.Place);
        Assert.Equal("Random House Audio", r.Publication.Publisher);
        Assert.Equal("2024", r.Publication.Date);
        Assert.Equal("Online-Ressource", r.Extent);
        Assert.Equal("Ungekürzte Lesung, ungekürzte Ausgabe", r.Edition);
    }

    [Fact]
    public void Butcher_blendwerk_15_no_description()
    {
        var r = ParseFixture("butcher-blendwerk-15.xml", "1314588753");
        Assert.Null(r.Description);
    }

    [Fact]
    public void Butcher_blendwerk_15_strips_ISBD_trailing_punctuation_from_title_main()
    {
        var r = ParseFixture("butcher-blendwerk-15.xml", "1314588753");
        // Common DNB tail: "...Blendwerk /" → trim trailing ` /`
        Assert.DoesNotMatch("[ /:=;,]\\s*$", r.Title.Main);
    }
}

// tests/DnbCli.Tests/Marc/MarcXmlParserContributorsTests.cs
using System.Xml.Linq;
using DnbCli.Marc;

namespace DnbCli.Tests.Marc;

public class MarcXmlParserContributorsTests
{
    [Fact]
    public void YoRHa_vol4_ebook_contributors_with_roles_and_gnd_ids()
    {
        var xml = File.ReadAllText("fixtures/yorha-vol4-ebook.xml");
        var record = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1356869467");

        Assert.Equal(3, record.Contributors.Count);
        var yokoo = record.Contributors[0];
        Assert.Equal("Yokoo, Tarō", yokoo.Name);
        Assert.Equal("aut", yokoo.Role);
        Assert.Equal("Verfasser", yokoo.RoleLabel);
        Assert.Equal("1253467463", yokoo.GndId);

        var translator = record.Contributors.FirstOrDefault(c => c.Role == "trl");
        Assert.NotNull(translator);
        Assert.Equal("Lange, Markus", translator!.Name);
        Assert.Null(translator.GndId);
    }

    [Theory]
    [InlineData("(DE-588)1253467463", "1253467463")]
    [InlineData("https://d-nb.info/gnd/1253467463", "1253467463")]
    [InlineData("(DE-101)1253467463", "1253467463")]
    [InlineData("", null)]
    [InlineData("nonsense", null)]
    public void ExtractGndId_handles_all_observed_forms(string input, string? expected)
    {
        Assert.Equal(expected, MarcXmlParser.ExtractGndId(input));
    }
}

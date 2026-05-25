// tests/DnbCli.Tests/Marc/MarcXmlParserContributorsTests.cs
using System.Xml.Linq;
using DnbCli.Marc;

namespace DnbCli.Tests.Marc;

public class MarcXmlParserContributorsTests
{
    [Fact]
    public void Butcher_blendwerk_15_contributors_with_roles()
    {
        var xml = File.ReadAllText("fixtures/butcher-blendwerk-15.xml");
        var record = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1314588753");

        Assert.Equal(3, record.Contributors.Count);
        var butcher = record.Contributors[0];
        Assert.Equal("Butcher, Jim", butcher.Name);
        Assert.Equal("aut", butcher.Role);
        Assert.Equal("Verfasser", butcher.RoleLabel);
        Assert.Null(butcher.GndId);

        var translator = record.Contributors.FirstOrDefault(c => c.Role == "trl");
        Assert.NotNull(translator);
        Assert.Equal("Heinrici, Dominik", translator!.Name);
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

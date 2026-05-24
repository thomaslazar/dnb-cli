// tests/DnbCli.Tests/Marc/MarcXmlParserSearchTests.cs
using DnbCli.Marc;

namespace DnbCli.Tests.Marc;

public class MarcXmlParserSearchTests
{
    [Fact]
    public void ParseSearchResponse_returns_full_envelope_for_yorha_query()
    {
        var xml = File.ReadAllText("fixtures/yorha.xml");
        var env = MarcXmlParser.ParseSearchResponse(xml, query: "TIT=YoRHa and TIT=Abstieg", page: 1, limit: 5);
        Assert.True(env.TotalResults >= env.ReturnedResults);
        Assert.Equal(env.Results.Count, env.ReturnedResults);
        Assert.Equal(1, env.Page);
        Assert.Equal(5, env.Limit);
        Assert.Equal("TIT=YoRHa and TIT=Abstieg", env.Query);
        // Spot-check first record came through
        Assert.False(string.IsNullOrEmpty(env.Results[0].DnbId));
    }

    [Fact]
    public void ParseSearchResponse_zero_results_returns_empty_envelope()
    {
        var xml = File.ReadAllText("fixtures/empty.xml");
        var env = MarcXmlParser.ParseSearchResponse(xml, query: "TIT=DEFINITELY-NOT-A-THING", page: 1, limit: 20);
        Assert.Equal(0, env.TotalResults);
        Assert.Empty(env.Results);
    }
}

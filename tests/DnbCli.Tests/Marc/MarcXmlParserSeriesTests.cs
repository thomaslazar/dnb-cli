// tests/DnbCli.Tests/Marc/MarcXmlParserSeriesTests.cs
using System.Xml.Linq;
using DnbCli.Marc;

namespace DnbCli.Tests.Marc;

public class MarcXmlParserSeriesTests
{
    [Fact]
    public void Butcher_blendwerk_15_has_one_series_entry_volume_15()
    {
        var xml = File.ReadAllText("fixtures/butcher-blendwerk-15.xml");
        var r = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1314588753");
        Assert.Single(r.Series);
        Assert.Equal("Die Harry-Dresden-Serie", r.Series[0].Name);
        Assert.Equal("15", r.Series[0].Volume);
    }

    [Fact]
    public void DedupeSeries_collapses_case_and_whitespace_duplicates()
    {
        var input = new List<DnbCli.Models.SeriesEntry>
        {
            new() { Name = "Die Harry-Dresden-Serie",   Volume = "15" },
            new() { Name = "die  harry-dresden-serie",  Volume = "15" },
            new() { Name = "Die Harry-Dresden-Serie",   Volume = "16" },
        };
        var deduped = MarcXmlParser.DedupeSeries(input);
        Assert.Equal(2, deduped.Count);
    }
}

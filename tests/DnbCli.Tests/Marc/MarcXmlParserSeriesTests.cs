// tests/DnbCli.Tests/Marc/MarcXmlParserSeriesTests.cs
using System.Xml.Linq;
using DnbCli.Marc;

namespace DnbCli.Tests.Marc;

public class MarcXmlParserSeriesTests
{
    [Fact]
    public void YoRHa_vol4_ebook_has_one_series_entry_volume_4()
    {
        var xml = File.ReadAllText("fixtures/yorha-vol4-ebook.xml");
        var r = MarcXmlParser.ParseRecord(XElement.Parse(xml), "1356869467");
        Assert.Single(r.Series);
        Assert.Equal("YoRHa - Abstieg 11941", r.Series[0].Name);
        Assert.Equal("4", r.Series[0].Volume);
    }

    [Fact]
    public void DedupeSeries_collapses_case_and_whitespace_duplicates()
    {
        var input = new List<DnbCli.Models.SeriesEntry>
        {
            new() { Name = "YoRHa - Abstieg 11941",   Volume = "4" },
            new() { Name = "yorha  -  abstieg 11941", Volume = "4" },
            new() { Name = "YoRHa - Abstieg 11941",   Volume = "5" },
        };
        var deduped = MarcXmlParser.DedupeSeries(input);
        Assert.Equal(2, deduped.Count);
    }
}

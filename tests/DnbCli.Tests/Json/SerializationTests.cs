// tests/DnbCli.Tests/Json/SerializationTests.cs
using System.Text.Json;
using DnbCli.Models;

namespace DnbCli.Tests.Json;

public class SerializationTests
{
    [Fact]
    public void DnbRecord_round_trips_with_all_fields_set()
    {
        var record = new DnbRecord
        {
            DnbId = "1314588753",
            Isbns = new List<string> { "9783837165890" },
            Title = new Title
            {
                Main = "Die dunklen Fälle des Harry Dresden - Blendwerk",
                Subtitle = "Roman",
                PartNumber = null,
                PartName = null,
                Uniform = "Skin Game (The Dresden Files 15) (Penguin RoC, New York 2014)",
                StatementOfResponsibility = "Jim Butcher"
            },
            Series = new List<SeriesEntry>
            {
                new() { Name = "Die Harry-Dresden-Serie", Volume = "15" }
            },
            Languages = new Languages
            {
                Publication = new List<string> { "ger" },
                Original = new List<string> { "eng" }
            },
            Contributors = new List<Contributor>
            {
                new() { Name = "Butcher, Jim", Role = "aut", RoleLabel = "Verfasser", GndId = null },
                new() { Name = "Heinrici, Dominik", Role = "trl", RoleLabel = "Übersetzer", GndId = null }
            },
            Publication = new Publication
            {
                Place = "München",
                Publisher = "Random House Audio",
                Date = "2024"
            },
            Edition = "Ungekürzte Lesung, ungekürzte Ausgabe",
            Extent = "Online-Ressource",
            Description = null,
            Genres = new List<string> { "Fantasy" },
            Subjects = new List<string>(),
            Keywords = new List<string> { "krimi", "fantasy" },
            MarcSource = "https://services.dnb.de/sru/dnb?version=1.1&operation=searchRetrieve&query=IDN%3D1314588753&recordSchema=MARC21-xml"
        };

        var json = JsonSerializer.Serialize(record, JsonContext.Default.DnbRecord);
        var back = JsonSerializer.Deserialize(json, JsonContext.Default.DnbRecord)!;

        Assert.Equal(record.DnbId, back.DnbId);
        Assert.Equal(record.Isbns, back.Isbns);
        Assert.Equal(record.Title.Main, back.Title.Main);
        Assert.Equal(record.Series.Count, back.Series.Count);
        Assert.Equal(record.Series[0].Volume, back.Series[0].Volume);
        Assert.Equal(record.Contributors[1].Role, back.Contributors[1].Role);
        Assert.Equal(record.Publication.Publisher, back.Publication.Publisher);
        Assert.Null(back.Description);
        Assert.Empty(back.Subjects);
        Assert.Equal(record.MarcSource, back.MarcSource);
    }

    [Fact]
    public void SearchEnvelope_round_trips()
    {
        var envelope = new SearchEnvelope
        {
            Query = "PER=Butcher",
            TotalResults = 9,
            ReturnedResults = 5,
            Page = 1,
            Limit = 5,
            Results = new List<DnbRecord>()
        };
        var json = JsonSerializer.Serialize(envelope, JsonContext.Default.SearchEnvelope);
        var back = JsonSerializer.Deserialize(json, JsonContext.Default.SearchEnvelope)!;
        Assert.Equal(9, back.TotalResults);
        Assert.Equal("PER=Butcher", back.Query);
        Assert.Empty(back.Results);
    }

    [Fact]
    public void DnbRecord_emits_camelCase_keys()
    {
        var record = new DnbRecord { DnbId = "x" };
        var json = JsonSerializer.Serialize(record, JsonContext.Default.DnbRecord);
        Assert.Contains("\"dnbId\"", json);
        Assert.DoesNotContain("\"DnbId\"", json);
    }
}

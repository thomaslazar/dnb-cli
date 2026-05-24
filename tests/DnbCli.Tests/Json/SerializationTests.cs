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
            DnbId = "1356869467",
            Isbns = new List<string> { "9783753931104" },
            Title = new Title
            {
                Main = "YoRHa - Abstieg 11941",
                Subtitle = "Eine NieR:Automata Story",
                PartNumber = null,
                PartName = null,
                Uniform = "YoRHa Shinjuwan Koka Sakusen Kiroku 04",
                StatementOfResponsibility = "Taro Yoko, Megumu Soramichi"
            },
            Series = new List<SeriesEntry>
            {
                new() { Name = "YoRHa - Abstieg 11941", Volume = "4" }
            },
            Languages = new Languages
            {
                Publication = new List<string> { "ger" },
                Original = new List<string> { "jpn" }
            },
            Contributors = new List<Contributor>
            {
                new() { Name = "Yokoo, Tarō", Role = "aut", RoleLabel = "Verfasser", GndId = "1253467463" },
                new() { Name = "Lange, Markus", Role = "trl", RoleLabel = "Übersetzer", GndId = null }
            },
            Publication = new Publication
            {
                Place = "Hamburg",
                Publisher = "Altraverse",
                Date = "2025"
            },
            Edition = "1. Auflage",
            Extent = "228 Seiten",
            Description = null,
            Genres = new List<string> { "Comic" },
            Subjects = new List<string>(),
            Keywords = new List<string> { "Krieg", "Yoko Taro" },
            MarcSource = "https://services.dnb.de/sru/dnb?version=1.1&operation=searchRetrieve&query=IDN%3D1356869467&recordSchema=MARC21-xml"
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
            Query = "TIT=Naruto",
            TotalResults = 9,
            ReturnedResults = 5,
            Page = 1,
            Limit = 5,
            Results = new List<DnbRecord>()
        };
        var json = JsonSerializer.Serialize(envelope, JsonContext.Default.SearchEnvelope);
        var back = JsonSerializer.Deserialize(json, JsonContext.Default.SearchEnvelope)!;
        Assert.Equal(9, back.TotalResults);
        Assert.Equal("TIT=Naruto", back.Query);
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

// tests/DnbCli.Tests/Output/ConsoleOutputTests.cs
using System.Text.Json;
using DnbCli.Models;
using DnbCli.Output;

namespace DnbCli.Tests.Output;

[Collection("NLog")]  // mutates Console.Out — must not race with other globals-touching tests
public class ConsoleOutputTests
{
    [Fact]
    public void WriteJson_emits_compact_by_default()
    {
        var sw = new StringWriter();
        var original = Console.Out;
        Console.SetOut(sw);
        ConsoleOutput.WriteJson(new DnbRecord { DnbId = "x" }, JsonContext.Default.DnbRecord, pretty: false);
        Console.SetOut(original);
        var output = sw.ToString().Trim();
        Assert.DoesNotContain("\n  ", output);
        Assert.Contains("\"dnbId\":\"x\"", output);
    }

    [Fact]
    public void WriteJson_pretty_indents()
    {
        var sw = new StringWriter();
        var original = Console.Out;
        Console.SetOut(sw);
        ConsoleOutput.WriteJson(new DnbRecord { DnbId = "x" }, JsonContext.Default.DnbRecord, pretty: true);
        Console.SetOut(original);
        var output = sw.ToString();
        Assert.Contains("\n  ", output);
    }

    [Fact]
    public void WriteNull_emits_literal_null_line()
    {
        var sw = new StringWriter();
        var original = Console.Out;
        Console.SetOut(sw);
        ConsoleOutput.WriteNull();
        Console.SetOut(original);
        Assert.Equal("null", sw.ToString().Trim());
    }
}

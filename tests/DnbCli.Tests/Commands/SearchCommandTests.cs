// tests/DnbCli.Tests/Commands/SearchCommandTests.cs
using System.CommandLine;
using System.Net;
using DnbCli.Commands;
using DnbCli.Dnb;
using DnbCli.Output;

namespace DnbCli.Tests.Commands;

[Collection("NLog")]  // calls LogSetup.Configure + Console.SetOut — serialise with other globals-touching tests
public class SearchCommandTests
{
    private static async Task<(int rc, string stdout)> RunAsync(Command cmd, params string[] args)
    {
        var sw = new StringWriter();
        var original = Console.Out;
        Console.SetOut(sw);
        try
        {
            var rc = await cmd.Parse(args).InvokeAsync();
            return (rc, sw.ToString());
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    [Fact]
    public async Task Search_with_no_flags_returns_exit_3()
    {
        LogSetup.Configure(verbose: false);
        var svc = new DnbService(new HttpClient(new YorhaHandler()), TimeSpan.FromSeconds(10));
        var cmd = SearchCommand.Create(() => svc);
        var (rc, _) = await RunAsync(cmd);
        Assert.Equal(ExitCodes.BadInput, rc);
    }

    [Fact]
    public async Task Search_with_title_returns_envelope_and_exit_0()
    {
        LogSetup.Configure(verbose: false);
        var svc = new DnbService(new HttpClient(new YorhaHandler()), TimeSpan.FromSeconds(10));
        var cmd = SearchCommand.Create(() => svc);
        var (rc, stdout) = await RunAsync(cmd, "--title", "YoRHa", "--limit", "5");
        Assert.Equal(ExitCodes.Hit, rc);
        Assert.Contains("\"results\"", stdout);
        Assert.Contains("\"totalResults\"", stdout);
    }

    [Fact]
    public async Task Search_with_zero_results_returns_exit_2_with_empty_envelope()
    {
        LogSetup.Configure(verbose: false);
        var svc = new DnbService(new HttpClient(new EmptyHandler()), TimeSpan.FromSeconds(10));
        var cmd = SearchCommand.Create(() => svc);
        var (rc, stdout) = await RunAsync(cmd, "--title", "NOTHING-MATCHES");
        Assert.Equal(ExitCodes.NoResults, rc);
        Assert.Contains("\"totalResults\":0", stdout);
        Assert.Contains("\"results\":[]", stdout);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("101")]
    public async Task Search_with_limit_out_of_range_returns_exit_3(string limit)
    {
        LogSetup.Configure(verbose: false);
        var svc = new DnbService(new HttpClient(new YorhaHandler()), TimeSpan.FromSeconds(10));
        var cmd = SearchCommand.Create(() => svc);
        var (rc, _) = await RunAsync(cmd, "--title", "X", "--limit", limit);
        Assert.Equal(ExitCodes.BadInput, rc);
    }

    private sealed class YorhaHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(File.ReadAllText("fixtures/yorha.xml")) });
    }
    private sealed class EmptyHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(File.ReadAllText("fixtures/empty.xml")) });
    }
}

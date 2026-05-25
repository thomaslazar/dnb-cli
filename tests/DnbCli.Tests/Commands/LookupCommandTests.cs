// tests/DnbCli.Tests/Commands/LookupCommandTests.cs
using System.CommandLine;
using System.Net;
using DnbCli.Commands;
using DnbCli.Dnb;
using DnbCli.Output;

namespace DnbCli.Tests.Commands;

[Collection("NLog")]  // calls LogSetup.Configure + Console.SetOut — serialise with other globals-touching tests
public class LookupCommandTests
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
    public async Task Lookup_with_no_flag_returns_exit_3()
    {
        LogSetup.Configure(verbose: false);
        var svc = new DnbService(new HttpClient(new ButcherHandler()), TimeSpan.FromSeconds(10));
        var cmd = LookupCommand.Create(() => svc);
        var (rc, _) = await RunAsync(cmd);
        Assert.Equal(ExitCodes.BadInput, rc);
    }

    [Fact]
    public async Task Lookup_isbn_hit_returns_exit_0_and_writes_json()
    {
        LogSetup.Configure(verbose: false);
        var svc = new DnbService(new HttpClient(new ButcherHandler()), TimeSpan.FromSeconds(10));
        var cmd = LookupCommand.Create(() => svc);
        var (rc, stdout) = await RunAsync(cmd, "--isbn", "9783837165890");
        Assert.Equal(ExitCodes.Hit, rc);
        Assert.Contains("\"dnbId\"", stdout);
    }

    [Fact]
    public async Task Lookup_isbn_miss_returns_exit_2_and_writes_null()
    {
        LogSetup.Configure(verbose: false);
        var svc = new DnbService(new HttpClient(new EmptyHandler()), TimeSpan.FromSeconds(10));
        var cmd = LookupCommand.Create(() => svc);
        var (rc, stdout) = await RunAsync(cmd, "--isbn", "0000000000000");
        Assert.Equal(ExitCodes.NoResults, rc);
        Assert.Equal("null", stdout.Trim());
    }

    private sealed class ButcherHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(File.ReadAllText("fixtures/butcher.xml")) });
    }
    private sealed class EmptyHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(File.ReadAllText("fixtures/empty.xml")) });
    }
}

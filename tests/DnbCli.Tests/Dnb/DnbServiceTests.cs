// tests/DnbCli.Tests/Dnb/DnbServiceTests.cs
using System.Net;
using DnbCli.Dnb;

namespace DnbCli.Tests.Dnb;

public class DnbServiceTests
{
    private static DnbService BuildServiceWithHandler(HttpStatusCode code, string body, TimeSpan? timeout = null)
    {
        var handler = new StubHandler(code, body);
        var http = new HttpClient(handler);
        return new DnbService(http, timeout ?? TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task LookupByIsbn_returns_record_when_DNB_returns_one_hit()
    {
        var xml = File.ReadAllText("fixtures/butcher.xml");
        var svc = BuildServiceWithHandler(HttpStatusCode.OK, xml);
        var record = await svc.LookupByIsbnAsync("9783837165890");
        Assert.NotNull(record);
        Assert.False(string.IsNullOrEmpty(record!.DnbId));
    }

    [Fact]
    public async Task LookupByIsbn_returns_null_when_DNB_returns_zero_hits()
    {
        var xml = File.ReadAllText("fixtures/empty.xml");
        var svc = BuildServiceWithHandler(HttpStatusCode.OK, xml);
        var record = await svc.LookupByIsbnAsync("0000000000000");
        Assert.Null(record);
    }

    [Fact]
    public async Task Lookup_throws_DnbUpstreamException_on_5xx()
    {
        var svc = BuildServiceWithHandler(HttpStatusCode.InternalServerError, "<error/>");
        await Assert.ThrowsAsync<DnbUpstreamException>(() => svc.LookupByIsbnAsync("9783837165890"));
    }

    [Fact]
    public async Task Lookup_throws_DnbNetworkException_on_HttpRequestException()
    {
        var handler = new ThrowingHandler(new HttpRequestException("dns failure"));
        var svc = new DnbService(new HttpClient(handler), TimeSpan.FromSeconds(10));
        await Assert.ThrowsAsync<DnbNetworkException>(() => svc.LookupByIsbnAsync("9783837165890"));
    }

    [Fact]
    public async Task Search_propagates_query_string_to_envelope()
    {
        var xml = File.ReadAllText("fixtures/butcher.xml");
        var svc = BuildServiceWithHandler(HttpStatusCode.OK, xml);
        var env = await svc.SearchAsync(title: "Blendwerk*", limit: 5, page: 1);
        Assert.Equal("TIT=Blendwerk*", env.Query);
        Assert.Equal(5, env.Limit);
        Assert.Equal(1, env.Page);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _code;
        private readonly string _body;
        public StubHandler(HttpStatusCode code, string body) { _code = code; _body = body; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
            => Task.FromResult(new HttpResponseMessage(_code) { Content = new StringContent(_body) });
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        private readonly Exception _ex;
        public ThrowingHandler(Exception ex) { _ex = ex; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
            => throw _ex;
    }
}

using DnbCli.Marc;
using DnbCli.Models;

namespace DnbCli.Dnb;

public sealed class DnbService
{
    private const string Endpoint = "https://services.dnb.de/sru/dnb";
    private readonly HttpClient _http;
    private readonly TimeSpan _timeout;

    public DnbService(HttpClient http, TimeSpan timeout)
    {
        _http = http;
        _timeout = timeout;
        if (_http.Timeout == Timeout.InfiniteTimeSpan || _http.Timeout > timeout)
            _http.Timeout = timeout;
        if (!_http.DefaultRequestHeaders.UserAgent.Any())
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("dnb-cli/0.1.0 (+https://github.com/thomaslazar/dnb-cli)");
    }

    public Task<DnbRecord?> LookupByIsbnAsync(string isbn, CancellationToken ct = default)
        => LookupAsync(CqlBuilder.ForIsbn(isbn), ct);

    public Task<DnbRecord?> LookupByDnbIdAsync(string dnbId, CancellationToken ct = default)
        => LookupAsync(CqlBuilder.ForDnbId(dnbId), ct);

    private async Task<DnbRecord?> LookupAsync(string cql, CancellationToken ct)
    {
        var xml = await GetXmlAsync(cql, maximumRecords: 1, startRecord: 1, ct);
        var env = MarcXmlParser.ParseSearchResponse(xml, cql);
        return env.Results.FirstOrDefault();
    }

    public async Task<SearchEnvelope> SearchAsync(
        string? title = null,
        string? author = null,
        string? year = null,
        string? series = null,
        string? any = null,
        int limit = 20,
        int page = 1,
        CancellationToken ct = default)
    {
        var cql = CqlBuilder.ForSearch(title, author, year, series, any);
        var startRecord = (page - 1) * limit + 1;
        var xml = await GetXmlAsync(cql, limit, startRecord, ct);
        return MarcXmlParser.ParseSearchResponse(xml, cql, page, limit);
    }

    private async Task<string> GetXmlAsync(string cql, int maximumRecords, int startRecord, CancellationToken ct)
    {
        var url = $"{Endpoint}?version=1.1&operation=searchRetrieve&query={Uri.EscapeDataString(cql)}&recordSchema=MARC21-xml&maximumRecords={maximumRecords}&startRecord={startRecord}";
        HttpResponseMessage resp;
        try
        {
            resp = await _http.GetAsync(url, ct);
        }
        catch (TaskCanceledException ex)
        {
            throw new DnbNetworkException($"Request timed out after {_timeout.TotalMilliseconds:F0} ms", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new DnbNetworkException(ex.Message, ex);
        }
        if ((int)resp.StatusCode >= 500)
            throw new DnbUpstreamException($"DNB returned HTTP {(int)resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
            throw new DnbUpstreamException($"DNB returned HTTP {(int)resp.StatusCode}");
        return await resp.Content.ReadAsStringAsync(ct);
    }
}

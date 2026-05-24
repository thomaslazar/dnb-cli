using System.CommandLine;
using DnbCli.Commands;
using DnbCli.Dnb;
using DnbCli.Output;

LogSetup.Configure(verbose: Environment.GetEnvironmentVariable("DNB_VERBOSE") == "1");

var timeoutMs = int.TryParse(Environment.GetEnvironmentVariable("DNB_TIMEOUT_MS"), out var ms) ? ms : 10_000;
var sharedHttp = new HttpClient();
DnbService MakeService() => new(sharedHttp, TimeSpan.FromMilliseconds(timeoutMs));

var rootCommand = new RootCommand("dnb-cli — Deutsche Nationalbibliothek metadata lookup")
{
    LookupCommand.Create(MakeService)
};

return await rootCommand.Parse(args).InvokeAsync();

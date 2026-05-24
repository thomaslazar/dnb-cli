using System.CommandLine;
using DnbCli.Commands;
using DnbCli.Dnb;
using DnbCli.Output;

var _logger = NLog.LogManager.GetLogger("DnbCli.Program");

var verboseOption = new Option<bool>("--verbose") { Description = "Enable debug-level logging to stderr." };
var timeoutOption = new Option<int?>("--timeout") { Description = "HTTP timeout in milliseconds (default 10000)." };

// timeoutMs is mutated after parsing but before any command actually runs.
// The factory closure captures the variable by reference, so subcommands
// see the post-parse value when they request a DnbService instance.
var timeoutMs = 10_000;
var sharedHttp = new HttpClient();
DnbService MakeService() => new(sharedHttp, TimeSpan.FromMilliseconds(timeoutMs));

var rootCommand = new RootCommand("dnb-cli — Deutsche Nationalbibliothek metadata lookup");
rootCommand.Options.Add(verboseOption);
rootCommand.Options.Add(timeoutOption);
rootCommand.Subcommands.Add(LookupCommand.Create(MakeService));
rootCommand.Subcommands.Add(SearchCommand.Create(MakeService));
rootCommand.Subcommands.Add(SelfTestCommand.Create());
rootCommand.Subcommands.Add(ChangelogCommand.Create());

rootCommand.AddHelpSection("Environment variables",
    "DNB_TIMEOUT_MS=<ms>   Same as --timeout. HTTP timeout per request.");
rootCommand.UseCustomHelpSections();

var parseResult = rootCommand.Parse(args);
var verbose = parseResult.GetValue(verboseOption);
timeoutMs = parseResult.GetValue(timeoutOption)
            ?? (int.TryParse(Environment.GetEnvironmentVariable("DNB_TIMEOUT_MS"), out var envMs) ? envMs : 10_000);
LogSetup.Configure(verbose);

try
{
    return await parseResult.InvokeAsync();
}
catch (Exception ex)
{
    _logger.Error(ex.Message);
    _logger.Debug(ex.ToString());
    ConsoleOutput.WriteNull();
    return ExitCodes.Generic;
}

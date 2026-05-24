using System.CommandLine;
using DnbCli.Dnb;
using DnbCli.Models;
using DnbCli.Output;

namespace DnbCli.Commands;

public static class LookupCommand
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public static Command Create(Func<DnbService> serviceFactory)
    {
        var isbnOption = new Option<string?>("--isbn") { Description = "Look up by ISBN (10 or 13 digits, hyphens optional)" };
        var idOption = new Option<string?>("--id") { Description = "Look up by DNB record ID (the 001 control field)" };
        var prettyOption = new Option<bool>("--pretty") { Description = "Pretty-print JSON output" };

        var command = new Command("lookup", "Look up a single record by ISBN or DNB-ID") { isbnOption, idOption, prettyOption };
        command.AddExamples(
            "dnb lookup --isbn 9783753931104",
            "dnb lookup --id 1356869467",
            "dnb lookup --isbn 978-3-7539-3110-4 --pretty");
        command.AddResponseExample<DnbRecord>();

        command.SetAction(async (parseResult, ct) =>
        {
            var isbn = parseResult.GetValue(isbnOption);
            var id = parseResult.GetValue(idOption);
            var pretty = parseResult.GetValue(prettyOption);

            if ((isbn == null && id == null) || (isbn != null && id != null))
            {
                _logger.Error("Specify exactly one of --isbn or --id.");
                ConsoleOutput.WriteNull();
                return ExitCodes.BadInput;
            }

            if (isbn != null && !CqlBuilder.IsValidIsbnShape(isbn))
            {
                _logger.Error($"Invalid ISBN shape: '{isbn}'.");
                ConsoleOutput.WriteNull();
                return ExitCodes.BadInput;
            }

            DnbRecord? record;
            try
            {
                var svc = serviceFactory();
                record = isbn != null
                    ? await svc.LookupByIsbnAsync(isbn, ct)
                    : await svc.LookupByDnbIdAsync(id!, ct);
            }
            catch (DnbNetworkException ex) { _logger.Error(ex.Message); ConsoleOutput.WriteNull(); return ExitCodes.Network; }
            catch (DnbUpstreamException ex) { _logger.Error(ex.Message); ConsoleOutput.WriteNull(); return ExitCodes.Upstream; }
            catch (Exception ex) { _logger.Error(ex.Message); ConsoleOutput.WriteNull(); return ExitCodes.Generic; }

            if (record == null)
            {
                _logger.Warn("no results");
                ConsoleOutput.WriteNull();
                return ExitCodes.NoResults;
            }
            ConsoleOutput.WriteJson(record, JsonContext.Default.DnbRecord, pretty);
            return ExitCodes.Hit;
        });
        return command;
    }
}

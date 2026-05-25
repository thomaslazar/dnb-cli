using System.CommandLine;
using DnbCli.Dnb;
using DnbCli.Models;
using DnbCli.Output;

namespace DnbCli.Commands;

public static class SearchCommand
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public static Command Create(Func<DnbService> serviceFactory)
    {
        var titleOption = new Option<string?>("--title") { Description = "Title (CQL TIT). Use trailing * for prefix match." };
        var contributorOption = new Option<string?>("--contributor") { Description = "Person in any contributor role — author, translator, illustrator, editor, or narrator (CQL PER)." };
        var yearOption = new Option<string?>("--year") { Description = "Year of publication (CQL JHR)." };
        var seriesOption = new Option<string?>("--series") { Description = "Series name (CQL WOE)." };
        var anyOption = new Option<string?>("--any") { Description = "Match any field (CQL WOE)." };
        var limitOption = new Option<int>("--limit") { Description = "Max records to return, 1-100 (default 20)." };
        var pageOption = new Option<int>("--page") { Description = "Page number, 1-based (default 1)." };
        var prettyOption = new Option<bool>("--pretty") { Description = "Pretty-print JSON output." };
        limitOption.DefaultValueFactory = _ => 20;
        pageOption.DefaultValueFactory = _ => 1;

        var command = new Command("search", "Search DNB by title, contributor, year, series, or any field")
        { titleOption, contributorOption, yearOption, seriesOption, anyOption, limitOption, pageOption, prettyOption };
        command.AddExamples(
            "dnb search --title \"Blendwerk*\" --contributor Butcher --limit 5",
            "dnb search --series \"Flüsse von London\" --pretty",
            "dnb search --any Dresden --year 2024 --page 2");
        command.AddResponseExample<SearchEnvelope>();

        command.SetAction(async (parseResult, ct) =>
        {
            var title = parseResult.GetValue(titleOption);
            var contributor = parseResult.GetValue(contributorOption);
            var year = parseResult.GetValue(yearOption);
            var series = parseResult.GetValue(seriesOption);
            var any = parseResult.GetValue(anyOption);
            var limit = parseResult.GetValue(limitOption);
            var page = parseResult.GetValue(pageOption);
            var pretty = parseResult.GetValue(prettyOption);

            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(contributor)
                && string.IsNullOrWhiteSpace(year) && string.IsNullOrWhiteSpace(series)
                && string.IsNullOrWhiteSpace(any))
            {
                _logger.Error("At least one of --title/--contributor/--year/--series/--any is required.");
                ConsoleOutput.WriteNull();
                return ExitCodes.BadInput;
            }
            if (limit is < 1 or > 100)
            {
                _logger.Error($"--limit must be between 1 and 100 (got {limit}).");
                ConsoleOutput.WriteNull();
                return ExitCodes.BadInput;
            }
            if (page < 1)
            {
                _logger.Error($"--page must be >= 1 (got {page}).");
                ConsoleOutput.WriteNull();
                return ExitCodes.BadInput;
            }

            SearchEnvelope envelope;
            try
            {
                envelope = await serviceFactory().SearchAsync(title, contributor, year, series, any, limit, page, ct);
            }
            catch (ArgumentException ex) { _logger.Error(ex.Message); ConsoleOutput.WriteNull(); return ExitCodes.BadInput; }
            catch (DnbNetworkException ex) { _logger.Error(ex.Message); ConsoleOutput.WriteNull(); return ExitCodes.Network; }
            catch (DnbUpstreamException ex) { _logger.Error(ex.Message); ConsoleOutput.WriteNull(); return ExitCodes.Upstream; }
            catch (Exception ex) { _logger.Error(ex.Message); ConsoleOutput.WriteNull(); return ExitCodes.Generic; }

            ConsoleOutput.WriteJson(envelope, JsonContext.Default.SearchEnvelope, pretty);
            if (envelope.ReturnedResults == 0)
            {
                _logger.Warn("no results");
                return ExitCodes.NoResults;
            }
            return ExitCodes.Hit;
        });
        return command;
    }
}

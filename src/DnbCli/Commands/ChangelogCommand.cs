using System.CommandLine;
using DnbCli.Services;

namespace DnbCli.Commands;

public static class ChangelogCommand
{
    public static Command Create()
    {
        var allOption = new Option<bool>("--all") { Description = "Print the entire CHANGELOG.md instead of just the latest entry" };
        var command = new Command("changelog", "Print release notes from the bundled CHANGELOG.md") { allOption };
        command.AddExamples("dnb changelog", "dnb changelog --all");
        command.SetAction(parseResult =>
        {
            var all = parseResult.GetValue(allOption);
            var output = all ? ChangelogReader.ReadAll() : ChangelogReader.ReadLatest();
            parseResult.InvocationConfiguration.Output.Write(output + "\n");
            return 0;
        });
        return command;
    }
}

using System.CommandLine;

var rootCommand = new RootCommand("dnb-cli — Deutsche Nationalbibliothek metadata lookup");

return await rootCommand.Parse(args).InvokeAsync();

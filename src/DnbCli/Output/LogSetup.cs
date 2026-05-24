using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace DnbCli.Output;

public static class LogSetup
{
    public static void Configure(bool verbose)
    {
        var config = new LoggingConfiguration();

        Layout layout = new SimpleLayout(
            "${date:format=yyyy-MM-ddTHH\\:mm\\:ss.fffZ:universalTime=true} ${level:uppercase=true:padding=-5} ${message}");

        var target = new ConsoleTarget("stderr")
        {
            StdErr = true,
            Layout = layout
        };
        config.AddTarget(target);

        var minLevel = verbose ? LogLevel.Debug : LogLevel.Warn;
        config.AddRule(minLevel, LogLevel.Fatal, target);

        LogManager.Configuration = config;
    }
}

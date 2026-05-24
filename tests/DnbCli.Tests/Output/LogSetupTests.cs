// tests/DnbCli.Tests/Output/LogSetupTests.cs
using DnbCli.Output;
using NLog;

namespace DnbCli.Tests.Output;

[Collection("NLog")]
public class LogSetupTests
{
    [Fact]
    public void Configure_default_sets_min_level_to_Warn()
    {
        LogSetup.Configure(verbose: false);
        var logger = LogManager.GetLogger("LogSetupTests");
        Assert.False(logger.IsDebugEnabled);
        Assert.True(logger.IsWarnEnabled);
    }

    [Fact]
    public void Configure_verbose_sets_min_level_to_Debug()
    {
        LogSetup.Configure(verbose: true);
        var logger = LogManager.GetLogger("LogSetupTests");
        Assert.True(logger.IsDebugEnabled);
    }
}

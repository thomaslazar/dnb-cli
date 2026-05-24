// tests/DnbCli.Tests/Services/ChangelogReaderTests.cs
using DnbCli.Services;

namespace DnbCli.Tests.Services;

public class ChangelogReaderTests
{
    [Fact]
    public void ReadAll_returns_nonempty_string_starting_with_h1()
    {
        var content = ChangelogReader.ReadAll();
        Assert.False(string.IsNullOrWhiteSpace(content));
        Assert.StartsWith("# Changelog", content);
    }

    [Fact]
    public void ReadLatest_returns_the_first_release_section()
    {
        var latest = ChangelogReader.ReadLatest();
        Assert.False(string.IsNullOrWhiteSpace(latest));
        Assert.StartsWith("## ", latest);
    }
}

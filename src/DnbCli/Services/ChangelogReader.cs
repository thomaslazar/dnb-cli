using System.Reflection;

namespace DnbCli.Services;

public static class ChangelogReader
{
    public static string ReadAll()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream("CHANGELOG.md")
            ?? throw new InvalidOperationException("CHANGELOG.md resource not embedded.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static string ReadLatest()
    {
        var all = ReadAll();
        var lines = all.Split('\n');
        var start = -1;
        var end = lines.Length;
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("## ", StringComparison.Ordinal))
            {
                if (start < 0) { start = i; continue; }
                end = i; break;
            }
        }
        if (start < 0) return all;
        return string.Join('\n', lines.Skip(start).Take(end - start)).TrimEnd();
    }
}

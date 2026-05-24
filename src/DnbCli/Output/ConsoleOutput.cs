using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DnbCli.Output;

public static class ConsoleOutput
{
    public static void WriteJson<T>(T value, JsonTypeInfo<T> typeInfo, bool pretty)
    {
        var options = pretty
            ? new JsonSerializerOptions(typeInfo.Options) { WriteIndented = true }
            : typeInfo.Options;
        var resolvedTypeInfo = pretty ? (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T)) : typeInfo;
        var json = JsonSerializer.Serialize(value, resolvedTypeInfo);
        Console.Out.WriteLine(json);
    }

    public static void WriteNull()
    {
        Console.Out.WriteLine("null");
    }
}

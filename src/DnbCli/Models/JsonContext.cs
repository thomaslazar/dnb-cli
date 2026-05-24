using System.Text.Json;
using System.Text.Json.Serialization;

namespace DnbCli.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(DnbRecord))]
[JsonSerializable(typeof(SearchEnvelope))]
[JsonSerializable(typeof(Title))]
[JsonSerializable(typeof(SeriesEntry))]
[JsonSerializable(typeof(Languages))]
[JsonSerializable(typeof(Contributor))]
[JsonSerializable(typeof(Publication))]
[JsonSerializable(typeof(List<DnbRecord>))]
public partial class JsonContext : JsonSerializerContext { }

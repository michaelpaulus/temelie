using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cornerstone.Database.Models;
public class ModelsJsonSerializerOptions
{
    public static JsonSerializerOptions Default => new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = ModelsJsonSerializerContext.Default
    };
}

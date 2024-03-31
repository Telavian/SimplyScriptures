using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimplyScriptures.Common.Extensions;

public static class SerializationExtensions
{
    #region Private Variables

    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
    {
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    #endregion

    public static string SerializeToJson<T>(this T item)
    {
        return JsonSerializer.Serialize(item, _options);
    }

    public static T? DeserializeFromJson<T>(this string? json)
    {
        return json switch
        {
            null => default,
            _ => JsonSerializer.Deserialize<T>(json, _options),
        };
    }

    public static T? DeserializeFromJson<T>(this byte[]? json)
    {
        return json switch
        {
            null => default,
            _ => JsonSerializer.Deserialize<T>(json, _options),
        };
    }

    public static ValueTask<T?> DeserializeFromJsonAsync<T>(this Stream? json)
    {
        return json switch
        {
            null => ValueTask.FromResult<T?>(default),
            _ => JsonSerializer.DeserializeAsync<T>(json, _options),
        };
    }
}

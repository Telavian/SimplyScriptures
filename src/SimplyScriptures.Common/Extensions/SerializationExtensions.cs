using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace SimplyScriptures.Common.Extensions;

public static class SerializationExtensions
{
    #region Private Variables

    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.General)
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
        switch (json)
        {
            case null:
                return default;
            default:
                return JsonSerializer.Deserialize<T>(json, _options);
        }
    }

    public static T? DeserializeFromJson<T>(this byte[]? json)
    {
        switch (json)
        {
            case null:
                return default;
            default:
                return JsonSerializer.Deserialize<T>(json, _options);
        }
    }

    public static ValueTask<T?> DeserializeFromJsonAsync<T>(this Stream? json)
    {
        switch (json)
        {
            case null:
                return ValueTask.FromResult<T?>(default);
            default:
                return JsonSerializer.DeserializeAsync<T>(json, _options);
        }
    }
}
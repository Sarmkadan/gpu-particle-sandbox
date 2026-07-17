using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GpuParticleSandbox;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="ParticleSystem"/> using System.Text.Json.
/// Serializes particle data to/from JSON format with camelCase property naming.
/// </summary>
public static class ParticleSystemJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes a <see cref="ParticleSystem"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The particle system to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the particle system.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ParticleSystem value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (indented)
        {
            var indentedOptions = new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true,
            };
            return JsonSerializer.Serialize(value, indentedOptions);
        }

        return JsonSerializer.Serialize(value, _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ParticleSystem"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized particle system, or <see langword="null"/> if the JSON is <see langword="null"/> or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ParticleSystem? FromJson(string json)
    {
        return string.IsNullOrEmpty(json)
            ? null
            : JsonSerializer.Deserialize<ParticleSystem>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ParticleSystem"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized particle system if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ParticleSystem? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<ParticleSystem>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
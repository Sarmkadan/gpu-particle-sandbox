using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GpuParticleSandbox;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="ShaderProgram"/>.
/// </summary>
public static class ShaderProgramJsonExtensions
{
	private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		WriteIndented = false
	};

	/// <summary>
	/// Serializes a <see cref="ShaderProgram"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The shader program to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the shader program.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this ShaderProgram value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		return JsonSerializer.Serialize(value, indented
			? new JsonSerializerOptions(_options) { WriteIndented = true }
			: _options);
	}

	/// <summary>
	/// Deserializes a JSON string into a <see cref="ShaderProgram"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized shader program, or <see langword="null"/> if <paramref name="json"/> is <see langword="null"/> or empty.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="JsonException">The JSON is invalid or cannot be deserialized.</exception>
	public static ShaderProgram? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return string.IsNullOrEmpty(json)
			? null
			: JsonSerializer.Deserialize<ShaderProgram?>(json, _options);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string into a <see cref="ShaderProgram"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized shader program if successful.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out ShaderProgram? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		if (string.IsNullOrEmpty(json))
		{
			return true;
		}

		try
		{
			value = JsonSerializer.Deserialize<ShaderProgram?>(json, _options);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}
using System.Collections.Generic;

namespace GpuParticleSandbox;

/// <summary>
/// Provides validation helpers for GLSL shader source strings.
/// </summary>
public static class ShaderSourceValidator
{
	/// <summary>
	/// Validates a GLSL source string for basic correctness.
	/// </summary>
	/// <param name="glslSource">The GLSL source code to validate.</param>
	/// <returns>A list of validation error messages; empty if the source is valid.</returns>
	public static IReadOnlyList<string> ValidateSource(string glslSource)
	{
		var errors = new List<string>();

		if (string.IsNullOrWhiteSpace(glslSource))
		{
			errors.Add("Source is empty.");
			return errors.AsReadOnly();
		}

		// Check for a #version directive
		var lines = glslSource.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
		bool hasVersion = false;
		foreach (var line in lines)
		{
			var trimmed = line.TrimStart();
			if (trimmed.StartsWith("#version"))
			{
				hasVersion = true;
				break;
			}
		}
		if (!hasVersion)
		{
			errors.Add("Missing #version directive.");
		}

		// Check for a main() function
		if (!glslSource.Contains("main("))
		{
			errors.Add("Missing main() function.");
		}

		// Check for balanced braces
		int balance = 0;
		foreach (char c in glslSource)
		{
			if (c == '{')
			{
				balance++;
			}
			else if (c == '}')
			{
				balance--;
				if (balance < 0)
				{
					errors.Add("Unbalanced braces: more closing braces than opening.");
					break;
				}
			}
		}
		if (balance > 0)
		{
			errors.Add("Unbalanced braces: more opening braces than closing.");
		}

		return errors.AsReadOnly();
	}
}

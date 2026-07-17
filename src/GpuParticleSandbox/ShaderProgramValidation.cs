using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GpuParticleSandbox;

/// <summary>
/// Provides validation helpers for <see cref="ShaderProgram"/> instances.
/// </summary>
public static class ShaderProgramValidation
{
    /// <summary>
    /// Validates that a <see cref="ShaderProgram"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The shader program to validate.</param>
    /// <returns>A list of validation messages; empty if the program is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate([NotNull] this ShaderProgram? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Handle
        if (value.Handle <= 0)
        {
            errors.Add($"ShaderProgram.Handle must be a positive integer, but was {value.Handle}.");
        }

        // Validate uniform cache consistency (indirect check via usage pattern)
        // The _uniformCache field is private, so we can't directly validate it,
        // but we can check that the program handle is valid for GL operations

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ShaderProgram"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The shader program to check.</param>
    /// <returns>True if the program is valid; otherwise, false.</returns>
    public static bool IsValid([NotNullWhen(true)] this ShaderProgram? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ShaderProgram"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The shader program to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the program is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid([NotNull] this ShaderProgram? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ShaderProgram is invalid. Validation errors:\n  - {
                    string.Join("\n  - ", errors)
                }");
        }
    }
}
using System;

namespace GpuParticleSandbox;

/// <summary>
/// Extension methods for <see cref="ColorMode"/> that cycle through modes and provide display names.
/// </summary>
public static class ColorModeExtensions
{
    /// <summary>
    /// Returns the next color mode in declaration order, wrapping from the last mode to the first.
    /// </summary>
    /// <param name="mode">The current color mode.</param>
    /// <returns>The following <see cref="ColorMode"/> value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> is not a defined <see cref="ColorMode"/> value.</exception>
    public static ColorMode Next(this ColorMode mode) => Step(mode, 1);

    /// <summary>
    /// Returns the previous color mode in declaration order, wrapping from the first mode to the last.
    /// </summary>
    /// <param name="mode">The current color mode.</param>
    /// <returns>The preceding <see cref="ColorMode"/> value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> is not a defined <see cref="ColorMode"/> value.</exception>
    public static ColorMode Previous(this ColorMode mode) => Step(mode, -1);

    /// <summary>
    /// Returns a human-readable name for the color mode, suitable for on-screen display.
    /// </summary>
    /// <param name="mode">The color mode.</param>
    /// <returns>The display name of the mode.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> is not a defined <see cref="ColorMode"/> value.</exception>
    public static string DisplayName(this ColorMode mode) => mode switch
    {
        ColorMode.Velocity => "Velocity",
        ColorMode.Age => "Age",
        ColorMode.Uniform => "Uniform",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported color mode.")
    };

    private static ColorMode Step(ColorMode mode, int delta)
    {
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported color mode.");

        var values = Enum.GetValues<ColorMode>();
        int count = values.Length;
        int index = Array.IndexOf(values, mode);
        return values[((index + delta) % count + count) % count];
    }
}

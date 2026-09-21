using System;
using System.Drawing;
using System.Numerics;

namespace GpuParticleSandbox
{
    /// <summary>
    /// Defines the color mode used for mapping particle attributes to RGB colors.
    /// </summary>
    public enum ColorMode
    {
        /// <summary>
        /// Color based on the particle's velocity magnitude (blue to red gradient).
        /// </summary>
        Velocity,

        /// <summary>
        /// Color based on the particle's age.
        /// </summary>
        Age,

        /// <summary>
        /// Uniform color for all particles.
        /// </summary>
        Uniform
    }

    /// <summary>
    /// Maps particle attributes to an RGB <see cref="Vector3"/> based on configured color modes.
    /// </summary>
    public sealed class ColorModeMapper
    {
        private readonly ColorMode _mode;
        private readonly Color[] _palette;
        private readonly (float T, Color Color)[] _gradientStops;
        private readonly Color _fallback;

        internal ColorModeMapper(ColorMode mode, Color[]? palette, (float T, Color Color)[]? gradientStops, Color fallback)
        {
            _mode = mode;
            _palette = palette ?? Array.Empty<Color>();
            _gradientStops = gradientStops ?? Array.Empty<(float, Color)>();
            _fallback = fallback;
        }

        /// <summary>
        /// Maps the given particle attributes to an RGB color based on the configured mode.
        /// </summary>
        public Vector3 Map(float velocityMagnitude, float age)
        {
            if (velocityMagnitude < 0f || velocityMagnitude > 1f)
                throw new ArgumentOutOfRangeException(nameof(velocityMagnitude), "Velocity magnitude must be in the range [0, 1].");
            if (age < 0f || age > 1f)
                throw new ArgumentOutOfRangeException(nameof(age), "Age must be in the range [0, 1].");

            return _mode switch
            {
                ColorMode.Velocity => MapVelocityToColor(velocityMagnitude),
                ColorMode.Age => new Vector3(0f, age, 0f),
                ColorMode.Uniform => new Vector3(_fallback.R / 255f, _fallback.G / 255f, _fallback.B / 255f),
                _ => throw new ArgumentOutOfRangeException(nameof(_mode), "Unsupported color mode.")
            };
        }

        private Vector3 MapVelocityToColor(float velocityMagnitude)
        {
            if (_gradientStops.Length >= 2)
            {
                return InterpolateGradient(velocityMagnitude);
            }
            if (_palette.Length >= 2)
            {
                return InterpolatePalette(velocityMagnitude);
            }
            // Default blue-to-red gradient
            float normalized = Math.Clamp(velocityMagnitude, 0f, 1f);
            return new Vector3(normalized, 0f, 1f - normalized);
        }

        private Vector3 InterpolateGradient(float t)
        {
            var stops = _gradientStops.OrderBy(s => s.T).ToArray();
            if (t <= stops[0].T) return stops[0].Color.ToVector3();
            if (t >= stops[^1].T) return stops[^1].Color.ToVector3();

            for (int i = 0; i < stops.Length - 1; i++)
            {
                if (t >= stops[i].T && t <= stops[i + 1].T)
                {
                    float localT = (t - stops[i].T) / (stops[i + 1].T - stops[i].T);
                    return Vector3.Lerp(stops[i].Color.ToVector3(), stops[i + 1].Color.ToVector3(), localT);
                }
            }
            return stops[^1].Color.ToVector3();
        }

        private Vector3 InterpolatePalette(float t)
        {
            float normalized = Math.Clamp(t, 0f, 1f);
            int count = _palette.Length;
            float segment = normalized * (count - 1);
            int i = (int)segment;
            float localT = segment - i;
            if (i >= count - 1) return _palette[^1].ToVector3();
            return Vector3.Lerp(_palette[i].ToVector3(), _palette[i + 1].ToVector3(), localT);
        }

        /// <summary>
        /// Creates a new fluent builder for configuring a <see cref="ColorModeMapper"/>.
        /// </summary>
        public static ColorModeMapperBuilder CreateBuilder() => new();
    }

    internal static class ColorExtensions
    {
        public static Vector3 ToVector3(this Color color) => new(color.R / 255f, color.G / 255f, color.B / 255f);
    }
}

using System;
using System.Drawing;
using System.Linq;

namespace GpuParticleSandbox
{
    /// <summary>
    /// Fluent builder for configuring and constructing a <see cref="ColorModeMapper"/> instance.
    /// </summary>
    public sealed class ColorModeMapperBuilder
    {
        private ColorMode _mode = ColorMode.Velocity;
        private Color[]? _palette;
        private (float T, Color Color)[]? _gradientStops;
        private Color? _fallback;

        /// <summary>
        /// Sets the color mapping mode.
        /// </summary>
        public ColorModeMapperBuilder WithMode(ColorMode mode)
        {
            _mode = mode;
            return this;
        }

        /// <summary>
        /// Sets a palette of colors to interpolate between.
        /// </summary>
        public ColorModeMapperBuilder WithPalette(params Color[] palette)
        {
            _palette = palette;
            return this;
        }

        /// <summary>
        /// Sets gradient stops with normalized positions and colors.
        /// </summary>
        public ColorModeMapperBuilder WithGradientStops(params (float T, Color Color)[] stops)
        {
            _gradientStops = stops;
            return this;
        }

        /// <summary>
        /// Sets the fallback color for uniform mode.
        /// </summary>
        public ColorModeMapperBuilder WithFallback(Color fallback)
        {
            _fallback = fallback;
            return this;
        }

        /// <summary>
        /// Validates the configuration and builds the <see cref="ColorModeMapper"/>.
        /// </summary>
        public ColorModeMapper Build()
        {
            if (_gradientStops != null)
            {
                foreach (var stop in _gradientStops)
                {
                    if (stop.T < 0f || stop.T > 1f)
                        throw new ArgumentException("Gradient stop T must be in the range [0, 1].", nameof(_gradientStops));
                }
                if (_gradientStops.Length < 2)
                    throw new ArgumentException("Gradient stops must contain at least two entries.", nameof(_gradientStops));
            }

            if (_palette != null && _palette.Length < 2)
                throw new ArgumentException("Palette must contain at least two colors.", nameof(_palette));

            if (_mode == ColorMode.Uniform && !_fallback.HasValue)
                throw new ArgumentException("Fallback color must be specified for Uniform mode.", nameof(_fallback));

            return new ColorModeMapper(_mode, _palette, _gradientStops, _fallback ?? Color.White);
        }
    }
}

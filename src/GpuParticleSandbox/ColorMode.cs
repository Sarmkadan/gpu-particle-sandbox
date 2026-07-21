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
    /// Provides mapping from particle attributes to an RGB <see cref="Vector3"/>.
    /// </summary>
    public static class ColorModeMapper
    {
        /// <summary>
        /// Maps the given particle attributes to an RGB color based on the specified <paramref name="mode"/>.
        /// </summary>
        /// <param name="velocityMagnitude">The magnitude of the particle's velocity.</param>
        /// <param name="age">The age of the particle.</param>
        /// <param name="mode">The color mode to use for mapping.</param>
        /// <returns>An RGB <see cref="Vector3"/> representing the color.</returns>
        public static Vector3 Map(float velocityMagnitude, float age, ColorMode mode)
        {
            return mode switch
            {
                ColorMode.Velocity => MapVelocityToColor(velocityMagnitude),
                ColorMode.Age => new Vector3(0f, age, 0f),
                ColorMode.Uniform => Vector3.One,
                _ => Vector3.Zero
            };
        }

        /// <summary>
        /// Maps velocity magnitude to a blue-to-red gradient.
        /// </summary>
        /// <param name="velocityMagnitude">The velocity magnitude to map.</param>
        /// <returns>A Vector3 representing the RGB color.</returns>
        private static Vector3 MapVelocityToColor(float velocityMagnitude)
        {
            // Normalize velocity magnitude to [0, 1] range
            // Blue (0, 0, 1) at low velocity, Red (1, 0, 0) at high velocity
            float normalizedVelocity = Math.Clamp(velocityMagnitude, 0f, 1f);
            return new Vector3(normalizedVelocity, 0f, 1f - normalizedVelocity);
        }
    }
}

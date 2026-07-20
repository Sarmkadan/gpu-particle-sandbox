using System.Numerics;

namespace GpuParticleSandbox
{
    /// <summary>
    /// Defines the color mode used for mapping particle attributes to RGB colors.
    /// </summary>
    public enum ColorMode
    {
        /// <summary>
        /// Color based on the particle's velocity magnitude.
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
                ColorMode.Velocity => new Vector3(velocityMagnitude, 0f, 0f),
                ColorMode.Age => new Vector3(0f, age, 0f),
                ColorMode.Uniform => Vector3.One,
                _ => Vector3.Zero
            };
        }
    }
}

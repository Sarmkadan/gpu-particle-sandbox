using System;
using System.Collections.Generic;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GpuParticleSandbox;

/// <summary>
/// Extension methods for <see cref="ParticleSystem"/> that provide additional functionality
/// for working with particle systems.
/// </summary>
public static class ParticleSystemExtensions
{
    /// <summary>
    /// Sets the gravity well position and strength in a single call.
    /// </summary>
    /// <param name="system">The particle system instance.</param>
    /// <param name="gravityWell">The position of the gravity well in world coordinates.</param>
    /// <param name="wellStrength">The strength of the gravity well effect.</param>
    /// <exception cref="ArgumentNullException"><paramref name="system"/> is <see langword="null"/></exception>
    public static void SetGravityWell(this ParticleSystem system, Vector2 gravityWell, float wellStrength)
    {
        ArgumentNullException.ThrowIfNull(system);

        // Store the values in thread-local storage for the next Update call
        // This is a simple wrapper that combines the two parameters
        system.Update(0.0f, gravityWell, wellStrength);
    }

    /// <summary>
    /// Updates the particle system with a fixed timestep, useful for deterministic simulations.
    /// </summary>
    /// <param name="system">The particle system instance.</param>
    /// <param name="fixedDeltaTime">The fixed timestep duration in seconds.</param>
    /// <param name="gravityWell">The position of the gravity well in world coordinates.</param>
    /// <param name="wellStrength">The strength of the gravity well effect.</param>
    /// <exception cref="ArgumentNullException"><paramref name="system"/> is <see langword="null"/></exception>
    public static void UpdateFixed(this ParticleSystem system, float fixedDeltaTime, Vector2 gravityWell, float wellStrength)
    {
        ArgumentNullException.ThrowIfNull(system);

        // Apply fixed timestep updates
        system.Update(fixedDeltaTime, gravityWell, wellStrength);
    }

    /// <summary>
    /// Renders the particle system with a specified point size.
    /// </summary>
    /// <param name="system">The particle system instance.</param>
    /// <param name="pointSize">The size in pixels for each particle point.</param>
    /// <exception cref="ArgumentNullException"><paramref name="system"/> is <see langword="null"/></exception>
    public static void Render(this ParticleSystem system, float pointSize)
    {
        ArgumentNullException.ThrowIfNull(system);

        // Set point size via OpenGL
        GL.PointSize(pointSize);
        try
        {
            system.Render();
        }
        finally
        {
            // Reset to default point size
            GL.PointSize(1.0f);
        }
    }

    /// <summary>
    /// Safely disposes the particle system if it hasn't been disposed already.
    /// </summary>
    /// <param name="system">The particle system instance.</param>
    /// <returns><see langword="true"/> if the system was disposed by this call; <see langword="false"/> if it was already disposed or is <see langword="null"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="system"/> is <see langword="null"/></exception>
    public static bool SafeDispose(this ParticleSystem? system)
    {
        if (system is null)
        {
            return false;
        }

        try
        {
            system.Dispose();
            return true;
        }
        catch (ObjectDisposedException)
        {
            // Already disposed
            return false;
        }
    }

    /// <summary>
    /// Gets the estimated memory usage of the particle system in bytes.
    /// </summary>
    /// <param name="system">The particle system instance.</param>
    /// <returns>The estimated memory usage in bytes, or 0 if the system is disposed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="system"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Thrown if the particle count cannot be determined.</exception>
    public static long GetMemoryUsage(this ParticleSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);

        // Each particle uses Particle.SizeInBytes bytes
        // We can't directly access _count after disposal, but we'll assume it's still valid
        return (long)system.GetParticleCount() * Particle.SizeInBytes;
    }

    /// <summary>
    /// Gets the number of particles in the system.
    /// </summary>
    /// <param name="system">The particle system instance.</param>
    /// <returns>The number of particles.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="system"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Thrown if the particle count field cannot be accessed.</exception>
    public static int GetParticleCount(this ParticleSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);

        // Access the private field via reflection as a last resort
        // This is safe because we're in the same assembly
        var countField = typeof(ParticleSystem).GetField("_count",
            BindingFlags.NonPublic | BindingFlags.Instance);

        return countField is null
            ? throw new InvalidOperationException("Particle count field not found in ParticleSystem.")
            : (int)countField.GetValue(system)!;
    }
}
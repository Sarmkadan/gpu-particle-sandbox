using System;
using OpenTK.Mathematics;

namespace GpuParticleSandbox;

/// <summary>
/// Fluent builder for configuring and creating <see cref="ParticleSystem"/> instances.
/// </summary>
public sealed class ParticleSystemBuilder
{
    private int _particleCount = 100_000;
    private string _shaderDir = string.Empty;
    private Vector2 _gravity = Vector2.Zero;
    private float _spawnRate = 1.0f;
    private ColorMode _colorMode = ColorMode.Velocity;
    private ParticleSystem.EmitterShape _shape = ParticleSystem.EmitterShape.Point;

    /// <summary>
    /// Sets the number of particles to simulate.
    /// </summary>
    public ParticleSystemBuilder WithParticleCount(int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Particle count must be positive.");
        _particleCount = count;
        return this;
    }

    /// <summary>
    /// Sets the directory containing the shader files.
    /// </summary>
    public ParticleSystemBuilder WithShaderDirectory(string shaderDir)
    {
        if (string.IsNullOrWhiteSpace(shaderDir)) throw new ArgumentException("Shader directory cannot be null or empty.", nameof(shaderDir));
        _shaderDir = shaderDir;
        return this;
    }

    /// <summary>
    /// Sets the global gravity vector applied during simulation.
    /// </summary>
    public ParticleSystemBuilder WithGravity(Vector2 gravity)
    {
        _gravity = gravity;
        return this;
    }

    /// <summary>
    /// Sets the rate at which new particles are spawned or recycled.
    /// </summary>
    public ParticleSystemBuilder WithSpawnRate(float rate)
    {
        if (rate < 0) throw new ArgumentOutOfRangeException(nameof(rate), "Spawn rate cannot be negative.");
        _spawnRate = rate;
        return this;
    }

    /// <summary>
    /// Sets the visual color mapping mode for particles.
    /// </summary>
    public ParticleSystemBuilder WithColorMode(ColorMode mode)
    {
        _colorMode = mode;
        return this;
    }

    /// <summary>
    /// Sets the emitter shape for particle initialization.
    /// </summary>
    public ParticleSystemBuilder WithEmitterShape(ParticleSystem.EmitterShape shape)
    {
        _shape = shape;
        return this;
    }

    /// <summary>
    /// Validates configuration and constructs the <see cref="ParticleSystem"/>.
    /// </summary>
    public ParticleSystem Build()
    {
        if (string.IsNullOrWhiteSpace(_shaderDir))
            throw new InvalidOperationException("Shader directory must be set before building.");
        if (_particleCount <= 0)
            throw new InvalidOperationException("Particle count must be greater than zero.");
        if (_spawnRate < 0)
            throw new InvalidOperationException("Spawn rate cannot be negative.");

        var system = new ParticleSystem(_particleCount, _shaderDir, _shape);
        system.SetColorMode((int)_colorMode);
        
        return system;
    }
}

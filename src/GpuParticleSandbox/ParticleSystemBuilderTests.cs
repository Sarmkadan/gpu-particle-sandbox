using System;
using OpenTK.Mathematics;

namespace GpuParticleSandbox;

/// <summary>
/// Hand-rolled unit tests for <see cref="ParticleSystemBuilder"/>.
/// </summary>
public static class ParticleSystemBuilderTests
{
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception($"Assertion failed: {message}");
    }

    /// <summary>Checks that a fully specified builder chain runs without throwing.</summary>
    public static void TestValidConfigurationChain()
    {
        var builder = new ParticleSystemBuilder()
            .WithParticleCount(50_000)
            .WithShaderDirectory("Shaders")
            .WithGravity(new Vector2(0.0f, -9.81f))
            .WithSpawnRate(2.5f)
            .WithColorMode(ColorMode.Velocity)
            .WithEmitterShape(ParticleSystem.EmitterShape.Circle);

        Assert(true, "Configuration chain executed successfully.");
    }

    /// <summary>Checks that a zero particle count is rejected.</summary>
    public static void TestInvalidParticleCountZero()
    {
        var builder = new ParticleSystemBuilder();
        try
        {
            builder.WithParticleCount(0);
            Assert(false, "Should have thrown ArgumentOutOfRangeException for zero particles.");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Expected
        }
    }

    /// <summary>Checks that a negative particle count is rejected.</summary>
    public static void TestInvalidParticleCountNegative()
    {
        var builder = new ParticleSystemBuilder();
        try
        {
            builder.WithParticleCount(-100);
            Assert(false, "Should have thrown ArgumentOutOfRangeException for negative particles.");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Expected
        }
    }

    /// <summary>Checks that a negative spawn rate is rejected.</summary>
    public static void TestNegativeSpawnRate()
    {
        var builder = new ParticleSystemBuilder();
        try
        {
            builder.WithSpawnRate(-0.1f);
            Assert(false, "Should have thrown ArgumentOutOfRangeException for negative spawn rate.");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Expected
        }
    }

    /// <summary>Checks that Build() rejects an invalid configuration.</summary>
    public static void TestBuildValidationThrows()
    {
        var builder = new ParticleSystemBuilder().WithParticleCount(0);
        try
        {
            builder.Build();
            Assert(false, "Build() should have thrown InvalidOperationException for invalid state.");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }

    /// <summary>Checks that Build() fails when the shader directory is missing.</summary>
    public static void TestMissingShaderDirectory()
    {
        var builder = new ParticleSystemBuilder().WithParticleCount(1000);
        try
        {
            builder.Build();
            Assert(false, "Build() should have thrown InvalidOperationException for missing shader directory.");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }
}

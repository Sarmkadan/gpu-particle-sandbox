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

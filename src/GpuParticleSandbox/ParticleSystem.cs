using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GpuParticleSandbox;

/// <summary>
/// Owns the particle SSBO and drives it: seed on the CPU once, then let the
/// compute shader integrate it every frame. Rendering reads the same buffer,
/// so the data never leaves VRAM after the initial upload.
/// </summary>
public sealed class ParticleSystem : IDisposable
{
    private const int LocalSize = 256; // must match layout(local_size_x) in the .comp

    public enum EmitterShape
    {
        Point,
        Circle,
        Line,
        Ring
    }

    /// <summary>
    /// Gravity well definition for CPU-side simulation.
    /// Position: world coordinates
    /// Strength: attraction force multiplier
    /// Radius: distance at which attraction starts (inverse-square law)
    /// </summary>
    public readonly struct GravityWell
    {
        public readonly Vector2 Position;
        public readonly float Strength;
        public readonly float Radius;

        public GravityWell(Vector2 position, float strength, float radius = 0.0f)
        {
            Position = position;
            Strength = strength;
            Radius = radius;
        }
    }

    private readonly int _count;
    private readonly int _ssbo;
    private readonly int _vao;
    private readonly EmitterShape _shape;
private int _colorMode = 0;

    private readonly ShaderProgram _compute;
    private readonly ShaderProgram _render;

    public ParticleSystem(int count, string shaderDir, EmitterShape shape = EmitterShape.Point)
    {
        _count = count;
        _shape = shape;

        _compute = ShaderProgram.FromCompute(Path.Combine(shaderDir, "particles.comp"));
        _render = ShaderProgram.FromVertexFragment(
            Path.Combine(shaderDir, "particles.vert"),
            Path.Combine(shaderDir, "particles.frag"));

        Particle[] seed = CreateSeed(count);

        _ssbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _ssbo);
        GL.BufferData(
            BufferTarget.ShaderStorageBuffer,
            count * Particle.SizeInBytes,
            seed,
            BufferUsageHint.DynamicDraw);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, _ssbo);

        // the vertex shader pulls straight from the SSBO by gl_VertexID, so the
        // VAO carries no attributes - it just needs to exist to issue the draw
        _vao = GL.GenVertexArray();
    }

    public EmitterShape Shape => _shape;

public void SetColorMode(int colorMode)
{
    _colorMode = colorMode;
}

    private Particle[] CreateSeed(int count)
    {
        var rng = new Random(1337);
        var particles = new Particle[count];
        for (int i = 0; i < count; i++)
        {
            particles[i] = CreateParticle(rng, count, i);
        }
        return particles;
    }

    private Particle CreateParticle(Random rng, int count, int index)
    {
        float t = index / (float)count;
        switch (_shape)
        {
            case EmitterShape.Point:
                return new Particle
                {
                    Position = Vector2.Zero,
                    Velocity = Vector2.Zero,
                    Life = 0.5f + (float)rng.NextDouble() * 3.5f,
                };

            case EmitterShape.Circle:
                {
                    double angle = rng.NextDouble() * Math.PI * 2.0;
                    float radius = (float)rng.NextDouble();
                    return new Particle
                    {
                        Position = new Vector2(
                            (float)Math.Cos(angle) * radius,
                            (float)Math.Sin(angle) * radius),
                        Velocity = Vector2.Zero,
                        Life = 0.5f + (float)rng.NextDouble() * 3.5f,
                    };
                }

            case EmitterShape.Line:
                {
                    float x = (float)rng.NextDouble() * 2.0f - 1.0f;
                    return new Particle
                    {
                        Position = new Vector2(x, 0.0f),
                        Velocity = Vector2.Zero,
                        Life = 0.5f + (float)rng.NextDouble() * 3.5f,
                    };
                }

            case EmitterShape.Ring:
                {
                    double angle = rng.NextDouble() * Math.PI * 2.0;
                    return new Particle
                    {
                        Position = new Vector2(
                            (float)Math.Cos(angle),
                            (float)Math.Sin(angle)),
                        Velocity = Vector2.Zero,
                        Life = 0.5f + (float)rng.NextDouble() * 3.5f,
                    };
                }

            default:
                return new Particle
                {
                    Position = Vector2.Zero,
                    Velocity = Vector2.Zero,
                    Life = 0.5f + (float)rng.NextDouble() * 3.5f,
                };
        }
    }

    public void Update(float deltaTime, Vector2 gravityWell, float wellStrength, float wellRadius = 0.0f)
    {
        Update(deltaTime, new[] { new GravityWell(gravityWell, wellStrength, wellRadius) });
    }

    public void UpdateCpu(float deltaTime, IReadOnlyList<GravityWell> wells)
    {
        if (wells.Count == 0)
            return;

        // Create a copy of the particle data from GPU
        Particle[] particles = new Particle[_count];
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _ssbo);
        IntPtr ptr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadWrite);
        unsafe
        {
            Particle* particlePtr = (Particle*)ptr;
            for (int i = 0; i < _count; i++)
            {
                particles[i] = particlePtr[i];
            }
        }
        GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);

        // Apply gravity well forces on CPU
        for (int i = 0; i < _count; i++)
        {
            Vector2 pos = particles[i].Position;
            Vector2 vel = particles[i].Velocity;

            foreach (var well in wells)
            {
                Vector2 toWell = well.Position - pos;
                float distSq = toWell.LengthSquared;

                if (distSq > 0.0001f) // Avoid division by zero
                {
                    float dist = MathF.Sqrt(distSq);
                    float radius = MathF.Max(well.Radius, 0.01f); // Clamp radius to avoid division issues

                    // Apply inverse-square law attraction, clamped at min distance
                    if (dist > radius)
                    {
                        float force = well.Strength / distSq;
                        vel += toWell.Normalized() * force * deltaTime;
                    }
                    else if (dist > 0.0001f)
                    {
                        // Clamped attraction when within radius
                        float t = dist / radius;
                        float force = well.Strength / (radius * radius) * t; // Linear interpolation from radius to center
                        vel += toWell.Normalized() * force * deltaTime;
                    }
                }

                particles[i].Velocity = vel;
                particles[i].Position += vel * deltaTime;
            }
        }

        // Upload updated particles back to GPU
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _ssbo);
        unsafe
        {
            Particle* particlePtr = (Particle*)GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.WriteOnly);
            for (int i = 0; i < _count; i++)
            {
                particlePtr[i] = particles[i];
            }
            GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);
        }
    }

    public void Update(float deltaTime, IReadOnlyList<GravityWell> wells)
    {
        _compute.Use();
        _compute.SetFloat("uDeltaTime", deltaTime);
        _compute.SetUInt("uCount", (uint)_count);
        _compute.SetUInt("uWellCount", (uint)wells.Count);

        // Upload well data to a UBO
        int ubo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, ubo);
        var wellData = new Vector4[wells.Count];
        for (int i = 0; i < wells.Count; i++)
        {
            wellData[i] = new Vector4(wells[i].Position.X, wells[i].Position.Y, wells[i].Strength, wells[i].Radius);
        }
        GL.BufferData(BufferTarget.UniformBuffer, wells.Count * Vector4.SizeInBytes, wellData, BufferUsageHint.DynamicDraw);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, ubo);

        int groups = (_count + LocalSize - 1) / LocalSize;
        GL.DispatchCompute(groups, 1, 1);

        // Clean up
        GL.DeleteBuffer(ubo);

        // make the compute writes visible to the subsequent vertex fetch
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit
            | MemoryBarrierFlags.VertexAttribArrayBarrierBit);
    }

    public void Render()
    {
        _render.Use();
    _render.SetInt("uColorMode", _colorMode);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Points, 0, _count);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_ssbo);
        GL.DeleteVertexArray(_vao);
        _compute.Dispose();
        _render.Dispose();
    }
}

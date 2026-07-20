using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

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

    private readonly int _count;
    private readonly int _ssbo;
    private readonly int _vao;
    private readonly EmitterShape _shape;

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

    public void Update(float deltaTime, Vector2 gravityWell, float wellStrength)
    {
        Update(deltaTime, new[] { (gravityWell, wellStrength) });
    }

    public void Update(float deltaTime, IReadOnlyList<(Vector2 pos, float strength)> wells)
    {
        _compute.Use();
        _compute.SetFloat("uDeltaTime", deltaTime);
        _compute.SetUInt("uCount", (uint)_count);
        _compute.SetUInt("uWellCount", (uint)wells.Count);

        // Upload well data to a UBO
        int ubo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, ubo);
        var wellData = new Vector3[wells.Count];
        for (int i = 0; i < wells.Count; i++)
        {
            wellData[i] = new Vector3(wells[i].pos.X, wells[i].pos.Y, wells[i].strength);
        }
        GL.BufferData(BufferTarget.UniformBuffer, wells.Count * Vector3.SizeInBytes, wellData, BufferUsageHint.DynamicDraw);
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

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

    private readonly int _count;
    private readonly int _ssbo;
    private readonly int _vao;

    private readonly ShaderProgram _compute;
    private readonly ShaderProgram _render;

    public ParticleSystem(int count, string shaderDir)
    {
        _count = count;

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

    private static Particle[] CreateSeed(int count)
    {
        var rng = new Random(1337);
        var particles = new Particle[count];
        for (int i = 0; i < count; i++)
        {
            double angle = rng.NextDouble() * Math.PI * 2.0;
            float radius = 0.2f + (float)rng.NextDouble() * 0.6f;
            particles[i] = new Particle
            {
                Position = new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius),
                Velocity = Vector2.Zero,
                Life = 0.5f + (float)rng.NextDouble() * 3.5f,
            };
        }
        return particles;
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

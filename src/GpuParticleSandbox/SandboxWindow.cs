using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace GpuParticleSandbox;

/// <summary>
/// The one and only window. Spins up an OpenGL 4.3 context (needed for compute
/// shaders), steps the particle system, and draws it. The gravity well tracks
/// the mouse so there's at least something to poke at.
/// </summary>
public sealed class SandboxWindow : GameWindow
{
    private const int ParticleCount = 100_000;

    private ParticleSystem _particles = null!;
    private Vector2 _well = Vector2.Zero;

    public SandboxWindow()
        : base(
            GameWindowSettings.Default,
            new NativeWindowSettings
            {
                ClientSize = new Vector2i(1024, 1024),
                Title = "GPU Particle Sandbox",
                APIVersion = new Version(4, 3),
                Profile = ContextProfile.Core,
            })
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.02f, 0.02f, 0.04f, 1.0f);
        GL.Enable(EnableCap.ProgramPointSize);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One); // additive glow

        string shaderDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _particles = new ParticleSystem(ParticleCount, shaderDir);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            Close();

        // map pixel-space mouse to clip space [-1, 1]
        float x = (MouseState.X / ClientSize.X) * 2f - 1f;
        float y = 1f - (MouseState.Y / ClientSize.Y) * 2f;
        _well = new Vector2(x, y);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        float dt = (float)args.Time;
        _particles.Update(dt, _well, wellStrength: 0.15f);

        GL.Clear(ClearBufferMask.ColorBufferBit);
        _particles.Render();

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
    }

    protected override void OnUnload()
    {
        _particles.Dispose();
        base.OnUnload();
    }
}

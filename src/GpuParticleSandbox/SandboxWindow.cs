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
    private bool _isPaused = false;
private bool _singleStepQueued = false;
    private float _simulationSpeed = 1.0f;
    private int _colorMode = 0;

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

        // Load default preset on startup
        LoadPreset("presets.json");
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        var kb = KeyboardState;
        if (kb.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            Close();

        if (kb.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space))
            _isPaused = !_isPaused;

if (kb.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Period))
_singleStepQueued = true;

        if (kb.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Equal) || kb.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadAdd))
            _simulationSpeed = Math.Clamp(_simulationSpeed + 0.1f, 0.1f, 5.0f);
        else if (kb.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Minus) || kb.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadSubtract))
            _simulationSpeed = Math.Clamp(_simulationSpeed - 0.1f, 0.1f, 5.0f);

        // Color mode cycling
        if (kb.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.C))
        {
            _colorMode = (_colorMode + 1) % 3;
                _particles.SetColorMode(_colorMode);
        }

        // Save preset (F5)
        if (kb.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.F5))
        {
            SavePreset("presets.json");
        }

        // Load preset (F9)
        if (kb.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.F9))
        {
            LoadPreset("presets.json");
        }

        // map pixel-space mouse to clip space [-1, 1]
        float x = (MouseState.X / ClientSize.X) * 2f - 1f;
        float y = 1f - (MouseState.Y / ClientSize.Y) * 2f;
        _well = new Vector2(x, y);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        if (!_isPaused || _singleStepQueued)
        {
            float dt = (float)args.Time * _simulationSpeed;
            _particles.Update(dt, _well, wellStrength: 0.15f);
if (_singleStepQueued)
_singleStepQueued = false;
        }

        GL.Clear(ClearBufferMask.ColorBufferBit);
        _particles.Render();

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
    }

    private void SavePreset(string filePath)
    {
    _particles.SetColorMode(_colorMode);
        var preset = ParticlePreset.FromSystem(
            ParticleCount,
            _colorMode,
            _particles.Shape,
            Vector2.Zero, // emitter params
            _simulationSpeed,
            0.15f, // well strength
            0.0f,  // well radius
            _isPaused
        );

        ParticlePreset.Save(preset, Path.Combine(AppContext.BaseDirectory, filePath));
        Console.WriteLine($"Preset saved to {filePath}");
    }

    private void LoadPreset(string filePath)
    {
        var preset = ParticlePreset.Load(Path.Combine(AppContext.BaseDirectory, filePath));

        _colorMode = preset.ColorMode;
    _particles.SetColorMode(_colorMode);
        _simulationSpeed = preset.SimulationSpeed;
        _isPaused = preset.IsPaused;

        Console.WriteLine($"Preset loaded from {filePath}");
    }

    protected override void OnUnload()
    {
        _particles.Dispose();
        base.OnUnload();
    }
}

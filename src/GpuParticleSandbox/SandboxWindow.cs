using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GpuParticleSandbox;

/// <summary>
/// The one and only window. Spins up an OpenGL 4.3 context (needed for compute
/// shaders), steps the particle system, and draws it. The gravity well tracks
/// the mouse so there's at least something to poke at.
/// </summary>
public sealed class SandboxWindow : GameWindow
{
    private const int ParticleCount = 100_000;
    private const float DefaultWellStrength = 0.15f;
    private const float WellStrengthStep = 0.01f;
    private const float WellStrengthMin = 0.01f;
    private const float WellStrengthMax = 1.0f;
    private const float SimSpeedStep = 0.1f;
    private const float SimSpeedMin = 0.1f;
    private const float SimSpeedMax = 5.0f;
    private const double FpsSmoothing = 0.1;
    private const int WindowSize = 1024;
    private const string PresetFileName = "presets.json";
    private static readonly Color4 ClearColor = new(0.02f, 0.02f, 0.04f, 1.0f);
    private static readonly Version OpenGlVersion = new(4, 3);

    private ParticleSystem _particles = null!;
    private Vector2 _well = Vector2.Zero;
    private bool _isPaused = false;
    private bool _singleStepQueued = false;
    private float _simulationSpeed = 1.0f;
    private float _wellStrength = DefaultWellStrength;
    private int _colorMode = 0;
    private FpsCounter _fpsCounter = new FpsCounter(FpsSmoothing);

    // Input handling maps
    private readonly Dictionary<Keys, Action> _keyPressActions = new();
    private readonly Dictionary<Keys, Action> _keyDownActions = new();

    public SandboxWindow()
        : base(
            GameWindowSettings.Default,
            new NativeWindowSettings
            {
                ClientSize = new Vector2i(WindowSize, WindowSize),
                Title = "GPU Particle Sandbox",
                APIVersion = OpenGlVersion,
                Profile = ContextProfile.Core,
            })
    {
        InitializeInputMaps();
    }

    /// <summary>
    /// Sets up dictionaries that map keys to actions.
    /// </summary>
    private void InitializeInputMaps()
    {
        // Actions that should fire once per key press
        _keyPressActions[Keys.Escape] = () => Close();

        _keyPressActions[Keys.Space] = () => _isPaused = !_isPaused;

        _keyPressActions[Keys.Period] = () => _singleStepQueued = true;

        _keyPressActions[Keys.C] = () =>
        {
            _colorMode = (_colorMode + 1) % 3;
            _particles.SetColorMode(_colorMode);
        };

        _keyPressActions[Keys.F5] = () => SavePreset(PresetFileName);

        _keyPressActions[Keys.F9] = () => LoadPreset(PresetFileName);

        // Actions that should fire while the key is held down
        _keyDownActions[Keys.Equal] = () => _simulationSpeed = Math.Clamp(_simulationSpeed + SimSpeedStep, SimSpeedMin, SimSpeedMax);
        _keyDownActions[Keys.KeyPadAdd] = () => _simulationSpeed = Math.Clamp(_simulationSpeed + SimSpeedStep, SimSpeedMin, SimSpeedMax);
        _keyDownActions[Keys.Minus] = () => _simulationSpeed = Math.Clamp(_simulationSpeed - SimSpeedStep, SimSpeedMin, SimSpeedMax);
        _keyDownActions[Keys.KeyPadSubtract] = () => _simulationSpeed = Math.Clamp(_simulationSpeed - SimSpeedStep, SimSpeedMin, SimSpeedMax);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(ClearColor);
        GL.Enable(EnableCap.ProgramPointSize);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One); // additive glow

        string shaderDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _particles = new ParticleSystemBuilder()
            .WithParticleCount(ParticleCount)
            .WithShaderDirectory(shaderDir)
            .WithEmitterShape(ParticleSystem.EmitterShape.Point)
            .WithColorMode(ColorMode.Velocity)
            .Build();

        // Load default preset on startup
        LoadPreset(PresetFileName);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        var kb = KeyboardState;

        // Process key‑press actions (fires once per press)
        foreach (var kvp in _keyPressActions)
        {
            if (kb.IsKeyPressed(kvp.Key))
                kvp.Value();
        }

        // Process key‑down actions (fires while held)
        foreach (var kvp in _keyDownActions)
        {
            if (kb.IsKeyDown(kvp.Key))
                kvp.Value();
        }

        // map pixel-space mouse to clip space [-1, 1]
        float x = (MouseState.X / ClientSize.X) * 2f - 1f;
        float y = 1f - (MouseState.Y / ClientSize.Y) * 2f;
        _well = new Vector2(x, y);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        _fpsCounter.Tick();

        if (!_isPaused || _singleStepQueued)
        {
            float dt = (float)args.Time * _simulationSpeed;
            _particles.Update(dt, _well, wellStrength: _wellStrength);
            if (_singleStepQueued)
                _singleStepQueued = false;
        }

        Title = $"GPU Particle Sandbox - {_fpsCounter.GetDisplayString()} - Well: {_wellStrength:F2}";
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _particles.Render();

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        // Guard against zero‑size dimensions (e.g., when the window is minimized)
        if (e.Width == 0 || e.Height == 0)
            return;

        // Update the OpenGL viewport to match the new window size
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        _wellStrength = Math.Clamp(
            _wellStrength + e.OffsetY * WellStrengthStep,
            WellStrengthMin,
            WellStrengthMax);
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
            _wellStrength, // well strength
            0.0f,  // well radius
            _isPaused
        );

        ParticlePreset.Save(preset, Path.Combine(AppContext.BaseDirectory, filePath));
        Console.WriteLine($"Preset saved to {filePath}");
    }

    private void LoadPreset(string filePath)
    {
        string presetPath = Path.Combine(AppContext.BaseDirectory, filePath);
        if (!File.Exists(presetPath))
        {
            Console.WriteLine($"Preset file not found: {filePath}");
            return;
        }

        try
        {
            // Validate here because ParticlePreset.Load falls back to its own defaults
            // when deserialization fails, which would overwrite the current settings.
            using (JsonDocument.Parse(File.ReadAllText(presetPath)))
            {
            }

            var preset = ParticlePreset.Load(presetPath);

            _colorMode = preset.ColorMode;
            _particles.SetColorMode(_colorMode);
            _simulationSpeed = preset.SimulationSpeed;
            _wellStrength = Math.Clamp(preset.WellStrength, WellStrengthMin, WellStrengthMax);
            _isPaused = preset.IsPaused;

            Console.WriteLine($"Preset loaded from {filePath}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Console.WriteLine($"Could not load preset {filePath}: {ex.Message}");
        }
    }

    protected override void OnUnload()
    {
        _particles.Dispose();
        base.OnUnload();
    }
}

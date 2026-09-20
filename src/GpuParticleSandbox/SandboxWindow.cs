using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using GpuParticleSandbox.Exceptions;

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
    private readonly CancellationTokenSource _cts = new();

    // Input handling maps
    private readonly Dictionary<Keys, Action> _keyPressActions = new();
    private readonly Dictionary<Keys, Action> _keyDownActions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxWindow"/> class.
    /// Configures the window size, title, and requests an OpenGL 4.3 Core profile context.
    /// </summary>
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
    /// <remarks>
    /// <list type="table">
    ///   <listheader><term>Key</term><th>Action</th></listheader>
    ///   <item><term>Escape</term><th>Closes the application.</th></item>
    ///   <item><term>Space</term><th>Toggles simulation pause/resume.</th></item>
    ///   <item><term>Period (.)</term><th>Queues a single simulation step while paused.</th></item>
    ///   <item><term>C</term><th>Cycles through color visualization modes.</th></item>
    ///   <item><term>F5</term><th>Saves the current simulation state to a preset file.</th></item>
    ///   <item><term>F9</term><th>Loads the most recently saved preset file.</th></item>
    ///   <item><term>= / Numpad +</term><th>Increases simulation speed.</th></item>
    ///   <item><term>- / Numpad -</term><th>Decreases simulation speed.</th></item>
    /// </list>
    /// </remarks>
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

    /// <summary>
    /// Called when the window is loaded and the OpenGL context is ready.
    /// </summary>
    /// <remarks>
    /// <para>Thread: Main render thread.</para>
    /// <para>GL State Assumptions: Assumes a valid OpenGL 4.3 Core context is current. Synchronously initializes the particle system and OpenGL state via <see cref="InitializeAsync"/>.</para>
    /// </remarks>
    protected override void OnLoad()
    {
        base.OnLoad();
        try
        {
            InitializeAsync(_cts.Token).GetAwaiter().GetResult();
        }
        catch (SandboxInitializationException ex)
        {
            Console.Error.WriteLine($"Sandbox initialization failed at stage '{ex.Stage}': {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.Error.WriteLine($"  Inner: {ex.InnerException.Message}");
            }
            throw;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected initialization failure: {ex.Message}");
            throw new SandboxInitializationException(InitializationStage.ResourceInitialization, "Failed to initialize sandbox resources", ex);
        }
    }

    /// <summary>
    /// Asynchronously loads configuration/assets and initializes OpenGL resources.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel initialization.</param>
    /// <remarks>
    /// <para>Thread: Main render thread (invoked synchronously from <see cref="OnLoad"/>).</para>
    /// <para>GL State Assumptions: Assumes the OpenGL context is current and valid. Configures clear color, enables point size program control, and sets up additive blending.</para>
    /// </remarks>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // OpenGL context & state setup
            GL.ClearColor(ClearColor);
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One); // additive glow
        }
        catch (Exception ex)
        {
            throw new SandboxInitializationException(InitializationStage.ContextCreation, "Failed to configure OpenGL context/state", ex);
        }

        try
        {
            // Shader loading & resource initialization
            // Note: Shader source loading is delegated to ParticleSystemBuilder.
            // If async shader loading is required, the builder can be updated to accept pre-loaded sources.
            string shaderDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
            _particles = new ParticleSystemBuilder()
                .WithParticleCount(ParticleCount)
                .WithShaderDirectory(shaderDir)
                .WithEmitterShape(ParticleSystem.EmitterShape.Point)
                .WithColorMode(ColorMode.Velocity)
                .Build();
        }
        catch (Exception ex)
        {
            DisposePartialResources();
            throw new SandboxInitializationException(InitializationStage.ResourceInitialization, "Failed to load shaders or initialize particle system", ex);
        }

        // Load default preset on startup asynchronously
        await LoadPresetAsync(PresetFileName, cancellationToken);
    }

    private void DisposePartialResources()
    {
        _particles?.Dispose();
        _particles = null!;
    }

    /// <summary>
    /// Called every frame to process input and update the simulation state.
    /// </summary>
    /// <param name="args">Frame timing information.</param>
    /// <remarks>
    /// <para>Thread: Main render thread.</para>
    /// <para>GL State Assumptions: Does not make direct OpenGL calls. Delegates rendering state updates to <see cref="ParticleSystem"/>.</para>
    /// <para>Processes keyboard input maps and updates the gravity well position based on mouse coordinates.</para>
    /// </remarks>
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

    /// <summary>
    /// Called every frame to render the particle simulation to the screen.
    /// </summary>
    /// <param name="args">Frame timing information.</param>
    /// <remarks>
    /// <para>Thread: Main render thread.</para>
    /// <para>GL State Assumptions: Assumes the OpenGL context is current and the particle system is fully initialized. Clears the color buffer, dispatches the particle render pass, and swaps the back/front buffers.</para>
    /// </remarks>
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

    /// <summary>
    /// Called when the window is resized.
    /// </summary>
    /// <param name="e">Event data containing the new client area dimensions.</param>
    /// <remarks>
    /// <para>Thread: Main render thread.</para>
    /// <para>GL State Assumptions: Assumes the OpenGL context is current. Updates the OpenGL viewport to match the new window dimensions. Guards against zero-width or zero-height dimensions (e.g., minimized windows).</para>
    /// </remarks>
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        // Guard against zero‑size dimensions (e.g., when the window is minimized)
        if (e.Width == 0 || e.Height == 0)
            return;

        // Update the OpenGL viewport to match the new window size
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    /// <summary>
    /// Called when the mouse wheel is scrolled.
    /// </summary>
    /// <param name="e">Event data containing the scroll offset.</param>
    /// <remarks>
    /// <para>Thread: Main render thread.</para>
    /// <para>GL State Assumptions: Does not make direct OpenGL calls. Adjusts the gravity well strength based on scroll direction. Clamps the strength to predefined minimum and maximum bounds.</para>
    /// </remarks>
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

    private async Task LoadPresetAsync(string filePath, CancellationToken cancellationToken)
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
            using (JsonDocument.Parse(await File.ReadAllTextAsync(presetPath, cancellationToken)))
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

    private void LoadPreset(string filePath)
    {
        // Keep synchronous wrapper for runtime key-press actions to avoid blocking the render thread
        LoadPresetAsync(filePath, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Called when the window is about to be unloaded and the OpenGL context is being destroyed.
    /// </summary>
    /// <remarks>
    /// <para>Thread: Main render thread.</para>
    /// <para>GL State Assumptions: Cancels any pending asynchronous operations and disposes of the particle system resources. Assumes the OpenGL context is still valid for cleanup, but no new GL calls should be made after <c>base.OnUnload()</c>.</para>
    /// </remarks>
    protected override void OnUnload()
    {
        _cts.Cancel();
        _particles.Dispose();
        base.OnUnload();
    }
}

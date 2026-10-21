# GPU Particle Sandbox

A tiny GPU-driven particle simulation. All the per-frame integration happens in
a GLSL compute shader writing into an SSBO; the render pass reads the exact same
buffer, so once the particles are seeded the data never round-trips back to the
CPU. ~100k particles chasing a gravity well that follows your mouse.

Built with C# / .NET 8 and [OpenTK](https://opentk.net/) (OpenGL 4.3 core).

## Status: abandoned

This was meant to grow into a reusable GPU particle engine - configurable
emitters, force fields, collision, a proper editor, the works. It never got
past the demo. The compute + render loop works and looks decent, and that
turned out to be as far as I took it. Shelved.

If you're digging through this expecting a library, there isn't one here. It's
one window and one hardcoded emitter.

## What actually works

- Compute shader integrates position/velocity/life for every particle in place
- Single gravity well, mouse-controlled
- Particles respawn near the edges when their life runs out
- Additive blending so dense regions glow
- Age-based color fade (hot white -> cool blue)

## Getting started

### Prerequisites

| Requirement | Details |
| --- | --- |
| .NET SDK | .NET 8 SDK (the project targets `net8.0`). Check with `dotnet --version`. |
| OpenGL | **4.3 core profile** or newer. Compute shaders are mandatory; there is no fallback path. |
| GPU / driver | A GPU whose driver reports OpenGL 4.3 core. Check on Linux with `glxinfo -B \| grep "core profile version"`, on Windows with GPU-Z (OpenGL field). |
| OS | Windows or Linux with a desktop session. macOS won't run it: Apple's OpenGL tops out at 4.1, which has no compute shaders. |
| Network | The first `dotnet restore` downloads OpenTK 4.8.2 from NuGet. |

### Clone and run

```bash
git clone https://github.com/Sarmkadan/gpu-particle-sandbox.git
cd gpu-particle-sandbox
dotnet run --project src/GpuParticleSandbox
```

The shaders are copied next to the built binary, so the working directory doesn't matter.

### Controls

Controls for the `SandboxWindow` demo (see `InitializeInputMaps` and `OnMouseWheel` in `SandboxWindow.cs`):

| Input | Action |
| --- | --- |
| Mouse move | Moves the gravity well; it follows the cursor. |
| Mouse wheel | Raises or lowers well strength by 0.01 per notch (range 0.01–1.00). |
| `Space` | Pause / resume the simulation. |
| `.` (period) | While paused, advance one frame of simulation. |
| `C` | Cycle color mode: 0 = age, 1 = velocity heat, 2 = single hue. |
| `=` / Numpad `+` | Speed up (hold; applied every frame, range 0.1×–5.0×). |
| `-` / Numpad `-` | Slow down (hold; same range). |
| `F5` | Save speed, well strength, color mode and pause state to `presets.json` next to the executable. |
| `F9` | Load `presets.json`. Restores the same four settings; particle count isn't restored. |
| `Esc` | Quit. |

### Using ParticleSystem in your own app

`ParticleSystem` is public. To use it from another project, reference the library and copy its shaders into your output folder:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/gpu-particle-sandbox/src/GpuParticleSandbox/GpuParticleSandbox.csproj" />
  <None Include="path/to/gpu-particle-sandbox/src/GpuParticleSandbox/Shaders/*"
        CopyToOutputDirectory="PreserveNewest" Link="Shaders/%(Filename)%(Extension)" />
</ItemGroup>
```

`ParticleSystem` loads `particles.comp`, `particles.vert` and `particles.frag` from the directory you pass it. The examples below use `AppContext.BaseDirectory`, as `SandboxWindow` does.

**Create a system.** The constructor needs a current OpenGL context, so call it from `OnLoad` (see the window example below), not from a constructor.

```csharp
string shaderDir = Path.Combine(AppContext.BaseDirectory, "Shaders");

var particles = new ParticleSystem(100_000, shaderDir);
particles.SetColorMode(0); // 0 = age, 1 = velocity heat, 2 = single hue
```

`count` must be greater than zero, and `SetColorMode` accepts 0–2. Otherwise it throws `ArgumentOutOfRangeException`.

**Emission.** The shape sets where particles start:

| `ParticleSystem.EmitterShape` | Initial positions |
| --- | --- |
| `Point` (default) | Every particle at the origin. |
| `Circle` | Random points in the unit disc. |
| `Line` | Random points on the x-axis from -1 to 1. |
| `Ring` | Random points on the unit circle. |

```csharp
var ring = new ParticleSystem(20_000, shaderDir, ParticleSystem.EmitterShape.Ring);
ring.Resize(200_000); // re-seeds all particles with the same shape
```

The shape only applies when particles are seeded. When a particle's life runs out, the compute shader respawns it on a ring of radius 0.8–1.0 at a random angle, whatever the shape. There is no emission rate or burst control: `ParticleSystemBuilder.WithSpawnRate` and `WithGravity` are stored but never passed to `ParticleSystem`.

**Gravity and attractors.** There is no global gravity vector. Every force is a `GravityWell`:

- `Position` is in clip space: x and y run from -1 to 1. Particles are drawn at their position directly, with no camera.
- `Strength` is positive to attract and negative to repel.
- `Radius` is a floor on the distance used by the force, so the force stays bounded near the centre. It defaults to 0. With 0, a particle sitting exactly on a well's centre gets a NaN velocity, because the shader normalises a zero vector. Use something like 0.02–0.05 for wells that particles can reach.

```csharp
// One well, with and without a radius
particles.Update(dt, new Vector2(0.0f, 0.0f), 0.15f);
particles.Update(dt, new Vector2(0.0f, 0.0f), 0.15f, 0.05f);

// Several wells; a negative strength repels
var wells = new List<ParticleSystem.GravityWell>
{
    new(new Vector2(-0.5f, 0.0f), 0.2f, 0.05f),
    new(new Vector2(0.5f, 0.0f), 0.2f, 0.05f),
};
particles.Update(dt, wells);
```

**Stepping manually.** `Update` dispatches the compute shader immediately and doesn't draw. `Render` draws whatever is in the buffer. Nothing advances on its own, so the simulation moves only when you call `Update`:

```csharp
// Advance exactly 1/120 s, then draw the result
particles.Update(1.0f / 120.0f, wells);
particles.Render();
```

Call `Update` with a fixed step, as the example below does, to keep the simulation independent of frame rate.

**Custom window loop.** This is a complete `Program.cs`. It opens a GL 4.3 core window like `SandboxWindow` does, creates the system in `OnLoad`, steps it with a fixed timestep, draws it in `OnRenderFrame`, and disposes it in `OnUnload`:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using GpuParticleSandbox;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

using var window = new EmitterWindow();
window.Run();

sealed class EmitterWindow : GameWindow
{
    private const float FixedStep = 1.0f / 60.0f;
    private const float MaxFrameTime = 0.25f;

    private readonly List<ParticleSystem.GravityWell> _wells = new()
    {
        new(new Vector2(0.0f, 0.0f), 0.15f, 0.05f),
        new(new Vector2(0.6f, 0.4f), -0.05f, 0.05f),
    };

    private ParticleSystem _particles = null!;
    private float _accumulator;

    public EmitterWindow()
        : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            ClientSize = new Vector2i(1024, 1024),
            Title = "My particles",
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
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        string shaderDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _particles = new ParticleSystem(50_000, shaderDir, ParticleSystem.EmitterShape.Ring);
        _particles.SetColorMode(1);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        // Cap the frame time so a stall doesn't trigger a huge catch-up
        _accumulator = Math.Min(_accumulator + (float)args.Time, MaxFrameTime);
        while (_accumulator >= FixedStep)
        {
            _particles.Update(FixedStep, _wells);
            _accumulator -= FixedStep;
        }

        GL.Clear(ClearBufferMask.ColorBufferBit);
        _particles.Render();
        SwapBuffers();
    }

    protected override void OnUnload()
    {
        _particles.Dispose();
        base.OnUnload();
    }
}
```

Create a console project, add the `ProjectReference` and `Shaders` items shown above, and drop this in as `Program.cs`.

### Troubleshooting: "no GL context"

**Symptom:** an exception or crash on the first line that touches OpenGL (`new ParticleSystem`, `ShaderProgram.From…`, or any `GL.*` call). Without a current context, OpenTK usually fails with a NullReferenceException or AccessViolationException instead of a clear message. In the demo, stderr may also show `Sandbox initialization failed at stage 'ContextCreation'`.

Check these in order:

1. **Are you on the window's thread, after the window has loaded?** The GL context is current only on the thread that owns it. Create `ParticleSystem` and `ShaderProgram` in `OnLoad` (or later), not in a constructor, not before `Run()`, and not on a `Task` or thread-pool thread.
2. **Is the context really 4.3 core?** On Linux run `glxinfo -B` and read "OpenGL core profile version string". A context below 4.3 can still come up, but compute shaders fail to compile, and you get an `InvalidOperationException` ending in `failed to compile`.
3. **Is there a GPU with a display?** Headless machines, SSH sessions, CI runners, VMs without 3D acceleration and WSL without GPU passthrough normally can't provide GL 4.3 core. A headless Linux shell fails with `GLFWException: X11: The DISPLAY environment variable is missing`. Run the app from a desktop session.
4. **Did the window land on the integrated GPU?** On hybrid laptops, force the discrete GPU. On Linux with Mesa, prefix the command with `DRI_PRIME=1`. With NVIDIA's proprietary driver, use `__NV_PRIME_RENDER_OFFLOAD=1 __GLX_VENDOR_LIBRARY_NAME=nvidia`. Or set the app's GPU preference in your vendor's control panel.

If the error is a `FileNotFoundException` for `particles.comp` or another shader, the `Shaders` folder isn't in your output directory. Add the `None` item from the snippet above.

## Layout

```
GpuParticleSandbox.sln
src/GpuParticleSandbox/
  Program.cs           entry point, just opens the window
  SandboxWindow.cs     GameWindow, input, frame loop
  ParticleSystem.cs    owns the SSBO, dispatches compute, issues the draw
  ShaderProgram.cs     GL program wrapper + uniform cache
  Particle.cs          CPU mirror of the GLSL struct (std430 padding matters)
  Shaders/
    particles.comp     the actual simulation
    particles.vert     pulls positions from the SSBO by gl_VertexID
    particles.frag     color/fade
```

## Ideas that never happened

Leaving these here mostly so I remember where I stopped:

- [ ] Emitter abstraction (shapes, rates, bursts) instead of one hardcoded ring
- [ ] Multiple force fields (attractors, repulsors, wind, curl noise)
- [ ] Double-buffered SSBO so read/write don't alias (works fine as-is at this
      scale, but it's technically sloppy)
- [ ] Textured/soft point sprites instead of hard GL points
- [ ] Some kind of scene/config file so it's not all constants in C#
- [ ] An OpenCL backend was the original plan - never touched it

## ParticleSystem

`ParticleSystem` handles the GPU-accelerated particle simulation lifecycle, managing the Shader Storage Buffer Object (SSBO) and driving both the compute shader integration and the vertex shader rendering process. It ensures data remains in VRAM after initialization for maximum efficiency.

Example usage:
```csharp
// Initialize the system
var particleSystem = new ParticleSystem(100000, "Shaders");

// Update simulation with a gravity well
particleSystem.Update(0.016f, new Vector2(0.5f, 0.5f), 10.0f);

// Render the particles
particleSystem.Render();

// Cleanup resources
particleSystem.Dispose();
```

## ParticleSystemExtensions

Extension methods for `ParticleSystem` that provide safe resource management and performance monitoring utilities. These methods wrap common operations like updating the particle simulation, rendering, and resource cleanup with proper error handling and memory tracking.

Example usage:
```csharp
// Create and initialize particle system
var particleSystem = new ParticleSystem();
particleSystem.SetGravityWell(new Vector3(0, 0, 0));

// Update simulation in fixed timestep
particleSystem.UpdateFixed(1.0f / 60.0f);

// Render particles
particleSystem.Render(cameraViewMatrix, cameraProjectionMatrix);

// Check memory usage
long memoryUsed = particleSystem.GetMemoryUsage();
Console.WriteLine($"Particles using {memoryUsed} bytes");

// Cleanup when done
if (particleSystem.SafeDispose())
{
    Console.WriteLine("Resources safely disposed");
}
```

## ShaderProgram

`ShaderProgram` is a lightweight wrapper around an OpenGL shader program that manages program linking, uniform location caching, and resource cleanup. It supports both traditional vertex+fragment pipelines and standalone compute shaders, providing a simple API for setting uniforms and activating the program.

Example usage:
```csharp
// Create a vertex-fragment shader program from files
var renderShader = ShaderProgram.FromVertexFragment(
    "Shaders/particles.vert",
    "Shaders/particles.frag"
);

// Create a compute shader program
var computeShader = ShaderProgram.FromCompute("Shaders/particles.comp");

// Activate the shader before setting uniforms or rendering
renderShader.Use();

// Set uniform values
renderShader.SetFloat("pointSize", 3.0f);
renderShader.SetUInt("maxParticles", 100000u);
renderShader.SetVector2("gravityWell", new Vector2(mouseX, mouseY));

// When done with the shader, dispose it to free GPU resources
computeShader.Dispose();
```

## ShaderProgramExtensions

Extension methods for `ShaderProgram` that simplify common shader uniform operations. These methods provide type-safe ways to set uniform values including float arrays, uint arrays, Vector2 arrays, individual Vector3 values, Matrix4 values, and int values, with proper null checking and uniform existence validation.

Example usage:
```csharp
// Create shader program
var shader = new ShaderProgram("particles.vert", "particles.frag");

// Set individual uniform values
shader.SetInt("pointSize", 3);
shader.SetVector3("gravityWell", new Vector3(0, -9.8f, 0));
shader.SetMatrix4("viewProjection", cameraViewMatrix * cameraProjectionMatrix);

// Set uniform arrays
float[] particleLifetimes = new float[100000];
shader.SetFloatArray("particleLifetimes", particleLifetimes);

uint[] particleIds = Enumerable.Range(0, 100000).Select(i => (uint)i).ToArray();
shader.SetUIntArray("particleIds", particleIds);

Vector2[] offsets = new Vector2[100];
shader.SetVector2Array("offsets", offsets);

// Check if a uniform exists before setting
if (shader.HasUniform("particleCount"))
{
shader.SetInt("particleCount", 100000);
}
```

## License

Do whatever. It's a toy.

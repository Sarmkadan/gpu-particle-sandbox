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

## Running it

```bash
dotnet run --project src/GpuParticleSandbox
```

Needs a GPU/driver that exposes an OpenGL 4.3 core context (compute shaders).
Esc to quit. Move the mouse to drag the gravity well around.

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

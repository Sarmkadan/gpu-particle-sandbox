# CLAUDE.md

GPU-driven particle demo in C# / .NET 8 + OpenTK 4.8 (OpenGL 4.3 core): a compute shader integrates ~100k particles in an SSBO, the render pass reads the same buffer. Single-project solution, status "abandoned" per README.

## Build

```bash
dotnet restore
dotnet build GpuParticleSandbox.sln
dotnet run --project src/GpuParticleSandbox      # needs a GPU with an OpenGL 4.3 core context
```

Shaders under `src/GpuParticleSandbox/Shaders/` are copied to the output dir (`PreserveNewest`); the app loads them relative to the working directory.

## Tests

There is no test project and no test framework. `src/GpuParticleSandbox/FpsCounterTests.cs` holds hand-rolled static test methods (`TestInitialization`, `TestExponentialMovingAverage`, ...) with a private `Assert` helper that throws. They are compiled into the main exe but not invoked from `Program.cs`; call them manually if needed. No `dotnet test` target exists.

## Lint / format

No `.editorconfig`, analyzers or CI. Use `dotnet format` defaults. `Nullable` and `ImplicitUsings` are enabled; `AllowUnsafeBlocks` is on.

## Key directories and entry points

- `GpuParticleSandbox.sln` - solution, one project
- `src/GpuParticleSandbox/Program.cs` - entry point; creates `SandboxWindow` and runs it
- `src/GpuParticleSandbox/SandboxWindow.cs` - `GameWindow` subclass: input, frame loop, pause/step, color mode
- `src/GpuParticleSandbox/ParticleSystem.cs` - owns SSBO/VAO, dispatches compute, issues draw; `LocalSize = 256` must match `layout(local_size_x)` in `particles.comp`
- `src/GpuParticleSandbox/ShaderProgram.cs` - GL program wrapper with uniform location cache; `FromVertexFragment`, `FromCompute`
- `src/GpuParticleSandbox/Particle.cs` - CPU mirror of the GLSL struct (std430 padding matters)
- `src/GpuParticleSandbox/ParticlePreset.cs`, `ColorMode.cs`, `FpsCounter.cs` - config record, enum, EMA FPS counter
- `src/GpuParticleSandbox/*Extensions.cs`, `*JsonExtensions.cs`, `ShaderProgramValidation.cs`, `ShaderSourceValidator.cs` - static helper classes
- `src/GpuParticleSandbox/Shaders/particles.{comp,vert,frag}` - simulation, vertex pull by `gl_VertexID`, color/fade
- `docs/*.md` - per-class API notes (ParticleSystem, ShaderProgram and their extensions)

## Conventions

- Single namespace `GpuParticleSandbox`, file-scoped `namespace` declarations, one public type per file named after the type
- Classes are `sealed`; GL-owning types implement `IDisposable`; helpers are `public static class XxxExtensions`
- Private fields `_camelCase`, constants `PascalCase`, XML doc comments on public members
- Public methods guard arguments with `ArgumentNullException` / `ArgumentOutOfRangeException`
- GLSL uniforms are `uPascalCase` (e.g. `uDeltaTime`); UBO/SSBO blocks use explicit `binding = N`
- Commit messages: conventional commits, optionally scoped `type(gpuparticlesandbox): ...`
- `bin/`, `obj/`, `.aider*` are gitignored; do not commit build output

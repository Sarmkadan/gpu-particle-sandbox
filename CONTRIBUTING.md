# Contributing

Thanks for looking at this. The project is a small, shelved demo, so the process is kept light.

## Prerequisites

| Requirement | Details |
| --- | --- |
| .NET SDK | .NET 8 SDK. The project targets `net8.0`, and CI uses the `8.0.x` line. There is no `global.json`, so any 8.x SDK works. Check with `dotnet --version`. |
| OpenGL | 4.3 core profile or newer. Compute shaders are required, with no fallback. |
| GPU / driver | Needed only to **run** the sandbox. Building and restoring don't need a GPU. |
| OS | Windows or Linux with a desktop session. macOS is not supported (OpenGL 4.1 max). |

Verify the GL version on Linux with `glxinfo -B | grep "core profile version"`.

## Clone and restore

```bash
git clone https://github.com/Sarmkadan/gpu-particle-sandbox.git
cd gpu-particle-sandbox
dotnet restore GpuParticleSandbox.sln
```

## Build

```bash
dotnet build GpuParticleSandbox.sln -c Release
```

The build currently reports 31 `CS1591` warnings (missing XML doc comments). See the PR checklist for how this applies.

## Run the sandbox

```bash
dotnet run --project src/GpuParticleSandbox
```

This needs a desktop session with a GPU that provides OpenGL 4.3 core. It won't run on headless machines, CI or most VMs. The shaders in `src/GpuParticleSandbox/Shaders/` are copied next to the binary on build.

## Tests

**There is currently no runnable test suite.** The solution contains only the main project, and no test project references a test framework:

- `dotnet test GpuParticleSandbox.sln` exits 0 but runs zero tests.
- `tests/GpuParticleSandbox.Tests/` and `tests/SandboxInitializationExceptionTests.cs` contain xUnit-style tests that aren't part of any `.csproj` or the `.sln`, so they don't compile or run.
- `src/GpuParticleSandbox/FpsCounterTests.cs` and `ParticleSystemBuilderTests.cs` are hand-rolled static methods compiled into the executable. Nothing calls them.

If you add a test project, add it to `GpuParticleSandbox.sln` and make sure `dotnet test GpuParticleSandbox.sln -c Release` runs tests. Then update this section.

Benchmarks live in `benchmarks/GpuParticleSandbox.Benchmarks` (BenchmarkDotNet). They are not in the solution or in CI. The entry point currently runs only `ShaderProgramBenchmarks`.

## Code style

- There is **no `.editorconfig`** in the repository, so there is no enforced style file yet. Use `dotnet format` defaults.
- Nullable reference types and implicit usings are enabled. `AllowUnsafeBlocks` is on.
- One public type per file, named after the type. Single namespace `GpuParticleSandbox`, with file-scoped `namespace` declarations in new files.
- Classes are `sealed`. Types that own GL objects implement `IDisposable`.
- Private fields are `_camelCase`. Constants are `PascalCase`.
- Public members have XML doc comments. Public methods validate arguments with `ArgumentNullException` / `ArgumentOutOfRangeException`.
- Static helpers are `public static class XxxExtensions`.
- GLSL uniforms are `uPascalCase`. UBO/SSBO blocks use explicit `binding = N`.
- `LocalSize` in `ParticleSystem.cs` must match `local_size_x` in `particles.comp`.

If you add an `.editorconfig`, it becomes the source of truth and this section should point to it.

## Branch naming

The repository has no enforced convention. Use `<type>/<short-kebab-description>`, where `<type>` matches the commit type, for example:

- `feat/gravity-well-radius`
- `fix/fps-counter-reset`
- `docs/contributing`

Branch from `main` and open the PR against `main`.

## Commit messages

Use [Conventional Commits](https://www.conventionalcommits.org/), optionally with a scope:

```
type(scope): short imperative summary
```

- Types in use: `feat`, `fix`, `perf`, `refactor`, `test`, `docs`, `chore`.
- Scope is optional. The project commonly uses `gpuparticlesandbox`, e.g. `feat(gpuparticlesandbox): add ring emitter`.
- Keep the subject short. Use the body to explain *why* if it isn't obvious from the diff.

## Pull request checklist

Before requesting review, confirm:

- [ ] `dotnet build GpuParticleSandbox.sln -c Release` succeeds (CI runs the same command on Ubuntu, without a GPU).
- [ ] `dotnet test GpuParticleSandbox.sln -c Release` passes. Until a test project exists, this command runs no tests, so note that in the PR description.
- [ ] No new compiler warnings. The baseline is 31 `CS1591` warnings. Adding XML doc comments to members you touch is welcome, but don't add new undocumented public members.
- [ ] Public API changes are reflected in the matching `docs/*.md` file.
- [ ] Build output (`bin/`, `obj/`) and local editor files are not committed.
- [ ] The sandbox still starts and renders on a GPU with OpenGL 4.3, if your change touches rendering or shaders. CI can't check this.
